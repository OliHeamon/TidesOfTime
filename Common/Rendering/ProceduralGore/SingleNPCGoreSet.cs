using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;

namespace TidesOfTime.Common.Rendering.ProceduralGore
{
    public struct SingleNPCGoreSet
    {
        public Vector4[] DivisionTextureCoordinates { get; private set; }

        public Vector2[] SegmentPositions { get; private set; }

        public Vector2[] SegmentVelocities { get; private set; }

        public Vector2[] SegmentRotations { get; private set; }

        public SingleNPCGoreSet(Vector2 origin, int width, int height, DivisionType type)
        {
            switch (type)
            {
                case DivisionType.RandomQuads:
                    RandomQuads(origin, width, height);
                    break;
            }
        }

        private void RandomQuads(Vector2 origin, int width, int height)
        {
            List<Vector4> subdivisions = new()
            {
                new(0, 0, 1, 1)
            };

            int depth = Main.rand.Next(1, 3);

            // Larger sprites need more divisions.
            if (width > 60 || height > 60)
            {
                depth++;
            }

            if (width > 200 || height > 200)
            {
                depth += 2;
            }

            if (width > 400 || height > 400)
            {
                depth += 2;
            }

            // Percentage deviation of a cut from the middle.
            float varianceAmount = 0.2f;

            for (int i = 0; i < depth; i++)
            {
                List<Vector4> newDivisions = new();

                for (int j = 0; j < subdivisions.Count; j++)
                {
                    Vector4 subdivision = subdivisions[j];

                    float subdivisionWidth = subdivision.Z - subdivision.X;
                    float subdivisionHeight = subdivision.W - subdivision.Y;

                    bool vertical = depth == 0 ? Main.rand.NextBool() : subdivisionWidth > subdivisionHeight;

                    if (vertical)
                    {
                        float midX = (subdivision.X + subdivision.Z) / 2;

                        // Variance on the cutting point from the middle.
                        float variance = Main.rand.NextFloat(-varianceAmount, varianceAmount) * subdivisionWidth;

                        float divisionX = MathHelper.Clamp(midX + variance, subdivision.X, subdivision.Z);

                        newDivisions.Add(new Vector4(subdivision.X, subdivision.Y, divisionX, subdivision.W));
                        newDivisions.Add(new Vector4(divisionX, subdivision.Y, subdivision.Z, subdivision.W));
                    }
                    else
                    {
                        float midY = (subdivision.Y + subdivision.W) / 2;

                        // Variance on the cutting point from the middle.
                        float variance = Main.rand.NextFloat(-varianceAmount, varianceAmount) * subdivisionHeight;

                        float divisionY = MathHelper.Clamp(midY + variance, subdivision.Y, subdivision.W);

                        newDivisions.Add(new Vector4(subdivision.X, subdivision.Y, subdivision.Z, divisionY));
                        newDivisions.Add(new Vector4(subdivision.X, divisionY, subdivision.Z, subdivision.W));
                    }
                }

                subdivisions.Clear();
                subdivisions.AddRange(newDivisions);
            }

            DivisionTextureCoordinates = subdivisions.ToArray();

            SegmentPositions = new Vector2[DivisionTextureCoordinates.Length];
            SegmentVelocities = new Vector2[DivisionTextureCoordinates.Length];
            SegmentRotations = new Vector2[DivisionTextureCoordinates.Length];

            for (int i = 0; i < DivisionTextureCoordinates.Length; i++)
            {
                // Use for exploded view of gores.
                Vector2 offset = new Vector2(DivisionTextureCoordinates[i].X, DivisionTextureCoordinates[i].Y) * 16;

                Vector2 subCoordinates = new(DivisionTextureCoordinates[i].X * width, DivisionTextureCoordinates[i].Y * height);

                SegmentPositions[i] = origin + subCoordinates + offset;

                Vector2 toPoint = (subCoordinates - new Vector2(width / 2, height / 2)).SafeNormalize(Vector2.Zero);

                SegmentVelocities[i] = toPoint * Main.rand.NextFloat(1f, 2f);
                SegmentRotations[i] = new Vector2(0, Main.rand.NextFloat(-0.0075f, 0.0075f));
            }
        }
    }
}
