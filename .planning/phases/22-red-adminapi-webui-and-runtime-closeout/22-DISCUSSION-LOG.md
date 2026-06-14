# Phase 22: Red AdminApi/WebUI and Runtime Closeout - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md; this log preserves the alternatives considered.

**Date:** 2026-06-15
**Phase:** 22-Red AdminApi/WebUI and Runtime Closeout
**Areas discussed:** Red normal readouts, Don Challenge page, final closeout evidence

---

## Red Normal Readouts

### Readout Scope

| Option | Description | Selected |
|--------|-------------|----------|
| Full parity | Wire Red into the existing implemented AC15 profile, scores, history, favorites, Dani, catalog, and customization readouts. | Yes |
| Gameplay only | Expose profile, scores, history, favorites, and Dani while deferring customization/catalog editing surfaces. | |
| Minimum closeout | Expose only the readouts needed to support final runtime smoke and avoid broader WebUI parity. | |

**User's choice:** Full parity
**Notes:** Red should expose implemented Red-owned normal readout surfaces through the existing generic AdminApi/WebUI path.

### Edit Policy

| Option | Description | Selected |
|--------|-------------|----------|
| Existing AC15 edits | Allow the same profile, favorite, customization, and settings edits already supported by existing AC15 pages and permissions. | Yes |
| Read-only Red | Expose Red data for inspection only, leaving all mutations to cabinet/runtime flows. | |
| Opt-in only | Keep normal readouts read-only and allow editing only the ChallengeCompe opt-in state. | |

**User's choice:** Existing AC15 edits
**Notes:** Edits are allowed where backing Red state already exists.

### API Shape

| Option | Description | Selected |
|--------|-------------|----------|
| Era routes | Use existing `/api/{era}/...` contracts and generic WebUI pages so Red behaves like Green, Blue, and Yellow. | Yes |
| Red-specific routes | Create dedicated Red endpoints or DTOs where Red fields differ, even when generic surfaces could work. | |
| You decide | Let the planner choose per surface based on the existing code and Red field differences. | |

**User's choice:** Era routes
**Notes:** Avoid Red-only API shapes when existing era-routed contracts fit.

### Compatibility Diagnostics

| Option | Description | Selected |
|--------|-------------|----------|
| No UI diagnostics | Keep compatibility route evidence in verification artifacts and logs, not operator-facing pages. | Yes |
| Minimal status | Add a small read-only status surface showing observed/available compatibility endpoints. | |
| You decide | Let the planner add diagnostics only if it falls out naturally from existing AdminApi patterns. | |

**User's choice:** No UI diagnostics
**Notes:** Compatibility-only routes should stay out of operator UI.

---

## Don Challenge Page

### Placement

| Option | Description | Selected |
|--------|-------------|----------|
| Play Data page | Add a separate user-era page under the existing Play Data navigation, e.g. alongside Songs, High Scores, History, and Dani. | Yes |
| Profile section | Put Don Challenge opt-in and progress inside the existing Profile page instead of a standalone page. | |
| Admin-only page | Expose Don Challenge only to admins, not regular logged-in users viewing their own profile. | |

**User's choice:** Play Data page
**Notes:** The page belongs in the user-era Play Data area.

### Page Content

| Option | Description | Selected |
|--------|-------------|----------|
| Progress dashboard | Show opt-in state, active bundle, personal task progress, completion status, and configured song/title reward status. | Partial |
| Simple status | Show only opt-in state and whether the active challenge is configured, leaving detailed task progress out. | |
| Include raw facts | Also expose uploaded raw challenge facts for debugging, separate from derived task progress. | |

**User's choice:** Progress/dashboard cards, but ignore opt-in state.
**Notes:** User clarified: "Actually let's ignore opt-in state (which we already do). So cards are always in."

### Opt-In Handling

| Option | Description | Selected |
|--------|-------------|----------|
| Cards always visible | Do not show an opt-in control on the Don Challenge page; the page focuses on challenge cards/progress. | Yes |
| Profile setting only | Expose opt-in editing on the existing Profile/UserSettings page, separate from the Don Challenge page. | |
| Keep page toggle | Show an editable opt-in toggle on the Don Challenge page before or above the challenge cards. | |

**User's choice:** Cards always visible
**Notes:** User clarified: "And there is no opt-in at all."

### Runtime Opt-In Gate

| Option | Description | Selected |
|--------|-------------|----------|
| Always participating | Treat configured Don Challenge as active for all Red/older-capability users; ignore opt-in for progress, readback, and reward locks. | Yes |
| UI only | Hide opt-in from AdminApi/WebUI but keep the current backend opt-in gate from Phase 21. | |
| Future correction | Record no-opt-in as a later runtime correction outside Phase 22. | |

**User's choice:** Always participating
**Notes:** This supersedes the Phase 21 opt-in gate decisions.

### Older-Era Reuse

