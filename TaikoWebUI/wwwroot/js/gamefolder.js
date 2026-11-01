// "Choose game folder" (ExtractPanel, with AllowRemoteGameDir): sends only the game files extract reads, from a folder
// picked in the browser, so the server can run extract when the game isn't on its machine.

const authFetch = (url, init) => {
  let token = null;
  try { token = JSON.parse(localStorage.getItem("authToken")); } catch { }
  return fetch(url, token ? { ...init, headers: { ...init?.headers, Authorization: `Bearer ${token}` } } : init);
};
const message = async res => (await res.json().catch(() => null))?.message || `HTTP ${res.status}`;

// Batches stay under the server's 128 MB request limit (a bigger file goes alone) and its form field limit.
const BatchBytes = 64 * 1024 * 1024, BatchFiles = 500;

// Returns { error, mods }: error "no-game" when the folder has no Data/x64; mods is whether any Data_mods file was sent.
export async function send(input, withMods, progress) {
  try {
    const files = [...input.files];
    input.value = "";
    let res = await authFetch("api/Extract/Inputs");
    if (!res.ok) return { error: await message(res), mods: false };
    const list = await res.json();

    // The pick may be the game folder, a folder above it, Data, or x64 itself: find x64 by its music table.
    const key = files.map(f => f.webkitRelativePath).find(p => /(^|\/)x64\/datatable\/musicinfo\.bin$/.test(p));
    if (!key) return { error: "no-game", mods: false };
    const x64 = key.slice(0, -"datatable/musicinfo.bin".length);
    const game = x64.endsWith("Data/x64/") ? x64.slice(0, -"Data/x64/".length) : null;
    // A path relative to the game folder (Data/x64/…, Data_mods/…), or null for files outside it.
    const relative = p => game !== null ? (p.startsWith(game) ? p.slice(game.length) : null)
      : p.startsWith(x64) ? "Data/x64/" + p.slice(x64.length) : null;

    const sets = withMods && list.mods ? [list, list.mods] : [list];
    // false: send it; true: only its name matters (movies); null: extract doesn't read it.
    const match = path => {
      for (const set of sets) {
        if (set.files.some(r => r.endsWith("/") ? path.startsWith(r) : path === r)) return false;
        if (set.names.some(r => path.startsWith(r) && !path.slice(r.length).includes("/"))) return true;
      }
      return null;
    };
    const queue = [];
    let total = 0, mods = false;
    for (const f of files) {
      const path = relative(f.webkitRelativePath);
      const namesOnly = path && match(path);
      if (namesOnly === null || namesOnly === undefined || !path) continue;
      const body = namesOnly ? new Blob([]) : f;
      queue.push({ path, body });
      total += body.size;
      mods ||= path.startsWith("Data_mods/");
    }

    res = await authFetch("api/Extract/Upload", { method: "DELETE" });
    if (!res.ok) return { error: await message(res), mods };
    let done = 0, batch = [], size = 0;
    const flush = async () => {
      if (!batch.length) return;
      const form = new FormData();
      for (const b of batch) form.append(b.path, b.body, "file");
      const r = await authFetch("api/Extract/Upload", { method: "POST", body: form });
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
    return { error: null, mods };
  } catch (e) {
    return { error: e.message || String(e), mods: false };
  }
}
