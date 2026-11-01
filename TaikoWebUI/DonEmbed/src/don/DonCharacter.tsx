import { useEffect, useMemo, useRef, useState } from 'react'
import { useFrame } from '@react-three/fiber'
import { useLoader } from '@react-three/fiber'
import * as THREE from 'three'
import { SkeletonUtils } from 'three-stdlib'
import { GLTFLoader } from 'three-stdlib'
import type { AnimProgress, BoneData, BoneInfo, DebugScenes, DonColors } from './types'
import { isLoopingAnimation, scaledClip } from './anim'
import { normalizePart } from './normalize'

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
  /** Face expression keys per clip; defaults to the bundled file. */
  faceKeysUrl?: string
  animName: string
  playing: boolean
  forceLoop: boolean
  animSpeed: number
  colors: DonColors
  outline: { visible: boolean; thickness: number }
  flattenSkeleton: boolean
  debugSkeleton: boolean
  collectBoneData: boolean

  /** Called whenever normalized parts are ready (also fires on colors/outline change). */
  onPartsReady?: (scenes: DebugScenes) => void
  /** If provided, bone transforms are written here every frame for the debug panel. */
  boneDataRef?: React.MutableRefObject<BoneData>
  /** Written every frame with {time, duration} of the primary action. */
  animProgressRef?: React.MutableRefObject<AnimProgress>
  /** Assigned a function that seeks all mixers to an absolute time (seconds). */
  seekRef?: React.MutableRefObject<((t: number) => void) | null>
}

type FaceKey = { time: number; faceIndex: number }
type FaceKeyData = { frameCount?: number; keys?: FaceKey[] }
type FaceKeyBundle = { animations?: Record<string, FaceKeyData> }

function AnimatedAccePlane(props: { texture: THREE.Texture | null; animSpeed: number }) {
  const meshRef = useRef<THREE.Mesh | null>(null)
  const baseX = -0.135
  const baseY = 0.115
  const baseZ = 0.06
  const baseSize = 0.2

  useEffect(() => {
    const material = meshRef.current?.material as THREE.MeshBasicMaterial | undefined
    if (!material || !props.texture) return
    material.map = props.texture
    material.needsUpdate = true
  }, [props.texture])

  useFrame(({ clock, camera }) => {
    if (!props.texture) return
    const speed = Number.isFinite(props.animSpeed) ? Math.max(0, props.animSpeed) : 1
    const time = clock.getElapsedTime() * speed
    const frame = Math.floor(time) % 2
    props.texture.offset.y = frame === 0 ? 0.5 : 0

    const mesh = meshRef.current
    if (mesh) {
      mesh.position.y = baseY + Math.sin(time * (2 * Math.PI)) * 0.01
      mesh.quaternion.copy(camera.quaternion)
    }
  })

  if (!props.texture) return null

  return (
    <mesh ref={meshRef} name="puchi" position={[baseX, baseY, baseZ]} renderOrder={1000} userData={{ donOutlineOverlay: true, restY: baseY }}>
      <planeGeometry args={[baseSize, baseSize]} />
      <meshBasicMaterial
        map={props.texture}
        transparent
        side={THREE.DoubleSide}
        depthTest={false}
        depthWrite={false}
      />
    </mesh>
  )
}

