import { useEffect, useMemo, useRef, useState } from 'react'
import { useFrame, useLoader } from '@react-three/fiber'
import * as THREE from 'three'
import { GLTFLoader, SkeletonUtils } from 'three-stdlib'
import type { AnimProgress, DonColors } from './types'
import { disposePart, normalizePart } from './normalize'

type Props = {
  bodyUrl: string
  headUrl: string
  cosUrl: string
  faceUrl: string
  paintUrl: string
  acceUrl: string

  showBody: boolean
  showHead: boolean
  showCos: boolean

  animLibraryUrl: string
  /** Face expression keys per clip. */
  faceKeysUrl: string
  animName: string
  colors: DonColors
  outline: { visible: boolean; thickness: number }

  /** Called whenever the parts are dressed (also on a colour or outline change). */
  onPartsReady?: () => void
  /** Written every frame with {time, duration} of the primary action. */
  animProgressRef?: React.MutableRefObject<AnimProgress>
  /** Assigned a function that seeks all mixers to an absolute time (seconds). */
  seekRef?: React.MutableRefObject<((t: number) => void) | null>
}

type FaceKey = { time: number; faceIndex: number }
type FaceKeyData = { frameCount?: number; keys?: FaceKey[] }
type FaceKeyBundle = { animations?: Record<string, FaceKeyData> }

// The puchi: a camera-facing sprite that flips between its two atlas frames and bobs once a second.
function AnimatedAccePlane(props: { texture: THREE.Texture }) {
  const meshRef = useRef<THREE.Mesh | null>(null)
  const baseY = 0.115

  useFrame(({ clock, camera }) => {
    const time = clock.getElapsedTime()
    props.texture.offset.y = Math.floor(time) % 2 === 0 ? 0.5 : 0
    const mesh = meshRef.current
    if (mesh) {
      mesh.position.y = baseY + Math.sin(time * (2 * Math.PI)) * 0.01
      mesh.quaternion.copy(camera.quaternion)
    }
  })

  return (
    <mesh ref={meshRef} name="puchi" position={[-0.135, baseY, 0.06]} renderOrder={1000} userData={{ donOutlineOverlay: true, restY: baseY }}>
      <planeGeometry args={[0.2, 0.2]} />
      <meshBasicMaterial map={props.texture} transparent side={THREE.DoubleSide} depthTest={false} depthWrite={false} />
    </mesh>
  )
}

