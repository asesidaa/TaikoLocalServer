//! The attract movies a cabinet can play: `movie/<8-digit id>.wmv`. The server's movie settings (movie_data.json and
//! each cabinet's list) take these ids, so the WebUI offers them instead of asking for numbers.
//! The attract demo videos (`attractdemo_<lang>.wmv`) aren't numbered and aren't listed.

use std::fs;
use std::path::Path;

use anyhow::{Context, Result};
use serde_json::json;

use crate::game::Game;

pub const INDEX: &str = "movies.json";

/// Ids from file names such as `00000012.wmv`, ascending.
fn movie_ids<'a>(names: impl IntoIterator<Item = &'a str>) -> Vec<u32> {
    let mut ids: Vec<u32> = names
        .into_iter()
        .filter_map(|n| n.strip_suffix(".wmv").or_else(|| n.strip_suffix(".WMV")))
        .filter(|stem| !stem.is_empty() && stem.bytes().all(|b| b.is_ascii_digit()))
        .filter_map(|stem| stem.parse().ok())
        .collect();
    ids.sort_unstable();
    ids.dedup();
    ids
}

/// Writes `<dst_dir>/movies.json` (`{ "movies": [1, 2, …] }`) from every layer's `movie` folder.
pub fn convert(game: &Game, dst_dir: &Path, report: &mut crate::Report) -> Result<()> {
    let Some(files) = game.list("movie") else {
        report.missing.push(game.file("movie").display().to_string());
        return Ok(());
    };
    let names: Vec<String> = files.iter().filter_map(|p| p.file_name()?.to_str().map(str::to_owned)).collect();
    let body = serde_json::to_string_pretty(&json!({ "movies": movie_ids(names.iter().map(String::as_str)) }))?;
    let out = dst_dir.join(INDEX);
    if fs::read_to_string(&out).is_ok_and(|old| old == body) {
        report.up_to_date.push(INDEX.into());
        return Ok(());
    }
    fs::create_dir_all(dst_dir).with_context(|| format!("creating {}", dst_dir.display()))?;
    fs::write(&out, body).with_context(|| format!("writing {}", out.display()))?;
    report.copied.push(INDEX.into());
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::movie_ids;

    #[test]
    fn numbered_movies_only() {
        let names = ["00000012.wmv", "00000001.wmv", "attractdemo_en_us.wmv", "00000003.WMV", "readme.txt", ".wmv", "00000001.wmv"];
        assert_eq!(movie_ids(names), vec![1, 3, 12]);
    }
}
