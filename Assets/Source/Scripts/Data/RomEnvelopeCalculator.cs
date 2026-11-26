using System;
using System.Collections.Generic;
using UnityEngine;

namespace LB
{
    public static class RomEnvelopeCalculator
    {
        public static List<int[]> BuildConvexHull(IList<Vector3> points)
        {
            if (points == null || points.Count < 4) return null;
            int a = 0;
            int b = 0;
            float maxDistance = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                float distance = (points[i] - points[a]).sqrMagnitude;
                if (distance <= maxDistance) continue;
                maxDistance = distance;
                b = i;
            }
            if (maxDistance < 0.000001f) return null;

            Vector3 line = points[b] - points[a];
            int c = -1;
            maxDistance = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                if (i == a || i == b) continue;
                float distance = DistanceToLineSquared(points[i], points[a], line);
                if (distance <= maxDistance) continue;
                maxDistance = distance;
                c = i;
            }
            if (c < 0 || maxDistance < 0.000001f) return null;

            Vector3 normal = Vector3.Cross(line, points[c] - points[a]).normalized;
            int d = -1;
            maxDistance = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                if (i == a || i == b || i == c) continue;
                float distance = Mathf.Abs(Vector3.Dot(normal, points[i] - points[a]));
                if (distance <= maxDistance) continue;
                maxDistance = distance;
                d = i;
            }
            if (d < 0 || maxDistance < 0.000001f) return null;

            Vector3 interior = Vector3.zero;
            foreach (Vector3 point in points) interior += point;
            interior /= points.Count;

            List<HullFace> faces = new List<HullFace>
            {
                CreateFace(a, b, c, interior, points),
                CreateFace(a, d, b, interior, points),
                CreateFace(b, d, c, interior, points),
                CreateFace(c, d, a, interior, points)
            };

            for (int i = 0; i < points.Count; i++)
            {
                if (i == a || i == b || i == c || i == d) continue;
                List<HullFace> visible = new List<HullFace>();
                foreach (HullFace face in faces)
                {
                    if (Vector3.Dot(face.Normal, points[i] - points[face.A]) > 0.00001f) visible.Add(face);
                }
                if (visible.Count == 0) continue;

                Dictionary<long, int> edgeCounts = new Dictionary<long, int>();
                foreach (HullFace face in visible)
                {
                    CountEdge(edgeCounts, face.A, face.B);
                    CountEdge(edgeCounts, face.B, face.C);
                    CountEdge(edgeCounts, face.C, face.A);
                }
                faces.RemoveAll(face => visible.Contains(face));
                foreach (KeyValuePair<long, int> edge in edgeCounts)
                {
                    if (edge.Value != 1) continue;
                    int first = (int)(edge.Key / int.MaxValue);
                    int second = (int)(edge.Key % int.MaxValue);
                    faces.Add(CreateFace(first, second, i, interior, points));
                }
            }

            List<int[]> triangles = new List<int[]>(faces.Count);
            foreach (HullFace face in faces)
            {
                if (Vector3.Cross(points[face.B] - points[face.A], points[face.C] - points[face.A]).sqrMagnitude < 0.000001f) continue;
                triangles.Add(new[] { face.A, face.B, face.C });
            }
            return triangles.Count > 0 ? triangles : null;
        }

        private static HullFace CreateFace(int a, int b, int c, Vector3 interior, IList<Vector3> points)
        {
            Vector3 normal = Vector3.Cross(points[b] - points[a], points[c] - points[a]).normalized;
            if (Vector3.Dot(normal, interior - points[a]) > 0f)
            {
                int swap = b;
                b = c;
                c = swap;
                normal = -normal;
            }
            return new HullFace(a, b, c, normal);
        }

        private static void CountEdge(Dictionary<long, int> counts, int a, int b)
        {
            int min = Mathf.Min(a, b);
            int max = Mathf.Max(a, b);
            long key = (long)min * int.MaxValue + max;
            int count;
            counts.TryGetValue(key, out count);
            counts[key] = count + 1;
        }

        private static float DistanceToLineSquared(Vector3 point, Vector3 start, Vector3 direction)
        {
            float lengthSquared = direction.sqrMagnitude;
            if (lengthSquared < 0.000001f) return (point - start).sqrMagnitude;
            float t = Mathf.Clamp01(Vector3.Dot(point - start, direction) / lengthSquared);
            return (point - (start + direction * t)).sqrMagnitude;
        }

        private struct HullFace
        {
            public readonly int A;
            public readonly int B;
            public readonly int C;
            public readonly Vector3 Normal;

            public HullFace(int a, int b, int c, Vector3 normal)
            {
                A = a;
                B = b;
                C = c;
                Normal = normal;
            }
        }
    }
}
