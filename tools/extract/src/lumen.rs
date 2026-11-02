//! Lumen (`.nulm`, magic `LMB\0`) UI files: the Flash-like format the game's 2D UI uses.
//! Parses the shapes and sprite timelines and rasterizes a sprite at a frame label from its
//! texture atlas, so UI art (title plates, dan badges) comes from the game instead of the repo.
//!
//! Layout: a 0x40-byte header, then tags `u32 type, u32 size_in_words, payload` until `0xFF00`.
//! See `docs/webui-refactor-plan.md` §3d for the tag table.

use std::collections::HashMap;

use anyhow::{bail, Context, Result};
use image::RgbaImage;
use serde_json::{json, Map, Value};

const T_SYMBOLS: u32 = 0xF001;
const T_COLORS: u32 = 0xF002;
const T_MATRICES: u32 = 0xF003;
const T_POSITIONS: u32 = 0xF103;
const T_BOUNDS: u32 = 0xF004;
const T_ATLASES: u32 = 0xF007;
const T_SHAPE: u32 = 0xF022;
/// The older games' (PS3) graphic: one textured quad, `x y u v` × 4, then the atlas id and the fill.
const T_QUAD: u32 = 0xF023;
const T_GRAPHIC: u32 = 0xF024;
const T_SPRITE: u32 = 0x0027;
const T_LABEL: u32 = 0x002B;
const T_FRAME: u32 = 0x0001;
const T_KEYFRAME: u32 = 0xF105;
const T_PLACE: u32 = 0x0004;
const T_REMOVE: u32 = 0x0005;
const T_TEXT: u32 = 0x0025;
const T_END: u32 = 0xFF00;

/// `F024` fill type 0: a solid colour, with the colour index stored where the atlas id normally is.
const FILL_SOLID: u16 = 0;

type Matrix = [f32; 6]; // a b c d tx ty
const IDENTITY: Matrix = [1.0, 0.0, 0.0, 1.0, 0.0, 0.0];

#[derive(Debug, Default)]
pub struct Lumen {
    pub symbols: Vec<String>,
    colors: Vec<[f32; 4]>,
    matrices: Vec<Matrix>,
    positions: Vec<[f32; 2]>,
    bounds: Vec<[f32; 4]>,
    /// Every atlas in a file has the same size; layer `i` of the `.nutexb` is atlas `i`.
    atlas_size: (f32, f32),
    atlas_count: usize,
    texts: HashMap<u32, Text>,
    shapes: HashMap<u32, Vec<Graphic>>,
    pub sprites: HashMap<u32, Sprite>,
}

#[derive(Debug, Clone)]
struct Graphic {
    atlas: u32,
    fill: u16,
    verts: Vec<[f32; 4]>, // x y u v
    indices: Vec<u16>,
}

/// A text field: the game fills it at runtime, so only its box and style matter here.
#[derive(Debug, Clone)]
struct Text {
    bounds: u32,
    /// Symbol holding an HTML template: `<p align="center"><font ... size="22" color="#000000">sample</font></p>`.
    template: u32,
    outline: f32,
}

#[derive(Debug, Default, Clone)]
pub struct Sprite {
    pub labels: Vec<(String, u32)>,
    frames: Vec<(u32, Vec<Op>)>,
}

#[derive(Debug, Clone)]
enum Op {
    Place { character: u32, name: u32, mode: u16, depth: u16, clip: u16, transform: u32, color_mult: i32, color_add: i32 },
    Remove { depth: u16 },
}

/// Nijiiro's files are little-endian; the PS3 games' are big-endian with the same layout.
struct Reader<'a> {
    b: &'a [u8],
    be: bool,
}

impl Reader<'_> {
    fn u32(&self, o: usize) -> Result<u32> {
        let b = self.b.get(o..o + 4).context("truncated LMB")?.try_into()?;
        Ok(if self.be { u32::from_be_bytes(b) } else { u32::from_le_bytes(b) })
    }
    fn u16(&self, o: usize) -> Result<u16> {
        let b = self.b.get(o..o + 2).context("truncated LMB")?.try_into()?;
        Ok(if self.be { u16::from_be_bytes(b) } else { u16::from_le_bytes(b) })
    }
    fn f32(&self, o: usize) -> Result<f32> {
        Ok(f32::from_bits(self.u32(o)?))
    }
}

