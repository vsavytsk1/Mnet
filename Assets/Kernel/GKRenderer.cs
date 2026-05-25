// =============================================================================
//  GKRenderer.cs  v5 ? VR READY ? OPENING SEQUENCE
//  Black background. Grid floor. C60 emerges from center. Properly lit.
//  Matches mnet_v7.html opening exactly.
//
//  HTML ref:
//    background: #050510
//    cam.position.set(0, 6, 12)
//    GridHelper(16, 16, 0x141e28, 0x0a1018)
//    C60 appears after BEGIN click (here: appears on Play)
//    pentFillMat: color 0xff6030, opacity 0.45, DoubleSide
//    hexFillMat:  color 0x004466, opacity 0.35, DoubleSide
//    eMat:        color 0x00d4ff, opacity 0.5
//    nMat:        color 0x00d4ff (node spheres)
//    pMat:        color 0xffd700 (pentagon nodes)
//
//  KEYBOARD:
//    R     - refine all
//    Z     - (undo ? future)
//    Space - toggle auto-rotate
//    L     - manual log
// =============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MachineNet
{
    public class GKRenderer : MonoBehaviour
    {
        // ?? inspector ??????????????????????????????????????????????????????
        [Header("Geometry")]
        public int autoRefineOnStart = 0;

        [Header("Emergence")]
        public float emergeDuration  = 1.4f;   // seconds to full size
        public AnimationCurve emergeEase = AnimationCurve.EaseInOut(0,0,1,1);

        [Header("Auto-rotate")]
        public bool  autoRotate  = true;   // ON by default for VR
        public float rotateSpeed = 14f;

        [Header("VR Placement")]
        public Vector3 vrStartPos   = new Vector3(0f, 1.4f, -2f);
        public float   vrStartScale = 1.5f;
        public float   vrZoomSpeed  = 2f;
        public float   vrMoveSpeed  = 1.5f;

        // ?? HTML color palette ?????????????????????????????????????????????
        // pentFillMat: #ff6030  opacity 0.45
        // hexFillMat:  #004466  opacity 0.35
        // eMat:        #00d4ff  opacity 0.5
        // nMat nodes:  #00d4ff
        // pMat pents:  #ffd700
        // bg:          #050510
        static readonly Color BG_COLOR   = new Color(0.020f, 0.020f, 0.063f, 1f);
        static readonly Color PENT_COLOR = new Color(1.000f, 0.376f, 0.188f, 1f); // #ff6030
        static readonly Color HEX_COLOR  = new Color(0.000f, 0.267f, 0.400f, 1f); // #004466
        static readonly Color EDGE_COLOR = new Color(0.000f, 0.831f, 1.000f, 1f); // #00d4ff
        static readonly Color NODE_PENT  = new Color(1.000f, 0.843f, 0.000f, 1f); // #ffd700
        static readonly Color NODE_HEX   = new Color(0.000f, 0.831f, 1.000f, 1f); // #00d4ff
        static readonly Color GRID_MAIN  = new Color(0.078f, 0.118f, 0.157f, 1f); // #141e28
        static readonly Color GRID_SUB   = new Color(0.039f, 0.063f, 0.094f, 1f); // #0a1018

        // ?? C60 raw radius (PHI-based kernel coords) ???????????????????????
        const float C60R = 4.956036f;

        // ?? state ??????????????????????????????????????????????????????????
        GKState    _state;
        GameObject _meshRoot;
        GameObject _gridObj;
        Material   _pentMat, _hexMat, _edgeMat;
        bool       _dirty = true;
        string     _logDir;

        // emergence
        bool  _emerging = false;
        float _emergeT  = 0f;

        // VR input state
        float _vrZoom = 1f;
        bool  _vrAWasPressed = false;
        bool  _vrBWasPressed = false;
        bool  _vrTriggerWasPressed = false;

        // ?? lifecycle ??????????????????????????????????????????????????????

        // crash display
        string _crashMsg = null;

        void OnGUI()
        {
            if (_crashMsg == null) return;
            var style = new GUIStyle(GUI.skin.box);
            style.fontSize  = 28;
            style.wordWrap  = true;
            style.normal.textColor = Color.red;
            GUI.Box(new Rect(10, 10, Screen.width-20, Screen.height-20),
                    "CRASH:\n" + _crashMsg, style);
        }

        void Start()
        {
          try
          {
            _logDir = Path.Combine(Application.dataPath,
                      "Kernel", "Tests 1", "TestLogs");
            Directory.CreateDirectory(_logDir);

            // ?? scene: black background (PC only - Quest uses XR rig) ??
            #if !UNITY_ANDROID
            var cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = BG_COLOR;
                cam.clearFlags      = CameraClearFlags.SolidColor;
                cam.transform.position = new Vector3(0f, 4f, 9f);
                cam.transform.LookAt(Vector3.zero);
                cam.fieldOfView = 50f;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane  = 500f;
            }
            #endif

            // ?? scene lighting ??
            SetupLighting();

            // ?? grid floor (HTML: GridHelper(16,16,0x141e28,0x0a1018)) ??
            BuildGrid();

            // ?? materials ??
            BuildMaterials();

            // ?? C60 ? position for VR ??
            _state = GK.BuildC60();
            for (int i = 0; i < autoRefineOnStart; i++)
                _state = GK.RefineAll(_state);

            #if UNITY_ANDROID
            // VR: place at eye level, 2m in front, full size, no emergence
            transform.position   = vrStartPos;
            transform.localScale = Vector3.one * vrStartScale;
            _emerging = false;
            autoRotate = true;
            #else
            // PC: emerge from zero
            transform.localScale = Vector3.zero;
            _emerging = true;
            _emergeT  = 0f;
            #endif

            _dirty = true;
            LogState("START");
          }
          catch (System.Exception e)
          {
            _crashMsg = e.GetType().Name + "\n" + e.Message + "\n" + e.StackTrace;
            Debug.LogError("[GK] START CRASH: " + e);
          }
        }

        void OnDestroy()
        {
            ClearMesh();
            KillMats();
            if (_gridObj != null) DestroyImmediate(_gridObj);
        }

        // ?? update ?????????????????????????????????????????????????????????

        void Update()
        {
          try
          {
            // emergence animation
            if (_emerging)
            {
                _emergeT += Time.deltaTime / emergeDuration;
                float t  = Mathf.Clamp01(_emergeT);
                float s  = emergeEase.Evaluate(t);
                transform.localScale = Vector3.one * s;
                if (t >= 1f) { _emerging = false; transform.localScale = Vector3.one; }
            }

            // auto-rotate
            if (autoRotate && !_emerging)
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

            // ?? VR controller input ??
            #if UNITY_ANDROID
            HandleVRInput();
            #endif

            // keyboard
            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.rKey.wasPressedThisFrame)
                {
                    _state = GK.RefineAll(_state);
                    _dirty = true;
                    LogState("REFINE");
                }
                if (kb.spaceKey.wasPressedThisFrame)
                {
                    autoRotate = !autoRotate;
                    Debug.Log($"[GK] AutoRotate: {autoRotate}");
                }
                if (kb.lKey.wasPressedThisFrame)
                    LogState("MANUAL");
                if (kb.sKey.wasPressedThisFrame)
                    TakeScreenshot();
            }

            // orbit: mouse on PC, skip on Quest (hand tracking handles it)
            #if !UNITY_ANDROID
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                {
                    var delta = mouse.delta.ReadValue();
                    transform.Rotate(Vector3.up,   -delta.x * 0.3f, Space.World);
                    transform.Rotate(Vector3.right,  delta.y * 0.3f, Space.Self);
                }
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f && Camera.main != null)
                    Camera.main.transform.position +=
                        Camera.main.transform.forward * scroll * 0.01f;
            }
            #endif

            if (_dirty)
            {
                _dirty = false;
                #if UNITY_ANDROID
                StartCoroutine(RebuildMeshAsync());
                #else
                RebuildMesh();
                #endif
            }
          }
          catch (System.Exception e)
          {
            _crashMsg = e.GetType().Name + "\n" + e.Message;
            Debug.LogError("[GK] UPDATE CRASH: " + e);
          }
        }

        // ?? lighting ???????????????????????????????????????????????????????
        // HTML: AmbientLight(0x606080, 0.7) + DirectionalLight(0xffffff, 1.0) at (5,10,7)

        void SetupLighting()
        {
            // ambient via RenderSettings
            RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.376f, 0.376f, 0.502f) * 0.7f;

            // directional ? find existing or create
            Light dirLight = null;
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { dirLight = l; break; }

            if (dirLight == null)
            {
                var go = new GameObject("DirectionalLight");
                dirLight = go.AddComponent<Light>();
            }
            dirLight.type      = LightType.Directional;
            dirLight.color     = Color.white;
            dirLight.intensity = 1.0f;
            // HTML: dL.position.set(5, 10, 7)
            dirLight.transform.rotation = Quaternion.LookRotation(
                new Vector3(-5f, -10f, -7f).normalized);
        }

        // ?? grid ???????????????????????????????????????????????????????????
        // HTML: new THREE.GridHelper(16, 16, 0x141e28, 0x0a1018)

        void BuildGrid()
        {
            if (_gridObj != null) DestroyImmediate(_gridObj);

            _gridObj = new GameObject("GK_Grid");
            _gridObj.transform.position = new Vector3(0f, -1.2f, 0f);

            var mf = _gridObj.AddComponent<MeshFilter>();
            var mr = _gridObj.AddComponent<MeshRenderer>();

            // Build grid mesh: 16?16 squares over 16 units
            int   divs  = 16;
            float size  = 16f;
            float step  = size / divs;
            float half  = size * 0.5f;

            var verts = new List<Vector3>();
            var tris  = new List<int>();
            var cols  = new List<Color>();

            for (int i = 0; i <= divs; i++)
            {
                float x = -half + i * step;
                bool major = (i % 4 == 0);
                Color c = major ? GRID_MAIN : GRID_SUB;

                // line along Z
                int vi = verts.Count;
                verts.Add(new Vector3(x,        0f, -half));
                verts.Add(new Vector3(x + 0.01f,0f, -half));
                verts.Add(new Vector3(x + 0.01f,0f,  half));
                verts.Add(new Vector3(x,        0f,  half));
                cols.Add(c); cols.Add(c); cols.Add(c); cols.Add(c);
                tris.AddRange(new[]{vi,vi+2,vi+1, vi,vi+3,vi+2});

                // line along X
                vi = verts.Count;
                verts.Add(new Vector3(-half, 0f, x));
                verts.Add(new Vector3( half, 0f, x));
                verts.Add(new Vector3( half, 0f, x + 0.01f));
                verts.Add(new Vector3(-half, 0f, x + 0.01f));
                cols.Add(c); cols.Add(c); cols.Add(c); cols.Add(c);
                tris.AddRange(new[]{vi,vi+2,vi+1, vi,vi+3,vi+2});
            }

            var mesh = new Mesh();
            mesh.vertices  = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.colors    = cols.ToArray();
            mesh.RecalculateNormals();
            mf.sharedMesh  = mesh;

            var gridMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            gridMat.color = GRID_MAIN;
            mr.sharedMaterial = gridMat;
        }

        // ?? mesh rebuild ???????????????????????????????????????????????????

        void RebuildMesh()
        {
            ClearMesh();
            if (_state == null) return;

            _meshRoot = new GameObject("C60_Mesh");
            _meshRoot.transform.SetParent(transform, false);

            var inv = GK.Invariants(_state);

            // faces ? double-sided, emit color
            foreach (var face in _state.faces)
            {
                bool isPent = face.type == "pent";
                var go  = new GameObject(isPent ? "Pent" : "Hex");
                go.transform.SetParent(_meshRoot.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = BuildFaceMesh(face);
                go.AddComponent<MeshRenderer>().sharedMaterial =
                    isPent ? _pentMat : _hexMat;
            }

            // edges ? combined mesh
            var eMesh = BuildEdgeMesh(_state);
            if (eMesh != null)
            {
                var ego = new GameObject("Edges");
                ego.transform.SetParent(_meshRoot.transform, false);
                ego.AddComponent<MeshFilter>().sharedMesh = eMesh;
                ego.AddComponent<MeshRenderer>().sharedMaterial = _edgeMat;
            }

            Debug.Log($"[GK] Rebuilt: F={inv.faces} V={inv.vertices} " +
                      $"E={inv.edges} pents={inv.pents} chi={inv.vertices-inv.edges+inv.faces}");
        }

        // Quest-safe: yield between face batches
        System.Collections.IEnumerator RebuildMeshAsync()
        {
            ClearMesh();
            if (_state == null) yield break;

            _meshRoot = new GameObject("C60_Mesh");
            _meshRoot.transform.SetParent(transform, false);

            var inv = GK.Invariants(_state);
            int batch = 0;

            foreach (var face in _state.faces)
            {
                bool isPent = face.type == "pent";
                var go = new GameObject(isPent ? "Pent" : "Hex");
                go.transform.SetParent(_meshRoot.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = BuildFaceMesh(face);
                go.AddComponent<MeshRenderer>().sharedMaterial =
                    isPent ? _pentMat : _hexMat;

                batch++;
                if (batch % 8 == 0) yield return null; // breathe every 8 faces
            }

            var eMesh = BuildEdgeMesh(_state);
            if (eMesh != null)
            {
                var ego = new GameObject("Edges");
                ego.transform.SetParent(_meshRoot.transform, false);
                ego.AddComponent<MeshFilter>().sharedMesh = eMesh;
                ego.AddComponent<MeshRenderer>().sharedMaterial = _edgeMat;
            }

            Debug.Log($"[GK] Rebuilt: F={inv.faces} V={inv.vertices} " +
                      $"E={inv.edges} pents={inv.pents} chi={inv.vertices-inv.edges+inv.faces}");
        }

        void ClearMesh()
        {
            if (_meshRoot != null) DestroyImmediate(_meshRoot);
            _meshRoot = null;
        }

        // ?? face mesh (double-sided fan) ???????????????????????????????????

        Mesh BuildFaceMesh(GKFace face)
        {
            var pts = face.pts;
            int n   = pts.Length;
            float cx=0,cy=0,cz=0;
            foreach(var p in pts){cx+=p[0];cy+=p[1];cz+=p[2];}
            cx/=n; cy/=n; cz/=n;

            var verts = new Vector3[n+1];
            verts[0]  = new Vector3(cx/C60R, cy/C60R, cz/C60R);
            for(int i=0;i<n;i++)
                verts[i+1] = new Vector3(pts[i][0]/C60R, pts[i][1]/C60R, pts[i][2]/C60R);

            // double-sided: n*6 indices
            var tris = new int[n*6];
            for(int i=0;i<n;i++)
            {
                tris[i*6+0]=0; tris[i*6+1]=i+1; tris[i*6+2]=(i+1)%n+1; // front
                tris[i*6+3]=0; tris[i*6+4]=(i+1)%n+1; tris[i*6+5]=i+1; // back
            }
            var m=new Mesh();
            m.vertices=verts; m.triangles=tris;
            m.RecalculateNormals();
            return m;
        }

        // ?? edge mesh (thin quads) ?????????????????????????????????????????

        Mesh BuildEdgeMesh(GKState state)
        {
            var seen  = new HashSet<string>();
            var verts = new List<Vector3>();
            var tris  = new List<int>();
            float w   = 0.006f;

            foreach(var face in state.faces)
            {
                var pts = face.pts;
                for(int i=0;i<pts.Length;i++)
                {
                    var a=pts[i]; var b=pts[(i+1)%pts.Length];
                    string ka=$"{a[0]:F3},{a[1]:F3},{a[2]:F3}";
                    string kb=$"{b[0]:F3},{b[1]:F3},{b[2]:F3}";
                    string key=string.Compare(ka,kb)<0?ka+"|"+kb:kb+"|"+ka;
                    if(seen.Contains(key))continue; seen.Add(key);

                    var A=new Vector3(a[0]/C60R,a[1]/C60R,a[2]/C60R);
                    var B=new Vector3(b[0]/C60R,b[1]/C60R,b[2]/C60R);
                    var dir=(B-A).normalized;
                    var perp=Vector3.Cross(dir,((A+B)*0.5f).normalized).normalized*w;

                    int vi=verts.Count;
                    verts.Add(A-perp); verts.Add(A+perp);
                    verts.Add(B+perp); verts.Add(B-perp);
                    tris.AddRange(new[]{vi,vi+1,vi+2, vi,vi+2,vi+3});
                }
            }
            if(verts.Count==0)return null;
            var m=new Mesh();
            m.vertices=verts.ToArray(); m.triangles=tris.ToArray();
            m.RecalculateNormals();
            return m;
        }

        // ?? materials ??????????????????????????????????????????????????????
        // Use Unlit for faces ? color is authoritative, no lighting math needed
        // Matches HTML: MeshPhongMaterial colors are designer-exact

        void BuildMaterials()
        {
            var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color");

            _pentMat  = MakeUnlit(unlitShader, PENT_COLOR);
            _hexMat   = MakeUnlit(unlitShader, HEX_COLOR);
            _edgeMat  = MakeUnlit(unlitShader, EDGE_COLOR);
        }

        Material MakeUnlit(Shader s, Color c)
        {
            var m = new Material(s);
            m.color = c;
            return m;
        }

        void KillMats()
        {
            foreach(var m in new[]{_pentMat,_hexMat,_edgeMat})
                if(m!=null) DestroyImmediate(m);
        }

        // ?? gizmos (scene view) ????????????????????????????????????????????

        void OnDrawGizmos()
        {
            if (_state == null) return;
            foreach(var face in _state.faces)
            {
                Gizmos.color = face.type=="pent"
                    ? new Color(1f,0.84f,0f,1f)
                    : new Color(0f,0.83f,1f,0.8f);
                foreach(var pt in face.pts)
                    Gizmos.DrawSphere(
                        transform.position+new Vector3(pt[0]/C60R,pt[1]/C60R,pt[2]/C60R),
                        0.04f);
            }
            Gizmos.color = new Color(0f,0.83f,1f,0.4f);
            var seen=new HashSet<string>();
            foreach(var face in _state.faces)
            {
                var pts=face.pts;
                for(int i=0;i<pts.Length;i++)
                {
                    var a=pts[i];var b=pts[(i+1)%pts.Length];
                    string ka=$"{a[0]:F3}";string kb=$"{b[0]:F3}";
                    string key=string.Compare(ka,kb)<0?ka+kb:kb+ka;
                    if(seen.Contains(key))continue;seen.Add(key);
                    Gizmos.DrawLine(
                        transform.position+new Vector3(a[0]/C60R,a[1]/C60R,a[2]/C60R),
                        transform.position+new Vector3(b[0]/C60R,b[1]/C60R,b[2]/C60R));
                }
            }
        }


        // ?? VR controller input ??????????????????????????????????????????

        void HandleVRInput()
        {
            // Get controllers via XR API
            UnityEngine.XR.InputDevice rightHand = default;
            UnityEngine.XR.InputDevice leftHand = default;

            var rightDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
                UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller,
                rightDevices);
            if (rightDevices.Count > 0) rightHand = rightDevices[0];

            var leftDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
                UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller,
                leftDevices);
            if (leftDevices.Count > 0) leftHand = leftDevices[0];

            // Right thumbstick Y = zoom
            Vector2 rStick;
            if (rightHand.isValid && rightHand.TryGetFeatureValue(
                    UnityEngine.XR.CommonUsages.primary2DAxis, out rStick))
            {
                if (Mathf.Abs(rStick.y) > 0.1f)
                {
                    _vrZoom += rStick.y * vrZoomSpeed * Time.deltaTime;
                    _vrZoom  = Mathf.Clamp(_vrZoom, 0.2f, 5f);
                    transform.localScale = Vector3.one * vrStartScale * _vrZoom;
                }
            }

            // Left thumbstick = move C60
            Vector2 lStick;
            if (leftHand.isValid && leftHand.TryGetFeatureValue(
                    UnityEngine.XR.CommonUsages.primary2DAxis, out lStick))
            {
                if (lStick.magnitude > 0.1f)
                {
                    transform.position += new Vector3(
                        lStick.x * vrMoveSpeed * Time.deltaTime,
                        0f,
                        lStick.y * vrMoveSpeed * Time.deltaTime);
                }
            }

            // A button = refine
            bool aPressed;
            if (rightHand.isValid && rightHand.TryGetFeatureValue(
                    UnityEngine.XR.CommonUsages.primaryButton, out aPressed))
            {
                if (aPressed && !_vrAWasPressed)
                {
                    _state = GK.RefineAll(_state);
                    _dirty = true;
                    LogState("VR_REFINE");
                }
                _vrAWasPressed = aPressed;
            }

            // B button = reset
            bool bPressed;
            if (rightHand.isValid && rightHand.TryGetFeatureValue(
                    UnityEngine.XR.CommonUsages.secondaryButton, out bPressed))
            {
                if (bPressed && !_vrBWasPressed)
                {
                    ResetState();
                    transform.position = vrStartPos;
                    _vrZoom = 1f;
                }
                _vrBWasPressed = bPressed;
            }

            // Right trigger = screenshot
            float triggerVal;
            if (rightHand.isValid && rightHand.TryGetFeatureValue(
                    UnityEngine.XR.CommonUsages.trigger, out triggerVal))
            {
                bool trigPressed = triggerVal > 0.8f;
                if (trigPressed && !_vrTriggerWasPressed)
                    TakeScreenshot();
                _vrTriggerWasPressed = trigPressed;
            }
        }

        // ?? public API for GKAutopilot ??????????????????????????????????????
        public int Re = 100; // Reynolds number (for NSFlow later)

        public void Seed()
        {
            _state = GK.BuildC60();
            // restart emergence
            transform.localScale = Vector3.zero;
            _emerging = true;
            _emergeT  = 0f;
            _dirty    = true;
            LogState("SEED");
        }

        public void ResetState()
        {
            _state = GK.BuildC60();
            transform.localScale   = Vector3.one;
            transform.localRotation = UnityEngine.Quaternion.identity;
            autoRotate = false;
            _dirty     = true;
            LogState("RESET");
        }

        public void SetZoom(float mult)
        {
            #if !UNITY_ANDROID
            var cam = Camera.main;
            if (cam == null) return;
            float baseZ = -9f;
            cam.transform.position = new Vector3(0f, 4f, baseZ / mult);
            #endif
            Debug.Log($"[GK] Zoom {mult}x");
        }

                // ?? public API for sequencer ??????????????????????????????????????????
        public void TriggerRefine()
        {
            _state = GK.RefineAll(_state);
            _dirty = true;
            LogState("REFINE");
        }


        // ?? auto screenshot (Quest accessible via Windows Explorer) ????
        // Android/data/com.vladmnet.machinenet/files/screenshot_YYYYMMDD_HHmmss.png
        public void TakeScreenshot()
        {
            string ts   = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = System.IO.Path.Combine(
                Application.persistentDataPath,
                $"screenshot_{ts}.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[GK] Screenshot: {path}");
        }

        // ?? logger ?????????????????????????????????????????????????????????

        void LogState(string tag)
        {
            if(_state==null)return;
            var inv=GK.Invariants(_state);
            int chi=inv.vertices-inv.edges+inv.faces;
            string ts=System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fname=$"PlayLog_{ts}_{tag}_F{inv.faces}_chi{chi}.txt";
            var sb=new System.Text.StringBuilder();
            sb.AppendLine("=== MachineNet GKRenderer Play Log ===");
            sb.AppendLine($"Tag       : {tag}");
            sb.AppendLine($"Time      : {System.DateTime.Now}");
            sb.AppendLine($"Faces     : {inv.faces}");
            sb.AppendLine($"Vertices  : {inv.vertices}");
            sb.AppendLine($"Edges     : {inv.edges}");
            sb.AppendLine($"Pentagons : {inv.pents}  (always 12 by Euler)");
            sb.AppendLine($"Hexagons  : {inv.hexes}");
            sb.AppendLine($"MaxLevel  : {inv.maxLevel}");
            sb.AppendLine($"Chi       : {chi}  (must be 2)");
            sb.AppendLine($"Anchors   : {inv.anchorCount}");
            sb.AppendLine("INVARIANTS:");
            sb.AppendLine($"  pents==12 : {inv.pents==12}");
            sb.AppendLine($"  chi==2    : {chi==2}");
            File.WriteAllText(Path.Combine(_logDir,fname),sb.ToString());
            Debug.Log($"[GK] LOG [{tag}] F={inv.faces} V={inv.vertices} "+
                      $"E={inv.edges} pents={inv.pents} chi={chi}");
            if(inv.pents!=12) Debug.LogError($"[GK] BROKEN: pents={inv.pents}");
            if(chi!=2)        Debug.LogError($"[GK] BROKEN: chi={chi}");
        }
    }
}
