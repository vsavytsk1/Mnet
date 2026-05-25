# VR Rendering Pipeline Debug Strategy
*No math. Just: does it SHOW UP in the headset?*
*May 24 2026*

---

## WHAT WE KNOW WORKS

```
[OK] Unity builds APK                     (v1=17min, v3=4min)
[OK] APK installs on Quest via MTP drag    
[OK] App LAUNCHES (blue background + grid lines visible)
[OK] OnGUI text renders (saw "DevelopmentBuild" watermark)
[OK] Grid floor visible (white/blue lines at bottom)
[OK] GKRenderer.Start() runs (no crash screen)
[OK] GK.BuildC60() runs (no error in OnGUI crash display)
```

## WHAT WE DON'T KNOW

```
[??] Are 3D objects rendering at all?       ? TEST WITH CUBE
[??] Is URP Unlit shader working on Quest?  ? TEST WITH CUBE  
[??] Is the C60 mesh actually built?        ? TEST WITH LOG
[??] Is the C60 at the right position?      ? TEST WITH CUBE AT SAME POS
[??] Is the scale correct for VR?           ? TEST WITH KNOWN SIZE
[??] Are materials assigned?                ? TEST WITH DEFAULT MATERIAL
[??] Is lighting affecting Unlit?           ? SHOULD NOT, BUT TEST
```

## TEST SEQUENCE

### Test 1: Basic Primitive (v3 ? IN PROGRESS)
```
VRTestCube.cs spawns:
  - Magenta cube at (0, 1.5, -2), scale 0.4, spinning
  - Gold sphere at (0.8, 1.5, -2), scale 0.3
  - OnGUI text "MACHINENET VR v2" + FPS counter

IF VISIBLE:  Pipeline works. Problem is in GKRenderer mesh/materials.
IF NOT:      Fundamental rendering issue. Check URP/XR camera setup.
```

### Test 2: Same Position as C60
```
Put test cube at EXACT same transform as GKRenderer:
  - position: vrStartPos (0, 1.4, -2)  
  - scale: vrStartScale (1.5)
  
IF VISIBLE:  C60 mesh generation is the problem.
IF NOT:      Transform/parenting issue.
```

### Test 3: C60 with Default Material
```
Replace our custom Unlit materials with Unity default:
  - Shader.Find("Standard") or default-Diffuse
  - Bright white color
  
IF VISIBLE:  Our Unlit shader doesn't work on Quest.
IF NOT:      Mesh geometry is wrong.
```

### Test 4: C60 with Debug Logging
```
Add to RebuildMesh():
  - Log vertex count per face
  - Log actual world positions
  - Log material shader name
  - Write to persistentDataPath (readable via MTP)
  
Pull logs from Quest via Windows Explorer:
  Android/data/com.vladmnet.machinenet/files/
```

## KNOWN ISSUES FROM v1/v2

### Issue A: C60 at origin = inside player's head
```
v1: transform at (0,0,0), XR Origin at (0,0,0)
    Player spawns INSIDE the C60. Faces behind camera. Nothing visible.
FIX: v2 moved to (0, 1.4, -2) ? 2m in front, eye level
STATUS: v2 still showed blue screen. Maybe not far enough?
```

### Issue B: Emergence animation on Android
```
v1: transform.localScale = Vector3.zero ? emerges to 1.0
    On Quest, maybe emergence doesn't complete? Stuck at scale 0?
FIX: v2 skips emergence on Android (#if UNITY_ANDROID)
STATUS: v2 should have full scale immediately
```

### Issue C: Mesh coordinates divided by C60R
```
All vertex positions divided by C60R (4.956)
C60 raw vertices are ~5 units from origin
After division: ~1 unit radius
With scale 1.5: ~1.5 unit radius sphere
At 2m distance: should subtend ~40 degrees. SHOULD be visible.
```

### Issue D: Materials/Shaders
```
Using: Shader.Find("Universal Render Pipeline/Unlit")
Fallback: Shader.Find("Unlit/Color")
Quest uses Vulkan/GLES ? both should support URP Unlit
BUT: if neither shader is found, material is magenta (error pink)
```

### Issue E: Face winding / backface culling
```
BuildFaceMesh uses double-sided triangles (front + back winding)
Should render from any angle.
BUT: if normals are wrong, lighting-dependent shaders won't render.
Unlit should be immune to this.
```

## LOG RETRIEVAL (no ADB needed)

```
Quest shows as MTP device in Windows Explorer.
Navigate to: Quest 3 / Internal shared storage / Android / data / 
             com.vladmnet.machinenet / files /

Debug.Log output ? NOT accessible via MTP
File.WriteAllText ? IS accessible via MTP

Strategy: Write debug info to persistentDataPath
          Pull file via Windows Explorer
          Read on PC
```

## ANIMATION TEST PLAN

```
Goal: Simple animation that proves the full render stack:
  1. Primitive spawns               ? CreatePrimitive works
  2. Material applies               ? URP shader works  
  3. Transform animates             ? Update loop runs
  4. OnGUI renders text             ? Screen overlay works
  5. Multiple objects render         ? Draw calls work
  
THEN:
  6. Custom mesh (triangle fan)     ? Same as C60 faces
  7. Custom material (Unlit color)  ? Same as C60 materials
  8. Hierarchy (parent+children)    ? Same as C60 structure
  9. Scale/position from script     ? Same as C60 placement
  
Each step isolates one variable. When one FAILS, that's the bug.
```

---

*Find where it breaks. Fix that. Only that. Build. Test. 3 minutes.*