impl Lumen {
    pub fn parse(bytes: &[u8]) -> Result<Lumen> {
        if bytes.get(..4) != Some(b"LMB\0") {
            bail!("not a Lumen file (missing LMB magic)");
        }
        // The header's second word is 0x10 either way round.
        let r = Reader { b: bytes, be: bytes.get(4..8) == Some(&[0, 0, 0, 0x10]) };
        let mut lm = Lumen::default();
        let mut shape: Option<u32> = None;
        let mut sprite: Option<u32> = None;
        // Ops after a keyframe tag belong to that keyframe (a snapshot for seeking), not to the last frame.
        let mut in_keyframe = false;
        let mut o = 0x40;
        while o + 8 <= bytes.len() {
            let (tag, size) = (r.u32(o)?, r.u32(o + 4)? as usize);
            let p = o + 8;
            o = p + size * 4;
            let count = || r.u32(p).map(|n| n as usize);
            match tag {
                T_SYMBOLS => {
                    let mut q = p + 4;
                    for _ in 0..count()? {
                        let len = r.u32(q)? as usize;
                        let s = bytes.get(q + 4..q + 4 + len).context("truncated symbol")?;
                        lm.symbols.push(String::from_utf8_lossy(s).into_owned());
                        q += 4 + ((len + 4) & !3); // NUL-terminated, padded to 4
                    }
                }
                T_COLORS => {
                    for i in 0..count()? {
                        let c = p + 4 + i * 8;
                        lm.colors.push([r.u16(c)?, r.u16(c + 2)?, r.u16(c + 4)?, r.u16(c + 6)?].map(|v| v as f32 / 256.0));
                    }
                }
                T_MATRICES => {
                    for i in 0..count()? {
                        let m = p + 4 + i * 24;
                        lm.matrices.push([r.f32(m)?, r.f32(m + 4)?, r.f32(m + 8)?, r.f32(m + 12)?, r.f32(m + 16)?, r.f32(m + 20)?]);
                    }
                }
                T_POSITIONS => {
                    for i in 0..count()? {
                        lm.positions.push([r.f32(p + 4 + i * 8)?, r.f32(p + 8 + i * 8)?]);
                    }
                }
                T_BOUNDS => {
                    for i in 0..count()? {
                        let b = p + 4 + i * 16;
                        lm.bounds.push([r.f32(b)?, r.f32(b + 4)?, r.f32(b + 8)?, r.f32(b + 12)?]);
                    }
                }
                T_ATLASES => {
                    lm.atlas_count = count()?;
                    // Entries: id, name symbol, width, height. The PS3 files' atlases differ in size (`render_layers`).
                    if lm.atlas_count > 0 {
                        lm.atlas_size = (r.f32(p + 12)?, r.f32(p + 16)?);
                    }
                }
                T_QUAD => {
                    let verts = (0..4).map(|k| { let v = p + k * 16; Ok([r.f32(v)?, r.f32(v + 4)?, r.f32(v + 8)?, r.f32(v + 12)?]) }).collect::<Result<_>>()?;
                    let graphic = Graphic { atlas: r.u32(p + 64)?, fill: r.u16(p + 68)?, verts, indices: vec![0, 1, 2, 0, 2, 3] };
                    lm.shapes.get_mut(&shape.context("graphic outside a shape")?).unwrap().push(graphic);
                }
                T_TEXT => {
                    // Words: id, ?, template symbol, ?, ?, bounds index, … word 15 = outline width.
                    let text = Text { template: r.u32(p + 8)?, bounds: r.u32(p + 20)?, outline: r.f32(p + 60)? };
                    lm.texts.insert(r.u32(p)?, text);
                }
                T_SHAPE => {
                    let id = r.u32(p)?;
                    lm.shapes.insert(id, Vec::new());
                    shape = Some(id);
                }
                T_GRAPHIC => {
                    let (nv, ni) = (r.u16(p + 6)? as usize, r.u32(p + 8)? as usize);
                    let verts = (0..nv)
                        .map(|k| { let v = p + 12 + k * 16; Ok([r.f32(v)?, r.f32(v + 4)?, r.f32(v + 8)?, r.f32(v + 12)?]) })
                        .collect::<Result<_>>()?;
                    let ib = p + 12 + nv * 16;
                    let indices = (0..ni).map(|k| r.u16(ib + k * 2)).collect::<Result<_>>()?;
                    let graphic = Graphic { atlas: r.u32(p)?, fill: r.u16(p + 4)?, verts, indices };
                    lm.shapes.get_mut(&shape.context("graphic outside a shape")?).unwrap().push(graphic);
                }
                T_SPRITE => {
                    let id = r.u32(p)?;
                    lm.sprites.insert(id, Sprite::default());
                    sprite = Some(id);
                    in_keyframe = false;
                }
                T_LABEL => {
                    let name = lm.symbols.get(r.u32(p)? as usize).cloned().unwrap_or_default();
                    current(&mut lm, sprite)?.labels.push((name, r.u32(p + 4)?));
                }
                T_FRAME => {
                    current(&mut lm, sprite)?.frames.push((r.u32(p)?, Vec::new()));
                    in_keyframe = false;
                }
                T_KEYFRAME => in_keyframe = true, // duplicates state already reachable by playing frames
                T_PLACE | T_REMOVE if in_keyframe => {}
                T_PLACE => {
                    let op = Op::Place {
                        character: r.u32(p)?,
                        name: r.u32(p + 12)?,
                        mode: r.u16(p + 16)?,
                        depth: r.u16(p + 20)?,
                        clip: r.u16(p + 22)?,
                        transform: r.u32(p + 28)?,
                        color_mult: r.u32(p + 32)? as i32,
                        color_add: r.u32(p + 36)? as i32,
                    };
                    push_op(&mut lm, sprite, op)?;
                }
                T_REMOVE => push_op(&mut lm, sprite, Op::Remove { depth: r.u16(p + 4)? })?,
                T_END => break,
                _ => {}
            }
        }
        if lm.atlas_size.0 <= 0.0 {
            bail!("Lumen file has no texture atlas");
        }
        Ok(lm)
    }

