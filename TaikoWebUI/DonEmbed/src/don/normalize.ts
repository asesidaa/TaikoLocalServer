import * as THREE from 'three'
import type { DonColors } from './types'

type DonMaterialFlags = {
  face: boolean
  faceHip: boolean
  recolor: boolean
  glass: boolean
  glassAlphaTestOne: boolean
  additive: boolean
  alphaTestOne: boolean
  alphaTestOneSuffix: boolean
  alphaTestZero: boolean
  transparentOutline: boolean
  outlineCutout: boolean
  forceOpaque: boolean
  forceDepthWrite: boolean
  disableDepthTest: boolean
  forceTransparent: boolean
  facialPaint: boolean
}

const defaultMaterialFlags: DonMaterialFlags = {
  face: false,
  faceHip: false,
  recolor: false,
  glass: false,
  glassAlphaTestOne: false,
  additive: false,
  alphaTestOne: false,
  alphaTestOneSuffix: false,
  alphaTestZero: false,
  transparentOutline: false,
  outlineCutout: false,
  forceOpaque: false,
  forceDepthWrite: false,
  disableDepthTest: false,
  forceTransparent: false,
  facialPaint: false,
}

const meshOutlineUniforms = {
  thickness: { value: 0.0012 },
}

const HEAD_FACE_RIM_SIZE = 0.17618

export function flattenBoneRoot(root: THREE.Object3D) {
  const rootBone = root.getObjectByName('BONE_root') as THREE.Bone | null
  if (!rootBone) return
  const bones: THREE.Bone[] = []
  rootBone.traverse((child) => {
    if (child !== rootBone && (child as any).isBone) bones.push(child as THREE.Bone)
  })
  for (const bone of bones) {
    if (bone.parent !== rootBone) rootBone.attach(bone)
  }
}

function extrasText(material: THREE.Material) {
  const anyMat = material as any
  const data = anyMat?.userData || {}
  const extras = data.gltfExtensions || data.extras || data
  try {
    return JSON.stringify(extras).toLowerCase()
  } catch {
    return ''
  }
}

