using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    [RequireComponent(typeof(CanvasRenderer))]
    [DisallowMultipleComponent]
    public class ReportChartGraphic : MaskableGraphic
    {
        public enum ChartKind
        {
            StrengthRadar,
            BaselineLine
        }

        [SerializeField] private ChartKind chartKind;

        private float[][] strengthSeries;
        private float[][] baselineSeries;

        private static readonly Color GridColor = new Color(0.62f, 0.62f, 0.62f, 0.72f);
        private static readonly Color AxisColor = new Color(0.28f, 0.28f, 0.28f, 0.92f);
        private static readonly Color BlueColor = new Color(0.12f, 0.39f, 0.71f, 1f);
        private static readonly Color OrangeColor = new Color(0.88f, 0.58f, 0.16f, 1f);
        private static readonly Color GreenColor = new Color(0.28f, 0.62f, 0.34f, 1f);
        private static readonly Color RedColor = new Color(0.78f, 0.27f, 0.34f, 1f);

        public ChartKind Kind
        {
            get { return chartKind; }
            set
            {
                if (chartKind == value) return;
                chartKind = value;
                SetVerticesDirty();
            }
        }

        public void SetStrengthData(float[][] series)
        {
            strengthSeries = series;
            SetVerticesDirty();
        }

        public void SetBaselineData(float[] participation, float[] trackingError, float[] deviation)
        {
            baselineSeries = participation != null && trackingError != null && deviation != null &&
                             participation.Length == trackingError.Length && participation.Length == deviation.Length
                ? new[] { participation, trackingError, deviation }
                : null;
            SetVerticesDirty();
        }

        public void ClearData()
        {
            strengthSeries = null;
            baselineSeries = null;
            SetVerticesDirty();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = rectTransform.rect;
            if (rect.width <= 1f || rect.height <= 1f) return;

            switch (chartKind)
            {
                case ChartKind.StrengthRadar:
                    DrawStrengthRadar(vh, rect);
                    break;
                case ChartKind.BaselineLine:
                    DrawBaselineLine(vh, rect);
                    break;
            }
        }

        private void DrawStrengthRadar(VertexHelper vh, Rect rect)
        {
            Vector2 center = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.37f;
            Vector2[] axes = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = -Mathf.PI / 2f + i * Mathf.PI / 3f;
                axes[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            for (int ring = 1; ring <= 5; ring++)
            {
                List<Vector2> points = new List<Vector2>();
                float ringRadius = radius * ring / 5f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = -Mathf.PI / 2f + i * Mathf.PI / 3f;
                    points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ringRadius);
                }
                DrawPolyline(vh, rect, points, true, GridColor, 1f);
            }
            foreach (Vector2 axis in axes) DrawLine(vh, rect, center, axis, GridColor, 1f);

            if (strengthSeries == null || strengthSeries.Length == 0) return;
            float maxValue = 0.0001f;
            foreach (float[] series in strengthSeries)
            {
                if (series == null) continue;
                foreach (float value in series) maxValue = Mathf.Max(maxValue, value);
            }

            Color[] colors = { BlueColor, OrangeColor, GreenColor, RedColor };
            int seriesCount = Mathf.Min(strengthSeries.Length, colors.Length);
            for (int s = 0; s < seriesCount; s++)
            {
                float[] values = strengthSeries[s];
                if (values == null || values.Length < 6) continue;
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < 6; i++)
                {
                    float angle = -Mathf.PI / 2f + i * Mathf.PI / 3f;
                    float normalized = Mathf.Clamp01(values[i] / maxValue);
                    points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * normalized);
                }
                DrawPolygon(vh, rect, points, colors[s], 0.07f);
                DrawPolyline(vh, rect, points, true, colors[s], 1.9f);
                foreach (Vector2 point in points) DrawCircle(vh, rect, point, 2.2f, colors[s]);
            }
        }

        private void DrawBaselineLine(VertexHelper vh, Rect rect)
        {
            float left = 48f;
            float right = rect.width - 48f;
            float top = 24f;
            float bottom = rect.height - 26f;
            float width = right - left;
            float height = bottom - top;

            for (int i = 0; i <= 4; i++)
            {
                float y = top + height * i / 4f;
                DrawLine(vh, rect, new Vector2(left, y), new Vector2(right, y), GridColor, 1f);
            }
            DrawLine(vh, rect, new Vector2(left, top), new Vector2(left, bottom), AxisColor, 1.3f);
            DrawLine(vh, rect, new Vector2(left, bottom), new Vector2(right, bottom), AxisColor, 1.3f);

            if (baselineSeries == null || baselineSeries.Length < 3 || baselineSeries[0].Length == 0) return;
            int dataCount = baselineSeries[0].Length;
            for (int i = 0; i < dataCount; i++)
            {
                float x = dataCount == 1 ? left + width * 0.5f : left + width * i / (dataCount - 1);
                DrawLine(vh, rect, new Vector2(x, top), new Vector2(x, bottom), new Color(0.75f, 0.75f, 0.75f, 0.45f), 1f);
            }

            float maxError = 0.0001f;
            foreach (float value in baselineSeries[1]) maxError = Mathf.Max(maxError, value);
            foreach (float value in baselineSeries[2]) maxError = Mathf.Max(maxError, value);
            Color[] colors = { BlueColor, RedColor, GreenColor };

            for (int s = 0; s < baselineSeries.Length; s++)
            {
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < dataCount; i++)
                {
                    float x = dataCount == 1 ? left + width * 0.5f : left + width * i / (dataCount - 1);
                    float normalized = s == 0
                        ? Mathf.Clamp01(baselineSeries[s][i] / 100f)
                        : Mathf.Clamp01(baselineSeries[s][i] / maxError);
                    points.Add(new Vector2(x, bottom - normalized * height));
                }
                DrawPolyline(vh, rect, points, false, colors[s], 2.2f);
                foreach (Vector2 point in points) DrawCircle(vh, rect, point, 3.2f, colors[s]);
            }
        }

        private static void DrawPolygon(VertexHelper vh, Rect rect, IList<Vector2> points, Color color, float alpha)
        {
            if (points.Count < 3) return;
            Color fill = new Color(color.r, color.g, color.b, color.a * alpha);
            for (int i = 1; i < points.Count - 1; i++) DrawTriangle(vh, rect, points[0], points[i], points[i + 1], fill);
        }

        private static void DrawPolyline(VertexHelper vh, Rect rect, IList<Vector2> points, bool close, Color color, float width)
        {
            for (int i = 0; i < points.Count - 1; i++) DrawLine(vh, rect, points[i], points[i + 1], color, width);
            if (close && points.Count > 2) DrawLine(vh, rect, points[points.Count - 1], points[0], color, width);
        }

        private static void DrawLine(VertexHelper vh, Rect rect, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 direction = end - start;
            float length = direction.magnitude;
            if (length < 0.001f) return;
            Vector2 normal = new Vector2(-direction.y, direction.x).normalized * width * 0.5f;
            DrawQuad(vh, rect, start + normal, start - normal, end - normal, end + normal, color);
        }

        private static void DrawCircle(VertexHelper vh, Rect rect, Vector2 center, float radius, Color color)
        {
            const int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2f / segments;
                float a1 = (i + 1) * Mathf.PI * 2f / segments;
                Vector2 p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
                Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
                DrawTriangle(vh, rect, center, p0, p1, color);
            }
        }

        private static void DrawTriangle(VertexHelper vh, Rect rect, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            int index = vh.currentVertCount;
            AddVertex(vh, rect, a, color);
            AddVertex(vh, rect, b, color);
            AddVertex(vh, rect, c, color);
            vh.AddTriangle(index, index + 1, index + 2);
        }

        private static void DrawQuad(VertexHelper vh, Rect rect, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int index = vh.currentVertCount;
            AddVertex(vh, rect, a, color);
            AddVertex(vh, rect, b, color);
            AddVertex(vh, rect, c, color);
            AddVertex(vh, rect, d, color);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private static void AddVertex(VertexHelper vh, Rect rect, Vector2 point, Color color)
        {
            UIVertex vertex = new UIVertex
            {
                position = new Vector3(rect.x + point.x, rect.y + rect.height - point.y, 0f),
                color = color,
                uv0 = Vector2.zero
            };
            vh.AddVert(vertex);
        }
    }
}
