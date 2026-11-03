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

  // three.js for don-chan in 3D (don-viewer.js): its addons import "three". Added here, before any module loads, rather
  // than in index.html, which a browser may still have cached from before an update.
  document.head.append(Object.assign(document.createElement("script"),
    { type: "importmap", textContent: JSON.stringify({ imports: { three: new URL("lib/three/three.module.min.js", document.baseURI).href } }) }));

  // ---- The don note from the game (extract; api/Brand/Icon) as the favicon and logo, when it's there ----
  // The logo's <img> shows through CSS (html[data-brand-icon]), so Blazor's renders don't fight it.
  const brand = new Image();
  brand.onload = () => {
    root.dataset.brandIcon = "";
    const link = document.getElementById("favicon");
    if (link) { link.type = "image/png"; link.href = brand.src; }
  };
  brand.src = "api/Brand/Icon";

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
  // The nameplate's face is the game's *_30 sheets (Lumen font slot 1; see Infrastructure.Extract/README.md).
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
    const texts = [...el.querySelectorAll("canvas[data-gt]")].map(cv => gameText(cv, cv.dataset.gt === "title"
      ? { text: o.title, lang: o.lang, size: 22, fill: "#000", unit }
      : { text: o.name, lang: o.lang, size: 24, fill: "#fff", outline: 3, unit }));
    try {
      await buildNameplate(el, o);
      await Promise.allSettled(texts);
    } finally {
      // Built (or fallen back to the plain plate): fade it in, once; a later render leaves it shown. A timer, not a
      // frame: frames stop in a hidden tab, and the fade covers the player's first frame.
      if (!el.dataset.ready) setTimeout(() => { el.dataset.ready = "1"; }, 50);
    }
  }

  async function buildNameplate(el, o) {
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

  // ---- 3D don-chan (don-viewer.js, three.js), loaded on first use like the nameplate player ----
  let donModule = null;
  const loadDon = () => donModule ??= import("./don-viewer.js")
    .catch(err => { console.info("3D don-chan unavailable:", err.message); return null; });

  // <img data-asset="nameplate/dan_10dan_gold.png">: api/Assets needs the token, which <img src> can't send,
  // so fetch once and use a blob URL. A missing asset hides the element (any element: a button can depend on one).
  const blobUrls = new Map();
  const blobUrl = url => {
    if (!blobUrls.has(url)) {
      const found = authFetch(url).then(r => r.ok ? r.blob() : null).then(b => b && URL.createObjectURL(b)).catch(() => null);
      blobUrls.set(url, found);
      // A miss is asked again on the next page.
      found.then(u => { if (!u) blobUrls.delete(url); });
    }
    return blobUrls.get(url);
  };
  const assetUrl = path => blobUrl(`api/Assets/${path}`);
  const hydrate = (attr, toUrl) => document.querySelectorAll(`[data-${attr}]`).forEach(el => {
    const path = el.dataset[attr];
    if (el.dataset.loaded === path) return;
    el.dataset.loaded = path;
    // data-asset-fallback: another asset to show when this one is missing (e.g. large art from a newer extract).
    toUrl(path).then(url => url || (el.dataset.assetFallback ? toUrl(el.dataset.assetFallback) : null))
      .then(url => { if (el.dataset[attr] !== path) return; el.hidden = !url; if (url && el.tagName === "IMG") el.src = url; });
  });
  const hydrateAssets = () => hydrate("asset", assetUrl);

  // Tone preview: the tone's don, then its ka (extract writes tone/NN_{don,ka}.wav).
  const playTone = async id => {
    const n = String(id).padStart(2, "0");
    const [don, ka] = await Promise.all([assetUrl(`tone/${n}_don.wav`), assetUrl(`tone/${n}_ka.wav`)]);
    if (don) new Audio(don).play();
    if (ka) setTimeout(() => new Audio(ka).play(), 350);
  };

  // Draws don-chan in `el` (an element or a selector). The canvas covers the whole of it; `anchorEl`, when given, is the
  // box zoom/camY frame, so don-chan keeps his size and place in the card.
  const donFramed = async (el, anchorEl, state) => {
    if (typeof el === "string") el = document.querySelector(el);
    if (!el) return;
    onResize(el, donFramed, [el, anchorEl, state]);
    onResize(anchorEl, donFramed, [el, anchorEl, state]);
    const m = await loadDon();
    if (!m || !el.isConnected) return;
    const f = el.getBoundingClientRect(), a = anchorEl?.getBoundingClientRect();
    const anchor = a ? [a.left - f.left + a.width / 2, a.top - f.top + a.height / 2, a.height].map(Math.round) : null;
    (el._don ??= m.mount(el)).set({ ...state, anchor });
  };

  // Tooltips for dense grids (heatmaps, calendars): an element's data-tip shows at once above it, on hover or tap.
  // A native title waits a second and never shows on touch. The element keeps its own aria-label for screen readers.
  const tip = document.createElement("div");
  tip.className = "pointer-events-none fixed z-50 -translate-x-1/2 -translate-y-full whitespace-nowrap rounded-lg bg-zinc-950/90 px-2 py-1 text-xs text-white shadow-lg";
  tip.setAttribute("aria-hidden", "true");
  tip.hidden = true;
  const showTip = e => {
    const el = e.target.closest?.("[data-tip]");
    if (!el) { tip.hidden = true; return; }
    if (!tip.isConnected) document.body.append(tip);   // this script runs in <head>, before there's a body
    const r = el.getBoundingClientRect();
    tip.textContent = el.dataset.tip;
    tip.hidden = false;
    // Kept inside the window: centred over the element, clamped to the edges.
    const half = tip.offsetWidth / 2, x = Math.min(innerWidth - half - 4, Math.max(half + 4, r.left + r.width / 2));
    tip.style.left = `${x}px`;
    tip.style.top = `${r.top - 6}px`;
  };
  addEventListener("pointerover", showTip);
  addEventListener("pointerdown", showTip);
  addEventListener("scroll", () => { tip.hidden = true; }, true);

  // Phone menu (MainLayout): Escape closes it, and so does growing past lg, where the sidebar is always shown.
  const closeDrawer = () => document.querySelector("[data-drawer-close]")?.click();
  addEventListener("keydown", e => { if (e.key === "Escape") closeDrawer(); });
  matchMedia("(min-width: 1024px)").addEventListener("change", m => { if (m.matches) closeDrawer(); });

  // Rails (Rail.razor): sideways rows of chips or tabs. When the selected item changes it's centred (so 5th Dan stays
  // in view); arrows show while the row overflows, each disabled at its end; a mouse can drag the row.
  const railTrack = rail => rail.querySelector(":scope > div");
  const updateRail = rail => {
    const t = railTrack(rail), [prev, next] = rail.querySelectorAll(":scope > button");
    if (!t || !next) return;
    prev.hidden = next.hidden = t.scrollWidth <= t.clientWidth + 1;
    prev.disabled = t.scrollLeft <= 1;
    next.disabled = t.scrollLeft >= t.scrollWidth - t.clientWidth - 1;
  };
  // A rail's size, or one of its items' (the window, a sidebar, a web font loading, a count filling in), changed:
  // recheck its arrows and re-centre the selection. Both observers batch their callbacks, so no frame scheduling.
  const railSize = new ResizeObserver(entries => {
    entries.forEach(e => e.target.closest("[data-rail] > div")._sel = null);
    updateRails();
  });
  document.fonts?.addEventListener("loadingdone", () => updateRails(true));
  const updateRails = (recenter = false) => document.querySelectorAll("[data-rail]").forEach(rail => {
    const t = railTrack(rail);
    if (!t) return;
    for (const el of [t, ...t.children]) if (!el._sized) { el._sized = true; railSize.observe(el); }
    if (recenter) t._sel = null;
    // Arrows first: showing them narrows the row, which would move a selection centred before it.
    updateRail(rail);
    const sel = t.querySelector('[aria-current="page"],[aria-current="true"],[aria-pressed="true"],[aria-selected="true"]');
    // Also when the row's content width changed (labels filling in), which can push the selection out of view.
    if (sel && (t._sel !== sel || t._sw !== t.scrollWidth) && t.scrollWidth > t.clientWidth) {
      t._sel = sel;
      t._sw = t.scrollWidth;
      const r = t.getBoundingClientRect(), s = sel.getBoundingClientRect();
      t.scrollLeft += s.left + s.width / 2 - (r.left + r.width / 2);
    }
    updateRail(rail);
  });
  addEventListener("scroll", e => { const rail = e.target.parentElement?.closest?.("[data-rail]"); if (rail) updateRail(rail); }, true);
  addEventListener("click", e => {
    const step = e.target.closest?.("[data-rail-step]");
    if (!step) return;
    const t = railTrack(step.closest("[data-rail]"));
    t.scrollBy({ left: Math.sign(+step.dataset.railStep) * t.clientWidth * 0.75, behavior: "smooth" });
  });
  let drag = null;
  addEventListener("pointerdown", e => {
    if (e.pointerType !== "mouse" || e.button !== 0) return;
    const t = e.target.closest?.("[data-rail] > div");
    if (t && t.scrollWidth > t.clientWidth) drag = { t, x: e.clientX, left: t.scrollLeft, moved: false };
  });
  addEventListener("pointermove", e => {
    if (!drag) return;
    const dx = e.clientX - drag.x;
    if (!drag.moved && Math.abs(dx) < 5) return;
    if (!drag.moved) { drag.moved = true; drag.t.style.scrollSnapType = "none"; drag.t.style.cursor = "grabbing"; }
    drag.t.scrollLeft = drag.left - dx;
  });
  const endDrag = () => {
    const d = drag;
    drag = null;
    if (!d?.moved) return;
    d.t.style.scrollSnapType = d.t.style.cursor = "";
    // A drag isn't a click on the chip it ends on (that click, if any, comes right after pointerup).
    const eat = e => { e.stopPropagation(); e.preventDefault(); };
    addEventListener("click", eat, { capture: true, once: true });
    setTimeout(() => removeEventListener("click", eat, { capture: true }));
  };
  addEventListener("pointerup", endDrag);
  addEventListener("pointercancel", endDrag);
  new MutationObserver(() => updateRails())
    .observe(document.documentElement, { childList: true, subtree: true, characterData: true, attributes: true, attributeFilter: ["aria-current", "aria-pressed", "aria-selected"] });

  window.ui = {
    setDark: on => { root.classList.toggle("dark", on); store.set("ui.dark", on ? "1" : "0"); },
    isDark: () => root.classList.contains("dark"),
    setAccent: id => { root.dataset.accent = id; store.set("ui.accent", id); },
    // The 3D don-chan's quality on player cards (don-viewer.js reads it): low, standard (the default) or high.
    getQuality3D: () => store.get("ui.quality3d") || "standard",
    setQuality3D: q => store.set("ui.quality3d", q),
    getAccent: () => root.dataset.accent,
    focusIn: el => el?.querySelector("input, button")?.focus(),
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
    playTone,
  };
})();