    /// The sprite whose labels include `label` and that has the most labels. Sprite ids differ
    /// between builds, so callers find sprites by what they contain.
    pub fn sprite_with_label(&self, label: &str) -> Option<u32> {
        self.sprites
            .iter()
            .filter(|(_, s)| s.labels.iter().any(|(n, _)| n == label))
            .max_by_key(|(id, s)| (s.labels.len(), std::cmp::Reverse(**id)))
            .map(|(id, _)| *id)
    }

    /// The sprite whose labels are exactly `labels` (comma-separated, in order), for when a single label is ambiguous.
    pub fn sprite_with_labels(&self, labels: &str) -> Option<u32> {
        let want: Vec<&str> = labels.split(',').collect();
        self.sprites
            .iter()
            .filter(|(_, s)| s.labels.iter().map(|(n, _)| n.as_str()).eq(want.iter().copied()))
            .map(|(id, _)| *id)
            .min()
    }

    /// Characters `sprite` places on any frame, with their instance names (stable across builds).
    fn placements(&self, sprite: u32) -> impl Iterator<Item = (u32, &str)> {
        self.sprites.get(&sprite).into_iter().flat_map(|s| &s.frames).flat_map(|(_, ops)| ops).filter_map(|op| match op {
            Op::Place { character, name, .. } => Some((*character, self.symbols.get(*name as usize).map_or("", String::as_str))),
            Op::Remove { .. } => None,
        })
    }

    /// The sprite that places an instance called `instance` (lowest id if several do).
    pub fn sprite_placing(&self, instance: &str) -> Option<u32> {
        let mut ids: Vec<u32> = self.sprites.keys().copied().filter(|&id| self.placements(id).any(|(_, n)| n == instance)).collect();
        ids.sort();
        ids.first().copied()
    }

    /// The character `parent` places under the instance name `instance`.
    pub fn child(&self, parent: u32, instance: &str) -> Option<u32> {
        self.placements(parent).find(|(_, n)| *n == instance).map(|(c, _)| c)
    }

    /// The labels of `sprite`, in timeline order.
    pub fn labels(&self, sprite: u32) -> Vec<String> {
        self.sprites.get(&sprite).map(|s| s.labels.iter().map(|(n, _)| n.clone()).collect()).unwrap_or_default()
    }

    /// Renders `sprite` at `label` from `atlas` (the decoded `.nutexb`, layers stacked vertically).
    /// `bounds` (stage units, x0 y0 x1 y1) fixes the canvas; otherwise it fits the drawn content.
    pub fn render(&self, sprite: u32, label: &str, atlas: &RgbaImage, bounds: Option<[f32; 4]>) -> Result<RgbaImage> {
        self.render_with(sprite, label, bounds, &|layer, u, v| sample(atlas, self.atlas_size, layer, u, v))
    }

    /// `render` from one image per atlas, each its own size (the PS3 files' `img*.nut`). Atlases the sprite
    /// doesn't use (see `atlases`) may be empty images.
    pub fn render_layers(&self, sprite: u32, label: &str, layers: &[RgbaImage], bounds: Option<[f32; 4]>) -> Result<RgbaImage> {
        self.render_with(sprite, label, bounds, &|layer, u, v| {
            layers.get(layer as usize).map_or([0.0; 4], |img| sample(img, (img.width() as f32, img.height() as f32), 0, u, v))
        })
    }

    /// The atlases `sprite` draws from, over all its labels.
    pub fn atlases(&self, sprite: u32) -> Result<std::collections::BTreeSet<u32>> {
        let mut used = std::collections::BTreeSet::new();
        for label in self.labels(sprite) {
            used.extend(self.scene(sprite, &label)?.tris.iter().filter_map(|t| match t.fill { Fill::Atlas(a) => Some(a), Fill::Solid(_) => None }));
        }
        Ok(used)
    }

