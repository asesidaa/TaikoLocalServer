//! Converts Nijiiro don-chan models (MODL + MESH + SKEL + MATL) to binary glTF.
//!
//! The layout mirrors what `don-chan-previewer`'s `normalize.ts` was written against:
//! one node per bone (skeleton order), one skinned mesh node per MESH object named
//! `<object>-<subindex>`, one material per MATL label (name kept verbatim, since flags are
//! parsed out of it), PNG base textures named after their material, and `extras.shaderType`
//! taken from the `__nust__` MATL.

use std::collections::HashMap;
use std::fs;
use std::io::Cursor;
use std::path::{Path, PathBuf};

use anyhow::{bail, Context, Result};
use glam::{Mat4, Quat, Vec3};
use serde_json::{json, Value};
use ssbh_data::matl_data::ParamId;
use ssbh_data::mesh_data::{MeshObjectData, VectorData};
use ssbh_data::prelude::*;

const ARRAY_BUFFER: u32 = 34962;
const ELEMENT_ARRAY_BUFFER: u32 = 34963;
const FLOAT: u32 = 5126;
const UNSIGNED_SHORT: u32 = 5123;
const UNSIGNED_INT: u32 = 5125;

/// Converts every model under `model/<category>` to `<dst_dir>/<name>.glb`.
/// A model folder normally holds one `<name>/<name>.numdlb`, but can hold variants (e.g. `head_102001`).
pub fn convert_category(src_dir: &Path, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    let Ok(entries) = fs::read_dir(src_dir) else {
        report.missing.push(src_dir.display().to_string());
        return Ok(());
    };
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;

    let mut model_dirs: Vec<PathBuf> = entries.filter_map(|e| e.ok().map(|e| e.path())).filter(|p| p.is_dir()).collect();
    model_dirs.sort();
    for model_dir in model_dirs {
        let mut subs: Vec<PathBuf> = fs::read_dir(&model_dir)?.filter_map(|e| e.ok().map(|e| e.path())).collect();
        subs.sort();
        for sub in subs {
            let name = sub.file_name().unwrap().to_string_lossy().into_owned();
            let modl_path = sub.join(format!("{name}.numdlb"));
            if !modl_path.is_file() {
                continue;
            }
            let dst = dst_dir.join(format!("{name}.glb"));
            // shortcut: freshness only checks the .numdlb, not its textures; delete the output to force a rebuild.
            if crate::is_newer_than(&dst, &modl_path)? {
                report.up_to_date.push(name);
                continue;
            }
            match build_glb(&sub, &name).and_then(|glb| fs::write(&dst, glb).with_context(|| format!("writing {}", dst.display()))) {
                Ok(()) => report.copied.push(name),
                Err(e) => report.failed.push(format!("{name}: {e:#}")),
            }
        }
    }
    Ok(())
}

#[derive(Default)]
struct MaterialSource {
    texture: Option<PathBuf>,
    shader: Option<String>,
}

