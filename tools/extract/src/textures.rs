use std::fs;
use std::path::Path;

use anyhow::{Context, Result};
use image::RgbaImage;
use nutexb::ddsfile::{AlphaMode, D3D10ResourceDimension, Dds, NewDxgiParams};
use nutexb::NutexbFile;

/// `NutexbFooter::unk3` value for Tegra-swizzled (Switch) data. The PC build ships linear data (`0`).
const SWIZZLED: u32 = 0x1000;

/// Size of the footer, which starts with a magic the `nutexb` crate checks.
const FOOTER_SIZE: usize = 112;

/// Decodes the base mip of a `.nutexb` to RGBA8.
pub fn decode(path: &Path) -> Result<RgbaImage> {
    let mut bytes = fs::read(path).with_context(|| format!("reading {}", path.display()))?;
    // Nijiiro PC writes "46XT" where Smash/Switch files have " XNT"; the rest of the footer is identical.
    if let Some(magic) = bytes.len().checked_sub(FOOTER_SIZE).map(|i| &mut bytes[i..i + 4]) {
        if magic == b"46XT" {
            magic.copy_from_slice(b" XNT");
        }
    }
    let nutexb = NutexbFile::read(&mut std::io::Cursor::new(bytes))
        .with_context(|| format!("parsing {}", path.display()))?;
    let dds = if nutexb.footer.unk3 == SWIZZLED {
        nutexb.to_dds().with_context(|| format!("deswizzling {}", path.display()))?
    } else {
        linear_dds(&nutexb)?
    };
    // Array layers (the 12-row face atlases) come out stacked vertically, which is what the previewer expects.
    image_dds::image_from_dds(&dds, 0).with_context(|| format!("decoding {}", path.display()))
}

/// Mirrors `nutexb::dds::create_dds` without its unconditional deswizzle, for the PC build's linear data.
fn linear_dds(nutexb: &NutexbFile) -> Result<Dds> {
    let f = &nutexb.footer;
    let mut dds = Dds::new_dxgi(NewDxgiParams {
        height: f.height,
        width: f.width,
        depth: None,
        format: f.image_format.into(),
        mipmap_levels: Some(f.mipmap_count.max(1)),
        array_layers: Some(f.layer_count.max(1)),
        caps2: None,
        is_cubemap: false,
        resource_dimension: D3D10ResourceDimension::Texture2D,
        alpha_mode: AlphaMode::Unknown,
    })?;
    dds.data = nutexb.data[..f.data_size as usize].to_vec();
    Ok(dds)
}

/// Converts every `*.nutexb` in `src_dir` to `<dst_dir>/<name>.png`, skipping outputs that are current.
pub fn convert_dir(src_dir: &Path, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    let Ok(entries) = fs::read_dir(src_dir) else {
        report.missing.push(src_dir.display().to_string());
        return Ok(());
    };
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;

    let mut paths: Vec<_> = entries
        .filter_map(|e| e.ok().map(|e| e.path()))
        .filter(|p| p.extension().is_some_and(|e| e == "nutexb"))
        .collect();
    paths.sort();

    for src in paths {
        let name = src.file_stem().unwrap().to_string_lossy().into_owned();
        let dst = dst_dir.join(format!("{name}.png"));
        if crate::is_newer_than(&dst, &src)? {
            report.up_to_date.push(name);
            continue;
        }
        // One bad texture shouldn't stop the rest of the extraction.
        match decode(&src).and_then(|img| img.save(&dst).with_context(|| format!("writing {}", dst.display()))) {
            Ok(()) => report.copied.push(name),
            Err(e) => report.failed.push(format!("{name}: {e:#}")),
        }
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Builds a minimal linear (PC-style) R8G8B8A8 nutexb with Nijiiro's "46XT" footer magic.
    fn nijiiro_rgba8(width: u32, height: u32, layers: u32, pixels: &[u8]) -> Vec<u8> {
        let mut b = pixels.to_vec();
        let layer_size = width * height * 4;
        for _ in 0..layers {
            let mut mips = vec![0u8; 0x40];
            mips[..4].copy_from_slice(&layer_size.to_le_bytes());
            b.extend(mips);
        }
        b.extend(b"46XT");
        b.extend([0u8; 0x40]); // name
        for v in [width, height, 1, 0x0400, 4, 1, 0, layers, pixels.len() as u32] {
            b.extend(v.to_le_bytes());
        }
        b.extend(b" XET");
        b.extend([1, 0, 2, 0]);
        b
    }

    #[test]
    fn decodes_linear_nijiiro_texture_with_layers_stacked() {
        let pixels: Vec<u8> = (0..2 * 2 * 4 * 2).map(|i| i as u8).collect(); // 2x2, 2 layers
        let path = std::env::temp_dir().join(format!("taiko-extract-tex-{}.nutexb", std::process::id()));
        fs::write(&path, nijiiro_rgba8(2, 2, 2, &pixels)).unwrap();

        let img = decode(&path).unwrap();
        fs::remove_file(&path).unwrap();

        assert_eq!(img.dimensions(), (2, 4));
        assert_eq!(img.as_raw(), &pixels);
    }
}
