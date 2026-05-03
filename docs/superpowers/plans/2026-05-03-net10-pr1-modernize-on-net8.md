# .NET 10 Upgrade — PR1: Modernize on .NET 8

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Modernize project infrastructure (Central Package Management, Directory.Build.props, .slnx, deduped versions) without changing the runtime. The solution stays on .NET 8 throughout this PR; PR2 (separate plan) does the runtime move.

**Architecture:** Introduce three repo-root MSBuild files (`Directory.Build.props`, `Directory.Packages.props`, `global.json`), then strip the now-redundant per-project boilerplate. Replace the `.sln` with `.slnx`. No source code in `Controllers/`, `Handlers/`, `Mappers/`, `Services/`, `GameDatabase/Entities/`, or `*.razor` is touched.

**Tech Stack:** .NET 8 SDK (no upgrade in this PR), MSBuild, NuGet Central Package Management, `.slnx` solution format.

**Branch:** `dev/modernize-on-net8` off `dev`.

---

## Context — read these first

Both files are required reading before executing any task. They give the file-layout, conventions, and decisions this plan assumes:

- `CLAUDE.md` (repo root) — solution layout, build commands, configuration model.
- `docs/superpowers/specs/2026-05-03-net10-upgrade-design.md` — full spec with all three PRs and rationale.

This plan covers **only Section 3 of the spec (PR1)**.

## File structure

**Files created:**
- `Directory.Build.props` (repo root)
- `Directory.Packages.props` (repo root)
- `global.json` (repo root)
- `TaikoLocalServer.slnx` (repo root)

**Files modified:**
- `SharedProject/SharedProject.csproj` — drop inherited props, drop versions, bump Throw 1.3.0 → 1.4.0.
- `GameDatabase/GameDatabase.csproj` — drop inherited props, drop versions, align EF Core to 8.0.4.
- `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj` — drop inherited props, drop versions, align EF Core to 8.0.4, bump SharpZipLib 1.4.0 → 1.4.2.
- `TaikoWebUI/TaikoWebUI.csproj` — drop inherited props, drop versions; preserve all other custom blocks.
- `TaikoLocalServer/TaikoLocalServer.csproj` — drop inherited props, drop versions; preserve all custom blocks (`<BuildTime>`, certificate/configuration/datatable `<None>`/`<Content>` items, `<PublishSingleFile>` on Release).

**Files deleted:**
- `TaikoLocalServer.sln` (replaced by `.slnx`).

**Files NOT touched:**
- `TaikoLocalServer.sln.DotSettings` — kept; ReSharper picks it up by basename match.
- All source files under `*/`.

## Pre-flight

- [ ] **Step 0.1: Verify clean working tree**

Run:
```bash
git status
```
Expected: `nothing to commit, working tree clean`. If not clean, stash or commit first.

- [ ] **Step 0.2: Create branch**

Run:
```bash
git checkout dev
git pull --ff-only
git checkout -b dev/modernize-on-net8
```

- [ ] **Step 0.3: Confirm .NET 8 SDK is installed**

Run:
```bash
dotnet --version
```
Expected: `8.0.x`. If you have multiple SDKs installed, the `global.json` we add later will pin the band.

- [ ] **Step 0.4: Verify baseline build is green before any change**

Run:
```bash
dotnet restore
dotnet build
```
Expected: build succeeds. If it fails on the unmodified `dev` branch, stop and investigate before continuing.

---

## Task 1: Add `Directory.Build.props`

**Files:**
- Create: `Directory.Build.props`

- [ ] **Step 1.1: Create the file**

Write `D:\TaikoLocalServer\Directory.Build.props` with this exact content:

```xml
<Project>

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>12</LangVersion>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

</Project>
```

These properties become defaults inherited by every `.csproj` in the solution. Per-project overrides are still allowed.

- [ ] **Step 1.2: Verify the build still succeeds**

Run:
```bash
dotnet build
```
Expected: build succeeds. (No project removed its local `<TargetFramework>` etc. yet, so behavior is unchanged.)

- [ ] **Step 1.3: Commit**

```bash
git add Directory.Build.props
git commit -m "Add Directory.Build.props with shared project defaults"
```

---

## Task 2: Add `Directory.Packages.props` (Central Package Management)

**Files:**
- Create: `Directory.Packages.props`

