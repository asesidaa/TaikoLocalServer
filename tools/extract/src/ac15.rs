//! The older (AC15, PS3-based) games: Green, Blue, Yellow, Red, White, Murasaki, Kimidori and Momoiro. Copies the
//! game files the server reads into `<out>/data/<era>/data` (`PathHelper.GetDataPath(era)/data`), keeping their paths.

use std::fs;
use std::path::Path;

use anyhow::{Context, Result};
use clap::ValueEnum;

use crate::source::Source;
use crate::Report;

#[derive(Clone, Copy, PartialEq, Eq, Debug, ValueEnum)]
pub enum Era {
    Green,
    Blue,
    Yellow,
    Red,
    White,
    Murasaki,
    Kimidori,
    Momoiro,
}

impl Era {
    pub fn name(self) -> &'static str {
        match self {
            Era::Green => "green",
            Era::Blue => "blue",
            Era::Yellow => "yellow",
            Era::Red => "red",
            Era::White => "white",
            Era::Murasaki => "murasaki",
            Era::Kimidori => "kimidori",
            Era::Momoiro => "momoiro",
        }
    }

    /// The `config/<id>` folder the server reads (`*GameDataPaths.cs`). Kimidori and Momoiro keep it at the top of `data`.
    fn config(self) -> Option<&'static str> {
        match self {
            Era::Green => Some("S11100-1"),
            Era::Blue => Some("S10100-1"),
            Era::Yellow => Some("ST9100-1"),
            Era::Red => Some("ST8100-1"),
            Era::White => Some("ST7100-1"),
            Era::Murasaki => Some("ST6100-1"),
            Era::Kimidori | Era::Momoiro => None,
        }
    }

    /// Files in the config folder: (name, required by `*RequiredDataFiles.cs`).
    fn config_files(self) -> Vec<(&'static str, bool)> {
        let mut v = vec![("musicinfo.xml", true), ("musicmedleyinfo.xml", true)];
        match self {
            Era::Green => v.push(("rewardtitlefiltering.xml", true)),
            Era::Blue => {}
            Era::Yellow | Era::Red | Era::Momoiro => v.push(("defmusic.bin", true)),
            Era::White | Era::Murasaki => v.extend([("defmusic.bin", true), ("present.xml", true), ("spacialbaid.xml", true)]),
            Era::Kimidori => v.extend([("defmusic.bin", true), ("present.xml", false), ("spacialbaid.xml", false)]),
        }
        v
    }
}

/// One `--inputs` rule, relative to `data`: a file, or (ending in `/`) everything under a folder. A `*` segment
/// matches any one folder name. `names`: only the names of the folder's direct children are read.
pub struct Rule {
    pub path: String,
    pub names: bool,
}

impl Rule {
    fn files(path: impl Into<String>) -> Rule {
        Rule { path: path.into(), names: false }
    }
    fn names(path: impl Into<String>) -> Rule {
        Rule { path: path.into(), names: true }
    }

    /// The same test as the server's `ExtractRunner.Match`.
    pub fn matches(&self, path: &str) -> bool {
        let (rule, p): (Vec<_>, Vec<_>) = (self.path.split('/').collect(), path.split('/').collect());
        let seg = |r: &str, s: &str| r == "*" || r == s;
        if self.names {
            // "a/b/" splits to [a, b, ""]: the file is the one segment after the folder.
            rule.len() == p.len() && rule.iter().zip(&p).take(rule.len() - 1).all(|(r, s)| seg(r, s))
        } else if self.path.ends_with('/') {
            p.len() >= rule.len() && rule.iter().zip(&p).take(rule.len() - 1).all(|(r, s)| seg(r, s))
        } else {
            rule.len() == p.len() && rule.iter().zip(&p).all(|(r, s)| seg(r, s))
        }
    }
}

/// The folders the server scans for costume, title and tone ids by file name (`Ac15Don3dDirScanner`,
/// `Ac15CustomizationSourceParser.ScanNameIds`): `nutdata/**/title_name` sits 0, 2 or 3 folders down.
const DON3D_DIRS: [&str; 8] = ["cos", "face", "parts/acc", "parts/body", "parts/head", "parts/paint", "full/cos", "full/face"];
const NAME_DIRS: [&str; 3] = ["nutdata/", "nutdata/*/*/", "nutdata/*/*/*/"];

/// What the server reads, with the config folder this game actually has (see `config_dir`).
fn server_rules(era: Era, config: &str) -> Vec<(Rule, bool)> {
    let mut rules: Vec<(Rule, bool)> = era.config_files().into_iter().map(|(f, req)| (Rule::files(format!("{config}{f}")), req)).collect();
    rules.push((Rule::files("fumen/tuning.bin"), true));
    if era == Era::Blue {
        rules.push((Rule::files(format!("{config}battle/")), false));
    }
    if era == Era::Green {
        rules.extend(["cos_name", "title_name", "tone_name"].map(|d| (Rule::files(format!("nutdata/{d}/nutdatapack.ndp")), true)));
    }
    rules.push((Rule::names("movie/"), false));
    rules.extend(DON3D_DIRS.map(|d| (Rule::names(format!("don3d/{d}/")), false)));
    rules.extend(NAME_DIRS.iter().flat_map(|d| ["title_name", "tone_name"].map(|n| (Rule::names(format!("{d}{n}/")), false))));
    rules
}

