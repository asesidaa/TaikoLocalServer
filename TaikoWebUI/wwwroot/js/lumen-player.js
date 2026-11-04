// lumen-player.js: plays Taiko Nijiiro Lumen UI animations (title plates, dan badges, …) with WebGL.
// Input: a lumen-scene v1 JSON (+ its atlas PNG) as written by Infrastructure.Extract. No dependencies.
//
//   const scene = await LumenScene.load("/api/Assets/ui/nameplate/scene.json", { fetch: authedFetch });
//   const player = scene.player(canvas, { set: "plates", label: "gold", scale: 1 });
//   player.setLabel("ai_3");  player.pause();  player.play();  player.destroy();
//
// Semantics match Infrastructure.Extract/Lumen/Lumen.cs: depth-ordered display lists, place/move/remove,
// matrix or position transforms, colour multiply/add, textured or solid fills, and clip masks
// (a placement's clip depth hides deeper objects outside its shape). The root sprite holds the
// label's pose; nested sprites loop independently at 60 fps (each from when it was placed).

const VERT = `
attribute vec2 a_pos; attribute vec2 a_uv;
uniform mat3 u_m; uniform vec2 u_layer;
varying vec2 v_uv;
void main() {
  vec3 p = u_m * vec3(a_pos, 1.0);
  gl_Position = vec4(p.xy, 0.0, 1.0);
  v_uv = vec2(a_uv.x, (a_uv.y + u_layer.x) / u_layer.y);
}`;

const FRAG = `
precision mediump float;
uniform sampler2D u_tex; uniform bool u_solid; uniform vec4 u_color; uniform vec4 u_mult; uniform vec4 u_add;
varying vec2 v_uv;
void main() {
  vec4 c = u_solid ? u_color : texture2D(u_tex, v_uv);
  c = clamp(c * u_mult + u_add, 0.0, 1.0);
  gl_FragColor = vec4(c.rgb * c.a, c.a); // premultiplied for ONE, ONE_MINUS_SRC_ALPHA
}`;

const IDENTITY = [1, 0, 0, 1, 0, 0];
const FILL_SOLID = 0;

const mul = (a, b) => [
  a[0] * b[0] + a[2] * b[1], a[1] * b[0] + a[3] * b[1],
  a[0] * b[2] + a[2] * b[3], a[1] * b[2] + a[3] * b[3],
  a[0] * b[4] + a[2] * b[5] + a[4], a[1] * b[4] + a[3] * b[5] + a[5],
];

export class LumenScene {
  /** Loads a scene and its atlas. `fetch` lets callers add auth (e.g. a bearer token). */
  static async load(url, { fetch: f = fetch.bind(globalThis) } = {}) {
    const res = await f(url);
    if (!res.ok) throw new Error(`lumen scene ${url}: HTTP ${res.status}`);
    const data = await res.json();
    if (data.format !== "lumen-scene" || data.version !== 1) throw new Error("unsupported lumen scene format");
    const atlasUrl = new URL(data.atlas.image, new URL(url, location.href)).href;
    const atlas = await f(atlasUrl);
    if (!atlas.ok) throw new Error(`lumen atlas ${atlasUrl}: HTTP ${atlas.status}`);
    const blob = await atlas.blob(); // blob, so no asset URL ends up in the DOM
    const image = await createImageBitmap(blob, { premultiplyAlpha: "none" });
    return new LumenScene(data, image);
  }

  constructor(data, image) {
    this.data = data;
    this.image = image;
    this.cursors = new Map(); // sprite id → { frame, next, list }: its display list as of `frame`
  }

  /** Labels of a set (e.g. "plates"), in timeline order. */
  labels(set) {
    return this.data.sets[set]?.labels ?? [];
  }

  player(canvas, options) {
    return new LumenPlayer(this, canvas, options);
  }

