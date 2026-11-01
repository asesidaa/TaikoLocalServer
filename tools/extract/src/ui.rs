//! Game UI art rendered from the game's Lumen files instead of committed images:
//! title plates and dan badges (`common/name_plate`) and the scoreboard panel (`common/player_board`).
//! Sprites are located by frame labels or instance names, never by id, since ids change between builds.

use std::fs;
use std::path::{Path, PathBuf};

use anyhow::{Context, Result};
use image::RgbaImage;
use serde_json::{json, Map, Value};

use crate::lumen::Lumen;

pub const NAMEPLATE_INDEX: &str = "plates.json";
pub const SCOREBOARD_INDEX: &str = "scoreboard.json";

/// Title plates (index = `TitlePlateId`) and dan badges.
pub fn nameplates(x64: &Path, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    convert(x64, "name_plate", dst_dir, NAMEPLATE_INDEX, report, |lm| {
        Ok(vec![
            ("plates", "plate", lm.sprite_with_label("wood").context("no sprite labelled wood")?),
            ("dan", "dan", lm.sprite_with_label("5kyuu_red").context("no sprite labelled 5kyuu_red")?),
        ])
    })
}

/// The per-difficulty achievement panel (crowns and 粋/雅/極 rank slots) and its digit glyphs.
pub fn scoreboard(x64: &Path, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    convert(x64, "player_board", dst_dir, SCOREBOARD_INDEX, report, |lm| {
        // The board places the panel as bg_instance next to its rank counters; each counter places digits as n1_ins….
        let board = lm.sprite_placing("num_kiwami_instance").context("no scoreboard sprite (num_kiwami_instance)")?;
        let counter = lm.child(board, "num_kiwami_instance").context("no rank counter")?;
        Ok(vec![
            ("panels", "panel", lm.child(board, "bg_instance").context("no scoreboard panel (bg_instance)")?),
            ("digits", "digit", lm.child(counter, "n1_ins").context("no digit glyphs (n1_ins)")?),
        ])
    })
}

/// Renders each `(index key, file prefix, sprite)` set from `lumen/000_default/common/<file>` into
/// `<dst_dir>/<prefix>_<label>.png`, one shared canvas per set, and writes an index of the labels.
fn convert(
    x64: &Path,
    file: &str,
    dst_dir: &Path,
    index_name: &str,
    report: &mut crate::Report,
    sets: impl FnOnce(&Lumen) -> Result<Vec<(&'static str, &'static str, u32)>>,
) -> Result<()> {
    let src = x64.join("lumen").join("000_default").join("common");
    let sources: [PathBuf; 2] = [src.join(format!("{file}.nulm")), src.join(format!("{file}.nutexb"))];
    if let Some(missing) = sources.iter().find(|p| !p.is_file()) {
        report.missing.push(missing.display().to_string());
        return Ok(());
    }
    let index = dst_dir.join(index_name);
    if sources.iter().try_fold(true, |ok, s| crate::is_newer_than(&index, s).map(|n| ok && n))? {
        report.up_to_date.push(file.into());
        return Ok(());
    }

    let lm = Lumen::parse(&fs::read(&sources[0])?).with_context(|| format!("parsing {}", sources[0].display()))?;
    let atlas = crate::textures::decode(&sources[1])?;
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;

    let mut labels = Map::new();
    for (key, prefix, sprite) in sets(&lm)? {
        labels.insert(key.into(), Value::from(render_set(&lm, &atlas, sprite, prefix, dst_dir, report)?));
    }
    fs::write(&index, serde_json::to_string_pretty(&json!(labels))?).with_context(|| format!("writing {}", index.display()))?;
    Ok(())
}

/// Renders every label of `sprite` on one canvas (the union of their mask-clipped bounds) so they line up.
fn render_set(lm: &Lumen, atlas: &RgbaImage, sprite: u32, prefix: &str, dst_dir: &Path, report: &mut crate::Report) -> Result<Vec<String>> {
    let names = lm.labels(sprite);
    let mut union: Option<[f32; 4]> = None;
    for name in &names {
        if let Some(b) = lm.bounds(sprite, name)? {
            union = Some(union.map_or(b, |u| [u[0].min(b[0]), u[1].min(b[1]), u[2].max(b[2]), u[3].max(b[3])]));
        }
    }
    for name in &names {
        let out = dst_dir.join(format!("{prefix}_{name}.png"));
        match lm.render(sprite, name, atlas, union).and_then(|img| img.save(&out).with_context(|| format!("writing {}", out.display()))) {
            Ok(()) => report.copied.push(format!("{prefix}_{name}")),
            Err(e) => report.failed.push(format!("{prefix}_{name}: {e:#}")),
        }
    }
    Ok(names)
}
