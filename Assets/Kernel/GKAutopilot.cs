// =============================================================================
//  GKAutopilot.cs ? The Animation Programming Language
//  
//  EXACT PORT of mnet_v7.html _apSequences + apRun() + apCmd()
//
//  Same structure:
//    sequences = Dictionary of named scripts
//    each script = GKStep[] { cmd, waitMs }
//    Run(name) = coroutine: foreach step -> dispatch cmd -> wait
//
//  COMMANDS (from apCmd() in mnet_v7):
//    SEED        doSeed()
//    RESET       doReset()
//    REFINE      refineAll x1
//    REFINE2     refineAll x2
//    FLOW_ON     toggleFlow on
//    FLOW_OFF    toggleFlow off
//    NANITE_ON   toggleNanite on
//    NANITE_OFF  toggleNanite off
//    MOB_ON      toggleMobius on
//    MOB_OFF     toggleMobius off
//    MOB_50      setMobiusTwist(50)
//    MOB_100     setMobiusTwist(100)
//    ROTATE_ON   autoRotate on
//    ROTATE_OFF  autoRotate off
//    RE_HI       Re = 500
//    RE_LO       Re = 50
//    ZOOM1       zoom x1
//    ZOOM2       zoom x2
//    ZOOM3       zoom x3
//    THRESH_HI   nanite threshold high
//    THRESH_MID  nanite threshold mid
//    THRESH_LO   nanite threshold low
//    SOUND_xxx   GKAudio commands
// =============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MachineNet
{
    [System.Serializable]
    public struct GKStep
    {
        public string cmd;
        public int    waitMs;
        public GKStep(string c, int w) { cmd=c; waitMs=w; }
    }

    public class GKAutopilot : MonoBehaviour
    {
        [Header("References")]
        public GKRenderer gkRenderer;
        public GKAudio    gkAudio;

        // ?? state ??????????????????????????????????????????????????????
        bool      _running = false;
        Coroutine _co      = null;

        // ?? SEQUENCES (exact port of _apSequences) ?????????????????????
        static readonly Dictionary<string, GKStep[]> Sequences =
            new Dictionary<string, GKStep[]>
        {
            // ? WAKE ? Seed ? Refine?3 ? Flow
            ["wake"] = new GKStep[] {
                new GKStep("RESET",      400),
                new GKStep("SEED",       800),
                new GKStep("ROTATE_ON",  300),
                new GKStep("RE_HI",      200),
                new GKStep("REFINE",     600),
                new GKStep("REFINE",     700),
                new GKStep("REFINE",     800),
                new GKStep("FLOW_ON",   2000),
                new GKStep("ZOOM2",      500),
            },
            // ? CONVERGE ? Refine?4 ? Flow ? Nanite ON
            ["converge"] = new GKStep[] {
                new GKStep("RESET",      400),
                new GKStep("SEED",       800),
                new GKStep("RE_HI",      200),
                new GKStep("REFINE2",    800),
                new GKStep("REFINE2",    800),
                new GKStep("FLOW_ON",   3000),
                new GKStep("NANITE_ON", 1000),
                new GKStep("THRESH_MID", 800),
                new GKStep("ZOOM2",      500),
            },
            // ? MOBIUS ? Flow ? Twist ?=2?0 ? Diverge
            ["mobius"] = new GKStep[] {
                new GKStep("RESET",      400),
                new GKStep("SEED",       800),
                new GKStep("RE_HI",      200),
                new GKStep("REFINE",     600),
                new GKStep("REFINE",     700),
                new GKStep("FLOW_ON",   4000),
                new GKStep("NANITE_ON",  500),
                new GKStep("MOB_ON",     600),
                new GKStep("MOB_50",    1500),
                new GKStep("MOB_100",   2000),
            },
            // ? TOPOLOGY ? ? proof: sphere then M?bius
            ["topology"] = new GKStep[] {
                new GKStep("RESET",      400),
                new GKStep("SEED",       800),
                new GKStep("RE_HI",      200),
                new GKStep("REFINE",     600),
                new GKStep("REFINE",     700),
                new GKStep("NANITE_ON",  400),
                new GKStep("FLOW_ON",   5000),
                new GKStep("THRESH_MID", 500),
                new GKStep("MOB_ON",     600),
                new GKStep("MOB_100",   3000),
                new GKStep("MOB_OFF",   1000),
                new GKStep("THRESH_LO",  500),
            },
            // ? NANITE ? DAG cut demo: threshold sweep
            ["nanite"] = new GKStep[] {
                new GKStep("RESET",      400),
                new GKStep("SEED",       800),
                new GKStep("RE_HI",      200),
                new GKStep("REFINE2",    800),
                new GKStep("REFINE2",    800),
                new GKStep("FLOW_ON",   2000),
                new GKStep("NANITE_ON",  800),
                new GKStep("THRESH_HI", 1500),
                new GKStep("THRESH_MID",1500),
                new GKStep("THRESH_LO", 1500),
                new GKStep("THRESH_MID",1000),
                new GKStep("ZOOM3",      800),
                new GKStep("ZOOM1",      800),
            },
            // OPENING ? the birth sequence
            ["opening"] = new GKStep[] {
                new GKStep("SOUND_HUM",    0),
                new GKStep("GRID_FADE",  800),
                new GKStep("SOUND_LOCK", 1500),
                new GKStep("SOUND_CALC", 300),
                new GKStep("SOUND_EXPAND",400),
                new GKStep("SEED",        200),
                new GKStep("ROTATE_ON",   0),
                new GKStep("REFINE",     1600),
                new GKStep("SOUND_CHUNK1",0),
                new GKStep("REFINE",     1600),
                new GKStep("SOUND_CHUNK2",0),
                new GKStep("REFINE",     1800),
                new GKStep("SOUND_BOOM",   0),
                new GKStep("SOUND_STEADY",1800),
            },
        };

        // ?? PUBLIC API ?????????????????????????????????????????????????

        public bool IsRunning => _running;

        public void Run(string seqName)
        {
            if (_running) Stop();
            if (!Sequences.ContainsKey(seqName))
            {
                Debug.LogWarning($"[GKAuto] Unknown sequence: {seqName}");
                return;
            }
            _co = StartCoroutine(Execute(seqName));
        }

        public void Stop()
        {
            if (_co != null) StopCoroutine(_co);
            _running = false;
            _co      = null;
            Debug.Log("[GKAuto] stopped");
        }

        // ?? EXECUTOR ???????????????????????????????????????????????????

        IEnumerator Execute(string seqName)
        {
            _running  = true;
            var steps = Sequences[seqName];
            Debug.Log($"[GKAuto] START {seqName.ToUpper()} ({steps.Length} steps)");

            for (int i = 0; i < steps.Length; i++)
            {
                var step = steps[i];
                Debug.Log($"[GKAuto] [{i+1}/{steps.Length}] {step.cmd}");
                Dispatch(step.cmd);
                if (step.waitMs > 0)
                    yield return new WaitForSeconds(step.waitMs / 1000f);
            }

            _running = false;
            Debug.Log($"[GKAuto] DONE {seqName.ToUpper()}");
        }

        // ?? DISPATCHER (port of apCmd) ?????????????????????????????????

        void Dispatch(string cmd)
        {
            var r = gkRenderer;
            var a = gkAudio;

            switch (cmd)
            {
                case "SEED":        if (r) { r.Seed();        } break;
                case "RESET":       if (r) { r.ResetState();  } break;
                case "REFINE":      if (r) { r.TriggerRefine();  } break;
                case "REFINE2":     if (r) { r.TriggerRefine(); r.TriggerRefine(); } break;
                case "ROTATE_ON":   if (r) { r.autoRotate = true;  } break;
                case "ROTATE_OFF":  if (r) { r.autoRotate = false; } break;
                case "RE_HI":       if (r) { r.Re = 500; } break;
                case "RE_LO":       if (r) { r.Re = 50;  } break;
                case "ZOOM1":       if (r) { r.SetZoom(1f); } break;
                case "ZOOM2":       if (r) { r.SetZoom(2f); } break;
                case "ZOOM3":       if (r) { r.SetZoom(3f); } break;
                // sound
                case "SOUND_HUM":    if (a) a.PlayVoidHum(4f);     break;
                case "SOUND_TICK":   if (a) a.PlayTick();           break;
                case "SOUND_LOCK":   if (a) a.PlayLock();           break;
                case "SOUND_CALC":   if (a) a.PlayCalculate();      break;
                case "SOUND_EXPAND": if (a) a.PlayExpand();         break;
                case "SOUND_CHUNK1": if (a) a.PlayChunk(1);         break;
                case "SOUND_CHUNK2": if (a) a.PlayChunk(2);         break;
                case "SOUND_BOOM":   if (a) a.PlayResonantHit();    break;
                case "SOUND_STEADY": if (a) a.PlaySteadyHum();      break;
                case "GRID_FADE":    StartCoroutine(FadeGrid());     break;
                // stubs for future
                case "FLOW_ON":     Debug.Log("[GKAuto] FLOW_ON ? NSFlow.cs next"); break;
                case "FLOW_OFF":    Debug.Log("[GKAuto] FLOW_OFF ? NSFlow.cs next"); break;
                case "NANITE_ON":   Debug.Log("[GKAuto] NANITE_ON ? MNetNanite.cs next"); break;
                case "MOB_ON":      Debug.Log("[GKAuto] MOB_ON ? Mobius.cs next"); break;
                case "MOB_OFF":     Debug.Log("[GKAuto] MOB_OFF"); break;
                case "MOB_50":      Debug.Log("[GKAuto] MOB_50"); break;
                case "MOB_100":     Debug.Log("[GKAuto] MOB_100"); break;
                case "THRESH_HI":   Debug.Log("[GKAuto] THRESH_HI"); break;
                case "THRESH_MID":  Debug.Log("[GKAuto] THRESH_MID"); break;
                case "THRESH_LO":   Debug.Log("[GKAuto] THRESH_LO"); break;
                default: Debug.LogWarning($"[GKAuto] unknown cmd: {cmd}"); break;
            }
        }

        IEnumerator FadeGrid()
        {
            var gridGO = GameObject.Find("GK_Grid");
            if (gridGO == null) yield break;
            var mr  = gridGO.GetComponent<MeshRenderer>();
            if (mr == null) yield break;
            var mat = mr.material;
            Color baseCol = new Color(0.078f,0.118f,0.157f,1f);
            float t = 0f;
            float dur = 1.5f;
            mr.enabled = true;
            float lastTick = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float n = t / dur;
                float smooth = n*n*(3f-2f*n);
                mat.color = new Color(baseCol.r,baseCol.g,baseCol.b,smooth);
                if (t - lastTick > 0.18f)
                {
                    if (gkAudio != null) gkAudio.PlayTick();
                    lastTick = t;
                }
                yield return null;
            }
            mat.color = baseCol;
        }

        // FIX #10: warn if references not wired in Inspector
        void Start()
        {
            if (gkRenderer == null) Debug.LogWarning("[GKAuto] gkRenderer is NULL ? wire it in Inspector!");
            if (gkAudio == null)    Debug.LogWarning("[GKAuto] gkAudio is NULL ? wire it in Inspector!");
        }

        // ?? keyboard shortcuts (editor) ????????????????????????????????
        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            // number keys run sequences
            if (kb.digit1Key.wasPressedThisFrame) Run("wake");
            if (kb.digit2Key.wasPressedThisFrame) Run("converge");
            if (kb.digit3Key.wasPressedThisFrame) Run("mobius");
            if (kb.digit4Key.wasPressedThisFrame) Run("topology");
            if (kb.digit5Key.wasPressedThisFrame) Run("nanite");
            if (kb.digit0Key.wasPressedThisFrame) Run("opening");
            if (kb.escapeKey.wasPressedThisFrame) Stop();
        }
    }
}
