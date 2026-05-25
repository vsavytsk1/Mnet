// =============================================================================
//  GKAudio.cs ? Procedural Sound Design for MachineNet Opening Sequence
//
//  ALL sounds generated mathematically. No audio files needed.
//  Steampunk heavy machine aesthetic.
//
//  SEQUENCE:
//    0.0s  void hum (40hz sub-bass drone, purple void)
//    1.5s  grid tick-tick (mechanical, grid fading in)
//    3.0s  LOCK click (grid fully visible)
//    3.8s  calculate blips (PC calculation sequence)
//    4.0s  EXPAND whoosh (C60 emerges from point)
//    5.5s  CHUNK x1 (refine to F=224)
//    7.0s  CHUNK x2 (refine to F=1568)
//    8.5s  RESONANT HIT (refine to F=10976 - full detail)
//   10.0s  steady hum (running state)
// =============================================================================

using UnityEngine;
using System.Collections;

namespace MachineNet
{
    [RequireComponent(typeof(AudioSource))]
    public class GKAudio : MonoBehaviour
    {
        AudioSource _src;
        const int   SR = 44100; // sample rate

        void Awake()
        {
            _src = GetComponent<AudioSource>();
            _src.spatialBlend = 0f; // 2D ? UI sound
            _src.volume       = 0.7f;
        }

        // ?? PUBLIC API ? called by GKOpeningSequence ???????????????????????

        // Deep void hum ? 40hz sub-bass + slow wobble (the universe breathing)
        public void PlayVoidHum(float duration = 3f)
        {
            PlayClip(MakeVoidHum(duration));
        }

        // Mechanical tick ? grid materialization
        public void PlayTick()
        {
            PlayClip(MakeTick());
        }

        // Hard lock click ? grid complete
        public void PlayLock()
        {
            PlayClip(MakeLock());
        }

        // PC calculation blips ? rapid beep sequence
        public void PlayCalculate()
        {
            PlayClip(MakeCalculate());
        }

        // Expand whoosh ? C60 emerging
        public void PlayExpand()
        {
            PlayClip(MakeExpand());
        }

        // Refine chunk ? each refinement level
        public void PlayChunk(int level)
        {
            // deeper hit for higher levels
            float pitch = 1f - level * 0.15f;
            PlayClip(MakeChunk(pitch));
        }

        // Resonant hit ? final detail level
        public void PlayResonantHit()
        {
            PlayClip(MakeResonantHit());
        }

        // Steady running hum ? loop
        public void PlaySteadyHum()
        {
            var clip = MakeSteadyHum(2f);
            _src.clip = clip;
            _src.loop = true;
            _src.Play();
        }

        public void StopAll() { _src.Stop(); _src.loop = false; }

        // ?? SOUND GENERATORS (pure math ? float[]) ????????????????????????

        // 40hz sub + 80hz + slow AM wobble ? the void of space
        AudioClip MakeVoidHum(float dur)
        {
            int N = (int)(SR * dur);
            var s = new float[N];
            for (int i = 0; i < N; i++)
            {
                float t   = (float)i / SR;
                float env = Mathf.Min(1f, t * 0.8f) *         // fade in
                            Mathf.Min(1f, (dur - t) * 0.8f);  // fade out
                float am  = 0.7f + 0.3f * Mathf.Sin(2*Mathf.PI * 0.3f * t); // 0.3hz wobble
                s[i] = env * am * (
                    0.6f * Mathf.Sin(2*Mathf.PI * 40f  * t) +  // sub
                    0.3f * Mathf.Sin(2*Mathf.PI * 80f  * t) +  // octave
                    0.1f * Mathf.Sin(2*Mathf.PI * 120f * t)    // texture
                ) * 0.4f;
            }
            return MakeClip(s, "VoidHum");
        }

        // Short mechanical tick ? steel on steel
        AudioClip MakeTick()
        {
            int N = (int)(SR * 0.05f);
            var s = new float[N];
            for (int i = 0; i < N; i++)
            {
                float t   = (float)i / SR;
                float env = Mathf.Exp(-t * 120f);
                // high freq click with metallic noise
                float noise = (Random.value * 2f - 1f);
                s[i] = env * (
                    0.6f * Mathf.Sin(2*Mathf.PI * 3200f * t) +
                    0.4f * noise
                ) * 0.5f;
            }
            return MakeClip(s, "Tick");
        }

