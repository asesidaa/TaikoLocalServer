// The game's material-name rules (normalize.ts materialFlags): how each blend suffix renders. `npm test`.
import { test } from 'node:test'
import assert from 'node:assert/strict'
import * as THREE from 'three'
import { materialFlags } from './normalize.ts'

const flags = (name: string, extras?: object) => {
  const m = new THREE.MeshBasicMaterial()
  m.name = name
  if (extras) m.userData = { extras }
  return materialFlags(m)
}

test('a custom-colour material with no blend suffix is opaque and outlined', () => {
  const f = flags('cos_001000_color_s_cus_0x10000001')
  assert.equal(f.forceOpaque, true)
  assert.equal(f.recolor, true)   // no shaderType metadata: the colour mask still recolours
  assert.equal(f.transparentOutline, false)
})

test('_AA_ADD is additive: no depth writes, no opaque override', () => {
  const f = flags('cos_002000_color_s_cus_AA_ADD')
  assert.equal(f.additive, true)
  assert.equal(f.forceOpaque, false)
  assert.equal(f.transparentOutline, true)
})

test('_AT_ONE is a hard cutout, outlined through its alpha', () => {
  const f = flags('cos_003000_color_s_cus_AT_ONE')
  assert.equal(f.alphaTestOne, true)
  assert.equal(f.outlineCutout, true)
  assert.equal(f.forceOpaque, true)
})

test('_AT_ONE with _A_AB stays blended', () => {
  const f = flags('cos_004000_color_s_cus_AT_ONE_A_AB')
  assert.equal(f.alphaTestOne, false)
  assert.equal(f.forceOpaque, false)
  assert.equal(f.transparentOutline, true)
})

test('_nom_ with _AT_ONE stays transparent but writes depth', () => {
  const f = flags('cos_005000_nom_AT_ONE')
  assert.equal(f.alphaTestOne, false)
  assert.equal(f.forceTransparent, true)
})

test('_AT_ZERO alone writes depth', () => {
  const f = flags('cos_006000_AT_ZERO')
  assert.equal(f.alphaTestZero, true)
  assert.equal(f.forceDepthWrite, true)
  assert.equal(f.additive, false)
})

test('the face and the facial paint are told apart', () => {
  assert.equal(flags('head_facial_s_cus').face, true)
  const paint = flags('Paint_FACIAL')
  assert.equal(paint.facialPaint, true)
  assert.equal(paint.face, false)
})

test('glass with _AT_ONE is the half-see-through kind', () => {
  const f = flags('cos_007000_glass_AT_ONE', { shaderType: 'taikoEffectGlass' })
  assert.equal(f.glass, true)
  assert.equal(f.glassAlphaTestOne, true)
  assert.equal(f.alphaTestOne, false)
})
