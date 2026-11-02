//! The older games' don-chan rig and animations (`don3d/ani/*.bin`, reverse-engineered; see the README).
//!
//! A file is a big-endian f32 frame count, then that many frames of 98 vec3s (1,176 bytes). Every model shares one
//! 39-bone rig, so the slots are fixed: bone 0 (translation, rotation), bone 1 the body (translation, rotation,
//! scale), four limbs of nine bones from slot 5 (23 slots each), and slot 97, whose x is the face expression.
//! Rotations are Euler angles in radians. `don_bind.bin` is the bind pose (its first frame).

use std::path::Path;

use anyhow::{ensure, Result};
use glam::{EulerRot, Mat4, Quat, Vec3};
use serde_json::{json, Map, Value};

use crate::models::{Gltf, FLOAT};

pub const BIND: &str = "don3d/ani/don_bind.bin";
/// Vec3 slots per frame: the don rig's, and a puchi's (`parts/acc/*_{bind,loop}.bin`: the first two bones only).
pub const DON_SLOTS: usize = 98;
pub const PUCHI_SLOTS: usize = 5;
const FPS: f32 = 60.0;
/// Expressions in a face atlas (`full/face/*.nut` holds 12 textures).
const FACE_ROWS: u32 = 12;

pub struct Bone {
    pub name: String,
    pub parent: Option<usize>,
    /// The bone's translation, rotation and scale slots; a bone without one keeps identity there.
    slots: [Option<usize>; 3],
}

/// The rig, in the bone-index order NUD vertices use. In a limb, bones 1–5 chain along +x from the limb root (whose
/// translation is in model space, so it hangs off bone 0), bone 6 copies bone 5, bone 7 copies the root and bone 8
/// sits at the chain's length along bone 7's x: the IK target. Bone 38 has no slots (slot 97 is the face).
pub fn rig() -> Vec<Bone> {
    let bone = |name: String, parent: Option<usize>, t: usize, r: Option<usize>, s: Option<usize>| Bone { name, parent, slots: [Some(t), r, s] };
    let mut bones = vec![bone("root".into(), None, 0, Some(1), None), bone("body".into(), Some(0), 2, Some(3), Some(4))];
    for limb in 0..4 {
        let (s, b) = (5 + limb * 23, 2 + limb * 9);
        let name = |k: usize| format!("limb{}_{k}", limb + 1);
        bones.push(bone(name(0), Some(0), s, Some(s + 1), Some(s + 2)));
        for k in 1..6 {
            bones.push(bone(name(k), Some(b + k - 1), s + k * 3, Some(s + k * 3 + 1), Some(s + k * 3 + 2)));
        }
        bones.push(bone(name(6), Some(b + 4), s + 18, Some(s + 19), None));
        bones.push(bone(name(7), Some(0), s + 20, Some(s + 21), None));
        bones.push(bone(name(8), Some(b + 7), s + 22, None, None));
    }
    bones.push(Bone { name: "face".into(), parent: Some(1), slots: [None; 3] });
    bones
}

pub type Pose = Vec<(Vec3, Quat, Vec3)>;

/// The frames of an animation file.
pub fn frames(b: &[u8], slots: usize) -> Result<Vec<&[u8]>> {
    let frame = slots * 12;
    ensure!(b.len() >= 4 && (b.len() - 4) % frame == 0, "not a don animation ({} bytes)", b.len());
    let n = (b.len() - 4) / frame;
    ensure!(f32::from_be_bytes(b[..4].try_into()?) as usize == n, "frame count doesn't match the size");
    Ok(b[4..].chunks_exact(frame).collect())
}

fn slot(frame: &[u8], s: usize) -> Vec3 {
    let f = |c: usize| f32::from_be_bytes(frame[(s * 3 + c) * 4..(s * 3 + c) * 4 + 4].try_into().unwrap());
    Vec3::new(f(0), f(1), f(2))
}

/// Each bone's local translation, rotation (Z·Y·X, as Smash Forge reads VBN Euler angles) and scale in `frame`.
pub fn pose(rig: &[Bone], frame: &[u8]) -> Pose {
    rig.iter()
        .map(|b| {
            let [t, r, s] = b.slots;
            let r = r.map(|i| slot(frame, i)).map_or(Quat::IDENTITY, |e| Quat::from_euler(EulerRot::ZYX, e.z, e.y, e.x));
            (t.map_or(Vec3::ZERO, |i| slot(frame, i)), r, s.map_or(Vec3::ONE, |i| slot(frame, i)))
        })
        .collect()
}

fn face(frame: &[u8]) -> u32 {
    slot(frame, 97).x.round().max(0.0) as u32
}

/// Adds the rig as nodes `0..rig.len()` in `pose`, and returns the root's index.
pub fn push_bones(g: &mut Gltf, rig: &[Bone], pose: &Pose) -> usize {
    for (i, (b, (t, r, s))) in rig.iter().zip(pose).enumerate() {
        let children: Vec<usize> = (0..rig.len()).filter(|&c| rig[c].parent == Some(i)).collect();
        let mut node = json!({ "name": b.name, "translation": t.to_array(), "rotation": r.to_array(), "scale": s.to_array() });
        if !children.is_empty() {
            node["children"] = json!(children);
        }
        g.nodes.push(node);
    }
    0
}

