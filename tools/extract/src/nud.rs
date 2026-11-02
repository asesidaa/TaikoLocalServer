//! The older games' 3D don-chan parts: NUD (`NDP3`, PS3, big-endian) meshes with their `.nut` textures beside them,
//! converted to binary glTF. The layout follows Smash Forge's `NUD.cs` (MIT); see the README for what the files use.

use std::collections::HashMap;
use std::io::Cursor;

use anyhow::{bail, ensure, Context, Result};
use serde_json::json;

use crate::models::{Gltf, ARRAY_BUFFER, ELEMENT_ARRAY_BUFFER, UNSIGNED_INT, UNSIGNED_SHORT};

pub struct Object {
    pub name: String,
    /// The bone a whole object moves with (faces, hats), when its vertices carry no weights.
    pub bone: Option<u16>,
    pub parts: Vec<Part>,
}

pub struct Part {
    pub positions: Vec<f32>,
    pub normals: Vec<f32>,
    pub colors: Option<Vec<f32>>,
    pub uvs: Vec<f32>,
    pub joints: Option<Vec<u16>>,
    pub weights: Option<Vec<f32>>,
    pub indices: Vec<u32>,
    /// The first pass's first texture (a `GIDX` id in the `.nut`); the second pass is the outline shell.
    pub texture: Option<u32>,
    pub double_sided: bool,
    /// The first pass's material flags (`0x10000001`: its low byte picks the shader; see the README).
    pub flags: u32,
    /// Source or destination blend factor set: alpha-blended. Otherwise the pass is opaque and ignores texture alpha.
    pub blend: bool,
    /// Alpha test: the reference (0–255) a texel's alpha must reach.
    pub alpha_ref: Option<u16>,
    /// A second pass culling front faces: the black outline shell.
    pub outline: bool,
}

struct R<'a>(&'a [u8]);

impl R<'_> {
    fn u8(&self, o: usize) -> Result<u8> {
        self.0.get(o).copied().context("truncated NUD")
    }
    fn u16(&self, o: usize) -> Result<u16> {
        Ok(u16::from_be_bytes(self.0.get(o..o + 2).context("truncated NUD")?.try_into()?))
    }
    fn u32(&self, o: usize) -> Result<u32> {
        Ok(u32::from_be_bytes(self.0.get(o..o + 4).context("truncated NUD")?.try_into()?))
    }
    fn f32(&self, o: usize) -> Result<f32> {
        Ok(f32::from_bits(self.u32(o)?))
    }
    fn f16(&self, o: usize) -> Result<f32> {
        Ok(half(self.u16(o)?))
    }
}

fn half(h: u16) -> f32 {
    let (sign, exp, frac) = ((h >> 15) as u32, ((h >> 10) & 0x1F) as i32, (h & 0x3FF) as f32);
    let v = match exp {
        0 => frac / 1024.0 * 2f32.powi(-14),
        31 => if frac == 0.0 { f32::INFINITY } else { f32::NAN },
        e => (1.0 + frac / 1024.0) * 2f32.powi(e - 15),
    };
    if sign == 1 { -v } else { v }
}

/// Header: `NDP3`, u32 size, u16 version (0x0200), u16 object count, u16 type, u16 bone count, then the sizes of the
/// index, vertex and vertex-add clumps (the index clump starts at the first value + 0x30), a bounding sphere, and
/// 0x30-byte object entries; the names follow the clumps.
pub fn parse(b: &[u8]) -> Result<Vec<Object>> {
    let r = R(b);
    ensure!(b.starts_with(b"NDP3"), "not an NDP3 model");
    ensure!(r.u16(8)? == 0x0200, "NDP3 version {:#x} isn't supported", r.u16(8)?);
    let count = r.u16(10)? as usize;
    let index_at = r.u32(0x10)? as usize + 0x30;
    let vert_at = index_at + r.u32(0x14)? as usize;
    let add_at = vert_at + r.u32(0x18)? as usize;
    let name_at = add_at + r.u32(0x1C)? as usize;
    let mut objects = Vec::with_capacity(count);
    for i in 0..count {
        // Bounding sphere (8 floats), name offset, flags, single-bind bone, polygon count, polygon header offset.
        let o = 0x30 + i * 0x30;
        let name_start = name_at + r.u32(o + 32)? as usize;
        let name_end = b[name_start..].iter().position(|&c| c == 0).map_or(b.len(), |n| name_start + n);
        let bone = r.u16(o + 40)?;
        let mut parts = Vec::new();
        for k in 0..r.u16(o + 42)? as usize {
            parts.push(part(&r, r.u32(o + 44)? as usize + k * 0x30, index_at, vert_at, add_at)?);
        }
        objects.push(Object { name: String::from_utf8_lossy(&b[name_start..name_end]).into_owned(), bone: (bone != 0xFFFF).then_some(bone), parts });
    }
    Ok(objects)
}

