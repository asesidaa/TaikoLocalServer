//! The older games' textures: `NTP3` (`.nut`, PS3 NUT, big-endian) and the `NUT_PACK_TYPE1` packs (`.ndp`) that hold
//! the costume and tone icons and the title, tone and costume names (rendered text, as images).

use std::collections::BTreeMap;
use std::fs;
use std::path::Path;

use anyhow::{bail, ensure, Context, Result};
use image::RgbaImage;
use image_dds::{ImageFormat, Surface};

use crate::ac15::{Era, Rule};
use crate::source::Source;
use crate::Report;

fn be16(b: &[u8], at: usize) -> Result<u16> {
    Ok(u16::from_be_bytes(b.get(at..at + 2).context("truncated")?.try_into()?))
}
fn be32(b: &[u8], at: usize) -> Result<u32> {
    Ok(u32::from_be_bytes(b.get(at..at + 4).context("truncated")?.try_into()?))
}

pub struct Texture {
    /// The `GIDX` id that model materials refer to.
    pub id: u32,
    pub image: RgbaImage,
}

/// Decodes every texture's base mip. Version 0x0100 stores header, data, header, data; 0x0200 stores all headers
/// first, each with its data's offset (from the header) at +0x20.
pub fn decode(b: &[u8]) -> Result<Vec<Texture>> {
    ensure!(b.get(..4) == Some(b"NTP3"), "not an NTP3 texture");
    let (version, count) = (be16(b, 4)?, be16(b, 6)?);
    let mut h = 0x10;
    let mut out = Vec::with_capacity(count as usize);
    for _ in 0..count {
        let (total, size, header) = (be32(b, h)? as usize, be32(b, h + 8)? as usize, be16(b, h + 12)? as usize);
        let (format, width, height) = (b[h + 19], be16(b, h + 20)? as u32, be16(b, h + 22)? as u32);
        let data = if version >= 0x0200 { h + be32(b, h + 0x20)? as usize } else { h + header };
        // The GIDX block ends the header: "GIDX", its size, then the id.
        let id = b.get(h..h + header).and_then(|hd| hd.windows(4).position(|w| w == b"GIDX")).map(|i| be32(b, h + i + 8)).transpose()?.unwrap_or(0);
        let pixels = b.get(data..data + size).context("texture data out of range")?;
        out.push(Texture { id, image: to_rgba(format, width, height, pixels).with_context(|| format!("texture {id:#x}, format {format}"))? });
        h += if version >= 0x0200 { header } else { total };
    }
    Ok(out)
}

fn to_rgba(format: u8, width: u32, height: u32, data: &[u8]) -> Result<RgbaImage> {
    let image_format = match format {
        0 => ImageFormat::BC1RgbaUnorm,
        1 => ImageFormat::BC2RgbaUnorm,
        2 => ImageFormat::BC3RgbaUnorm,
        // 32-bit: 14 is A8R8G8B8 and 17 is R8G8B8A8, as bytes.
        14 | 17 => {
            let px = data.get(..(width * height * 4) as usize).context("short RGBA data")?;
            let rgba = px.chunks_exact(4).flat_map(|p| if format == 14 { [p[1], p[2], p[3], p[0]] } else { [p[0], p[1], p[2], p[3]] }).collect();
            return RgbaImage::from_raw(width, height, rgba).context("bad size");
        }
        f => bail!("NTP3 format {f} isn't supported"),
    };
    let surface = Surface { width, height, depth: 1, layers: 1, mipmaps: 1, image_format, data };
    Ok(surface.decode_rgba8()?.into_image()?)
}

/// The `.nut` files in a `NUT_PACK_TYPE1` pack, by name. A boost binary archive: the entry count at 0x3E, then each
/// entry's name (u32 length + bytes), offset and size; the first entry also carries a 5-byte class header after its
/// name. The data follows the table and a u32.
pub fn pack(b: &[u8]) -> Result<Vec<(String, &[u8])>> {
    ensure!(b.starts_with(b"NUT_PACK_TYPE1"), "not a NUT_PACK_TYPE1 pack");
    let count = be32(b, 0x3E)?;
    let mut p = 0x4B;
    let mut entries = Vec::with_capacity(count as usize);
    for i in 0..count {
        let len = be32(b, p)? as usize;
        let name = String::from_utf8_lossy(b.get(p + 4..p + 4 + len).context("truncated name")?).trim_end_matches('\0').to_string();
        p += 4 + len + if i == 0 { 5 } else { 0 };
        entries.push((name, be32(b, p)? as usize, be32(b, p + 4)? as usize));
        p += 8;
    }
    let base = p + 4;
    entries.into_iter().map(|(n, o, s)| Ok((n, b.get(base + o..base + o + s).context("pack entry out of range")?))).collect()
}

/// The `nutdata` folders converted to PNG, under `game_data/<era>/<folder>/<name>.png`.
const KINDS: [&str; 5] = ["cos_icon", "tone_icon", "cos_name", "title_name", "tone_name"];

/// `nutdata/<kind>/`, `nutdata/pack/<…>/<kind>/` and `nutdata/<id>/appendable/<nn>/<kind>/`, as in `ac15::NAME_DIRS`.
pub fn inputs(_era: Era) -> Vec<Rule> {
    ["nutdata/", "nutdata/*/*/", "nutdata/*/*/*/"].iter().flat_map(|d| KINDS.map(|k| Rule { path: format!("{d}{k}/"), names: false })).collect()
}

