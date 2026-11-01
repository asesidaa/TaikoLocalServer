//! Game UI art rendered from the game's Lumen files instead of committed images:
//! title plates and dan badges (`common/name_plate`), the scoreboard panel (`common/player_board`) and the
//! icons the WebUI shows (song options, difficulties, crowns, Dani Dojo pass stamps).
//! Sprites are located by frame labels or instance names, never by id, since ids change between builds.

use std::fs;
use std::path::{Path, PathBuf};

use anyhow::{Context, Result};
use image::RgbaImage;
use serde_json::{json, Map, Value};

use crate::game::Game;
use crate::lumen::Lumen;

pub const NAMEPLATE_INDEX: &str = "plates.json";
/// The animated nameplate for the WebUI's Lumen player, with its atlas beside it as `atlas.png`.
pub const NAMEPLATE_SCENE: &str = "nameplate.scene.json";
/// The instance the game's whole nameplate places the title plate as.
const PLATE_INSTANCE: &str = "title_instance";
pub const SCOREBOARD_INDEX: &str = "scoreboard.json";

/// Icon sets: (Lumen file under `lumen/000_default`, index name, [(index key, file prefix, a label the sprite has)]).
/// Each set is the sprite carrying that label, rendered one PNG per label. A comma-separated label names the sprite
/// whose labels are exactly those, where one label alone would match a bigger animation.
const ICONS: [(&str, &str, &[(&str, &str, &str)]); 5] = [
    // none, doron, abekobe (mirror), kimagure / detarame (random: whimsical / messy), note_pos, speedx1.1 … speedx4
    ("common/option", "options.json", &[("options", "option", "doron")]),
    // easy, normal, hard, oni, ura
    ("common_songselect/common_song_select_song_board_icon", "difficulty.json", &[("difficulty", "difficulty", "ura")]),
    // badge_crown_1…3 (clear, gold, donderful) and badge_rank_1…7 (the seven score ranks)
    ("common_songselect/common_song_select_main_diff_board", "badges.json", &[("crowns", "crown", "badge_crown_1"), ("ranks", "rank", "badge_rank_1")]),
    // stamp_red_01…03, stamp_gold_01…03: a Dani Dojo pass (normal, full combo, perfect) in red or gold
    ("enso_dani/dani_select/dani_select", "dani.json", &[("stamps", "dani", "stamp_gold_01")]),
    // AI Battle: win / lose (the result emblems), and each section's crown: none, crown_silver, crown_gold, crown_rainbow
    ("enso_ai/result/result_ai_section", "ai.json", &[("results", "ai", "win,lose"), ("crowns", "ai", "crown_rainbow")]),
];

const NAMEPLATE_LUMEN: &str = "common/name_plate";
const SCOREBOARD_LUMEN: &str = "common/player_board";
/// The notes on the lane; one sprite has a frame per note type, each with its caption (ドン) under the note.
const NOTE_LUMEN: &str = "enso_normal/enso/onpu/onp_all";
/// The WebUI's logo and favicon (served by api/Brand/Icon).
pub const NOTE_ICON: &str = "don_note.png";

/// The Lumen files under `lumen/000_default` this module reads (`--inputs`).
pub fn lumen_files() -> impl Iterator<Item = &'static str> {
    ICONS.iter().map(|(file, ..)| *file).chain([NAMEPLATE_LUMEN, SCOREBOARD_LUMEN, NOTE_LUMEN])
}

/// The WebUI's icons (see [`ICONS`]), into `<dst_dir>/<prefix>_<label>.png` with one index per source file.
pub fn icons(game: &Game, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    for (file, index, sets) in ICONS {
        convert(game, file, dst_dir, index, None, report, |lm| {
            sets.iter()
                .map(|&(key, prefix, label)| {
                    let sprite = if label.contains(',') { lm.sprite_with_labels(label) } else { lm.sprite_with_label(label) };
                    Ok((key, prefix, sprite.with_context(|| format!("no sprite labelled {label} in {file}"))?))
                })
                .collect()
        })?;
    }
    Ok(())
}

/// The don note, without its caption, on a square transparent canvas: `<dst_dir>/don_note.png`.
pub fn note_icon(game: &Game, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    let sources: [PathBuf; 2] = ["nulm", "nutexb"].map(|ext| game.file(format!("lumen/000_default/{NOTE_LUMEN}.{ext}")));
    if let Some(missing) = sources.iter().find(|s| !s.is_file()) {
        report.missing.push(missing.display().to_string());
        return Ok(());
    }
    let out = dst_dir.join(NOTE_ICON);
    if sources.iter().try_fold(true, |ok, s| crate::is_newer_than(&out, s).map(|n| ok && n))? {
        report.up_to_date.push(NOTE_ICON.into());
        return Ok(());
    }
    let lm = Lumen::parse(&fs::read(&sources[0])?).with_context(|| format!("parsing {}", sources[0].display()))?;
    let atlas = crate::textures::decode(&sources[1])?;
    // The lane's note sprite is the one with the two-player notes (wdon1p…); the others are captions.
    let sprite = lm.sprite_with_label("wdon1p").context("no note sprite (wdon1p) in onp_all")?;
    let icon = first_blob(&lm.render(sprite, "don", &atlas, None)?).context("the don note rendered empty")?;
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;
    icon.save(&out).with_context(|| format!("writing {}", out.display()))?;
    report.copied.push(NOTE_ICON.into());
    Ok(())
}