/// A polygon header (0x30): index, vertex and vertex-add offsets, u16 vertex count, the vertex type (high nibble
/// bones: 0 none, 1 four u32 indices + four float weights; low nibble normals: 1 float, 6 half float), the UV type
/// (high nibble UV count; low nibble colour: 0 none, 2 RGBA bytes), four material offsets, u16 index count, strip
/// flags. With bones the UVs and colours are in the vertex clump and the rest in the vertex-add clump.
fn part(r: &R, h: usize, index_at: usize, vert_at: usize, add_at: usize) -> Result<Part> {
    let (index_off, vert_off, add_off) = (r.u32(h)? as usize, r.u32(h + 4)? as usize, r.u32(h + 8)? as usize);
    let n = r.u16(h + 12)? as usize;
    let (vtype, uvtype) = (r.u8(h + 14)?, r.u8(h + 15)?);
    let (bones, normal) = (vtype >> 4, vtype & 0xF);
    let (uv_count, color) = ((uvtype >> 4) as usize, uvtype & 0xF);
    ensure!(matches!(bones, 0 | 1) && matches!(normal, 1 | 6) && matches!(color, 0 | 2), "vertex type {vtype:#x}/{uvtype:#x} isn't supported");
    ensure!(r.u8(h + 34)? >> 4 != 4, "triangle-list polygons aren't supported");

    let mut p = Part { positions: Vec::with_capacity(n * 3), normals: Vec::with_capacity(n * 3), colors: (color == 2).then(Vec::new), uvs: Vec::new(), joints: None, weights: None, indices: Vec::new(), texture: None, double_sided: false, flags: 0, blend: false, alpha_ref: None, outline: false };
    // Colour and UVs: interleaved per vertex, after the position and normal when there are no bones.
    let uv_block = |p: &mut Part, mut o: usize| -> Result<usize> {
        if let Some(c) = p.colors.as_mut() {
            // 0x7F is 1.0, as in Smash's NUD; values above it brighten.
            c.extend((0..4).map(|k| r.u8(o + k).map(|v| v as f32 / 127.0)).collect::<Result<Vec<_>>>()?);
            o += 4;
        }
        for u in 0..uv_count {
            if u == 0 {
                p.uvs.extend([r.f16(o)?, r.f16(o + 2)?]);
            }
            o += 4;
        }
        Ok(o)
    };
    let (mut o, mut uo) = if bones == 1 { (add_at + add_off, vert_at + vert_off) } else { (vert_at + vert_off, 0) };
    let (mut joints, mut weights) = (Vec::new(), Vec::new());
    for _ in 0..n {
        p.positions.extend([r.f32(o)?, r.f32(o + 4)?, r.f32(o + 8)?]);
        if normal == 1 {
            // Position w, normal xyz, normal w.
            p.normals.extend([r.f32(o + 16)?, r.f32(o + 20)?, r.f32(o + 24)?]);
            o += 32;
        } else {
            p.normals.extend([r.f16(o + 12)?, r.f16(o + 14)?, r.f16(o + 16)?]);
            o += 20;
        }
        if bones == 1 {
            let w: Vec<f32> = (0..4).map(|k| r.f32(o + 16 + k * 4)).collect::<Result<_>>()?;
            let sum: f32 = w.iter().sum();
            for k in 0..4 {
                joints.push(if w[k] > 0.0 { r.u32(o + k * 4)? as u16 } else { 0 });
                weights.push(if sum > 0.0 { w[k] / sum } else if k == 0 { 1.0 } else { 0.0 });
            }
            o += 32;
            uo = uv_block(&mut p, uo)?;
        } else {
            o = uv_block(&mut p, o)?;
        }
    }
    if bones == 1 {
        (p.joints, p.weights) = (Some(joints), Some(weights));
    }

    // Triangle strips, 0xFFFF restarting one; every other triangle flips to keep the winding.
    let strip: Vec<u16> = (0..r.u16(h + 32)? as usize).map(|k| r.u16(index_at + index_off + k * 2)).collect::<Result<_>>()?;
    for run in strip.split(|&i| i == 0xFFFF) {
        for k in 2..run.len() {
            let (a, b, c) = if k % 2 == 0 { (run[k - 2], run[k - 1], run[k]) } else { (run[k - 1], run[k - 2], run[k]) };
            if a != b && b != c && a != c {
                p.indices.extend([a as u32, b as u32, c as u32]);
            }
        }
    }
    ensure!(p.indices.iter().all(|&i| (i as usize) < n), "index out of range");

    // Material: u32 flags, u32, u16 source blend, u16 texture count, u16 destination blend, u8 alpha test (2 on), u8
    // alpha function, u16 alpha reference, u16 cull mode (0x405 back, 0x404 front, 0 none), …; textures from +0x20,
    // 0x18 bytes each, the first u32 being the texture's GIDX id.
    let mat = r.u32(h + 16)? as usize;
    if mat != 0 {
        if r.u16(mat + 10)? > 0 {
            p.texture = Some(r.u32(mat + 0x20)?);
        }
        p.double_sided = r.u16(mat + 18)? == 0;
        p.flags = r.u32(mat)?;
        p.blend = r.u16(mat + 8)? != 0 || r.u16(mat + 12)? != 0;
        p.alpha_ref = (r.u8(mat + 14)? != 0).then(|| r.u16(mat + 16)).transpose()?;
    }
    let second = r.u32(h + 20)? as usize;
    p.outline = second != 0 && r.u16(second + 18)? == 0x404;
    Ok(p)
}

