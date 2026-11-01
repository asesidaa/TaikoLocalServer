//! nuanmb v1.2 curve buffers
//! `nu::DecompressCurve::Decompress` (0x140619040) and its helpers.
//!
//! Every property buffer starts with a u32 header:
//!   bits 12..15  component count (1 = scalar, 3 = vec3, 4 = quat/vec4)
//!   bits  8..11  key time table: 0 none, 1 f32 times, 2 u16 * step, 3 u8 * step, 4 uniform step
//!   bits  4..5   0x10 = values/endpoints stored as i16 / 32767 instead of f32
//!   bits  0..3   codec: 0 linear keys, 2 stepped keys, 3 constant,
//!                8 one DCT block, 9 multi-block DCT (33 keys per block)
//! Rotations are plain 4-component curves; the caller normalizes.

use anyhow::{anyhow, Result};

use super::common::{decode_residual_vector, read_f32_le, read_u16_le, read_u32_le};

const BLOCK: usize = 33;

pub struct Curve {
    /// Key times in frames.
    times: Vec<f32>,
    keys: Vec<[f32; 4]>,
    stepped: bool,
}

impl Curve {
    pub fn decode(bytes: &[u8]) -> Result<Curve> {
        let header = read_u32_le(bytes, 0)?;
        let comps = (header >> 12) as usize;
        let short = header & 0x30 == 0x10;
        let codec = header & 0xF;
        if !(1..=4).contains(&comps) {
            return Err(anyhow!("bad component count in header {header:#x}"));
        }

        // Time table (FUN_140619290).
        let (times, data) = match header & 0xF00 {
            0x000 => (vec![0.0], 4),
            mode => {
                let n = read_u32_le(bytes, 4)? as usize;
                match mode {
                    0x100 => {
                        let t = (0..n).map(|i| read_f32_le(bytes, 8 + 4 * i)).collect::<Result<_>>()?;
                        (t, 8 + 4 * n)
                    }
                    0x200 => {
                        let step = read_f32_le(bytes, 8)?;
                        let t = (0..n)
                            .map(|i| Ok(read_u16_le(bytes, 12 + 2 * i)? as f32 * step))
                            .collect::<Result<_>>()?;
                        (t, align4(12 + 2 * n))
                    }
                    0x300 => {
                        let step = read_f32_le(bytes, 8)?;
                        let t = bytes
                            .get(12..12 + n)
                            .ok_or_else(|| anyhow!("truncated u8 time table"))?
                            .iter()
                            .map(|&b| b as f32 * step)
                            .collect();
                        (t, align4(12 + n))
                    }
                    0x400 => {
                        let step = read_f32_le(bytes, 8)?;
                        ((0..n).map(|i| i as f32 * step).collect(), 12)
                    }
                    _ => return Err(anyhow!("unknown time mode in header {header:#x}")),
                }
            }
        };
        let n = times.len();
        if n == 0 {
            return Err(anyhow!("curve has no keys"));
        }
        let d = &bytes.get(data..).ok_or_else(|| anyhow!("truncated curve"))?;

        let keys = match codec {
            0 | 2 => (0..n).map(|k| read_value(d, k * comps, comps, short)).collect::<Result<_>>()?,
            3 => vec![read_value(d, 0, comps, short)?],
            8 | 9 => decode_dct(d, codec, comps, short, n)?,
            _ => return Err(anyhow!("unknown codec in header {header:#x}")),
        };
        Ok(Curve { times, keys, stepped: codec == 2 })
    }

    /// Sample at `frame`, clamping outside the key range (non-looping game behavior).
    pub fn sample(&self, frame: f32) -> [f32; 4] {
        if self.keys.len() == 1 || frame <= self.times[0] {
            return self.keys[0];
        }
        let last = self.keys.len() - 1;
        if frame >= self.times[last] {
            return self.keys[last];
        }
        let k = self.times.partition_point(|&t| t <= frame) - 1;
        if self.stepped {
            return self.keys[k];
        }
        let span = self.times[k + 1] - self.times[k];
        let f = if span > 0.0 { (frame - self.times[k]) / span } else { 0.0 };
        let (a, b) = (self.keys[k], self.keys[k + 1]);
        std::array::from_fn(|i| a[i] + (b[i] - a[i]) * f)
    }
}

fn align4(x: usize) -> usize {
    (x + 3) & !3
}