/// Builds a `.glb` for the model whose `<name>.numdlb` lives in `dir`.
pub fn build_glb(dir: &Path, name: &str) -> Result<Vec<u8>> {
    let read_err = |what: &str| format!("reading {what} for {name}");
    let modl = ModlData::from_file(dir.join(format!("{name}.numdlb"))).map_err(|e| anyhow::anyhow!("{e}")).with_context(|| read_err("MODL"))?;
    let mesh = MeshData::from_file(dir.join(&modl.mesh_file_name)).map_err(|e| anyhow::anyhow!("{e}")).with_context(|| read_err("MESH"))?;
    let skel = SkelData::from_file(dir.join(&modl.skeleton_file_name)).map_err(|e| anyhow::anyhow!("{e}")).with_context(|| read_err("SKEL"))?;

    // The maya MATL carries the DiffuseMap paths, the nust MATL carries the shader labels.
    let mut sources: HashMap<String, MaterialSource> = HashMap::new();
    for file in &modl.material_file_names {
        let path = dir.join(file.trim_start_matches("./"));
        let matl = MatlData::from_file(&path).map_err(|e| anyhow::anyhow!("{e}")).with_context(|| format!("reading {}", path.display()))?;
        let base = path.parent().unwrap();
        for entry in matl.entries {
            let src = sources.entry(entry.material_label).or_default();
            if !entry.shader_label.is_empty() {
                src.shader = Some(entry.shader_label);
            }
            if let Some(t) = entry.textures.iter().find(|t| t.param_id == ParamId::DiffuseMap) {
                src.texture = Some(base.join(format!("{}.nutexb", t.data)));
            }
        }
    }

    let mut g = Gltf::default();

    // Bones: node i == bone i, so skin joints are simply 0..n.
    let bone_index: HashMap<&str, usize> = skel.bones.iter().enumerate().map(|(i, b)| (b.name.as_str(), i)).collect();
    let mut children: Vec<Vec<usize>> = vec![Vec::new(); skel.bones.len()];
    let mut roots = Vec::new();
    for (i, bone) in skel.bones.iter().enumerate() {
        match bone.parent_index {
            Some(p) => children[p].push(i),
            None => roots.push(i),
        }
    }
    let mut inverse_binds = Vec::with_capacity(skel.bones.len() * 16);
    for (i, bone) in skel.bones.iter().enumerate() {
        let (s, r, t) = decompose(bone.transform);
        let mut node = json!({ "name": bone.name, "translation": t.to_array(), "rotation": r.to_array(), "scale": s.to_array() });
        if !children[i].is_empty() {
            node["children"] = json!(children[i]);
        }
        g.nodes.push(node);
        let world: Mat4 = skel.calculate_world_transform(bone).map_err(|e| anyhow::anyhow!("{e:?}"))?;
        // Zero-scale (hidden) bones aren't invertible; identity matches numd-exporter.
        let mut ibm = Some(world.inverse()).filter(|m| m.is_finite()).unwrap_or(Mat4::IDENTITY).to_cols_array();
        // glTF requires the bottom row to be exactly 0,0,0,1; f32 inversion leaves it at 0.9999999.
        (ibm[3], ibm[7], ibm[11], ibm[15]) = (0.0, 0.0, 0.0, 1.0);
        inverse_binds.extend(ibm);
    }
    let has_skin = !skel.bones.is_empty();
    if has_skin {
        let ibm = g.accessor_f32(&inverse_binds, "MAT4", 16, None);
        g.skins.push(json!({ "joints": (0..skel.bones.len()).collect::<Vec<_>>(), "inverseBindMatrices": ibm }));
    }

    let mut material_index: HashMap<String, usize> = HashMap::new();
    let mut texture_view: HashMap<PathBuf, (usize, bool)> = HashMap::new();
    for object in &mesh.objects {
        let label = modl
            .entries
            .iter()
            .find(|e| e.mesh_object_name == object.name && e.mesh_object_subindex == object.subindex)
            .map(|e| e.material_label.clone());
        let material = match &label {
            Some(label) => Some(match material_index.get(label) {
                Some(&i) => i,
                None => {
                    let i = g.material(label, sources.get(label), &mut texture_view)?;
                    material_index.insert(label.clone(), i);
                    i
                }
            }),
            None => None,
        };

        let skinned = has_skin && !object.bone_influences.is_empty();
        let mesh_index = g.mesh(object, material, skinned.then_some(&bone_index))?;
        let node_index = g.nodes.len();
        let mut node = json!({ "name": format!("{}-{}", object.name, object.subindex), "mesh": mesh_index });
        if skinned {
            node["skin"] = json!(0);
            roots.push(node_index);
        } else if let Some(&parent) = bone_index.get(object.parent_bone_name.as_str()) {
            // Rigid mesh: ride along with its parent bone.
            children_push(&mut g.nodes[parent], node_index);
        } else {
            roots.push(node_index);
        }
        g.nodes.push(node);
    }

    g.finish(roots)
}

/// TRS from a bone matrix. Some costumes (e.g. `head_003000`, no arms or legs) hide bones with a
/// zero-scale basis; glam then returns a NaN rotation. Rotation is meaningless at zero scale, so use identity.
fn decompose(m: Mat4) -> (Vec3, Quat, Vec3) {
    let (s, r, t) = m.to_scale_rotation_translation();
    if r.is_finite() {
        return (s, r, t);
    }
    let scale = Vec3::new(m.x_axis.truncate().length(), m.y_axis.truncate().length(), m.z_axis.truncate().length());
    (scale, Quat::IDENTITY, t)
}

fn children_push(node: &mut Value, child: usize) {
    match node.get_mut("children").and_then(Value::as_array_mut) {
        Some(c) => c.push(json!(child)),
        None => node["children"] = json!([child]),
    }
}