/// glTF's alpha mode for a pass: blended, alpha-tested (cutoff = reference / 255), or opaque. An opaque pass ignores
/// texture alpha: the bodies' recolour masks keep colour under alpha 0.
fn alpha_mode(p: &Part) -> (&'static str, Option<f32>) {
    match (p.blend, p.alpha_ref) {
        (true, _) => ("BLEND", None),
        (false, Some(r)) => ("MASK", Some(r.min(255) as f32 / 255.0)),
        _ => ("OPAQUE", None),
    }
}

/// The model's objects as glTF meshes (one primitive per polygon) with their textures embedded. With the rig and its
/// bind pose, the meshes are skinned to it (nodes `0..rig.len()`, named as in `animation/don.glb`); objects bound
/// whole to one bone get that bone at full weight.
pub fn glb(objects: &[Object], textures: &[crate::nut::Texture], rig: Option<(&[crate::ani::Bone], &crate::ani::Pose)>, clip: Option<(&[u8], usize)>) -> Result<Vec<u8>> {
    let by_id: HashMap<u32, &image::RgbaImage> = textures.iter().map(|t| (t.id, &t.image)).collect();
    let mut g = Gltf::default();
    let mut images: HashMap<u32, usize> = HashMap::new();
    let mut nodes = Vec::new();
    if let Some((bones, bind)) = rig {
        nodes.push(crate::ani::push_bones(&mut g, bones, bind));
        let inverse: Vec<u8> = crate::ani::world(bones, bind).iter().flat_map(|m| m.inverse().to_cols_array()).flat_map(|f| f.to_le_bytes()).collect();
        let accessor = g.accessor(&inverse, crate::models::FLOAT, bones.len(), "MAT4", None, None);
        g.skins.push(json!({ "joints": (0..bones.len()).collect::<Vec<_>>(), "inverseBindMatrices": accessor, "skeleton": 0 }));
    }
    let bone_count = rig.map_or(0, |(b, _)| b.len());
    for o in objects {
        let mut primitives = Vec::new();
        for p in &o.parts {
            let n = p.positions.len() / 3;
            let (mut min, mut max) = (vec![f32::MAX; 3], vec![f32::MIN; 3]);
            for v in p.positions.chunks_exact(3) {
                for c in 0..3 {
                    min[c] = min[c].min(v[c]);
                    max[c] = max[c].max(v[c]);
                }
            }
            let pos: Vec<u8> = p.positions.iter().flat_map(|f| f.to_le_bytes()).collect();
            let mut attributes = json!({ "POSITION": g.accessor(&pos, crate::models::FLOAT, n, "VEC3", Some(ARRAY_BUFFER), Some((min, max))) });
            attributes["NORMAL"] = json!(g.accessor_f32(&p.normals, "VEC3", 3, Some(ARRAY_BUFFER)));
            if !p.uvs.is_empty() {
                attributes["TEXCOORD_0"] = json!(g.accessor_f32(&p.uvs, "VEC2", 2, Some(ARRAY_BUFFER)));
            }
            if let Some(c) = &p.colors {
                attributes["COLOR_0"] = json!(g.accessor_f32(c, "VEC4", 4, Some(ARRAY_BUFFER)));
            }
            let whole = (vec![o.bone.unwrap_or(0), 0, 0, 0].repeat(n), vec![1.0, 0.0, 0.0, 0.0].repeat(n));
            let skin = match (&p.joints, &p.weights) {
                (Some(j), Some(w)) => Some((j, w)),
                _ if rig.is_some() => Some((&whole.0, &whole.1)),
                _ => None,
            };
            if let Some((j, w)) = skin.filter(|_| rig.is_some()) {
                ensure!(j.iter().all(|&b| (b as usize) < bone_count), "bone index past the rig's {bone_count}");
                let jb: Vec<u8> = j.iter().flat_map(|v| v.to_le_bytes()).collect();
                attributes["JOINTS_0"] = json!(g.accessor(&jb, UNSIGNED_SHORT, n, "VEC4", Some(ARRAY_BUFFER), None));
                attributes["WEIGHTS_0"] = json!(g.accessor_f32(w, "VEC4", 4, Some(ARRAY_BUFFER)));
            }
            let ib: Vec<u8> = p.indices.iter().flat_map(|i| i.to_le_bytes()).collect();
            let indices = g.accessor(&ib, UNSIGNED_INT, p.indices.len(), "SCALAR", Some(ELEMENT_ARRAY_BUFFER), None);

            let mut pbr = json!({ "baseColorFactor": [1.0, 1.0, 1.0, 1.0], "metallicFactor": 0.0, "roughnessFactor": 1.0 });
            if let Some(img) = p.texture.and_then(|id| by_id.get(&id).map(|img| (id, img))) {
                let texture = match images.get(&img.0) {
                    Some(&t) => t,
                    None => {
                        let mut png = Vec::new();
                        img.1.write_to(&mut Cursor::new(&mut png), image::ImageFormat::Png)?;
                        let view = g.view(&png, None);
                        g.images.push(json!({ "name": format!("{:08x}", img.0), "mimeType": "image/png", "bufferView": view }));
                        g.textures.push(json!({ "sampler": 0, "source": g.images.len() - 1 }));
                        let t = g.textures.len() - 1;
                        images.insert(img.0, t);
                        t
                    }
                };
                pbr["baseColorTexture"] = json!({ "index": texture, "texCoord": 0 });
            }
            let (mode, cutoff) = alpha_mode(p);
            let mut material = json!({
                "name": format!("{}-{}", o.name, primitives.len()),
                "alphaMode": mode,
                "doubleSided": p.double_sided,
                "pbrMetallicRoughness": pbr,
                "extras": { "nudFlags": p.flags, "outline": p.outline },
            });
            if let Some(c) = cutoff {
                material["alphaCutoff"] = json!(c);
            }
            g.materials.push(material);
            primitives.push(json!({ "attributes": attributes, "indices": indices, "material": g.materials.len() - 1 }));
        }
        if primitives.is_empty() {
            continue;
        }
        g.meshes.push(json!({ "name": o.name, "primitives": primitives }));
        let mut node = json!({ "name": o.name, "mesh": g.meshes.len() - 1 });
        if rig.is_some() {
            node["skin"] = json!(0);
        }
        g.nodes.push(node);
        nodes.push(g.nodes.len() - 1);
    }
    if nodes.is_empty() {
        bail!("no meshes");
    }
    if let (Some((bones, _)), Some((b, slots))) = (rig, clip) {
        crate::ani::clip(&mut g, bones, b, slots, "loop")?;
    }
    g.finish(nodes)
}

