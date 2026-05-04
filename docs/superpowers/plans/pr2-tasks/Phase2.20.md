# Phase 2.20 — User smoke gates

User-driven, not Claude. Per memory, the user runs the server (admin required for ports 80/443) and exercises the smoke tests; Claude only verifies builds.

**Pre-state**: T10 done. `dotnet build` is green. All 7 projects compile. The dependency graph reads `Domain ← Application ← Infrastructure ← TaikoLocalServer + LocalSaveModScoreMigrator`.

---

## Step 1 — Build green (Claude does this)

```bash
dotnet build 2>&1 | tail -10
```

Expected: 7 projects build, 0 errors. Inspect Mediator generator output:

```bash
ls Application/obj/Debug/net10.0/generated/Mediator.SourceGenerator/Mediator.SourceGenerator.IncrementalMediatorGenerator/
```

Expected: `Mediator.g.cs` present, with all 16 handler registrations (one per `Application/Handlers/*.cs` file). T3 confirmed 16; T6's catalog rewire shouldn't affect this count.

## Step 2 — User runs the server

User runs `dotnet run --project TaikoLocalServer` (admin required because Kestrel binds 80/443). Watch for:

- `Server starting up...`
- `Mapped <N> endpoints` — record this and compare to PR2 baseline. If it differs, a controller's routing was lost during T8/T9 rewrite — investigate.
- `Now listening on:` lines for all configured ports (5000, 80, 10122, 54430, 54431, 57402, 443).

## Step 3 — Game endpoint smoke (user)

Either with captured fixtures, or just verifying routes exist:

```bash
curl -o /dev/null -s -w "%{http_code}\n" -X POST http://localhost:54430/v12r08_ww/chassis/initialdatacheck.php
curl -o /dev/null -s -w "%{http_code}\n" -X POST http://localhost:57402/v12r00_cn/chassis/initialdatacheck.php
```

Expected: NOT 404 (route reachable; 400/500 due to empty body is fine).

## Step 4 — Admin API smoke (auth off, user)

`Configurations/AuthSettings.json` has `"AuthenticationRequired": false`:

```bash
curl http://localhost:5000/api/users -i
curl http://localhost:5000/api/users/1000 -i
curl -X DELETE http://localhost:5000/api/users/99999999 -i
```

Expected: 200 (list/single user), 404 (no such user).

## Step 5 — Admin API smoke (auth on, user)

Set `"AuthenticationRequired": true` in `Configurations/AuthSettings.json`, restart server.

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"AccessCode":"<card>","Password":"<password>"}' -i
```

Expected: 200 with JWT in body. **Critical (Risk #3)**: decode the JWT (jwt.io or `python -c 'import jwt; print(jwt.decode(...))'`) and confirm `exp - iat` matches the original AuthController.Login `Expires` value (typically 24h). Mismatch = T5/T9 copied the wrong expiration.

```bash
TOKEN="<paste-jwt>"
curl http://localhost:5000/api/users/1000 -H "Authorization: Bearer $TOKEN" -i
```

Test admin path (succeeds) and cross-baid as non-admin (returns null/403).

## Step 6 — WebUI smoke (user, browser)

Browse to `http://localhost:5000/`:
- Dashboard renders.
- Login flow round-trips — JWT stored, returns to Dashboard with auth context.
- DevTools console: no red errors.

## Step 7 — Migrator smoke (user)

```bash
dotnet run --project LocalSaveModScoreMigrator
```

Expected: argparse banner or "missing required argument" error. Confirms it builds + starts.

## Step 8 — Restore AuthenticationRequired

If changed for Step 5, restore the previous value.

---

## Push

User pushes branch (per memory: solo-owned repo, push branches and let user merge to dev directly):

```bash
git push origin clean-arch/pr2-app-infra
```

No PR. The user merges to dev locally.

---

## Done criteria

- All 7 projects build clean.
- Server starts; route count matches baseline; all ports bind.
- Game endpoints reachable for both `_ww` and `_cn`.
- Admin API works auth-off and auth-on; JWT lifetime preserved.
- WebUI dashboard + login round-trips.
- Migrator builds + starts.
