// TaikoWebUI browser helpers. Loaded in <head> so the theme applies before first paint.
(() => {
  const store = {
    get: k => { try { return localStorage.getItem(k); } catch { return null; } },
    set: (k, v) => { try { localStorage.setItem(k, v); } catch { } },
  };

  // ---- Theme: dark mode (defaults to the OS setting) + accent colour ----
  const root = document.documentElement;
  const dark = store.get("ui.dark");
  root.classList.toggle("dark", dark === null ? matchMedia("(prefers-color-scheme: dark)").matches : dark === "1");
  root.dataset.accent = store.get("ui.accent") || "don";

  window.blazorCulture = {
    get: () => store.get("BlazorCulture"),
    set: v => store.set("BlazorCulture", v),
  };

  // Game assets (api/Assets) need the user's bearer token; Blazored.LocalStorage stores it JSON-encoded.
  const authFetch = (url, init) => {
    let token = null;
    try { token = JSON.parse(localStorage.getItem("authToken")); } catch { }
    return fetch(url, token ? { ...init, headers: { ...init?.headers, Authorization: `Bearer ${token}` } } : init);
  };

  // ---- Game text: the game's glyph pages (extract: game_data/font/<sheet>/<page>.{png,json}) ----
  // Pages hold 256 code points. Missing glyphs fall back along the sheet chain, then Zen Maru Gothic.
  // The nameplate's face is the game's *_30 sheets (Lumen font slot 1; see tools/extract/src/font.rs).
  const GT_LANG = ["jp_30", "en_30", "tw_30", "kr_30", "tw_30"];   // MyDonNameLanguage → sheet (no Simplified sheet)
  const GT_CHAIN = ["jp_30", "tw_30", "kr_30", "en_30"];
  const gtCache = new Map(), gtPages = new Map();
  // extract's pages.json lists a sheet's pages; without it (older extract) every page is tried.
  const gtHas = (sheet, p) => {
    if (!gtPages.has(sheet)) gtPages.set(sheet, authFetch(`api/Assets/font/${sheet}/pages.json`)
      .then(r => r.ok ? r.json() : null).then(a => a && new Set(a)).catch(() => null));
    return gtPages.get(sheet).then(set => !set || set.has(p));
  };
  const gtPage = async (sheet, p) => {
    if (!await gtHas(sheet, p)) return null;
    const key = `${sheet}/${p}`;
    if (!gtCache.has(key)) gtCache.set(key, authFetch(`api/Assets/font/${key}.json`)
      .then(r => r.ok ? r.json() : null).catch(() => null)
      .then(json => json && authFetch(`api/Assets/font/${key}.png`).then(r => r.ok ? r.blob() : null)
        .then(b => b && createImageBitmap(b)).then(img => img && { ...json, img }).catch(() => null)));
    return gtCache.get(key);
  };
  // unit = CSS px per game layout unit. Text is centred in the canvas and squeezed horizontally if too wide.
  async function gameText(cv, { text, lang, size, fill, outline = 0, unit }) {
    const seq = cv._seq = (cv._seq || 0) + 1;
    const order = [GT_LANG[lang] ?? GT_LANG[0], ...GT_CHAIN.filter(s => s !== GT_LANG[lang])];
    const runs = [];
    for (const ch of text) {
      const cp = ch.codePointAt(0); let hit = null;
      for (const s of order) { const pg = await gtPage(s, cp >> 8); if (pg?.glyphs[cp]) { hit = { pg, g: pg.glyphs[cp] }; break; } }
      runs.push(hit || { ch });
    }
    if (runs.some(r => r.ch)) await document.fonts.load(`900 ${size}px "Zen Maru Gothic"`).catch(() => { });
    if (seq !== cv._seq || !cv.clientWidth || !cv.clientHeight) return;
    const dpr = devicePixelRatio || 1, k = unit * dpr;
    cv.width = Math.round(cv.clientWidth * dpr); cv.height = Math.round(cv.clientHeight * dpr);
    const mask = new OffscreenCanvas(cv.width, cv.height), m = mask.getContext("2d");
    m.font = `900 ${size}px "Zen Maru Gothic"`;
    for (const r of runs) r.w = r.g ? r.g[2] * size / r.pg.point : m.measureText(r.ch).width;
    const total = runs.reduce((a, r) => a + r.w, 0) || 1, room = cv.width / k - 2 * outline, sx = Math.min(1, room / total);
    const lineH = size * 1.3, y0 = (cv.height / k - lineH) / 2;
    m.setTransform(k * sx, 0, 0, k, (cv.width - total * k * sx) / 2, 0);
    let x = 0;
    for (const r of runs) {
      if (r.g) { const s = size / r.pg.point; m.drawImage(r.pg.img, r.g[0], r.g[1], r.g[2], r.g[3], x, y0, r.g[2] * s, r.g[3] * s); }
      else { m.textBaseline = "middle"; m.fillText(r.ch, x, y0 + lineH / 2); }
      x += r.w;
    }
    m.setTransform(1, 0, 0, 1, 0, 0); m.globalCompositeOperation = "source-in"; m.fillStyle = fill; m.fillRect(0, 0, cv.width, cv.height);
    const c = cv.getContext("2d"); c.clearRect(0, 0, cv.width, cv.height);
    if (outline) {   // stamp the mask around a circle, then paint it black
      const o = new OffscreenCanvas(cv.width, cv.height), oc = o.getContext("2d"), rad = outline * k;
      for (let a = 0; a < 32; a++) oc.drawImage(mask, Math.cos(a * Math.PI / 16) * rad, Math.sin(a * Math.PI / 16) * rad);
      oc.globalCompositeOperation = "source-in"; oc.fillStyle = "#000"; oc.fillRect(0, 0, cv.width, cv.height);
      c.drawImage(o, 0, 0);
    }
    c.drawImage(mask, 0, 0);
  }

  // ---- Nameplate: the game's whole nameplate (Lumen "nameplate" set), one animated player per page ----
  // One player = one WebGL context; its canvas moves into whichever slot is rendered (moving keeps the context).
  const NP_W = 352;   // nameplate set bounds −172…180 stage units
  let scenePromise = null, player = null;
  const plateCanvas = Object.assign(document.createElement("canvas"), { className: "absolute left-0 top-0" });
  const loadScene = () => scenePromise ??= import("./lumen-player.js")
    // Absolute URL: the player resolves the atlas against location.href, which ignores <base href>.
    .then(m => m.LumenScene.load(new URL("api/Assets/nameplate/nameplate.scene.json", document.baseURI).href, { fetch: authFetch }))
    .catch(err => { console.info("Animated nameplate unavailable, using the plain one:", err.message); return null; });

  // Blazor calls these after each render; a ResizeObserver re-runs them with the last arguments when the
  // element changes size (window resize, sidebar, breakpoints), since canvases don't follow their box on their own.
  const lastArgs = new WeakMap();
  const resizer = new ResizeObserver(entries => {
    for (const { target } of entries) { const a = lastArgs.get(target); if (a) a.fn(...a.args); }
  });
  const onResize = (el, fn, args) => {
    if (!el) return;
    if (!lastArgs.has(el)) resizer.observe(el);
    lastArgs.set(el, { fn, args });
  };

  async function renderNameplate(el, o) {
    onResize(el, renderNameplate, [el, o]);
    hydrateAssets();   // the static fallback's plate / dan PNGs
    const unit = el.clientWidth / NP_W;
    el.querySelectorAll("canvas[data-gt]").forEach(cv => gameText(cv, cv.dataset.gt === "title"
      ? { text: o.title, lang: o.lang, size: 22, fill: "#000", unit }
      : { text: o.name, lang: o.lang, size: 24, fill: "#fff", outline: 3, unit }));
    const scene = await loadScene(), slot = el.querySelector("[data-plate-slot]");
    if (!scene || !slot?.clientWidth) return;
    const scale = slot.clientWidth / NP_W, key = JSON.stringify(o.children);
    try {
      if (!player) player = scene.player(plateCanvas, { set: "nameplate", label: o.layout, children: o.children, scale });
      else if (player.label !== o.layout) { player.scale = scale; player.children = o.children; player.setLabel(o.layout); }
      else {
        if (Math.abs(player.scale - scale) > 0.002) player.setScale(scale);
        if (JSON.stringify(player.children) !== key) player.setChildren(o.children);
      }
    } catch (err) { console.info("No WebGL, using the plain nameplate:", err.message); scenePromise = Promise.resolve(null); return; }
    // The layout is anchored at (−172, −62); a build whose nameplate starts elsewhere (ASB61: y −67) shifts the canvas.
    const [bx, by] = scene.data.sets.nameplate.bounds;
    plateCanvas.style.left = `${(bx + 172) * scale}px`;
    plateCanvas.style.top = `${(by + 62) * scale}px`;
    slot.replaceChildren(plateCanvas);
    el.dataset.live = "1";   // hides the plain fallback plate
  }

  // ---- 3D don-chan embed (display-only iframe; protocol in docs/webui-design-decisions.md) ----
  // The embed listens only once mounted, so the last state is resent when it first reports in.
  const donLast = new Map();
  const donSend = (frame, msg) => {
    if (!frame) return;
    donLast.set(frame, msg);
    try { frame.contentWindow?.postMessage(msg, new URL(frame.src).origin); } catch { }
  };
  addEventListener("message", e => {
    if (e.data?.type !== "don-status") return;
    for (const [frame, msg] of donLast) {
      if (!frame.isConnected) { donLast.delete(frame); continue; }
      if (frame.contentWindow !== e.source) continue;
      if (!frame._ready) { frame._ready = true; donSend(frame, msg); }
      frame._loading = e.data.loading || !!e.data.error;
      avatarRun(frame);
    }
  });

  // <img data-asset="nameplate/dan_10dan_gold.png">: api/Assets needs the token, which <img src> can't send,
  // so fetch once and use a blob URL. A missing asset hides the element (any element: a button can depend on one).
  // <img data-avatar="api/Users/N/Avatar?v=key"> works the same way (see avatar below).
  const blobUrls = new Map();
  const blobUrl = url => {
    if (!blobUrls.has(url)) blobUrls.set(url, authFetch(url)
      .then(r => r.ok ? r.blob() : null).then(b => b && URL.createObjectURL(b)).catch(() => null));
    return blobUrls.get(url);
  };
  const assetUrl = path => blobUrl(`api/Assets/${path}`);
  const hydrate = (attr, toUrl) => document.querySelectorAll(`[data-${attr}]`).forEach(el => {
    const path = el.dataset[attr];
    if (el.dataset.loaded === path) return;
    el.dataset.loaded = path;
    toUrl(path).then(url => { if (el.dataset[attr] !== path) return; el.hidden = !url; if (url && el.tagName === "IMG") el.src = url; });
  });
  const hydrateAssets = () => { hydrate("asset", assetUrl); hydrate("avatar", blobUrl); };

  // Avatar: when the card's costume + pose (the key, CardStyle.AvatarKey) has no stored image, the embed renders a
  // transparent face crop off-screen once the model has loaded, and it's uploaded as WebP. The circle colour is CSS.
  const avatarSrc = (baid, key) => `api/Users/${baid}/Avatar?v=${encodeURIComponent(key)}`;
  const avatar = (frame, baid, key) => {
    if (!frame || frame._avatarKey === key) return;
    frame._avatarKey = key;
    clearTimeout(frame._avatarTimer);
    frame._avatar = null;
    // Debounced like the old Profile's: stepping through parts shouldn't upload each one.
    frame._avatarTimer = setTimeout(async () => {
      if (frame._avatarKey !== key || await blobUrl(avatarSrc(baid, key))) return;
      frame._avatar = { baid, key };
      avatarRun(frame);
    }, 400);
  };
  const avatarRun = async frame => {
    const job = frame._avatar;
    if (!job || frame._loading !== false) return;
    frame._avatar = null;
    const dataUrl = await new Promise(resolve => {
      const onMsg = e => { if (e.source === frame.contentWindow && e.data?.type === "don-snapshot") { removeEventListener("message", onMsg); resolve(e.data.dataUrl); } };
      addEventListener("message", onMsg);
      // Face centred (camX), without the puchi. Changing the framing means bumping CardStyle.AvatarKey's version.
      frame.contentWindow.postMessage({ type: "don-snapshot", zoom: 0.14, camY: 0.165, camX: 0.04, puchi: false, size: 256 }, new URL(frame.src).origin);
    });
    if (frame._avatarKey !== job.key) return;   // the costume changed while rendering; the newer job uploads
    const img = new Image();
    img.src = dataUrl;
    await img.decode();
    const canvas = document.createElement("canvas");
    canvas.width = img.width; canvas.height = img.height;
    canvas.getContext("2d").drawImage(img, 0, 0);
    // Browsers without WebP encoding hand back PNG, which the server also takes.
    const blob = await new Promise(resolve => canvas.toBlob(resolve, "image/webp", 0.9));
    const src = avatarSrc(job.baid, job.key);
    const r = await authFetch(`api/Users/${job.baid}/Avatar?key=${encodeURIComponent(job.key)}`, { method: "PUT", body: blob }).catch(() => null);
    if (!r?.ok) return;
    blobUrls.set(src, Promise.resolve(URL.createObjectURL(blob)));
    document.querySelectorAll("[data-avatar]").forEach(el => { if (el.dataset.avatar === src) delete el.dataset.loaded; });
    hydrateAssets();
  };

  // Tone preview: the tone's don, then its ka (extract writes tone/NN_{don,ka}.wav).
  const playTone = async id => {
    const n = String(id).padStart(2, "0");
    const [don, ka] = await Promise.all([assetUrl(`tone/${n}_don.wav`), assetUrl(`tone/${n}_ka.wav`)]);
    if (don) new Audio(don).play();
    if (ka) setTimeout(() => new Audio(ka).play(), 350);
  };

  // The 3D canvas covers the whole card; `anchor` is the box (in px, relative to the iframe) that zoom/camY frame.
  const donFramed = (frame, anchorEl, msg) => {
    onResize(frame, donFramed, [frame, anchorEl, msg]);
    onResize(anchorEl, donFramed, [frame, anchorEl, msg]);
    const f = frame?.getBoundingClientRect(), a = anchorEl?.getBoundingClientRect();
    const anchor = f && a ? [a.left - f.left + a.width / 2, a.top - f.top + a.height / 2, a.height].map(Math.round) : null;
    donSend(frame, { type: "don", ...msg, anchor });
  };

  window.ui = {
    setDark: on => { root.classList.toggle("dark", on); store.set("ui.dark", on ? "1" : "0"); },
    isDark: () => root.classList.contains("dark"),
    setAccent: id => { root.dataset.accent = id; store.set("ui.accent", id); },
    getAccent: () => root.dataset.accent,
    copy: text => navigator.clipboard?.writeText(text).then(() => true, () => false) ?? false,
    // Live log: it follows new lines only while the reader is at the bottom.
    atBottom: el => !el || el.scrollHeight - el.scrollTop - el.clientHeight < 40,
    scrollToEnd: el => { if (el) el.scrollTop = el.scrollHeight; },
    saveText: (name, text) => {
      const a = Object.assign(document.createElement("a"), { href: URL.createObjectURL(new Blob([text], { type: "text/plain" })), download: name });
      a.click();
      setTimeout(() => URL.revokeObjectURL(a.href), 1000);
    },
    nameplate: renderNameplate,
    don: donFramed,
    hydrateAssets,
    avatar,
    playTone,
  };
})();
