# VR MECHANICS CHECKLIST ? MachineNet Unity
*From mnet_v7.html to Quest 3. One mechanic at a time.*
*Read this before every session. Update it after every session.*
*Buenos Aires ? May 2026*

---

## THE PHILOSOPHY
> "Mimic first. Then add the Vlad asset at the end like Tony."
> HTML/JS = the blueprint. Unity = the projector. VR = the destination.
> Each item: MIMIC (match html exactly) ? LOCK (test it) ? DRESS (Vlad visual asset)

---

## TIER 1 ? GEOMETRY (the shell itself)

### 1.1 C60 Base Mesh
- [ ] **MIMIC** Gold pentagons (x12) + blue hexagons (x20) visible from outside
- [ ] **MIMIC** Edges as dark thin lines between vertices
- [ ] **LOCK**  F=32 V=60 E=90 pents=12 chi=2 printed in console
- [ ] **LOCK**  PlayLog saved to TestLogs on every Play
- [ ] **DRESS** Emission glow on pentagons. Edge pulse animation.
- HTML ref: `pentFillMat`, `hexFillMat`, `nMat`, `eMat`
- Status: ?? mesh renders, black (no lighting fix yet)

### 1.2 Refine / Undo
- [ ] **MIMIC** R key = refineAll() ? F grows 32?224?1568
- [ ] **MIMIC** Z key = undo() ? F shrinks back
- [ ] **LOCK**  PlayLog_REFINE_F224_chi2.txt saved after R press
- [ ] **DRESS** Face splits with particle burst. Undo = faces collapse inward.
- HTML ref: `doRefine()`, SEED C60 button, REFINE button, ?2 button
- Status: ? R key coded, not tested yet

### 1.3 Orbit Controls (mouse/hand rotation)
- [ ] **MIMIC** Right-drag = rotate C60
- [ ] **MIMIC** Scroll = zoom in/out
- [ ] **MIMIC** Middle-drag = pan
- [ ] **LOCK**  C60 rotatable from all angles, no gimbal lock
- [ ] **DRESS** Inertia damping (dampingFactor=0.08 like Three.js OrbitControls)
- HTML ref: `OrbitControls`, `ctrl.enableDamping=true`, `ctrl.dampingFactor=0.08`
- Status: ? not implemented

---

## TIER 2 ? PHYSICS (the flow)

### 2.1 NS Flow Solver
- [ ] **MIMIC** NS FLOW button ? starts Navier-Stokes on graph
- [ ] **MIMIC** PRESSURE() ? Jacobi SOR pressure projection
- [ ] **MIMIC** MOMENTUM() ? gradP + viscous + convective
- [ ] **MIMIC** INTEGRATE() ? velocity update + sphere re-projection
- [ ] **LOCK**  Residual drops toward 0 on sphere topology (chi=2)
- [ ] **LOCK**  Residual diverges on M?bius topology (chi=0)
- [ ] **DRESS** Flow lines as animated particles along edges. Color = velocity magnitude.
- HTML ref: `toggleFlow()`, `PRESSURE()`, `MOMENTUM()`, `INTEGRATE()`
- Status: ? NSFlow.cs not written yet

### 2.2 Reynolds Number Slider
- [ ] **MIMIC** Re slider 10?500 ? changes flow regime
- [ ] **MIMIC** Re=100 default, Re=500 turbulent
- [ ] **LOCK**  Residual changes with Re value
- [ ] **DRESS** World-space slider panel. Re value glows red at high turbulence.
- HTML ref: `slRe` slider, `setRe()`, range min=10 max=500 value=100
- Status: ? not implemented

### 2.3 Jacobi Iterations Slider
- [ ] **MIMIC** Jacobi 1?20, default 8
- [ ] **LOCK**  More iterations = lower residual per step
- [ ] **DRESS** World-space slider. Bar fills as iterations increase.
- HTML ref: `slJac`, `setJacobi()`, range min=1 max=20 value=8
- Status: ? not implemented

### 2.4 SOR ? Slider
- [ ] **MIMIC** SOR ? 0.10?1.90, default 1.20
- [ ] **LOCK**  Sweet spot at ?=1.2 ? fastest convergence
- [ ] **DRESS** Gold color at sweet spot.
- HTML ref: `slSOR`, `setSOR()`, range min=0.10 max=1.90 step=0.05 value=1.20
- Status: ? not implemented