/// `don3d/<folder>` → `game_data/<era>/<folder>`: models (`.nud` + `.nut` → `.glb`) and textures (`.nut` → `.png`).
const MODELS: [(&str, &str); 4] = [("parts/body", "body"), ("parts/head", "head"), ("full/cos", "cos"), ("parts/acc", "acce")];
const TEXTURES: [(&str, &str); 2] = [("parts/paint", "paint"), ("full/face", "face")];

pub fn inputs() -> Vec<crate::ac15::Rule> {
    MODELS.iter().chain(&TEXTURES).map(|(d, _)| crate::ac15::Rule { path: format!("don3d/{d}/"), names: false }).collect()
}

pub fn convert(src: &crate::source::Source, out: &std::path::Path) -> Result<Vec<crate::Report>> {
    let mut reports = Vec::new();
    // Without the bind pose the models are written unskinned, in rest pose.
    let bones = crate::ani::rig();
    let bind = if src.exists(crate::ani::BIND) { Some(crate::ani::pose(&bones, crate::ani::frames(&src.read(crate::ani::BIND)?, crate::ani::DON_SLOTS)?[0])) } else { None };
    let rig = bind.as_ref().map(|b| (bones.as_slice(), b));
    for (dir, name, model) in MODELS.iter().map(|(d, n)| (d, n, true)).chain(TEXTURES.iter().map(|(d, n)| (d, n, false))) {
        let prefix = format!("don3d/{dir}/");
        let files: Vec<&str> = src.files(|p| p.strip_prefix(&prefix).is_some_and(|f| !f.contains('/') && f.ends_with(if model { ".nud" } else { ".nut" }))).collect();
        if files.is_empty() {
            continue;
        }
        let era = out.file_name().map_or(String::new(), |e| e.to_string_lossy().into_owned());
        let mut report = crate::Report::new(&format!("{era} {name} {}", if model { "models" } else { "textures" }));
        let dst_dir = out.join(name);
        std::fs::create_dir_all(&dst_dir)?;
        for rel in files {
            let stem = &rel[prefix.len()..rel.len() - 4];
            let dst = dst_dir.join(format!("{stem}.{}", if model { "glb" } else { "png" }));
            if crate::nut::is_fresh(&dst, src, rel) {
                report.up_to_date.push(stem.into());
                continue;
            }
            let result = (|| -> Result<()> {
                if model {
                    let nut = rel.replace(".nud", ".nut");
                    let textures = if src.exists(&nut) { crate::nut::decode(&src.read(&nut)?)? } else { Vec::new() };
                    let model = parse(&src.read(rel)?)?;
                    // A puchi has its own two-bone rig (the don rig's first two) and its idle loop beside it.
                    let (bind, looped) = (rel.replace(".nud", "_bind.bin"), rel.replace(".nud", "_loop.bin"));
                    let glb = if *dir == "parts/acc" && src.exists(&bind) {
                        let puchi: Vec<_> = crate::ani::rig().into_iter().take(2).collect();
                        let pose = crate::ani::pose(&puchi, crate::ani::frames(&src.read(&bind)?, crate::ani::PUCHI_SLOTS)?[0]);
                        let looped = if src.exists(&looped) { Some(src.read(&looped)?) } else { None };
                        glb(&model, &textures, Some((&puchi, &pose)), looped.as_deref().map(|b| (b, crate::ani::PUCHI_SLOTS)))?
                    } else {
                        glb(&model, &textures, rig, None)?
                    };
                    std::fs::write(&dst, glb)?;
                } else {
                    // A face holds its 12 expressions as 12 textures: stacked top to bottom, as Nijiiro's atlases are.
                    let images: Vec<_> = crate::nut::decode(&src.read(rel)?)?.into_iter().map(|t| t.image).collect();
                    let (w, h) = images.first().map(|i| i.dimensions()).context("no texture")?;
                    let mut atlas = image::RgbaImage::new(w, h * images.len() as u32);
                    for (k, img) in images.iter().enumerate() {
                        image::imageops::replace(&mut atlas, img, 0, (k as u32 * h) as i64);
                    }
                    atlas.save(&dst)?;
                }
                Ok(())
            })();
            match result {
                Ok(()) => report.copied.push(stem.into()),
                Err(e) => report.failed.push(format!("{stem}: {e:#}")),
            }
        }
        reports.push(report);
    }
    Ok(reports)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn alpha_mode_follows_the_material_not_the_texture() {
        let part = |blend, alpha_ref| Part { positions: vec![], normals: vec![], colors: None, uvs: vec![], joints: None, weights: None, indices: vec![], texture: None, double_sided: false, flags: 0x1000_0001, blend, alpha_ref, outline: true };
        assert_eq!(alpha_mode(&part(false, None)), ("OPAQUE", None), "the bodies' recolour pass: texture alpha is ignored");
        assert_eq!(alpha_mode(&part(false, Some(255))), ("MASK", Some(1.0)));
        assert_eq!(alpha_mode(&part(true, Some(255))), ("BLEND", None));
    }

    #[test]
    fn half_floats() {
        assert_eq!(half(0x3C00), 1.0);
        assert_eq!(half(0xC000), -2.0);
        assert_eq!(half(0x3800), 0.5);
        assert_eq!(half(0), 0.0);
    }
}
