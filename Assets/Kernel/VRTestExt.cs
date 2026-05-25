using UnityEngine;

namespace MachineNet
{
    public class VRTestExt : MonoBehaviour
    {
        // Attach to SECOND GameObject. Tests custom mesh (same method as C60)
        void Start()
        {
            try
            {
                // Build a simple triangle mesh by hand - same pattern as BuildFaceMesh
                var go = new GameObject("CustomTriangle");
                go.transform.position = new Vector3(0f, 1.5f, -1.5f);

                var mf = go.AddComponent<MeshFilter>();
                var mr = go.AddComponent<MeshRenderer>();

                var mesh = new Mesh();
                mesh.vertices = new Vector3[] {
                    new Vector3(0f, 0.5f, 0f),
                    new Vector3(-0.5f, -0.5f, 0f),
                    new Vector3(0.5f, -0.5f, 0f)
                };
                // Double sided like C60
                mesh.triangles = new int[] { 0, 1, 2, 0, 2, 1 };
                mesh.RecalculateNormals();
                mf.sharedMesh = mesh;

                var sh = Shader.Find("Universal Render Pipeline/Unlit");
                if (sh == null) sh = Shader.Find("Unlit/Color");
                if (sh == null) sh = Shader.Find("Standard");
                var mat = new Material(sh);
                mat.color = Color.red;
                mr.sharedMaterial = mat;

                // Also build a custom quad (like C60 edges)
                var go2 = new GameObject("CustomQuad");
                go2.transform.position = new Vector3(0.5f, 1.5f, -1.5f);
                var mf2 = go2.AddComponent<MeshFilter>();
                var mr2 = go2.AddComponent<MeshRenderer>();

                var mesh2 = new Mesh();
                mesh2.vertices = new Vector3[] {
                    new Vector3(-0.3f, -0.3f, 0f),
                    new Vector3( 0.3f, -0.3f, 0f),
                    new Vector3( 0.3f,  0.3f, 0f),
                    new Vector3(-0.3f,  0.3f, 0f)
                };
                mesh2.triangles = new int[] { 0, 2, 1, 0, 3, 2, 0, 1, 2, 0, 2, 3 };
                mesh2.RecalculateNormals();
                mf2.sharedMesh = mesh2;

                var mat2 = new Material(sh);
                mat2.color = new Color(0f, 1f, 0.5f);
                mr2.sharedMaterial = mat2;

                Debug.Log("[VRTestExt] Custom triangle + quad spawned at (0, 1.5, -1.5)");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[VRTestExt] CRASH: " + e);
            }
        }
    }
}
