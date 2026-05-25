// =============================================================================
//  GKTests.cs ? GoldbergKernel invariant tests
//  Run in Unity: Window > General > Test Runner > EditMode
//  ALL tests must pass before any VR code touches the kernel.
//  Same invariants as goldberg_kernel.js ? math does not change between languages.
// =============================================================================

using NUnit.Framework;
using MachineNet;

namespace MachineNet.Tests
{
    public class GKTests
    {
        // =====================================================================
        //  THE INVARIANTS (mirrors JS GK.invariants checks exactly)
        // =====================================================================

        [Test]
        public void BuildC60_ExactlyTwelvePentagons()
        {
            var state = GK.BuildC60();
            var inv = GK.Invariants(state);
            Assert.AreEqual(12, inv.pents,
                "C60 must have exactly 12 pentagons. Euler says so.");
        }

        [Test]
        public void BuildC60_ExactlyTwentyHexagons()
        {
            var state = GK.BuildC60();
            var inv = GK.Invariants(state);
            Assert.AreEqual(20, inv.hexes,
                "C60 base has 20 hexagons.");
        }

        [Test]
        public void BuildC60_SixtyVertices()
        {
            var state = GK.BuildC60();
            var inv = GK.Invariants(state);
            Assert.AreEqual(60, inv.vertices,
                "C60: V = 60.");
        }

        [Test]
        public void BuildC60_NinetyEdges()
        {
            var state = GK.BuildC60();
            var inv = GK.Invariants(state);
            Assert.AreEqual(90, inv.edges,
                "C60: E = 90.");
        }

        [Test]
        public void BuildC60_ThirtyTwoFaces()
        {
            var state = GK.BuildC60();
            var inv = GK.Invariants(state);
            Assert.AreEqual(32, inv.faces,
                "C60: F = 32.");
        }

        [Test]
        public void BuildC60_EulerCharacteristicIsTwo()
        {
            var state = GK.BuildC60();
            var inv = GK.Invariants(state);
            int chi = inv.vertices - inv.edges + inv.faces;
            Assert.AreEqual(2, chi,
                "Euler: V - E + F must equal 2 (sphere topology, chi=2).");
        }

        [Test]
        public void BuildC60_TwelveAnchorsDefined()
        {
            var state = GK.BuildC60();
            var inv = GK.Invariants(state);
            Assert.AreEqual(12, inv.anchorCount,
                "Each pentagon gets its own anchor. 12 pentagons = 12 anchors.");
        }

        [Test]
        public void BuildC60_AllFacesAtLevelZero()
        {
            var state = GK.BuildC60();
            var inv = GK.Invariants(state);
            Assert.AreEqual(0, inv.maxLevel,
                "Base C60: all faces at level 0.");
        }

        // =====================================================================
        //  REFINEMENT INVARIANTS
        // =====================================================================

        [Test]
        public void RefineAll_PentagonCountStaysTwelve()
        {
            var state = GK.BuildC60();
            state = GK.RefineAll(state);
            var inv = GK.Invariants(state);
            Assert.AreEqual(12, inv.pents,
                "After RefineAll: pentagon count must stay exactly 12. Always.");
        }

        [Test]
        public void RefineAll_LevelIncreases()
        {
            var state = GK.BuildC60();
            state = GK.RefineAll(state);
            var inv = GK.Invariants(state);
            Assert.AreEqual(1, inv.maxLevel,
                "After one RefineAll: maxLevel = 1.");
        }

        [Test]
        public void RefineAll_TwicePentagonCountStillTwelve()
        {
            var state = GK.BuildC60();
            state = GK.RefineAll(state);
            state = GK.RefineAll(state);
            var inv = GK.Invariants(state);
            Assert.AreEqual(12, inv.pents,
                "After two RefineAll passes: still 12 pentagons. Forever.");
        }

        [Test]
        public void RefineOne_PentagonCountPreserved()
        {
            var state = GK.BuildC60();
            // Find first pentagon
            int pentIdx = -1;
            for(int i=0;i<state.faces.Length;i++)
                if(state.faces[i].type=="pent"){ pentIdx=i; break; }
            state = GK.RefineOne(state, pentIdx);
            var inv = GK.Invariants(state);
            Assert.AreEqual(12, inv.pents,
                "RefineOne on a pent: still 12 pentagons (inner pent replaces parent).");
        }

        // =====================================================================
        //  VECTOR MATH
        // =====================================================================

        [Test]
        public void VNorm_ReturnsUnitVector()
        {
            float[] v = GK.V3(3f, 4f, 0f);
            float[] n = GK.VNorm(v);
            float len = GK.VLen(n);
            Assert.AreEqual(1f, len, 0.0001f,
                "VNorm must return a unit vector.");
        }

        [Test]
        public void VLerp_AtZeroReturnsA()
        {
            float[] a = GK.V3(1f, 2f, 3f);
            float[] b = GK.V3(4f, 5f, 6f);
            float[] r = GK.VLerp(a, b, 0f);
            Assert.AreEqual(a[0], r[0], 0.0001f);
            Assert.AreEqual(a[1], r[1], 0.0001f);
            Assert.AreEqual(a[2], r[2], 0.0001f);
        }

        [Test]
        public void VLerp_AtOneReturnsB()
        {
            float[] a = GK.V3(1f, 2f, 3f);
            float[] b = GK.V3(4f, 5f, 6f);
            float[] r = GK.VLerp(a, b, 1f);
            Assert.AreEqual(b[0], r[0], 0.0001f);
            Assert.AreEqual(b[1], r[1], 0.0001f);
            Assert.AreEqual(b[2], r[2], 0.0001f);
        }

        [Test]
        public void VCross_Perpendicular()
        {
            float[] x = GK.V3(1f, 0f, 0f);
            float[] y = GK.V3(0f, 1f, 0f);
            float[] z = GK.VCross(x, y);
            Assert.AreEqual(0f, z[0], 0.0001f);
            Assert.AreEqual(0f, z[1], 0.0001f);
            Assert.AreEqual(1f, z[2], 0.0001f);
        }

        // =====================================================================
        //  FACE LOCAL FRAME
        // =====================================================================

        [Test]
        public void FaceLocalFrame_NormalIsUnitVector()
        {
            var state = GK.BuildC60();
            var frame = GK.FaceLocalFrame(state.faces[0]);
            float len = GK.VLen(frame.normal);
            Assert.AreEqual(1f, len, 0.001f,
                "Face normal must be unit length.");
        }

        [Test]
        public void FaceLocalFrame_TangentOrthogonalToNormal()
        {
            var state = GK.BuildC60();
            var frame = GK.FaceLocalFrame(state.faces[0]);
            float dot = GK.VDot(frame.normal, frame.tangent);
            Assert.AreEqual(0f, dot, 0.001f,
                "Tangent must be orthogonal to normal.");
        }
    }
}
