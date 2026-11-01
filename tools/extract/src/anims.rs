//! Converts the game's `animation/*.nuanmb` (SSBH ANIM v1.2) into one animation-library GLB that
//! `don-chan-previewer` loads, plus `facekeys.json` with the face-expression changes.
//! Ported from `don-chan-previewer/scripts/nuanmb_to_glb`; the rig now comes from the game's own
//! `.nusktb` instead of a pre-made skeleton GLB.

use std::collections::{BTreeMap, HashMap};
use std::fs;
use std::path::{Path, PathBuf};

use anyhow::{bail, Context, Result};
use glam::{Quat, Vec3};
use serde_json::{json, Map, Value};
use ssbh_lib::formats::anim::{Anim, TrackTypeV1};
use ssbh_data::prelude::*;

use crate::models::{decompose, push_bones, Gltf, FLOAT};
use crate::v12::curve::Curve;

/// Output names under `game_data/animation/`. The standalone previewer calls the library `Don_3D_Nij.glb`;
/// the WebUI embed points at `don.glb` instead.
pub const LIBRARY: &str = "don.glb";
pub const FACE_KEYS: &str = "facekeys.json";

/// The game animates at 60 fps; ANIM v1.2 keys are frame indices.
const FPS: f32 = 60.0;

/// Any model's skeleton works (they share the don rig); the default body is always present.
const RIG_MODEL: &str = "body_000000";

pub fn convert(game: &crate::game::Game, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    let rig = game.file(format!("model/body/{RIG_MODEL}/{RIG_MODEL}/{RIG_MODEL}.numdlb"));
    let rig_dir = rig.parent().unwrap().to_path_buf();
    let Some(entries) = game.list("animation") else {
        report.missing.push(game.file("animation").display().to_string());
        return Ok(());
    };
    if !rig.is_file() {
        report.missing.push(rig.display().to_string());
        return Ok(());
    }
    let paths: Vec<PathBuf> = entries.into_iter().filter(|p| p.extension().is_some_and(|e| e == "nuanmb")).collect();

    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;
    let (glb_path, keys_path) = (dst_dir.join(LIBRARY), dst_dir.join(FACE_KEYS));
    let mut current = true;
    for src in paths.iter().chain([&rig]) {
        current &= crate::is_newer_than(&glb_path, src)? && crate::is_newer_than(&keys_path, src)?;
    }
    if current {
        report.up_to_date.push(format!("{LIBRARY} ({} clips)", paths.len()));
        return Ok(());
    }

    let modl = ModlData::from_file(&rig).map_err(|e| anyhow::anyhow!("{e}")).context("reading rig MODL")?;
    let skel = SkelData::from_file(rig_dir.join(&modl.skeleton_file_name)).map_err(|e| anyhow::anyhow!("{e}")).context("reading rig SKEL")?;

    let mut g = Gltf::default();
    let roots = push_bones(&mut g, &skel);
    let rig = Rig {
        index: skel.bones.iter().enumerate().map(|(i, b)| (b.name.clone(), i)).collect(),
        parent: skel.bones.iter().map(|b| b.parent_index).collect(),
        rest_scale: skel.bones.iter().map(|b| decompose(b.transform).0).collect(),
    };

    let mut face_keys = Map::new();
    for path in &paths {
        let name = path.file_stem().unwrap().to_string_lossy().into_owned();
        match clip(&mut g, &rig, path, &name) {
            Ok((channels, keys)) => {
                if let Some(keys) = keys {
                    face_keys.insert(name.clone(), keys);
                }
                if let Some(animation) = channels {
                    g.animations.push(animation);
                }
                report.copied.push(name);
            }
            Err(e) => report.failed.push(format!("{name}: {e:#}")),
        }
    }

    fs::write(&glb_path, g.finish(roots)?).with_context(|| format!("writing {}", glb_path.display()))?;
    fs::write(&keys_path, serde_json::to_string_pretty(&json!({ "animations": face_keys }))?)
        .with_context(|| format!("writing {}", keys_path.display()))?;
    Ok(())
}

struct Rig {
    index: HashMap<String, usize>,
    parent: Vec<Option<usize>>,
    rest_scale: Vec<Vec3>,
}

struct NodeAnim {
    t: Vec<Vec3>,
    r: Vec<Quat>,
    s: Vec<Vec3>,
    compensate_scale: bool,
}

