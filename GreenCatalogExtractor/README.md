# GreenCatalogExtractor

GreenCatalogExtractor is a standalone utility for deriving Green AC15 customization catalog JSON from a Green `USRDIR/data` tree.

## Role

The extractor reads game-owned Green AC15 data and writes server-owned JSON under `Host/wwwroot/data/green`. The server can also run catalog extraction on first startup when `ServerSettings:Eras:Green:AutoExtractCatalog` is enabled and generated Green customization JSON is missing.

## Usage

From the repo root:

```powershell
dotnet run --project GreenCatalogExtractor -- extract --game-data Host/wwwroot/data/green/data --out Host/wwwroot/data/green
```

## When To Add Code Here

- Add support for another Green catalog file.
- Adjust derived Green customization output.
- Add command-line behavior for Green catalog extraction.

Keep runtime catalog loading in `Infrastructure/GameDataCatalog/Green`.
