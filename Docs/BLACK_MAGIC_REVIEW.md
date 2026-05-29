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

---

## SESSION UPDATE -- May 28-29 2026

### 16. CAMERA INSIDE THE SPHERE (chi=2 = closed surface)
THE ONE BUG THAT KILLED THE VR BUILD.
chi=2 topology = the sphere is a closed surface.
Camera at (0,0,0) = camera INSIDE = black screen. Always.
Camera must be at r > sphere_r looking at (0,0,0).

FIX:
  sphere_r = 1.6  (GoldbergKernel default)
  camera.position.z = -(sphere_r * 1.8) = -2.88
  camera.LookAt(Vector3.zero)
  That is the ONE number. One commit.

  See: gk_spherical.py -> print_unity_camera_setup()
  See: WHITE_MAGIC_VR.md Tier 1.1

WHY IT HAPPENS:
  Mobius (chi=0) = camera has no inside/outside
  Sphere  (chi=2) = camera HAS inside/outside
  We built chi=2. We forgot the camera was inside.
  Euler punished us with a black screen.

### 17. COLAB sys.argv CRASH
navierKolmogorov.py uses sys.argv[1] for LEVEL.
Colab passes '-f' as sys.argv[1].
int('-f') = ValueError. Instant crash.

FIX: check sys.argv[1] is a digit before parsing.
  level = int(sys.argv[1]) if len(sys.argv)>1 and sys.argv[1].isdigit() else 3
  See: builder/SimGglColab/c1.py -- no sys.argv at all.

### 18. CIRCULAR DISSIPATION DIAGNOSTIC
compute_diagnostics() defined:
  diss = nu * enst * 2
Then checked:
  diss / enst == 2*nu  <- ALWAYS TRUE BY DEFINITION
The Kraichnan identity check had zero teeth.
Like checking if x/2 == x/2.

FIX: compute diss independently via Laplacian:
  diss = nu * mean(omega * (-L @ omega))
  This is independent of enstrophy.
  Now the check has teeth.
  Result: diss/enst = 0.000097 vs 2*nu = 0.000100
  97% match. Not circular. Real.
  See: builder/SimGglColab/c2.py -- honest diagnostics patch.

### 19. FAKE ENERGY SPECTRUM
compute_spectrum() sorted omega by magnitude,
ran FFT on the sorted array,
binned the result.
This is NOT E(k). It is noise with log bins.
The k^(-5/3) line was just overlaid -- not measured.

FIX: use real Laplacian eigenmodes as wavenumber basis.
  eigsh(-L, k=256) -> real eigenvectors
  project omega onto eigenvectors
  E(k) = |<phi_m, omega>|^2 / eigenvalue_m
  NOW k is real. NOW E(k) is real.
  NOW the -5/3 comparison has teeth.
  See: builder/SimGglColab/c2.py -- eigenmode spectrum.

### 20. L.toarray() MISSING FOR SCIPY SPARSE
c2.py eigsh() call needs a dense matrix for symmetrisation.
  Lsym = -0.5 * (L_cpu + L_cpu.T)
If L is scipy sparse, L_cpu.T works but arithmetic
with dense ops fails silently or crashes.

FIX:
  L_cpu = self.L.toarray() if hasattr(self.L,'toarray') else self.L
  Then astype(np.float64) before eigsh.

## WHAT IS SOLID AFTER THIS SESSION
- GoldbergKernel.cs         18/18 tests. Immutable. Sacred.
- SimGglColab c1/c2/c3      honest diagnostics. eigenmode spectrum.
- diss=2*nu*enst             CONFIRMED independent. 3 machines. 5 runs.
- E(k) ~ k^(-5/3)           OBSERVED on real eigenmodes. A100.
- gk_spherical.py            camera outside formula. soul crystal export.
- GALACTIC_LAW.md            Axiom 01 + 02. Permanent.
- DIVINE IDEAS 28-30         GKLedger, VALE soul crystal, Obsidius.

