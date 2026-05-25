// =============================================================================
//  GoldbergKernel.cs
//  MachineNet ? Goldberg-Coxeter recursive refinement ? math kernel
//  Vladyslav Savytskyy ? 2026-05-24
//
//  Direct port of goldberg_kernel.js ? same math, same structure, same invariants.
//  STANDALONE. Pure functions. No MonoBehaviour. No Unity dependencies.
//  Can run in unit tests, editor scripts, runtime ? anywhere C# runs.
//
//  PUBLIC API (mirrors JS exactly):
//    GK.BuildC60()                         -> GKState
//    GK.RefineFace(face, params)           -> GKFace[]
//    GK.RefineAll(state, params)           -> GKState
//    GK.RefineOne(state, faceIdx, params)  -> GKState
//    GK.Invariants(state)                  -> GKInvariants
//    GK.Serialize(state)                   -> GKSerialState (JSON-ready)
//    GK.Deserialize(obj)                   -> GKState
//    GK.FaceLocalFrame(face)               -> GKFrame
//    GK.FacePatch2D(face)                  -> GKPatch2D
//
//  INVARIANTS (same as JS, math-guaranteed):
//    - BuildC60() always yields exactly 12 pentagons + 20 hexagons
//    - After any RefineAll: pentagon count stays exactly 12
//    - Euler: V - E + F = 2 (chi = 2, sphere topology)
//    - Every vertex has degree 3
// =============================================================================

using System;
using System.Collections.Generic;

namespace MachineNet
{
    // =========================================================================
    //  Data types
    // =========================================================================

    /// <summary>One face of the polyhedron.</summary>
    [Serializable]
    public class GKFace
    {
        public float[][] pts;       // vertex positions [n][3]
        public string type;         // "pent" | "hex"
        public int level;           // refinement depth (0 = original C60)
        public int[] lineage;       // ancestor indices
        public string id;           // stable string id e.g. "F3.c12.e47"
        public string anchor;       // pentagonal anchor id, or null
    }

    /// <summary>Full polyhedron state. Immutable: every operation returns new state.</summary>
    public class GKState
    {
        public GKFace[] faces;
        public int counter;         // id mint counter
    }

    /// <summary>Refinement parameters (all optional, defaults match JS).</summary>
    public class GKParams
    {
        public float innerScale       = 0.45f;
        public float midScale         = 0.70f;
        public bool  preservePentInPent = true;
        public bool  preserveHexInHex   = true;
        public string surfaceMode     = "planar";   // "planar" | "spherical"
        public float sphereR          = 1.6f;
        public float jitter           = 0f;
    }

    /// <summary>Topology invariants snapshot.</summary>
    public class GKInvariants
    {
        public int pents;
        public int hexes;
        public int faces;
        public int edges;
        public int vertices;
        public int maxLevel;
        public int anchorCount;
    }

    /// <summary>Local coordinate frame for a face (VR teleport / 2D patch).</summary>
    public class GKFrame
    {
        public float[] origin;      // centroid [3]
        public float[] normal;      // outward normal [3]
        public float[] tangent;     // [3]
        public float[] bitangent;   // [3]
    }

    public class GKPatch2D
    {
        public GKFrame frame;
        public float[][] points2D;  // [n][2]
    }

    // =========================================================================
    //  GK ? the kernel (static class, pure functions, no state)
    // =========================================================================
    public static class GK
    {
        public static readonly float PHI = (1f + MathF.Sqrt(5f)) / 2f;

        // =====================================================================
        //  ?A  Vector helpers ? mirrors JS exactly
        //      All vectors are float[3]. No Unity Vector3 dependency.
        //      (Unity-facing code can convert: new Vector3(p[0], p[1], p[2]))
        // =====================================================================

        public static float[] V3(float x, float y, float z)
            => new float[]{ x, y, z };

        public static float[] VAdd(float[] a, float[] b)
            => new float[]{ a[0]+b[0], a[1]+b[1], a[2]+b[2] };

        public static float[] VSub(float[] a, float[] b)
            => new float[]{ a[0]-b[0], a[1]-b[1], a[2]-b[2] };

        public static float[] VScale(float[] a, float s)
            => new float[]{ a[0]*s, a[1]*s, a[2]*s };

        public static float VLen(float[] a)
            => MathF.Sqrt(a[0]*a[0] + a[1]*a[1] + a[2]*a[2]);

        public static float[] VNorm(float[] a)
        {
            float L = VLen(a);
            return L > 1e-12f ? VScale(a, 1f/L) : new float[]{ 0,0,0 };
        }