    fn render_with(&self, sprite: u32, label: &str, bounds: Option<[f32; 4]>, texel: &dyn Fn(u32, f32, f32) -> [f32; 4]) -> Result<RgbaImage> {
        let scene = self.scene(sprite, label)?;
        let [x0, y0, x1, y1] = bounds.or_else(|| scene.bounds()).context("nothing drawn")?;
        let (w, h) = ((x1 - x0).ceil().max(1.0) as u32, (y1 - y0).ceil().max(1.0) as u32);
        let mut out = vec![[0f32; 4]; (w * h) as usize];
        for tri in &scene.tris {
            let pts = tri.p.map(|[x, y, u, v]| [x - x0, y - y0, u, v]);
            let [a, b, c] = pts;
            let den = (b[1] - c[1]) * (a[0] - c[0]) + (c[0] - b[0]) * (a[1] - c[1]);
            if den == 0.0 {
                continue;
            }
            let (bx0, bx1) = (pts.iter().map(|p| p[0]).fold(f32::MAX, f32::min), pts.iter().map(|p| p[0]).fold(f32::MIN, f32::max));
            let (by0, by1) = (pts.iter().map(|p| p[1]).fold(f32::MAX, f32::min), pts.iter().map(|p| p[1]).fold(f32::MIN, f32::max));
            for y in (by0.floor().max(0.0) as u32)..=(by1.ceil().min(h as f32 - 1.0).max(0.0) as u32) {
                for x in (bx0.floor().max(0.0) as u32)..=(bx1.ceil().min(w as f32 - 1.0).max(0.0) as u32) {
                    let (px, py) = (x as f32 + 0.5, y as f32 + 0.5);
                    let l1 = ((b[1] - c[1]) * (px - c[0]) + (c[0] - b[0]) * (py - c[1])) / den;
                    let l2 = ((c[1] - a[1]) * (px - c[0]) + (a[0] - c[0]) * (py - c[1])) / den;
                    let l3 = 1.0 - l1 - l2;
                    if l1 < -1e-6 || l2 < -1e-6 || l3 < -1e-6 {
                        continue;
                    }
                    if !tri.masks.iter().all(|&m| scene.masks[m].contains(px + x0, py + y0)) {
                        continue;
                    }
                    let texel = match tri.fill {
                        Fill::Solid(color) => color,
                        Fill::Atlas(layer) => texel(layer, l1 * a[2] + l2 * b[2] + l3 * c[2], l1 * a[3] + l2 * b[3] + l3 * c[3]),
                    };
                    let src: [f32; 4] = std::array::from_fn(|k| (texel[k] * tri.mult[k] + tri.add[k]).clamp(0.0, 1.0));
                    let dst = &mut out[(y * w + x) as usize];
                    let alpha = src[3] + dst[3] * (1.0 - src[3]);
                    for k in 0..3 {
                        dst[k] = if alpha > 0.0 { (src[k] * src[3] + dst[k] * dst[3] * (1.0 - src[3])) / alpha } else { 0.0 };
                    }
                    dst[3] = alpha;
                }
            }
        }
        let raw = out.iter().flat_map(|p| p.map(|v| (v * 255.0).round() as u8)).collect();
        Ok(RgbaImage::from_raw(w, h, raw).unwrap())
    }

    #[cfg(test)]
    /// Every sprite with labels, and the instance names it places: what `sprite_with_label` and `child` can find.
    pub fn describe(&self) -> String {
        let mut ids: Vec<_> = self.sprites.keys().copied().collect();
        ids.sort();
        ids.iter()
            .filter_map(|&id| {
                let names: std::collections::BTreeSet<_> = self.placements(id).map(|(c, n)| format!("{n}:{c}")).filter(|n| !n.starts_with(':')).collect();
                let labels = self.labels(id);
                (!labels.is_empty() || !names.is_empty()).then(|| format!("sprite {id}: labels [{}] places [{}]", labels.join(" "), names.into_iter().collect::<Vec<_>>().join(" ")))
            })
            .collect::<Vec<_>>()
            .join("\n")
    }

    /// Stage-space bounds of what `sprite` draws at `label`, clipped by its masks.
    pub fn bounds(&self, sprite: u32, label: &str) -> Result<Option<[f32; 4]>> {
        Ok(self.scene(sprite, label)?.bounds())
    }

    fn scene(&self, sprite: u32, label: &str) -> Result<Scene> {
        let s = self.sprites.get(&sprite).context("no such sprite")?;
        let frame = s.labels.iter().find(|(n, _)| n == label).with_context(|| format!("no label {label}"))?.1;
        let mut scene = Scene::default();
        self.collect(sprite, frame, IDENTITY, [1.0; 4], [0.0; 4], &[], &mut scene, 0);
        Ok(scene)
    }

    /// Union of `sprite`'s mask-clipped bounds over all its labels: a canvas every label fits on.
    pub fn union_bounds(&self, sprite: u32) -> Result<Option<[f32; 4]>> {
        let mut union: Option<[f32; 4]> = None;
        for name in self.labels(sprite) {
            if let Some(b) = self.bounds(sprite, &name)? {
                union = Some(union.map_or(b, |u| [u[0].min(b[0]), u[1].min(b[1]), u[2].max(b[2]), u[3].max(b[3])]));
            }
        }
        Ok(union)
    }

    /// Where `sprite` places `instance` at `label` (its translation).
    pub fn position_of(&self, sprite: u32, label: &str, instance: &str) -> Option<[f32; 2]> {
        let s = self.sprites.get(&sprite)?;
        let frame = s.labels.iter().find(|(n, _)| n == label)?.1;
        self.display_list(s, frame).into_iter().find(|(_, o)| self.symbol(o.name) == instance).map(|(_, o)| [o.matrix[4], o.matrix[5]])
    }

    /// The text fields `sprite` shows at `label`, by instance name: `{ box: [x0,y0,x1,y1], size, color, align, outline }`
    /// in the sprite's stage units, with the style read from the field's HTML template.
    pub fn text_fields(&self, sprite: u32, label: &str) -> Map<String, Value> {
        let Some(s) = self.sprites.get(&sprite) else { return Map::new() };
        let Some(&(_, frame)) = s.labels.iter().find(|(n, _)| n == label) else { return Map::new() };
        let mut fields = Map::new();
        for (_, o) in self.display_list(s, frame) {
            let Some(t) = self.texts.get(&o.character) else { continue };
            let [x0, y0, x1, y1] = self.bounds.get(t.bounds as usize).copied().unwrap_or_default();
            let m = o.matrix;
            let html = self.symbol(t.template);
            let attr = |name: &str| html.split(&format!("{name}=\"")).nth(1).and_then(|v| v.split('"').next()).map(str::to_string);
            fields.insert(self.symbol(o.name).to_string(), json!({
                "box": [m[0] * x0 + m[4], m[3] * y0 + m[5], m[0] * x1 + m[4], m[3] * y1 + m[5]],
                "size": attr("size").and_then(|v| v.parse::<f32>().ok()).unwrap_or(0.0),
                "color": attr("color").unwrap_or_else(|| "#000000".into()),
                "align": attr("align").unwrap_or_else(|| "left".into()),
                "outline": t.outline,
            }));
        }
        fields
    }

