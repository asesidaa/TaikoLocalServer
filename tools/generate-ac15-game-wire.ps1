param(
    [string[]] $Surface = @("reviewed"),
    [string] $ProtogenPath
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($ProtogenPath)) {
    $ProtogenPath = Join-Path $repoRoot ".tools/protogen.exe"
}

if (-not (Test-Path -LiteralPath $ProtogenPath -PathType Leaf)) {
    throw "protogen executable not found: $ProtogenPath"
}

$generationTargets = @(
    [pscustomobject]@{
        Surface = "white-final"
        ProtoDir = "proto/white-final"
        ProtoFile = "taiko.proto"
        Namespace = "TaikoLocalServer.Adapters.GameProtocol.White.Wire"
        Output = "Adapters.GameProtocol.White/Wire/Game.cs"
    },
    [pscustomobject]@{
        Surface = "white-legacy"
        ProtoDir = "proto/white"
        ProtoFile = "taiko.proto"
        Namespace = "TaikoLocalServer.Adapters.GameProtocol.White.LegacyWire"
        Output = "Adapters.GameProtocol.White/LegacyWire/Game.cs"
    },
    [pscustomobject]@{
        Surface = "murasaki-final"
        ProtoDir = "proto/murasaki-final"
        ProtoFile = "taiko.proto"
        Namespace = "TaikoLocalServer.Adapters.GameProtocol.Murasaki.Wire"
        Output = "Adapters.GameProtocol.Murasaki/Wire/Game.cs"
    },
    [pscustomobject]@{
        Surface = "murasaki-legacy"
        ProtoDir = "proto/murasaki"
        ProtoFile = "taiko.proto"
        Namespace = "TaikoLocalServer.Adapters.GameProtocol.Murasaki.LegacyWire"
        Output = "Adapters.GameProtocol.Murasaki/LegacyWire/Game.cs"
    },
    [pscustomobject]@{
        Surface = "kimidori"
        ProtoDir = "proto/kimidori"
        ProtoFile = "taiko.proto"
        Namespace = "TaikoLocalServer.Adapters.GameProtocol.Kimidori.Wire"
        Output = "Adapters.GameProtocol.Kimidori/Wire/Game.cs"
    },
    [pscustomobject]@{
        Surface = "kimidori-final"
        ProtoDir = "proto/kimidori-final"
        ProtoFile = "taiko.proto"
        Namespace = "TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire"
        Output = "Adapters.GameProtocol.Kimidori/FinalWire/Game.cs"
    },
    [pscustomobject]@{
        Surface = "momoiro"
        ProtoDir = "proto/momoiro"
        ProtoFile = "taiko.proto"
        Namespace = "TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire"
        Output = "Adapters.GameProtocol.Momoiro/Wire/Game.cs"
    }
)

$reviewedSurfaces = @(
    "white-final",
    "white-legacy",
    "murasaki-final",
    "murasaki-legacy",
    "kimidori"
)

$requested = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($name in $Surface) {
    if ($name -eq "all") {
        foreach ($target in $generationTargets) {
            [void]$requested.Add($target.Surface)
        }

        continue
    }

    if ($name -eq "reviewed") {
        foreach ($targetName in $reviewedSurfaces) {
            [void]$requested.Add($targetName)
        }

        continue
    }

    [void]$requested.Add($name)
}

$unknown = $requested | Where-Object {
    $candidate = $_
    -not ($generationTargets | Where-Object { $_.Surface -ieq $candidate })
}

if ($unknown) {
    throw "unknown AC15 game wire surface(s): $($unknown -join ', ')"
}

foreach ($target in $generationTargets | Where-Object { $requested.Contains($_.Surface) }) {
    $protoDir = Join-Path $repoRoot $target.ProtoDir
    $outputPath = Join-Path $repoRoot $target.Output
    $outputDir = Split-Path -Parent $outputPath
    $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("taiko-ac15-wire-" + [guid]::NewGuid().ToString("N"))

    New-Item -ItemType Directory -Path $tempDir | Out-Null
    try {
        & $ProtogenPath `
            --csharp_out=$tempDir `
            "-I$protoDir" `
            +nullablevaluetype=yes `
            "--package=$($target.Namespace)" `
            $($target.ProtoFile)

        if ($LASTEXITCODE -ne 0) {
            throw "protogen failed for $($target.Surface) with exit code $LASTEXITCODE"
        }

        $generatedFile = Join-Path $tempDir ([System.IO.Path]::GetFileNameWithoutExtension($target.ProtoFile) + ".cs")
        if (-not (Test-Path -LiteralPath $generatedFile -PathType Leaf)) {
            throw "expected generated file was not created: $generatedFile"
        }

        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
        Move-Item -LiteralPath $generatedFile -Destination $outputPath -Force
        Write-Host "generated $($target.Surface) -> $($target.Output)"
    }
    finally {
        if (Test-Path -LiteralPath $tempDir) {
            Remove-Item -LiteralPath $tempDir -Recurse -Force
        }
    }
}