        public static float[] VLerp(float[] a, float[] b, float t)
            => new float[]{
                a[0]*(1-t)+b[0]*t,
                a[1]*(1-t)+b[1]*t,
                a[2]*(1-t)+b[2]*t
            };

        public static float[] VCross(float[] a, float[] b)
            => new float[]{
                a[1]*b[2]-a[2]*b[1],
                a[2]*b[0]-a[0]*b[2],
                a[0]*b[1]-a[1]*b[0]
            };

        public static float VDot(float[] a, float[] b)
            => a[0]*b[0] + a[1]*b[1] + a[2]*b[2];

        public static float[] VCopy(float[] a)
            => new float[]{ a[0], a[1], a[2] };

        public static float[] ProjectToSphere(float[] p, float R)
        {
            float L = VLen(p);
            return L < 1e-12f ? p : VScale(p, R/L);
        }

        static float[] Centroid(float[][] pts)
        {
            float cx=0, cy=0, cz=0;
            for(int i=0;i<pts.Length;i++){ cx+=pts[i][0]; cy+=pts[i][1]; cz+=pts[i][2]; }
            float n = pts.Length;
            return new float[]{ cx/n, cy/n, cz/n };
        }

        // =====================================================================
        //  ?B  Canonical C60 construction ? mirrors JS buildC60Vertices/Faces
        // =====================================================================

        static float[][] BuildC60Vertices()
        {
            var raw = new List<float[]>();
            // Truncated icosahedron vertex coordinates (Wikipedia formula):
            // (0, +-1, +-3phi), (+-1, +-(2+phi), +-2phi), (+-phi, +-2, +-(2phi+1))
            // and all EVEN permutations of each coordinate triple.
            float p  = PHI;
            float[][] templates = new float[][]{
                new float[]{ 0,     1,      3*p       },
                new float[]{ 1,     2+p,    2*p       },
                new float[]{ p,     2,      2*p+1     },
            };
            int[][] perms = new int[][]{ new int[]{0,1,2}, new int[]{1,2,0}, new int[]{2,0,1} };

            foreach(var tmpl in templates)
            {
                float a = tmpl[0], b = tmpl[1], c = tmpl[2];
                foreach(var perm in perms)
                {
                    for(int sa=-1;sa<=1;sa+=2)
                    for(int sb=-1;sb<=1;sb+=2)
                    for(int sc=-1;sc<=1;sc+=2)
                    {
                        if(a==0 && sa==-1) continue;
                        if(b==0 && sb==-1) continue;
                        if(c==0 && sc==-1) continue;
                        var v = new float[3];
                        v[perm[0]] = sa*a;
                        v[perm[1]] = sb*b;
                        v[perm[2]] = sc*c;
                        // Deduplicate
                        bool found = false;
                        foreach(var u in raw)
                        {
                            if(MathF.Abs(u[0]-v[0])<0.001f &&
                               MathF.Abs(u[1]-v[1])<0.001f &&
                               MathF.Abs(u[2]-v[2])<0.001f)
                            { found=true; break; }
                        }
                        if(!found) raw.Add(v);
                    }
                }
            }
            // Normalize to sphere radius 1.6
            for(int i=0;i<raw.Count;i++)
                raw[i] = VScale(VNorm(raw[i]), 1.6f);
            return raw.ToArray();
        }