export function materialFlags(material: THREE.Material) {
  const anyMat = material as any
  if (anyMat?.userData?._donFlags) {
    return anyMat.userData._donFlags as DonMaterialFlags
  }
  const text = extrasText(material)
  // Check the original GLTF material name for Taiko blend-mode suffixes.
  // _AA_ADD  = additive alpha blend (no hard alpha cutoff).
  // _AT_ONE  = alpha-test-one (hard cutoff — these CAN use a depth pre-pass).
  // _AT_ZERO = alpha-test-zero (threshold 0 = no real cutoff, behaves like pure blend).
  // Rule: treat as additive (skip depth pre-pass) when the name signals pure additive
  // blending without a hard alpha edge to test against.
  const name = String(anyMat?.name || '').toLowerCase()
  const hasAdditive = name.includes('_aa_add')
  const hasAlphaTestOne = name.includes('_at_one')
  const hasAlphaTestZero = name.includes('_at_zero')
  const hasAlphaBlend = name.includes('_a_ab')
  const hasCullNone = name.includes('cullnone')
  const isFacialPaint = name.includes('paint_facial') || (name.includes('face') && name.includes('facial'))
  const hasShaderTypeMetadata = text.includes('shadertype')
  const isStrippedDonColorMask = name === 'rgb_don' || name.startsWith('rgb_don_')
  const isStrippedFaceHipColorMask = /^_*don_facehip(?:_|$)/.test(name)
  const isMissingMetadataChangeColorMask = !hasShaderTypeMetadata &&
    (name.includes('_color_s_cus_0x10000001') || name.includes('_col_s_cus_0x10000001'))
  const isFace = text.includes('taikoeffectface') || (name.includes('facial_s_cus') && !isFacialPaint)
  const isFaceHip = name.includes('facehip') || name.includes('face_hip')
  const isGlass = text.includes('taikoeffectglass')
  const isTras = name.includes('tras') || name.includes('_transparent_')
  const isNom = name.includes('_nom_')
  const hasTaikoEffect = text.includes('taikoeffect')
  const isOpaque = text.includes('"opaque"') || text.includes('opaque')

  // Truly transparent blend modes: _AA_ADD (additive) and _A_AB (standard alpha blend).
  // _AT_ZERO and _AT_ONE are alpha-TEST modes — they are alpha tested (hard cutout / always-pass),
  // NOT alpha blended.  The GLB converter sometimes incorrectly exports them as alphaMode:BLEND.
  const hasBlendSuffix = hasAdditive || hasAlphaBlend

  // _AT_ONE means hard alpha-cutout only when it is not combined with true blend flags.
  // EXCEPTIONS: transparent (tras/nom) materials and glass should remain transparent.
  const effectiveAlphaTestOne = hasAlphaTestOne && !hasBlendSuffix && !isTras && !isNom && !isGlass

  // Don material convention: _color_s_cus_ or _col_s_cus_ in the name with no true-blend suffix means the
  // material is opaque.  This covers:
  //   • no suffix at all       — plainly opaque
  //   • _AT_ZERO               — alpha-test at 0 = every fragment passes = opaque
  //   • _AT_ONE (without _A_AB) — hard cutout, depth-writes already enforced by alphaTestOne
  //   • _CULLNONE alone        — double-sided opaque
  const isDonCusMaterial = name.includes('_color_s_cus_') || name.includes('_col_s_cus_')
  const flags = {
    face: isFace,
    faceHip: isFaceHip,
    recolor: text.includes('taikoeffectchangecolors') ||
      isStrippedDonColorMask ||
      isStrippedFaceHipColorMask ||
      isMissingMetadataChangeColorMask,
    glass: isGlass,
    glassAlphaTestOne: isGlass && hasAlphaTestOne && !hasBlendSuffix,
    // Additive-blend (_AA_ADD without effective _AT_ONE and without _AT_ZERO) must NOT write depth.
    // _AT_ZERO overrides additive depth-skip: alpha-test-at-0 means all fragments always pass the
    // depth test, so depth writes are enabled regardless of the blend mode.
    additive: hasAdditive && !effectiveAlphaTestOne && !hasAlphaTestZero,
    alphaTestOne: effectiveAlphaTestOne,
    alphaTestOneSuffix: hasAlphaTestOne,
    alphaTestZero: hasAlphaTestZero,
    transparentOutline: isDonCusMaterial && hasBlendSuffix && !isFace && !isFaceHip && !isGlass,
    // outlineCutout — trace the outline through the texture alpha channel:
    //   only _AT_ONE hard-cutout materials (without an overriding _A_AB blend flag).
    outlineCutout: effectiveAlphaTestOne,
    // forceOpaque — override any GLTF alphaMode:BLEND for materials that are actually opaque.
    // Materials are forced opaque when:
    // 1. They have Don custom material naming with no blend suffix
    // 2. They have _AT_ZERO alone (no blend flags)
    // 3. They have taikoEffect extras with OPAQUE flag
    // _AT_ZERO with blend flags (_A_AB or _AA_ADD) should remain transparent.
    // EXCEPTIONS: face, faceHip, glass, and transparent (tras/nom) materials always stay transparent.
    forceOpaque: (isDonCusMaterial && (!hasBlendSuffix || (hasAlphaTestZero && !hasBlendSuffix)) && !isFace && !isFaceHip && !isGlass && !isTras && !isNom) || 
                 (hasTaikoEffect && isOpaque && !isFace && !isGlass),
    // forceDepthWrite — _AT_ZERO = alpha test threshold 0 = every fragment always passes the
    // depth test = always write to the depth buffer, even for otherwise transparent/additive materials.
    // EXCEPTION: Don't force depth writes when _AT_ZERO is combined with blend flags, or for
    // transparent (tras/nom) materials.
    forceDepthWrite: hasAlphaTestZero && !hasBlendSuffix && !isTras && !isNom,
    // disableDepthTest — Removed: all materials should have depthTest: true by default
    disableDepthTest: false,
    // forceTransparent — nom materials with _AT_ONE should stay transparent but write depth.
    forceTransparent: isNom && hasAlphaTestOne,
    // facialPaint — Paint_FACIAL materials need depthTest: true enforced
    facialPaint: isFacialPaint,
  }
  void hasCullNone
  if (!anyMat.userData) anyMat.userData = {}
  anyMat.userData._donFlags = flags
  return flags
}

function copyDebugMaterialMetadata(source: THREE.Material, target: THREE.Material, suffix = '') {
  const anySource = source as any
  const anyTarget = target as any
  const sourceName = anySource?.userData?._donSourceMaterialName || source.name || ''
  target.name = sourceName ? `${sourceName}${suffix}` : target.name
  anyTarget.userData = {
    ...(anySource?.userData || {}),
    ...(anyTarget.userData || {}),
    _donSourceMaterialName: sourceName,
    _donSourceMap: anySource?.userData?._donSourceMap ?? (anySource?.map || null),
    _donSourceAlphaMap: anySource?.userData?._donSourceAlphaMap ?? (anySource?.alphaMap || null),
    _donSourceColor: anySource?.userData?._donSourceColor ?? (anySource?.color ? anySource.color.clone() : null),
  }
}