### 2.5 dt Multiplier Slider
- [ ] **MIMIC** dt 0.1??3.0?, default 1.0?
- [ ] **LOCK**  High dt = faster but unstable
- [ ] **DRESS** Red warning glow above dt=2.0
- HTML ref: `slDt`, `setDtMult()`, range min=0.1 max=3.0 step=0.1 value=1.0
- Status: ? not implemented

---

## TIER 3 ? TOPOLOGY (the finding)

### 3.1 M?bius Twist
- [ ] **MIMIC** MOBIUS button ? lerp sphere?M?bius mid-simulation
- [ ] **MIMIC** M?bius slider 0?100 ? continuous twist
- [ ] **LOCK**  chi=2 ? chi=0 transition visible in console
- [ ] **LOCK**  Residual diverges after twist (0.001 ? 0.76+)
- [ ] **DRESS** Twist animation with color shift blue?red as chi changes.
- HTML ref: `toggleMobius()`, `mobSlider`, `setMobiusTwist()`, `sphereToMobius()`
- Status: ? not implemented

### 3.2 Cage (Dodecahedral Boundary)
- [ ] **MIMIC** CAGE?? button ? 12-plane dodecahedral boundary active
- [ ] **MIMIC** CAGE_BOUNCE() ? velocity reflection at planes
- [ ] **LOCK**  Particles bounce inside cage
- [ ] **DRESS** Wireframe dodecahedron glows at boundary hits.
- HTML ref: `toggleCage()`, `CAGE_BOUNCE()`, `S.cageOn`
- Status: ? not implemented

---

## TIER 4 ? LOD / NANITE (the algorithm)