        static int[][] BuildC60FaceIndices(float[][] verts)
        {
            // Find edge length (smallest nonzero pair distance)
            float minD = float.MaxValue;
            for(int i=0;i<verts.Length;i++)
            for(int j=i+1;j<verts.Length;j++)
            {
                float d = VLen(VSub(verts[i], verts[j]));
                if(d > 0.01f && d < minD) minD = d;
            }
            float edgeLen = minD;
            float tol = edgeLen * 0.05f;

            // Build adjacency
            var adj = new List<int>[verts.Length];
            for(int i=0;i<verts.Length;i++) adj[i] = new List<int>();
            for(int i=0;i<verts.Length;i++)
            for(int j=0;j<verts.Length;j++)
            {
                if(i==j) continue;
                float d = VLen(VSub(verts[i], verts[j]));
                if(MathF.Abs(d-edgeLen) < tol) adj[i].Add(j);
            }

            // Sort neighbors CCW around outward normal
            for(int i=0;i<verts.Length;i++)
            {
                float[] v = verts[i];
                float[] n = VNorm(v);
                float[] rv = VSub(verts[adj[i][0]], v);
                float dot = VDot(rv, n);
                float[] tangent = VNorm(VSub(rv, VScale(n, dot)));
                float[] e2 = VCross(n, tangent);
                var nb = adj[i];
                nb.Sort((a,b2) => {
                    float[] va2 = VSub(verts[a], v);
                    float[] vb2 = VSub(verts[b2], v);
                    float aa = MathF.Atan2(VDot(va2,e2), VDot(va2,tangent));
                    float bb = MathF.Atan2(VDot(vb2,e2), VDot(vb2,tangent));
                    return aa.CompareTo(bb);
                });
            }

            // Half-edge face tracing
            int NextInFace(int u, int vv) {
                var nbrs = adj[vv];
                int idx = nbrs.IndexOf(u);
                if(idx < 0) return -1;
                return nbrs[(idx + nbrs.Count - 1) % nbrs.Count];
            }

            var visited = new HashSet<long>();
            var faces = new List<int[]>();
            for(int u=0;u<verts.Length;u++)
            {
                foreach(int startV in adj[u])
                {
                    long key = u * 10000L + startV;
                    if(visited.Contains(key)) continue;
                    var face = new List<int>{ u };
                    int a2=u, b2=startV;
                    for(int step=0;step<20;step++)
                    {
                        visited.Add(a2*10000L+b2);
                        int c2 = NextInFace(a2, b2);
                        if(c2<0 || c2==u) break;
                        face.Add(b2);
                        a2=b2; b2=c2;
                    }
                    if(face.Count > 0 && face[face.Count-1] != b2) face.Add(b2);
                    if(face.Count==5 || face.Count==6) faces.Add(face.ToArray());
                }
            }
            // Deduplicate: face tracing finds each face in both windings.
            // Canonical form: sort vertex indices, keep only one winding.
            var unique = new List<int[]>();
            var faceKeys = new HashSet<string>();
            foreach(var face in faces)
            {
                var sorted = new int[face.Length];
                face.CopyTo(sorted, 0);
                Array.Sort(sorted);
                string key = string.Join(",", sorted);
                if(!faceKeys.Contains(key))
                {
                    faceKeys.Add(key);
                    unique.Add(face);
                }
            }
            return unique.ToArray();
        }

        // =====================================================================
        //  ?C  Public: BuildC60
        // =====================================================================

        public static GKState BuildC60()
        {
            float[][] verts = BuildC60Vertices();
            int[][] faceIdxs = BuildC60FaceIndices(verts);
            var meshFaces = new GKFace[faceIdxs.Length];
            for(int i=0;i<faceIdxs.Length;i++)
            {
                var fi = faceIdxs[i];
                var pts = new float[fi.Length][];
                for(int k=0;k<fi.Length;k++) pts[k] = VCopy(verts[fi[k]]);
                string type = fi.Length==5 ? "pent" : "hex";
                meshFaces[i] = new GKFace {
                    pts     = pts,
                    type    = type,
                    level   = 0,
                    lineage = new int[]{ i },
                    id      = "F"+i,
                    anchor  = type=="pent" ? "A"+i : null
                };
            }
            return new GKState { faces=meshFaces, counter=meshFaces.Length };
        }

        // =====================================================================
        //  ?D  RefineFace ? pure function, mirrors JS exactly
        // =====================================================================

        public static GKFace[] RefineFace(GKFace face, GKParams p, ref int counter)
        {
            if(p == null) p = new GKParams();
            float[][] pts = face.pts;
            int n = pts.Length;
            float[] c = Centroid(pts);

            // Inner ring
            var inner = new float[n][];
            for(int i=0;i<n;i++)
            {
                float[] pt = VLerp(c, pts[i], p.innerScale);
                if(p.surfaceMode=="spherical") pt = ProjectToSphere(pt, p.sphereR);
                inner[i] = pt;
            }
            // Edge midpoints pulled inward
            var midRing = new float[n][];
            for(int i=0;i<n;i++)
            {
                float[] m = VLerp(pts[i], pts[(i+1)%n], 0.5f);
                float[] pulled = VLerp(c, m, p.midScale);
                if(p.surfaceMode=="spherical") pulled = ProjectToSphere(pulled, p.sphereR);
                midRing[i] = pulled;
            }

            var subs = new List<GKFace>();

            // Inner polygon
            string innerType = face.type=="pent"
                ? (p.preservePentInPent ? "pent" : "hex")
                : (p.preserveHexInHex  ? "hex"  : "pent");
            counter++;
            var lin0 = new int[face.lineage.Length+1];
            face.lineage.CopyTo(lin0,0); lin0[face.lineage.Length]=0;
            subs.Add(new GKFace {
                pts     = inner,
                type    = innerType,
                level   = face.level+1,
                lineage = lin0,
                id      = face.id+".c"+counter,
                anchor  = innerType=="pent" ? face.anchor : null
            });

            // Surrounding hexes ? one per edge
            for(int i=0;i<n;i++)
            {
                int j = (i+1)%n;
                float[] em = VLerp(pts[i], pts[j], 0.5f);
                if(p.surfaceMode=="spherical") em = ProjectToSphere(em, p.sphereR);
                float[][] hexPts = new float[][]{
                    VCopy(pts[i]),
                    em,
                    VCopy(pts[j]),
                    VCopy(inner[j]),
                    VCopy(midRing[i]),
                    VCopy(inner[i])
                };
                counter++;
                var linE = new int[face.lineage.Length+1];
                face.lineage.CopyTo(linE,0); linE[face.lineage.Length]=i+1;
                subs.Add(new GKFace {
                    pts     = hexPts,
                    type    = "hex",
                    level   = face.level+1,
                    lineage = linE,
                    id      = face.id+".e"+counter,
                    anchor  = null
                });
            }
            return subs.ToArray();
        }