fn vec2s(v: &VectorData) -> Vec<f32> {
    match v {
        VectorData::Vector2(x) => x.iter().flat_map(|v| v.to_array()).collect(),
        VectorData::Vector3(x) => x.iter().flat_map(|v| [v.x, v.y]).collect(),
        VectorData::Vector4(x) => x.iter().flat_map(|v| [v.x, v.y]).collect(),
    }
}

fn vec3s(v: &VectorData) -> Vec<f32> {
    match v {
        VectorData::Vector2(x) => x.iter().flat_map(|v| [v.x, v.y, 0.0]).collect(),
        VectorData::Vector3(x) => x.iter().flat_map(|v| v.to_array()).collect(),
        VectorData::Vector4(x) => x.iter().flat_map(|v| [v.x, v.y, v.z]).collect(),
    }
}

fn vec4s(v: &VectorData) -> Vec<f32> {
    match v {
        VectorData::Vector2(x) => x.iter().flat_map(|v| [v.x, v.y, 0.0, 1.0]).collect(),
        VectorData::Vector3(x) => x.iter().flat_map(|v| [v.x, v.y, v.z, 1.0]).collect(),
        VectorData::Vector4(x) => x.iter().flat_map(|v| v.to_array()).collect(),
    }
}

/// Per-vertex top-4 joint influences, renormalized to sum to 1 as glTF requires.
fn skin_attributes(object: &MeshObjectData, vertex_count: usize, bone_index: &HashMap<&str, usize>) -> Result<(Vec<u16>, Vec<f32>)> {
    let mut per_vertex: Vec<Vec<(u16, f32)>> = vec![Vec::new(); vertex_count];
    for influence in &object.bone_influences {
        let Some(&joint) = bone_index.get(influence.bone_name.as_str()) else {
            bail!("{} weights unknown bone {}", object.name, influence.bone_name);
        };
        for w in &influence.vertex_weights {
            if let Some(v) = per_vertex.get_mut(w.vertex_index as usize) {
                v.push((joint as u16, w.vertex_weight));
            }
        }
    }
    let mut joints = Vec::with_capacity(vertex_count * 4);
    let mut weights = Vec::with_capacity(vertex_count * 4);
    for mut v in per_vertex {
        v.sort_by(|a, b| b.1.total_cmp(&a.1));
        v.truncate(4);
        let sum: f32 = v.iter().map(|x| x.1).sum();
        for slot in 0..4 {
            let (j, w) = v.get(slot).copied().unwrap_or((0, 0.0));
            joints.push(j);
            weights.push(if sum > 0.0 { w / sum } else if slot == 0 { 1.0 } else { 0.0 });
        }
    }
    Ok((joints, weights))
}

#[derive(Default)]
struct Gltf {
    bin: Vec<u8>,
    views: Vec<Value>,
    accessors: Vec<Value>,
    nodes: Vec<Value>,
    meshes: Vec<Value>,
    skins: Vec<Value>,
    materials: Vec<Value>,
    textures: Vec<Value>,
    images: Vec<Value>,
}

impl Gltf {
    fn view(&mut self, bytes: &[u8], target: Option<u32>) -> usize {
        while !self.bin.len().is_multiple_of(4) {
            self.bin.push(0);
        }
        let mut v = json!({ "buffer": 0, "byteOffset": self.bin.len(), "byteLength": bytes.len() });
        if let Some(t) = target {
            v["target"] = json!(t);
        }
        self.bin.extend_from_slice(bytes);
        self.views.push(v);
        self.views.len() - 1
    }

    fn accessor(&mut self, bytes: &[u8], component: u32, count: usize, kind: &str, target: Option<u32>, min_max: Option<(Vec<f32>, Vec<f32>)>) -> usize {
        let view = self.view(bytes, target);
        let mut a = json!({ "bufferView": view, "componentType": component, "count": count, "type": kind });
        if let Some((min, max)) = min_max {
            a["min"] = json!(min);
            a["max"] = json!(max);
        }
        self.accessors.push(a);
        self.accessors.len() - 1
    }

    fn accessor_f32(&mut self, values: &[f32], kind: &str, width: usize, target: Option<u32>) -> usize {
        let bytes: Vec<u8> = values.iter().flat_map(|f| f.to_le_bytes()).collect();
        self.accessor(&bytes, FLOAT, values.len() / width, kind, target, None)
    }