export function DonCharacter(props: Props) {
  const debugGroupRef = useRef<THREE.Group | null>(null)
  // Keep a stable ref to the callback so it doesn't appear in useEffect deps.
  const onPartsReadyRef = useRef(props.onPartsReady)
  onPartsReadyRef.current = props.onPartsReady
  // Keep playing state in a ref so the mixer bind effect can read it without
  // being in its deps (avoiding a full mixer-rebind on every play/pause toggle).
  const playingRef = useRef(props.playing)
  playingRef.current = props.playing
  const animLib = useLoader(GLTFLoader, props.animLibraryUrl)
  const bodyGltf = useLoader(GLTFLoader, props.bodyUrl)
  const headGltf = useLoader(GLTFLoader, props.headUrl)
  const cosGltf = useLoader(GLTFLoader, props.cosUrl)
  const faceTex = useLoader(THREE.TextureLoader, props.faceUrl)
  const paintTex = useLoader(THREE.TextureLoader, props.paintUrl)
  const acceTex = useLoader(THREE.TextureLoader, props.acceUrl)

  // Important: SkinnedMesh hierarchies must be cloned with SkeletonUtils
  // or animation binding can fail / stick in T-pose.
  const body = useMemo(() => SkeletonUtils.clone(bodyGltf.scene) as THREE.Object3D, [bodyGltf.scene, props.flattenSkeleton])
  const head = useMemo(() => SkeletonUtils.clone(headGltf.scene) as THREE.Object3D, [headGltf.scene, props.flattenSkeleton])
  const cos = useMemo(() => SkeletonUtils.clone(cosGltf.scene) as THREE.Object3D, [cosGltf.scene, props.flattenSkeleton])

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

  useEffect(() => {
    return () => {
      acceAtlas.dispose()
    }
  }, [acceAtlas])

  useEffect(() => {
    const castFaceToCos = props.showCos
    normalizePart(body, paintTex, props.colors, 'body', props.outline, false, true, props.flattenSkeleton)
    normalizePart(head, paintTex, props.colors, 'head', props.outline, false, false, props.flattenSkeleton)
    normalizePart(cos, faceTex, props.colors, 'cos', props.outline, castFaceToCos, false, props.flattenSkeleton)
    onPartsReadyRef.current?.({ body, head, cos })
  }, [body, head, cos, faceTex, paintTex, props.colors, props.outline.visible, props.outline.thickness, props.showCos, props.flattenSkeleton])

  
  const clip = useMemo(() => {
    const found = THREE.AnimationClip.findByName(animLib.animations, props.animName)
    return found || animLib.animations[0] || null
  }, [animLib.animations, props.animName])

  useEffect(() => {
    if (!clip) return
    // Helpful sanity check in dev: if there are no tracks, nothing will move.
    // eslint-disable-next-line no-console
    console.log(`[DonCharacter] clip ${clip.name} tracks=${clip.tracks.length}`)
  }, [clip])

  const mixersRef = useRef<THREE.AnimationMixer[]>([])
  const actionsRef = useRef<THREE.AnimationAction[]>([])
  const faceBundleRef = useRef<FaceKeyBundle | null>(null)
  const faceKeysRef = useRef<FaceKey[]>([{ time: 0, faceIndex: 0 }])
  const faceFrameCountRef = useRef<number>(12)
  const faceAtlasTexturesRef = useRef<THREE.Texture[]>([])
  const [faceBundleVersion, setFaceBundleVersion] = useState(0)

  useEffect(() => {
    let cancelled = false
    fetch(props.faceKeysUrl ?? '/don-model/animation/facekeys.json')
      .then((r) => (r.ok ? r.json() : null))
      .then((json: FaceKeyBundle | null) => {
        if (cancelled) return
        faceBundleRef.current = json
        setFaceBundleVersion((v) => v + 1)
      })
      .catch(() => {
        if (cancelled) return
        faceBundleRef.current = null
        setFaceBundleVersion((v) => v + 1)
      })
    return () => {
      cancelled = true
    }
  }, [props.faceKeysUrl])

  useEffect(() => {
    const applyFaceData = (data: FaceKeyData | null) => {
      if (!data) {
        faceKeysRef.current = [{ time: 0, faceIndex: 0 }]
        faceFrameCountRef.current = 12
        return
      }
      const keys = Array.isArray(data.keys)
        ? data.keys
            .map((k) => ({
              time: Number(k?.time) || 0,
              faceIndex: Math.max(0, Math.floor(Number(k?.faceIndex) || 0)),
            }))
            .sort((a, b) => a.time - b.time)
        : []
      faceKeysRef.current = keys.length > 0 ? keys : [{ time: 0, faceIndex: 0 }]
      faceFrameCountRef.current = Math.max(1, Math.floor(Number(data.frameCount) || 12))
    }

    const data = faceBundleRef.current?.animations?.[props.animName] || null
    applyFaceData(data)
  }, [props.animName, faceBundleVersion])

  useEffect(() => {
    const textures: THREE.Texture[] = []
    const seen = new Set<THREE.Texture>()

    const collect = (root: THREE.Object3D) => {
      root.traverse((obj) => {
        const anyObj = obj as any
        if (!anyObj.isMesh && !anyObj.isSkinnedMesh) return
        const mats = Array.isArray(anyObj.material) ? anyObj.material : [anyObj.material]
        for (const mat of mats) {
          const anyMat = mat as any
          const flags = anyMat?.userData?._donFlags as { face?: boolean } | undefined
          const tex = anyMat?.map as THREE.Texture | undefined
          if (!flags?.face || !tex || seen.has(tex)) continue
          seen.add(tex)
          textures.push(tex)
        }
      })
    }

    collect(body)
    collect(head)
    collect(cos)
    faceAtlasTexturesRef.current = textures
  }, [
    body,
    head,
    cos,
    props.showBody,
    props.showHead,
    props.showCos,
    props.faceUrl,
    props.paintUrl,
    props.colors,
  ])

  useEffect(() => {
    // Rebind on clip/parts change.
    for (const m of mixersRef.current) m.stopAllAction()
    mixersRef.current = []
    actionsRef.current = []
    if (!clip) return

    // The RF FBX export stores translations at 1/100 of the model's units.
    const clipScaled = props.animLibraryUrl.endsWith('/Don_3D_RF.glb') ? scaledClip(clip) : clip.clone()

    const bind = (root: THREE.Object3D | null, enabled: boolean) => {
      if (!root || !enabled) return
      const mixer = new THREE.AnimationMixer(root)
      const action = mixer.clipAction(clipScaled)
      if (props.forceLoop || isLoopingAnimation(props.animName)) {
        action.setLoop(THREE.LoopRepeat, Infinity)
      } else {
        action.setLoop(THREE.LoopOnce, 1)
        action.clampWhenFinished = true
      }
      action.reset()
      action.play()
      action.paused = !playingRef.current
      mixersRef.current.push(mixer)
      actionsRef.current.push(action)
    }

    bind(body, props.showBody)
    bind(head, props.showHead)
    bind(cos, props.showCos)

    // Assign a seek function that works whether the animation is playing or paused.
    // mixer.setTime(t) calls mixer.update(t) internally, which calls action._update().
    // When action.paused=true the effective time scale is forced to 0 (Three.js source:
    //   this._effectiveTimeScale = this.paused ? 0 : timeScale
    // ) so the delta is discarded and the pose never changes.
    // Fix: temporarily unpause each action around the setTime call, then restore.
    if (props.seekRef) {
      props.seekRef.current = (t: number) => {
        const clamped = Math.max(0, Math.min(t, clipScaled.duration))
        for (let i = 0; i < mixersRef.current.length; i++) {
          const m = mixersRef.current[i]
          const a = actionsRef.current[i]
          const wasPaused = a.paused
          a.paused = false
          m.setTime(clamped)
          a.paused = wasPaused
        }
        applyFaceFrame()
      }
    }

    return () => {
      for (const m of mixersRef.current) m.stopAllAction()
      mixersRef.current = []
      actionsRef.current = []
      if (props.seekRef) props.seekRef.current = null
    }
  }, [
    body,
    head,
    cos,
    clip,
    props.animName,
    props.forceLoop,
    props.showBody,
    props.showHead,
    props.showCos,
  ])

   useEffect(() => {
     const boneLinesAndIndicators: THREE.Object3D[] = []
     const boneLineUpdaters: (() => void)[] = []
     const seenBones = new Set<THREE.Bone>()
     if (!props.debugSkeleton) return
     const debugGroup = debugGroupRef.current
     if (!debugGroup) return

     const createBoneIndicator = (color: number, size: number = 0.008) => {
       // Create a sphere to mark the joint
       const geometry = new THREE.SphereGeometry(size, 8, 8)
       const material = new THREE.MeshBasicMaterial({
         color,
         transparent: true,
         opacity: 0.85,
         depthTest: false,
         depthWrite: false,
       })
       const mesh = new THREE.Mesh(geometry, material)
       mesh.renderOrder = 10_001
       mesh.frustumCulled = false
       return mesh
     }

     const createBoneLine = (color: number) => {
       // Create a line from parent to child bone
       const geometry = new THREE.BufferGeometry()
       const positions = new Float32Array([0, 0, 0, 0, 0, 0])
       geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3))
       
       const material = new THREE.LineBasicMaterial({
         color,
         transparent: true,
         opacity: 0.8,
         depthTest: false,
         depthWrite: false,
         fog: false,
       })
       const line = new THREE.Line(geometry, material)
       line.renderOrder = 10_000
       line.frustumCulled = false
       return line
     }

     const createRotationAxes = (size: number = 0.015) => {
       // Create XYZ axes helper to show bone rotation
       // X = red, Y = green, Z = blue
       const axes = new THREE.AxesHelper(size)
       axes.renderOrder = 10_002
       axes.frustumCulled = false
       
       // Make axes more visible by adjusting line materials
       axes.traverse((obj) => {
         const line = obj as THREE.Line
         if (line.material && (line.material as THREE.LineBasicMaterial).isLineBasicMaterial) {
           const mat = line.material as THREE.LineBasicMaterial
           mat.depthTest = false
           mat.depthWrite = false
           mat.transparent = true
           mat.opacity = 0.9
         }
       })
       
       return axes
     }

     const addHelpers = (root: THREE.Object3D | null, enabled: boolean, color: number) => {
       if (!root || !enabled) return
       const bonesArray: THREE.Bone[] = []
       
       root.traverse((obj) => {
         const anyObj = obj as any
         if (!anyObj.isSkinnedMesh || !anyObj.skeleton) return
         for (const bone of anyObj.skeleton.bones as THREE.Bone[]) {
           if (seenBones.has(bone)) continue
           seenBones.add(bone)
           bonesArray.push(bone)
         }
       })

       // Add bone lines connecting parent to child
       for (const bone of bonesArray) {
         const parent = bone.parent as THREE.Bone | null
         if (parent && seenBones.has(parent)) {
           const line = createBoneLine(color)
           
           // Create update function that captures bone and parent references
           const updateLinePos = () => {
             const positions = (line.geometry as THREE.BufferGeometry).getAttribute('position') as THREE.BufferAttribute
             const arr = positions.array as Float32Array
             arr[0] = 0
             arr[1] = 0
             arr[2] = 0
             
             const childWorldPos = bone.getWorldPosition(new THREE.Vector3())
             const parentWorldPos = parent.getWorldPosition(new THREE.Vector3())
             const relPos = childWorldPos.sub(parentWorldPos)
             
             arr[3] = relPos.x
             arr[4] = relPos.y
             arr[5] = relPos.z
             positions.needsUpdate = true
           }
           
           // Store updater function separately
           boneLineUpdaters.push(updateLinePos)
           parent.add(line)
           boneLinesAndIndicators.push(line)
         }

         // Add visual bone joint indicator
         const indicator = createBoneIndicator(color, 0.008)
         bone.add(indicator)
         boneLinesAndIndicators.push(indicator)

         // Add rotation axes to show bone orientation
         const axes = createRotationAxes(0.015)
         bone.add(axes)
         boneLinesAndIndicators.push(axes)
       }
       
       console.log(`[DonCharacter] Added ${bonesArray.length} bones with ${boneLineUpdaters.length} line updaters`)
     }

     addHelpers(body, props.showBody, 0x25c2ff)
     addHelpers(head, props.showHead, 0x7bff5b)
     addHelpers(cos, props.showCos, 0xff9f43)

     // Store updaters on debugGroup so useFrame can access them
     ;(debugGroup as any)._boneLineUpdaters = boneLineUpdaters

     return () => {
       for (const obj of boneLinesAndIndicators) {
         // Recursively dispose all geometries and materials
         obj.traverse((child) => {
           const mesh = child as any
           if (mesh.geometry) {
             mesh.geometry.dispose()
           }
           if (mesh.material) {
             const mat = mesh.material as THREE.Material
             mat.dispose()
           }
         })
         obj.parent?.remove(obj)
       }
       ;(debugGroup as any)._boneLineUpdaters = []
     }
   }, [body, head, cos, props.showBody, props.showHead, props.showCos, props.debugSkeleton])

   useEffect(() => {
     for (const action of actionsRef.current) {
       action.paused = !props.playing
     }
   }, [props.playing])

    // The face atlas frame for the primary action's current time (its face keys).
    function applyFaceFrame() {
      const keys = faceKeysRef.current
      const frameCount = faceFrameCountRef.current
      const action = actionsRef.current[0]
      const clipDuration = action?.getClip()?.duration || 0
      const t = action?.time || 0
      const sampleTime = clipDuration > 0 ? (t % clipDuration) : t

      let faceIndex = keys[0]?.faceIndex || 0
      for (let i = 1; i < keys.length; i += 1) {
        if (sampleTime >= keys[i].time) faceIndex = keys[i].faceIndex
        else break
      }

      const frame = ((faceIndex % frameCount) + frameCount) % frameCount
      const frameOffset = frame / frameCount
      for (const tex of faceAtlasTexturesRef.current) {
        tex.offset.y = frameOffset
      }
    }

    useFrame((_, delta) => {
      // Update all bone line positions
      const updaters = (debugGroupRef.current as any)?._boneLineUpdaters as ((() => void)[] | undefined)
      if (updaters) {
        for (const update of updaters) {
          update()
        }
      }

      const speed = Number.isFinite(props.animSpeed) ? Math.max(0, props.animSpeed) : 1
    // Cap delta so a single slow frame doesn't cause the animation to jump forward
    // by a large amount. AnimationMixer.update() is linear integration — sub-stepping
    // produces the same final pose as a single update, so the previous while-loop
    // only wasted CPU on already-slow frames (spiral-of-death).
    const cappedDelta = Math.min(delta, 1 / 15)
    for (const m of mixersRef.current) m.update(cappedDelta * speed)
    applyFaceFrame()

    // Write live bone data for the debug panel (only when a ref is provided).
    if (props.collectBoneData && props.boneDataRef) {
      const extractBones = (root: THREE.Object3D | null, enabled: boolean): BoneInfo[] => {
        if (!root || !enabled) return []
        const infos: BoneInfo[] = []
        const seen = new Set<THREE.Bone>()
        root.traverse((obj) => {
          const anyObj = obj as any
          if (!anyObj.isSkinnedMesh || !anyObj.skeleton) return
          for (const bone of anyObj.skeleton.bones as THREE.Bone[]) {
            if (seen.has(bone)) continue
            seen.add(bone)
            infos.push({
              name: bone.name,
              position: { x: bone.position.x, y: bone.position.y, z: bone.position.z },
              quaternion: {
                x: bone.quaternion.x,
                y: bone.quaternion.y,
                z: bone.quaternion.z,
                w: bone.quaternion.w,
              },
              scale: { x: bone.scale.x, y: bone.scale.y, z: bone.scale.z },
            })
          }
        })
        return infos
      }
      props.boneDataRef.current = {
        body: extractBones(body, props.showBody),
        head: extractBones(head, props.showHead),
        cos: extractBones(cos, props.showCos),
      }
    }

    // Write animation progress for the scrubber (uses first active action).
    if (props.animProgressRef) {
      const progressAction = actionsRef.current[0]
      props.animProgressRef.current = {
        time: progressAction?.time ?? 0,
        duration: progressAction?.getClip()?.duration ?? 0,
      }
    }
  })

  return (
    <group>
      <group ref={debugGroupRef} />
      {body && props.showBody ? <primitive object={body} /> : null}
      {head && props.showHead ? <primitive object={head} /> : null}
      {cos && props.showCos ? <primitive object={cos} /> : null}
      <AnimatedAccePlane texture={acceAtlas} animSpeed={props.animSpeed} />
    </group>
  )
}
