---
phase: 22
slug: red-adminapi-webui-and-runtime-closeout
status: approved
shadcn_initialized: false
preset: none
created: 2026-06-15
---

# Phase 22 - UI Design Contract

> Visual and interaction contract for Red AdminApi/WebUI routing and the Don Challenge readback page. Generated for `gsd-ui-phase`, verified inline against the current MudBlazor WebUI.

---

## Scope

Phase 22 must keep the existing WebUI shape: a compact MudBlazor admin/player tool with era-parametric pages. Red normal readouts should appear through existing pages once `WebUiEra` and AdminApi support Red:

- `Users/{baid}/{era}/Profile`
- `Users/{baid}/{era}/Songs`
- `Users/{baid}/{era}/HighScores`
- `Users/{baid}/{era}/PlayHistory`
- `Users/{baid}/{era}/Songs/{songId}`
- `Users/{baid}/{era}/DaniDojo`

The only new page in this phase is Don Challenge:

- Route: `Users/{baid}/{era}/DonChallenge`
- Nav label: `Don Challenge`
- API family: `api/{era}/DonChallenge/...`
- First supported binding: Red
- Capability boundary: shared older-AC15 ChallengeCompe, not a Red-only feature name

Out of scope for the UI:

- ChallengeCompe opt-in toggle or editable opt-in state
- Raw uploaded ChallengeCompe facts
- `ary_user_compe_*` or `ary_bng_compe_*` buckets
- compatibility route diagnostics
- WaiWai, battle, item shop, medal/shop-season, or unproven challenge controls
- bundle authoring, schedule editing, JSON editing, or reward management

---

## Design System

| Property | Value |
|----------|-------|
| Tool | none |
| Preset | not applicable |
| Component library | MudBlazor |
| Icon library | MudBlazor Material icons |
| Font | Existing WebUI stack: MudBlazor typography plus `Nijiiro` where the app already applies it |

Use existing `MainLayout`, `NavMenu`, `MudContainer`, `MudGrid`, `MudTable`, `MudTabs`, `MudPaper`, `MudCard`, `MudChip`, `MudProgressLinear`, `MudTooltip`, `MudAlert`, `MudSkeleton`, and `MudProgressCircular` patterns. Do not introduce a landing-page treatment, hero section, marketing copy, decorative gradients, or a new visual theme.

---

## Spacing Scale

Declared values must stay aligned to MudBlazor spacing and multiples of 4.

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | Icon gaps, inline metadata, dense table cells |
| sm | 8px | Compact row gaps, toolbar padding, card inner gaps |
| md | 16px | Default card/content padding and grid spacing |
| lg | 24px | Page section spacing |
| xl | 32px | Tab panel padding and major page blocks |
| 2xl | 48px | Rare full-page separation only |

Exceptions: existing difficulty icons and game imagery may keep their established fixed sizes. New Don Challenge cards must not add spacing values outside this scale.

---

## Typography

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | MudBlazor `Typo.body2` / 14px | 400 | default MudBlazor |
| Label | MudBlazor `Typo.caption` / 12px | 400-500 | default MudBlazor |
| Section heading | MudBlazor `Typo.h5` or `Typo.h6` | 500 | default MudBlazor |
| Card heading | MudBlazor `Typo.subtitle2` or `Typo.body1` | 500-700 | default MudBlazor |
| Display | not used | not used | not used |

Don Challenge has no display-scale type. Page titles and section headings should match existing `DaniDojo`, `SongList`, and `HighScores` density.

---

## Color

| Role | Value | Usage |
|------|-------|-------|
| Dominant (60%) | `var(--mud-palette-background)` | Page background and full-width layout |
| Secondary (30%) | `var(--mud-palette-surface)` / `var(--mud-palette-table-striped)` | Tables, task cards, status strips |
| Accent (10%) | `var(--mud-palette-primary)` from the existing Indigo theme | Active tabs, focus outlines, selected states, primary progress emphasis |
| Success | MudBlazor `Color.Success` | Completed tasks and earned rewards |
| Warning | MudBlazor `Color.Warning` | Locked rewards |
| Info | MudBlazor `Color.Info` | Unavailable configured challenge or neutral capability state |
| Destructive | MudBlazor `Color.Error` | Error states only |

Accent reserved for: active navigation, progress emphasis, focus rings, selected rows, and explicit retry actions. Status must never rely on color alone; pair every status color with text and a Material icon.

---

## Page Layout

### Standard Page Shell

