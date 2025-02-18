using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StormlightMod {
    public static class GlowCircleRenderer {
        private static Dictionary<Color, Material> cachedMaterials = new Dictionary<Color, Material>();

        public static void DrawCustomCircle(Pawn caster, float radius, Color color) {
            color.a = 0.15f;
            Vector3 center = caster.DrawPos;
            center.y = AltitudeLayer.MetaOverlays.AltitudeFor();

            Material mat = SolidColorMaterials.SimpleSolidColorMaterial(color);

            // Build a matrix for position/rotation/scale
            Matrix4x4 matrix = default;
            matrix.SetTRS(
                pos: center,
                q: Quaternion.identity,
                s: new Vector3(radius, 1f, radius)
            );

            Mesh circleMesh = MeshPool.circle;
            Graphics.DrawMesh(circleMesh, matrix, mat, -1);

        }
    }
}