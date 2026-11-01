//! Drum tones: `sound/se_neiro_NN_v12a.nus3bank` holds the tone's don and ka hits as two plain PCM WAVs,
//! in that order. They're cut out as-is, since browsers play PCM WAV natively.

use std::fs;
use std::path::Path;

use anyhow::{bail, Context, Result};

/// The tone ids the game ships (`GameData.Tones` in the WebUI).
const TONES: u32 = 20;

/// Every embedded `RIFF....WAVE` chunk, whole, in file order.
fn wavs(bank: &[u8]) -> Vec<&[u8]> {
    let mut out = Vec::new();
    let mut i = 0;
    while let Some(at) = bank[i..].windows(4).position(|w| w == b"RIFF").map(|p| p + i) {
        let size = bank.get(at + 4..at + 8).map(|b| u32::from_le_bytes(b.try_into().unwrap()) as usize);
        match size.map(|s| at + 8 + s) {
            Some(end) if end <= bank.len() && bank.get(at + 8..at + 12) == Some(b"WAVE") => {
                out.push(&bank[at..end]);
                i = end;
            }
            _ => i = at + 4,
        }
    }
    out
}

/// Writes `<dst_dir>/<NN>_don.wav` and `<NN>_ka.wav` for every tone.
pub fn convert(game: &crate::game::Game, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;
    for id in 0..TONES {
        let name = format!("{id:02}");
        let src = game.file(format!("sound/se_neiro_{name}_v12a.nus3bank"));
        if !src.is_file() {
            report.missing.push(src.display().to_string());
            continue;
        }
        let ka = dst_dir.join(format!("{name}_ka.wav"));
        if crate::is_newer_than(&ka, &src)? {
            report.up_to_date.push(name);
            continue;
        }
        let result = fs::read(&src).context("reading").and_then(|bank| match wavs(&bank)[..] {
            [don, ka_wav] => {
                fs::write(dst_dir.join(format!("{name}_don.wav")), don)?;
                fs::write(&ka, ka_wav)?; // written last: its timestamp marks the pair as done
                Ok(())
            }
            ref found => bail!("expected 2 WAVs, found {}", found.len()),
        });
        match result {
            Ok(()) => report.copied.push(name),
            Err(e) => report.failed.push(format!("tone {name}: {e:#}")),
        }
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    fn wav(body: &[u8]) -> Vec<u8> {
        let mut b = b"RIFF".to_vec();
        b.extend((4 + body.len() as u32).to_le_bytes());
        b.extend(b"WAVE");
        b.extend(body);
        b
    }

    #[test]
    fn finds_whole_wavs_and_skips_stray_magic() {
        let (don, ka) = (wav(b"fmt don"), wav(b"fmt RIFF ka"));
        let mut bank = b"NUS3 RIFF junk".to_vec();
        bank.extend(&don);
        bank.extend(b"PACK");
        bank.extend(&ka);
        assert_eq!(wavs(&bank), [&don[..], &ka[..]]);
    }
}
