//! An older (AC15) game's `USRDIR/data` folder, read from disk or straight out of a `.zip` of the RPCS3 install
//! (stored or deflated members, zip64 for the 3 GB archives). Paths are relative to `data`, `/`-separated.

use std::collections::BTreeMap;
use std::fs::{self, File};
use std::io::{Read, Seek, SeekFrom};
use std::path::{Path, PathBuf};
use std::sync::Mutex;
use std::time::SystemTime;

use anyhow::{bail, ensure, Context, Result};

pub struct Source {
    /// Every file under `data`: its size, and where it is.
    files: BTreeMap<String, Entry>,
    kind: Kind,
    /// What `--game` resolved to, for the report.
    pub label: String,
}

enum Kind {
    Dir(PathBuf),
    Zip { file: Mutex<File>, modified: SystemTime },
}

#[derive(Clone, Copy)]
struct Entry {
    size: u64,
    /// Zip members: compressed size, method, CRC and local header offset.
    packed: u64,
    method: u16,
    crc: u32,
    offset: u64,
}

impl Source {
    /// `path` is a `.zip`, or a folder: `USRDIR/data` itself, or one above it (the RPCS3 root, `USRDIR`), possibly
    /// one folder further up (Blue's `[ARCADE SYSTEM 357] … .7z/dev_hdd0/…`).
    pub fn open(path: &Path) -> Result<Source> {
        if path.is_file() {
            return open_zip(path);
        }
        let mut tops = vec![path.to_path_buf()];
        tops.extend(fs::read_dir(path).with_context(|| format!("reading {}", path.display()))?.flatten().map(|e| e.path()).filter(|p| p.is_dir()));
        for top in tops {
            for rel in ["", "data", "USRDIR/data", "dev_hdd0/game/SCEEXE000/USRDIR/data"] {
                let data = if rel.is_empty() { top.clone() } else { top.join(rel) };
                if data.join("fumen").is_dir() {
                    let mut files = BTreeMap::new();
                    walk(&data, "", &mut files)?;
                    return Ok(Source { files, label: data.display().to_string(), kind: Kind::Dir(data) });
                }
            }
        }
        bail!("{} doesn't look like an older Taiko game: no USRDIR/data folder with fumen in it", path.display())
    }

    /// Files whose path is matched by `keep`, in path order.
    pub fn files<'a>(&'a self, keep: impl Fn(&str) -> bool + 'a) -> impl Iterator<Item = &'a str> + 'a {
        self.files.keys().map(String::as_str).filter(move |p| keep(p))
    }

    pub fn exists(&self, rel: &str) -> bool {
        self.files.contains_key(rel)
    }

    pub fn size(&self, rel: &str) -> Option<u64> {
        self.files.get(rel).map(|e| e.size)
    }

    /// When the file last changed. A zip's members all count as the archive's own time.
    pub fn modified(&self, rel: &str) -> Result<SystemTime> {
        match &self.kind {
            Kind::Dir(root) => Ok(fs::metadata(root.join(rel))?.modified()?),
            Kind::Zip { modified, .. } => Ok(*modified),
        }
    }

    pub fn read(&self, rel: &str) -> Result<Vec<u8>> {
        let e = *self.files.get(rel).with_context(|| format!("{rel} isn't in the game"))?;
        match &self.kind {
            Kind::Dir(root) => fs::read(root.join(rel)).with_context(|| format!("reading {rel}")),
            Kind::Zip { file, .. } => {
                let mut f = file.lock().unwrap();
                let mut head = [0u8; 30];
                f.seek(SeekFrom::Start(e.offset))?;
                f.read_exact(&mut head)?;
                ensure!(u32le(&head, 0) == 0x0403_4b50, "{rel}: bad local header");
                let skip = u16le(&head, 26) as i64 + u16le(&head, 28) as i64;
                f.seek(SeekFrom::Current(skip))?;
                let mut packed = vec![0; e.packed as usize];
                f.read_exact(&mut packed)?;
                drop(f);
                let data = match e.method {
                    0 => packed,
                    8 => {
                        let mut out = Vec::with_capacity(e.size as usize);
                        flate2::read::DeflateDecoder::new(&packed[..]).read_to_end(&mut out).with_context(|| format!("inflating {rel}"))?;
                        out
                    }
                    m => bail!("{rel}: zip compression method {m} isn't supported (only stored and deflate)"),
                };
                ensure!(data.len() as u64 == e.size && crc32fast::hash(&data) == e.crc, "{rel}: corrupt in the zip (size or CRC mismatch)");
                Ok(data)
            }
        }
    }
}