    /// A `lumen-scene` v1 document (played by the WebUI's lumen-player.js) holding `sets` (each `{ "sprite": id, … }`)
    /// and every shape and sprite they reach. The atlas is the decoded `.nutexb`, saved beside it as `atlas.png`.
    pub fn scene_json(&self, sets: Map<String, Value>) -> Value {
        let mut keep = std::collections::BTreeSet::new();
        let mut stack: Vec<u32> = sets.values().filter_map(|s| s["sprite"].as_u64()).map(|id| id as u32).collect();
        while let Some(id) = stack.pop() {
            if keep.insert(id) {
                stack.extend(self.placements(id).map(|(c, _)| c));
            }
        }
        let round = |v: f32, k: f64| (v as f64 * k).round() / k;
        let (mut shapes, mut sprites) = (Map::new(), Map::new());
        for &id in &keep {
            if let Some(graphics) = self.shapes.get(&id) {
                let gs: Vec<Value> = graphics
                    .iter()
                    .map(|g| json!({ "atlas": g.atlas, "fill": g.fill, "verts": g.verts.iter().flatten().map(|&v| round(v, 1e5)).collect::<Vec<_>>(), "indices": g.indices }))
                    .collect();
                shapes.insert(id.to_string(), gs.into());
            }
            if let Some(s) = self.sprites.get(&id) {
                // place: [1, char, mode, depth, clip, transform, colorMult, colorAdd, instanceName?]; remove: [0, depth]
                let op = |op: &Op| match *op {
                    Op::Remove { depth } => json!([0, depth]),
                    Op::Place { character, name, mode, depth, clip, transform, color_mult, color_add } => {
                        let mut v = vec![json!(1), json!(character), json!(mode), json!(depth), json!(clip), json!(transform), json!(color_mult), json!(color_add)];
                        if !self.symbol(name).is_empty() {
                            v.push(json!(self.symbol(name)));
                        }
                        Value::Array(v)
                    }
                };
                sprites.insert(id.to_string(), json!({
                    "frameCount": s.frames.iter().map(|(f, _)| f + 1).max().unwrap_or(1),
                    "labels": s.labels.iter().map(|(n, f)| (n.clone(), json!(f))).collect::<Map<_, _>>(),
                    "frames": s.frames.iter().map(|(f, ops)| json!([f, ops.iter().map(op).collect::<Vec<_>>()])).collect::<Vec<_>>(),
                }));
            }
        }
        json!({
            "format": "lumen-scene", "version": 1, "fps": 60,
            "atlas": { "image": "atlas.png", "width": self.atlas_size.0, "height": self.atlas_size.1, "layers": self.atlas_count },
            "colors": self.colors.iter().map(|c| c.map(|v| round(v, 1e5))).collect::<Vec<_>>(),
            "matrices": self.matrices.iter().map(|m| m.map(|v| round(v, 1e5))).collect::<Vec<_>>(),
            "positions": self.positions.iter().map(|p| p.map(|v| round(v, 1e3))).collect::<Vec<_>>(),
            "shapes": shapes, "sprites": sprites, "sets": sets,
        })
    }

    fn symbol(&self, index: u32) -> &str {
        self.symbols.get(index as usize).map_or("", String::as_str)
    }

    /// Plays `sprite`'s frames up to `frame` into a depth-ordered display list.
    fn display_list(&self, sprite: &Sprite, frame: u32) -> Vec<(u16, Placed)> {
        let mut list: HashMap<u16, Placed> = HashMap::new();
        for (_, ops) in sprite.frames.iter().take_while(|(id, _)| *id <= frame) {
            for op in ops {
                match *op {
                    Op::Remove { depth } => {
                        list.remove(&depth);
                    }
                    Op::Place { character, name, mode, depth, clip, transform, color_mult, color_add } => {
                        // A placement on an occupied depth keeps what it doesn't give (transform 0xFFFF, colour -1);
                        // mode 2 (move) also keeps the character. E.g. toho_y22_3 swaps the chibi with "unchanged" transform.
                        let mut obj = match list.get(&depth) {
                            Some(prev) => prev.clone(),
                            None => Placed { character, name, matrix: IDENTITY, mult: [1.0; 4], add: [0.0; 4], clip },
                        };
                        if mode != 2 {
                            obj.character = character;
                            obj.name = name;
                            obj.clip = clip;
                        }
                        let (kind, index) = (transform >> 16, (transform & 0xFFFF) as usize);
                        match kind {
                            0x0000 => obj.matrix = self.matrices.get(index).copied().unwrap_or(IDENTITY),
                            0x8000 => {
                                let [x, y] = self.positions.get(index).copied().unwrap_or_default();
                                obj.matrix = [1.0, 0.0, 0.0, 1.0, x, y];
                            }
                            _ => {} // 0xFFFF: unchanged
                        }
                        if let Some(c) = usize::try_from(color_mult).ok().and_then(|i| self.colors.get(i)) {
                            obj.mult = *c;
                        }
                        if let Some(c) = usize::try_from(color_add).ok().and_then(|i| self.colors.get(i)) {
                            obj.add = *c;
                        }
                        list.insert(depth, obj);
                    }
                }
            }
        }
        let mut list: Vec<_> = list.into_iter().collect();
        list.sort_by_key(|(d, _)| *d);
        list
    }

