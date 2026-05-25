# MachineNet ? VR

> *A fullerene is the only closed structure you can build from pentagons and hexagons.*
> *Euler proved it. Chemistry confirmed it. This repo runs it in VR.*

**C# ? Unity 6.3 ? Meta Quest 3 ? MIT License**

---

## VR Demo ? May 25, 2026

> *No cut. No edits. No filter. Raw Quest 3 recording from the cave.*

https://github.com/vsavytsk1/Mnet/raw/main/Mnet_Demo_v5.1.mp4

36 FPS. Hand tracking. Haptic feedback on contact. Buenos Aires, 2 AM.
The pipeline is proven. The C60 is next.

---

## What this is

Graph-discrete Navier-Stokes running on a C60 fullerene topology, rendered in VR on Meta Quest 3. The math kernel is certified (18/18 topology invariant tests, 30+ consecutive runs). The geometry is procedural ? zero asset files, zero textures, pure math.

**The topology is not a metaphor.** Euler's theorem forces it:

```
V - E + F = 2   (sphere topology, chi = 2)

solving: F_5 = 12, always, regardless of F_6
```

Twelve pentagons. Forever. No matter how large the graph grows.

---

## Live simulations (browser)

| Simulation | What it is |
|---|---|
| [Dodecahedron of Open Questions](https://vsavytsk1.github.io/SpookyPrimes/) | 12 open physics problems. Spin it. Click a pentagon. |
| [MachineNet v7 ? Nanite Physics DAG](https://vsavytsk1.github.io/Mnetv1/) | C60 NS solver + physics-driven LOD. The browser prototype. |

---

## The Unity kernel

```csharp
var state = GK.BuildC60();
// -> F=32, V=60, E=90, pents=12, chi=2

state = GK.RefineAll(state);
// -> F=224, V=?, E=?, pents=12, chi=2  (always)

var inv = GK.Invariants(state);
// inv.pents == 12    (Euler guarantees this)
// chi == 2           (sphere topology, always)
```

**Certified x30+.** Same math as the JavaScript kernel. Same invariants. Different language. The topology doesn't care.

---

## Project structure

```
Assets/Kernel/
  GoldbergKernel.cs     ? pure math, zero Unity deps, 18/18 tests
  GKRenderer.cs         ? procedural mesh, URP Unlit, VR controls
  GKAudio.cs            ? procedural audio (sin/cos/exp, zero files)
  GKAutopilot.cs        ? 5 scripted demo sequences
  GKOpeningSequence.cs  ? steampunk emergence animation
  VRTestCube.cs         ? VR pipeline test (camera-parented objects)
  Editor/               ? Inspector tools (Editor-only)
  Tests 1/              ? NUnit tests (Editor-only .asmdef)

Docs/
  NANITE_CRAFTSMAN.md   ? 50KB complete project reference
  UNITY_RULES.md        ? read-before-touching guide
  CODE_REVIEW.md        ? all review items (15/15 fixed)
  DIVINE_IDEAS.md       ? product ideas #20-27
  RenderStrategy/       ? VR render debug docs
```

## Build

```
Unity 6.3 LTS (6000.3.16f1)
Platform: Android (ARM64, IL2CPP)
XR: Meta OpenXR + XR Interaction Toolkit
Target: Meta Quest 3
```

Build APK: `File > Build Settings > Build`
Deploy: MTP drag-and-drop to Quest, or `adb install` when org verified.

## The key scientific finding

```
chi = 2 (sphere):   residual 0.000091  ->  CONVERGES
chi = 0 (Mobius):    residual 0.761927  ->  DIVERGES

Euler characteristic determines NS convergence on the graph.
Reproducible. Logged. 69 runs across 6 versions.
```

## Connected work

| Repo | What |
|---|---|
| [Mnetv1](https://github.com/vsavytsk1/Mnetv1) | Browser prototypes, JS kernel, 69 simulation logs |
| [SpookyPrimes](https://github.com/vsavytsk1/SpookyPrimes) | NCG research, 16-dim Dirac plateau, dodecahedron |

## License

MIT. The math is open. The geometry is open. The shape closes when it closes.

See the full [LICENSE](./LICENSE) for the honest statement.
Read the [DISCLAIMER](./DISCLAIMER.md) before putting on the headset.
Want to help? [CONTRIBUTING](./CONTRIBUTING.md) ? no gatekeeping, no CLA.

---

*@Sagaific ? Buenos Aires ? 2026*

*Steam target: $10. Quest 3 native. The cave is in your chest.*

*[Don't Panic.](./DISCLAIMER.md)*
