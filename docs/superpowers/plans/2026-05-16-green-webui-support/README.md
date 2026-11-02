# Green WebUI Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add initial Green support to TaikoWebUI through URL-scoped era selection, era-aware AdminApi routes, normalized Green score/Dan/favorite projections, and shared page reuse.

**Architecture:** Keep WebUI pages shared and pass `{era}` through routes and API URLs. AdminApi controllers become thin era dispatchers that project Nijiiro and Green tables into existing shared contracts, with small contract additions for Green paired score tracks. Card/account routes remain shared and era-neutral.

**Tech Stack:** .NET 10, ASP.NET Core controllers, EF Core/SQLite tests, Blazor WebAssembly, MudBlazor, Contracts.AdminApi shared DTOs.

---

## File Structure

- `Contracts.AdminApi/ViewModels/ScoreFacet.cs`: new optional score facet for Green paired score display.
- `Contracts.AdminApi/ViewModels/SongBestData.cs`: add `AlternateScore` and helper display flags if needed.
- `Contracts.AdminApi/Requests/SetFavoriteRequest.cs`: keep the existing semantic request shape unless implementation discovers Green needs a disambiguating field.
- `Adapters.AdminApi/Controllers/EraRoute.cs`: new helper for parsing route era values into `GameEra` and returning consistent invalid-era responses.
- `Adapters.AdminApi/Controllers/PlayDataController.cs`: add `api/{era}/PlayData/{baid}` and Green projection while preserving Nijiiro compatibility route.
- `Adapters.AdminApi/Controllers/PlayHistoryController.cs`: add era route and Green projection.
- `Adapters.AdminApi/Controllers/FavoriteSongsController.cs`: add era route, Green favorite add/remove, Green cap enforcement.
- `Adapters.AdminApi/Controllers/DanBestDataController.cs`: add era route and Green Dan projection.
- `Adapters.AdminApi/Controllers/GameDataController.cs`: add era-aware catalog routes.
- `Tests/Tests.csproj`: reference `Adapters.AdminApi` so controller/projection tests can compile.
- `Tests/Green/GreenAdminApiControllerTests.cs`: new backend tests for Green AdminApi projections and favorite cap.
- `TaikoWebUI/Utilities/WebUiEra.cs`: new WebUI helper for parsing/building era route and API fragments.
- `TaikoWebUI/Services/IGameDataService.cs`: add era-aware catalog methods.
- `TaikoWebUI/Services/GameDataService.cs`: cache music/Dan data per era and call era-aware API routes.
- `TaikoWebUI/Components/NavMenu.razor`: add mode selector and era-aware gameplay links.
- `TaikoWebUI/Components/UserCard.razor`: add era-aware profile/play-data links.
- `TaikoWebUI/Pages/Profile.razor` and `.razor.cs`: add era route, hide unsupported Green controls with TODO comments.
- `TaikoWebUI/Pages/HighScores.razor` and `.razor.cs`: add era route/API calls and Green display branches.
- `TaikoWebUI/Pages/PlayHistory.razor` and `.razor.cs`: add era route/API calls and Green display branches.
- `TaikoWebUI/Pages/SongList.razor` and `.razor.cs`: add era route/API calls and Green favorite cap branch.
- `TaikoWebUI/Pages/Song.razor` and `.razor.cs`: add era route/API calls and Green favorite cap branch.
- `TaikoWebUI/Pages/DaniDojo.razor` and `.razor.cs`: add era route/API calls and Green Dan clear mapping display.

## Stage Files

1. [01-contracts-and-test-wiring.md](01-contracts-and-test-wiring.md)
2. [02-adminapi-score-history-favorites.md](02-adminapi-score-history-favorites.md)
3. [03-adminapi-dan-and-gamedata.md](03-adminapi-dan-and-gamedata.md)
4. [04-webui-era-routing-services.md](04-webui-era-routing-services.md)
5. [05-webui-page-adaptation.md](05-webui-page-adaptation.md)
6. [06-final-verification.md](06-final-verification.md)

## Execution Order

Execute stages in order. Each stage ends with tests/build and a commit. Do not start WebUI page adaptation before the AdminApi Green routes compile and pass tests.