This file consolidates every package version used across the solution. After PR1, no `.csproj` should specify `Version="…"` on a `<PackageReference>`.

- [ ] **Step 2.1: Create the file**

Write `D:\TaikoLocalServer\Directory.Packages.props` with this exact content:

```xml
<Project>

  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <!-- ASP.NET Core / Blazor (server hosts WASM, so these appear in TaikoLocalServer too) -->
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.4" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.4" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.4" />
    <PackageVersion Include="Microsoft.AspNetCore.Components.WebAssembly.Server" Version="8.0.4" />
    <PackageVersion Include="Microsoft.AspNetCore.ResponseCompression" Version="2.3.0" />

    <!-- Entity Framework Core -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.4" />
    <PackageVersion Include="EntityFrameworkCore.Exceptions.Sqlite" Version="8.1.0" />

    <!-- Microsoft.Extensions -->
    <PackageVersion Include="Microsoft.Extensions.Localization" Version="8.0.4" />
    <PackageVersion Include="Microsoft.Extensions.Localization.Abstractions" Version="8.0.4" />
    <PackageVersion Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />

    <!-- Server-side dependencies -->
    <PackageVersion Include="MediatR" Version="12.2.0" />
    <PackageVersion Include="protobuf-net" Version="3.2.30" />
    <PackageVersion Include="protobuf-net.AspNetCore" Version="3.2.12" />
    <PackageVersion Include="Riok.Mapperly" Version="3.5.1" />
    <PackageVersion Include="Serilog.AspNetCore" Version="8.0.2-dev-00334" />
    <PackageVersion Include="Serilog.Expressions" Version="4.0.0" />
    <PackageVersion Include="Serilog.Sinks.File.Header" Version="1.0.2" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageVersion Include="Yoh.Text.Json.NamingPolicies" Version="1.1.2" />

    <!-- Cross-project shared -->
    <PackageVersion Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageVersion Include="DotNetZip" Version="1.16.0" />
    <PackageVersion Include="Otp.NET" Version="1.4.0" />
    <PackageVersion Include="SharpZipLib" Version="1.4.2" />
    <PackageVersion Include="Swan.Core" Version="7.0.0-beta.2" />
    <PackageVersion Include="Swan.Logging" Version="6.0.2-beta.96" />
    <PackageVersion Include="Throw" Version="1.4.0" />

    <!-- TaikoWebUI (Blazor WASM) -->
    <PackageVersion Include="MudBlazor" Version="6.20.0" />
    <PackageVersion Include="CodeBeam.MudBlazor.Extensions" Version="6.9.2" />
    <PackageVersion Include="Blazored.LocalStorage" Version="4.5.0" />
    <PackageVersion Include="Markdig" Version="0.37.0" />
    <PackageVersion Include="Autocomplete.Clients" Version="1.1.0" />
    <PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="7.7.1" />

    <!-- LocalSaveModScoreMigrator -->
    <PackageVersion Include="JorgeSerrano.Json.JsonSnakeCaseNamingPolicy" Version="0.9.0" />
    <PackageVersion Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
  </ItemGroup>

</Project>
```

This file lists every package currently referenced anywhere in the solution at its **deduped** version (highest version where projects disagreed today: `Microsoft.EntityFrameworkCore` 8.0.4 wins over 8.0.3; `SharpZipLib` 1.4.2 wins over 1.4.0; `Throw` 1.4.0 wins over 1.3.0; `Microsoft.EntityFrameworkCore.Sqlite/Tools` 8.0.4 wins over `8.0.0-rc.2.23480.1`).

- [ ] **Step 2.2: Verify the build still succeeds**

Run:
```bash
dotnet restore
dotnet build
```
Expected: build succeeds. CPM is now active but no `.csproj` has been modified, so each `<PackageReference>` still has its own `Version` attribute. NuGet 6.x prefers the project-level version when both are present, so this is intentionally a no-op here. We strip the local versions in Tasks 4–8.

- [ ] **Step 2.3: Commit**

```bash
git add Directory.Packages.props
git commit -m "Add Directory.Packages.props with deduped package versions"
```

---

## Task 3: Add `global.json`

**Files:**
- Create: `global.json`

- [ ] **Step 3.1: Detect installed .NET 8 SDK band**

