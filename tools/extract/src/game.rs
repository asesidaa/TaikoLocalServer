//! The game's files as the game sees them. With `--mods`, a LayeredFS `Data_mods` folder is layered over
//! `Data`: a file in `Data_mods/x64/<path>` replaces `Data/x64/<path>`, and folders list both layers' entries.
//! Modded datatables live re-encrypted in `Data_mods/x64_enc/datatable` (the same IV-prefixed format the
//! server decrypts); `Data_mods/x64/datatable` holds the modding tools' decrypted JSON and is ignored.

use std::collections::BTreeMap;
use std::fs;
use std::path::{Path, PathBuf};

use anyhow::{bail, Result};

pub struct Game {
    /// Top layer first: `Data_mods/x64` (with `--mods`), then `Data/x64`.
    layers: Vec<PathBuf>,
    /// `Data_mods/x64_enc/datatable`, with `--mods`.
    mod_datatables: Option<PathBuf>,
}

impl Game {
    /// `x64` is `<game>/Data/x64`; with `mods`, `<game>/Data_mods` is layered on top of it.
    pub fn new(x64: PathBuf, mods: bool) -> Result<Game> {
        let mut layers = vec![x64.clone()];
        let mut mod_datatables = None;
        if mods {
            let root = x64.parent().and_then(Path::parent).map(|g| g.join("Data_mods"));
            let Some(root) = root.filter(|r| r.is_dir()) else {
                bail!("--mods: no Data_mods folder next to {}", x64.parent().unwrap_or(&x64).display());
            };
            layers.insert(0, root.join("x64"));
            mod_datatables = Some(root.join("x64_enc").join("datatable"));
        }
        Ok(Game { layers, mod_datatables })
    }

    pub fn base(&self) -> &Path {
        self.layers.last().unwrap()
    }

    pub fn modded(&self) -> bool {
        self.layers.len() > 1
    }

    /// The file at `rel` from the top layer that has it; the base path when none do (so reports name it).
    pub fn file(&self, rel: impl AsRef<Path>) -> PathBuf {
        let rel = rel.as_ref();
        self.layers.iter().map(|l| l.join(rel)).find(|p| p.exists()).unwrap_or_else(|| self.base().join(rel))
    }

    /// A datatable `.bin`: the modded, re-encrypted one when there is one.
    pub fn datatable(&self, name: &str) -> PathBuf {
        self.mod_datatables.iter().map(|d| d.join(name)).find(|p| p.is_file()).unwrap_or_else(|| self.base().join("datatable").join(name))
    }

    /// The entries of folder `rel` across layers, by name with the top layer winning, sorted by name.
    /// `None` when no layer has the folder.
    pub fn list(&self, rel: impl AsRef<Path>) -> Option<Vec<PathBuf>> {
        let rel = rel.as_ref();
        let mut found = false;
        let mut entries = BTreeMap::new();
        for layer in &self.layers {
            let Ok(dir) = fs::read_dir(layer.join(rel)) else { continue };
            found = true;
            for e in dir.flatten() {
                entries.entry(e.file_name()).or_insert_with(|| e.path());
            }
        }
        found.then(|| entries.into_values().collect())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn mods_replace_files_and_add_entries() {
        let root = std::env::temp_dir().join(format!("extract-game-test-{}", std::process::id()));
        let (base, mods) = (root.join("Data").join("x64"), root.join("Data_mods").join("x64"));
        for (dir, file, body) in [(&base, "font/en_32.xml", "base"), (&base, "font/jp_32_DB.xml", "base"), (&mods, "font/en_32.xml", "mod"), (&mods, "font/kr_32.xml", "mod")] {
            fs::create_dir_all(dir.join("font")).unwrap();
            fs::write(dir.join(file), body).unwrap();
        }
        fs::create_dir_all(root.join("Data_mods").join("x64_enc").join("datatable")).unwrap();
        fs::write(root.join("Data_mods").join("x64_enc").join("datatable").join("musicinfo.bin"), "enc").unwrap();

        let plain = Game::new(base.clone(), false).unwrap();
        assert_eq!(fs::read_to_string(plain.file("font/en_32.xml")).unwrap(), "base");
        let game = Game::new(base.clone(), true).unwrap();
        assert_eq!(fs::read_to_string(game.file("font/en_32.xml")).unwrap(), "mod", "a mod file replaces the base one");
        let names: Vec<_> = game.list("font").unwrap().iter().map(|p| p.file_name().unwrap().to_string_lossy().into_owned()).collect();
        assert_eq!(names, ["en_32.xml", "jp_32_DB.xml", "kr_32.xml"], "folders list both layers");
        assert_eq!(fs::read_to_string(game.datatable("musicinfo.bin")).unwrap(), "enc");
        assert_eq!(game.datatable("neiro.bin"), base.join("datatable").join("neiro.bin"), "unmodded datatables come from Data");
        fs::remove_dir_all(root).ok();
    }
}