function prepareSourceMaterial(material: THREE.Material, fallback: THREE.MeshBasicMaterial) {
  fallback.name = material.name || (material as any).userData?._donSourceMaterialName || ''
  ;(fallback as any).userData = { ...((material as any).userData || {}) }
  return fallback
}

function recolorMaterial(source: THREE.MeshBasicMaterial, colors: DonColors, flags: Partial<DonMaterialFlags>) {
  const map = source.map || null
  if (map) {
    map.colorSpace = THREE.SRGBColorSpace
    map.needsUpdate = true
  }

  const material = new THREE.MeshBasicMaterial({
    color: source.color ? source.color.clone() : new THREE.Color(0xffffff),
    map,
    transparent: flags.forceTransparent ? true : ((flags.alphaTestOne || flags.forceOpaque) ? false : Boolean(source.transparent)),
    opacity: source.opacity ?? 1,
    depthWrite: (flags.alphaTestOne || flags.forceOpaque || flags.forceDepthWrite || flags.forceTransparent) ? true : !source.transparent,
    depthTest: flags.disableDepthTest ? false : true,
    side: THREE.DoubleSide,
    alphaMap: (source as any).alphaMap || null,
    alphaTest: flags.alphaTestOne ? Math.max((source as any).alphaTest || 0, 0.5) : ((source as any).alphaTest || 0),
  })
  copyDebugMaterialMetadata(source, material)

  const bodyColor = new THREE.Color(colors.body || '#ffffff')
  const faceColor = new THREE.Color(colors.face || '#ffffff')
  const rimColor = new THREE.Color(colors.rim || '#ffffff')

  material.onBeforeCompile = (shader) => {
    shader.uniforms.bodyColor = { value: bodyColor }
    shader.uniforms.faceColor = { value: faceColor }
    shader.uniforms.rimColor = { value: rimColor }
    shader.fragmentShader = shader.fragmentShader.replace(
      '#include <common>',
      `
      #include <common>
      uniform vec3 bodyColor;
      uniform vec3 faceColor;
      uniform vec3 rimColor;
      `,
    )
    shader.fragmentShader = shader.fragmentShader.replace(
      '#include <map_fragment>',
      `
      vec4 sampledDiffuseColor = texture2D(map, vMapUv);
      float strongestMask = max(sampledDiffuseColor.r, max(sampledDiffuseColor.g, sampledDiffuseColor.b));
      float weakestMask = min(sampledDiffuseColor.r, min(sampledDiffuseColor.g, sampledDiffuseColor.b));
      float maskDominance = strongestMask - weakestMask;
      vec3 recolored = sampledDiffuseColor.rgb;
      if (strongestMask > 0.05 && maskDominance > 0.08) {
        recolored = bodyColor;
        if (sampledDiffuseColor.b > sampledDiffuseColor.r && sampledDiffuseColor.b >= sampledDiffuseColor.g) {
          recolored = rimColor;
        } else if (sampledDiffuseColor.g > sampledDiffuseColor.r && sampledDiffuseColor.g > sampledDiffuseColor.b) {
          recolored = faceColor;
        }
      }
      diffuseColor *= vec4(recolored, sampledDiffuseColor.a);
      `,
    )
  }
  ;(material as any).userData._donRecolorUniforms = {
    bodyColor,
    faceColor,
    rimColor,
  }
  material.needsUpdate = true
  material.customProgramCacheKey = () => 'taikoEffectChangeColors'
  return material
}

function updateRecolorUniforms(material: THREE.Material, colors: DonColors) {
  const anyMat = material as any
  const u = anyMat?.userData?._donRecolorUniforms
  if (!u) return
  u.bodyColor.set(colors.body)
  u.faceColor.set(colors.face)
  u.rimColor.set(colors.rim)
}

function updateFaceMaterialColor(material: THREE.Material, colors: DonColors) {
  const anyMat = material as any
  const flags = anyMat?.userData?._donFlags as Partial<DonMaterialFlags> | undefined
  if (!flags?.face || !anyMat?.isMeshBasicMaterial) return
  anyMat.color.set(colors.face)
  anyMat.needsUpdate = true
}

