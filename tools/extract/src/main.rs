mod ac15;
mod ani;
mod anims;
mod font;
mod game;
mod lmpack;
mod lumen;
mod models;
mod movies;
mod nub;
mod nud;
mod nut;
mod source;
mod textures;
mod tones;
mod ui;
mod v12;

use std::fs;
use std::path::{Path, PathBuf};
use std::process::ExitCode;

use anyhow::{bail, Context, Result};
use clap::Parser;

/// Datatables the server decrypts on startup (see `FileGameDataCatalog`). music_attribute is optional there (the
/// Song List's "new" flag).
const DATATABLES: [&str; 7] = [
    "musicinfo.bin",
    "music_order.bin",
    "music_attribute.bin",
    "wordlist.bin",
    "don_cos_reward.bin",
    "shougou.bin",
    "neiro.bin",
];

/// Flat texture folders under `Data/x64/model`: face paint (the `face` costume slot),
/// puchi accessories, and the per-kigurumi expression atlases.
const TEXTURE_DIRS: [&str; 3] = ["paint", "acce", "face"];

/// Model folders under `Data/x64/model`: bodies, heads, and kigurumi (full costumes).
const MODEL_DIRS: [&str; 3] = ["body", "head", "cos"];

/// Extract the game files TaikoLocalServer needs from a Taiko no Tatsujin Nijiiro install.
#[derive(Parser)]
#[command(version)]
struct Args {
    /// Game install folder: the one containing `Data/x64` (passing `Data/x64` itself also works).
    #[arg(long, required_unless_present = "inputs")]
    game: Option<PathBuf>,

    /// The server's `wwwroot` folder. Defaults to `wwwroot` next to this exe,
    /// which is right when it sits beside `TaikoLocalServer.exe`.
    #[arg(long)]
    out: Option<PathBuf>,

    /// Layer the install's LayeredFS `Data_mods` folder over `Data`, as the game sees it with the plugin:
    /// modded fonts, models and UI replace the originals, and modded datatables (new songs) are used.
    #[arg(long)]
    mods: bool,

    /// Print the game files this tool reads, as JSON, and exit. The WebUI uploads only these from a folder the
    /// admin picks in the browser, into a copy of the install's layout that extract then runs on.
    #[arg(long)]
    inputs: bool,

    /// An older game (Green … Momoiro) instead of Nijiiro. `--game` is then its `USRDIR/data` folder (or a folder
    /// above it), or a `.zip` of the RPCS3 install, read without unpacking it.
    #[arg(long, value_enum)]
    era: Option<ac15::Era>,
}

/// Records which layers the outputs came from, so switching `--mods` on or off rebuilds them.
const STAMP: &str = "extract.json";

/// The folder under `<out>` for everything but the datatables; the server serves it through `api/Assets`.
const GAME_DATA: &str = "game_data";

#[derive(Default, Debug, PartialEq)]
pub struct Report {
    label: String,
    copied: Vec<String>,
    up_to_date: Vec<String>,
    missing: Vec<String>,
    failed: Vec<String>,
}

impl Report {
    fn new(label: &str) -> Self {
        Self { label: label.to_string(), ..Default::default() }
    }
}

/// `--inputs`: paths relative to the game folder. `files` are exact files or, ending in `/`, whole folders;
/// in `names` folders only the file names are read (empty files will do). `mods` is the same for `Data_mods`.
fn inputs() -> serde_json::Value {
    let mut rel: Vec<String> = DATATABLES.iter().map(|n| format!("datatable/{n}")).collect();
    rel.extend(TEXTURE_DIRS.iter().chain(&MODEL_DIRS).map(|d| format!("model/{d}/")));
    rel.push("animation/".into());
    rel.extend(font::SHEETS.iter().flat_map(|s| [format!("font/{s}.xml"), format!("font/{s}.nutexb")]));
    rel.extend(ui::lumen_files().flat_map(|f| ["nulm", "nutexb"].map(|ext| format!("lumen/000_default/{f}.{ext}"))));
    rel.extend(tones::inputs());
    // Data_mods keeps its datatables encrypted in x64_enc (see game.rs).
    let mod_files = rel.iter().filter(|r| !r.starts_with("datatable/")).map(|r| format!("Data_mods/x64/{r}"))
        .chain(DATATABLES.iter().map(|n| format!("Data_mods/x64_enc/datatable/{n}")));
    serde_json::json!({
        "files": rel.iter().map(|r| format!("Data/x64/{r}")).collect::<Vec<_>>(),
        "names": ["Data/x64/movie/"],
        "mods": { "files": mod_files.collect::<Vec<_>>(), "names": ["Data_mods/x64/movie/"] },
    })
}