    #[allow(clippy::too_many_arguments)]
    fn collect(&self, character: u32, frame: u32, m: Matrix, mult: [f32; 4], add: [f32; 4], masks: &[usize], scene: &mut Scene, depth: u32) {
        if depth > 32 {
            return; // malformed recursion guard
        }
        if let Some(graphics) = self.shapes.get(&character) {
            for g in graphics {
                let fill = if g.fill == FILL_SOLID {
                    Fill::Solid(self.colors.get(g.atlas as usize).copied().unwrap_or([0.0; 4]))
                } else {
                    Fill::Atlas(g.atlas)
                };
                for t in g.indices.chunks_exact(3) {
                    let Some(p) = t.iter().map(|&i| g.verts.get(i as usize).map(|&[x, y, u, v]| [m[0] * x + m[2] * y + m[4], m[1] * x + m[3] * y + m[5], u, v])).collect::<Option<Vec<_>>>() else { continue };
                    scene.tris.push(Tri { p: [p[0], p[1], p[2]], fill, mult, add, masks: masks.to_vec() });
                }
            }
            return;
        }
        let Some(sprite) = self.sprites.get(&character) else { return };
        // Nested sprites are drawn at frame 0; only the root plays to its label.
        let mut active: Vec<(usize, u16)> = Vec::new(); // (mask index, clips up to depth)
        for (d, obj) in self.display_list(sprite, frame) {
            active.retain(|&(_, until)| d <= until);
            let child = mul(m, obj.matrix);
            let current: Vec<usize> = masks.iter().copied().chain(active.iter().map(|&(i, _)| i)).collect();
            if obj.clip > 0 {
                // A mask: its shape limits depths d+1..=clip and is never drawn itself.
                let mut mask_scene = Scene::default();
                self.collect(obj.character, 0, child, [1.0; 4], [0.0; 4], &[], &mut mask_scene, depth + 1);
                scene.masks.push(Mask::new(mask_scene.tris.iter().map(|t| t.p.map(|[x, y, ..]| [x, y])).collect()));
                active.push((scene.masks.len() - 1, obj.clip));
                continue;
            }
            let mult = std::array::from_fn(|k| mult[k] * obj.mult[k]);
            let add = std::array::from_fn(|k| add[k] + obj.add[k]);
            self.collect(obj.character, 0, child, mult, add, &current, scene, depth + 1);
        }
    }
}

fn current(lm: &mut Lumen, sprite: Option<u32>) -> Result<&mut Sprite> {
    lm.sprites.get_mut(&sprite.context("tag outside a sprite")?).context("unknown sprite")
}

fn push_op(lm: &mut Lumen, sprite: Option<u32>, op: Op) -> Result<()> {
    let s = current(lm, sprite)?;
    match s.frames.last_mut() {
        Some((_, ops)) => ops.push(op),
        None => bail!("display op before any frame"),
    }
    Ok(())
}

#[derive(Debug, Clone)]
struct Placed {
    character: u32,
    name: u32,
    matrix: Matrix,
    mult: [f32; 4],
    add: [f32; 4],
    clip: u16,
}

#[derive(Clone, Copy)]
enum Fill {
    Atlas(u32),
    Solid([f32; 4]),
}

struct Tri {
    p: [[f32; 4]; 3], // stage x y, atlas u v
    fill: Fill,
    mult: [f32; 4],
    add: [f32; 4],
    masks: Vec<usize>,
}

struct Mask {
    tris: Vec<[[f32; 2]; 3]>,
    bbox: [f32; 4],
}

impl Mask {
    fn new(tris: Vec<[[f32; 2]; 3]>) -> Mask {
        let mut bbox = [f32::MAX, f32::MAX, f32::MIN, f32::MIN];
        for p in tris.iter().flatten() {
            bbox = [bbox[0].min(p[0]), bbox[1].min(p[1]), bbox[2].max(p[0]), bbox[3].max(p[1])];
        }
        Mask { tris, bbox }
    }

    fn contains(&self, x: f32, y: f32) -> bool {
        if x < self.bbox[0] || x > self.bbox[2] || y < self.bbox[1] || y > self.bbox[3] {
            return false;
        }
        self.tris.iter().any(|[a, b, c]| {
            let s = |p: &[f32; 2], q: &[f32; 2]| (q[0] - p[0]) * (y - p[1]) - (q[1] - p[1]) * (x - p[0]);
            let (d1, d2, d3) = (s(a, b), s(b, c), s(c, a));
            !((d1 < 0.0 || d2 < 0.0 || d3 < 0.0) && (d1 > 0.0 || d2 > 0.0 || d3 > 0.0))
        })
    }
}

#[derive(Default)]
struct Scene {
    tris: Vec<Tri>,
    masks: Vec<Mask>,
}

