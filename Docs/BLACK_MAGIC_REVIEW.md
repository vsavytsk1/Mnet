# ASSHOLE CODE REVIEW ? MachineNet Unity
*May 25 2026. Read before touching code. Every item was real.*

---

## CRITICAL (fixed)

### 1. GKRenderer _logDir writes to Assets/ at runtime on Quest
`Application.dataPath` on Android is inside the APK (read-only).
Every LogState() call was silently failing on device.
FIX: Use `Application.persistentDataPath` on Android.

### 2. Empty Assets/Kernel/Tests/ directory tracked in git
No .asmdef = NUnit IL2CPP crash if anyone drops a .cs there.
FIX: Delete folder AND its .meta.

### 3. FindObjectsByType<Light> in SetupLighting() ? wasteful on Quest
FIX: Cache or skip on Android.

### 4. Grid mesh has vertex colors but material ignores them
Dead code. Grid is one solid color, not major/minor pattern.
FIX: Remove dead vertex color code, or use vertex-color shader.

## MEDIUM (fixed)

### 5. DestroyImmediate() used in runtime code
Should be Destroy() at runtime, DestroyImmediate() only in editor.
FIX: Conditional destroy helper.

### 6. Random.value in GKAudio ? not seeded, not reproducible
FIX: Acceptable for procedural audio. Documented as intentional.

### 7. GKRenderer OnDrawGizmos edge dedup only uses X coordinate
FIX: Use full 3-coord key like edge mesh builder does.

### 8. VRTestCube.cs spawns 10-unit sphere that obscures everything
FIX: Remove or shrink the giant sphere.

### 9. GKOpeningSequence SetActive(false/true) re-triggers Start()
FIX: Use a flag to skip re-init, or use enable/disable instead.

### 10. No null-check warning for GKAutopilot references
FIX: Log warning in Start() if references are null.

## MINOR (fixed)

### 11. GKScreenLogger fills 50 PNGs in 4 minutes
FIX: Document behavior, reduce default to active=false.

### 12. C60R magic number not documented
FIX: Add comment explaining relationship to kernel sphere radius.

### 13. Kernel asmdef couples pure math with Unity InputSystem
FIX: Acceptable for now. Document that GK.cs has zero Unity deps.

### 14. Mnet.slnx was committed then gitignored
FIX: git rm --cached.

### 15. dashboard.py / pull_screenshots.ps1 in limbo
FIX: Either commit or delete.

## WHAT'S SOLID (don't touch)
- GoldbergKernel.cs ? 18 tests, pure math, immutable state
- Test .asmdef setup ? Editor-only, correct NUnit refs
- VR input ? fully qualified XR, #if guards, edge detection
- try/catch + _crashMsg + OnGUI ? genius Quest debug pattern
- Procedural audio ? zero files, clean API
- Autopilot ? command pattern, coroutine timing

---
*All items addressed in commit "purify: fix all review items"*
