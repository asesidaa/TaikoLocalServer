//! The older games' Lumen UI packs: `lumendata/packed/<scene>/packeddata.ddp` (`LM_NUT_TYPE1`), each holding
//! several `.lm` files (big-endian `LMB`) and their atlas textures (`img*.nut`, NTP3).

use anyhow::{ensure, Context, Result};

pub struct LmFile<'a> {
    pub name: String,
    pub lm: &'a [u8],
    /// The `.lm`'s atlases, in atlas-id order.
    pub nuts: Vec<&'a [u8]>,
}

fn be32(b: &[u8], at: usize) -> Result<usize> {
    Ok(u32::from_be_bytes(b.get(at..at + 4).context("truncated pack")?.try_into()?) as usize)
}

/// A boost binary archive like the NUT packs: the `.lm` count at 0x3E, then from 0x4B per `.lm` its name (u32
/// length + bytes; the first entry adds a 5-byte class header), offset, size and the `[from, to)` range of its
/// textures in the `.nut` list. Then 5 bytes, the `.nut` count, 9 bytes, and an (offset, size) pair per `.nut`.
/// The data starts 16 bytes after the last pair: the `.lm` files, then the `.nut` files (offsets from there).
pub fn read(b: &[u8]) -> Result<Vec<LmFile<'_>>> {
    ensure!(b.starts_with(b"LM_NUT_TYPE1"), "not an LM_NUT_TYPE1 pack");
    let count = be32(b, 0x3E)?;
    let mut p = 0x4B;
    let mut lms = Vec::with_capacity(count);
    for i in 0..count {
        let len = be32(b, p)?;
        let name = String::from_utf8_lossy(b.get(p + 4..p + 4 + len).context("truncated name")?).trim_end_matches('\0').to_string();
        p += 4 + len + if i == 0 { 5 } else { 0 };
        lms.push((name, be32(b, p)?, be32(b, p + 4)?, be32(b, p + 8)?, be32(b, p + 12)?));
        p += 16;
    }
    let nut_count = be32(b, p + 5)?;
    p += 5 + 4 + 9;
    let nuts = (0..nut_count).map(|k| Ok((be32(b, p + k * 8)?, be32(b, p + k * 8 + 4)?))).collect::<Result<Vec<_>>>()?;
    let base = p + nut_count * 8 + 16;
    let nut_base = base + lms.iter().map(|l| l.1 + l.2).max().unwrap_or(0);
    let slice = |at: usize, len: usize| b.get(at..at + len).context("pack entry out of range");
    lms.into_iter()
        .map(|(name, off, size, from, to)| {
            let lm = slice(base + off, size)?;
            ensure!(lm.starts_with(b"LMB\0"), "{name}: not an LMB file");
            let nuts = nuts.get(from..to).context("texture range out of bounds")?.iter().map(|&(o, s)| slice(nut_base + o, s)).collect::<Result<_>>()?;
            Ok(LmFile { name, lm, nuts })
        })
        .collect()
}

