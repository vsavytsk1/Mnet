# UNITY RULES ? Read Before Touching ANYTHING
*"Unity is a big flex graph. One broken edge = 16 minute rebuild."*
*Created: May 24 2026*

---

## THE GOLDEN RULE

```
READ ? UNDERSTAND ? PLAN ? CHANGE ? BUILD ? TEST
Never skip to CHANGE. The 16 minutes you save by reading first 
is the 16 minutes you DON'T spend recompiling a broken build.
```

---

## 1. THE GRAPH PROBLEM

Unity is a dependency graph. Everything connects:

```
Scene
 ??? GameObject
      ??? Transform (position, rotation, scale)
      ??? Component A (references Component B)
      ??? Component B (references Material M)
      ??? Material M (references Shader S)
           ??? Shader S (compiled per platform)
                ??? IL2CPP strips unused paths
                     ??? APK = minimal closed surface
```

Change Shader S ? Material M recompiles ? Component B reconnects ? 
Scene re-serializes ? IL2CPP re-strips ? 16 min rebuild.

**This is Goldberg refinement in reverse. Every change propagates.**

## 2. FILE STRUCTURE LAW

```
Assets/
??? Kernel/           ? OUR MATH (GK.cs, GKRenderer.cs) ? SACRED
?   ??? Editor/       ? Inspector tools ? EDITOR ONLY
?   ??? Tests 1/      ? NUnit tests ? EDITOR ONLY (has .asmdef)
?       ??? TestLogs/ ? Play-mode logs
??? Scenes/           ? Scene files (.unity)
??? Settings/         ? URP, XR settings
??? Audio/            ? Future
??? CompositionLayers/? Meta VR splash/loading
??? XR/               ? XR Plug-in config
    ??? Loaders/
    ??? Resources/
    ??? Settings/
```

### NEVER:
- Put test files outside Tests 1/ (the NUnit APK crash)
- Put Editor scripts in runtime folders
- Reference editor-only assemblies from runtime code
- Duplicate files across folders

### ALWAYS:
- Check .asmdef includes "Editor" in includePlatforms for test code
- Use #if UNITY_ANDROID for platform-specific code
- Use #if !UNITY_ANDROID for PC-only code (camera, mouse)

## 3. BUILD PIPELINE

```
Step 1: C# compile          (~30 sec)
Step 2: IL2CPP strip         (~2 min)  ? where NUnit crash happened
Step 3: IL2CPP C++ compile   (~8 min)  ? ARM64 native code
Step 4: Gradle package       (~3 min)  ? final APK
Step 5: Sign                 (~30 sec)
TOTAL:                       ~16 min

EVERY build repeats ALL steps. There is no "quick rebuild" for Android.
```

### Build checklist:
```
[ ] All compiler errors fixed (check Console)
[ ] No test files in runtime assemblies
[ ] No editor-only references in runtime code
[ ] Scene saved (Ctrl+S)
[ ] File ? Build (NOT Build And Run if no ADB)
[ ] Save as MachineNet_v1.apk in C:\MnetUni\Mnet\
```

## 4. VR-SPECIFIC RULES

### Scene Hierarchy for Quest 3:
```
Scene
??? Directional Light
??? XR Interaction Manager
??? XR Origin (VR)
?   ??? Camera Offset
?       ??? Main Camera      ? DON'T add another camera
??? C60 (our GKRenderer)
??? [optional] Event System
```

### NEVER:
- Have two cameras (Main Camera + XR camera = black screen crash)
- Use Camera.main on Android (XR Origin controls the camera)
- Set camera position/FOV on Android (XR rig handles this)
- Forget #if !UNITY_ANDROID around PC camera code

### ALWAYS:
- Position objects RELATIVE to XR Origin (0,0,0 = floor)
- Eye level = Y: 1.4 to 1.7 (average human height seated/standing)
- Comfortable distance = Z: -1.5 to -3.0 meters
- Use UnityEngine.XR.InputDevices for controller input
- Fully qualify XR types to avoid InputSystem ambiguity

## 5. CONTROLLER MAPPING (Quest 3)

```
RIGHT CONTROLLER:
  Thumbstick Y    ? Zoom (vrZoomSpeed)
  A button        ? primaryButton ? Refine
  B button        ? secondaryButton ? Reset
  Trigger         ? trigger ? Screenshot
  Grip            ? grip ? [available]

LEFT CONTROLLER:
  Thumbstick XY   ? Move C60 in space
  X button        ? primaryButton ? [available]
  Y button        ? secondaryButton ? [available]
  Trigger         ? trigger ? [available]
  Grip            ? grip ? [available]
```

## 6. PERFORMANCE BUDGETS (Quest 3)

```
Target: 72 FPS (13.8ms per frame) ? MINIMUM for comfort
        90 FPS (11.1ms per frame) ? recommended
        120 FPS (8.3ms per frame) ? smooth

Draw calls:     < 100 per eye (200 total)
Triangles:      < 750K per eye
Texture memory: < 1GB
RAM total:      < 2.5GB (Quest 3 has 8GB but OS takes 5+)

OUR C60 at level 0: 32 faces = ~64 draw calls (front+back) ? FINE
OUR C60 refined 1x: ~150 faces ? FINE
OUR C60 refined 3x: ~3000+ faces ? WATCH IT
```

## 7. DEPLOYMENT PATH

```
CURRENT (no ADB):
  Build ? APK on disk ? MTP drag to Quest ? Files app ? Install

WHEN ADB WORKS:
  Build And Run (Ctrl+B) ? auto-deploy + auto-launch

ADB PATH:
  "C:\Program Files\Unity\Hub\Editor\6000.3.16f1\...\adb.exe"

APK OUTPUT:
  C:\MnetUni\Mnet\MachineNet_v1.apk

QUEST STORAGE:
  Internal shared storage / Download / MachineNet_v1.apk
```

## 8. META QUEST SPLASH / LOADING

```
The "whoosh into VR" sequence:
  1. System splash (Player Settings ? Splash Image)
  2. Loading indicator (automatic)
  3. App starts ? first frame renders
  4. Composition Layer can overlay loading screen

Assets/CompositionLayers/ ? already in project
Configure in: XR Plug-in Management ? OpenXR ? Meta XR Features
```

## 9. THE 18 DIVINE IDEAS (apply to Unity)

```
#5  Porting Topology Theorem  ? platform #if guards
#7  Camera conflict = ? break ? never two cameras
#12 Compilation = GK.Undo()   ? respect the build graph
#13 Linker error = topology   ? tests must be Editor-only
#19 Observer-Compute Duality  ? foveated rendering from first principles
```

## 10. BEFORE ANY SESSION

```
1. Open this file
2. Read section relevant to what you're changing
3. Check Console for existing errors
4. Run tests (Edit ? Test Runner ? Run All)
5. THEN make changes
6. Build
7. Test
```

---

*The graph is sacred. Read before you cut. ?=2.*