function updateFaceTexture(material: THREE.Material, faceTexture: THREE.Texture | null) {
  const anyMat = material as any
  const flags = anyMat?.userData?._donFlags as Partial<DonMaterialFlags> | undefined
  if ((!flags?.face && !flags?.facialPaint) || !anyMat?.isMeshBasicMaterial) return

  if (!faceTexture) {
    anyMat.map = anyMat.userData?._donSourceMap || null
    anyMat.alphaMap = anyMat.userData?._donSourceAlphaMap || null
    if (anyMat.userData?._donSourceColor) anyMat.color.copy(anyMat.userData._donSourceColor)
    anyMat.transparent = true
    anyMat.opacity = 1
    anyMat.depthWrite = false
    anyMat.needsUpdate = true
    return
  }

  const atlasFrame = faceTexture.clone()
  atlasFrame.repeat.set(1, 1 / 12)
  atlasFrame.offset.set(0, 0)
  atlasFrame.colorSpace = THREE.SRGBColorSpace
  atlasFrame.flipY = false
  atlasFrame.wrapS = THREE.ClampToEdgeWrapping
  atlasFrame.wrapT = THREE.ClampToEdgeWrapping
  atlasFrame.needsUpdate = true

  anyMat.map = atlasFrame
  anyMat.color.set(0xffffff)
  anyMat.transparent = Boolean(flags.facialPaint && !flags.face)
  anyMat.opacity = 1
  anyMat.alphaMap = null
  anyMat.depthWrite = !(flags.facialPaint && !flags.face)
  anyMat.needsUpdate = true
}

function updateGlassMaterial(material: THREE.Material) {
  const anyMat = material as any
  const flags = anyMat?.userData?._donFlags as Partial<DonMaterialFlags> | undefined
  if (!flags?.glass || !anyMat?.isMeshBasicMaterial) return
  if (!flags.glassAlphaTestOne) return
  anyMat.transparent = true
  anyMat.opacity = 0.5
  anyMat.depthWrite = true
  anyMat.depthTest = true
  anyMat.map = null
  anyMat.alphaMap = null
  anyMat.side = THREE.DoubleSide
  anyMat.needsUpdate = true
}

function updateFacialPaintMaterial(material: THREE.Material) {
  const anyMat = material as any
  const flags = anyMat?.userData?._donFlags as Partial<DonMaterialFlags> | undefined
  if (!flags?.facialPaint || !anyMat?.isMeshBasicMaterial) return
  anyMat.depthTest = true
  anyMat.needsUpdate = true
}

function basicMaterial(
  source: THREE.MeshBasicMaterial,
  flags: Partial<DonMaterialFlags>,
  faceTexture: THREE.Texture | null,
  colors: DonColors,
) {
  if (flags.glassAlphaTestOne) {
     const material = new THREE.MeshBasicMaterial({
       transparent: true,
       opacity: 0.5,
       depthWrite: true,
       side: THREE.DoubleSide,
     })
     copyDebugMaterialMetadata(source, material)
     return material
   }

  if (flags.glass) {
    const material = new THREE.MeshBasicMaterial({
      transparent: true,
      opacity: 0,
      depthWrite: false,
      side: THREE.DoubleSide,
    })
    copyDebugMaterialMetadata(source, material)
    return material
  }

  if (flags.recolor && source.map) {
    return recolorMaterial(source, colors, flags)
  }

  const hasAlphaTexture = Boolean((source as any).alphaMap) ||
    (source.map ? String((source.map as any).name || '').toLowerCase().includes('_a') : false)
  // A Don material with no blend-mode suffix in its name must be rendered opaque even if the
  // GLB converter happened to export it with alphaMode: BLEND.
  const usesAlphaBlend = (flags.forceOpaque || flags.alphaTestOne)
    ? false
    : Boolean(source.transparent || (source as any).alphaMap || source.opacity < 1)
  const alphaTest = (source as any).alphaTest || 0

  const material = new THREE.MeshBasicMaterial({
    color: source.color ? source.color.clone() : new THREE.Color(0xffffff),
    map: source.map || null,
    transparent: flags.forceTransparent ? true : ((flags.alphaTestOne || flags.forceOpaque) ? false : usesAlphaBlend),
    alphaMap: (source as any).alphaMap || null,
    alphaTest: flags.alphaTestOne ? Math.max(alphaTest, 0.5) : alphaTest,
    opacity: source.opacity ?? 1,
    side: THREE.DoubleSide,
    depthWrite: (flags.alphaTestOne || flags.alphaTestOneSuffix || flags.forceOpaque || flags.forceDepthWrite || flags.forceTransparent) ? true : !usesAlphaBlend,
    depthTest: flags.disableDepthTest ? false : true,
  })
  copyDebugMaterialMetadata(source, material)

  if (!flags.alphaTestOne && !flags.alphaTestZero && !material.alphaMap && hasAlphaTexture && alphaTest <= 0) {
    // Special case: cos_006000 taikoEffect blended _AT_ONE should not have auto cutoff
    const name = String((source as any).name || '').toLowerCase()
    if (!name.includes('cos_006000_color_s_cus')) {
      material.alphaTest = 0.5
    }
  }

  if ((flags.face || flags.facialPaint) && faceTexture) {
    const atlasFrame = faceTexture.clone()
    atlasFrame.repeat.set(1, 1 / 12)
    atlasFrame.offset.set(0, 0)
    atlasFrame.needsUpdate = true
    material.map = atlasFrame
    material.color.set(0xffffff)
    material.transparent = Boolean(flags.facialPaint && !flags.face)
    material.opacity = 1
    material.alphaMap = null
    material.depthWrite = !(flags.facialPaint && !flags.face)
  }

  if ((flags.face || flags.facialPaint) && !faceTexture) {
    material.color.set(colors.face)
  }

  if (material.map) {
    material.map.colorSpace = THREE.SRGBColorSpace
    material.needsUpdate = true
  }

  // Ensure recompilation when swapping materials.
  material.needsUpdate = true

  return material
}

