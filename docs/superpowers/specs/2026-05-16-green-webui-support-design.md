# Green WebUI Support Design

Date: 2026-05-16

## Context

TaikoWebUI is currently a shared Blazor/MudBlazor frontend, but its AdminApi backing endpoints are effectively Nijiiro-only. Pages such as Profile, High Scores, Play History, Song List, Song detail, and Dani Dojo call routes like `api/PlayData/{baid}` and those controllers read Nijiiro save, score, play-log, favorite, Dan, and catalog data.

Green support now exists in the server domain for the first gameplay features: Green save data, score/play rows, normal and Shin score rows, favorites capped at five, recent songs, pushed/recommend data, and Green-specific Dani state. The WebUI needs initial Green support without forking into a second app.

## Goals

- Add an explicit WebUI mode/version selector so users can switch between Nijiiro and Green, and so future versions can be added with the same pattern.
- Use canonical era-aware URLs instead of hidden local UI state.
- Reuse the existing WebUI pages and components where the concepts are shared.
- Support Green score/result display, shared card management, Green Dan display, and favorite song selection.
- Hide Green costumes and detailed settings until those features are implemented, with explicit TODO comments at the code gates.
- Keep Green/Nijiiro persistence differences contained in AdminApi projection/mapping code instead of leaking table-specific logic through the UI.

## Non-Goals

- Implement Green costume management.
- Implement Green detailed settings editing.
- Rework the visual design of the WebUI.
- Split cards by era. Access codes remain shared account/card state.
- Add all future AC15 versions now; the design only makes room for them.

## Routing

Canonical WebUI routes include the selected era after the user id:

```text
/Users/{baid}/{era}/Profile
/Users/{baid}/{era}/Songs
/Users/{baid}/{era}/Songs/{songId}
/Users/{baid}/{era}/HighScores
/Users/{baid}/{era}/PlayHistory
/Users/{baid}/{era}/DaniDojo
```

Shared account/card routes stay era-neutral:

```text
/Users/{baid}/AccessCode
/ChangePassword
/Users
/Login
/Register
```

The mode selector navigates to the equivalent route for the selected era when possible. Existing Nijiiro routes may remain as compatibility aliases that redirect to or render as Nijiiro.

AdminApi mirrors the same resource identity:

```text
api/{era}/UserSettings/{baid}
api/{era}/PlayData/{baid}
api/{era}/PlayHistory/{baid}
api/{era}/DanBestData/{baid}
api/{era}/FavoriteSongs
api/{era}/FavoriteSongs/{baid}
api/{era}/GameData/MusicDetails
api/{era}/GameData/DanData
```

The server parses `{era}` into `GameEra`. Unsupported era values return a consistent client error instead of falling back silently.

## Architecture

Use shared pages with era-aware routes and era-aware AdminApi dispatch:

```text
WebUI route
  /Users/{baid}/{era}/HighScores
        |
        v
WebUI page parses era and calls
  api/{era}/PlayData/{baid}
        |
        v
AdminApi controller validates auth/user once
        |
        +-- Nijiiro projection -> existing Nijiiro tables/catalog
        |
        +-- Green projection   -> Green tables/catalog
        |
        v
Shared WebUI contract models
```

Controllers should remain thin dispatchers. Era-specific projection helpers own the table queries, identifier translation, and enum normalization.

## Contracts And Normalization

Keep current WebUI view models mostly intact, but make era differences explicit.

`SongBestData` continues to represent one song/difficulty row. For Green, normal and Shin score records are displayed together on the same row when both exist. Add an optional nested score facet such as `AlternateScore` with score, rate, crown, and label/key. The primary score remains the row's existing `BestScore`, `BestRate`, and `BestCrown` fields. Nijiiro leaves `AlternateScore` empty.

Green does not have Nijiiro score ranks. Green projections set `BestScoreRank = ScoreRank.None`, and pages naturally hide rank icons when the value is `None`.

Green crowns are a subset of Nijiiro crowns: none, clear, and full combo. Green projections must not invent Dondaful/perfect crown states. WebUI pages hide perfect-crown summaries for Green.

`SongHistoryData` remains the play-log row contract. Green rows include score, crown, hit counts, play time, and favorite state. Green play options can remain default/empty until `OptionFlg` decoding is intentionally implemented.