  /**
   * Display list of `spriteId` at local `frame`: [{char, name, m, mult, add, clip, placedAt}] in depth order.
   * Built up from the last frame asked for: playback asks for the next frame (or the same one, held), and only a
   * loop back to an earlier frame replays the timeline from 0. Replaying it every frame cost its whole length.
   */
  displayList(spriteId, frame) {
    const frames = this.data.sprites[spriteId].frames;
    let c = this.cursors.get(spriteId);
    if (!c || frame < c.frame) this.cursors.set(spriteId, c = { frame: -1, next: 0, list: new Map() });
    const list = c.list;
    for (; c.next < frames.length && frames[c.next][0] <= frame; c.next++) {
      const [id, ops] = frames[c.next];
      for (const op of ops) {
        if (op[0] === 0) { list.delete(op[1]); continue; }
        const [, char, mode, depth, clip, transform, cm, ca, name] = op;
        const prev = list.get(depth);
        // A placement on an occupied depth keeps what it doesn't give (transform 0xFFFF, colour -1);
        // mode 2 (move) also keeps the character. E.g. toho_y22_3 swaps the chibi with "unchanged" transform.
        const obj = prev
          ? { ...prev }
          : { char, m: IDENTITY, mult: [1, 1, 1, 1], add: [0, 0, 0, 0], clip, placedAt: id };
        if (mode !== 2) { obj.char = char; obj.clip = clip; obj.name = name; }
        if (mode !== 2 && prev && prev.char !== char) obj.placedAt = id;
        const kind = transform >>> 16, index = transform & 0xffff;
        if (kind === 0x0000) obj.m = this.data.matrices[index] ?? IDENTITY;
        else if (kind === 0x8000) { const [x, y] = this.data.positions[index] ?? [0, 0]; obj.m = [1, 0, 0, 1, x, y]; }
        if (cm >= 0 && this.data.colors[cm]) obj.mult = this.data.colors[cm];
        if (ca >= 0 && this.data.colors[ca]) obj.add = this.data.colors[ca];
        list.set(depth, obj);
      }
    }
    c.frame = frame;
    return [...list.entries()].sort((a, b) => a[0] - b[0]).map(([depth, o]) => ({ depth, ...o }));
  }
}

export class LumenPlayer {
  /**
   * @param {LumenScene} scene
   * @param {HTMLCanvasElement} canvas  sized automatically from the set's bounds × scale × devicePixelRatio
   * @param {{set:string,label:string,children?:Object<string,string>,scale?:number,autoplay?:boolean}} options
   *   children: instance name → label to hold that child at, e.g. { title_instance: "gold" } on the "nameplate" set
   */
  constructor(scene, canvas, { set, label, children = {}, scale = 1, autoplay = true }) {
    this.scene = scene;
    this.children = children;
    this.canvas = canvas;
    this.setName = set;
    this.scale = scale;
    const gl = canvas.getContext("webgl", { stencil: true, premultipliedAlpha: true, alpha: true, preserveDrawingBuffer: true });
    if (!gl) throw new Error("WebGL unavailable; show the static PNG instead");
    this.gl = gl;
    this.survivors = new Map(); // sprite id → depths whose child survives the sprite's loop
    this.#init();
    this.frame = 0;
    this.playing = false;
    this.reducedMotion = matchMedia("(prefers-reduced-motion: reduce)").matches;
    this.setLabel(label);
    // Only animate while visible.
    this.visible = true;
    this.observer = new IntersectionObserver(([e]) => { this.visible = e.isIntersecting; });
    this.observer.observe(canvas);
    if (autoplay && !this.reducedMotion) this.play();
  }

  /** Replaces the held children (e.g. a new plate or dan badge); nested animations restart. */
  setChildren(children) {
    this.children = children;
    this.frame = 0;
    this.renderFrame(0);
  }

  /** Resizes the canvas (e.g. its container changed width) and redraws in place; the animation keeps going. */
  setScale(scale) {
    this.scale = scale;
    this.#resize();
    this.renderFrame(this.frame);
  }

  setLabel(label) {
    const set = this.scene.data.sets[this.setName];
    if (!set) throw new Error(`no set ${this.setName}`);
    const sprite = this.scene.data.sprites[set.sprite];
    if (!(label in sprite.labels)) throw new Error(`no label ${label} in ${this.setName}`);
    this.label = label;
    this.frame = 0; // nested animations restart, as in the game when the plate changes
    this.#resize();
    this.renderFrame(0);
  }

  play() {
    if (this.playing) return;
    this.playing = true;
    let last = performance.now(), acc = 0;
    const step = (now) => {
      if (!this.playing) return;
      acc += now - last; last = now;
      const frameMs = 1000 / this.scene.data.fps;
      if (acc >= frameMs) {
        const n = Math.floor(acc / frameMs);
        acc -= n * frameMs;
        if (this.visible) { this.frame += n; this.renderFrame(this.frame); }
      }
      this.raf = requestAnimationFrame(step);
    };
    this.raf = requestAnimationFrame(step);
  }

