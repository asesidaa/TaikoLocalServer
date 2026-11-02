// don-chan in 3D: three.js on a canvas inside a page element (PlayerCard through ui.don). Game files aren't public: every part is fetched from api/Assets with the WebUI's
// login token and handed to three.js as a blob: URL. The addons import "three" through the page's import map, which
// names the same file as the relative imports here, so there's one three.js.
import * as THREE from "../lib/three/three.module.min.js";
import { GLTFLoader } from "../lib/three/addons/loaders/GLTFLoader.js";
import { clone as cloneSkinned } from "../lib/three/addons/utils/SkeletonUtils.js";
import { disposePart, normalizePart } from "./don-normalize.js";

// The game's colour ids.
const PALETTE = ["#F84828", "#68C0C0", "#DC1500", "#F8F0E0", "#009687", "#00BF87", "#00FF9A", "#66FFC2", "#FFFFFF", "#690000",
  "#FF0000", "#FF6666", "#FFB3B3", "#00BCC2", "#00F7FF", "#66FAFF", "#B3FDFF", "#E4E4E4", "#993800", "#FF5E00", "#FF9E78",
  "#FFCFB3", "#005199", "#0088FF", "#66B8FF", "#B3DBFF", "#B9B9B9", "#B37700", "#FFAA00", "#FFCC66", "#FFE2B3", "#000C80",
  "#0019FF", "#6675FF", "#B3BAFF", "#858585", "#B39B00", "#FFDD00", "#FFFF00", "#FFFF71", "#2B0080", "#5500FF", "#9966FF",
  "#CCB3FF", "#505050", "#38A100", "#78C900", "#B3FF00", "#DCFF8A", "#610080", "#C400FF", "#DC66FF", "#EDB3FF", "#232323",
  "#006600", "#00B800", "#00FF00", "#8AFF9E", "#990059", "#FF0095", "#FF66BF", "#FFB3DF", "#000000"];

/**
 * What set() takes; anything left out keeps its value. zoom/camY frame don-chan (smaller zoom = bigger), anchor is
 * [cx, cy, h] in px inside the element: the box zoom/camY frame, so don-chan keeps his size and place while the canvas
 * covers the whole card. cosFile is an exact kigurumi variant (e.g. "036014", the Dani Dojo outfit per rank) and wins
 * over kigurumi. quality: low = 1x, standard = the screen's own, high = at least 2x; unset = the player's choice
 * (Settings › Display).
 */
const DEFAULTS = {
  body: 0, head: 0, kigurumi: 0, face: 0, puchi: 0, bodyColor: 1, faceColor: 0, limbColor: 3,
  anim: "don_result_full_loop", zoom: 0.34, camY: 0.27, anchor: null, cosFile: "", quality: null,
};
const PARTS = ["body", "head", "kigurumi", "face", "puchi", "cosFile"];
const OUTLINE = { visible: true, thickness: 0.0025 };
const CAM_X = -0.017, CAM_YAW = THREE.MathUtils.degToRad(-29);
const PUCHI_Y = 0.115;

const pad = id => String(id * 1000).padStart(6, "0");
const stored = key => { try { return localStorage.getItem(key); } catch { return null; } };
const token = () => { try { return JSON.parse(stored("authToken") ?? "null"); } catch { return null; } };
const reducedMotion = matchMedia("(prefers-reduced-motion: reduce)");

