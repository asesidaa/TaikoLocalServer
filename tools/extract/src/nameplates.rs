//! Title plates and dan badges, rendered from the game's Lumen nameplate UI
//! (`lumen/000_default/common/name_plate.{nulm,nutexb}`).

use std::fs;
use std::path::Path;

use anyhow::{Context, Result};
use serde_json::json;

use crate::lumen::Lumen;

/// Frame labels that identify the sprites; their ids differ between builds.
const PLATE_LABEL: &str = "wood";
const BADGE_LABEL: &str = "5kyuu_red";

/// Lists the rendered labels in timeline order; a plate's index is its `TitlePlateId`.
pub const INDEX: &str = "plates.json";

pub fn convert(x64: &Path, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    let src = x64.join("lumen").join("000_default").join("common");
    let (nulm, nutexb) = (src.join("name_plate.nulm"), src.join("name_plate.nutexb"));
    for p in [&nulm, &nutexb] {
        if !p.is_file() {
            report.missing.push(p.display().to_string());
            return Ok(());
        }
    }
    let index = dst_dir.join(INDEX);
    if crate::is_newer_than(&index, &nulm)? && crate::is_newer_than(&index, &nutexb)? {
        report.up_to_date.push("nameplates".into());
        return Ok(());
    }

    let lm = Lumen::parse(&fs::read(&nulm)?).with_context(|| format!("parsing {}", nulm.display()))?;
    let atlas = crate::textures::decode(&nutexb)?;
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;

    let mut labels = json!({});
    for (key, label, prefix) in [("plates", PLATE_LABEL, "plate"), ("dan", BADGE_LABEL, "dan")] {
        let Some(sprite) = lm.sprite_with_label(label) else {
            report.missing.push(format!("sprite labelled {label}"));
            continue;
        };
        let names: Vec<String> = lm.sprites[&sprite].labels.iter().map(|(n, _)| n.clone()).collect();
        // One shared canvas per set so every plate (or badge) lines up.
        let mut union: Option<[f32; 4]> = None;
        for name in &names {
            if let Some(b) = lm.bounds(sprite, name)? {
                union = Some(union.map_or(b, |u| [u[0].min(b[0]), u[1].min(b[1]), u[2].max(b[2]), u[3].max(b[3])]));
            }
        }
        for name in &names {
            let out = dst_dir.join(format!("{prefix}_{name}.png"));
            match lm.render(sprite, name, &atlas, union).and_then(|img| img.save(&out).with_context(|| format!("writing {}", out.display()))) {
                Ok(()) => report.copied.push(format!("{prefix}_{name}")),
                Err(e) => report.failed.push(format!("{prefix}_{name}: {e:#}")),
            }
        }
        labels[key] = json!(names);
    }
    fs::write(&index, serde_json::to_string_pretty(&labels)?).with_context(|| format!("writing {}", index.display()))?;
    Ok(())
}