  pause() {
    this.playing = false;
    cancelAnimationFrame(this.raf);
  }

  destroy() {
    this.pause();
    this.observer.disconnect();
    this.gl.getExtension("WEBGL_lose_context")?.loseContext();
  }

  /** Draws the label's pose with nested sprites at `t` frames since the label was set. */
  renderFrame(t) {
    const gl = this.gl;
    const set = this.scene.data.sets[this.setName];
    const root = this.scene.data.sprites[set.sprite];
    gl.viewport(0, 0, this.canvas.width, this.canvas.height);
    gl.clearColor(0, 0, 0, 0);
    gl.clearStencil(0);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.STENCIL_BUFFER_BIT);
    this.stencilLevel = 0;
    this.#drawSprite(set.sprite, root.labels[this.label], t, true, this.#projection(), [1, 1, 1, 1], [0, 0, 0, 0]);
  }

  #projection() {
    const [x0, y0, x1, y1] = this.bounds;
    const sx = 2 / (x1 - x0), sy = -2 / (y1 - y0); // stage y grows down
    return [sx, 0, 0, sy, -1 - x0 * sx, 1 - y0 * sy];
  }

  /** The canvas's stage-space box: the set's bounds, grown to what the label actually draws over its animation. */
  get bounds() {
    const key = `${this.label}|${JSON.stringify(this.children)}`;
    if (this.measuredKey !== key) { this.measuredKey = key; this.measured = this.#measure(); }
    return this.measured;
  }

  // The set's bounds come from each label's resting pose, so a part that animates outside them (ASB61's twso25_r
  // plates: a medal and sparkle above the plate) was cut off. Walk the same display tree renderFrame draws, without
  // drawing, over the longest loop in it, and take the union of every visible shape (clipped by its masks).
  #measure() {
    const set = this.scene.data.sets[this.setName];
    const root = this.scene.data.sprites[set.sprite];
    let box = [...set.bounds];
    const grow = b => { if (b) box = [Math.min(box[0], b[0]), Math.min(box[1], b[1]), Math.max(box[2], b[2]), Math.max(box[3], b[3])]; };
    const frames = Math.min(this.#longestLoop(set.sprite, new Set()), 1200);
    for (let t = 0; t < frames; t += 2) grow(this.#extent(set.sprite, root.labels[this.label], t, true, [1, 0, 0, 1, 0, 0], 1));
    return box;
  }

  #longestLoop(id, seen) {
    if (seen.has(id)) return 1;
    seen.add(id);
    const sprite = this.scene.data.sprites[id];
    if (!sprite) return 1;
    let n = sprite.frameCount;
    for (let f = 0; f < sprite.frameCount; f++)
      for (const obj of this.scene.displayList(id, f)) if (this.scene.data.sprites[obj.char]) n = Math.max(n, this.#longestLoop(obj.char, seen));
    return n;
  }

  // What #drawSprite would draw: stage-space box of its visible shapes, or null.
  #extent(id, frame, time, held, m, alpha) {
    const sprite = this.scene.data.sprites[id];
    const local = held ? frame : time % sprite.frameCount;
    let box = null, clip = null, until = -1;
    const union = (a, b) => !a ? b : !b ? a : [Math.min(a[0], b[0]), Math.min(a[1], b[1]), Math.max(a[2], b[2]), Math.max(a[3], b[3])];
    for (const obj of this.scene.displayList(id, local)) {
      if (obj.depth > until) clip = null;
      const childM = mul(m, obj.m);
      const childTime = held ? time : this.#survivesLoop(id, obj) ? time - obj.placedAt : Math.max(0, local - obj.placedAt);
      if (obj.clip > 0) { clip = this.#objExtent(obj, childTime, childM, 1); until = obj.clip; continue; }
      const a = alpha * obj.mult[3] + obj.add[3];
      if (a <= 0) continue;
      let b = this.#objExtent(obj, childTime, childM, a);
      if (b && clip) b = b[0] < clip[2] && clip[0] < b[2] && b[1] < clip[3] && clip[1] < b[3]
        ? [Math.max(b[0], clip[0]), Math.max(b[1], clip[1]), Math.min(b[2], clip[2]), Math.min(b[3], clip[3])] : null;
      box = union(box, b);
    }
    return box;
  }

  #objExtent({ char, name }, time, m, alpha) {
    const shape = this.scene.data.shapes[char], sprite = this.scene.data.sprites[char];
    if (shape) {
      let b = null;
      for (const g of shape) for (let i = 0; i < g.verts.length; i += 4) {
        const x = m[0] * g.verts[i] + m[2] * g.verts[i + 1] + m[4], y = m[1] * g.verts[i] + m[3] * g.verts[i + 1] + m[5];
        b = b ? [Math.min(b[0], x), Math.min(b[1], y), Math.max(b[2], x), Math.max(b[3], y)] : [x, y, x, y];
      }
      return b;
    }
    if (!sprite) return null;
    const heldAt = sprite.labels[this.children[name]] ?? (Object.keys(sprite.labels).length > 1 ? 0 : undefined);
    return this.#extent(char, heldAt ?? 0, time, heldAt !== undefined, m, alpha);
  }

  #resize() {
    const [x0, y0, x1, y1] = this.bounds;
    const k = this.scale * (window.devicePixelRatio || 1);
    this.canvas.width = Math.ceil((x1 - x0) * k);
    this.canvas.height = Math.ceil((y1 - y0) * k);
    this.canvas.style.width = `${Math.ceil((x1 - x0) * this.scale)}px`;
    this.canvas.style.height = `${Math.ceil((y1 - y0) * this.scale)}px`;
  }

  // Held sprites (the root, and children named in `children`) stay at `frame`, their label's pose;
  // other nested sprites loop at their own length from when placed.
  #drawSprite(id, frame, time, held, m, mult, add) {
    const sprite = this.scene.data.sprites[id];
    const local = held ? frame : time % sprite.frameCount;
    const masks = []; // [{obj, until}]
    for (const obj of this.scene.displayList(id, local)) {
      while (masks.length && obj.depth > masks[masks.length - 1].until) this.#popMask(masks.pop());
      const childM = mul(m, obj.m);
      // A child placed at frame p has played for (local − p) frames. One that survives this timeline's
      // loop (same placement on the last frame and frame 0) is never re-placed, so it keeps playing
      // across loops (Flash semantics; e.g. an 800-frame overlay inside a 300-frame loop).
      // A held sprite keeps its pose, so its children run for the whole elapsed time.
      const childTime = held ? time
        : this.#survivesLoop(id, obj) ? time - obj.placedAt
        : Math.max(0, local - obj.placedAt);
      if (obj.clip > 0) {
        this.#pushMask(obj, childM, childTime);
        masks.push({ obj, m: childM, time: childTime, until: obj.clip });
        continue;
      }
      const childMult = mult.map((v, k) => v * obj.mult[k]);
      const childAdd = add.map((v, k) => v + obj.add[k]);
      this.#draw(obj, childTime, childM, childMult, childAdd);
    }
    while (masks.length) this.#popMask(masks.pop());
  }

  #survivesLoop(id, obj) {
    let depths = this.survivors.get(id);
    if (!depths) {
      const sprite = this.scene.data.sprites[id];
      const last = new Map(this.scene.displayList(id, sprite.frameCount - 1).map(o => [o.depth, o]));
      depths = new Set(this.scene.displayList(id, 0)
        .filter(o => last.get(o.depth)?.char === o.char && last.get(o.depth).placedAt === o.placedAt)
        .map(o => o.depth));
      this.survivors.set(id, depths);
    }
    return depths.has(obj.depth) && obj.placedAt === 0;
  }

  #draw({ char, name }, time, m, mult, add) {
    const sprite = this.scene.data.sprites[char];
    if (this.scene.data.shapes[char]) this.#drawShape(char, m, mult, add);
    else if (sprite) {
      // Sprites with several labels are state machines the game drives from code (plate, dan badge, rank-up
      // effect): hold them at the label `children` names, else at frame 0. Ambient loops have one "loop" label.
      const heldAt = sprite.labels[this.children[name]] ?? (Object.keys(sprite.labels).length > 1 ? 0 : undefined);
      this.#drawSprite(char, heldAt ?? 0, time, heldAt !== undefined, m, mult, add);
    }
  }

  // Masks: draw the mask shape into the stencil only (level → level+1); content then requires level+1.
  #pushMask(obj, m, time) {
    const gl = this.gl;
    gl.colorMask(false, false, false, false);
    gl.stencilFunc(gl.EQUAL, this.stencilLevel, 0xff);
    gl.stencilOp(gl.KEEP, gl.KEEP, gl.INCR);
    this.#draw(obj, time, m, [1, 1, 1, 1], [0, 0, 0, 0]);
    this.stencilLevel++;
    gl.colorMask(true, true, true, true);
    gl.stencilFunc(gl.EQUAL, this.stencilLevel, 0xff);
    gl.stencilOp(gl.KEEP, gl.KEEP, gl.KEEP);
  }

  #popMask(mask) {
    const gl = this.gl;
    gl.colorMask(false, false, false, false);
    gl.stencilFunc(gl.EQUAL, this.stencilLevel, 0xff);
    gl.stencilOp(gl.KEEP, gl.KEEP, gl.DECR);
    this.#draw(mask.obj, mask.time, mask.m, [1, 1, 1, 1], [0, 0, 0, 0]);
    this.stencilLevel--;
    gl.colorMask(true, true, true, true);
    gl.stencilFunc(gl.EQUAL, this.stencilLevel, 0xff);
    gl.stencilOp(gl.KEEP, gl.KEEP, gl.KEEP);
  }

  #drawShape(char, m, mult, add) {
    const gl = this.gl, u = this.uniforms, atlas = this.scene.data.atlas;
    // 2x3 stage matrix → column-major mat3 for GLSL.
    gl.uniformMatrix3fv(u.m, false, [m[0], m[1], 0, m[2], m[3], 0, m[4], m[5], 1]);
    gl.uniform4fv(u.mult, mult);
    gl.uniform4fv(u.add, add);
    for (const g of this.buffers.get(char)) {
      const solid = g.fill === FILL_SOLID;
      gl.uniform1i(u.solid, solid ? 1 : 0);
      if (solid) gl.uniform4fv(u.color, this.scene.data.colors[g.atlas] ?? [0, 0, 0, 0]);
      else gl.uniform2f(u.layer, g.atlas, atlas.layers);
      gl.bindBuffer(gl.ARRAY_BUFFER, g.vbo);
      gl.vertexAttribPointer(this.attribs.pos, 2, gl.FLOAT, false, 16, 0);
      gl.vertexAttribPointer(this.attribs.uv, 2, gl.FLOAT, false, 16, 8);
      gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, g.ibo);
      gl.drawElements(gl.TRIANGLES, g.count, gl.UNSIGNED_SHORT, 0);
    }
  }

  #init() {
    const gl = this.gl;
    const compile = (type, src) => {
      const s = gl.createShader(type);
      gl.shaderSource(s, src);
      gl.compileShader(s);
      if (!gl.getShaderParameter(s, gl.COMPILE_STATUS)) throw new Error(gl.getShaderInfoLog(s));
      return s;
    };
    const prog = gl.createProgram();
    gl.attachShader(prog, compile(gl.VERTEX_SHADER, VERT));
    gl.attachShader(prog, compile(gl.FRAGMENT_SHADER, FRAG));
    gl.linkProgram(prog);
    if (!gl.getProgramParameter(prog, gl.LINK_STATUS)) throw new Error(gl.getProgramInfoLog(prog));
    gl.useProgram(prog);
    this.attribs = { pos: gl.getAttribLocation(prog, "a_pos"), uv: gl.getAttribLocation(prog, "a_uv") };
    gl.enableVertexAttribArray(this.attribs.pos);
    gl.enableVertexAttribArray(this.attribs.uv);
    this.uniforms = Object.fromEntries(["m", "layer", "tex", "solid", "color", "mult", "add"].map(n => [n, gl.getUniformLocation(prog, `u_${n}`)]));

    // Atlas: straight alpha in, premultiplied in the shader after the colour transform.
    const tex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, tex);
    gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, false);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, this.scene.image);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.uniform1i(this.uniforms.tex, 0);

    // One VBO/IBO per graphic, uploaded once.
    this.buffers = new Map();
    for (const [id, graphics] of Object.entries(this.scene.data.shapes)) {
      this.buffers.set(+id, graphics.map(g => {
        const vbo = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, vbo);
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(g.verts), gl.STATIC_DRAW);
        const ibo = gl.createBuffer();
        gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, ibo);
        gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(g.indices), gl.STATIC_DRAW);
        return { vbo, ibo, count: g.indices.length, atlas: g.atlas, fill: g.fill };
      }));
    }
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
    gl.enable(gl.STENCIL_TEST);
    gl.stencilFunc(gl.EQUAL, 0, 0xff);
    gl.stencilOp(gl.KEEP, gl.KEEP, gl.KEEP);
  }
}