fn main() -> ExitCode {
    let args = Args::parse();
    if args.inputs {
        println!("{}", args.era.map_or_else(inputs, ac15::inputs));
        return ExitCode::SUCCESS;
    }
    match run(args) {
        Ok(reports) => {
            reports.iter().for_each(print_report);
            if reports.iter().any(|r| !r.missing.is_empty() || !r.failed.is_empty()) {
                ExitCode::from(2)
            } else {
                ExitCode::SUCCESS
            }
        }
        Err(e) => {
            eprintln!("error: {e:#}");
            ExitCode::FAILURE
        }
    }
}

fn run(args: Args) -> Result<Vec<Report>> {
    let game = args.game.context("--game is required")?;
    let out = match args.out {
        Some(p) => p,
        None => std::env::current_exe()?
            .parent()
            .context("exe has no parent folder")?
            .join("wwwroot"),
    };
    if let Some(era) = args.era {
        return ac15::run(era, &game, &out);
    }
    let source = game.clone();
    let x64 = resolve_x64(&game)?;
    let game = game::Game::new(x64, args.mods)?;
    println!("game:   {}{}", game.base().display(), if game.modded() { " + Data_mods" } else { "" });
    println!("output: {}", out.display());
    let game_data = out.join(GAME_DATA);
    // The folder used to be called don-model: keep what an older extract wrote.
    let old = out.join("don-model");
    if old.is_dir() && !game_data.exists() {
        fs::rename(&old, &game_data).with_context(|| format!("renaming {} to {GAME_DATA}", old.display()))?;
    }
    reset_if_layers_changed(&game, &out, &game_data)?;

    let mut reports = vec![copy_datatables(&game, &out.join("data").join("datatable"))?];
    for dir in TEXTURE_DIRS {
        let mut report = Report::new(&format!("{dir} textures"));
        textures::convert_dir(&game, &format!("model/{dir}"), &game_data.join(dir), &mut report)?;
        reports.push(report);
    }
    for dir in MODEL_DIRS {
        let mut report = Report::new(&format!("{dir} models"));
        models::convert_category(&game, &format!("model/{dir}"), &game_data.join(dir), &mut report)?;
        reports.push(report);
    }
    let mut report = Report::new("animations");
    anims::convert(&game, &game_data.join("animation"), &mut report)?;
    reports.push(report);
    let mut report = Report::new("nameplates");
    ui::nameplates(&game, &game_data.join("nameplate"), &mut report)?;
    reports.push(report);
    let mut report = Report::new("scoreboard");
    ui::scoreboard(&game, &game_data.join("scoreboard"), &mut report)?;
    reports.push(report);
    let mut report = Report::new("icons");
    ui::icons(&game, &game_data.join("ui"), &mut report)?;
    ui::note_icon(&game, &game_data.join("ui"), &mut report)?;
    reports.push(report);
    let mut report = Report::new("font pages");
    font::convert(&game, &game_data.join("font"), &mut report)?;
    reports.push(report);
    let mut report = Report::new("movies");
    movies::convert(&game, &game_data, &mut report)?;
    reports.push(report);
    let mut report = Report::new("tones");
    tones::convert(&game, &game_data.join("tone"), &mut report)?;
    reports.push(report);
    fs::write(game_data.join(STAMP), serde_json::to_string(&serde_json::json!({ "mods": game.modded() }))?)?;
    write_manifest(&out.join("data").join(MANIFEST), "nijiiro", &source, &reports, Vec::new())?;
    Ok(reports)
}