Run:
```bash
dotnet --list-sdks
```
Note the latest 8.0.x version listed (e.g. `8.0.404`). Use that exact version in the next step. If only 8.0.100 or similar is installed, use that.

- [ ] **Step 3.2: Create the file**

Write `D:\TaikoLocalServer\global.json` with this content (replace `8.0.100` with whatever the previous step printed; the `rollForward: latestFeature` setting allows higher 8.0.x patch levels):

```json
{
  "sdk": {
    "version": "8.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

- [ ] **Step 3.3: Verify the SDK pin is honored**

Run:
```bash
dotnet --version
```
Expected: an `8.0.x` version, not `9.x` or `10.x`. If you see a higher major, the `global.json` is pinning correctly to 8.

- [ ] **Step 3.4: Verify the build still succeeds**

Run:
```bash
dotnet build
```
Expected: build succeeds.

- [ ] **Step 3.5: Commit**

```bash
git add global.json
git commit -m "Pin .NET 8 SDK via global.json"
```

---

## Task 4: Strip versions and inherited props from `SharedProject.csproj`

**Files:**
- Modify: `SharedProject/SharedProject.csproj`

- [ ] **Step 4.1: Replace the file content**

Overwrite `D:\TaikoLocalServer\SharedProject\SharedProject.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <ItemGroup>
      <PackageReference Include="Throw" />
    </ItemGroup>

</Project>
```

Changes from before:
- Removed the `<PropertyGroup>` block (TFM, ImplicitUsings, Nullable, LangVersion now inherited from `Directory.Build.props`).
- Removed `Version="1.3.0"` from the `Throw` reference (now centralized to 1.4.0 in CPM, an automatic bump).

- [ ] **Step 4.2: Verify SharedProject builds**

Run:
```bash
dotnet build SharedProject/SharedProject.csproj
```
Expected: build succeeds.

- [ ] **Step 4.3: Commit**

```bash
git add SharedProject/SharedProject.csproj
git commit -m "Migrate SharedProject to centralized props and CPM"
```

---

## Task 5: Strip versions and inherited props from `GameDatabase.csproj` (also fixes EF Core mix)

**Files:**
- Modify: `GameDatabase/GameDatabase.csproj`

- [ ] **Step 5.1: Replace the file content**

Overwrite `D:\TaikoLocalServer\GameDatabase\GameDatabase.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <ItemGroup>
        <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="BCrypt.Net-Next" />
        <PackageReference Include="EntityFrameworkCore.Exceptions.Sqlite" />
        <PackageReference Include="Microsoft.EntityFrameworkCore" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
    </ItemGroup>

</Project>
```

Changes:
- Removed the `<PropertyGroup>` block (now inherited).
- Removed all `Version="…"` attributes.
- Implicitly aligns `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Tools` from the `8.0.0-rc.2.23480.1` prerelease to stable `8.0.4` via CPM.
- Implicitly aligns `Microsoft.EntityFrameworkCore` from `8.0.3` to `8.0.4` via CPM.

- [ ] **Step 5.2: Verify GameDatabase builds**

Run:
```bash
dotnet build GameDatabase/GameDatabase.csproj
```
Expected: build succeeds. NuGet may log "Microsoft.EntityFrameworkCore.Sqlite version was determined to be 8.0.4" — this confirms the dedupe took effect.

- [ ] **Step 5.3: Commit**

```bash
git add GameDatabase/GameDatabase.csproj
git commit -m "Migrate GameDatabase to centralized props and CPM (aligns EF Core to 8.0.4)"
```

---

## Task 6: Strip versions and inherited props from `LocalSaveModScoreMigrator.csproj`

**Files:**
- Modify: `LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj`

- [ ] **Step 6.1: Replace the file content**

Overwrite `D:\TaikoLocalServer\LocalSaveModScoreMigrator\LocalSaveModScoreMigrator.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <Version>1.0.0-beta1</Version>
    </PropertyGroup>

    <ItemGroup>
      <PackageReference Include="JorgeSerrano.Json.JsonSnakeCaseNamingPolicy" />
      <PackageReference Include="Microsoft.EntityFrameworkCore" />
      <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
      <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
      <PackageReference Include="SharpZipLib" />
      <PackageReference Include="System.CommandLine" />
    </ItemGroup>

    <ItemGroup>
      <ProjectReference Include="..\GameDatabase\GameDatabase.csproj" />
    </ItemGroup>

