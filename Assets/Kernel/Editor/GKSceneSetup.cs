using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MachineNet
{
    public static class GKSceneSetup
    {
        [MenuItem("MNet/1 - Setup C60 in Scene")]
        public static void SetupC60()
        {
            var existing = GameObject.Find("C60");
            if (existing != null)
            {
                Debug.Log("[GKSetup] C60 already in scene. Skipping create.");
            }
            else
            {
                var c60 = new GameObject("C60");
                c60.transform.position = Vector3.zero;
                c60.AddComponent<GKRenderer>();
                Debug.Log("[GKSetup] C60 + GKRenderer created.");
            }

            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 1.5f, -11f);
                cam.transform.LookAt(Vector3.zero);
                Debug.Log("[GKSetup] Camera at (0,1.5,-11) looking at origin.");
            }

            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("[GKSetup] DONE. Scene saved. Hit Play.");
        }
    }
}
