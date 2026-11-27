using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIResetDiagram : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = rectTransform.rect;
            float w = rect.width;
            float h = rect.height;
            Color rail = new Color(0.55f, 0.61f, 0.68f, 1f);
            Color arm = new Color(0.08f, 0.34f, 0.94f, 1f);
            Color sling = new Color(0.36f, 0.75f, 0.58f, 1f);
            Color motion = new Color(0.96f, 0.62f, 0.16f, 1f);

            Vector2 a = P(0.34f, 0.10f, w, h);
            Vector2 b = P(0.34f, 0.90f, w, h);
            DrawLine(vh, a, b, 10f, rail);
            DrawLine(vh, P(0.25f, 0.10f, w, h), P(0.43f, 0.10f, w, h), 10f, rail);
            DrawLine(vh, P(0.34f, 0.88f, w, h), P(0.62f, 0.88f, w, h), 10f, rail);
            DrawLine(vh, P(0.58f, 0.88f, w, h), P(0.58f, 0.64f, w, h), 4f, rail);

            Vector2 shoulder = P(0.58f, 0.64f, w, h);
            Vector2 elbow = P(0.70f, 0.43f, w, h);
            Vector2 hand = P(0.84f, 0.50f, w, h);
            DrawLine(vh, shoulder, elbow, 22f, arm);
            DrawLine(vh, elbow, hand, 18f, arm);
            DrawCircle(vh, shoulder, 13f, arm);
            DrawCircle(vh, elbow, 11f, arm);
            DrawCircle(vh, hand, 9f, arm);
            DrawLine(vh, P(0.50f, 0.64f, w, h), shoulder, 5f, sling);
            DrawLine(vh, shoulder, P(0.63f, 0.55f, w, h), 5f, sling);
            DrawLine(vh, P(0.63f, 0.55f, w, h), elbow, 5f, sling);

            Vector2 home = P(0.46f, 0.34f, w, h);
            DrawLine(vh, hand, home, 5f, motion);
            DrawLine(vh, home, P(0.50f, 0.31f, w, h), 5f, motion);
            DrawLine(vh, home, P(0.41f, 0.31f, w, h), 5f, motion);
        }

        private static Vector2 P(float x, float y, float w, float h)
        {
            return new Vector2((x - 0.5f) * w, (y - 0.5f) * h);
        }

        private static void DrawLine(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 direction = (b - a).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * width * 0.5f;
            Vector2 p0 = a + normal;
            Vector2 p1 = b + normal;
            Vector2 p2 = b - normal;
            Vector2 p3 = a - normal;
            int index = vh.currentVertCount;
            vh.AddVert(new UIVertex { position = p0, color = color });
            vh.AddVert(new UIVertex { position = p1, color = color });
            vh.AddVert(new UIVertex { position = p2, color = color });
            vh.AddVert(new UIVertex { position = p3, color = color });
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private static void DrawCircle(VertexHelper vh, Vector2 center, float radius, Color color)
        {
            const int segments = 24;
            int index = vh.currentVertCount;
            vh.AddVert(new UIVertex { position = center, color = color });
            for (int i = 0; i <= segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vh.AddVert(new UIVertex { position = point, color = color });
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(index, index + i + 1, index + i + 2);
        }
    }
}
