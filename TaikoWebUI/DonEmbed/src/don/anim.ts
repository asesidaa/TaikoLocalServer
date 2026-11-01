import * as THREE from 'three'

export function isLoopingAnimation(name: string) {
  return name.endsWith('_loop') || name.endsWith('_loop_mirror')
}

export function scaledClip(clip: THREE.AnimationClip) {
  const clone = clip.clone()
  for (const track of clone.tracks) {
    if (track.name.endsWith('.position')) {
      const values = (track as unknown as { values: number[] }).values
      for (let i = 0; i < values.length; i += 1) values[i] *= 100
    }
  }
  return clone
}