        // Hard mechanical lock ? relay snap
        AudioClip MakeLock()
        {
            int N = (int)(SR * 0.12f);
            var s = new float[N];
            for (int i = 0; i < N; i++)
            {
                float t   = (float)i / SR;
                float env = Mathf.Exp(-t * 60f);
                float noise = (Random.value * 2f - 1f);
                // two-part: click + resonance
                float click = Mathf.Exp(-t * 200f) * noise;
                float ring  = env * Mathf.Sin(2*Mathf.PI * 800f * t);
                s[i] = (click * 0.4f + ring * 0.6f) * 0.6f;
            }
            return MakeClip(s, "Lock");
        }

        // Rapid calculation blips ? ascending frequency sweep
        AudioClip MakeCalculate()
        {
            float dur = 0.6f;
            int N = (int)(SR * dur);
            var s = new float[N];
            int blips = 8;
            for (int b = 0; b < blips; b++)
            {
                float bt  = (float)b / blips * dur;
                float bfreq = 800f + b * 300f; // ascending
                int   bstart = (int)(bt * SR);
                int   blen   = (int)(0.04f * SR);
                for (int i = 0; i < blen && bstart+i < N; i++)
                {
                    float t   = (float)i / SR;
                    float env = Mathf.Sin(Mathf.PI * i / blen); // bell
                    s[bstart + i] += env * 0.3f * 
                        Mathf.Sin(2*Mathf.PI * bfreq * t);
                }
            }
            return MakeClip(s, "Calculate");
        }

        // Expand whoosh ? rising sweep + air
        AudioClip MakeExpand()
        {
            float dur = 1.2f;
            int N = (int)(SR * dur);
            var s = new float[N];
            for (int i = 0; i < N; i++)
            {
                float t   = (float)i / SR;
                float env = Mathf.Sin(Mathf.PI * t / dur);       // arch
                float freq = 100f + 2000f * (t / dur);           // sweep up
                float noise = (Random.value * 2f - 1f) * 0.15f;  // air
                s[i] = env * (
                    0.5f * Mathf.Sin(2*Mathf.PI * freq * t) +
                    noise
                ) * 0.45f;
            }
            return MakeClip(s, "Expand");
        }

        // Chunk ? metallic thud with pitch control
        AudioClip MakeChunk(float pitchMult = 1f)
        {
            float dur = 0.3f;
            int N = (int)(SR * dur);
            var s = new float[N];
            for (int i = 0; i < N; i++)
            {
                float t   = (float)i / SR;
                float env = Mathf.Exp(-t * 25f);
                float freq = 180f * pitchMult;
                float noise = (Random.value * 2f - 1f);
                s[i] = env * (
                    0.5f * Mathf.Sin(2*Mathf.PI * freq * t) +
                    0.3f * Mathf.Sin(2*Mathf.PI * freq*2.1f * t) +
                    0.2f * noise
                ) * 0.55f;
            }
            return MakeClip(s, "Chunk");
        }

        // Resonant hit ? deep bell + sub thud
        AudioClip MakeResonantHit()
        {
            float dur = 1.8f;
            int N = (int)(SR * dur);
            var s = new float[N];
            for (int i = 0; i < N; i++)
            {
                float t   = (float)i / SR;
                // sub thud
                float thud = Mathf.Exp(-t * 8f)  * Mathf.Sin(2*Mathf.PI * 55f * t);
                // bell ring
                float bell = Mathf.Exp(-t * 2.5f) * Mathf.Sin(2*Mathf.PI * 220f * t);
                // metallic overtone
                float meta = Mathf.Exp(-t * 5f)  * Mathf.Sin(2*Mathf.PI * 440f * t * 1.4f);
                s[i] = (thud * 0.5f + bell * 0.35f + meta * 0.15f) * 0.6f;
            }
            return MakeClip(s, "ResonantHit");
        }

        // Steady running hum ? for looping during simulation
        AudioClip MakeSteadyHum(float dur)
        {
            int N = (int)(SR * dur);
            var s = new float[N];
            for (int i = 0; i < N; i++)
            {
                float t  = (float)i / SR;
                float am = 0.85f + 0.15f * Mathf.Sin(2*Mathf.PI * 1.2f * t);
                s[i] = am * (
                    0.5f * Mathf.Sin(2*Mathf.PI * 60f  * t) +
                    0.3f * Mathf.Sin(2*Mathf.PI * 120f * t) +
                    0.2f * Mathf.Sin(2*Mathf.PI * 180f * t)
                ) * 0.25f;
            }
            return MakeClip(s, "SteadyHum");
        }

        // ?? UTILITY ????????????????????????????????????????????????????????

        AudioClip MakeClip(float[] samples, string name)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, SR, false);
            clip.SetData(samples, 0);
            return clip;
        }

        void PlayClip(AudioClip clip)
        {
            _src.loop = false;
            _src.PlayOneShot(clip);
        }
    }
}
