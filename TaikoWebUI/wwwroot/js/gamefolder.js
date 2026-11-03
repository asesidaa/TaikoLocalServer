// "Choose <Game>'s folder…" (ExtractChecklist, with AllowRemoteGameDir): sends only the game files extract reads,
// from a folder picked in the browser, so the server can run extract when the game isn't on its machine. pick()
// finds the game in the folder and counts what would be sent; send() sends it, when Extract is pressed.

const authFetch = (url, init) => {
  let token = null;
  try { token = JSON.parse(localStorage.getItem("authToken")); } catch { }
  return fetch(url, token ? { ...init, headers: { ...init?.headers, Authorization: `Bearer ${token}` } } : init);
};
const message = async res => (await res.json().catch(() => null))?.message || `HTTP ${res.status}`;

// Batches stay under the server's 128 MB request limit (a bigger file goes alone) and its form field limit.
const BatchBytes = 64 * 1024 * 1024, BatchFiles = 500;

// The picked games' files waiting to be sent: game → [{ path, body }].
const picked = new Map();
// The send in progress, for stop().
let sending = null;

// Where the game is in the picked folder, as the prefix its files' paths share, and how a path relative to the game
// folder (what extract --inputs lists) is made. Nijiiro: the folder holding Data/x64 (the pick may be it, a folder
// above it, Data, or x64 itself), found by its music table. An older game: the folder holding data/ (USRDIR), found
// by its musicinfo.xml (in data/, or data/config/<id>/).
const locate = (game, paths) => {
  if (game === "Nijiiro") {
    const key = paths.find(p => /(^|\/)x64\/datatable\/musicinfo\.bin$/.test(p));
    if (!key) return null;
    const x64 = key.slice(0, -"datatable/musicinfo.bin".length);
    const root = x64.endsWith("Data/x64/") ? x64.slice(0, -"Data/x64/".length) : null;
    return { root: root ?? x64, relative: p => root !== null ? (p.startsWith(root) ? p.slice(root.length) : null)
      : p.startsWith(x64) ? "Data/x64/" + p.slice(x64.length) : null };
  }
  const key = paths.map(p => /^(.*?)data\/(config\/[^/]+\/)?musicinfo\.xml$/.exec(p)).find(m => m && (m[1] === "" || m[1].endsWith("/")));
  if (!key) return null;
  const root = key[1];
  return { root, relative: p => p.startsWith(root) ? p.slice(root.length) : null };
};

// Which older game a picked folder is, as ExtractRunner.OlderGameOf: each keeps the older games' config folders too, so
// it's the newest one there (any variant: an early Red has only ST8100-7). Kimidori and Momoiro have none: their
// PARAM.SFO, next to USRDIR, is titled "…(ST51)" and "…(ST41)". null when it can't tell.
const ConfigsNewestFirst = [["Green", "S11100"], ["Blue", "S10100"], ["Yellow", "ST9100"], ["Red", "ST8100"], ["White", "ST7100"], ["Murasaki", "ST6100"]];
const olderGameOf = async (root, files) => {
  const configs = new Set(files.map(f => f.webkitRelativePath.startsWith(root + "data/config/") ? f.webkitRelativePath.slice(root.length + 12).split("/")[0] : null));
  for (const [game, id] of ConfigsNewestFirst)
    if ([...configs].some(c => c?.startsWith(id + "-"))) return game;
  const sfoPath = root.replace(/USRDIR\/$/, "") + "PARAM.SFO";
  const sfo = files.find(f => f.webkitRelativePath === sfoPath);
  const title = sfo ? await sfo.text() : "";
  return title.includes("(ST51)") ? "Kimidori" : title.includes("(ST41)") ? "Momoiro" : null;
};

