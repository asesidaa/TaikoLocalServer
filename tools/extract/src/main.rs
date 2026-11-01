mod anims;
mod lumen;
mod models;
mod nameplates;
mod textures;
mod v12;

use std::fs;
use std::path::{Path, PathBuf};
use std::process::ExitCode;

use anyhow::{bail, Context, Result};
use clap::Parser;

/// Datatables the server decrypts on startup (see `FileGameDataCatalog`).
const DATATABLES: [&str; 6] = [
    "musicinfo.bin",
    "music_order.bin",
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
    #[arg(long)]
    game: PathBuf,

    /// The server's `wwwroot` folder. Defaults to `wwwroot` next to this exe,
    /// which is right when it sits beside `TaikoLocalServer.exe`.
    #[arg(long)]
    out: Option<PathBuf>,
}

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

fn main() -> ExitCode {
    match run(Args::parse()) {
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
    let x64 = resolve_x64(&args.game)?;
    let out = match args.out {
        Some(p) => p,
        None => std::env::current_exe()?
            .parent()
            .context("exe has no parent folder")?
            .join("wwwroot"),
    };
    println!("game:   {}", x64.display());
    println!("output: {}", out.display());

    let mut reports = vec![copy_datatables(&x64.join("datatable"), &out.join("data").join("datatable"))?];
    for dir in TEXTURE_DIRS {
        let mut report = Report::new(&format!("{dir} textures"));
        textures::convert_dir(&x64.join("model").join(dir), &out.join("don-model").join(dir), &mut report)?;
        reports.push(report);
    }
    for dir in MODEL_DIRS {
        let mut report = Report::new(&format!("{dir} models"));
        models::convert_category(&x64.join("model").join(dir), &out.join("don-model").join(dir), &mut report)?;
        reports.push(report);
    }
    let mut report = Report::new("animations");
    anims::convert(&x64, &out.join("don-model").join("animation"), &mut report)?;
    reports.push(report);
    let mut report = Report::new("nameplates");
    nameplates::convert(&x64, &out.join("don-model").join("nameplate"), &mut report)?;
    reports.push(report);
    Ok(reports)
}

/// Accepts the game root or `Data/x64` directly. `Data_mods` is never used.
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

fn copy_datatables(src_dir: &Path, dst_dir: &Path) -> Result<Report> {
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;
    let mut report = Report::new("datatables");
    for name in DATATABLES {
        let src = src_dir.join(name);
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
        for name in &DATATABLES[..5] {
            fs::write(src.join(name), name.as_bytes()).unwrap();
        }
        let x64 = resolve_x64(&root).unwrap();
        let dst = root.join("out");

        let first = copy_datatables(&x64.join("datatable"), &dst).unwrap();
        assert_eq!(first.copied.len(), 5);
        assert_eq!(first.missing, vec!["neiro.bin".to_string()]);

        let second = copy_datatables(&x64.join("datatable"), &dst).unwrap();
        assert!(second.copied.is_empty());
        assert_eq!(second.up_to_date.len(), 5);

        fs::remove_dir_all(&root).unwrap();
    }

    #[test]
    fn rejects_folder_without_datatables() {
        let root = temp_dir("bad");
        assert!(resolve_x64(&root).is_err());
        fs::remove_dir_all(&root).unwrap();
    }
}
