import type * as THREE from 'three'

export type DonManifest = {
  body: string[]
  head: string[]
  cos: string[]
  face: string[]
  paint?: string[]
  acce?: string[]
  animationLibrary: string
}

export type DonColors = {
  body: string
  face: string
  rim: string
}

export type BoneInfo = {
  name: string
  position: { x: number; y: number; z: number }
  quaternion: { x: number; y: number; z: number; w: number }
  scale: { x: number; y: number; z: number }
}

export type BoneData = {
  body: BoneInfo[]
  head: BoneInfo[]
  cos: BoneInfo[]
}

export type DebugScenes = {
  body: THREE.Object3D | null
  head: THREE.Object3D | null
  cos: THREE.Object3D | null
}

export type AnimProgress = {
  time: number
  duration: number
}