function meshOutlineMaterial(source?: THREE.Material) {
  const anySource = source as any
  const flags = anySource?.userData?._donFlags as DonMaterialFlags | undefined
  if (flags?.face || flags?.faceHip || flags?.glass) {
    const material = new THREE.MeshBasicMaterial({ transparent: true, opacity: 0, depthWrite: false, colorWrite: false })
    if (source) copyDebugMaterialMetadata(source, material, '_outline_skip')
    return material
  }
  if (flags?.transparentOutline) {
    const material = new THREE.MeshBasicMaterial({ 
      transparent: true, 
      opacity: 0, 
      alphaTest: 1, 
      depthWrite: anySource?.depthWrite ?? false,
      colorWrite: false 
    })
    if (source) copyDebugMaterialMetadata(source, material, '_outline_transparent')
    return material
  }
  const useCutoutAlpha = Boolean(flags?.outlineCutout)
  const material = new THREE.MeshBasicMaterial({
    color: 0x111111,
    map: useCutoutAlpha ? anySource?.map || null : null,
    alphaMap: useCutoutAlpha ? anySource?.alphaMap || null : null,
    alphaTest: useCutoutAlpha ? Math.max(anySource?.alphaTest || 0, 0.5) : 0,
    side: THREE.BackSide,
    depthTest: true,
    depthWrite: true,
    polygonOffset: true,
    polygonOffsetFactor: 1,
    polygonOffsetUnits: 1,
    transparent: false,
  })
  if (source) copyDebugMaterialMetadata(source, material, '_outline')
  if (material.map) material.map.colorSpace = THREE.SRGBColorSpace
  material.onBeforeCompile = (shader) => {
    shader.uniforms.outlineThickness = meshOutlineUniforms.thickness
    shader.vertexShader = shader.vertexShader.replace(
      '#include <common>',
      `
      #include <common>
      uniform float outlineThickness;
      attribute vec3 outlineNormal;
      `,
    )
    shader.vertexShader = shader.vertexShader.replace(
      '#include <begin_vertex>',
      'vec3 transformed = vec3(position + outlineNormal * outlineThickness);',
    )
  }
  material.customProgramCacheKey = () => 'don-accent-mesh-outline'
  return material
}

export function setMeshOutlineParams(root: THREE.Object3D, visible: boolean, thickness: number) {
  meshOutlineUniforms.thickness.value = thickness
  root.traverse((obj) => {
    const anyObj = obj as any
    if (anyObj.userData?.donOutline) {
      anyObj.visible = visible && thickness > 0
    }
  })
}

// Hull extrusion per vertex: the average normal of every vertex sharing a position (hard
// edges split normals, which tears the hull into two offset shells), scaled by the
// vertex colour green channel. The game uses green as outline width: 0 on the drum rims,
// which already have lines baked into the mesh, so outlining them again doubles the line.
function ensureOutlineNormals(geometry: THREE.BufferGeometry) {
  if (geometry.getAttribute('outlineNormal')) return
  const pos = geometry.getAttribute('position')
  const nrm = geometry.getAttribute('normal')
  const color = geometry.getAttribute('color')
  const out = new Float32Array(pos.count * 3)
  const sums = new Map<string, THREE.Vector3>()
  const key = (i: number) => `${pos.getX(i).toFixed(5)},${pos.getY(i).toFixed(5)},${pos.getZ(i).toFixed(5)}`
  const n = new THREE.Vector3()
  for (let i = 0; i < pos.count; i += 1) {
    const k = key(i)
    const sum = sums.get(k) ?? sums.set(k, new THREE.Vector3()).get(k)!
    sum.add(n.fromBufferAttribute(nrm, i).normalize())
  }
  for (let i = 0; i < pos.count; i += 1) {
    n.copy(sums.get(key(i))!)
    // Opposing normals (two-sided cards) cancel out; keep the vertex's own normal there.
    if (n.lengthSq() < 1e-4) n.fromBufferAttribute(nrm, i)
    n.normalize().multiplyScalar(color ? color.getY(i) : 1).toArray(out, i * 3)
  }
  geometry.setAttribute('outlineNormal', new THREE.BufferAttribute(out, 3))
}

