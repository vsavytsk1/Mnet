# MachineNet VR Render Skeleton
*The minimum viable render path ? no NS, no flow, just show the shape.*

---

## THE SKELETON (what runs right now)

```
Start()
  ??? BuildC60()           ? _state = 60 vertices, 90 edges, 32 faces
  ??? BuildMaterials()     ? _pentMat (orange), _hexMat (teal), _edgeMat (cyan)
  ??? SetupLighting()      ? directional light + ambient
  ??? BuildGrid()          ? floor grid at y=-1.2
  ??? Set position/scale   ? (0, 1.4, -2) scale 1.5 on Android

Update()
  ??? Emergence anim       ? SKIPPED on Android
  ??? Auto-rotate          ? YES (14 deg/sec)
  ??? HandleVRInput()      ? thumbsticks + buttons (Android only)
  ??? if (_dirty)
       ??? RebuildMeshAsync()
            ??? foreach face ? BuildFaceMesh() ? GameObject + MeshFilter + MeshRenderer
            ??? BuildEdgeMesh() ? combined edge quads
```

## WHAT EACH PIECE DOES

### BuildFaceMesh(face)
```
Input:  face.pts = array of [x,y,z] (3-5 points per face)
Output: Mesh with fan triangles from centroid, DOUBLE SIDED

Vertices: center + N perimeter points
          ALL divided by C60R (4.956) to normalize to ~1 unit radius
Triangles: N*2 triangles (front + back face)
```

### BuildEdgeMesh(state)
```
Input:  all faces, extract unique edges
Output: Combined mesh of thin quads (width 0.006 units)

For each edge: 4 vertices forming a quad along the edge direction
Deduplication via string key
```

### Materials
```
_pentMat: URP/Unlit, color #ff6030 (orange-red)
_hexMat:  URP/Unlit, color #004466 (dark teal)  ? MAYBE TOO DARK FOR VR?
_edgeMat: URP/Unlit, color #00d4ff (bright cyan)
```

## LIKELY FAILURE POINTS

### 1. _hexMat is too dark
```
#004466 = RGB(0, 68, 102) ? very dark teal
On a dark blue background (#050510) this is almost INVISIBLE
20 of 32 faces are hexagons = 62.5% of the C60 is near-invisible

FIX: Brighten hex color for VR
  new Color(0.0f, 0.5f, 0.7f) = #0080B3 ? much brighter
```

### 2. Scale too small
```
C60R = 4.956, vertices at ~5 units, divided = ~1 unit radius
Scale 1.5 = 1.5 unit radius
At 2m distance = subtends ~40 degrees
SHOULD be visible, but in VR everything feels smaller

FIX: Try scale 3.0 or 4.0
```

### 3. Mesh parenting
```
_meshRoot is child of GKRenderer transform
GKRenderer transform is at (0, 1.4, -2)
_meshRoot is at (0,0,0) LOCAL = (0, 1.4, -2) WORLD
Face GameObjects are children of _meshRoot at (0,0,0) LOCAL

This should work. But if GKRenderer is not on a scene object,
the position won't apply.

CHECK: Is GKRenderer attached to a GameObject in the scene?
```

### 4. Shader not found
```
Shader.Find("Universal Render Pipeline/Unlit") 
might return null on Quest if shader is stripped from build

Fallback: Shader.Find("Unlit/Color")
might ALSO return null if stripped

When shader is null ? material is magenta error pink
BUT we didn't see magenta ? either shader works or mesh doesn't render at all
```

## QUICK FIX ATTEMPT (for next build)

```csharp
// In GKRenderer.Start(), after everything, add:
#if UNITY_ANDROID
// DEBUG: spawn a primitive at same position to prove transform works
var probe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
probe.transform.position = transform.position;
probe.transform.localScale = Vector3.one * 0.5f;
probe.GetComponent<Renderer>().material.color = Color.red;
Debug.Log($"[GK] DEBUG PROBE at {transform.position}, scale={transform.localScale}");
#endif
```

---

*The skeleton is simple. 32 faces, 90 edges, 3 materials.*
*One of these is broken. The test sequence will find which one.*