</Project>
```

Changes:
- Kept `<OutputType>Exe</OutputType>` and `<Version>` (project-specific assembly version).
- Removed TFM/Nullable/ImplicitUsings/LangVersion from PropertyGroup (now inherited).
- Removed all `Version="…"` attributes.
- Implicit bumps via CPM: `Microsoft.EntityFrameworkCore.Sqlite/Tools` from `8.0.0-rc.2.23480.1` → `8.0.4`; `Microsoft.EntityFrameworkCore` from `8.0.3` → `8.0.4`; `SharpZipLib` from `1.4.0` → `1.4.2`.

- [ ] **Step 6.2: Verify LocalSaveModScoreMigrator builds**

Run:
```bash
dotnet build LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj
```
Expected: build succeeds.

- [ ] **Step 6.3: Commit**

```bash
git add LocalSaveModScoreMigrator/LocalSaveModScoreMigrator.csproj
git commit -m "Migrate LocalSaveModScoreMigrator to centralized props and CPM"
```

---

## Task 7: Strip versions and inherited props from `TaikoWebUI.csproj`

**Files:**
- Modify: `TaikoWebUI/TaikoWebUI.csproj`

The TaikoWebUI csproj has many custom blocks (resx generators, content copy rules, _ContentIncludedByDefault excludes). These ALL stay; only `<PropertyGroup>` defaults and `Version="…"` attributes are stripped.

- [ ] **Step 7.1: Read the current file in full first**

Run:
```bash
cat TaikoWebUI/TaikoWebUI.csproj
```
Expected: the existing file. Note the property group, the `<ItemGroup>` blocks for resx, `<_ContentIncludedByDefault>` excludes, and `<UpToDateCheckInput>` removes — all of these are kept.

- [ ] **Step 7.2: Replace the file content**

Overwrite `D:\TaikoLocalServer\TaikoWebUI\TaikoWebUI.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

  <PropertyGroup>
    <WasmEnableSIMD>false</WasmEnableSIMD>
    <WasmEnableExceptionHandling>false</WasmEnableExceptionHandling>
    <BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>
    <InvariantGlobalization>false</InvariantGlobalization>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Autocomplete.Clients" />
    <PackageReference Include="Blazored.LocalStorage" />
    <PackageReference Include="CodeBeam.MudBlazor.Extensions" />
    <PackageReference Include="Markdig" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" PrivateAssets="all" />
    <PackageReference Include="Microsoft.Extensions.Localization" />
    <PackageReference Include="Microsoft.Extensions.Localization.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" />
    <PackageReference Include="MudBlazor" />
    <PackageReference Include="Otp.NET" />
    <PackageReference Include="SharpZipLib" />
    <PackageReference Include="Swan.Core" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!--Settings and Dashboard-->
    <Content Update="wwwroot\appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\Dashboard.md">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>


    <!--Encrypted Game Datatables-->
    <Content Update="wwwroot\data\datatable\wordlist.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\musicinfo.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\music_order.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>


    <!--Decrypted Game Datatables-->
    <Content Update="wwwroot\data\datatable\wordlist.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\music_order.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\musicinfo.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <ItemGroup>
    <_ContentIncludedByDefault Remove="Pages\Pages\AccessCode.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\ChangePassword.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\DaniDojo.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Dashboard.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Dialogs\AccessCodeDeleteConfirmDialog.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Dialogs\ChooseTitleDialog.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Dialogs\UserDeleteConfirmDialog.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Dialogs\UserQrCodeDialog.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\HighScores.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Profile.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Register.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Users.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Dialogs\OTPDialog.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Dialogs\ResetPasswordConfirmDialog.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Login.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\PlayHistory.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\Song.razor" />
    <_ContentIncludedByDefault Remove="Pages\Pages\SongList.razor" />
    <_ContentIncludedByDefault Remove="wwwroot\css\bootstrap\bootstrap.min.css" />
    <_ContentIncludedByDefault Remove="wwwroot\css\bootstrap\bootstrap.min.css.map" />
  </ItemGroup>

  <ItemGroup>
    <EmbeddedResource Update="Localization\LocalizationResource.fr-FR.resx">
      <SubType>Designer</SubType>
      <CustomToolNamespace>LocalizationResource</CustomToolNamespace>
      <Generator>ResXFileCodeGenerator</Generator>
    </EmbeddedResource>
    <EmbeddedResource Update="Localization\LocalizationResource.en-US.resx">
      <SubType>Designer</SubType>
      <Generator>ResXFileCodeGenerator</Generator>
      <CustomToolNamespace>LocalizationResource</CustomToolNamespace>
    </EmbeddedResource>
    <EmbeddedResource Update="Localization\LocalizationResource.zh-Hans.resx">
      <SubType>Designer</SubType>
      <Generator>ResXFileCodeGenerator</Generator>
      <CustomToolNamespace>LocalizationResource</CustomToolNamespace>
    </EmbeddedResource>
    <EmbeddedResource Update="Localization\LocalizationResource.resx">
      <Generator>ResXFileCodeGenerator</Generator>
      <LastGenOutput>LocalizationResource.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="Localization\LocalizationResource.ja.resx">
      <SubType>Designer</SubType>
      <Generator>ResXFileCodeGenerator</Generator>
      <CustomToolNamespace>LocalizationResource</CustomToolNamespace>
    </EmbeddedResource>
    <EmbeddedResource Update="Localization\LocalizationResource.zh-Hant.resx">
      <SubType>Designer</SubType>
      <Generator>ResXFileCodeGenerator</Generator>
      <CustomToolNamespace>LocalizationResource</CustomToolNamespace>
    </EmbeddedResource>
  </ItemGroup>

  <ItemGroup>
    <Compile Update="Localization\LocalizationResource.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>LocalizationResource.resx</DependentUpon>
    </Compile>
  </ItemGroup>

  <ItemGroup>
    <UpToDateCheckInput Remove="Pages\Pages\AccessCode.razor" />
    <UpToDateCheckInput Remove="Pages\Pages\ChangePassword.razor" />
    <UpToDateCheckInput Remove="Pages\Pages\DaniDojo.razor" />
    <UpToDateCheckInput Remove="Pages\Pages\Dashboard.razor" />
    <UpToDateCheckInput Remove="Pages\Pages\Dialogs\AccessCodeDeleteConfirmDialog.razor" />
    <UpToDateCheckInput Remove="Pages\Pages\Dialogs\ChooseTitleDialog.razor" />
  </ItemGroup>