enum Sel {
    /// The sprite with this label and the most labels (`Lumen::sprite_with_label`).
    Label(&'static str),
    /// The sprite whose labels are exactly these (`Lumen::sprite_with_labels`).
    Labels(&'static str),
    /// The sprite placed under this instance name.
    Instance(&'static str),
}

/// The UI sets rendered to `game_data/<era>/ui/<prefix>_<label>.png`, each from the first (scene, `.lm`, selector)
/// the game has. Sprites are found by labels or instance names, never ids, which change between builds.
const SETS: [(&str, &[(&str, &str, Sel)]); 5] = [
    ("crown", &[("song_select", "song_select/song_select.lm", Sel::Labels("none,silver,gold"))]),
    // A crown beside each difficulty's face: gold_easy … silver_extreme.
    ("crown_course", &[("song_select", "song_select/song_select.lm", Sel::Label("gold_mania"))]),
    ("difficulty", &[
        ("song_select", "song_select/song_select.lm", Sel::Instance("courseicon")),
        ("enso_result", "result/result.lm", Sel::Labels("easy,normal,hard,mania")),
    ]),
    ("dan", &[("dani_result", "dani_result/dani_result.lm", Sel::Instance("dani_title"))]),
    // Title plates by rarity: normal, gold, platinum, rainbow.
    ("plate", &[("enso_result", "result/result.lm", Sel::Instance("titlepanel"))]),
];

pub fn inputs() -> Vec<crate::ac15::Rule> {
    let mut scenes: Vec<&str> = SETS.iter().flat_map(|(_, c)| c.iter().map(|(s, _, _)| *s)).collect();
    scenes.dedup();
    scenes.iter().map(|s| crate::ac15::Rule { path: format!("lumendata/packed/{s}/packeddata.ddp"), names: false }).collect()
}

/// Renders `SETS`, with one `ui.json` listing each set's labels in order.
pub fn convert(src: &crate::source::Source, out: &std::path::Path, report: &mut crate::Report) -> Result<()> {
    use std::collections::HashMap;
    let dir = out.join("ui");
    let index = dir.join("ui.json");
    let packs: Vec<String> = inputs().into_iter().map(|r| r.path).filter(|p| src.exists(p)).collect();
    if packs.is_empty() {
        return Ok(());
    }
    if packs.iter().all(|p| crate::nut::is_fresh(&index, src, p)) {
        report.up_to_date.push("ui".into());
        return Ok(());
    }
    std::fs::create_dir_all(&dir)?;
    let mut bytes = HashMap::new();
    for p in &packs {
        bytes.insert(p.as_str(), src.read(p)?);
    }
    let mut lumens = HashMap::new();
    for (key, b) in &bytes {
        for f in read(b).with_context(|| format!("reading {key}"))? {
            if SETS.iter().flat_map(|(_, c)| c.iter()).any(|(_, name, _)| *name == f.name) {
                lumens.insert((key.to_string(), f.name.clone()), (crate::lumen::Lumen::parse(f.lm).with_context(|| format!("parsing {}", f.name))?, f.nuts));
            }
        }
    }
    let mut index_json = serde_json::Map::new();
    for (prefix, choices) in SETS {
        let found = choices.iter().find_map(|(scene, lm_name, sel)| {
            let (lm, nuts) = lumens.get(&(format!("lumendata/packed/{scene}/packeddata.ddp"), lm_name.to_string()))?;
            let sprite = match sel {
                Sel::Label(l) => lm.sprite_with_label(l),
                Sel::Labels(l) => lm.sprite_with_labels(l),
                Sel::Instance(i) => lm.sprite_placing(i).and_then(|p| lm.child(p, i)),
            }?;
            Some((lm, nuts, sprite))
        });
        let Some((lm, nuts, sprite)) = found else { continue };
        let used = lm.atlases(sprite)?;
        let layers: Vec<_> = (0..nuts.len() as u32)
            .map(|k| if used.contains(&k) { crate::nut::decode(nuts[k as usize]).map(|mut t| t.remove(0).image) } else { Ok(image::RgbaImage::new(0, 0)) })
            .collect::<Result<_>>()?;
        let union = lm.union_bounds(sprite)?;
        let labels = lm.labels(sprite);
        for label in &labels {
            let path = dir.join(format!("{prefix}_{label}.png"));
            match lm.render_layers(sprite, label, &layers, union).and_then(|img| Ok(img.save(&path)?)) {
                Ok(()) => report.copied.push(format!("{prefix}_{label}")),
                Err(e) => report.failed.push(format!("{prefix}_{label}: {e:#}")),
            }
        }
        index_json.insert(prefix.into(), labels.into());
    }
    // Last, so its being newer than the packs means everything else is too.
    std::fs::write(&index, serde_json::to_string_pretty(&index_json)?)?;
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    /// `LM_PACK=<packeddata.ddp> [LM_NAME=<substring>] [LM_SPRITE=<id> LM_OUT=<dir>] cargo test --release lm_ls -- --ignored
    /// --nocapture` lists each `.lm`'s sprites, labels and instance names; with `LM_SPRITE`, renders every label of it.
    #[test]
    #[ignore]
    fn lm_ls() {
        let b = std::fs::read(std::env::var("LM_PACK").unwrap()).unwrap();
        let only = std::env::var("LM_NAME").unwrap_or_default();
        for f in read(&b).unwrap().into_iter().filter(|f| f.name.contains(&only)) {
            let Ok(lm) = crate::lumen::Lumen::parse(f.lm) else { println!("== {} (no atlas)", f.name); continue };
            println!("== {} ({} atlases)\n{}", f.name, f.nuts.len(), lm.describe());
            let Ok(sprite) = std::env::var("LM_SPRITE").map(|s| s.parse::<u32>().unwrap()) else { continue };
            let images: Vec<_> = f.nuts.iter().map(|n| crate::nut::decode(n).unwrap().remove(0).image).collect();
            let out = std::path::PathBuf::from(std::env::var("LM_OUT").unwrap());
            for label in lm.labels(sprite) {
                match lm.render_layers(sprite, &label, &images, None) {
                    Ok(img) => img.save(out.join(format!("{sprite}_{label}.png"))).unwrap(),
                    Err(e) => println!("{label}: {e:#}"),
                }
            }
        }
    }
}