pub fn convert(era: Era, src: &Source, out: &Path) -> Result<Vec<Report>> {
    let rules = inputs(era);
    let own = era.config().map(|id| format!("nutdata/{id}/"));
    let mut reports = Vec::new();
    for kind in KINDS {
        let mut report = Report::new(&format!("{} {kind}", era.name()));
        // Packs from other games' folders first, so this game's own appendable packs win a name clash.
        let mut files: Vec<&str> = src.files(|p| p.split('/').rev().nth(1) == Some(kind) && rules.iter().any(|r| r.matches(p))).collect();
        files.sort_by_key(|p| own.as_deref().is_some_and(|o| p.starts_with(o)));
        let mut images = BTreeMap::new();
        for rel in files {
            let result = src.read(rel).and_then(|b| {
                if rel.ends_with(".ndp") {
                    for (name, nut) in pack(&b)? {
                        images.insert(stem(&name), (rel.to_string(), nut.to_vec()));
                    }
                } else if rel.ends_with(".nut") {
                    images.insert(stem(rel), (rel.to_string(), b));
                }
                Ok(())
            });
            if let Err(e) = result {
                report.failed.push(format!("{rel}: {e:#}"));
            }
        }
        if images.is_empty() {
            continue;
        }
        let dir = out.join(kind);
        fs::create_dir_all(&dir)?;
        for (name, (rel, nut)) in images {
            let dst = dir.join(format!("{name}.png"));
            if is_fresh(&dst, src, &rel) {
                report.up_to_date.push(name);
                continue;
            }
            match decode(&nut).and_then(|t| t.into_iter().next().context("no texture")).and_then(|t| Ok(t.image.save(&dst)?)) {
                Ok(()) => report.copied.push(name),
                Err(e) => report.failed.push(format!("{name} ({rel}): {e:#}")),
            }
        }
        reports.push(report);
    }
    Ok(reports)
}

/// True when `dst` exists and is no older than the game file `rel`.
pub fn is_fresh(dst: &Path, src: &Source, rel: &str) -> bool {
    fs::metadata(dst).and_then(|m| m.modified()).is_ok_and(|t| src.modified(rel).is_ok_and(|s| t >= s))
}

fn stem(name: &str) -> String {
    let file = name.rsplit('/').next().unwrap_or(name);
    file.strip_suffix(".nut").or_else(|| file.strip_suffix(".ndp")).unwrap_or(file).to_string()
}

#[cfg(test)]
mod tests {
    use super::*;

    /// A one-texture v0x0100 NTP3: 2x1, format 14 (A8R8G8B8).
    fn ntp3_argb() -> Vec<u8> {
        let mut b = b"NTP3".to_vec();
        b.extend([1, 0, 0, 1]);
        b.extend([0; 8]);
        let mut h = vec![0u8; 0x50];
        h[0..4].copy_from_slice(&0x58u32.to_be_bytes());
        h[8..12].copy_from_slice(&8u32.to_be_bytes());
        h[12..14].copy_from_slice(&0x50u16.to_be_bytes());
        h[17] = 1;
        h[19] = 14;
        h[20..22].copy_from_slice(&2u16.to_be_bytes());
        h[22..24].copy_from_slice(&1u16.to_be_bytes());
        h[0x40..0x48].copy_from_slice(b"GIDX\0\0\0\x10");
        h[0x48..0x4C].copy_from_slice(&7u32.to_be_bytes());
        b.extend(h);
        b.extend([0x80, 1, 2, 3, 0xFF, 4, 5, 6]);
        b
    }

    #[test]
    fn decodes_argb_and_reads_the_gidx_id() {
        let t = decode(&ntp3_argb()).unwrap();
        assert_eq!(t.len(), 1);
        assert_eq!(t[0].id, 7);
        assert_eq!(t[0].image.as_raw(), &[1, 2, 3, 0x80, 4, 5, 6, 0xFF]);
    }

    /// `NUT=<file.nut> NUT_OUT=<dir> cargo test --release dump_nut -- --ignored` writes each texture as `<id>.png`.
    #[test]
    #[ignore]
    fn dump_nut() {
        let (file, out) = (std::env::var("NUT").unwrap(), std::env::var("NUT_OUT").unwrap());
        for t in decode(&fs::read(file).unwrap()).unwrap() {
            t.image.save(Path::new(&out).join(format!("{:08x}.png", t.id))).unwrap();
        }
    }

    #[test]
    fn reads_pack_entries() {
        let nut = ntp3_argb();
        let mut b = b"NUT_PACK_TYPE1\0\0".to_vec();
        b.resize(0x3E, 0);
        b.extend(2u32.to_be_bytes());
        b.resize(0x4B, 0);
        for (i, name) in ["a_001.nut", "a_002.nut"].iter().enumerate() {
            b.extend((name.len() as u32).to_be_bytes());
            b.extend(name.as_bytes());
            if i == 0 {
                b.extend([0; 5]);
            }
            b.extend(((i * nut.len()) as u32).to_be_bytes());
            b.extend((nut.len() as u32).to_be_bytes());
        }
        b.extend(((2 * nut.len()) as u32).to_be_bytes());
        b.extend(&nut);
        b.extend(&nut);
        let entries = pack(&b).unwrap();
        assert_eq!(entries.iter().map(|e| e.0.as_str()).collect::<Vec<_>>(), ["a_001.nut", "a_002.nut"]);
        assert!(entries.iter().all(|e| e.1 == &nut[..]));
    }
}