### 4.1 Cluster DAG Builder
- [ ] **MIMIC** MNetNanite.buildClusterHierarchy() in C#
- [ ] **MIMIC** 16-face clusters (analogous to Nanite's 128-tri)
- [ ] **MIMIC** Spatial grouping by anchor ID
- [ ] **LOCK**  DAG has monotonic energy (parentError >= childError)
- [ ] **DRESS** Clusters visualized as colored bounding spheres.
- HTML ref: `mnet_nanite.js ?G`, `buildClusterHierarchy()`
- Status: ? MNetNanite.cs not written yet

### 4.2 NANITE Toggle + Threshold Slider
- [ ] **MIMIC** NANITE button ? switches renderer to DAG-cut mode
- [ ] **MIMIC** Threshold slider -5?-1 (log scale), default -3
- [ ] **MIMIC** Active clusters update every 30 steps
- [ ] **LOCK**  Active faces < total faces when threshold is high
- [ ] **DRESS** LOD transition: faces pop with brief flash.
- HTML ref: `toggleNanite()`, `nthresh` slider, `selectActiveClusters()`
- Status: ? not implemented

### 4.3 Node Limit Slider (PC Edition)
- [ ] **MIMIC** Node cap 800?50000, default 5000
- [ ] **LOCK**  Refinement stops at node cap
- [ ] **DRESS** Cap indicator glows orange when near limit.
- HTML ref: `slNodeLim`, `setNodeLimit()`, `S.nodeLimit`
- Status: ? not implemented

---

## TIER 5 ? DISPLAY PANELS (the UI)

### 5.1 HUD ? Top Left
- [ ] **MIMIC** V / E / F / chi / F5 / F6 / refLevel / step / Re / mode
- [ ] **MIMIC** chi=2 ? green, chi?2 ? red
- [ ] **MIMIC** F5=12 ? gold, F5?12 ? red
- [ ] **LOCK**  All values update every frame
- [ ] **DRESS** World-space panel floating to left of C60. Holographic style.
- HTML ref: `#hud`, `$('hV')`, `$('hChi')`, `$('hF5')`
- Status: ? not implemented

### 5.2 Convergence Panel ? Top Right
- [ ] **MIMIC** N nodes / residual / ms/step / digits / trend / ?s/node / speedup
- [ ] **MIMIC** Trend: ? converging (green) / ? diverging (red) / ? steady (gold)
- [ ] **LOCK**  Residual updates every 10 steps
- [ ] **DRESS** World-space panel floating to right. Residual bar pulses on convergence.
- HTML ref: `#conv`, `$('cRes')`, `$('cTrend')`, `$('cDig')`
- Status: ? not implemented

### 5.3 Euler Banner ? Center Top
- [ ] **MIMIC** Idle: "V ? E + F = 2" with live V/E/F values
- [ ] **MIMIC** Flow: "?u/?t + (u??)u = ??p + (1/Re)??u" with live residual
- [ ] **MIMIC** Walk: "E(tessellate) < E(keep)" with walk stats
- [ ] **LOCK**  Banner switches correctly with mode
- [ ] **DRESS** Equation floats in 3D space above C60. KaTeX rendering.
- HTML ref: `#eq-banner`, `$('eq-sym')`, `$('eq-num')`, `$('eq-sub')`
- Status: ? not implemented

### 5.4 Nanite DAG Panel ? Right of Convergence
- [ ] **MIMIC** Active clusters / total clusters / active faces / max level
- [ ] **MIMIC** Violations count (hot=red, cold=green)
- [ ] **MIMIC** Monotone: YES/NO
- [ ] **MIMIC** Topology badge: chi=2 converged / chi=0 diverging / transitioning
- [ ] **MIMIC** Progress bar (nfill)
- [ ] **LOCK**  Panel updates every 30 steps when NANITE on
- [ ] **DRESS** Topology badge glows color based on chi value.
- HTML ref: `#nanite-panel`, `updateNanitePanel()`, `$('nActiveClusters')`
- Status: ? not implemented

### 5.5 Push Meter ? Bottom Left (PC Edition)
- [ ] **MIMIC** nodes / edges / jacobi / SOR / dt / residual / ms/step
- [ ] **MIMIC** Appears when node count > threshold
- [ ] **LOCK**  Updates every 10 steps
- [ ] **DRESS** Compact floating panel. Numbers pulse on state change.
- HTML ref: `#push-meter`, `updatePushMeter()`
- Status: ? not implemented

---

## TIER 6 ? MODES & SPECIAL (the advanced stuff)

### 6.1 Freeze Mode
- [ ] **MIMIC** ? FREEZE ? stops physics, renderer keeps running
- [ ] **MIMIC** Blue border overlay when frozen
- [ ] **MIMIC** ? FROZEN badge center screen
- [ ] **LOCK**  Can manipulate sliders while frozen
- [ ] **DRESS** Time-stop visual effect. Particles freeze mid-air.
- HTML ref: `toggleFreeze()`, `#freeze-overlay`, `PC.frozen`
- Status: ? not implemented

### 6.2 Background Mode
- [ ] **MIMIC** ? BG MODE ? pauses renderer, physics at full speed
- [ ] **MIMIC** Black overlay with "? BACKGROUND MODE" text
- [ ] **LOCK**  Physics step count increases rapidly in BG mode
- [ ] **DRESS** Heartbeat pulse on BG mode indicator.
- HTML ref: `toggleBgMode()`, `#bg-mode-bar`, `PC.bgMode`
- Status: ? not implemented

### 6.3 Walk Mode
- [ ] **MIMIC** WALK button ? random walk on graph nodes
- [ ] **MIMIC** Walker visits nodes, tessellates when E(tess)<E(keep)
- [ ] **MIMIC** ? TRAPPED badge when walk is complete
- [ ] **MIMIC** Walk HUD: position / visited / tessellated
- [ ] **LOCK**  Walk terminates correctly
- [ ] **DRESS** Walker visualized as glowing particle. Trail fades.
- HTML ref: `toggleWalk()`, `walkStep()`, `#whud`, `W.trapped`
- Status: ? not implemented

### 6.4 Autopilot
- [ ] **MIMIC** ? AUTOPILOT button ? scripted simulation sequences
- [ ] **MIMIC** Modes: WAKE / CONVERGE / MOBIUS / TOPOLOGY / NANITE
- [ ] **MIMIC** Each mode is a timed sequence of commands
- [ ] **LOCK**  Autopilot runs full cycle without human input
- [ ] **DRESS** Autopilot indicator pulses pink. Mode name floats in 3D.
- HTML ref: `#autopilot`, `apRun()`, `AP_SCRIPTS`, `apDispatch()`
- Status: ? not implemented

### 6.5 Save / Load State
- [ ] **MIMIC** SAVE ? serialize GKState + flow state to JSON
- [ ] **MIMIC** LOAD ? restore from JSON
- [ ] **LOCK**  Save/load round-trip preserves chi, face count, residual
- [ ] **DRESS** Save = crystal ball materializes. Load = unfolds from point.
- HTML ref: `saveState()`, `loadState()`, `GK.serialize()`, `GK.deserialize()`
- Status: ? not implemented

### 6.6 Wire / Fill / Grid Toggles
- [ ] **MIMIC** WIRE toggle ? show/hide edge mesh
- [ ] **MIMIC** FILL toggle ? show/hide face mesh
- [ ] **MIMIC** GRID toggle ? show/hide ground grid
- [ ] **LOCK**  Each toggle independent, state persists
- [ ] **DRESS** Grid = holographic hexagonal floor grid.
- HTML ref: `toggleWire()`, `toggleFill()`, `toggleGrid()`
- Status: ? not implemented

### 6.7 Zoom
- [ ] **MIMIC** ZOOM slider 1??3? ? camera FOV telescope
- [ ] **MIMIC** Zoom badge shows current multiplier
- [ ] **LOCK**  No black-out on zoom (failsafe in HTML)
- [ ] **DRESS** Lens flare effect at max zoom.
- HTML ref: `setZoom()`, `#zoom-badge`, `_zoomLevel`
- Status: ? not implemented

### 6.8 Atom Size Slider
- [ ] **MIMIC** Atom slider ? nodeRadius 0.001?0.2
- [ ] **MIMIC** Node spheres scale live
- [ ] **LOCK**  Performance maintained at min and max size
- [ ] **DRESS** At max size nodes glow like plasma balls.
- HTML ref: `slAtom`, `setAtomSize()`, `_atomR`
- Status: ? not implemented

---

## TIER 7 ? VR SPECIFIC (Quest 3 only, no HTML equivalent)

### 7.1 XR Origin + Controllers
- [ ] Hand tracking: pinch = refine face under gaze
- [ ] Hand tracking: spread = undo last refine
- [ ] Ray interactor: point at face ? highlight ? pinch ? refine
- [ ] Teleport: point at ground ? teleport inside C60
- Status: ? Meta XR SDK not installed yet

### 7.2 World-Space UI Panels
- [ ] All HTML panels ? world-space Canvas objects
- [ ] HUD panel: floats left of C60
- [ ] Convergence panel: floats right of C60
- [ ] Euler banner: floats above C60
- [ ] Nanite DAG panel: floats right of convergence panel
- [ ] Sliders: physical grab-and-drag in 3D space
- Status: ? not implemented

### 7.3 Inside / Outside mode
- [ ] Walk inside C60 (teleport to inside)
- [ ] Faces visible from inside (double-sided ? already done)
- [ ] Pentagon = 12 bosses (game mechanic)
- [ ] Hexagon = explorable patch
- Status: ? double-sided mesh done, rest not implemented

---

## CURRENT STATUS SNAPSHOT

```
Tier 1 Geometry:    ?? C60 renders (black ? lighting fix next)
Tier 2 Physics:     ? NSFlow.cs not written
Tier 3 Topology:    ? M?bius not ported
Tier 4 LOD/Nanite:  ? MNetNanite.cs not written
Tier 5 Panels:      ? no world-space UI yet
Tier 6 Modes:       ? not implemented
Tier 7 VR:          ? no XR rig yet
```

## NEXT SESSION STARTS HERE

```
1. READ THIS MD
2. CHECK TestLogs ? run Ctrl+Shift+T, confirm 18/18
3. Fix Tier 1.1: lighting ? C60 visible (not black)
   ? Add AmbientLight + DirectionalLight aim at C60
   ? OR use Unlit shader (fastest)
4. Fix Tier 1.2: test R key refine ? PlayLog_REFINE
5. Fix Tier 1.3: orbit controls ? right-drag rotates
6. LOCK Tier 1 ? then move to Tier 2 (NSFlow.cs)
```

---

*mnet_v7.html = 2123 lines. Every feature documented above.*
*Unity port = mimic first, lock with tests, dress with Vlad assets.*
*The skeleton is legal. The crazy is optional. Both are beautiful.*
