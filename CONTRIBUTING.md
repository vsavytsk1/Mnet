# Contributing to MachineNet VR

No gatekeeping. No CLA. No Discord to join first.

---

## What's actually needed right now

Ranked by impact:

### 1. Make the C60 visible in Quest 3 (HIGHEST IMPACT)
The math works. The build works. The APK installs. The app launches.
The C60 is currently invisible in VR. This is the only problem.
One of: camera position, material color, mesh scale, or shader stripping.
See `Docs/RenderStrategy/PIPELINE_DEBUG.md` for the test sequence.

### 2. NSFlow.cs ? Navier-Stokes solver port
Port the 3 NS kernels from `mnet_v7.html` (in Mnetv1 repo) to C#.
PRESSURE, MOMENTUM, INTEGRATE. Same math. Different syntax.
Output: `float[] residual` per face ? feeds the Nanite DAG.

### 3. MNetNanite.cs ? cluster DAG port
Port `kernel/mnet_nanite.js` to C#. Physics-driven LOD.
Same algorithm as Unreal's Nanite but driven by NS residual, not pixels.
See `NANITE_CRAFTSMAN.md` sections 2-4 for the full design.

### 4. Hand tracking gestures
- Pinch on a face ? `GK.RefineOne(state, faceIdx)`
- Two-hand spread ? reset
- Look at face ? show info panel
Meta Quest 3, Meta XR Interaction SDK.

### 5. Sound design improvements
`GKAudio.cs` already generates all sounds procedurally (sin/cos/exp).
What's needed: tuning, mixing, maybe new sounds for VR events.
Zero audio files. Pure math. The Zeroth Law.

---

## How to submit

1. Fork the repo
2. Make your change in a branch
3. Open a PR with one sentence describing what changed and why
4. That's it

---

## What NOT to do

- Don't add asset files (textures, models, audio). Everything is procedural.
- Don't add cloud dependencies
- Don't break the kernel (`GoldbergKernel.cs` is SACRED ? 18/18 tests, certified x30+)
- Don't put test files outside `Tests 1/` (the NUnit IL2CPP crash ? we learned this one the hard way)
- Don't have two cameras in the scene (Main Camera + XR camera = instant black screen)

---

## The invariants you must not break

```csharp
var state = GK.BuildC60();
var inv = GK.Invariants(state);
Assert.AreEqual(12, inv.pents);    // always
Assert.AreEqual(2, inv.vertices - inv.edges + inv.faces);  // chi = 2, always
```

After ANY operation ? refine, undo, serialize, deserialize ? these hold.
If they don't, the topology is broken. The PR will not merge.

Read `Docs/UNITY_RULES.md` before touching anything. We mean it.

---

*The shape closes or it doesn't. Either way, it's real.*
*Put on the headset. The math doesn't change.*
