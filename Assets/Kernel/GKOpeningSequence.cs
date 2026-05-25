// =============================================================================
//  GKOpeningSequence.cs v2
//
//  SEQUENCE:
//    0.0s  PURE BLACK hold (camera bg #000000)
//    0.3s  purple void bleeds in (#0a0014 ? #050510)
//    0.8s  deep hum starts (40hz sub-bass)
//    1.8s  grid ticks in over 1.5s (tick tick tick)
//    3.3s  LOCK click ? grid complete
//    3.6s  calculate blips (8 ascending)
//    4.0s  center flash + EXPAND whoosh
//    4.2s  C60 emerges (F=32, scale 0?1)
//    5.8s  CHUNK ? refine F=224
//    7.2s  CHUNK ? refine F=1568
//    8.8s  RESONANT HIT ? refine F=10976
//   10.5s  steady hum. DONE.
// =============================================================================

using UnityEngine;
using System.Collections;

namespace MachineNet
{
    public class GKOpeningSequence : MonoBehaviour
    {
        [Header("References")]
        public GKRenderer gkRenderer;
        public GKAudio    gkAudio;

        [Header("Timing")]
        public float blackHold        = 0.3f;
        public float purpleFadeIn     = 0.5f;
        public float humStart         = 0.8f;
        public float gridFadeStart    = 1.8f;
        public float gridFadeDuration = 1.5f;
        public float c60AppearTime    = 4.2f;
        public float refine1Time      = 5.8f;
        public float refine2Time      = 7.2f;
        public float refine3Time      = 8.8f;
        public float sequenceDone     = 10.5f;

        // pure black
        static readonly Color BLACK  = new Color(0f,    0f,    0f,    1f);
        // deep purple void ? #0a0014
        static readonly Color VOID   = new Color(0.039f,0f,    0.078f,1f);
        // near-black space ? #050510 (matches GKRenderer BG)
        static readonly Color SPACE  = new Color(0.020f,0.020f,0.063f,1f);

        MeshRenderer _gridRenderer;
        Camera       _cam;
        bool         _hasRun = false; // FIX #9: guard against re-init from SetActive

        void Start()
        {
            if (_hasRun) return; // FIX #9: don't re-init if SetActive(true) called again
            _hasRun = true;
            _cam = Camera.main;

            // FRAME 0: pure black
            if (_cam != null)
            {
                _cam.backgroundColor = BLACK;
                _cam.clearFlags = CameraClearFlags.SolidColor;
            }

            // hide C60 until we trigger it
            if (gkRenderer != null)
                gkRenderer.gameObject.SetActive(false);

            // get grid ? start invisible
            var gridGO = GameObject.Find("GK_Grid");
            if (gridGO != null)
                _gridRenderer = gridGO.GetComponent<MeshRenderer>();

            if (_gridRenderer != null)
                _gridRenderer.enabled = false;

            StartCoroutine(RunSequence());
        }

        IEnumerator RunSequence()
        {
            // ?? 0.0s: pure black hold ??????????????????????????????????
            yield return new WaitForSeconds(blackHold);

            // ?? 0.3s: purple void bleeds in ???????????????????????????
            float t = 0f;
            while (t < purpleFadeIn)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / purpleFadeIn);
                if (_cam != null)
                    _cam.backgroundColor = Color.Lerp(BLACK, VOID, n);
                yield return null;
            }

            // ?? 0.8s: deep hum starts ?????????????????????????????????
            yield return new WaitForSeconds(humStart - blackHold - purpleFadeIn);
            if (gkAudio != null) gkAudio.PlayVoidHum(4f);

            // ?? 1.8s: grid fades in with ticks ????????????????????????
            yield return new WaitForSeconds(gridFadeStart - humStart);
            if (_gridRenderer != null) _gridRenderer.enabled = true;

            // fade grid alpha + fade bg from VOID to SPACE
            t = 0f;
            var gridMat = _gridRenderer != null
                ? _gridRenderer.material : null;

            // set grid start color (invisible)
            Color gridBase = new Color(0.078f, 0.118f, 0.157f, 1f); // #141e28
            if (gridMat != null) gridMat.color = new Color(
                gridBase.r, gridBase.g, gridBase.b, 0f);

            float lastTick = 0f;
            while (t < gridFadeDuration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / gridFadeDuration);
                float smooth = n * n * (3f - 2f * n); // smoothstep

                // fade grid in
                if (gridMat != null)
                    gridMat.color = new Color(
                        gridBase.r, gridBase.g, gridBase.b, smooth);

                // fade bg VOID ? SPACE
                if (_cam != null)
                    _cam.backgroundColor = Color.Lerp(VOID, SPACE, smooth);

                // tick every 0.18s
                if (t - lastTick > 0.18f)
                {
                    if (gkAudio != null) gkAudio.PlayTick();
                    lastTick = t;
                }
                yield return null;
            }

            // ?? 3.3s: LOCK ????????????????????????????????????????????
            if (_cam != null) _cam.backgroundColor = SPACE;
            if (gridMat != null) gridMat.color = gridBase;
            if (gkAudio != null) gkAudio.PlayLock();
            yield return new WaitForSeconds(0.3f);

            // ?? 3.6s: calculate blips ?????????????????????????????????
            if (gkAudio != null) gkAudio.PlayCalculate();
            yield return new WaitForSeconds(0.4f);

            // ?? 4.0s: center flash ????????????????????????????????????
            // brief white flash on camera bg then back to SPACE
            StartCoroutine(CenterFlash());
            yield return new WaitForSeconds(0.2f);

            // ?? 4.2s: C60 appears + expand whoosh ????????????????????
            if (gkAudio != null) gkAudio.PlayExpand();
            if (gkRenderer != null)
                gkRenderer.gameObject.SetActive(true);
            // GKRenderer.Start() now runs ? emergence animation begins

            yield return new WaitForSeconds(refine1Time - c60AppearTime);

            // ?? 5.8s: REFINE x1 ? F=224 ??????????????????????????????
            if (gkAudio != null) gkAudio.PlayChunk(1);
            if (gkRenderer != null) gkRenderer.TriggerRefine();
            yield return new WaitForSeconds(refine2Time - refine1Time);

            // ?? 7.2s: REFINE x2 ? F=1568 ?????????????????????????????
            if (gkAudio != null) gkAudio.PlayChunk(2);
            if (gkRenderer != null) gkRenderer.TriggerRefine();
            yield return new WaitForSeconds(refine3Time - refine2Time);

            // ?? 8.8s: RESONANT HIT ? F=10976 ?????????????????????????
            if (gkAudio != null) gkAudio.PlayResonantHit();
            if (gkRenderer != null) gkRenderer.TriggerRefine();
            yield return new WaitForSeconds(sequenceDone - refine3Time);

            // ?? 10.5s: steady hum. sequence complete. ?????????????????
            if (gkAudio != null) gkAudio.PlaySteadyHum();
            Debug.Log("[GKSeq] COMPLETE. F=10976. chi=2. Sequence done.");
        }

        IEnumerator CenterFlash()
        {
            // brief white flash then back to SPACE
            float t = 0f;
            float dur = 0.15f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float n = 1f - t / dur;
                if (_cam != null)
                    _cam.backgroundColor = Color.Lerp(SPACE,
                        new Color(0.8f, 0.9f, 1f, 1f), n * n);
                yield return null;
            }
            if (_cam != null) _cam.backgroundColor = SPACE;
        }
    }
}