impl Scene {
    /// Bounds of the drawn triangles, each clipped to its masks' boxes.
    fn bounds(&self) -> Option<[f32; 4]> {
        let mut out: Option<[f32; 4]> = None;
        for t in &self.tris {
            let mut b = [f32::MAX, f32::MAX, f32::MIN, f32::MIN];
            for p in &t.p {
                b = [b[0].min(p[0]), b[1].min(p[1]), b[2].max(p[0]), b[3].max(p[1])];
            }
            for &m in &t.masks {
                let mb = self.masks[m].bbox;
                b = [b[0].max(mb[0]), b[1].max(mb[1]), b[2].min(mb[2]), b[3].min(mb[3])];
            }
            if b[0] >= b[2] || b[1] >= b[3] {
                continue; // fully masked out
            }
            out = Some(match out {
                None => b,
                Some(o) => [o[0].min(b[0]), o[1].min(b[1]), o[2].max(b[2]), o[3].max(b[3])],
            });
        }
        out
    }
}

fn mul(a: Matrix, b: Matrix) -> Matrix {
    [
        a[0] * b[0] + a[2] * b[1],
        a[1] * b[0] + a[3] * b[1],
        a[0] * b[2] + a[2] * b[3],
        a[1] * b[2] + a[3] * b[3],
        a[0] * b[4] + a[2] * b[5] + a[4],
        a[1] * b[4] + a[3] * b[5] + a[5],
    ]
}