function createMeshOutline(source: THREE.Mesh | THREE.SkinnedMesh) {
  ensureOutlineNormals(source.geometry)
  const sourceMaterials = Array.isArray(source.material) ? source.material : [source.material]
  const material = Array.isArray(source.material)
    ? sourceMaterials.map((m) => meshOutlineMaterial(m))
    : meshOutlineMaterial(sourceMaterials[0])
  const outline = (source as any).isSkinnedMesh
    ? new THREE.SkinnedMesh(source.geometry, material)
    : new THREE.Mesh(source.geometry, material)
  outline.name = `${source.name || 'mesh'}_accent_outline`
  ;(outline as any).userData.donOutline = true
  outline.frustumCulled = false
  outline.renderOrder = source.renderOrder + 1
  outline.position.copy(source.position)
  outline.quaternion.copy(source.quaternion)
  outline.scale.copy(source.scale)
  if ((source as any).isSkinnedMesh) {
    const skinned = source as unknown as THREE.SkinnedMesh
    ;(outline as unknown as THREE.SkinnedMesh).bind(skinned.skeleton, skinned.bindMatrix)
    ;(outline as any).bindMatrixInverse.copy((skinned as any).bindMatrixInverse)
  }
  return outline
}

function createDepthPrePassMesh(source: THREE.Mesh | THREE.SkinnedMesh): THREE.Mesh | THREE.SkinnedMesh {
  const sourceMaterials = Array.isArray(source.material) ? source.material : [source.material]
  const prePassMaterials = sourceMaterials.map((m: THREE.Material) => {
    const anyM = m as any

    // Additive-blend materials (_AA_ADD without _AT_ONE) must never write depth.
    // Their smooth transparency has no hard alpha edge to test against, so any depth
    // write would occlude objects behind them (e.g. face behind a head overlay).
    if (anyM?.userData?._donFlags?.additive) {
      return new THREE.MeshBasicMaterial({ transparent: true, opacity: 0, depthWrite: false, colorWrite: false })
    }

    // Copy the source alpha information so the pre-pass only writes depth for fragments
    // that are actually visible.  Without this, the pre-pass writes depth for the entire
    // quad — including fully-transparent pixels — which:
    //   • gives sprite/plane decorations an opaque background
    //   • prevents the face from showing through transparent "window" areas in costumes
    const map: THREE.Texture | null = anyM?.map ?? null
    const alphaMap: THREE.Texture | null = anyM?.alphaMap ?? null
    const srcAlphaTest: number = anyM?.alphaTest ?? 0
    // Use the source's explicit alphaTest if set; otherwise default to 0.5 when there is
    // an alpha texture, so the pre-pass discards transparent pixels via GLSL `discard`
    // (which also suppresses the depth write for that fragment).
    const alphaTest = srcAlphaTest > 0 ? srcAlphaTest : (map || alphaMap ? 0.5 : 0)
    return new THREE.MeshBasicMaterial({
      map,
      alphaMap,
      alphaTest,
      colorWrite: false,
      depthWrite: true,
      transparent: false,
      side: THREE.FrontSide,
    })
  })
  const matArg = Array.isArray(source.material) ? prePassMaterials : prePassMaterials[0]
  const prePassMesh = (source as any).isSkinnedMesh
    ? new THREE.SkinnedMesh(source.geometry, matArg)
    : new THREE.Mesh(source.geometry, matArg)
  prePassMesh.name = `${source.name || 'mesh'}_depthprepass`
  ;(prePassMesh as any).userData.donDepthPrePass = true
  prePassMesh.frustumCulled = false
  // Keep generated transparent depth pre-passes after mesh outlines in the opaque
  // pass. If they write depth first, cutout costume cards can incorrectly reject
  // arm/body outlines even when the final costume pixels don't visually cover them.
  prePassMesh.renderOrder = source.renderOrder + 2
  prePassMesh.position.copy(source.position)
  prePassMesh.quaternion.copy(source.quaternion)
  prePassMesh.scale.copy(source.scale)
  if ((source as any).isSkinnedMesh) {
    const skinned = source as unknown as THREE.SkinnedMesh
    ;(prePassMesh as unknown as THREE.SkinnedMesh).bind(skinned.skeleton, skinned.bindMatrix)
    ;(prePassMesh as any).bindMatrixInverse.copy((skinned as any).bindMatrixInverse)
  }
  return prePassMesh
}