/// One key's components; `index` counts components, not keys.
fn read_value(d: &[u8], index: usize, comps: usize, short: bool) -> Result<[f32; 4]> {
    let mut v = [0.0; 4];
    for (c, out) in v.iter_mut().enumerate().take(comps) {
        *out = if short {
            read_u16_le(d, 2 * (index + c))? as i16 as f32 / 32767.0
        } else {
            read_f32_le(d, 4 * (index + c))?
        };
    }
    Ok(v)
}

/// Codecs 8 and 9 (FUN_1406198a0 / FUN_1406195e0): per block, a linear ramp between
/// endpoints plus a DCT residual. Blocks hold 33 key intervals; the last key of a
/// block is the next block's first endpoint.
fn decode_dct(d: &[u8], codec: u32, comps: usize, short: bool, n: usize) -> Result<Vec<[f32; 4]>> {
    let base_scale = read_f32_le(d, 0)?;
    let ep_size = if short { 2 } else { 4 };
    let (blocks, offsets, endpoints) = if codec == 9 {
        let blocks = read_u16_le(d, 4)? as usize;
        if blocks == 0 {
            return Err(anyhow!("DCT curve with zero blocks"));
        }
        let offsets = (0..blocks - 1).map(|b| read_u16_le(d, 6 + 2 * b)).collect::<Result<Vec<_>>>()?;
        let table_end = 6 + 2 * (blocks - 1);
        (blocks, offsets, if short { table_end } else { align4(table_end) })
    } else {
        (1, Vec::new(), 4)
    };
    let residual = endpoints + (blocks + 1) * comps * ep_size;

    let endpoint = |b: usize| read_value(&d[endpoints..], b * comps, comps, short);
    let block_len = |b: usize| if b + 1 == blocks { n - BLOCK * b - 1 } else { BLOCK };

    // Key k is block k/33 at local k%33; the final key is the last block's end endpoint.
    (0..n)
        .map(|k| {
            if n == 1 {
                return endpoint(0);
            }
            let (b, local) = if k == n - 1 {
                ((n - 2) / BLOCK, (n - 2) % BLOCK + 1)
            } else {
                (k / BLOCK, k % BLOCK)
            };
            if b >= blocks {
                return Err(anyhow!("key {k} outside {blocks} DCT blocks"));
            }
            let len = block_len(b);
            let (e0, e1) = (endpoint(b)?, endpoint(b + 1)?);
            let t = local as f32 / len as f32;
            // A zero offset for blocks after the first means "no residual".
            let start = match b {
                0 => Some(residual),
                _ => Some(offsets[b - 1] as usize).filter(|&o| o != 0).map(|o| residual + 4 * o),
            };
            let r = match start {
                Some(s) => decode_residual_vector(d, s, base_scale, local, comps, len)?.0,
                None => vec![0.0; comps],
            };
            Ok(std::array::from_fn(|i| {
                if i < comps { e0[i] + (e1[i] - e0[i]) * t + r[i] } else { 0.0 }
            }))
        })
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Decodes every curve in `$NUANMB_DIR` (e.g. the game's Data/x64/animation) and
    /// checks rotation keys come out near unit length before normalization.
    #[test]
    fn decodes_game_files() {
        let Ok(dir) = std::env::var("NUANMB_DIR") else { return };
        let (mut curves, mut worst) = (0, 0.0f32);
        for entry in std::fs::read_dir(dir).unwrap() {
            let path = entry.unwrap().path();
            let Ok(ssbh_lib::formats::anim::Anim::V12 { tracks, buffers, .. }) =
                ssbh_lib::formats::anim::Anim::from_file(&path)
            else {
                continue;
            };
            for t in tracks.elements.iter() {
                for p in t.properties.elements.iter() {
                    let bytes = &buffers.elements[p.buffer_index as usize].elements;
                    let curve = Curve::decode(bytes)
                        .unwrap_or_else(|e| panic!("{} {}: {e}", path.display(), t.name.to_string_lossy()));
                    curves += 1;
                    if p.name.to_string_lossy() == "Rotate" {
                        for k in &curve.keys {
                            let len = k.iter().map(|x| x * x).sum::<f32>().sqrt();
                            worst = worst.max((len - 1.0).abs());
                        }
                    }
                }
            }
        }
        eprintln!("{curves} curves, worst rotation |len-1| = {worst}");
        assert!(worst < 0.05);
    }
}