`DanBestData` remains the WebUI Dan display contract. Green `GreenDanClearGrade` maps only to states the Green client actually has: none, clear, and gold clear. Green Dan numbering differs from Nijiiro and must be resolved through the Green Dan catalog rather than reused from Nijiiro assumptions.

`SetFavoriteRequest` should remain semantic from the UI perspective. The WebUI sends a song id plus desired favorite state. The Green AdminApi branch owns translation between catalog song id and Green `SongNo` persistence.

## AdminApi Behavior

`PlayDataController`:

- `GET api/Nijiiro/PlayData/{baid}` returns the current Nijiiro projection.
- `GET api/Green/PlayData/{baid}` reads `SongBestDataGreen`, `SongPlayDataGreen`, and `GreenFavoriteSongs`.
- Green normal and Shin best rows are paired into one `SongBestData` row per song/difficulty when possible.
- Nijiiro compatibility route `api/PlayData/{baid}` may remain and call the Nijiiro branch.

`PlayHistoryController`:

- Dispatches to Nijiiro or Green play-log tables.
- Green rows set score rank to `None` and use only supported crown states.

`FavoriteSongsController`:

- Nijiiro updates `UserSaveDataNijiiro.FavoriteSongsArray` as today.
- Green updates `GreenFavoriteSongs`, translates ids through the Green catalog, and enforces the Green cap of five favorites server-side.
- Over-cap Green favorite additions should return a clear failure status so the UI can keep the toggle unchanged and show feedback.

`DanBestDataController`:

- Nijiiro reads existing normal Dan rows.
- Green reads `DanScoreDataGreen` and maps clear grade and Dan ids through Green catalog data.
- Green extra/normal handling should follow the existing Green Dan data model; the first UI slice may show normal Dans first as long as the mapping does not make Nijiiro numbering assumptions.

`GameDataController`:

- Music details and Dan data become era-aware.
- Green pages must use the Green music/Dan catalogs so song ids, names, star levels, and Dan definitions match Green persisted data.
- Costume/title catalog calls can return limited data or keep Nijiiro behavior only where the corresponding Green UI controls are hidden.

## WebUI Behavior

All gameplay/profile pages gain an `Era` route parameter. A small helper should parse valid eras, build equivalent era-aware URLs, and centralize API URL construction. Invalid era values show an unsupported/not-found state.

`NavMenu` and user cards expose a mode selector for Nijiiro and Green. The selector navigates to the same conceptual page for the same `baid`. Access-code/card pages remain shared and do not include `{era}`.

`Profile` reuses the current page. For Green, it shows identity/nameplate, supported Dan display, and score summary data. It hides costumes, title editing, detailed settings, and unsupported options. Each Green branch that hides unfinished controls should include a TODO comment identifying the postponed Green feature.

`HighScores`, `PlayHistory`, `SongList`, and `Song` reuse existing layouts. For Green, they hide score-rank and perfect-crown visuals, show paired normal/Shin score facets where available, and use the Green catalog for names and stars.

Favorite toggles enforce the Green five-favorite cap in WebUI before posting. The server also enforces the cap. If the server rejects an update, the UI leaves the local toggle unchanged and shows feedback.

`DaniDojo` reuses the current display structure but loads era-specific Dan catalog and best rows. Green clear states display only none, clear, and gold clear. Green Dan ids are catalog-driven.

`AccessCode` stays shared and keeps existing route behavior.

## Error Handling

- Invalid era in WebUI route: show unsupported/not-found state.
- Invalid era in AdminApi route: return a consistent 400 or 404.
- Missing user/card authorization remains unchanged.
- Green favorite cap violation: return a clear client error; UI does not optimistically keep the over-cap state.
- Missing Green catalog mapping for a persisted row: omit that row from catalog-dependent UI lists or mark it unresolved without crashing the whole page.

## Testing

Backend tests should cover:

- Green `PlayData` projection, including paired normal/Shin score rows.
- Green `PlayHistory` projection with rank omitted and unsupported crown states not emitted.
- Green favorite add/remove behavior, including cap of five and id translation.
- Green `DanBestData` projection with Green clear grade and catalog-driven numbering.
- Nijiiro compatibility routes still returning Nijiiro data.

WebUI tests or build-time checks should cover:

- Era-aware route parameters on shared pages.
- Correct API URL construction from selected era.
- Green display branches hiding rank/perfect crown/costume/settings controls.
- Favorite toggle behavior at the Green cap.

Verification should include `dotnet test` for the relevant test project and a WebUI build.