// Returns { error, files, bytes, folder, other }: error "no-game" when the folder doesn't hold that game (other: the
// older game it is instead, if it can tell); folder is the picked folder's name.
export async function pick(input, game, withMods) {
  try {
    const files = [...input.files];
    input.value = "";
    const name = files[0]?.webkitRelativePath.split("/")[0] ?? "";
    const res = await authFetch(`api/Extract/Inputs?game=${game}`);
    if (!res.ok) return { error: await message(res), files: 0, bytes: 0, folder: name };
    const list = await res.json();
    const where = locate(game, files.map(f => f.webkitRelativePath));
    if (!where) return { error: "no-game", files: 0, bytes: 0, folder: name };
    if (game !== "Nijiiro") {
      const other = await olderGameOf(where.root, files);
      if (other && other !== game) return { error: "no-game", files: 0, bytes: 0, folder: name, other };
    }

    const sets = withMods && list.mods ? [list, list.mods] : [list];
    // false: send it; true: only its name matters (movies, models); null: extract doesn't read it.
    // As ExtractRunner.Match: a * segment matches any one folder name.
    const covers = (rule, path, namesOnly) => {
      const r = rule.split("/"), p = path.split("/"), folder = rule.endsWith("/");
      if (namesOnly || !folder ? p.length !== r.length : p.length < r.length) return false;
      return r.slice(0, folder ? -1 : undefined).every((s, i) => s === "*" || s === p[i]);
    };
    const match = path => {
      for (const set of sets) {
        if (set.files.some(r => covers(r, path, false))) return false;
        if (set.names.some(r => covers(r, path, true))) return true;
      }
      return null;
    };
    const queue = [];
    let bytes = 0;
    for (const f of files) {
      const path = where.relative(f.webkitRelativePath);
      const namesOnly = path && match(path);
      if (namesOnly === null || namesOnly === undefined || !path) continue;
      const body = namesOnly ? new Blob([]) : f;
      queue.push({ path, body });
      bytes += body.size;
    }
    picked.set(game, queue);
    const folder = (where.root || files[0]?.webkitRelativePath.split("/")[0] + "/").replace(/\/$/, "");
    return { error: null, files: queue.length, bytes, folder };
  } catch (e) {
    return { error: e.message || String(e), files: 0, bytes: 0, folder: "" };
  }
}

// Sends a picked game's files (replacing what was sent for it before). Returns { error, mods }: mods is whether any
// Data_mods file was sent (Nijiiro); error "stopped" after stop().
export async function send(game, progress) {
  const controller = sending = new AbortController();
  const signal = controller.signal;
  try {
    const queue = picked.get(game);
    if (!queue) return { error: "no-game", mods: false };
    const total = queue.reduce((n, item) => n + item.body.size, 0);
    let res = await authFetch(`api/Extract/Upload?game=${game}`, { method: "DELETE" });
    if (!res.ok) return { error: await message(res), mods: false };
    let done = 0, batch = [], size = 0;
    const flush = async () => {
      if (!batch.length) return;
      const form = new FormData();
      for (const b of batch) form.append(b.path, b.body, "file");
      const r = await authFetch(`api/Extract/Upload?game=${game}`, { method: "POST", body: form, signal });
      if (!r.ok) throw new Error(await message(r));
      done += size;
      batch = [];
      size = 0;
      await progress.invokeMethodAsync("Progress", done, total);
    };
    await progress.invokeMethodAsync("Progress", 0, total);
    for (const item of queue) {
      if (batch.length && (size + item.body.size > BatchBytes || batch.length >= BatchFiles)) await flush();
      batch.push(item);
      size += item.body.size;
    }
    await flush();
    picked.delete(game);
    return { error: null, mods: queue.some(item => item.path.startsWith("Data_mods/")) };
  } catch (e) {
    if (!signal.aborted) return { error: e.message || String(e), mods: false };
    // What was sent is of no use on its own: the server drops it (the picked files stay, to send again).
    await authFetch(`api/Extract/Upload?game=${game}`, { method: "DELETE" }).catch(() => { });
    return { error: "stopped", mods: false };
  } finally {
    if (sending === controller) sending = null;
  }
}

// Stops the send in progress, if any: send() then returns error "stopped".
export function stop() {
  sending?.abort();
}