function createBackFaceMesh(source: THREE.Mesh | THREE.SkinnedMesh): THREE.Mesh | THREE.SkinnedMesh {
  const sourceMaterials = Array.isArray(source.material) ? source.material : [source.material]
  const backMaterials = sourceMaterials.map((m: THREE.Material) => {
    if (!(m as any).transparent) {
      // Opaque sub-materials are already rendered correctly as DoubleSide on the source mesh.
      // Rendering them again as BackSide here would double the back-face pass and z-fight.
      // Return an invisible placeholder that contributes nothing to color or depth.
      return new THREE.MeshBasicMaterial({ transparent: true, opacity: 0, depthWrite: false, colorWrite: false })
    }
    const clone = m.clone()
    clone.side = THREE.BackSide
    return clone
  })
  const matArg = Array.isArray(source.material) ? backMaterials : backMaterials[0]
  const backMesh = (source as any).isSkinnedMesh
    ? new THREE.SkinnedMesh(source.geometry, matArg)
    : new THREE.Mesh(source.geometry, matArg)
  backMesh.name = `${source.name || 'mesh'}_backface`
  ;(backMesh as any).userData.donBackFace = true
  backMesh.frustumCulled = false
  // Must render before the source mesh (renderOrder - 1) so back faces are always drawn
  // first in the transparent pass.  With equal renderOrder, Three.js's back-to-front sort is
  // indeterminate for two meshes at exactly the same depth, causing per-frame flickering.
  // Note: Three.js always finishes the opaque pass before starting the transparent pass
  // regardless of renderOrder, so -1 here does not affect opaque depth writes.
  backMesh.renderOrder = source.renderOrder - 1
  backMesh.position.copy(source.position)
  backMesh.quaternion.copy(source.quaternion)
  backMesh.scale.copy(source.scale)
  if ((source as any).isSkinnedMesh) {
    const skinned = source as unknown as THREE.SkinnedMesh
    ;(backMesh as unknown as THREE.SkinnedMesh).bind(skinned.skeleton, skinned.bindMatrix)
    ;(backMesh as any).bindMatrixInverse.copy((skinned as any).bindMatrixInverse)
  }
  return backMesh
}

function restoreHeadFaceGeometry(object: THREE.Mesh | THREE.SkinnedMesh) {
  const geometry = object.geometry
  const pos = geometry?.attributes?.position
  if (!pos) return
  const original = (geometry.userData as any)._donOriginalPositions as Float32Array | undefined
  if (!original) return
  const attr = pos as THREE.BufferAttribute
  attr.array.set(original)
  attr.needsUpdate = true
  geometry.computeBoundingBox()
  geometry.computeBoundingSphere()
}

function fitHeadFaceGeometry(object: THREE.Mesh | THREE.SkinnedMesh) {
  const geometry = object.geometry
  const pos = geometry?.attributes?.position as THREE.BufferAttribute | undefined
  if (!pos) return
  if (!(geometry.userData as any)._donOriginalPositions) {
    ;(geometry.userData as any)._donOriginalPositions = new Float32Array(pos.array as ArrayLike<number>)
  }
  restoreHeadFaceGeometry(object)
  geometry.computeBoundingBox()
  const box = geometry.boundingBox
  if (!box) return
  const width = box.max.x - box.min.x
  const height = box.max.y - box.min.y
  const largest = Math.max(width, height)
  if (largest <= 0 || largest <= HEAD_FACE_RIM_SIZE) return
  const scale = HEAD_FACE_RIM_SIZE / largest
  const centerX = (box.min.x + box.max.x) / 2
  const centerY = (box.min.y + box.max.y) / 2
  for (let i = 0; i < pos.count; i += 1) {
    const x = pos.getX(i)
    const y = pos.getY(i)
    pos.setX(i, centerX + (x - centerX) * scale)
    pos.setY(i, centerY + (y - centerY) * scale)
  }
  pos.needsUpdate = true
  geometry.computeBoundingBox()
  geometry.computeBoundingSphere()
}