/// `config/<id>/`, or `""` for Kimidori and Momoiro. When the game has only another variant of the id (Red's
/// 10.01 dump has `ST8100-7`, not `ST8100-1`), that one is used and copied under the id the server reads.
fn config_dir(era: Era, game: &Source) -> (String, String) {
    let Some(id) = era.config() else { return (String::new(), String::new()) };
    let want = format!("config/{id}/");
    if game.exists(&format!("{want}musicinfo.xml")) {
        return (want.clone(), want);
    }
    let stem = &id[..id.len() - 1];
    let found = game.files(|p| p.starts_with(&format!("config/{stem}")) && p.ends_with("/musicinfo.xml")).next()
        .map(|p| p[..p.len() - "musicinfo.xml".len()].to_string());
    (found.unwrap_or_else(|| want.clone()), want)
}

/// `--inputs --era`: the same shape as Nijiiro's, relative to `USRDIR` (so paths start with `data/`). Every config
/// variant is listed because the uploaded folder isn't known yet.
pub fn inputs(era: Era) -> serde_json::Value {
    let config = if era.config().is_some() { "config/*/" } else { "" };
    let rules: Vec<Rule> = server_rules(era, config).into_iter().map(|(r, _)| r).collect();
    let pick = |names: bool| rules.iter().filter(|r| r.names == names).map(|r| format!("data/{}", r.path)).collect::<Vec<_>>();
    serde_json::json!({ "files": pick(false), "names": pick(true), "mods": null })
}

/// `extract --era <era> --game <data folder or zip>`.
pub fn run(era: Era, game: &Path, out: &Path) -> Result<Vec<Report>> {
    let src = Source::open(game)?;
    println!("game:   {} ({})", src.label, era.name());
    println!("output: {}", out.display());
    let reports = vec![copy_server_files(era, &src, &out.join("data").join(era.name()).join("data"))?];
    Ok(reports)
}

fn copy_server_files(era: Era, src: &Source, dst: &Path) -> Result<Report> {
    let mut report = Report::new(&format!("{} server files", era.name()));
    let (have, want) = config_dir(era, src);
    if have != want {
        println!("{}: no {want}, using {have} (copied as {want})", era.name());
    }
    // Green's name packs match a content rule and a names rule: the first one wins, as in `ExtractRunner.Match`.
    let mut done = std::collections::HashSet::new();
    for (rule, required) in server_rules(era, &have) {
        // A `config/` match is written under the id the server reads.
        let target = |p: &str| if have != want && p.starts_with(&have) { format!("{want}{}", &p[have.len()..]) } else { p.to_string() };
        let mut any = false;
        for rel in src.files(|p| rule.matches(p)) {
            any = true;
            if !done.insert(rel.to_string()) {
                continue;
            }
            let to = dst.join(target(rel));
            if rule.names {
                // Only the name is read; never replace a real file someone copied in by hand.
                if !to.exists() {
                    fs::create_dir_all(to.parent().unwrap())?;
                    fs::write(&to, [])?;
                    report.copied.push(rel.to_string());
                } else {
                    report.up_to_date.push(rel.to_string());
                }
                continue;
            }
            let fresh = fs::metadata(&to).ok().filter(|m| Some(m.len()) == src.size(rel))
                .and_then(|m| m.modified().ok()).is_some_and(|t| src.modified(rel).is_ok_and(|s| t >= s));
            if fresh {
                report.up_to_date.push(rel.to_string());
                continue;
            }
            fs::create_dir_all(to.parent().unwrap())?;
            fs::write(&to, src.read(rel)?).with_context(|| format!("writing {}", to.display()))?;
            report.copied.push(rel.to_string());
        }
        if !any && required {
            report.missing.push(rule.path.clone());
        }
    }
    Ok(report)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn rules_match_like_the_server() {
        let file = Rule::files("fumen/tuning.bin");
        assert!(file.matches("fumen/tuning.bin") && !file.matches("fumen/tuning.bin2") && !file.matches("fumen/x/tuning.bin"));
        let dir = Rule::files("config/*/battle/");
        assert!(dir.matches("config/ST8100-1/battle/a.xml") && dir.matches("config/x/battle/a/b.xml") && !dir.matches("config/x/a.xml"));
        let names = Rule::names("nutdata/*/*/title_name/");
        assert!(names.matches("nutdata/pack/05/title_name/title_1_9.nut"));
        assert!(!names.matches("nutdata/title_name/a.nut") && !names.matches("nutdata/pack/05/title_name/x/a.nut"));
    }
}
