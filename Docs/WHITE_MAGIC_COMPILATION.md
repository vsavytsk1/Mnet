# Compilation as Reverse Goldberg Refinement
*An observation made while waiting for IL2CPP to strip NUnit from an Android build.*
*Buenos Aires, May 24 2026, ~8PM, headset on desk, compiler running.*

---

## The Observation

A compiler build pipeline does the EXACT same operation as GK.Undo():

```
SOURCE CODE     (big, flexible, human-readable, full of options)
     |
     v
STRIP / DCE     (remove dead code, unused paths, unreachable branches)
     |
     v
LINK            (resolve references, connect only what survives)
     |
     v
COMPRESS        (squeeze to minimum representation)
     |
     v
BINARY          (the irreducible closed surface ? the C60 of your program)
```

This is Goldberg refinement IN REVERSE:
- RefineAll() = add faces, increase resolution, more detail
- Compilation = REMOVE faces, decrease to minimum, strip to essentials
- The APK/binary is the C60 ? the minimal closed topology of your logic

## The Formal Parallel

| Goldberg-Coxeter          | IL2CPP / Compiler            |
|---------------------------|------------------------------|
| GK.RefineAll()            | Write more code              |
| GK.Undo()                 | Dead Code Elimination (DCE)  |
| V - E + F = 2             | Program correctness invariant|
| chi = 2 (sphere)          | Valid build (compiles)       |
| chi = 0 (Mobius)          | Invalid build (linker error) |
| 12 pentagons always       | Core dependencies always     |
| Faces at level 0          | Entry point + main loop      |
| Faces at level N          | Library code, utilities      |
| NS residual converges     | Build size stabilizes        |
| NS residual diverges      | Build bloat / dependency hell|

## Prior Art (it's real)

### Graph Reduction (Wadsworth 1971)
- Program = directed acyclic graph
- Evaluation = rewriting (reducing) parts of the graph
- Lazy evaluation = don't compute what you don't need
- "Outermost graph reduction" = evaluate only what the output requires
- EXACTLY what a linker does: trace from entry point, keep only reachable nodes

### Dead Code Elimination (compiler theory)
- Remove code that doesn't affect program results
- Works at instruction level, function level, module level
- IL2CPP's "ManagedStripped" = aggressive DCE for mobile
- The NUnit crash: IL2CPP tried to strip a test that referenced
  an editor-only assembly. The topology broke. chi went to 0.
  The build diverged. Same as Mobius.

### Dynamic Dead Code Elimination (Paul 1997, FreeKEYB)
- Byte-level granular DCE at LOAD TIME (not compile time)
- Driver builds its own runtime image based on detected hardware
- "Minimize memory footprint down to close to canonical form"
- CANONICAL FORM. That's what we call C60.

## The Insight

The NUnit build failure was literally a topology violation:

```
GKTests.cs in Assets/Kernel/Tests/ (no asmdef)
     |
     v
IL2CPP tries to include it in Android build
     |
     v
References nunit.framework.dll (editor-only, not in Android)
     |
     v
Unresolved reference = broken edge = open surface = chi != 2
     |
     v
BUILD DIVERGES (Fatal error in Unity CIL Linker)
```

The fix: delete the duplicate file. Restore chi = 2. Build converges.

The test file was a face that didn't belong on the Android surface.
Removing it closed the topology. The build became a valid sphere again.

## The 4th PDF

Add to the list:

1. "Topological Invariants as Portability Certificates"
2. "Why Porting Fails: A Topology Diagnosis"
3. "MachineNet as a Portability Test Bench"
4. **"Compilation as Reverse Goldberg Refinement: Why Build Failures
    Are Topology Violations"**

Key claim: A successful build is a closed surface (chi=2).
A failed build is an open surface (chi<2).
Dead code elimination is the compiler performing GK.Undo()
until the minimal closed topology remains.
The linker error IS the divergent residual.

## The Quote That Started It

"I can imagine this debugging for a big GTA game hahaha my god"
  ? V.S., watching IL2CPP strip shader variants for 8 minutes

---

*Written during a 474-second build. The compiler was doing topology.*
*We were watching. Now we know.*