fn walk(dir: &Path, prefix: &str, out: &mut BTreeMap<String, Entry>) -> Result<()> {
    for e in fs::read_dir(dir).with_context(|| format!("reading {}", dir.display()))?.flatten() {
        let name = e.file_name().to_string_lossy().into_owned();
        let rel = format!("{prefix}{name}");
        let meta = e.metadata()?;
        if meta.is_dir() {
            walk(&e.path(), &format!("{rel}/"), out)?;
        } else {
            out.insert(rel, Entry { size: meta.len(), packed: 0, method: 0, crc: 0, offset: 0 });
        }
    }
    Ok(())
}

fn u16le(b: &[u8], at: usize) -> u16 {
    u16::from_le_bytes([b[at], b[at + 1]])
}
fn u32le(b: &[u8], at: usize) -> u32 {
    u32::from_le_bytes(b[at..at + 4].try_into().unwrap())
}
fn u64le(b: &[u8], at: usize) -> u64 {
    u64::from_le_bytes(b[at..at + 8].try_into().unwrap())
}

/// Reads the central directory. The data root is the first `…/USRDIR/data/` in the member names.
fn open_zip(path: &Path) -> Result<Source> {
    let mut f = File::open(path).with_context(|| format!("opening {}", path.display()))?;
    let len = f.metadata()?.len();
    let tail_len = len.min(65_557 + 20);
    let mut tail = vec![0; tail_len as usize];
    f.seek(SeekFrom::Start(len - tail_len))?;
    f.read_exact(&mut tail)?;
    let eocd = (0..=tail.len().saturating_sub(22)).rev().find(|&i| u32le(&tail, i) == 0x0605_4b50)
        .with_context(|| format!("{} isn't a zip", path.display()))?;
    let (mut count, mut cd_size, mut cd_offset) = (u16le(&tail, eocd + 10) as u64, u32le(&tail, eocd + 12) as u64, u32le(&tail, eocd + 16) as u64);
    // zip64: the locator sits just before the EOCD and points at the zip64 EOCD record.
    if eocd >= 20 && u32le(&tail, eocd - 20) == 0x0706_4b50 {
        let mut rec = [0u8; 56];
        f.seek(SeekFrom::Start(u64le(&tail, eocd - 20 + 8)))?;
        f.read_exact(&mut rec)?;
        ensure!(u32le(&rec, 0) == 0x0606_4b50, "{}: bad zip64 record", path.display());
        (count, cd_size, cd_offset) = (u64le(&rec, 32), u64le(&rec, 40), u64le(&rec, 48));
    }
    let mut cd = vec![0; cd_size as usize];
    f.seek(SeekFrom::Start(cd_offset))?;
    f.read_exact(&mut cd)?;

    let mut members = Vec::with_capacity(count as usize);
    let mut at = 0;
    for _ in 0..count {
        ensure!(u32le(&cd, at) == 0x0201_4b50, "{}: bad central directory", path.display());
        let (name_len, extra_len, comment_len) = (u16le(&cd, at + 28) as usize, u16le(&cd, at + 30) as usize, u16le(&cd, at + 32) as usize);
        let name = String::from_utf8_lossy(&cd[at + 46..at + 46 + name_len]).replace('\\', "/");
        let mut e = Entry {
            method: u16le(&cd, at + 10),
            crc: u32le(&cd, at + 16),
            packed: u32le(&cd, at + 20) as u64,
            size: u32le(&cd, at + 24) as u64,
            offset: u32le(&cd, at + 42) as u64,
        };
        // zip64 extra field: the 0xFFFFFFFF fields, in order size, packed, offset.
        let mut x = at + 46 + name_len;
        let end = x + extra_len;
        while x + 4 <= end {
            let (id, n) = (u16le(&cd, x), u16le(&cd, x + 2) as usize);
            if id == 1 {
                let mut v = x + 4;
                for field in [&mut e.size, &mut e.packed, &mut e.offset] {
                    if *field == 0xFFFF_FFFF && v + 8 <= x + 4 + n {
                        *field = u64le(&cd, v);
                        v += 8;
                    }
                }
            }
            x += 4 + n;
        }
        members.push((name, e));
        at = end + comment_len;
    }

    const ROOT: &str = "USRDIR/data/";
    let prefix = members.iter().find_map(|(n, _)| n.find(ROOT).map(|i| n[..i + ROOT.len()].to_string()))
        .with_context(|| format!("{} has no USRDIR/data folder", path.display()))?;
    let files = members.into_iter()
        .filter(|(n, _)| !n.ends_with('/'))
        .filter_map(|(n, e)| n.strip_prefix(&prefix).map(|r| (r.to_string(), e)))
        .collect();
    let modified = f.metadata()?.modified()?;
    Ok(Source { files, label: format!("{} ({prefix})", path.display()), kind: Kind::Zip { file: Mutex::new(f), modified } })
}