</Project>
```

Changes:
- Removed `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`, `<LangVersion>` from the property group (now inherited).
- Kept Blazor-specific properties (`WasmEnableSIMD`, `WasmEnableExceptionHandling`, `BlazorWebAssemblyLoadAllGlobalizationData`, `InvariantGlobalization`) which are NOT in `Directory.Build.props`.
- Removed all `Version="…"` attributes from `<PackageReference>`.
- Kept all `<Content Update="…">`, `<EmbeddedResource Update="…">`, `<_ContentIncludedByDefault Remove="…">`, `<UpToDateCheckInput Remove="…">` blocks unchanged.
- Kept `PrivateAssets="all"` on `Microsoft.AspNetCore.Components.WebAssembly.DevServer`.

- [ ] **Step 7.3: Verify TaikoWebUI builds**

Run:
```bash
dotnet build TaikoWebUI/TaikoWebUI.csproj
```
Expected: build succeeds.

- [ ] **Step 7.4: Commit**

```bash
git add TaikoWebUI/TaikoWebUI.csproj
git commit -m "Migrate TaikoWebUI to centralized props and CPM"
```

---

## Task 8: Strip versions and inherited props from `TaikoLocalServer.csproj`

**Files:**
- Modify: `TaikoLocalServer/TaikoLocalServer.csproj`

The server csproj has the most custom blocks: `<BuildTime>` metadata attribute, certificate copy rules, configuration copy rules (`Configurations/*.json`), datatable copy rules. These ALL stay.

- [ ] **Step 8.1: Read the current file in full first**

Run:
```bash
cat TaikoLocalServer/TaikoLocalServer.csproj
```

- [ ] **Step 8.2: Replace the file content**

Overwrite `D:\TaikoLocalServer\TaikoLocalServer\TaikoLocalServer.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <Version>1.1.0</Version>
    <EnableConfigurationBindingGenerator>false</EnableConfigurationBindingGenerator>
    <ApplicationManifest>app.manifest</ApplicationManifest>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)'!='Debug'">
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>

  <PropertyGroup>
    <BuildTime>$([System.DateTime]::UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))</BuildTime>
  </PropertyGroup>

  <ItemGroup>
    <AssemblyAttribute Include="System.Reflection.AssemblyMetadataAttribute">
      <_Parameter1>BuildTime</_Parameter1>
      <_Parameter2>$(BuildTime)</_Parameter2>
    </AssemblyAttribute>
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="Templates\TemplateController.cs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="BCrypt.Net-Next" />
    <PackageReference Include="DotNetZip" />
    <PackageReference Include="MediatR" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Server" />
    <PackageReference Include="Microsoft.AspNetCore.ResponseCompression" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Otp.NET" />
    <PackageReference Include="protobuf-net" />
    <PackageReference Include="protobuf-net.AspNetCore" />
    <PackageReference Include="Riok.Mapperly" />
    <PackageReference Include="Serilog.AspNetCore" />
    <PackageReference Include="Serilog.Expressions" />
    <PackageReference Include="Serilog.Sinks.File.Header" />
    <PackageReference Include="SharpZipLib" />
    <PackageReference Include="Swan.Core" />
    <PackageReference Include="Swan.Logging" />
    <PackageReference Include="Swashbuckle.AspNetCore" />
    <PackageReference Include="Throw" />
    <PackageReference Include="Yoh.Text.Json.NamingPolicies" />
  </ItemGroup>

  <ItemGroup>
    <!--Certificates-->
    <None Update="Certificates\cert.pfx">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="Certificates\root.pfx">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>


    <!--Configuration-->
    <Content Remove="Configurations\AuthSettings.json" />
    <None Include="Configurations\AuthSettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\ServerSettings.json" />
    <None Include="Configurations\ServerSettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\Database.json" />
    <None Include="Configurations\Database.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\DataSettings.json" />
    <None Include="Configurations\DataSettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\Kestrel.json" />
    <None Include="Configurations\Kestrel.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <Content Remove="Configurations\Logging.json" />
    <None Include="Configurations\Logging.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>


    <!--Server Datatables-->
    <Content Update="wwwroot\data\locked_songs_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\locked_costume_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\locked_title_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <Content Update="wwwroot\data\intro_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\movie_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\shop_folder_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\dan_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\event_folder_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <Content Update="wwwroot\data\token_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\gaiden_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\qrcode_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\special_songs_data.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>


    <!--Encrypted Game Datatables-->
    <Content Update="wwwroot\data\datatable\wordlist.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\musicinfo.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\music_order.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <Content Update="wwwroot\data\datatable\neiro.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\shougou.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\don_cos_reward.bin">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>


    <!--Decrypted Game Datatables-->
    <Content Update="wwwroot\data\datatable\wordlist.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\musicinfo.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\music_order.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>

    <Content Update="wwwroot\data\datatable\neiro.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\shougou.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="wwwroot\data\datatable\don_cos_reward.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\GameDatabase\GameDatabase.csproj" />
    <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
    <ProjectReference Include="..\TaikoWebUI\TaikoWebUI.csproj" />
  </ItemGroup>
</Project>
```

Changes:
- Removed `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`, `<LangVersion>` from the first PropertyGroup (now inherited).
- Kept all other custom blocks: `<EnableConfigurationBindingGenerator>false</EnableConfigurationBindingGenerator>`, `<ApplicationManifest>`, `<Version>1.1.0</Version>`, the Release-only `<PublishSingleFile>` block, the `<BuildTime>` block + AssemblyAttribute, the `Templates\TemplateController.cs` Compile Remove, all certificate / configuration / datatable Content/None blocks, all three ProjectReferences.
- Removed all `Version="…"` attributes from `<PackageReference>`.

- [ ] **Step 8.3: Verify TaikoLocalServer builds**

Run:
```bash
dotnet build TaikoLocalServer/TaikoLocalServer.csproj
```
Expected: build succeeds.

- [ ] **Step 8.4: Commit**

```bash
git add TaikoLocalServer/TaikoLocalServer.csproj
git commit -m "Migrate TaikoLocalServer to centralized props and CPM"
```

---

## Task 9: Convert `.sln` to `.slnx`

**Files:**
- Create: `TaikoLocalServer.slnx`
- Delete: `TaikoLocalServer.sln`

The `.slnx` format is the XML successor to `.sln` introduced in .NET 9 SDK. The .NET 8 SDK does NOT understand `.slnx` natively, but `dotnet build` against a directory falls back to detecting any `.sln`/`.slnx` automatically. Since this PR stays on .NET 8, we'll keep both `.sln` and `.slnx` until the .NET 8 SDK can be dropped — actually, **the .NET 8 SDK does NOT understand .slnx**. We need to keep .sln in PR1 and convert to .slnx in PR2 once the SDK is .NET 10.

**Action:** Skip the .slnx conversion in PR1. Re-route this task to PR2's plan. Mark this task as deliberately deferred:

- [ ] **Step 9.1: Confirm .slnx is deferred to PR2**

This is a documentation step only. No file changes here. The PR1 → PR2 hand-off note in the spec already covers the deferral.

Reason: `.slnx` requires the .NET 9+ SDK. PR1 is constrained to .NET 8. PR2 will perform the conversion as part of the runtime move.

(If a future maintainer asks why PR1 didn't convert, point them at this step.)

---

## Task 10: Validation

This task verifies the whole solution still builds and runs correctly on .NET 8 with the new infrastructure.

**Files:** none modified — observational only.

- [ ] **Step 10.1: Clean restore**

Run:
```bash
dotnet restore --force
```
Expected: succeeds with no `NU1605` (downgrade) or `NU1701` (incompat) warnings. Some `NU1507` warnings about CPM are expected and benign on first restore.

- [ ] **Step 10.2: Full build**

Run:
```bash
dotnet build
```
Expected: succeeds with 0 errors. Warning count should be no higher than the pre-PR1 baseline.

- [ ] **Step 10.3: Publish**

Run:
```bash
dotnet publish TaikoLocalServer/TaikoLocalServer.csproj -c Release -o ./publish-pr1-test
```
Expected: produces a self-contained single-file Windows exe at `./publish-pr1-test/TaikoLocalServer.exe`. Check it exists:
```bash
ls -la ./publish-pr1-test/TaikoLocalServer.exe
```

- [ ] **Step 10.4: Smoke-test the running server**

Back up the existing dev DB before first run (it lives at `TaikoLocalServer/wwwroot/taiko.db3` after a build):
```bash
cp TaikoLocalServer/wwwroot/taiko.db3 TaikoLocalServer/wwwroot/taiko.db3.pr1-backup 2>/dev/null || true
```

Run the server:
```bash
dotnet run --project TaikoLocalServer
```
Expected log lines (in order):
- `TaikoLocalServer version 1.1.0`
- `Server starting up...`
- `Now listening on: http://0.0.0.0:5000` (and the other 6 endpoints)

In a separate terminal, hit the admin UI:
```bash
curl -I http://localhost:5000/
```
Expected: `HTTP/1.1 200 OK` (Blazor index.html served).

Stop the server (Ctrl+C).

- [ ] **Step 10.5: Clean up the test publish directory**

```bash
rm -rf ./publish-pr1-test
```

- [ ] **Step 10.6: Confirm git status is clean**

```bash
git status
```
Expected: `nothing to commit, working tree clean`. All planned changes are already committed via Tasks 1–8.

---

## Task 11: Update README and open PR

**Files:**
- Modify: `README.md`

- [ ] **Step 11.1: Add a brief note to README.md**

Add the following section to `D:\TaikoLocalServer\README.md` immediately after the existing `## Configuration` section (or at the end of the file if simpler):

```markdown
## For developers

This solution uses [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) — package versions live in `Directory.Packages.props` at the repo root, not in individual `.csproj` files. To add or bump a package, edit `Directory.Packages.props` and add a versionless `<PackageReference Include="…" />` to the project that uses it.

Shared MSBuild defaults (TFM, Nullable, ImplicitUsings, LangVersion) live in `Directory.Build.props`.

The repo pins the .NET SDK band via `global.json`.
```

- [ ] **Step 11.2: Commit the README update**

```bash
git add README.md
git commit -m "Document Central Package Management for contributors"
```

- [ ] **Step 11.3: Push the branch**

```bash
git push -u origin dev/modernize-on-net8
```

- [ ] **Step 11.4: Open the PR**

Use `gh pr create` with this body. Adjust the title if the project's PR-title convention differs.

```bash
gh pr create --base dev --title "Modernize project structure (CPM, Directory.Build.props, deduped versions)" --body "$(cat <<'EOF'
## Summary

Lay clean infrastructure foundations for the .NET 10 upgrade *without* changing the runtime. The solution stays on .NET 8 and behavior is unchanged. Sequenced as PR1 of three (PR2 = .NET 10 runtime + Mediator swap, PR3 = MudBlazor 6 → 9). See \`docs/superpowers/specs/2026-05-03-net10-upgrade-design.md\`.

## What changed

- Added \`Directory.Build.props\` with shared TFM/Nullable/ImplicitUsings/LangVersion.
- Added \`Directory.Packages.props\` with Central Package Management; every package version is consolidated in one file.
- Added \`global.json\` pinning the .NET 8 SDK band.
- Removed redundant per-project property declarations and per-reference \`Version="…"\` attributes from all five \`.csproj\` files.
- Aligned a handful of mismatched versions (EF Core 8.0.3 vs 8.0.4 vs 8.0.0-rc.2.23480.1 → 8.0.4 stable; SharpZipLib 1.4.0 → 1.4.2; Throw 1.3.0 → 1.4.0).
- README note for contributors about CPM.

## What did NOT change

- No source code in \`Controllers/\`, \`Handlers/\`, \`Mappers/\`, \`Services/\`, \`GameDatabase/Entities/\`, or \`*.razor\`.
- No DB migrations.
- Runtime, dependency *behavior*, configuration formats — all unchanged.

## Test plan

- [x] \`dotnet restore\` clean
- [x] \`dotnet build\` clean
- [x] \`dotnet publish -c Release\` produces the self-contained exe
- [x] Server starts; existing \`taiko.db3\` migrates without errors
- [x] Admin UI loads at http://localhost:5000
- [x] One game endpoint hand-tested (please describe in review which one)

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

- [ ] **Step 11.5: Confirm CI passes on the PR**

Visit the PR URL printed by the previous command. Confirm the GitHub Actions workflow `TLS GitHub Actions` runs to green. The artifact `TLS 1.1.<run>` should be uploadable.

If CI fails, do NOT merge. Investigate the failure (most likely cause: a transitive package version that NuGet can't resolve under CPM — check the build log).

---

## Self-review checklist

Run through this before declaring PR1 complete:

- [ ] Every `<PackageReference>` in every `.csproj` is **versionless** (no `Version="…"` attribute).
- [ ] No `.csproj` has a redundant `<TargetFramework>`, `<Nullable>`, `<ImplicitUsings>`, or `<LangVersion>` line that duplicates `Directory.Build.props`.
- [ ] `Directory.Packages.props` lists exactly the packages that any `.csproj` references — no orphaned versions, no missing entries (NuGet will fail restore if a referenced package has no `<PackageVersion>`).
- [ ] The five custom blocks in `TaikoLocalServer.csproj` are intact: `<BuildTime>` AssemblyAttribute, `<Templates\TemplateController.cs>` Compile Remove, `<PublishSingleFile>` Release block, certificate copy rules, configuration copy rules, datatable copy rules.
- [ ] `TaikoWebUI.csproj` retains `<WasmEnableSIMD>false</WasmEnableSIMD>`, `<WasmEnableExceptionHandling>false</WasmEnableExceptionHandling>`, `<BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>`, `<InvariantGlobalization>false</InvariantGlobalization>` and all resx/Content/UpToDateCheckInput blocks.
- [ ] `LocalSaveModScoreMigrator.csproj` retains `<OutputType>Exe</OutputType>` and `<Version>1.0.0-beta1</Version>`.
- [ ] `global.json` pins an `8.0.x` SDK with `"rollForward": "latestFeature"`.
- [ ] `dotnet --version` prints an `8.0.x` version (i.e. `global.json` is in effect).
- [ ] CI is green on the PR.

## What this PR sets up for PR2

After merge:
- PR2 changes `Directory.Build.props` `TargetFramework=net10.0` and `LangVersion=13`.
- PR2 changes `global.json` SDK pin to `10.0.100`.
- PR2 bumps every entry in `Directory.Packages.props` that has a 10.x release available (bulk edit in one file).
- PR2 swaps the `MediatR` PackageVersion for `Mediator.SourceGenerator` and `Mediator.Abstractions`.
- PR2 converts `.sln` → `.slnx` (only possible once the SDK is 10).

The CPM file shape PR2 inherits is the right shape for a clean version-bump diff.