export function DonCharacter(props: Props) {
  // Keep a stable ref to the callback so it doesn't appear in useEffect deps.
  const onPartsReadyRef = useRef(props.onPartsReady)
  onPartsReadyRef.current = props.onPartsReady
  const animLib = useLoader(GLTFLoader, props.animLibraryUrl)
  const bodyGltf = useLoader(GLTFLoader, props.bodyUrl)
  const headGltf = useLoader(GLTFLoader, props.headUrl)
  const cosGltf = useLoader(GLTFLoader, props.cosUrl)
  const faceTex = useLoader(THREE.TextureLoader, props.faceUrl)
  const paintTex = useLoader(THREE.TextureLoader, props.paintUrl)
  const acceTex = useLoader(THREE.TextureLoader, props.acceUrl)

  // Important: SkinnedMesh hierarchies must be cloned with SkeletonUtils
  // or animation binding can fail / stick in T-pose.
  const body = useMemo(() => SkeletonUtils.clone(bodyGltf.scene) as THREE.Object3D, [bodyGltf.scene])
  const head = useMemo(() => SkeletonUtils.clone(headGltf.scene) as THREE.Object3D, [headGltf.scene])
  const cos = useMemo(() => SkeletonUtils.clone(cosGltf.scene) as THREE.Object3D, [cosGltf.scene])
  // A replaced part frees its GPU buffers, materials and face textures (its source is dropped from the loader cache
  // in main.tsx, so nothing else shares them).
  useEffect(() => () => disposePart(body), [body])
  useEffect(() => () => disposePart(head), [head])
  useEffect(() => () => disposePart(cos), [cos])
  useEffect(() => () => { faceTex.dispose() }, [faceTex])
  useEffect(() => () => { paintTex.dispose() }, [paintTex])

  useEffect(() => {
    for (const texture of [faceTex, paintTex]) {
      texture.colorSpace = THREE.SRGBColorSpace
      texture.flipY = false
      texture.wrapS = THREE.ClampToEdgeWrapping
      texture.wrapT = THREE.ClampToEdgeWrapping
      texture.needsUpdate = true
    }
  }, [faceTex, paintTex])

  const acceAtlas = useMemo(() => {
    const t = acceTex.clone()
    t.colorSpace = THREE.SRGBColorSpace
    t.flipY = true
    t.wrapS = THREE.ClampToEdgeWrapping
    t.wrapT = THREE.ClampToEdgeWrapping
    t.repeat.set(1, 0.5)
    t.offset.set(0, 0.5)
    t.needsUpdate = true
    return t
  }, [acceTex])
  useEffect(() => () => { acceAtlas.dispose(); acceTex.dispose() }, [acceAtlas, acceTex])

  useEffect(() => {
    normalizePart(body, paintTex, props.colors, 'body', props.outline, false, true)
    normalizePart(head, paintTex, props.colors, 'head', props.outline, false, false)
    normalizePart(cos, faceTex, props.colors, 'cos', props.outline, props.showCos, false)
    onPartsReadyRef.current?.()
  }, [body, head, cos, faceTex, paintTex, props.colors, props.outline.visible, props.outline.thickness, props.showCos])

  const clip = useMemo(() => THREE.AnimationClip.findByName(animLib.animations, props.animName) || animLib.animations[0] || null,
    [animLib.animations, props.animName])

  const mixersRef = useRef<THREE.AnimationMixer[]>([])
  const actionsRef = useRef<THREE.AnimationAction[]>([])
  const faceBundleRef = useRef<FaceKeyBundle | null>(null)
  const faceKeysRef = useRef<FaceKey[]>([{ time: 0, faceIndex: 0 }])
  const faceFrameCountRef = useRef<number>(12)
  const faceAtlasTexturesRef = useRef<THREE.Texture[]>([])
  const [faceBundleVersion, setFaceBundleVersion] = useState(0)

  useEffect(() => {
    let cancelled = false
    fetch(props.faceKeysUrl)
      .then((r) => (r.ok ? r.json() : null))
      .catch(() => null)
      .then((json: FaceKeyBundle | null) => {
        if (cancelled) return
        faceBundleRef.current = json
        setFaceBundleVersion((v) => v + 1)
      })
    return () => { cancelled = true }
  }, [props.faceKeysUrl])

  useEffect(() => {
    const data = faceBundleRef.current?.animations?.[props.animName]
    const keys = Array.isArray(data?.keys)
      ? data.keys
          .map((k) => ({ time: Number(k?.time) || 0, faceIndex: Math.max(0, Math.floor(Number(k?.faceIndex) || 0)) }))
          .sort((a, b) => a.time - b.time)
      : []
    faceKeysRef.current = keys.length > 0 ? keys : [{ time: 0, faceIndex: 0 }]
    faceFrameCountRef.current = Math.max(1, Math.floor(Number(data?.frameCount) || 12))
  }, [props.animName, faceBundleVersion])

  // The face atlas textures the face frame moves (the face materials' copies, made by normalizePart).
  useEffect(() => {
    const textures = new Set<THREE.Texture>()
    for (const root of [body, head, cos]) {
      root.traverse((obj) => {
        const mesh = obj as THREE.Mesh
        if (!mesh.isMesh) return
        for (const mat of Array.isArray(mesh.material) ? mesh.material : [mesh.material]) {
          const basic = mat as THREE.MeshBasicMaterial
          if (basic?.userData?._donFlags?.face && basic.map) textures.add(basic.map)
        }
      })
    }
    faceAtlasTexturesRef.current = [...textures]
  }, [body, head, cos, props.showBody, props.showHead, props.showCos, props.faceUrl, props.paintUrl, props.colors])

  // The face atlas frame for the primary action's current time (its face keys).
  function applyFaceFrame() {
    const keys = faceKeysRef.current
    const frameCount = faceFrameCountRef.current
    const action = actionsRef.current[0]
    const clipDuration = action?.getClip()?.duration || 0
    const t = action?.time || 0
    const sampleTime = clipDuration > 0 ? t % clipDuration : t

    let faceIndex = keys[0]?.faceIndex || 0
    for (let i = 1; i < keys.length; i += 1) {
      if (sampleTime >= keys[i].time) faceIndex = keys[i].faceIndex
      else break
    }
    const frame = ((faceIndex % frameCount) + frameCount) % frameCount
    for (const tex of faceAtlasTexturesRef.current) tex.offset.y = frame / frameCount
  }

  useEffect(() => {
    // Rebind on clip/parts change. Every clip loops: the embed shows an idle pose.
    const stop = () => {
      for (const m of mixersRef.current) m.stopAllAction()
      mixersRef.current = []
      actionsRef.current = []
    }
    stop()
    if (!clip) return

    const bind = (root: THREE.Object3D, enabled: boolean) => {
      if (!enabled) return
      const mixer = new THREE.AnimationMixer(root)
      const action = mixer.clipAction(clip)
      action.setLoop(THREE.LoopRepeat, Infinity)
      action.play()
      mixersRef.current.push(mixer)
      actionsRef.current.push(action)
    }
    bind(body, props.showBody)
    bind(head, props.showHead)
    bind(cos, props.showCos)

    if (props.seekRef) {
      props.seekRef.current = (t: number) => {
        const clamped = Math.max(0, Math.min(t, clip.duration))
        for (const m of mixersRef.current) m.setTime(clamped)
        applyFaceFrame()
      }
    }

    return () => {
      stop()
      if (props.seekRef) props.seekRef.current = null
    }
  }, [body, head, cos, clip, props.showBody, props.showHead, props.showCos])

  useFrame((_, delta) => {
    // Cap delta so a single slow frame doesn't jump the animation forward by a large amount.
    const step = Math.min(delta, 1 / 15)
    for (const m of mixersRef.current) m.update(step)
    applyFaceFrame()

    if (props.animProgressRef) {
      const action = actionsRef.current[0]
      props.animProgressRef.current = { time: action?.time ?? 0, duration: action?.getClip()?.duration ?? 0 }
    }
  })

  return (
    <group>
      {props.showBody ? <primitive object={body} /> : null}
      {props.showHead ? <primitive object={head} /> : null}
      {props.showCos ? <primitive object={cos} /> : null}
      <AnimatedAccePlane texture={acceAtlas} />
    </group>
  )
}