/** Draws don-chan into `el` (which should be positioned) until `el` leaves the page. */
export function mount(el) {
  const api = new URL("api/Assets/", document.baseURI).href;
  const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true, powerPreference: "high-performance" });
  renderer.outputColorSpace = THREE.SRGBColorSpace;
  // ACES: what @react-three/fiber used, so the cards look as they did.
  renderer.toneMapping = THREE.ACESFilmicToneMapping;
  renderer.setClearColor(0x000000, 0);
  const canvas = renderer.domElement;
  Object.assign(canvas.style, { position: "absolute", inset: "0", width: "100%", height: "100%", opacity: "0" });
  const spin = spinner();
  el.append(canvas, spin);

  const scene = new THREE.Scene();
  const camera = new THREE.OrthographicCamera(-1, 1, 1, -1, -1, 1000);
  camera.rotation.set(0, CAM_YAW, 0, "XYZ");
  const key = new THREE.DirectionalLight(0xffffff, 2.2), fill = new THREE.DirectionalLight(0xffffff, 0.8);
  key.position.set(120, 260, 200);
  fill.position.set(-160, 80, -120);
  scene.add(new THREE.AmbientLight(0xffffff, 2.2), new THREE.HemisphereLight(0xffffff, 0xb8c0cc, 2.4), key, fill);

  let state = { ...DEFAULTS }, model = null, seq = 0, status = { loading: true, error: null };

  // ---- Assets: a blob: URL per path, kept until no model shows it (forget). A failure isn't kept, so an expired
  // token or a network blip is tried again on the next change instead of sticking.
  const blobs = new Map(), loaded = new Map();
  const asset = path => {
    let p = blobs.get(path);
    if (!p) {
      const t = token();
      p = fetch(api + path, t ? { headers: { Authorization: `Bearer ${t}` } } : undefined)
        .then(async r => (r.ok ? URL.createObjectURL(await r.blob()) : null))
        .catch(() => null);
      blobs.set(path, p);
      p.then(url => { if (!url) blobs.delete(path); });
    }
    return p;
  };
  // A costume part, falling back to part 0 when the operator's extract doesn't have that id.
  const part = async (prefix, file, ext) =>
    (await asset(`${prefix}/${prefix}_${file}.${ext}`)) ?? asset(`${prefix}/${prefix}_000000.${ext}`);
  const cached = (url, load) => {
    let p = loaded.get(url);
    if (!p) loaded.set(url, p = load(url));
    return p;
  };
  const gltfLoader = new GLTFLoader(), textureLoader = new THREE.TextureLoader();
  const gltf = url => cached(url, u => gltfLoader.loadAsync(u));
  const texture = url => cached(url, u => textureLoader.loadAsync(u));
  const json = url => cached(url, u => fetch(u).then(r => (r.ok ? r.json() : null)).catch(() => null));
  const forget = async url => {
    for (const [path, p] of blobs) if ((await p) === url) blobs.delete(path);
    const done = loaded.get(url);
    loaded.delete(url);
    done?.then(t => t?.isTexture && t.dispose(), () => { });
    URL.revokeObjectURL(url);
  };

  // ---- The model: the parts dressed by don-normalize, the puchi, and an animation mixer per shown part.
  const show = on => {
    canvas.style.transition = on && !reducedMotion.matches ? "opacity 300ms ease-out" : "none";
    canvas.style.opacity = on ? "1" : "0";
  };
  const setStatus = (loading, error = null) => {
    status = { loading, error };
    spin.hidden = !loading;
  };

  async function build(s, id) {
    const kig = s.kigurumi > 0 || !!s.cosFile;
    const cosFile = s.cosFile || (kig ? pad(s.kigurumi) : "000000");
    // A kigurumi brings its own face atlas (face_<cos id>), when the game has one.
    const urls = await Promise.all([
      part("body", pad(s.body), "glb"), part("head", pad(s.head), "glb"), part("cos", cosFile, "glb"),
      part("face", cosFile, "png"), part("paint", pad(s.face), "png"), part("acce", pad(s.puchi), "png"),
      asset("animation/don.glb"), asset("animation/facekeys.json"),
    ]);
    if (id !== seq) return;
    if (urls.some(u => !u)) return setStatus(false, "missing-assets");
    let loadedParts;
    try {
      const [body, head, cos, face, paint, acce, anim, faceKeys] = urls;
      loadedParts = await Promise.all([gltf(body), gltf(head), gltf(cos), texture(face), texture(paint), texture(acce), gltf(anim), json(faceKeys)]);
    } catch {
      if (id === seq) setStatus(false, "load-failed");
      return;
    }
    if (id !== seq) return;
    const [bodyG, headG, cosG, faceTex, paintTex, acceTex, animLib, faceKeys] = loadedParts;
    for (const t of [faceTex, paintTex]) {
      t.colorSpace = THREE.SRGBColorSpace;
      t.flipY = false;
      t.wrapS = t.wrapT = THREE.ClampToEdgeWrapping;
      t.needsUpdate = true;
    }
    // Skinned hierarchies must be cloned with SkeletonUtils, or the animation doesn't bind (T-pose).
    const roots = { body: cloneSkinned(bodyG.scene), head: cloneSkinned(headG.scene), cos: cloneSkinned(cosG.scene) };
    // The puchi: a camera-facing sprite that flips between its two atlas frames and bobs once a second.
    const atlas = acceTex.clone();
    atlas.colorSpace = THREE.SRGBColorSpace;
    atlas.wrapS = atlas.wrapT = THREE.ClampToEdgeWrapping;
    atlas.repeat.set(1, 0.5);
    atlas.offset.set(0, 0.5);
    atlas.needsUpdate = true;
    const puchi = new THREE.Mesh(new THREE.PlaneGeometry(0.2, 0.2),
      new THREE.MeshBasicMaterial({ map: atlas, transparent: true, side: THREE.DoubleSide, depthTest: false, depthWrite: false }));
    puchi.name = "puchi";
    puchi.position.set(-0.135, PUCHI_Y, 0.06);
    puchi.renderOrder = 1000;
    puchi.userData = { donOutlineOverlay: true, restY: PUCHI_Y };
    const shown = kig ? [roots.cos] : [roots.body, roots.head];
    const group = new THREE.Group();
    group.add(...shown, puchi);

    const next = { kig, roots, shown, group, faceTex, paintTex, atlas, puchi, animLib, faceKeys, urls, mixers: [], actions: [], faceTextures: [] };
    dress(next);
    bind(next);
    if (model) {
      scene.remove(model.group);
      release(model, urls);
    }
    model = next;
    scene.add(group);
    setStatus(false);
    // The first frame after a swap is the bind pose with raw materials; fade in two frames later.
    requestAnimationFrame(() => requestAnimationFrame(() => { if (model === next && !status.loading) show(true); }));
  }

  const colours = () => ({
    body: PALETTE[state.bodyColor] || PALETTE[1],
    face: PALETTE[state.faceColor] || PALETTE[0],
    rim: PALETTE[state.limbColor] || PALETTE[3],
  });

  // Materials, outlines and the face atlas copies; again on a colour change.
  function dress(m) {
    const c = colours();
    normalizePart(m.roots.body, m.paintTex, c, "body", OUTLINE, false, true);
    normalizePart(m.roots.head, m.paintTex, c, "head", OUTLINE, false, false);
    normalizePart(m.roots.cos, m.faceTex, c, "cos", OUTLINE, m.kig, false);
    // The face atlas textures the expression frame moves (the face materials' own copies).
    const textures = new Set();
    for (const root of Object.values(m.roots)) root.traverse(obj => {
      if (!obj.isMesh) return;
      for (const mat of Array.isArray(obj.material) ? obj.material : [obj.material])
        if (mat?.userData?._donFlags?.face && mat.map) textures.add(mat.map);
    });
    m.faceTextures = [...textures];
  }

  // The clip (every one loops: an idle pose), and its face keys.
  function bind(m) {
    for (const mixer of m.mixers) mixer.stopAllAction();
    m.mixers = [];
    m.actions = [];
    const data = m.faceKeys?.animations?.[state.anim];
    const keys = Array.isArray(data?.keys)
      ? data.keys.map(k => ({ time: Number(k?.time) || 0, faceIndex: Math.max(0, Math.floor(Number(k?.faceIndex) || 0)) }))
        .sort((a, b) => a.time - b.time)
      : [];
    m.keys = keys.length ? keys : [{ time: 0, faceIndex: 0 }];
    m.frameCount = Math.max(1, Math.floor(Number(data?.frameCount) || 12));
    const clip = THREE.AnimationClip.findByName(m.animLib.animations, state.anim) || m.animLib.animations[0];
    if (!clip) return;
    for (const root of m.shown) {
      const mixer = new THREE.AnimationMixer(root), action = mixer.clipAction(clip);
      action.setLoop(THREE.LoopRepeat, Infinity);
      action.play();
      m.mixers.push(mixer);
      m.actions.push(action);
    }
  }

  // The face atlas frame for the main action's time.
  function faceFrame(m) {
    const action = m.actions[0], duration = action?.getClip().duration || 0, t = action?.time || 0;
    const sample = duration > 0 ? t % duration : t;
    let index = m.keys[0].faceIndex;
    for (let i = 1; i < m.keys.length && sample >= m.keys[i].time; i++) index = m.keys[i].faceIndex;
    const frame = ((index % m.frameCount) + m.frameCount) % m.frameCount;
    for (const tex of m.faceTextures) tex.offset.y = frame / m.frameCount;
  }

  // Frees a replaced model, and the downloads the new one doesn't use.
  function release(m, keep) {
    for (const mixer of m.mixers) mixer.stopAllAction();
    for (const root of Object.values(m.roots)) disposePart(root);
    m.atlas.dispose();
    m.puchi.geometry.dispose();
    m.puchi.material.dispose();
    for (const url of m.urls) if (!keep.includes(url)) forget(url);
  }

  function load() {
    const id = ++seq;
    setStatus(true);
    show(false);
    build({ ...state }, id);
  }

  // ---- Drawing: the element's size and the chosen quality decide the canvas; the anchor decides the framing.
  let width = 0, height = 0, dpr = 0, last = performance.now();
  const start = last;
  function frame(now) {
    if (!el.isConnected) return dispose();
    const w = el.clientWidth, h = el.clientHeight;
    const quality = state.quality ?? stored("ui.quality3d") ?? "standard", screen = devicePixelRatio || 1;
    const ratio = quality === "low" ? 1 : quality === "high" ? Math.min(3, Math.max(2, screen)) : Math.min(3, Math.max(1, screen));
    if (w !== width || h !== height || ratio !== dpr) {
      [width, height, dpr] = [w, h, ratio];
      renderer.setPixelRatio(dpr);
      renderer.setSize(w, h, false);
    }
    const [cx, cy, box] = state.anchor || [w / 2, h / 2, h];
    const upp = box ? (2 * state.zoom) / box : 0;   // world units per px
    Object.assign(camera, { left: -cx * upp, right: (w - cx) * upp, top: cy * upp, bottom: -(h - cy) * upp });
    camera.position.set(CAM_X, state.camY, 0);
    camera.updateProjectionMatrix();

    // A slow frame doesn't jump the animation forward.
    const delta = Math.min((now - last) / 1000, 1 / 15), time = (now - start) / 1000;
    last = now;
    if (model) {
      for (const mixer of model.mixers) mixer.update(delta);
      faceFrame(model);
      model.atlas.offset.y = Math.floor(time) % 2 === 0 ? 0.5 : 0;
      model.puchi.position.y = PUCHI_Y + Math.sin(time * 2 * Math.PI) * 0.01;
      model.puchi.quaternion.copy(camera.quaternion);
    }
    if (w && h) renderer.render(scene, camera);
  }
  renderer.setAnimationLoop(frame);

  function dispose() {
    renderer.setAnimationLoop(null);
    seq++;
    if (model) release(model, []);
    model = null;
    for (const p of blobs.values()) p.then(url => url && URL.revokeObjectURL(url));
    renderer.dispose();
    renderer.forceContextLoss();
    canvas.remove();
    spin.remove();
    el._don = null;
  }

  return {
    /** {loading, error}: error is "missing-assets" (a part isn't extracted) or "load-failed". */
    get status() { return status; },
    set(next) {
      const prev = state;
      state = { ...state, ...next };
      if (seq === 0 || PARTS.some(k => state[k] !== prev[k])) return load();
      if (!model) return;
      if (state.bodyColor !== prev.bodyColor || state.faceColor !== prev.faceColor || state.limbColor !== prev.limbColor) dress(model);
      if (state.anim !== prev.anim) bind(model);
    },
  };
}

// White arc on a dark disc, so it reads on any card colour (40px / 24px: pixel-grid sizes).
function spinner() {
  const s = document.createElement("div");
  s.style.cssText = "position:absolute;inset:0;display:grid;place-items:center;pointer-events:none";
  s.innerHTML = '<div style="display:grid;place-items:center;width:40px;height:40px;border-radius:50%;background:rgba(0,0,0,.45)">' +
    '<div style="width:24px;height:24px;box-sizing:border-box;border-radius:50%;border:3px solid rgba(255,255,255,.3);border-top-color:#fff"></div></div>';
  s.firstChild.firstChild.animate([{ transform: "rotate(0)" }, { transform: "rotate(360deg)" }],
    { duration: reducedMotion.matches ? 2400 : 800, iterations: Infinity });
  return s;
}
