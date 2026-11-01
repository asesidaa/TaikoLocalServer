use std::f32::consts::PI;
use std::io::Cursor;
use std::sync::OnceLock;

use anyhow::{anyhow, Result};
use binrw::BinRead;

pub const G_CURVE_SHORT4_SCALE: f32 = 1.0 / 32767.0;
pub const G_CURVE_INT8_SCALE: f32 = 1.0 / 127.0;
pub const G_CURVE_NIBBLE_SCALE: f32 = 1.0 / 7.0;
pub const G_CURVE_NIBBLE_BIAS: f32 = 8.0;

#[inline]
pub fn read_u16_le(data: &[u8], off: usize) -> Result<u16> {
    let mut cursor = Cursor::new(data.get(off..).ok_or_else(|| anyhow!("InvalidData"))?);
    u16::read_le(&mut cursor).map_err(|_| anyhow!("InvalidData"))
}

#[inline]
pub fn read_u32_le(data: &[u8], off: usize) -> Result<u32> {
    let mut cursor = Cursor::new(data.get(off..).ok_or_else(|| anyhow!("InvalidData"))?);
    u32::read_le(&mut cursor).map_err(|_| anyhow!("InvalidData"))
}

#[inline]
pub fn read_f32_le(data: &[u8], off: usize) -> Result<f32> {
    let mut cursor = Cursor::new(data.get(off..).ok_or_else(|| anyhow!("InvalidData"))?);
    f32::read_le(&mut cursor).map_err(|_| anyhow!("InvalidData"))
}

pub struct KernelCache {
    data: Vec<f32>,
    offsets: [usize; 8],
}

impl KernelCache {
    fn generate() -> Self {
        let mut offsets = [0usize; 8];
        let mut accum = 0usize;
        for (i, slot) in offsets.iter_mut().enumerate() {
            *slot = accum;
            let dim = (i + 1) * 4;
            accum += dim * dim;
        }

        let mut data = Vec::with_capacity(3264);
        for v18 in 1..=8 {
            let dim = v18 * 4;
            let scale = (2.0 / dim as f32).sqrt();
            for row in 0..dim {
                for col in 0..dim {
                    let val = ((col as f32 + 0.5) * ((row as f32 + 0.5) * (PI / dim as f32))).cos()
                        * scale;
                    data.push(val);
                }
            }
        }
        KernelCache { data, offsets }
    }

    pub fn row(&self, v18: usize, row_idx: usize) -> &[f32] {
        let dim = v18 * 4;
        let base = self.offsets[v18 - 1];
        let start = base + row_idx * dim;
        let end = start + dim;
        &self.data[start..end]
    }
}

pub fn kernel() -> &'static KernelCache {
    static CACHE: OnceLock<KernelCache> = OnceLock::new();
    CACHE.get_or_init(KernelCache::generate)
}