Use the existing authenticated user-era page structure:

- `@page "/Users/{baid:int}/DonChallenge"`
- `@page "/Users/{baid:int}/{era}/DonChallenge"`
- `CurrentEra => WebUiEra.NormalizeOrDefault(Era, AuthService.DefaultEra)`
- existing owner/admin authorization redirect behavior
- breadcrumbs through `BreadcrumbsStateContainer`
- page body inside the existing `MudContainer` supplied by `MainLayout`

The page title should be `Don Challenge`. Do not prefix it with `Red`.

### Navigation

Add Don Challenge inside the existing `Play Data` group in `NavMenu.razor`, adjacent to Song List, High Scores, Play History, and Dani Dojo.

Normal navigation must hide the link unless both are true:

- selected era is a supported older-AC15 ChallengeCompe capability era
- an active configured ChallengeCompe bundle is available for that era

Direct navigation must render a graceful unavailable state instead of redirecting to another era or surfacing a raw server error.

### Summary Band

At the top of the page, use an un-nested `MudGrid` or `MudPaper Outlined` band with three compact cells:

- Active bundle: bundle id and configured window when present
- Tasks: completed personal tasks over configured personal tasks
- Rewards: earned reward count over configured reward thresholds

Use `MudChip` for `Active`, `Unavailable`, and `Completed` states. Avoid large counters, hero composition, or decorative era coloring.

### Task Cards

The main content is a responsive grid of repeated task cards:

- desktop: 2 or 3 columns based on available width
- tablet: 2 columns
- mobile: 1 column

Each card should be a `MudCard Outlined Elevation="0"` or equivalent `MudPaper` with radius no larger than the current MudBlazor default. Required content:

- slot number
- task name
- rule summary, using operator text rather than raw enum names
- completion chip with icon and text
- progress value and target where target is known
- `MudProgressLinear` for executable progress
- song rows when the task has configured songs
- latest/best progress timestamp only when present and useful

Task cards must not render reward rows. Rewards are summarized in the top band and displayed once in the bottom Rewards section, so completion-threshold rewards such as slot 8 and slot 10 are not duplicated inside task cards.

Task cards must not expose raw facts, upload arrays, wire field names, or database row names.

### Song Rows And Rewards Section

Song rows should reuse existing song display conventions:

- song title and artist from `GameDataService`
- difficulty icon or level when available
- link reward songs and task songs to `WebUiEra.UserRoute(Baid, CurrentEra, $"Songs/{songId}")` when the song exists in the current era catalog

Reward rows should appear only in the bottom Rewards section and should use compact status chips:

- `Earned` with check icon and success color
- `Locked` with lock icon and warning color
- `Unavailable` with info icon and neutral/info color

Reward titles should display localized title names when available, otherwise `#<id>` fallback matching existing customization picker behavior.

---

## State Contract

| State | UI Behavior |
|-------|-------------|
| Loading | Use the existing centered `MudProgressCircular` for first load, or skeleton task rows if the page shell is already visible. |
| Available | Render summary band, task cards, and reward rows from the Don Challenge API response. |
| No active bundle | Render `No active Don Challenge` and `No active Don Challenge is configured for this era.` as a neutral info state. |
| Unsupported era | Render `Don Challenge is not available for {era}.` on direct routes; hide from normal navigation. |
| No user progress | Render configured task cards with progress `0` or `No progress`; do not treat this as an empty page. |
| Error | Render `Unable to load Don Challenge.` plus a compact `Retry` button with a refresh icon. |
| Unauthorized | Reuse existing profile/play-data redirect behavior. |

Do not normalize unsupported or failed Don Challenge routes back to Nijiiro. A Red route must remain Red, and an unsupported route must fail visibly but cleanly.

---

## Data Contract

The page must consume a dedicated Don Challenge AdminApi contract. Do not add Don Challenge fields to `UserSetting`.

Preferred routes:

- `GET api/{era}/DonChallenge/availability`
- `GET api/{era}/DonChallenge/{baid}`

Minimum response shape for `GET api/{era}/DonChallenge/{baid}`:

| Field | Purpose |
|-------|---------|
| `isAvailable` | Active configured bundle exists and the era supports the capability. |
| `era` | Normalized era name returned by the server. |
| `bundleId` | Active bundle id. |
| `startsAt` / `endsAt` | Optional configured window. |
| `completedTaskCount` | Completed personal task count for the user. |
| `personalTaskCount` | Configured personal task count. |
| `tasks` | Task cards with task id, slot, name, rule summary, progress, completion, track/song rows. |
| `rewards` | Reward thresholds with song/title ids and status. |