    fn mesh(&mut self, object: &MeshObjectData, material: Option<usize>, skin: Option<&HashMap<&str, usize>>) -> Result<usize> {
        let positions = vec3s(&object.positions.first().context("object has no positions")?.data);
        let count = positions.len() / 3;
        let (mut min, mut max) = (vec![f32::MAX; 3], vec![f32::MIN; 3]);
        for p in positions.chunks_exact(3) {
            for c in 0..3 {
                min[c] = min[c].min(p[c]);
                max[c] = max[c].max(p[c]);
            }
        }
        let pos_bytes: Vec<u8> = positions.iter().flat_map(|f| f.to_le_bytes()).collect();
        let mut attributes = json!({ "POSITION": self.accessor(&pos_bytes, FLOAT, count, "VEC3", Some(ARRAY_BUFFER), Some((min, max))) });
        if let Some(n) = object.normals.first() {
            attributes["NORMAL"] = json!(self.accessor_f32(&vec3s(&n.data), "VEC3", 3, Some(ARRAY_BUFFER)));
        }
        // Most objects carry Color1, a few Color0; the previewer reads whichever comes first as COLOR_0.
        if let Some(c) = object.color_sets.first() {
            attributes["COLOR_0"] = json!(self.accessor_f32(&vec4s(&c.data), "VEC4", 4, Some(ARRAY_BUFFER)));
        }
        if let Some(uv) = object.texture_coordinates.first() {
            attributes["TEXCOORD_0"] = json!(self.accessor_f32(&vec2s(&uv.data), "VEC2", 2, Some(ARRAY_BUFFER)));
        }
        if let Some(bone_index) = skin {
            let (joints, weights) = skin_attributes(object, count, bone_index)?;
            let joint_bytes: Vec<u8> = joints.iter().flat_map(|j| j.to_le_bytes()).collect();
            attributes["JOINTS_0"] = json!(self.accessor(&joint_bytes, UNSIGNED_SHORT, count, "VEC4", Some(ARRAY_BUFFER), None));
            attributes["WEIGHTS_0"] = json!(self.accessor_f32(&weights, "VEC4", 4, Some(ARRAY_BUFFER)));
        }
        let index_bytes: Vec<u8> = object.vertex_indices.iter().flat_map(|i| i.to_le_bytes()).collect();
        let indices = self.accessor(&index_bytes, UNSIGNED_INT, object.vertex_indices.len(), "SCALAR", Some(ELEMENT_ARRAY_BUFFER), None);

        let mut primitive = json!({ "attributes": attributes, "indices": indices });
        if let Some(m) = material {
            primitive["material"] = json!(m);
        }
        self.meshes.push(json!({ "name": format!("{}-{}", object.name, object.subindex), "primitives": [primitive] }));
        Ok(self.meshes.len() - 1)
    }

    /// One material per MATL label, with its own image (named after the material, which `normalize.ts`
    /// reads); images of the same source texture share one PNG buffer view.
    /// `alphaMode` is BLEND exactly when the texture has any non-opaque pixel, matching `numd-exporter`
    /// (verified against all 2,152 materials in the previewer's GLBs).
    fn material(&mut self, label: &str, source: Option<&MaterialSource>, texture_view: &mut HashMap<PathBuf, (usize, bool)>) -> Result<usize> {
        let mut pbr = json!({ "baseColorFactor": [1.0, 1.0, 1.0, 1.0], "metallicFactor": 0.0, "roughnessFactor": 1.0 });
        let mut blend = false;
        if let Some(path) = source.and_then(|s| s.texture.as_ref()).filter(|p| p.is_file()) {
            let (view, has_alpha) = match texture_view.get(path) {
                Some(&v) => v,
                None => {
                    let img = crate::textures::decode(path)?;
                    let has_alpha = img.pixels().any(|p| p[3] < 255);
                    let mut png = Vec::new();
                    img.write_to(&mut Cursor::new(&mut png), image::ImageFormat::Png)?;
                    let v = (self.view(&png, None), has_alpha);
                    texture_view.insert(path.clone(), v);
                    v
                }
            };
            blend = has_alpha;
            self.images.push(json!({ "name": label, "mimeType": "image/png", "bufferView": view }));
            self.textures.push(json!({ "sampler": 0, "source": self.images.len() - 1 }));
            pbr["baseColorTexture"] = json!({ "index": self.textures.len() - 1, "texCoord": 0 });
        }
        let mut m = json!({
            "name": label,
            "alphaMode": if blend { "BLEND" } else { "OPAQUE" },
            "doubleSided": false,
            "pbrMetallicRoughness": pbr,
            "emissiveFactor": [0.0, 0.0, 0.0],
        });
        if let Some(shader) = source.and_then(|s| s.shader.as_ref()) {
            m["extras"] = json!({ "shaderType": shader });
        }
        self.materials.push(m);
        Ok(self.materials.len() - 1)
    }