/// The topmost shape (the rows down to the first empty one, e.g. the note above its caption), centred on a square.
fn first_blob(img: &RgbaImage) -> Option<RgbaImage> {
    let opaque = |x: u32, y: u32| img.get_pixel(x, y)[3] > 16;
    let filled = |y: u32| (0..img.width()).any(|x| opaque(x, y));
    let top = (0..img.height()).find(|&y| filled(y))?;
    let bottom = (top..img.height()).find(|&y| !filled(y)).unwrap_or(img.height());
    let cols: Vec<u32> = (0..img.width()).filter(|&x| (top..bottom).any(|y| opaque(x, y))).collect();
    let (left, right) = (*cols.first()?, *cols.last()? + 1);
    let (w, h) = (right - left, bottom - top);
    let side = w.max(h);
    let mut square = RgbaImage::new(side, side);
    image::imageops::overlay(&mut square, &image::imageops::crop_imm(img, left, top, w, h).to_image(), ((side - w) / 2) as i64, ((side - h) / 2) as i64);
    Some(square)
}

/// Title plates (index = `TitlePlateId`) and dan badges, plus a Lumen scene of the game's whole nameplate.
pub fn nameplates(game: &Game, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    let sets = |lm: &Lumen| {
        Ok(vec![
            ("plates", "plate", lm.sprite_with_label("wood").context("no sprite labelled wood")?),
            ("dan", "dan", lm.sprite_with_label("5kyuu_red").context("no sprite labelled 5kyuu_red")?),
        ])
    };
    convert(game, NAMEPLATE_LUMEN, dst_dir, NAMEPLATE_INDEX, Some((NAMEPLATE_SCENE, nameplate_set as SceneSets)), report, sets)
}