Task row fields:

| Field | Purpose |
|-------|---------|
| `taskId` | Configured personal task id. |
| `slot` | Display order. |
| `name` | Display name from sidecar catalog. |
| `ruleLabel` | User-facing rule summary assembled server-side or from a stable UI helper. |
| `progressValue` | Current progress value. |
| `targetValue` | Target value when known. |
| `completed` | Completion status. |
| `updatedAt` / `completedAt` | Optional timestamps. |
| `tracks` | Configured task song rows, not raw uploaded facts. |

Reward row fields:

| Field | Purpose |
|-------|---------|
| `requiredCompletedTasks` | Completion threshold. |
| `rewardSongNoes` | Configured reward songs. |
| `rewardTitleIds` | Configured reward titles. |
| `status` | `Earned`, `Locked`, or `Unavailable`. |

Navigation availability may be cached in a scoped WebUI service, but it must refresh when `selectedEra` changes. The service must request `WebUiEra.Api(selectedEra, "DonChallenge/availability")` and must not infer availability from the presence of Red in `EnabledEras` alone.

---

## Copywriting Contract

| Element | Copy |
|---------|------|
| Nav link | Don Challenge |
| Page title | Don Challenge |
| Summary labels | Active bundle, Tasks, Rewards |
| Section headings | Tasks, Rewards |
| Status chips | Active, Completed, In progress, Earned, Locked, Unavailable |
| Primary CTA | none |
| Error CTA | Retry |
| Empty state heading | No active Don Challenge |
| Empty state body | No active Don Challenge is configured for this era. |
| Unsupported state | Don Challenge is not available for {era}. |
| Error state | Unable to load Don Challenge. |
| Destructive confirmation | not applicable |

Do not use visible helper text to explain ChallengeCompe internals, protocol compatibility, opt-in behavior, or how the cabinet uses the feature.

---

## Interaction Contract

- The Don Challenge page is read-only in Phase 22.
- There is no opt-in switch, checkbox, segmented control, or edit form.
- Song references may navigate to existing song detail pages.
- Retry is the only page action, and only appears after a load failure.
- Status chips are informational and not clickable.
- The page must preserve the current era in every route and API call.
- Direct links for unsupported eras must not mutate the selected era in `NavMenu`.
- Long task names, song titles, and title names must wrap without overlapping adjacent content.

---

## Accessibility

- Icon-only buttons must have `aria-label` or MudBlazor tooltip text.
- Progress bars must include visible numeric text or adjacent status text.
- Completion and reward state must be readable from text, not color alone.
- Task cards must have stable heights where practical; loading, empty, and error states must not cause overlapping content.
- Keyboard focus must remain visible through MudBlazor focus outlines.
- Mobile layout must remain one column without horizontal page scrolling.

---

## Registry Safety

| Registry | Blocks Used | Safety Gate |
|----------|-------------|-------------|
| shadcn official | none | not required |
| third-party | none | not required |

No registry packages, shadcn blocks, or third-party UI snippets are approved for this phase.

---

## Verification Contract

Implementation plans should include focused checks for the UI behavior this contract creates:

- `WebUiEra` keeps Red supported and AC15-classified.
- Existing generic pages request `/api/Red/...` through `WebUiEra.Api`.
- Don Challenge service requests `api/Red/DonChallenge/availability`.
- Don Challenge page requests `api/Red/DonChallenge/{baid}`.
- Navigation hides Don Challenge when availability is false.
- Direct unavailable routes render the neutral unavailable state.
- The Don Challenge API/page contract omits raw facts and opt-in fields.
- Red routes do not read or write another era's gameplay state.

Recommended focused command once implemented:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~RedAdminApi|FullyQualifiedName~RedDonChallenge|FullyQualifiedName~GameDataServiceTests"
```

Before closeout, pair automated WebUI/AdminApi checks with the phase-level full suite and temp-output Host build from `22-VALIDATION.md`.

---

## Checker Sign-Off

- [x] Dimension 1 Copywriting: PASS
- [x] Dimension 2 Visuals: PASS
- [x] Dimension 3 Color: PASS
- [x] Dimension 4 Typography: PASS
- [x] Dimension 5 Spacing: PASS
- [x] Dimension 6 Registry Safety: PASS

**Approval:** approved 2026-06-15