    fn finish(mut self, scene_nodes: Vec<usize>) -> Result<Vec<u8>> {
        while !self.bin.len().is_multiple_of(4) {
            self.bin.push(0);
        }
        let mut doc = json!({
            "asset": { "version": "2.0", "generator": "TaikoLocalServer tools/extract" },
            "scene": 0,
            "scenes": [{ "nodes": scene_nodes }],
            "nodes": self.nodes,
            "meshes": self.meshes,
            "materials": self.materials,
            "accessors": self.accessors,
            "bufferViews": self.views,
            "buffers": [{ "byteLength": self.bin.len() }],
            "samplers": [{ "magFilter": 9729, "minFilter": 9729, "wrapS": 10497, "wrapT": 10497 }],
        });
        for (key, list) in [("skins", self.skins), ("textures", self.textures), ("images", self.images)] {
            if !list.is_empty() {
                doc[key] = json!(list);
            }
        }
        let mut json_bytes = serde_json::to_vec(&doc)?;
        while !json_bytes.len().is_multiple_of(4) {
            json_bytes.push(b' ');
        }

        let total = 12 + 8 + json_bytes.len() + 8 + self.bin.len();
        let mut out = Vec::with_capacity(total);
        out.extend(b"glTF");
        out.extend(2u32.to_le_bytes());
        out.extend((total as u32).to_le_bytes());
        out.extend((json_bytes.len() as u32).to_le_bytes());
        out.extend(b"JSON");
        out.extend(json_bytes);
        out.extend((self.bin.len() as u32).to_le_bytes());
        out.extend(b"BIN\0");
        out.extend(self.bin);
        Ok(out)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use ssbh_data::mesh_data::{BoneInfluence, VertexWeight};

    #[test]
    fn zero_scale_bone_decomposes_to_identity_rotation() {
        let hidden = Mat4::from_cols_array(&[0.0; 12].into_iter().chain([0.1, 0.2, 0.3, 1.0]).collect::<Vec<_>>().try_into().unwrap());
        let (s, r, t) = decompose(hidden);
        assert_eq!(s, Vec3::ZERO);
        assert_eq!(r, Quat::IDENTITY);
        assert_eq!(t, Vec3::new(0.1, 0.2, 0.3));
    }

    #[test]
    fn skin_keeps_top_four_influences_and_normalizes() {
        let weight = |w| vec![VertexWeight { vertex_index: 0, vertex_weight: w }];
        let object = MeshObjectData {
            name: "obj".into(),
            bone_influences: ["a", "b", "c", "d", "e"]
                .iter()
                .zip([0.1, 0.4, 0.2, 0.2, 0.1])
                .map(|(b, w)| BoneInfluence { bone_name: b.to_string(), vertex_weights: weight(w) })
                .collect(),
            ..Default::default()
        };
        let bones: HashMap<&str, usize> = ["a", "b", "c", "d", "e"].iter().enumerate().map(|(i, b)| (*b, i)).collect();

        let (joints, weights) = skin_attributes(&object, 1, &bones).unwrap();

        assert_eq!(joints[0], 1); // heaviest first
        assert!(!joints.contains(&4) || !joints.contains(&0)); // one of the two 0.1s was dropped
        assert!((weights.iter().sum::<f32>() - 1.0).abs() < 1e-6);
    }

    #[test]
    fn unknown_bone_in_weights_is_an_error() {
        let object = MeshObjectData {
            bone_influences: vec![BoneInfluence { bone_name: "missing".into(), vertex_weights: vec![] }],
            ..Default::default()
        };
        assert!(skin_attributes(&object, 1, &HashMap::new()).is_err());
    }
}