export function normalizePart(
  root: THREE.Object3D,
  faceTexture: THREE.Texture | null,
  colors: DonColors,
  kind: 'body' | 'head' | 'cos',
  outline: { visible: boolean; thickness: number },
  castFaceToCos: boolean,
  useFacialPaintMesh = false,
  flattenSkeleton = false,
) {
  if (flattenSkeleton) flattenBoneRoot(root)
  const activeFaceTexture = kind === 'head' || (castFaceToCos && kind === 'cos') ? faceTexture : null

  // Remove previously generated outlines, back-face meshes, and depth pre-pass meshes to avoid duplicates.
  const oldOutlines: THREE.Object3D[] = []
  const oldBackFaces: THREE.Object3D[] = []
  const oldDepthPrePasses: THREE.Object3D[] = []
  root.traverse((obj) => {
    if ((obj as any).userData?.donOutline) oldOutlines.push(obj)
    if ((obj as any).userData?.donBackFace) oldBackFaces.push(obj)
    if ((obj as any).userData?.donDepthPrePass) oldDepthPrePasses.push(obj)
  })
  for (const o of oldOutlines) o.parent?.remove(o)
  for (const o of oldBackFaces) o.parent?.remove(o)
  for (const o of oldDepthPrePasses) o.parent?.remove(o)

  const outlineTargets: (THREE.Mesh | THREE.SkinnedMesh)[] = []
  const backFaceTargets: (THREE.Mesh | THREE.SkinnedMesh)[] = []
  root.traverse((obj) => {
    const anyObj = obj as any
    if (!anyObj.isMesh && !anyObj.isSkinnedMesh) return
    if (anyObj.userData?.donOutline) return
    if (anyObj.userData?.donBackFace) return
    if (anyObj.userData?.donDepthPrePass) return
    anyObj.frustumCulled = false
    anyObj.renderOrder = 0

    const sourceMaterials = Array.isArray(anyObj.material) ? anyObj.material : [anyObj.material]
    const flagsForObj = sourceMaterials.map((m: THREE.Material) =>
      m ? materialFlags(m) : defaultMaterialFlags,
    )

    const alreadyConverted = sourceMaterials.every((m: any) => m?.userData?._donConverted)
    if (alreadyConverted) {
      for (const m of sourceMaterials) {
        updateGlassMaterial(m)
        updateFacialPaintMaterial(m)
        updateRecolorUniforms(m, colors)
        updateFaceMaterialColor(m, colors)
        updateFaceTexture(m, activeFaceTexture)
      }
    }
    if (kind === 'head' && activeFaceTexture && flagsForObj.some((f: { face: boolean; facialPaint: boolean }) => f.face && (!f.facialPaint || !useFacialPaintMesh))) {
      fitHeadFaceGeometry(anyObj)
    } else if (kind === 'head' && activeFaceTexture && useFacialPaintMesh && flagsForObj.some((f: { facialPaint: boolean }) => f.facialPaint)) {
      restoreHeadFaceGeometry(anyObj)
    }

    if (!alreadyConverted) {
      const converted = sourceMaterials.map((m: any, idx: number) => {
        if (!m) return m
        const flags = flagsForObj[idx] || defaultMaterialFlags
        const src = m.isMeshBasicMaterial
          ? (m as THREE.MeshBasicMaterial)
          : prepareSourceMaterial(m, new THREE.MeshBasicMaterial({
            map: m.map || null,
            color: m.color || new THREE.Color(0xffffff),
            transparent: Boolean(m.transparent),
            opacity: m.opacity ?? 1,
            side: THREE.DoubleSide,
            alphaMap: m.alphaMap || null,
            alphaTest: m.alphaTest || 0,
          }))
        const out = basicMaterial(src, flags, activeFaceTexture, colors)
        ;(out as any).userData = (out as any).userData || {}
        ;(out as any).userData._donConverted = true
        ;(out as any).userData._donFlags = flags
        return out
      })
      anyObj.material = Array.isArray(anyObj.material) ? converted : converted[0]
    }

    if (flagsForObj.some((f: DonMaterialFlags) => f.glass || f.face || f.faceHip)) return

    // Smooth transparent meshes need a BackSide clone for correct two-pass rendering.
    // Hard _AT_ONE cutouts also get an alpha-tested outline, so hair/cards outline
    // their visible silhouette instead of being skipped or outlining the full quad.
    const finalMats: THREE.Material[] = Array.isArray(anyObj.material) ? anyObj.material : [anyObj.material]
    const hasOutlineCutout = finalMats.some((m: any) => m?.userData?._donFlags?.outlineCutout)
    const hasTransparentOutline = finalMats.some((m: any) => m?.userData?._donFlags?.transparentOutline)
    const needsBackFace = finalMats.some((m: any) => m?.transparent === true)
    if (needsBackFace) {
      for (const m of finalMats) {
        if ((m as any)?.transparent) m.side = THREE.FrontSide
      }
      backFaceTargets.push(anyObj)
      if (hasOutlineCutout || hasTransparentOutline) outlineTargets.push(anyObj)
    } else {
      outlineTargets.push(anyObj)
    }
  })

  for (const target of outlineTargets) {
    if (target.parent) target.parent.add(createMeshOutline(target))
  }

  for (const target of backFaceTargets) {
    if (target.parent) {
      target.parent.add(createBackFaceMesh(target))
      target.parent.add(createDepthPrePassMesh(target))
    }
  }

  setMeshOutlineParams(root, outline.visible, Math.max(0, outline.thickness))
}