/// Bilinear sample of atlas `layer` (layers stacked vertically), clamped to that layer.
fn sample(atlas: &RgbaImage, (aw, ah): (f32, f32), layer: u32, u: f32, v: f32) -> [f32; 4] {
    let (x, y) = (u * aw - 0.5, v * ah - 0.5 + layer as f32 * ah);
    let (xi, yi) = (x.floor(), y.floor());
    let (fx, fy) = (x - xi, y - yi);
    let (top, bottom) = (layer as f32 * ah, layer as f32 * ah + ah - 1.0);
    let mut acc = [0f32; 4];
    for (dx, dy, wt) in [(0.0, 0.0, (1.0 - fx) * (1.0 - fy)), (1.0, 0.0, fx * (1.0 - fy)), (0.0, 1.0, (1.0 - fx) * fy), (1.0, 1.0, fx * fy)] {
        let sx = (xi + dx).clamp(0.0, aw - 1.0) as u32;
        let sy = (yi + dy).clamp(top, bottom) as u32;
        if sx >= atlas.width() || sy >= atlas.height() {
            continue;
        }
        let p = atlas.get_pixel(sx, sy).0;
        for k in 0..4 {
            acc[k] += p[k] as f32 / 255.0 * wt;
        }
    }
    acc
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Minimal LMB writer for tests: header, then `(tag, payload words)`.
    fn lmb(tags: &[(u32, Vec<u32>)]) -> Vec<u8> {
        let mut b = b"LMB\0".to_vec();
        b.resize(0x40, 0);
        for (tag, words) in tags {
            b.extend(tag.to_le_bytes());
            b.extend((words.len() as u32).to_le_bytes());
            words.iter().for_each(|w| b.extend(w.to_le_bytes()));
        }
        b.extend(T_END.to_le_bytes());
        b.extend(0u32.to_le_bytes());
        b
    }

    /// A solid-colour quad from (x0,y0) to (x1,y1) as an F024 payload.
    fn solid_quad(color: u32, [x0, y0, x1, y1]: [f32; 4]) -> Vec<u32> {
        let mut w = vec![color, FILL_SOLID as u32 | (4 << 16), 6];
        for (x, y) in [(x0, y0), (x1, y0), (x1, y1), (x0, y1)] {
            w.extend([x.to_bits(), y.to_bits(), 0, 0]);
        }
        w.extend([0 | (1 << 16), 2 | (0 << 16), 2 | (3 << 16)]); // indices 0 1 2, 0 2 3
        w
    }

    /// Place op words: character, mode 1, depth, clip, no transform change.
    fn place(character: u32, depth: u32, clip: u32) -> Vec<u32> {
        vec![character, 0, 0, 0, 1, depth | (clip << 16), 0, 0xFFFF_0000, u32::MAX, u32::MAX, 0, 0]
    }

    fn red_square_masked_to_left_half() -> Vec<u8> {
        let one = 0x100; // colour component 1.0
        lmb(&[
            (T_SYMBOLS, vec![1, 4, u32::from_le_bytes(*b"wood"), 0]),
            (T_COLORS, vec![2, one | (one << 16), one | (one << 16), one, one << 16]), // white, red
            (T_ATLASES, vec![1, 0, 0, 4f32.to_bits(), 4f32.to_bits()]),
            (T_SHAPE, vec![1, 0, 0, 0, 1]),
            (T_GRAPHIC, solid_quad(0, [0.0, 0.0, 2.0, 4.0])), // mask: left half
            (T_SHAPE, vec![2, 0, 0, 0, 1]),
            (T_GRAPHIC, solid_quad(1, [0.0, 0.0, 4.0, 4.0])), // content: whole square, red
            (T_SPRITE, vec![10, 0, 0, 1, 1, 1, 1]),
            (T_LABEL, vec![0, 0, 0]),
            (T_FRAME, vec![0, 2]),
            (T_PLACE, place(1, 1, 2)),
            (T_PLACE, place(2, 2, 0)),
        ])
    }

    #[test]
    fn parses_symbols_shapes_and_labels() {
        let lm = Lumen::parse(&red_square_masked_to_left_half()).unwrap();
        assert_eq!(lm.symbols, vec!["wood"]);
        assert_eq!(lm.atlas_size, (4.0, 4.0));
        assert_eq!(lm.shapes.len(), 2);
        assert_eq!(lm.sprite_with_label("wood"), Some(10));
        assert_eq!(lm.sprite_with_label("rainbow"), None);
    }

    #[test]
    fn clip_mask_limits_content_and_is_not_drawn() {
        let lm = Lumen::parse(&red_square_masked_to_left_half()).unwrap();
        let atlas = RgbaImage::new(4, 4);

        let img = lm.render(10, "wood", &atlas, Some([0.0, 0.0, 4.0, 4.0])).unwrap();

        assert_eq!(img.get_pixel(0, 1).0, [255, 0, 0, 255], "inside the mask: red content");
        assert_eq!(img.get_pixel(3, 1).0[3], 0, "outside the mask: nothing (mask itself not drawn)");
        assert_eq!(lm.bounds(10, "wood").unwrap(), Some([0.0, 0.0, 2.0, 4.0]), "bounds clipped to the mask");
    }

    #[test]
    fn finds_sprites_by_instance_name() {
        let lm = Lumen::parse(&red_square_masked_to_left_half()).unwrap();
        // Both test placements use name symbol 0 ("wood"); the first placed character wins.
        assert_eq!(lm.sprite_placing("wood"), Some(10));
        assert_eq!(lm.child(10, "wood"), Some(1));
        assert_eq!(lm.sprite_placing("missing"), None);
        assert_eq!(lm.labels(10), vec!["wood"]);
    }

    #[test]
    fn replacing_with_unchanged_transform_keeps_the_previous_one() {
        let one = 0x100;
        let mut moved = place(1, 1, 0);
        moved[7] = 0x8000_0000; // position 0
        let lm = Lumen::parse(&lmb(&[
            (T_SYMBOLS, vec![2, 4, u32::from_le_bytes(*b"aaaa"), 0, 4, u32::from_le_bytes(*b"bbbb"), 0]),
            (T_COLORS, vec![1, one | (one << 16), one | (one << 16)]),
            (T_POSITIONS, vec![1, 10f32.to_bits(), 0]),
            (T_ATLASES, vec![1, 0, 0, 4f32.to_bits(), 4f32.to_bits()]),
            (T_SHAPE, vec![1, 0, 0, 0, 1]),
            (T_GRAPHIC, solid_quad(0, [0.0, 0.0, 1.0, 1.0])),
            (T_SHAPE, vec![2, 0, 0, 0, 1]),
            (T_GRAPHIC, solid_quad(0, [0.0, 0.0, 2.0, 2.0])),
            (T_SPRITE, vec![10, 0, 0, 2, 2, 2, 2]),
            (T_LABEL, vec![0, 0, 0]),
            (T_LABEL, vec![1, 1, 0]),
            (T_FRAME, vec![0, 1]),
            (T_PLACE, moved),
            (T_FRAME, vec![1, 1]),
            (T_PLACE, place(2, 1, 0)), // new character, transform 0xFFFF (unchanged)
        ]))
        .unwrap();

        assert_eq!(lm.bounds(10, "aaaa").unwrap(), Some([10.0, 0.0, 11.0, 1.0]));
        assert_eq!(lm.bounds(10, "bbbb").unwrap(), Some([10.0, 0.0, 12.0, 2.0]), "replacement stays at the old position");
    }

    #[test]
    fn keyframe_ops_are_not_added_to_the_last_frame() {
        let one = 0x100;
        let lm = Lumen::parse(&lmb(&[
            (T_SYMBOLS, vec![1, 4, u32::from_le_bytes(*b"wood"), 0]),
            (T_COLORS, vec![1, one | (one << 16), one | (one << 16)]),
            (T_ATLASES, vec![1, 0, 0, 4f32.to_bits(), 4f32.to_bits()]),
            (T_SHAPE, vec![1, 0, 0, 0, 1]),
            (T_GRAPHIC, solid_quad(0, [0.0, 0.0, 1.0, 1.0])),
            (T_SHAPE, vec![2, 0, 0, 0, 1]),
            (T_GRAPHIC, solid_quad(0, [0.0, 0.0, 9.0, 9.0])),
            (T_SPRITE, vec![10, 0, 0, 1, 1, 1, 1]),
            (T_LABEL, vec![0, 0, 0]),
            (T_FRAME, vec![0, 1]),
            (T_PLACE, place(1, 1, 0)),
            (T_KEYFRAME, vec![0, 1]),
            (T_PLACE, place(2, 2, 0)), // keyframe snapshot, not part of frame 0
        ]))
        .unwrap();

        assert_eq!(lm.bounds(10, "wood").unwrap(), Some([0.0, 0.0, 1.0, 1.0]));
    }

    #[test]
    fn rejects_non_lumen_bytes() {
        assert!(Lumen::parse(b"NOPE").is_err());
    }
}
