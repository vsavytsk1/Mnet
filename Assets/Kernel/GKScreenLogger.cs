// =============================================================================
//  GKScreenLogger.cs
//  Auto-screenshot every N seconds. Saves to persistentDataPath.
//  Accessible via Windows Explorer:
//  Quest 3 -> Android -> data -> com.vladmnet.machinenet -> files -> screenshots/
// =============================================================================

using UnityEngine;
using System.IO;
using System.Collections;

namespace MachineNet
{
    public class GKScreenLogger : MonoBehaviour
    {
        [Header("Auto Screenshot")]
        public float intervalSeconds = 5f;
        public bool  active          = true;
        public int   maxShots        = 50; // dont fill storage

        string _dir;
        int    _count = 0;

        void Start()
        {
            _dir = Path.Combine(Application.persistentDataPath, "screenshots");
            Directory.CreateDirectory(_dir);
            Debug.Log($"[GKScreen] Saving to: {_dir}");

            if (active) StartCoroutine(AutoShot());
        }

        IEnumerator AutoShot()
        {
            // wait 2s for scene to fully load first
            yield return new WaitForSeconds(2f);

            while (active && _count < maxShots)
            {
                string ts   = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string file = Path.Combine(_dir, $"vr_{ts}_{_count:D3}.png");
                ScreenCapture.CaptureScreenshot(file);
                Debug.Log($"[GKScreen] Shot {_count}: {file}");
                _count++;
                yield return new WaitForSeconds(intervalSeconds);
            }
        }
    }
}
