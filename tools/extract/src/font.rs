//! The game's glyph sheets (`font/<sheet>.{nutexb,xml}`), split into 256-code-point pages so the WebUI's
//! nameplate and scoreboard previews can draw names in the game's lettering without loading a 4096² sheet:
//! `<dst>/<sheet>/<page>.png` (glyphs row-packed) and `<page>.json` `{ "point": 32, "glyphs": { "<cp>": [x, y, w, h] } }`.

use std::fs;
use std::path::Path;

use anyhow::{Context, Result};
use image::{GenericImageView, RgbaImage};
use serde_json::{json, Map};

/// One sheet per `MyDonNameLanguage` (Japanese, English, Traditional Chinese, Korean); there is no Simplified sheet.
pub const SHEETS: [&str; 4] = ["jp_32_DB", "en_32", "tw_32", "kr_32"];
const PAGE_WIDTH: u32 = 1024;

pub fn convert(x64: &Path, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    for sheet in SHEETS {
        let (xml, tex) = (x64.join("font").join(format!("{sheet}.xml")), x64.join("font").join(format!("{sheet}.nutexb")));
        if let Some(missing) = [&xml, &tex].into_iter().find(|p| !p.is_file()) {
            report.missing.push(missing.display().to_string());
            continue;
        }
        // Page 0 (ASCII) is written last, so its being up to date means the whole sheet is.
        let marker = dst_dir.join(sheet).join("0.json");
        if crate::is_newer_than(&marker, &xml)? && crate::is_newer_than(&marker, &tex)? {
            report.up_to_date.push(sheet.into());
            continue;
        }
        match split(&fs::read_to_string(&xml)?, &crate::textures::decode(&tex)?, &dst_dir.join(sheet)) {
            Ok(pages) => report.copied.push(format!("{sheet} ({pages} pages)")),
            Err(e) => report.failed.push(format!("{sheet}: {e:#}")),
        }
    }
    Ok(())
}

/// Writes the pages of one sheet; returns how many.
fn split(xml: &str, sheet: &RgbaImage, dst: &Path) -> Result<usize> {
    let point: u32 = attr(xml, "fontPoint").and_then(|v| v.parse().ok()).context("no fontPoint")?;
    let mut pages: std::collections::BTreeMap<u32, Vec<[u32; 5]>> = Default::default();
    for tag in xml.split("<glyph ").skip(1) {
        let n = |name| attr(tag, name).and_then(|v| v.parse::<u32>().ok());
        let (Some(cp), Some(u), Some(v), Some(w), Some(h)) = (n("index"), n("offsetU"), n("offsetV"), n("width"), n("height")) else { continue };
        if (w, h) == (4, 1) && cp != 32 {
            continue; // the sheet's "no glyph" placeholder
        }
        pages.entry(cp >> 8).or_default().push([cp, u, v, w, h]);
    }
    fs::create_dir_all(dst).with_context(|| format!("creating {}", dst.display()))?;
    // Which pages exist, so the WebUI never requests a missing one while walking its fallback sheets.
    fs::write(dst.join("pages.json"), serde_json::to_string(&pages.keys().collect::<Vec<_>>())?)?;
    for (&page, glyphs) in pages.iter().rev() {
        // Row-pack with a 1px gap so bilinear sampling never bleeds a neighbour in.
        let (mut x, mut y, mut row) = (0, 0, 0);
        let mut placed = Vec::with_capacity(glyphs.len());
        for &[cp, u, v, w, h] in glyphs {
            if x + w > PAGE_WIDTH {
                (x, y, row) = (0, y + row + 1, 0);
            }
            placed.push((cp, u, v, w, h, x, y));
            x += w + 1;
            row = row.max(h);
        }
        let mut img = RgbaImage::new(PAGE_WIDTH, (y + row).max(1));
        let mut map = Map::new();
        for (cp, u, v, w, h, x, y) in placed {
            if u + w > sheet.width() || v + h > sheet.height() {
                continue;
            }
            for (dx, dy, p) in sheet.view(u, v, w, h).pixels() {
                img.put_pixel(x + dx, y + dy, p);
            }
            map.insert(cp.to_string(), json!([x, y, w, h]));
        }
        img.save(dst.join(format!("{page}.png")))?;
        fs::write(dst.join(format!("{page}.json")), serde_json::to_string(&json!({ "point": point, "glyphs": map }))?)?;
    }
    Ok(pages.len())
}

/// The value of `name="…"` in an XML tag's text.
fn attr<'a>(tag: &'a str, name: &str) -> Option<&'a str> {
    let key = format!("{name}=\"");
    // Whole attribute names only ("width" must not match "texWidth").
    let at = tag.match_indices(&key).map(|(i, _)| i).find(|&i| i == 0 || tag.as_bytes()[i - 1].is_ascii_whitespace())?;
    tag[at + key.len()..].split('"').next()
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn splits_glyphs_into_pages_and_skips_placeholders() {
        let xml = r#"<root><font texWidth="8" texHeight="4" fontPoint="32">
            <glyph index="65" type="1" offsetU="0" offsetV="0" width="2" height="2" />
            <glyph index="66" type="1" offsetU="0" offsetV="0" width="4" height="1" />
            <glyph index="12354" type="1" offsetU="2" offsetV="0" width="3" height="4" />
        </font></root>"#;
        let mut sheet = RgbaImage::new(8, 4);
        sheet.put_pixel(2, 3, image::Rgba([1, 2, 3, 4]));
        let dst = std::env::temp_dir().join(format!("extract-font-test-{}", std::process::id()));

        assert_eq!(split(xml, &sheet, &dst).unwrap(), 2, "ASCII page 0 and page 0x30");
        let page0: serde_json::Value = serde_json::from_str(&fs::read_to_string(dst.join("0.json")).unwrap()).unwrap();
        assert_eq!(page0, json!({ "point": 32, "glyphs": { "65": [0, 0, 2, 2] } }), "placeholder 66 dropped");
        assert_eq!(fs::read_to_string(dst.join("pages.json")).unwrap(), "[0,48]");
        let kana = image::open(dst.join(format!("{}.png", 12354 >> 8))).unwrap().to_rgba8();
        assert_eq!(kana.get_pixel(0, 3).0, [1, 2, 3, 4], "glyph pixels copied from the sheet");
        fs::remove_dir_all(dst).ok();
    }
}
