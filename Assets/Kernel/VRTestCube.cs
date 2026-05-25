using UnityEngine;

namespace MachineNet
{
    public class VRTestCube : MonoBehaviour
    {
        float _time = 0f;
        string _debugInfo = "";

        void Start()
        {
            // Find the XR camera - objects will be placed RELATIVE to it
            Camera vrCam = Camera.main;
            string camInfo = "null";
            
            if (vrCam != null)
            {
                camInfo = vrCam.transform.position.ToString() + 
                          " fov=" + vrCam.fieldOfView +
                          " near=" + vrCam.nearClipPlane +
                          " far=" + vrCam.farClipPlane +
                          " clear=" + vrCam.clearFlags +
                          " culling=" + vrCam.cullingMask +
                          " stereo=" + vrCam.stereoEnabled;
                
                // FORCE camera settings for VR
                vrCam.clearFlags = CameraClearFlags.SolidColor;
                vrCam.backgroundColor = new Color(0.1f, 0.0f, 0.2f); // dark purple so we know
            }
            
            _debugInfo = "Cam: " + camInfo;

            // Strategy: put objects as CHILDREN of camera so they FOLLOW the head
            Transform parent = vrCam != null ? vrCam.transform : null;

            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            string shName = sh != null ? "URP/Unlit" : "null";
            if (sh == null) { sh = Shader.Find("Unlit/Color"); shName = sh != null ? "Unlit/Color" : "null"; }
            if (sh == null) { sh = Shader.Find("Standard"); shName = sh != null ? "Standard" : "null"; }

            _debugInfo += "\nShader: " + shName;

            // Spawn cube RIGHT IN FRONT of camera, 1.5m away, BIG
            SpawnObj(PrimitiveType.Cube, parent, new Vector3(0f, 0f, 1.5f), 0.5f, Color.magenta, sh);
            // Left
            SpawnObj(PrimitiveType.Sphere, parent, new Vector3(-1f, 0f, 1.5f), 0.4f, Color.green, sh);
            // Right
            SpawnObj(PrimitiveType.Sphere, parent, new Vector3(1f, 0f, 1.5f), 0.4f, Color.red, sh);
            // Below
            SpawnObj(PrimitiveType.Cube, parent, new Vector3(0f, -0.5f, 1f), 0.3f, Color.yellow, sh);
            // GIANT sphere surrounding player
            SpawnObj(PrimitiveType.Sphere, null, new Vector3(0f, 1.5f, 0f), 10f, new Color(0f, 0.3f, 0.5f, 0.5f), sh);

            _debugInfo += "\nSpawned 5 objects";

            // Write debug to file
            try
            {
                string logPath = System.IO.Path.Combine(
                    Application.persistentDataPath, "vr_debug_v4.txt");
                string fullLog = System.DateTime.Now + "\n" + _debugInfo +
                    "\nScreen: " + Screen.width + "x" + Screen.height +
                    "\nXR: " + UnityEngine.XR.XRSettings.enabled +
                    "\nXR mode: " + UnityEngine.XR.XRSettings.loadedDeviceName +
                    "\nRenderMode: " + UnityEngine.XR.XRSettings.stereoRenderingMode;
                System.IO.File.WriteAllText(logPath, fullLog);
                _debugInfo += "\nLog written";
            }
            catch (System.Exception e)
            {
                _debugInfo += "\nLog fail: " + e.Message;
            }

            Debug.Log("[VRTest v4] " + _debugInfo);
        }

        void SpawnObj(PrimitiveType type, Transform parent, Vector3 localPos, 
                      float scale, Color color, Shader sh)
        {
            var go = GameObject.CreatePrimitive(type);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
                go.transform.localPosition = localPos;
            }
            else
            {
                go.transform.position = localPos;
            }
            go.transform.localScale = Vector3.one * scale;
            
            if (sh != null)
            {
                var mat = new Material(sh);
                mat.color = color;
                go.GetComponent<Renderer>().material = mat;
            }
        }

        void Update()
        {
            _time += Time.deltaTime;
        }

        void OnGUI()
        {
            var s = new GUIStyle(GUI.skin.label);
            s.fontSize = 24;
            s.normal.textColor = Color.white;
            s.wordWrap = true;

            GUI.Box(new Rect(5, 5, Screen.width/2, 250), "");
            GUI.Label(new Rect(10, 10, Screen.width/2 - 20, 240),
                "MNET v4\nFPS: " + (int)(1f/Time.deltaTime) + 
                "\nT: " + _time.ToString("F1") + "s\n" + _debugInfo, s);
        }
    }
}