/// What a run did, for the server's Extract panel: `<out>/data/extract.json` (Nijiiro) or
/// `<out>/data/<era>/extract.json`. Overwritten by every run that gets to the end, failed steps or not.
pub const MANIFEST: &str = "extract.json";

/// `steps` are the reports in printed order, labels without the era (`server files`, not `red server files`).
/// `notes` are `{ kind: gap | absent | included, code, detail? }` (see the README).
pub fn write_manifest(path: &Path, era: &str, source: &Path, reports: &[Report], notes: Vec<serde_json::Value>) -> Result<()> {
    let prefix = format!("{era} ");
    let steps: Vec<_> = reports.iter().map(|r| serde_json::json!({
        "label": r.label.strip_prefix(&prefix).unwrap_or(&r.label),
        "written": r.copied.len(), "upToDate": r.up_to_date.len(), "missing": r.missing.len(), "failed": r.failed.len(),
    })).collect();
    let manifest = serde_json::json!({
        "era": era, "finishedAt": utc_now(), "source": source.display().to_string(), "steps": steps, "notes": notes,
    });
    fs::create_dir_all(path.parent().context("manifest has no folder")?)?;
    fs::write(path, serde_json::to_string_pretty(&manifest)?).with_context(|| format!("writing {}", path.display()))
}

fn utc_now() -> String {
    iso_utc(std::time::SystemTime::now().duration_since(std::time::UNIX_EPOCH).map_or(0, |d| d.as_secs()))
}

/// `2026-11-02T21:00:00Z` from Unix seconds (days to civil date: Howard Hinnant's algorithm).
fn iso_utc(secs: u64) -> String {
    let (days, s) = ((secs / 86_400) as i64, secs % 86_400);
    let z = days + 719_468;
    let era = z.div_euclid(146_097);
    let doe = z - era * 146_097;
    let yoe = (doe - doe / 1_460 + doe / 36_524 - doe / 146_096) / 365;
    let doy = doe - (365 * yoe + yoe / 4 - yoe / 100);
    let mp = (5 * doy + 2) / 153;
    let (d, m) = (doy - (153 * mp + 2) / 5 + 1, if mp < 10 { mp + 3 } else { mp - 9 });
    let y = yoe + era * 400 + i64::from(m <= 2);
    format!("{y:04}-{m:02}-{d:02}T{:02}:{:02}:{:02}Z", s / 3600, s / 60 % 60, s % 60)
}

/// Outputs are skipped when newer than their source, which can't tell a mod file from the original it
/// replaced. So when `--mods` differs from the last run, everything extract wrote is rebuilt once.
fn reset_if_layers_changed(game: &game::Game, out: &Path, game_data: &Path) -> Result<()> {
    let last = fs::read_to_string(game_data.join(STAMP)).ok()
        .and_then(|s| serde_json::from_str::<serde_json::Value>(&s).ok())
        .and_then(|v| v["mods"].as_bool());
    // No stamp: an older extract (base files only), or a first run.
    if last.unwrap_or(false) == game.modded() {
        return Ok(());
    }
    println!("{} Data_mods since the last run: rebuilding everything.", if game.modded() { "Using" } else { "No longer using" });
    if game_data.is_dir() {
        fs::remove_dir_all(game_data).with_context(|| format!("clearing {}", game_data.display()))?;
    }
    for name in DATATABLES {
        let _ = fs::remove_file(out.join("data").join("datatable").join(name));
    }
    Ok(())
}

/// Accepts the game root or `Data/x64` directly. `Data_mods` is layered on top only with `--mods`.
fn resolve_x64(game: &Path) -> Result<PathBuf> {
    for candidate in [game.join("Data").join("x64"), game.to_path_buf()] {
        if candidate.join("datatable").is_dir() {
            return Ok(candidate);
        }
    }
    bail!(
        "{} doesn't look like a Nijiiro install: no Data/x64/datatable folder found",
        game.display()
    )
}