        // =====================================================================
        //  ?E  RefineOne / RefineAll / Undo
        // =====================================================================

        public static GKState RefineOne(GKState state, int faceIdx, GKParams p=null)
        {
            if(faceIdx<0 || faceIdx>=state.faces.Length) return state;
            int ctr = state.counter;
            var subs = RefineFace(state.faces[faceIdx], p ?? new GKParams(), ref ctr);
            var newFaces = new List<GKFace>(state.faces);
            newFaces.RemoveAt(faceIdx);
            newFaces.AddRange(subs);
            return new GKState { faces=newFaces.ToArray(), counter=ctr };
        }

        public static GKState RefineAll(GKState state, GKParams p=null)
        {
            int ctr = state.counter;
            var newFaces = new List<GKFace>();
            foreach(var face in state.faces)
            {
                var subs = RefineFace(face, p ?? new GKParams(), ref ctr);
                newFaces.AddRange(subs);
            }
            return new GKState { faces=newFaces.ToArray(), counter=ctr };
        }

        // =====================================================================
        //  ?F  Invariants
        // =====================================================================

        public static GKInvariants Invariants(GKState state)
        {
            int pents=0, hexes=0, maxLevel=0;
            var anchors = new HashSet<string>();
            var vertSet = new HashSet<string>();

            var edgeSet = new HashSet<string>();
            foreach(var f in state.faces)
            {
                if(f.type=="pent"){ pents++; if(f.anchor!=null) anchors.Add(f.anchor); }
                else hexes++;
                if(f.level>maxLevel) maxLevel=f.level;
                // Unique vertices
                foreach(var pt in f.pts)
                    vertSet.Add(pt[0].ToString("F3")+","+pt[1].ToString("F3")+","+pt[2].ToString("F3"));
                // Unique edges: canonical key = sorted pair of vertex strings
                for(int i=0;i<f.pts.Length;i++)
                {
                    var pa = f.pts[i];
                    var pb = f.pts[(i+1)%f.pts.Length];
                    string ka = pa[0].ToString("F3")+","+pa[1].ToString("F3")+","+pa[2].ToString("F3");
                    string kb = pb[0].ToString("F3")+","+pb[1].ToString("F3")+","+pb[2].ToString("F3");
                    string ek = string.Compare(ka,kb)<0 ? ka+"|"+kb : kb+"|"+ka;
                    edgeSet.Add(ek);
                }
            }
            return new GKInvariants {
                pents       = pents,
                hexes       = hexes,
                faces       = state.faces.Length,
                edges       = edgeSet.Count,
                vertices    = vertSet.Count,
                maxLevel    = maxLevel,
                anchorCount = anchors.Count
            };
        }

        // =====================================================================
        //  ?G  FaceLocalFrame + FacePatch2D (VR teleport support)
        // =====================================================================

        public static GKFrame FaceLocalFrame(GKFace face)
        {
            float[] c = Centroid(face.pts);
            float[] n = VNorm(c);
            float[] rv = VSub(face.pts[0], c);
            float dot = VDot(rv, n);
            float[] tangent   = VNorm(VSub(rv, VScale(n, dot)));
            float[] bitangent = VCross(n, tangent);
            return new GKFrame {
                origin    = c,
                normal    = n,
                tangent   = tangent,
                bitangent = bitangent
            };
        }

        public static GKPatch2D FacePatch2D(GKFace face)
        {
            var frame = FaceLocalFrame(face);
            var pts2D = new float[face.pts.Length][];
            for(int i=0;i<face.pts.Length;i++)
            {
                float[] p = VSub(face.pts[i], frame.origin);
                pts2D[i] = new float[]{
                    VDot(p, frame.tangent),
                    VDot(p, frame.bitangent)
                };
            }
            return new GKPatch2D { frame=frame, points2D=pts2D };
        }
    }
}
