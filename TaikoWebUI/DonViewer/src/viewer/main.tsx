// Display-only don-chan for TaikoLocalServer's WebUI (built by TaikoWebUI.csproj into wwwroot/don-embed, served
// at /don-embed/). State comes from the URL query and from postMessage({ type: "don", ...fields }) sent by the
// host page. Game files are not public: every part is fetched from the server's authenticated api/Assets with
// the WebUI's login token and handed to three.js as a blob: URL, so no asset URL exists outside this page.
import { Suspense, useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react'
import { createRoot } from 'react-dom/client'
import { Canvas, useLoader, useThree } from '@react-three/fiber'
import * as THREE from 'three'
import { GLTFLoader } from 'three-stdlib'
import { DonCharacter } from '../don/DonCharacter'
import { NUMBER_TO_COLOUR_MAP } from '../don/palette'
import type { AnimProgress } from '../don/types'

type State = {
  body: number; head: number; kigurumi: number; face: number; puchi: number
  bodyColor: number; faceColor: number; limbColor: number
  anim: string; zoom: number; camY: number
  /** Pixel density: low = 1x, standard = the screen's own, high = at least 2x (supersampled, smoother edges). */
  quality: Quality
  /** [cx, cy, h] in px: the box inside the canvas that zoom/camY frame. */
  anchor?: [number, number, number]
  /** An exact kigurumi variant, e.g. "036014" (the Dani Dojo outfit per rank); wins over kigurumi. */
  cosFile?: string
}

type Quality = 'low' | 'standard' | 'high'
const qualities: Quality[] = ['low', 'standard', 'high']
const stored = (key: string) => { try { return localStorage.getItem(key) } catch { return null } }
const pad = (id: number) => String(id * 1000).padStart(6, '0')
const fromQuery = (): State => {
  const q = new URLSearchParams(location.search)
  const n = (k: string, d: number) => (q.has(k) ? Number(q.get(k)) : d)
  return {
    body: n('body', 0), head: n('head', 0), kigurumi: n('kigurumi', 0), face: n('face', 0), puchi: n('puchi', 0),
    bodyColor: n('bodyColor', 1), faceColor: n('faceColor', 0), limbColor: n('limbColor', 3),
    anim: q.get('anim') || 'don_result_full_loop',
    zoom: n('zoom', 0.34), camY: n('camY', 0.27),
    // ?quality= wins; otherwise the player's choice the WebUI keeps on this device (Settings › Display).
    quality: qualities.find((x) => x === (q.get('quality') ?? stored('ui.quality3d'))) ?? 'standard',
  }
}

// ---- Assets: api/Assets (beside the WebUI) with the bearer token Blazored.LocalStorage keeps JSON-encoded ----
const API = new URL('../api/Assets/', location.href).href
const token = () => {
  try { return JSON.parse(localStorage.getItem('authToken') ?? 'null') as string | null } catch { return null }
}
const blobs = new Map<string, Promise<string | null>>()
/**
 * A blob: URL for `path`, or null if the server doesn't have it. Kept until the part is replaced (forget); a failure
 * isn't kept, so an expired token or a network blip is tried again on the next change instead of sticking.
 */
function asset(path: string): Promise<string | null> {
  let p = blobs.get(path)
  if (!p) {
    const t = token()
    p = fetch(API + path, t ? { headers: { Authorization: `Bearer ${t}` } } : undefined)
      .then(async (r) => (r.ok ? URL.createObjectURL(await r.blob()) : null))
      .catch(() => null)
    blobs.set(path, p)
    p.then((url) => { if (!url) blobs.delete(path) })
  }
  return p
}
/** Frees a part's download and drops it from three.js's loader cache, once nothing shows it any more. */
async function forget(url: string) {
  for (const [path, p] of blobs) if ((await p) === url) blobs.delete(path)
  if (url.length) {
    useLoader.clear(GLTFLoader, url)
    useLoader.clear(THREE.TextureLoader, url)
    URL.revokeObjectURL(url)
  }
}
/** A costume part, falling back to part 0 when the operator's extract doesn't have that id. */
const part = async (prefix: string, file: string, ext: string) =>
  (await asset(`${prefix}/${prefix}_${file}.${ext}`)) ?? asset(`${prefix}/${prefix}_000000.${ext}`)

type Urls = { kig: boolean; body: string; head: string; cos: string; face: string; paint: string; acce: string; anim: string; faceKeys: string }

/** The parts' blob URLs, or null until they're in; `failed` when one is missing (missing-assets is posted). */
function useUrls(s: State): { urls: Urls | null; failed: boolean } {
  const [urls, setUrls] = useState<Urls | null>(null)
  const [failed, setFailed] = useState(false)
  useEffect(() => {
    let cancelled = false
    const kig = s.kigurumi > 0 || !!s.cosFile
    const cosFile = s.cosFile || (kig ? pad(s.kigurumi) : '000000')
    ;(async () => {
      // A kigurumi brings its own face atlas (face_<cos id>), when the game has one.
      const [body, head, cos, face, paint, acce, anim, faceKeys] = await Promise.all([
        part('body', pad(s.body), 'glb'), part('head', pad(s.head), 'glb'), part('cos', cosFile, 'glb'),
        part('face', cosFile, 'png'), part('paint', pad(s.face), 'png'), part('acce', pad(s.puchi), 'png'),
        asset('animation/don.glb'), asset('animation/facekeys.json'),
      ])
      if (cancelled) return
      if (!body || !head || !cos || !face || !paint || !acce || !anim || !faceKeys) {
        parent.postMessage({ type: 'don-status', loading: false, error: 'missing-assets' }, location.origin)
        setFailed(true)
        return
      }
      setFailed(false)
      setUrls((old) => {
        // The parts this change replaced: nothing shows them now (Suspense has swapped to the new ones by the
        // time they're freed, a frame later).
        const keep = new Set([body, head, cos, face, paint, acce, anim, faceKeys])
        const gone = old ? [old.body, old.head, old.cos, old.face, old.paint, old.acce].filter((u) => !keep.has(u)) : []
        if (gone.length) setTimeout(() => gone.forEach(forget), 1000)
        return { kig, body, head, cos, face, paint, acce, anim, faceKeys }
      })
    })()
    return () => { cancelled = true }
  }, [s.kigurumi, s.cosFile, s.body, s.head, s.face, s.puchi])
  return { urls, failed }
}

// Framing: smaller zoom = bigger don. The host sends zoom/camY per layout, and `anchor` so don-chan keeps
// his size and place while the canvas covers the whole card (nothing gets clipped).
function Camera({ zoom, camY, anchor }: { zoom: number; camY: number; anchor?: [number, number, number] }) {
  const { camera, size } = useThree()
  useLayoutEffect(() => {
    const [cx, cy, h] = anchor || [size.width / 2, size.height / 2, size.height]
    const upp = h ? (2 * zoom) / h : 0   // world units per px
    Object.assign(camera, { left: -cx * upp, right: (size.width - cx) * upp, top: cy * upp, bottom: -(size.height - cy) * upp })
    camera.position.set(-0.017, camY, 0)
    camera.rotation.set(0, THREE.MathUtils.degToRad(-29), 0, 'XYZ')
    ;(camera as THREE.OrthographicCamera).updateProjectionMatrix()
  }, [camera, size, zoom, camY, anchor?.join()])
  return null
}

// One object, so the parts aren't re-dressed on every render.
const OUTLINE = { visible: true, thickness: 0.0025 }

type SnapshotRequest = { zoom: number; camY: number; camX?: number; size?: number; poseT?: number; puchi?: boolean }

// Avatar snapshot rendered off-screen: a second orthographic camera draws a fixed pose into a square
// render target, so the visible preview never reframes or flickers. camX shifts the frame sideways (along the
// camera's own right axis); puchi: false leaves the puchi out of the snapshot only.
function Snapshotter({ apiRef, seekRef, progRef }: {
  apiRef: React.MutableRefObject<((r: SnapshotRequest) => string) | null>
  seekRef: React.MutableRefObject<((t: number) => void) | null>
  progRef: React.MutableRefObject<AnimProgress>
}) {
  const { gl, scene } = useThree()
  useEffect(() => {
    apiRef.current = ({ zoom, camY, camX = 0, size = 256, poseT = 0, puchi = true }) => {
      const t = progRef.current.time
      seekRef.current?.(poseT)
      const cam = new THREE.OrthographicCamera(-zoom + camX, zoom + camX, zoom, -zoom, -1, 1000)
      cam.position.set(-0.017, camY, 0)
      cam.rotation.set(0, THREE.MathUtils.degToRad(-29), 0, 'XYZ')
      cam.updateProjectionMatrix(); cam.updateMatrixWorld()
      const puchiMesh = scene.getObjectByName('puchi') as THREE.Mesh | undefined
      if (puchiMesh && !puchi) puchiMesh.visible = false
      if (puchiMesh) {
        // Its own frame loop flips frames and bobs it; useFrame sets them again on the next frame.
        const map = (puchiMesh.material as THREE.MeshBasicMaterial).map
        if (map) map.offset.y = 0.5
        if (typeof puchiMesh.userData.restY === 'number') puchiMesh.position.y = puchiMesh.userData.restY
        puchiMesh.quaternion.copy(cam.quaternion)
        puchiMesh.updateMatrixWorld()
      }
      const rt = new THREE.WebGLRenderTarget(size, size, { samples: 4 })
      rt.texture.colorSpace = THREE.SRGBColorSpace   // match the on-screen sRGB output
      const prevTarget = gl.getRenderTarget(), prevClear = gl.getClearAlpha()
      gl.setRenderTarget(rt); gl.setClearAlpha(0); gl.clear(); gl.render(scene, cam)
      const px = new Uint8Array(size * size * 4)
      gl.readRenderTargetPixels(rt, 0, 0, size, size, px)
      gl.setRenderTarget(prevTarget); gl.setClearAlpha(prevClear); rt.dispose()
      if (puchiMesh) puchiMesh.visible = true
      seekRef.current?.(t)
      // WebGL rows are bottom-up; flip into an ImageData.
      const img = new ImageData(size, size), row = size * 4
      for (let y = 0; y < size; y++) img.data.set(px.subarray((size - 1 - y) * row, (size - y) * row), y * row)
      const out = document.createElement('canvas'); out.width = out.height = size
      out.getContext('2d')!.putImageData(img, 0, 0)
      return out.toDataURL('image/png')
    }
  }, [gl, scene, apiRef, seekRef, progRef])
  return null
}

function App() {
  const [s, setS] = useState(fromQuery)
  const [loading, setLoading] = useState(true)
  const { urls, failed } = useUrls(s)
  const snapRef = useRef<((r: SnapshotRequest) => string) | null>(null)
  const seekRef = useRef<((t: number) => void) | null>(null)
  const progRef = useRef<AnimProgress>({ time: 0, duration: 0 })
  useEffect(() => {
    const onMsg = (e: MessageEvent) => {
      if (e.origin !== location.origin) return   // only the WebUI on this server drives the embed
      if (e.data?.type === 'don') setS((p) => ({ ...p, ...e.data }))
      if (e.data?.type === 'don-snapshot' && snapRef.current)
        parent.postMessage({ type: 'don-snapshot', dataUrl: snapRef.current(e.data) }, location.origin)
    }
    addEventListener('message', onMsg)
    return () => removeEventListener('message', onMsg)
  }, [])

  // Only parts that rebuild the model fire onPartsReady; puchi (acce) is just a texture swap,
  // so it must not turn the spinner on or it would never turn off.
  useEffect(() => { if (urls) setLoading(true) }, [urls?.body, urls?.head, urls?.cos, urls?.face, urls?.paint, urls?.kig])
  // A missing part ends loading (the host shows its fallback); without this the spinner would never stop.
  useEffect(() => { if (failed) setLoading(false) }, [failed])
  useEffect(() => { parent.postMessage({ type: 'don-status', loading }, location.origin) }, [loading])
  // The model's first frame after mounting is the bind pose with raw materials (colours and the first animation pose
  // land a frame later). So it stays hidden while parts load and fades in two frames after onPartsReady.
  const [shown, setShown] = useState(false)
  useEffect(() => { if (loading) setShown(false) }, [loading])
  const onPartsReady = () => {
    setLoading(false)
    requestAnimationFrame(() => requestAnimationFrame(() => setShown(true)))
  }

  const colors = useMemo(() => ({
    body: NUMBER_TO_COLOUR_MAP[s.bodyColor] || NUMBER_TO_COLOUR_MAP[1],
    face: NUMBER_TO_COLOUR_MAP[s.faceColor] || NUMBER_TO_COLOUR_MAP[0],
    rim: NUMBER_TO_COLOUR_MAP[s.limbColor] || NUMBER_TO_COLOUR_MAP[3],
  }), [s.bodyColor, s.faceColor, s.limbColor])

  const screen = devicePixelRatio || 1
  const dpr = s.quality === 'low' ? 1 : s.quality === 'high' ? Math.min(3, Math.max(2, screen)) : Math.min(3, Math.max(1, screen))
  return (
    <>
      <div className={shown ? 'model ready' : 'model'}>
      <Canvas dpr={dpr} orthographic camera={{ near: -1, far: 1000 }} gl={{ antialias: true, alpha: true }}
        onCreated={({ gl }) => { gl.outputColorSpace = THREE.SRGBColorSpace; gl.setClearColor(0x000000, 0) }}>
        <Camera zoom={s.zoom} camY={s.camY} anchor={s.anchor} />
        <Snapshotter apiRef={snapRef} seekRef={seekRef} progRef={progRef} />
        <ambientLight intensity={2.2} />
        <hemisphereLight args={[0xffffff, 0xb8c0cc, 2.4]} />
        <directionalLight position={[120, 260, 200]} intensity={2.2} />
        <directionalLight position={[-160, 80, -120]} intensity={0.8} />
        <Suspense fallback={null}>
          {urls && (
            <DonCharacter
              bodyUrl={urls.body} headUrl={urls.head} cosUrl={urls.cos} faceUrl={urls.face}
              paintUrl={urls.paint} acceUrl={urls.acce}
              showBody={!urls.kig} showHead={!urls.kig} showCos={urls.kig}
              animLibraryUrl={urls.anim} faceKeysUrl={urls.faceKeys} animName={s.anim} colors={colors}
              outline={OUTLINE} onPartsReady={onPartsReady} seekRef={seekRef} animProgressRef={progRef}
            />
          )}
        </Suspense>
      </Canvas>
      </div>
      {loading && <div className="spin" />}
    </>
  )
}

createRoot(document.getElementById('root')!).render(<App />)