fn copy_datatables(game: &game::Game, dst_dir: &Path) -> Result<Report> {
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;
    let mut report = Report::new("datatables");
    for name in DATATABLES {
        let src = game.datatable(name);
        let dst = dst_dir.join(name);
        if !src.is_file() {
            report.missing.push(name.to_string());
        } else if is_newer_than(&dst, &src)? && fs::metadata(&dst)?.len() == fs::metadata(&src)?.len() {
            report.up_to_date.push(name.to_string());
        } else {
            fs::copy(&src, &dst).with_context(|| format!("copying {}", src.display()))?;
            report.copied.push(name.to_string());
        }
    }
    Ok(report)
}

/// True when `dst` exists and is no older than `src`.
pub fn is_newer_than(dst: &Path, src: &Path) -> Result<bool> {
    let Ok(d) = fs::metadata(dst) else { return Ok(false) };
    Ok(d.modified()? >= fs::metadata(src)?.modified()?)
}

fn print_report(r: &Report) {
    println!();
    println!(
        "{}: {} written, {} already up to date, {} missing, {} failed",
        r.label,
        r.copied.len(),
        r.up_to_date.len(),
        r.missing.len(),
        r.failed.len()
    );
    for name in &r.missing {
        println!("  MISSING  {name}");
    }
    for msg in &r.failed {
        println!("  FAILED   {msg}");
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn temp_dir(tag: &str) -> PathBuf {
        let dir = std::env::temp_dir().join(format!("taiko-extract-{tag}-{}", std::process::id()));
        let _ = fs::remove_dir_all(&dir);
        fs::create_dir_all(&dir).unwrap();
        dir
    }

    #[test]
    fn copies_then_skips_and_reports_missing() {
        let root = temp_dir("copy");
        let src = root.join("Data").join("x64").join("datatable");
        fs::create_dir_all(&src).unwrap();
        // Every table but the last one, neiro.
        let present = &DATATABLES[..DATATABLES.len() - 1];
        for name in present {
            fs::write(src.join(name), name.as_bytes()).unwrap();
        }
        let game = game::Game::new(resolve_x64(&root).unwrap(), false).unwrap();
        let dst = root.join("out");

        let first = copy_datatables(&game, &dst).unwrap();
        assert_eq!(first.copied.len(), present.len());
        assert_eq!(first.missing, vec!["neiro.bin".to_string()]);

        let second = copy_datatables(&game, &dst).unwrap();
        assert!(second.copied.is_empty());
        assert_eq!(second.up_to_date.len(), present.len());

        fs::remove_dir_all(&root).unwrap();
    }

    #[test]
    fn manifest_lists_steps_without_the_era() {
        assert_eq!(iso_utc(0), "1970-01-01T00:00:00Z");
        assert_eq!(iso_utc(1_793_653_200), "2026-11-02T21:00:00Z");
        assert_eq!(iso_utc(951_782_400), "2000-02-29T00:00:00Z");
        let root = temp_dir("manifest");
        let mut r = Report::new("red server files");
        r.copied.push("a".into());
        r.missing.push("b".into());
        let path = root.join("data").join("red").join(MANIFEST);
        write_manifest(&path, "red", Path::new("D:/Red"), &[r], vec![serde_json::json!({ "kind": "gap", "code": "early-config", "detail": "ST8100-7" })]).unwrap();
        let m: serde_json::Value = serde_json::from_str(&fs::read_to_string(&path).unwrap()).unwrap();
        assert_eq!(m["era"], "red");
        assert_eq!(m["source"], "D:/Red");
        assert_eq!(m["steps"], serde_json::json!([{ "label": "server files", "written": 1, "upToDate": 0, "missing": 1, "failed": 0 }]));
        assert_eq!(m["notes"][0]["code"], "early-config");
        assert!(m["finishedAt"].as_str().unwrap().ends_with('Z'));
        fs::remove_dir_all(&root).unwrap();
    }

    #[test]
    fn rejects_folder_without_datatables() {
        let root = temp_dir("bad");
        assert!(resolve_x64(&root).is_err());
        fs::remove_dir_all(&root).unwrap();
    }
}
