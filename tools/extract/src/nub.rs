//! The older games' drum sounds: `sound/se/SE_GAME_NEIRO_<id>_C.nub`, NUB v2.1 banks of Sony VAG (PS-ADPCM) streams,
//! decoded to 16-bit WAV for the Profile's tone preview.

use anyhow::{ensure, Context, Result};

fn be32(b: &[u8], at: usize) -> Result<usize> {
    Ok(u32::from_be_bytes(b.get(at..at + 4).context("truncated NUB")?.try_into()?) as usize)
}

/// The bank's streams as (sample rate, PCM). Header: u32 version `0x00020100`, u32, u32 bank id, u32 stream count,
/// u32 data offset, u32 data size, u32 offset of the stream-header offsets. A stream header starts with its codec
/// (`vag\0`) and has the stream's offset in the data at +0x10, its size at +0x14 and its sample rate at +0xBC.
pub fn streams(b: &[u8]) -> Result<Vec<(u32, Vec<i16>)>> {
    ensure!(be32(b, 0)? == 0x0002_0100, "not a NUB v2.1 bank");
    let (count, data, table) = (be32(b, 0x0C)?, be32(b, 0x10)?, be32(b, 0x18)?);
    (0..count)
        .map(|i| {
            let h = be32(b, table + i * 4)?;
            ensure!(b.get(h..h + 4) == Some(b"vag\0"), "stream {i} isn't VAG");
            let (size, off) = (be32(b, h + 0x14)?, be32(b, h + 0x18)?);
            let adpcm = b.get(data + off..data + off + size).context("stream out of range")?;
            Ok((be32(b, h + 0xBC)? as u32, vag(adpcm)))
        })
        .collect()
}

/// PS-ADPCM: 16-byte frames of a predictor/shift byte, a flags byte (7 ends the stream) and 28 4-bit samples, low
/// nibble first.
fn vag(b: &[u8]) -> Vec<i16> {
    const FILTERS: [(i32, i32); 5] = [(0, 0), (60, 0), (115, -52), (98, -55), (122, -60)];
    let (mut h1, mut h2) = (0i32, 0i32);
    let mut out = Vec::with_capacity(b.len() / 16 * 28);
    for frame in b.chunks_exact(16) {
        if frame[1] == 7 {
            break;
        }
        let (f0, f1) = FILTERS[(frame[0] >> 4).min(4) as usize];
        let shift = (frame[0] & 0xF) as i32;
        for k in 0..28 {
            let nibble = (frame[2 + k / 2] >> ((k & 1) * 4)) & 0xF;
            let s = ((((nibble as i32) << 12) as i16 as i32) >> shift) + ((h1 * f0 + h2 * f1 + 32) >> 6);
            let s = s.clamp(i16::MIN as i32, i16::MAX as i32);
            out.push(s as i16);
            (h2, h1) = (h1, s);
        }
    }
    out
}

/// A mono 16-bit PCM WAV.
pub fn wav(rate: u32, pcm: &[i16]) -> Vec<u8> {
    let bytes = (pcm.len() * 2) as u32;
    let mut w = Vec::with_capacity(44 + bytes as usize);
    w.extend(b"RIFF");
    w.extend((36 + bytes).to_le_bytes());
    w.extend(b"WAVEfmt ");
    for v in [16u32, 1 | (1 << 16), rate, rate * 2, 2 | (16 << 16)] {
        w.extend(v.to_le_bytes());
    }
    w.extend(b"data");
    w.extend(bytes.to_le_bytes());
    w.extend(pcm.iter().flat_map(|s| s.to_le_bytes()));
    w
}

pub fn inputs() -> Vec<crate::ac15::Rule> {
    vec![crate::ac15::Rule { path: "sound/se/".into(), names: false }]
}

/// `sound/se/SE_GAME_NEIRO_<id>_C.nub` → `tone/<id>_{don,ka}.wav`: streams 0 and 1 (measured, not heard: the don
/// crosses zero ~550 times a second, the ka 4,000–7,000). Stream 2 is the same sound in every bank. `_L` and `_R` are
/// the same sounds placed left and right; the centred ones are enough for a preview.
pub fn convert(src: &crate::source::Source, out: &std::path::Path, report: &mut crate::Report) -> Result<()> {
    const PREFIX: &str = "sound/se/SE_GAME_NEIRO_";
    let files: Vec<&str> = src.files(|p| p.strip_prefix(PREFIX).is_some_and(|f| f.ends_with("_C.nub"))).collect();
    if files.is_empty() {
        return Ok(());
    }
    let dir = out.join("tone");
    std::fs::create_dir_all(&dir)?;
    for rel in files {
        let id = &rel[PREFIX.len()..rel.len() - "_C.nub".len()];
        let first = dir.join(format!("{id}_don.wav"));
        if crate::nut::is_fresh(&first, src, rel) {
            report.up_to_date.push(id.into());
            continue;
        }
        let result = src.read(rel).and_then(|b| streams(&b)).and_then(|s| {
            for ((rate, pcm), hit) in s.iter().zip(["don", "ka"]) {
                std::fs::write(dir.join(format!("{id}_{hit}.wav")), wav(*rate, pcm))?;
            }
            Ok(())
        });
        match result {
            Ok(()) => report.copied.push(id.into()),
            Err(e) => report.failed.push(format!("{id}: {e:#}")),
        }
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn decodes_vag_frames() {
        // Filter 0, shift 0: each nibble is the sample's top 4 bits (1 → 4096, 0xF → -4096); flags 7 ends the stream.
        let mut b = vec![0x00, 0x00, 0xF1];
        b.resize(16, 0);
        b.extend([0x00, 0x07]);
        b.resize(32, 0);
        let pcm = vag(&b);
        assert_eq!(pcm.len(), 28);
        assert_eq!(&pcm[..3], &[4096, -4096, 0]);
        // Filter 1 carries 60/64 of the previous sample.
        let mut b = vec![0x10, 0x00, 0x01];
        b.resize(16, 0);
        assert_eq!(&vag(&b)[..2], &[4096, 3840]);
    }

    #[test]
    fn wav_header() {
        let w = wav(44100, &[1, -1]);
        assert_eq!(w.len(), 48);
        assert_eq!(&w[..4], b"RIFF");
        assert_eq!(u32::from_le_bytes(w[24..28].try_into().unwrap()), 44100);
        assert_eq!(&w[44..], &[1, 0, 0xFF, 0xFF]);
    }
}
