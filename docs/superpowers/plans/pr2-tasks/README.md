# PR2 task files

Per-task slices of `../2026-05-04-clean-arch-pr2-application-infrastructure.md` so
sessions resuming PR2 can read just the file for the next task instead of the
107KB master plan.

**Always read `../PR2-CONTINUATION.md` first** — it captures the current branch
state, completed tasks, and PR2-specific deviations from the master plan.

| Task | Phase(s) | Scope |
|------|----------|-------|
| [T5.md](T5.md) | 2.11, 2.12 | TaikoDbContext implements ITaikoDbContext; create JwtTokenService, SystemClock, AllnetSettings; move AuthSettings into Infrastructure/Identity/Settings |
| [T6.md](T6.md) | 2.13 | Move GameDataService → FileGameDataCatalog in Infrastructure; move PathHelper, DataSettings; create CatalogConstants. Per-loader split deferred. |
| [T7.md](T7.md) | 2.14 | Create PersistenceConstants + Infrastructure/DependencyInjection.cs (`AddInfrastructure(IConfiguration)`); delete TaikoLocalServer/Common/Constants.cs |
| [T8.md](T8.md) | 2.16.2–2.16.6 | Rewrite 8 simple admin controllers (Pattern A) + GameDataController (Pattern B) to inject ports |
| [T9.md](T9.md) | 2.16.7 | Rewrite AuthController (Login/Register/ChangePassword/OTP); preserve JWT Expires verbatim (Risk #3) |
| [T10.md](T10.md) | 2.17, 2.18, 2.19 | Delete 13 wrapper service files; slim Program.cs to AddOptions/AddApplication/AddInfrastructure; finalize LocalSaveModScoreMigrator |
| [Phase2.20.md](Phase2.20.md) | 2.20 | User smoke gates |

Each task file is self-contained: it lists pre-state, exact commands, code
samples, and expected build state. The master plan
(`../2026-05-04-clean-arch-pr2-application-infrastructure.md`) has additional
rationale and worked examples if a deviation needs to be revisited.
