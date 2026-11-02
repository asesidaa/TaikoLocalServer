// don-chan in 3D: three.js on a canvas inside a page element (PlayerCard through ui.don). Game files aren't public: every part is fetched from api/Assets with the WebUI's
// login token and handed to three.js as a blob: URL. The addons import "three" through the page's import map, which
// names the same file as the relative imports here, so there's one three.js.
import * as THREE from "../lib/three/three.module.min.js";
import { GLTFLoader } from "../lib/three/addons/loaders/GLTFLoader.js";
import { clone as cloneSkinned } from "../lib/three/addons/utils/SkeletonUtils.js";
import { addMeshOutline, disposePart, normalizePart, setMeshOutlineParams } from "./don-normalize.js";

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
 * (Settings › Display). game: an older game ("Green"…) draws that game's own models from game_data/<game>/ (extract
 * --era); unset is Nijiiro.
 */
const DEFAULTS = {
  body: 0, head: 0, kigurumi: 0, face: 0, puchi: 0, bodyColor: 1, faceColor: 0, limbColor: 3,
  anim: "don_result_full_loop", zoom: 0.34, camY: 0.27, anchor: null, cosFile: "", quality: null, game: "",
};
const PARTS = ["body", "head", "kigurumi", "face", "puchi", "cosFile", "game"];
// The older games' (PS3) models are in the game's units, 100 times Nijiiro's.
const ERA_SCALE = 0.01;
// Their shaders, by the low byte of a material's nudFlags (extras): color_change mixes the player's colours by the
// texture's channels, face_anime shows the face set's current frame. The outline (black_line) is Nijiiro's hull: as
// wide as the vertex colour's green says (the only thing that reads it), drawn the same way.
const ERA_COLOR_CHANGE = 0x01, ERA_FACE = 0x02;
// The rim lines' half-width, in Nijiiro units (so they scale with don-chan, as Nijiiro's modelled lines do).
const ERA_LINE_WIDTH = 0.0012;
// Set while the line pass renders: color_change writes which colour slot each pixel takes, the other parts only hide
// what's behind them.
const eraAlphaPass = { value: 0 };
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
  // A costume part, falling back to part 0 when the operator's extract doesn't have that id. An older game's are in
  // game_data/<game>/, and its puchi are acce/acc_<id>.glb.
  const part = async (base, folder, file, ext) => {
    const name = base && folder === "acce" ? "acc" : folder;
    return (await asset(`${base}${folder}/${name}_${file}.${ext}`)) ?? asset(`${base}${folder}/${name}_000000.${ext}`);
  };
  // An older game's face set: a kigurumi's own (face_<cos id>), else the paint set. Paint ids are the same art in every
  // game and the face plane is Nijiiro's size, so Nijiiro's sharper set is used when it's extracted (the PS3's are 128 px).
  const eraFace = async (base, kig, cosFile, paint) =>
    (kig ? await asset(`${base}face/face_${cosFile}.png`) : null)
    ?? (await asset(`paint/paint_${paint}.png`)) ?? part(base, "paint", paint, "png");
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
    const era = !!s.game, base = era ? `${s.game.toLowerCase()}/` : "";
    // A kigurumi brings its own face atlas (face_<cos id>), when the game has one.
    const urls = await Promise.all([
      part(base, "body", pad(s.body), "glb"), part(base, "head", pad(s.head), "glb"), part(base, "cos", cosFile, "glb"),
      era ? eraFace(base, kig, cosFile, pad(s.face)) : part(base, "face", cosFile, "png"), part(base, "paint", pad(s.face), "png"), part(base, "acce", pad(s.puchi), era ? "glb" : "png"),
      asset(`${base}animation/don.glb`), asset(`${base}animation/facekeys.json`),
    ]);
    if (id !== seq) return;
    // An older game's puchi (urls[5]) is optional: Kimidori and Momoiro have none.
    if (urls.some((u, i) => !u && !(era && i === 5))) return setStatus(false, "missing-assets");
    let loadedParts;
    try {
      const [body, head, cos, face, paint, acce, anim, faceKeys] = urls;
      loadedParts = await Promise.all([gltf(body), gltf(head), gltf(cos), texture(face), texture(paint), era ? (acce ? gltf(acce) : null) : texture(acce), gltf(anim), json(faceKeys)]);
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
    if (era) return buildEra({ kig, roots, faceTex, paintTex, acce: acceTex, animLib, faceKeys, urls });
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

  // An older game's don-chan: its own models, rig and clips (game_data/<game>/), scaled to Nijiiro's units, and its
  // puchi a model with its own "loop" clip.
  function buildEra({ kig, roots, faceTex, paintTex, acce, animLib, faceKeys, urls }) {
    const puchiRoot = acce ? cloneSkinned(acce.scene) : null;
    // Every PS3 puchi card is 22.6 units square, centred at (−15, 14.25, 10). As Nijiiro's: sized and placed where its
    // puchi is, turned to face the camera, and drawn over don-chan (dressEra).
    let pivot = null;
    if (puchiRoot) {
      const k = 0.2 / ERA_SCALE / 22.6;
      puchiRoot.scale.setScalar(k);
      puchiRoot.position.set(15 * k, -14.25 * k, -10 * k);
      pivot = new THREE.Group();
      pivot.position.set(-0.135 / ERA_SCALE, PUCHI_Y / ERA_SCALE, 0.06 / ERA_SCALE);
      pivot.rotation.y = CAM_YAW;
      pivot.add(puchiRoot);
    }
    const shown = kig ? [roots.cos] : [roots.body, roots.head];
    const group = new THREE.Group();
    group.scale.setScalar(ERA_SCALE);
    group.add(...shown, ...(pivot ? [pivot] : []));
    const next = { era: true, kig, roots: puchiRoot ? { ...roots, puchi: puchiRoot } : roots, shown, group, faceTex, paintTex, puchiLib: acce, animLib, faceKeys, urls: urls.filter(Boolean),
      mixers: [], actions: [], faceTextures: [] };
    dressEra(next);
    bind(next);
    if (model) {
      scene.remove(model.group);
      release(model, urls);
    }
    model = next;
    scene.add(group);
    setStatus(false);
    requestAnimationFrame(() => requestAnimationFrame(() => { if (model === next && !status.loading) show(true); }));
  }

  // The game's own materials, made again on a colour change. The GLB's material stays on the mesh as its source.
  function dressEra(m) {
    const c = colours();
    for (const tex of m.faceTextures) tex.dispose();
    m.faceTextures = [];
    // Hidden in the alpha pass: the face plane (a decal over the skin), the outline hulls and the puchi.
    m.noAlpha ??= new Set();
    for (const root of Object.values(m.roots)) {
      const meshes = [];
      root.traverse(obj => { if (obj.isMesh && !obj.userData.donOutline) meshes.push(obj); });
      for (const obj of meshes) {
        const src = obj.userData.donEraSource ??= obj.material;
        if (obj.material !== src) obj.material.dispose();
        const shader = (src.userData?.nudFlags ?? 0) & 0xff;
        if (shader === ERA_COLOR_CHANGE) obj.material = eraColorChange(src, c);
        else if (shader === ERA_FACE) {
          // The plane's own texture is never read: it shows the face set's frame (12 rows, as Nijiiro's). Cut out at half
          // alpha rather than blended, so its edges are as crisp as Nijiiro's face.
          const frame = m.faceTex.clone();
          frame.repeat.set(1, 1 / 12);
          frame.needsUpdate = true;
          m.faceTextures.push(frame);
          obj.material = new THREE.MeshBasicMaterial({ map: frame, alphaTest: 0.5, side: src.side });
          m.noAlpha.add(obj);
        } else {
          obj.material = eraOccluder(new THREE.MeshBasicMaterial({ map: src.map, transparent: src.transparent, alphaTest: src.alphaTest,
            depthWrite: src.depthWrite, side: src.side }));
          if (obj.material.map) obj.material.map.colorSpace = THREE.SRGBColorSpace;
        }
        if (src.userData?.outline && !obj.userData.donEraOutlined && obj.geometry.attributes.normal) {
          obj.userData.donEraOutlined = true;
          m.noAlpha.add(addMeshOutline(obj, 1 / ERA_SCALE));
        }
        if (root === m.roots.puchi) {
          obj.material.depthTest = obj.material.depthWrite = false;
          obj.renderOrder = 1000;
          m.noAlpha.add(obj);
        }
      }
      setMeshOutlineParams(root, OUTLINE.visible, OUTLINE.thickness);
    }
  }

  // color_change: rgb = t.r·limb + t.g·(body − limb) + t.b·(face − limb), opaque (the RGB under alpha 0 counts), unlit.
  // Its a = t.a goes to the frame, where the game's outline_post draws black where alpha changes: the rim lines. The
  // masks' alpha edges sit where the rims meet the barrel and the skin, but the strip and the half-ring each have
  // their own, so following alpha doubles every line. The line pass writes the colour slot instead (g: limb 0.25,
  // body 0.5, face 0.75; r = 1 marks don-chan), and eraLines draws one line where it changes, as Nijiiro's.
  function eraColorChange(src, c) {
    const map = src.map;
    // A mask: its channels are weights, not colours (no sRGB decode). They're only a few texels wide, so each channel is
    // snapped at half (step below): blended as they are, the barrel's edge becomes a band of mixed colour; read texel
    // by texel, the face's circle turns into steps.
    if (map) map.colorSpace = THREE.NoColorSpace;
    const mat = new THREE.MeshBasicMaterial({ map, side: src.side });
    const limb = new THREE.Color(c.rim), body = new THREE.Color(c.body), face = new THREE.Color(c.face);
    mat.onBeforeCompile = shader => {
      Object.assign(shader.uniforms, { eraLimb: { value: limb }, eraBody: { value: body }, eraFace: { value: face } });
      shader.fragmentShader = shader.fragmentShader
        .replace("#include <common>", "#include <common>\nuniform vec3 eraLimb;\nuniform vec3 eraBody;\nuniform vec3 eraFace;")
        .replace("#include <map_fragment>", `vec4 eraTex = texture2D(map, vMapUv);
          vec4 eraMask = step(0.5, eraTex);
          vec3 eraRgb = eraMask.r * eraLimb + eraMask.g * (eraBody - eraLimb) + eraMask.b * (eraFace - eraLimb);
          diffuseColor = vec4(eraRgb, 1.0);`)
        .replace("#include <dithering_fragment>", "#include <dithering_fragment>\nif (eraAlphaPass > 0.5) gl_FragColor = vec4(1.0, eraMask.g > 0.5 ? 0.5 : eraMask.b > 0.5 ? 0.75 : 0.25, 0.0, 1.0);")
        .replace("#include <common>", "#include <common>\nuniform float eraAlphaPass;");
      shader.uniforms.eraAlphaPass = eraAlphaPass;
    };
    mat.customProgramCacheKey = () => "eraColorChange";
    return mat;
  }

  // A part that's drawn as it is (0x00 textures): in the alpha pass it only hides what's behind it, so no line shows
  // through a kigurumi's hull.
  function eraOccluder(mat) {
    mat.onBeforeCompile = shader => {
      shader.uniforms.eraAlphaPass = eraAlphaPass;
      shader.fragmentShader = shader.fragmentShader
        .replace("#include <common>", "#include <common>\nuniform float eraAlphaPass;")
        .replace("#include <dithering_fragment>", "#include <dithering_fragment>\nif (eraAlphaPass > 0.5) gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);");
    };
    mat.customProgramCacheKey = () => "eraOccluder";
    return mat;
  }

  // ---- The rim lines (outline_post): the alpha pass into a target, then a full-screen pass drawing black over the
  // frame where the colour slot changes between don-chan's pixels (never at his edge against the background: the hull
  // outlines that).
  let lines = null;
  function eraLines() {
    if (lines) return lines;
    const target = new THREE.WebGLRenderTarget(1, 1);
    const material = new THREE.ShaderMaterial({
      uniforms: { tAlpha: { value: target.texture }, step: { value: new THREE.Vector2() } },
      vertexShader: "varying vec2 vUv; void main() { vUv = uv; gl_Position = vec4(position.xy, 0.0, 1.0); }",
      fragmentShader: `uniform sampler2D tAlpha; uniform vec2 step; varying vec2 vUv;
        void main() {
          vec4 c = texture2D(tAlpha, vUv);
          if (c.r < 0.5) discard;
          float edge = 0.0;
          for (int i = -1; i <= 1; i++) for (int j = -1; j <= 1; j++) {
            vec4 t = texture2D(tAlpha, vUv + vec2(float(i), float(j)) * step);
            if (t.r > 0.5) edge = max(edge, abs(t.g - c.g));
          }
          float line = clamp(edge * 16.0 - 0.5, 0.0, 1.0);
          if (line <= 0.0) discard;
          gl_FragColor = vec4(0.0, 0.0, 0.0, line);
        }`,
      transparent: true, depthTest: false, depthWrite: false, toneMapped: false,
    });
    const quad = new THREE.Mesh(new THREE.PlaneGeometry(2, 2), material);
    quad.frustumCulled = false;
    const quadScene = new THREE.Scene();
    quadScene.add(quad);
    return lines = { target, material, quadScene, quadCamera: new THREE.Camera() };
  }

  function drawEraLines(upp) {
    const L = eraLines(), size = renderer.getDrawingBufferSize(new THREE.Vector2());
    if (L.target.width !== size.x || L.target.height !== size.y) L.target.setSize(size.x, size.y);
    for (const obj of model.noAlpha) obj.visible = false;
    eraAlphaPass.value = 1;
    renderer.setRenderTarget(L.target);
    renderer.clear();
    renderer.render(scene, camera);
    renderer.setRenderTarget(null);
    eraAlphaPass.value = 0;
    for (const obj of model.noAlpha) obj.visible = true;
    renderer.render(scene, camera);
    // The taps' reach: the line's half-width on screen, in the target's texels.
    const texels = Math.max(1, ERA_LINE_WIDTH / upp * dpr);
    L.material.uniforms.step.value.set(texels / size.x, texels / size.y);
    renderer.autoClear = false;
    renderer.render(L.quadScene, L.quadCamera);
    renderer.autoClear = true;
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
    // A clip the game doesn't have (older games lack don_wait_loop) plays the clear loop, with that clip's face keys.
    const clip = THREE.AnimationClip.findByName(m.animLib.animations, state.anim)
      || THREE.AnimationClip.findByName(m.animLib.animations, "don_result_clear_loop") || m.animLib.animations[0];
    const data = m.faceKeys?.animations?.[clip?.name ?? state.anim];
    const keys = Array.isArray(data?.keys)
      ? data.keys.map(k => ({ time: Number(k?.time) || 0, faceIndex: Math.max(0, Math.floor(Number(k?.faceIndex) || 0)) }))
        .sort((a, b) => a.time - b.time)
      : [];
    m.keys = keys.length ? keys : [{ time: 0, faceIndex: 0 }];
    m.frameCount = Math.max(1, Math.floor(Number(data?.frameCount) || 12));
    if (!clip) return;
    for (const root of m.shown) {
      const mixer = new THREE.AnimationMixer(root), action = mixer.clipAction(clip);
      action.setLoop(THREE.LoopRepeat, Infinity);
      action.play();
      m.mixers.push(mixer);
      m.actions.push(action);
    }
    // An older game's puchi plays its own loop (after the main actions, which the face keys follow).
    const loop = m.puchiLib?.animations.find(a => a.name === "loop") ?? m.puchiLib?.animations[0];
    if (loop && m.roots.puchi) {
      const mixer = new THREE.AnimationMixer(m.roots.puchi), action = mixer.clipAction(loop);
      action.setLoop(THREE.LoopRepeat, Infinity);
      action.play();
      m.mixers.push(mixer);
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
    if (!m.era) {
      m.atlas.dispose();
      m.puchi.geometry.dispose();
      m.puchi.material.dispose();
    }
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
      if (!model.era) {
        model.atlas.offset.y = Math.floor(time) % 2 === 0 ? 0.5 : 0;
        model.puchi.position.y = PUCHI_Y + Math.sin(time * 2 * Math.PI) * 0.01;
        model.puchi.quaternion.copy(camera.quaternion);
      }
    }
    if (w && h) {
      if (model?.era) drawEraLines(upp);
      else renderer.render(scene, camera);
    }
  }
  renderer.setAnimationLoop(frame);

  function dispose() {
    renderer.setAnimationLoop(null);
    seq++;
    if (model) release(model, []);
    model = null;
    if (lines) {
      lines.target.dispose();
      lines.material.dispose();
      lines.quadScene.children[0].geometry.dispose();
    }
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
      if (state.bodyColor !== prev.bodyColor || state.faceColor !== prev.faceColor || state.limbColor !== prev.limbColor) (model.era ? dressEra : dress)(model);
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
