# Task 1: Contracts And Test Wiring

**Goal:** Add the minimal shared DTO shape and test-project references needed for Green AdminApi work.

**Files:**
- Create: `Contracts.AdminApi/ViewModels/ScoreFacet.cs`
- Modify: `Contracts.AdminApi/ViewModels/SongBestData.cs`
- Modify: `Tests/Tests.csproj`
- Create: `Tests/Green/GreenAdminApiControllerTests.cs`

- [ ] **Step 1: Add a paired-score DTO**

Create `Contracts.AdminApi/ViewModels/ScoreFacet.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class ScoreFacet
{
    public string Label { get; set; } = string.Empty;

    public uint BestScore { get; set; }

    public uint BestRate { get; set; }

    public CrownType BestCrown { get; set; }
}
```

- [ ] **Step 2: Add optional alternate score to song best rows**

Modify `Contracts.AdminApi/ViewModels/SongBestData.cs` by adding this property after `BestScoreRank`:

```csharp
public ScoreFacet? AlternateScore { get; set; }
```

- [ ] **Step 3: Reference AdminApi from tests**

Modify `Tests/Tests.csproj` and add this project reference to the existing `<ItemGroup>` with other project references:

```xml
<ProjectReference Include="..\Adapters.AdminApi\Adapters.AdminApi.csproj" />
```

- [ ] **Step 4: Add compile-only AdminApi test scaffold**

Create `Tests/Green/GreenAdminApiControllerTests.cs`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Green;

public class GreenAdminApiControllerTests
{
    [Fact]
    public void ScoreFacet_CanRepresentAlternateGreenScore()
    {
        var row = new SongBestData
        {
            SongId = 101,
            Difficulty = Difficulty.Oni,
            BestScore = 900000,
            BestRate = 90,
            BestCrown = CrownType.Clear,
            BestScoreRank = ScoreRank.None,
            AlternateScore = new ScoreFacet
            {
                Label = "Shin",
                BestScore = 930000,
                BestRate = 93,
                BestCrown = CrownType.Gold
            }
        };

        Assert.Equal("Shin", row.AlternateScore.Label);
        Assert.Equal(ScoreRank.None, row.BestScoreRank);
    }
}
```

- [ ] **Step 5: Run contract compile test**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAdminApiControllerTests.ScoreFacet_CanRepresentAlternateGreenScore"
```

Expected: test passes.

- [ ] **Step 6: Commit**

```powershell
git add -- Contracts.AdminApi/ViewModels/ScoreFacet.cs Contracts.AdminApi/ViewModels/SongBestData.cs Tests/Tests.csproj Tests/Green/GreenAdminApiControllerTests.cs
git commit -m "Add WebUI score facet contract"
```