type SceneSets = fn(&Lumen, &Map<String, Value>) -> Result<(&'static str, Value)>;

/// The game's whole nameplate (frame, plate slot, dan slot); its labels are layouts such as `newentry_shougou_dani`.
/// Each layout lists its text fields (`text_title`, `text_name`) so the WebUI can draw names in the game's boxes.
fn nameplate_set(lm: &Lumen, sets: &Map<String, Value>) -> Result<(&'static str, Value)> {
    let sprite = lm.sprite_placing(PLATE_INSTANCE).context("no nameplate sprite (title_instance)")?;
    let labels = lm.labels(sprite);
    let mut bounds = lm.union_bounds(sprite)?.context("empty nameplate")?;
    // Every plate fits where the nameplate puts it, not only the one placed by default.
    if let (Some(pb), Some([x, y])) = (sets["plates"]["bounds"].as_array(), labels.first().and_then(|l| lm.position_of(sprite, l, PLATE_INSTANCE))) {
        let pb: Vec<f32> = pb.iter().filter_map(|v| v.as_f64()).map(|v| v as f32).collect();
        if let [x0, y0, x1, y1] = pb[..] {
            bounds = [bounds[0].min(x0 + x), bounds[1].min(y0 + y), bounds[2].max(x1 + x), bounds[3].max(y1 + y)];
        }
    }
    let text: Map<String, Value> = labels.iter().map(|l| (l.clone(), Value::Object(lm.text_fields(sprite, l)))).collect();
    Ok(("nameplate", json!({ "sprite": sprite, "bounds": bounds, "labels": labels, "plateInstance": PLATE_INSTANCE, "text": text })))
}

/// The per-difficulty achievement panel (crowns and 粋/雅/極 rank slots) and its digit glyphs.
pub fn scoreboard(game: &Game, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    convert(game, SCOREBOARD_LUMEN, dst_dir, SCOREBOARD_INDEX, None, report, |lm| {
        // The board places the panel as bg_instance next to its rank counters; each counter places digits as n1_ins….
        let board = lm.sprite_placing("num_kiwami_instance").context("no scoreboard sprite (num_kiwami_instance)")?;
        let counter = lm.child(board, "num_kiwami_instance").context("no rank counter")?;
        Ok(vec![
            ("panels", "panel", lm.child(board, "bg_instance").context("no scoreboard panel (bg_instance)")?),
            ("digits", "digit", lm.child(counter, "n1_ins").context("no digit glyphs (n1_ins)")?),
        ])
    })
}

/// Renders each `(index key, file prefix, sprite)` set from `lumen/000_default/<file>` into
/// `<dst_dir>/<prefix>_<label>.png`, one shared canvas per set, and writes an index of the labels.
/// With `scene`, also writes those sets plus the one `scene`'s function adds as a Lumen scene for live playback.
fn convert(
    game: &Game,
    file: &str,
    dst_dir: &Path,
    index_name: &str,
    scene: Option<(&str, SceneSets)>,
    report: &mut crate::Report,
    sets: impl FnOnce(&Lumen) -> Result<Vec<(&'static str, &'static str, u32)>>,
) -> Result<()> {
    let sources: [PathBuf; 2] = ["nulm", "nutexb"].map(|ext| game.file(format!("lumen/000_default/{file}.{ext}")));
    if let Some(missing) = sources.iter().find(|p| !p.is_file()) {
        report.missing.push(missing.display().to_string());
        return Ok(());
    }
    let index = dst_dir.join(index_name);
    let outputs: Vec<PathBuf> = std::iter::once(index.clone()).chain(scene.map(|(name, _)| dst_dir.join(name))).collect();
    let fresh = |o: &PathBuf| sources.iter().try_fold(true, |ok, s| crate::is_newer_than(o, s).map(|n| ok && n));
    if outputs.iter().try_fold(true, |ok, o| fresh(o).map(|n| ok && n))? {
        report.up_to_date.push(file.into());
        return Ok(());
    }

    let lm = Lumen::parse(&fs::read(&sources[0])?).with_context(|| format!("parsing {}", sources[0].display()))?;
    let atlas = crate::textures::decode(&sources[1])?;
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;

    let mut labels = Map::new();
    for (key, prefix, sprite) in sets(&lm)? {
        labels.insert(key.into(), render_set(&lm, &atlas, sprite, prefix, dst_dir, report)?);
    }
    if let Some((name, extra)) = scene {
        let mut sets = labels.clone();
        let (key, set) = extra(&lm, &labels)?;
        sets.insert(key.into(), set);
        let path = dst_dir.join(name);
        fs::write(&path, serde_json::to_string(&lm.scene_json(sets))?).with_context(|| format!("writing {}", path.display()))?;
        let atlas_path = dst_dir.join("atlas.png");
        atlas.save(&atlas_path).with_context(|| format!("writing {}", atlas_path.display()))?;
        report.copied.push(name.into());
    }
    // The index goes last: its being up to date means everything else is.
    fs::write(&index, serde_json::to_string_pretty(&json!(labels))?).with_context(|| format!("writing {}", index.display()))?;
    Ok(())
}

/// Renders every label of `sprite` on one canvas (the union of their mask-clipped bounds) so they line up.
/// Returns the set's index entry: its labels in order, the sprite id and the shared canvas bounds
/// (stage units), which a live Lumen player needs to frame the same canvas.
fn render_set(lm: &Lumen, atlas: &RgbaImage, sprite: u32, prefix: &str, dst_dir: &Path, report: &mut crate::Report) -> Result<Value> {
    let names = lm.labels(sprite);
    let union = lm.union_bounds(sprite)?;
    for name in &names {
        let out = dst_dir.join(format!("{prefix}_{name}.png"));
        match lm.render(sprite, name, atlas, union).and_then(|img| img.save(&out).with_context(|| format!("writing {}", out.display()))) {
            Ok(()) => report.copied.push(format!("{prefix}_{name}")),
            Err(e) => report.failed.push(format!("{prefix}_{name}: {e:#}")),
        }
    }
    Ok(json!({ "sprite": sprite, "bounds": union, "labels": names }))
}

#[cfg(test)]
mod tests {
    use image::{Rgba, RgbaImage};

    #[test]
    fn first_blob_keeps_the_shape_above_the_caption_on_a_square() {
        let mut img = RgbaImage::new(20, 20);
        for y in 2..8 { for x in 4..14 { img.put_pixel(x, y, Rgba([255, 0, 0, 255])); } } // the note: 10 x 6
        for y in 12..15 { for x in 1..19 { img.put_pixel(x, y, Rgba([0, 0, 0, 255])); } } // the caption
        let icon = super::first_blob(&img).unwrap();
        assert_eq!(icon.dimensions(), (10, 10));
        assert_eq!(icon.get_pixel(0, 1)[3], 0, "padding above the note");
        assert_eq!(icon.get_pixel(0, 2), &Rgba([255, 0, 0, 255]), "the note, centred");
        assert!(icon.pixels().all(|p| p[0] == 255 || p[3] == 0), "nothing of the caption");
        assert!(super::first_blob(&RgbaImage::new(4, 4)).is_none());
    }
}