/// Decodes one `.nuanmb` into a glTF animation (sampled once per frame, LINEAR) and its face keys.
fn clip(g: &mut Gltf, rig: &Rig, path: &Path, name: &str) -> Result<(Option<Value>, Option<Value>)> {
    let anim = Anim::from_file(path).with_context(|| format!("reading {}", path.display()))?;
    let Anim::V12 { unk2: last_frame, tracks, buffers, .. } = anim else {
        bail!("not an ANIM v1.2 file");
    };
    // v1.2 final_frame_index is always 60; unk2 is the real last frame (keys run 0..=unk2).
    let frame_count = last_frame.round() as usize + 1;

    let mut nodes: BTreeMap<usize, NodeAnim> = BTreeMap::new();
    let mut face_keys = None;
    for track in tracks.elements.iter().filter(|t| t.track_type == TrackTypeV1::Transform) {
        let bone = track.name.to_string_lossy();
        let (mut scale, mut rotate, mut translate, mut compensate_scale) = (None, None, None, false);
        for p in track.properties.elements.iter() {
            let Some(buf) = buffers.elements.get(p.buffer_index as usize) else { continue };
            let property = p.name.to_string_lossy();
            let slot = match property.as_str() {
                "Scale" => &mut scale,
                "Rotate" => &mut rotate,
                "Translate" => &mut translate,
                "CompensateScale" => {
                    compensate_scale = Curve::decode(&buf.elements).is_ok_and(|c| c.sample(0.0)[0] != 0.0);
                    continue;
                }
                // Expression row in the 12-row face atlas, held until the next key.
                "nfCustomInteger0" => {
                    let curve = Curve::decode(&buf.elements).with_context(|| format!("{bone}.{property}"))?;
                    let (mut keys, mut last) = (Vec::new(), None);
                    for f in 0..frame_count {
                        let face = curve.sample(f as f32)[0].round().max(0.0) as u32;
                        if last != Some(face) {
                            keys.push(json!({ "time": f as f32 / FPS, "faceIndex": face }));
                            last = Some(face);
                        }
                    }
                    face_keys = Some(json!({ "duration": (frame_count - 1) as f32 / FPS, "frameCount": 12, "keys": keys }));
                    continue;
                }
                _ => continue,
            };
            *slot = Some(Curve::decode(&buf.elements).with_context(|| format!("{bone}.{property}"))?);
        }

        let Some(&index) = rig.index.get(&bone) else { continue };
        let frames = (0..frame_count).map(|f| f as f32);
        nodes.insert(
            index,
            NodeAnim {
                s: frames.clone().map(|f| scale.as_ref().map_or(Vec3::ONE, |c| Vec3::from_slice(&c.sample(f)[..3]))).collect(),
                r: frames.clone().map(|f| rotate.as_ref().map_or(Quat::IDENTITY, |c| Quat::from_array(c.sample(f)).normalize())).collect(),
                t: frames.map(|f| translate.as_ref().map_or(Vec3::ZERO, |c| Vec3::from_slice(&c.sample(f)[..3]))).collect(),
                compensate_scale,
            },
        );
    }

    // CompensateScale (Maya segment scale compensate): a bone does not inherit its parent's scale,
    // but its translation is still in the parent's scaled space. three.js always inherits scale,
    // so divide it back out.
    let raw_scale: HashMap<usize, Vec<Vec3>> = nodes.iter().map(|(&i, a)| (i, a.s.clone())).collect();
    for (&i, anim) in nodes.iter_mut() {
        let Some(p) = rig.parent[i].filter(|_| anim.compensate_scale) else { continue };
        for (f, s) in anim.s.iter_mut().enumerate() {
            let parent_scale = raw_scale.get(&p).and_then(|v| v.get(f)).copied().unwrap_or(rig.rest_scale[p]);
            *s /= parent_scale;
        }
    }

    if nodes.is_empty() {
        return Ok((None, face_keys));
    }
    let times: Vec<f32> = (0..frame_count).map(|f| f as f32 / FPS).collect();
    let time_bytes: Vec<u8> = times.iter().flat_map(|t| t.to_le_bytes()).collect();
    let input = g.accessor(&time_bytes, FLOAT, times.len(), "SCALAR", None, Some((vec![times[0]], vec![*times.last().unwrap()])));

    let (mut samplers, mut channels) = (Vec::new(), Vec::new());
    for (node, anim) in &nodes {
        let outputs: [(&str, &str, Vec<f32>); 3] = [
            ("translation", "VEC3", anim.t.iter().flat_map(|v| v.to_array()).collect()),
            ("rotation", "VEC4", anim.r.iter().flat_map(|q| q.to_array()).collect()),
            ("scale", "VEC3", anim.s.iter().flat_map(|v| v.to_array()).collect()),
        ];
        for (target, kind, values) in outputs {
            let bytes: Vec<u8> = values.iter().flat_map(|v| v.to_le_bytes()).collect();
            let output = g.accessor(&bytes, FLOAT, frame_count, kind, None, None);
            channels.push(json!({ "sampler": samplers.len(), "target": { "node": node, "path": target } }));
            samplers.push(json!({ "input": input, "output": output, "interpolation": "LINEAR" }));
        }
    }
    Ok((Some(json!({ "name": name, "samplers": samplers, "channels": channels })), face_keys))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn missing_animation_folder_is_reported_not_fatal() {
        let x64 = std::env::temp_dir().join(format!("taiko-extract-anim-{}", std::process::id()));
        fs::create_dir_all(&x64).unwrap();
        let mut report = crate::Report::default();

        convert(&crate::game::Game::new(x64.clone(), false).unwrap(), &x64.join("out"), &mut report).unwrap();

        assert_eq!(report.missing.len(), 1);
        assert!(report.copied.is_empty());
        fs::remove_dir_all(&x64).unwrap();
    }
}