pub fn decode_residual_component(
    residual_stream: &[u8],
    stream_offset: usize,
    base_scale: f32,
    local_idx: usize,
    block_len: usize,
) -> Result<(f32, usize)> {
    if local_idx == 0 || local_idx >= block_len {
        return Ok((0.0, stream_offset));
    }
    let mut curr = stream_offset;
    let word0 = read_u16_le(residual_stream, curr)? as f32;
    let word1 = read_u16_le(residual_stream, curr + 2)? as f32;
    let byte4 = *residual_stream
        .get(curr + 4)
        .ok_or_else(|| anyhow!("InvalidData"))? as f32;
    let byte5 = *residual_stream
        .get(curr + 5)
        .ok_or_else(|| anyhow!("InvalidData"))?;
    let byte6 = *residual_stream
        .get(curr + 6)
        .ok_or_else(|| anyhow!("InvalidData"))?;
    let byte7 = *residual_stream
        .get(curr + 7)
        .ok_or_else(|| anyhow!("InvalidData"))?;
    curr += 8;

    let v14 = (byte5 >> 4) as usize;
    let v15 = (byte5 & 0xF) as usize;
    let v12 = (byte6 & 0xF) as usize;
    let v13 = (byte6 >> 4) as usize;
    let v18 = v14 + v15 + v12 + v13;
    if v18 == 0 || v18 > 8 {
        return Err(anyhow!("InvalidData"));
    }

    let base_amp = word0 * (1.0 / 65535.0) * base_scale;
    let slope = word1 * (1.0 / 65535.0) * base_amp;
    let amp_u8 = byte4;

    let v16 = (byte7 >> 4) as usize;
    let v17 = (byte7 & 0xF) as usize;
    if v16 + v17 > 8 {
        return Err(anyhow!("InvalidData"));
    }
    let v19 = 8 - v16 - v17;

    let mut w = [0.0f32; 8];
    let mut wi = 0usize;
    for _ in 0..v16 {
        w[wi] = base_amp;
        wi += 1;
    }
    for _ in 0..v17 {
        w[wi] = slope;
        wi += 1;
    }
    let scaled_slope = slope * (amp_u8 / 255.0);
    for _ in 0..v19 {
        w[wi] = scaled_slope;
        wi += 1;
    }

    let basis_row = kernel().row(v18, local_idx - 1);
    let mut acc = 0.0f32;
    let mut entry_idx = 0usize;
    let mut basis_ptr = 0usize;

    for _ in 0..v14 {
        if curr + 8 > residual_stream.len() {
            return Err(anyhow!("InvalidData"));
        }
        let shorts = [
            i16::from_le_bytes([residual_stream[curr], residual_stream[curr + 1]]) as f32
                * G_CURVE_SHORT4_SCALE,
            i16::from_le_bytes([residual_stream[curr + 2], residual_stream[curr + 3]]) as f32
                * G_CURVE_SHORT4_SCALE,
            i16::from_le_bytes([residual_stream[curr + 4], residual_stream[curr + 5]]) as f32
                * G_CURVE_SHORT4_SCALE,
            i16::from_le_bytes([residual_stream[curr + 6], residual_stream[curr + 7]]) as f32
                * G_CURVE_SHORT4_SCALE,
        ];
        curr += 8;
        let basis = &basis_row[basis_ptr..basis_ptr + 4];
        basis_ptr += 4;
        let weight = w[entry_idx % 8];
        entry_idx += 1;
        for i in 0..4 {
            acc += basis[i] * shorts[i] * weight;
        }
    }

    for _ in 0..v15 {
        if curr + 4 > residual_stream.len() {
            return Err(anyhow!("InvalidData"));
        }
        let coeff = [
            residual_stream[curr] as i8 as f32 * G_CURVE_INT8_SCALE,
            residual_stream[curr + 1] as i8 as f32 * G_CURVE_INT8_SCALE,
            residual_stream[curr + 2] as i8 as f32 * G_CURVE_INT8_SCALE,
            residual_stream[curr + 3] as i8 as f32 * G_CURVE_INT8_SCALE,
        ];
        curr += 4;
        let basis = &basis_row[basis_ptr..basis_ptr + 4];
        basis_ptr += 4;
        let weight = w[entry_idx % 8];
        entry_idx += 1;
        for i in 0..4 {
            acc += basis[i] * coeff[i] * weight;
        }
    }

    let pair_count = v13 >> 1;
    for _ in 0..pair_count {
        if curr + 4 > residual_stream.len() {
            return Err(anyhow!("InvalidData"));
        }
        let b = [
            residual_stream[curr],
            residual_stream[curr + 1],
            residual_stream[curr + 2],
            residual_stream[curr + 3],
        ];
        curr += 4;
        let decode_nibble = |val: u8, hi: bool| -> f32 {
            let n = if hi { val >> 4 } else { val & 0xF };
            (n as f32 - G_CURVE_NIBBLE_BIAS) * G_CURVE_NIBBLE_SCALE
        };
        let hi_vec = [
            decode_nibble(b[0], true),
            decode_nibble(b[1], true),
            decode_nibble(b[2], true),
            decode_nibble(b[3], true),
        ];
        let basis_hi = &basis_row[basis_ptr..basis_ptr + 4];
        basis_ptr += 4;
        let weight_hi = w[entry_idx % 8];
        entry_idx += 1;
        for i in 0..4 {
            acc += basis_hi[i] * hi_vec[i] * weight_hi;
        }
        let lo_vec = [
            decode_nibble(b[0], false),
            decode_nibble(b[1], false),
            decode_nibble(b[2], false),
            decode_nibble(b[3], false),
        ];
        let basis_lo = &basis_row[basis_ptr..basis_ptr + 4];
        basis_ptr += 4;
        let weight_lo = w[entry_idx % 8];
        entry_idx += 1;
        for i in 0..4 {
            acc += basis_lo[i] * lo_vec[i] * weight_lo;
        }
    }

    Ok((acc, curr))
}

pub fn decode_residual_vector(
    residual_stream: &[u8],
    start_offset: usize,
    base_scale: f32,
    local_idx: usize,
    comp_bits: usize,
    block_len: usize,
) -> Result<(Vec<f32>, usize)> {
    let mut result = vec![0.0f32; comp_bits];
    let mut curr = start_offset;
    for c in 0..comp_bits {
        let (val, next) =
            decode_residual_component(residual_stream, curr, base_scale, local_idx, block_len)?;
        result[c] = val;
        curr = next;
    }
    Ok((result, curr))
}