/// Model-space bone matrices.
pub fn world(rig: &[Bone], pose: &Pose) -> Vec<Mat4> {
    let mut out: Vec<Mat4> = Vec::with_capacity(rig.len());
    for (b, &(t, r, s)) in rig.iter().zip(pose) {
        let local = Mat4::from_scale_rotation_translation(s, r, t);
        out.push(b.parent.map_or(local, |p| out[p] * local));
    }
    out
}

/// `game_data/<era>/animation/don.glb` (the rig and one clip per file, sampled per frame at 60 fps, LINEAR) and
/// `facekeys.json` (the face expression over time), in the shapes Nijiiro's `animation/` uses.
pub fn convert(src: &crate::source::Source, out: &Path, report: &mut crate::Report) -> Result<()> {
    let files: Vec<&str> = src.files(|p| p.strip_prefix("don3d/ani/").is_some_and(|f| !f.contains('/') && f.ends_with(".bin"))).collect();
    if !src.exists(BIND) {
        return Ok(());
    }
    let (glb_path, keys_path) = (out.join("don.glb"), out.join("facekeys.json"));
    if files.iter().all(|f| crate::nut::is_fresh(&glb_path, src, f) && crate::nut::is_fresh(&keys_path, src, f)) {
        report.up_to_date.push(format!("don.glb ({} clips)", files.len() - 1));
        return Ok(());
    }
    let rig = rig();
    let mut g = Gltf::default();
    let root = push_bones(&mut g, &rig, &pose(&rig, frames(&src.read(BIND)?, DON_SLOTS)?[0]));
    let mut faces = Map::new();
    for rel in files.into_iter().filter(|&f| f != BIND) {
        let name = rel.trim_start_matches("don3d/ani/").trim_end_matches(".bin").to_string();
        match src.read(rel).and_then(|b| clip(&mut g, &rig, &b, DON_SLOTS, &name)) {
            Ok(keys) => {
                faces.insert(name.clone(), keys);
                report.copied.push(name);
            }
            Err(e) => report.failed.push(format!("{name}: {e:#}")),
        }
    }
    std::fs::create_dir_all(out)?;
    std::fs::write(&glb_path, g.finish(vec![root])?)?;
    std::fs::write(&keys_path, serde_json::to_string_pretty(&json!({ "animations": faces }))?)?;
    Ok(())
}

/// Adds `b` as the animation `name`, and returns its face keys.
pub fn clip(g: &mut Gltf, rig: &[Bone], b: &[u8], slots: usize, name: &str) -> Result<Value> {
    let frames = frames(b, slots)?;
    let poses: Vec<Pose> = frames.iter().map(|f| pose(rig, f)).collect();
    let times: Vec<f32> = (0..frames.len()).map(|f| f as f32 / FPS).collect();
    let tb: Vec<u8> = times.iter().flat_map(|t| t.to_le_bytes()).collect();
    let input = g.accessor(&tb, FLOAT, times.len(), "SCALAR", None, Some((vec![times[0]], vec![*times.last().unwrap()])));
    let (mut samplers, mut channels) = (Vec::new(), Vec::new());
    for (i, bone) in rig.iter().enumerate() {
        let tracks: [(&str, &str, Vec<f32>, bool); 3] = [
            ("translation", "VEC3", poses.iter().flat_map(|p| p[i].0.to_array()).collect(), bone.slots[0].is_some()),
            ("rotation", "VEC4", poses.iter().flat_map(|p| p[i].1.to_array()).collect(), bone.slots[1].is_some()),
            ("scale", "VEC3", poses.iter().flat_map(|p| p[i].2.to_array()).collect(), bone.slots[2].is_some()),
        ];
        for (path, kind, values, present) in tracks {
            if !present {
                continue;
            }
            let bytes: Vec<u8> = values.iter().flat_map(|v| v.to_le_bytes()).collect();
            let output = g.accessor(&bytes, FLOAT, frames.len(), kind, None, None);
            channels.push(json!({ "sampler": samplers.len(), "target": { "node": i, "path": path } }));
            samplers.push(json!({ "input": input, "output": output, "interpolation": "LINEAR" }));
        }
    }
    g.animations.push(json!({ "name": name, "samplers": samplers, "channels": channels }));
    let (mut keys, mut last) = (Vec::new(), None);
    for (f, frame) in frames.iter().enumerate() {
        let face = if slots > 97 { face(frame) } else { 0 };
        if last != Some(face) {
            keys.push(json!({ "time": f as f32 / FPS, "faceIndex": face }));
            last = Some(face);
        }
    }
    Ok(json!({ "duration": (frames.len() - 1) as f32 / FPS, "frameCount": FACE_ROWS, "keys": keys }))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn rig_matches_the_frame_layout() {
        let rig = rig();
        assert_eq!(rig.len(), 39, "NUD vertices use bones 0..=38");
        let used: Vec<usize> = rig.iter().flat_map(|b| b.slots.iter().flatten().copied()).collect();
        let mut sorted = used.clone();
        sorted.sort();
        assert_eq!(sorted, (0..97).collect::<Vec<_>>(), "every slot but the face's is one bone's, once");
        assert!(rig.iter().enumerate().all(|(i, b)| b.parent.is_none_or(|p| p < i)), "parents come first");
    }
}