| Option | Description | Selected |
|--------|-------------|----------|
| Capability page | Build it as a Don Challenge/ChallengeCompe capability page, shown for Red now and reusable for older eras when they bind the capability. | Yes |
| Red-first page | Implement it as a Red page now and refactor to older-era reuse only when another older era is added. | |
| All AC15 page | Show the page for every AC15 era, with unsupported eras displaying an unavailable state. | |

**User's choice:** Capability page
**Notes:** Red is the first binding, not the page boundary.

### API Contract

| Option | Description | Selected |
|--------|-------------|----------|
| Dedicated API | Add a Don Challenge AdminApi contract with active bundle, task cards, progress, and reward status for the page. | Yes |
| Reuse settings | Fold the needed fields into existing UserSettings/Profile responses even though the page is separate. | |
| You decide | Let the planner choose the smallest API shape that fits the page and existing contracts. | |

**User's choice:** Dedicated API
**Notes:** Do not overload user settings/profile DTOs with Don Challenge page data.

### Reward Visibility

| Option | Description | Selected |
|--------|-------------|----------|
| Show rewards | Show song/title reward targets and whether each is locked, earned, or unavailable from current progress. | Yes |
| Tasks only | Show challenge task progress but omit reward song/title state from the page. | |
| You decide | Let the planner include reward details only where existing data makes it straightforward. | |

**User's choice:** Show rewards
**Notes:** Phase 21 already has configured reward locks/grants, so page cards should show reward status.

### Empty State

| Option | Description | Selected |
|--------|-------------|----------|
| Empty page state | Keep the page reachable for supported eras and show a clear no-active-challenge state without errors. | |
| Hide navigation | Hide the Don Challenge navigation entry unless an active bundle exists. | Yes |
| Admin warning | Show an admin-oriented setup warning or schema/config hint when no active bundle exists. | |

**User's choice:** Hide navigation
**Notes:** Direct route behavior is left to planner discretion, but normal navigation should not advertise an unconfigured challenge.

---

## Final Closeout Evidence

### Runtime Smoke Evidence

| Option | Description | Selected |
|--------|-------------|----------|
| Full matrix | Record Red normal flow, Tokkun tutorial, simple compatibility, Don Challenge progress/readback, and Don Challenge WebUI/AdminApi behavior. | |
| Challenge-focused | Require Don Challenge and a small normal-flow sanity check, but avoid a broader Red runtime matrix. | |
| Automated only | Close with server tests/builds and no cabinet/RPCS3 smoke record. | |

**User's choice:** None of the listed options
**Notes:** User clarified: "All verification is manual. I will report back any issues." Follow-up clarified that meaningful implementation tests are still needed for UI/API interactions.

### Test Scope

| Option | Description | Selected |
|--------|-------------|----------|
| Focused interaction tests | Add targeted AdminApi/WebUI service/page tests for Red routes, Don Challenge page data, navigation gating, and no-cross-era boundaries. | Yes |
| Server API only | Test AdminApi and persistence behavior but skip WebUI/component coverage. | |
| Minimal regression | Only add enough Red regression tests to cover critical route wiring and avoid detailed UI interaction coverage. | |

**User's choice:** Focused interaction tests
**Notes:** User clarified: "Only when meaningful for WebUI."

### Manual Evidence Artifact

| Option | Description | Selected |
|--------|-------------|----------|
| User-reported record | Create a closeout artifact that records manual verification status, issues found, and unresolved manual gaps. | Yes |
| No artifact until pass | Do not create a final verification artifact until manual checks pass. | |
| Agent checklist only | Write a suggested manual checklist but do not record user results in the phase artifacts. | |

**User's choice:** User-reported record
**Notes:** The artifact should not overclaim agent-side cabinet/RPCS3 proof.

### Manual Issue Handling

| Option | Description | Selected |
|--------|-------------|----------|
| Fix in phase | Treat user-reported issues in implemented Phase 22 scope as blockers to closeout and fix them before completion. | Yes |
| Record and defer | Record issues and defer fixes unless they block basic Red runtime operation. | |
| Case by case | Decide per issue whether it belongs to Phase 22 or a future phase. | |

**User's choice:** Fix in phase
**Notes:** User-reported issues in Phase 22 scope block closeout.

## The Agent's Discretion

- Exact AdminApi route names, DTO names, WebUI page/component names, and plan splits.
- Exact direct-route behavior when no active Don Challenge bundle is configured, provided normal navigation hides the page cleanly.

## Deferred Ideas

- Operator schedule editing, authored bundle management UI, JSON/schema editing, and challenge reward-management tooling.
- User-created challenge letters, official/BNG competition buckets, community/global progress aggregation, and `ary_user_compe_*` / `ary_bng_compe_*` UI.
- Red WaiWai, battle, item-shop, medal/shop-season, and compatibility-route diagnostics UI.
