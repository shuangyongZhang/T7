using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;
using XUGL;

namespace LB
{
    [AddComponentMenu("LB/ROM Range Chart")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    [DisallowMultipleComponent]
    public class RomRangeChart : BaseChart
    {
        [SerializeField] [ListForComponent(typeof(GridCoord3D))] private List<GridCoord3D> grids3D = new List<GridCoord3D>();
        [SerializeField] [ListForComponent(typeof(XAxis3D))] private List<XAxis3D> xAxes3D = new List<XAxis3D>();
        [SerializeField] [ListForComponent(typeof(YAxis3D))] private List<YAxis3D> yAxes3D = new List<YAxis3D>();
        [SerializeField] [ListForComponent(typeof(ZAxis3D))] private List<ZAxis3D> zAxes3D = new List<ZAxis3D>();

        private static readonly int[][] CubeEdges =
        {
            new[] { 0, 1 }, new[] { 1, 2 }, new[] { 2, 3 }, new[] { 3, 0 },
            new[] { 4, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 4 },
            new[] { 0, 4 }, new[] { 1, 5 }, new[] { 2, 6 }, new[] { 3, 7 }
        };
        private static readonly int[][] VolumeFaces =
        {
            new[] { 0, 1, 2, 3 }, new[] { 4, 5, 6, 7 },
            new[] { 0, 1, 5, 4 }, new[] { 1, 2, 6, 5 },
            new[] { 2, 3, 7, 6 }, new[] { 3, 0, 4, 7 }
        };

        private Vector3 extents;
        private Vector3 axisMaxima = Vector3.one;
        private List<Vector3> envelopePoints;
        private List<int[]> envelopeHull;
        private bool hasData;

        protected override void Awake()
        {
            Vector2 size = rectTransform.sizeDelta;
            base.Awake();
            CheckChartInit();
            onDraw = DrawRomVolume;
            rectTransform.sizeDelta = size;
            UpdateSize();
            UpdateCubeLayout();
            raycastTarget = false;
            RestoreRomLabels();
            RefreshChart();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            Vector2 size = rectTransform.sizeDelta;
            base.Reset();
            rectTransform.sizeDelta = size;
            UpdateSize();
            UpdateCubeLayout();
            raycastTarget = false;
            RestoreRomLabels();
            RefreshChart();
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            onDraw = DrawRomVolume;
            RestoreRomLabels();
        }

        protected override void DefaultChart()
        {
            RemoveChartComponent<Title>();
            RemoveChartComponent<Tooltip>();
            RemoveChartComponent<Background>();

            GridCoord3D grid = AddChartComponent<GridCoord3D>();
            grid.showBorder = false;
            grid.viewControl.alpha = 90f;
            grid.viewControl.beta = 70f;

            ConfigureAxes();
            UpdateCubeLayout();
            onDraw = DrawRomVolume;
        }

        public void SetRomData(float widthCm, float heightCm, float depthCm)
        {
            SetRomData(widthCm, heightCm, depthCm, null);
        }

        public void SetRomData(float widthCm, float heightCm, float depthCm, IList<Vector3> rangePoints)
        {
            extents = new Vector3(Mathf.Max(0f, widthCm), Mathf.Max(0f, depthCm), Mathf.Max(0f, heightCm));
            hasData = true;
            envelopePoints = null;
            envelopeHull = null;
            UpdateAxisMaximum();
            if (rangePoints != null && rangePoints.Count >= 4)
            {
                envelopePoints = NormalizeEnvelopePoints(rangePoints);
                envelopeHull = RomEnvelopeCalculator.BuildConvexHull(envelopePoints);
            }
            UpdateCubeLayout();
            RefreshChart();
        }

        public void ClearData()
        {
            hasData = false;
            envelopePoints = null;
            envelopeHull = null;
            RefreshChart();
        }

        public bool TryGetRomAxisLabelPositions(out Vector2 vertical, out Vector2 coronal, out Vector2 sagittal)
        {
            GridCoord3D grid = GetChartComponent<GridCoord3D>();
            Rect rect = rectTransform.rect;
            if (grid == null || rect.width <= 1f || rect.height <= 1f)
            {
                vertical = coronal = sagittal = Vector2.zero;
                return false;
            }
            vertical = ToTopLeft(grid.context.pointE, rect) + new Vector2(-17f, -4f);
            coronal = ToTopLeft(grid.context.pointB, rect) + new Vector2(-4f, 14f);
            sagittal = ToTopLeft(grid.context.pointD, rect) + new Vector2(25f, -10f);
            return true;
        }

        protected override void DrawPainterBase(VertexHelper vh)
        {
            GridCoord3D grid = GetChartComponent<GridCoord3D>();
            if (grid == null) return;
            Vector3[] corners =
            {
                grid.context.pointA, grid.context.pointB, grid.context.pointC, grid.context.pointD,
                grid.context.pointE, grid.context.pointF, grid.context.pointG, grid.context.pointH
            };
            DrawSortedFaces(vh, corners, new Color32(190, 205, 225, 18), false);
            foreach (int[] edge in CubeEdges)
            {
                float depth = (corners[edge[0]].y + corners[edge[1]].y) * 0.5f;
                UGL.DrawLine(vh, corners[edge[0]], corners[edge[1]], 0.9f,
                    new Color32(105, 120, 140, (byte)(depth > grid.context.y ? 150 : 85)));
            }
        }

        private void DrawRomVolume(VertexHelper vh)
        {
            if (!hasData) return;
            GridCoord3D grid = GetChartComponent<GridCoord3D>();
            XAxis3D xAxis = GetChartComponent<XAxis3D>();
            YAxis3D yAxis = GetChartComponent<YAxis3D>();
            ZAxis3D zAxis = GetChartComponent<ZAxis3D>();
            if (grid == null || xAxis == null || yAxis == null || zAxis == null) return;
            if (envelopeHull != null && envelopePoints != null)
            {
                DrawRomHull(vh, grid, xAxis, yAxis, zAxis);
                return;
            }

            Vector3 minimum = (axisMaxima - extents) * 0.5f;
            Vector3 maximum = (axisMaxima + extents) * 0.5f;
            Vector3[] data =
            {
                new Vector3(minimum.x, minimum.y, minimum.z), new Vector3(maximum.x, minimum.y, minimum.z),
                new Vector3(maximum.x, maximum.y, minimum.z), new Vector3(minimum.x, maximum.y, minimum.z),
                new Vector3(minimum.x, minimum.y, maximum.z), new Vector3(maximum.x, minimum.y, maximum.z),
                new Vector3(maximum.x, maximum.y, maximum.z), new Vector3(minimum.x, maximum.y, maximum.z)
            };
            Vector3[] points = new Vector3[data.Length];
            for (int i = 0; i < data.Length; i++)
                points[i] = MapRomPoint(grid, xAxis, yAxis, zAxis, data[i].x, data[i].y, data[i].z);

            DrawSortedFaces(vh, points, new Color32(20, 68, 135, 58), true);
            foreach (int[] edge in CubeEdges)
                UGL.DrawLine(vh, points[edge[0]], points[edge[1]], 1.35f, new Color32(18, 61, 128, 235));
            for (int i = 0; i < 4; i++)
            {
                Vector3 floor = MapRomPoint(grid, xAxis, yAxis, zAxis, data[i].x, data[i].y, 0d);
                UGL.DrawLine(vh, points[i], floor, 0.75f, new Color32(25, 76, 148, 75));
            }
            foreach (Vector3 point in points) UGL.DrawCricle(vh, point, 2.2f, new Color32(15, 54, 119, 255));
        }

        private List<Vector3> NormalizeEnvelopePoints(IList<Vector3> source)
        {
            int step = Mathf.Max(1, Mathf.CeilToInt(source.Count / 600f));
            List<Vector3> sampled = new List<Vector3>();
            for (int i = 0; i < source.Count; i += step) sampled.Add(source[i]);
            if ((source.Count - 1) % step != 0) sampled.Add(source[source.Count - 1]);

            Vector3 min = sampled[0];
            Vector3 max = sampled[0];
            foreach (Vector3 point in sampled)
            {
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }
            Vector3 offset = (axisMaxima - extents) * 0.5f;
            List<Vector3> normalized = new List<Vector3>(sampled.Count);
            HashSet<Vector3> unique = new HashSet<Vector3>();
            foreach (Vector3 point in sampled)
            {
                Vector3 value = point - min + offset;
                if (unique.Add(value)) normalized.Add(value);
            }
            return normalized;
        }

        private void DrawRomHull(VertexHelper vh, GridCoord3D grid, XAxis3D xAxis, YAxis3D yAxis, ZAxis3D zAxis)
        {
            Vector3[] points = new Vector3[envelopePoints.Count];
            for (int i = 0; i < envelopePoints.Count; i++)
                points[i] = MapRomPoint(grid, xAxis, yAxis, zAxis, envelopePoints[i].x, envelopePoints[i].y, envelopePoints[i].z);

            List<int[]> faces = new List<int[]>(envelopeHull);
            faces.Sort((a, b) => AverageY(points, a).CompareTo(AverageY(points, b)));
            for (int i = 0; i < faces.Count; i++)
            {
                int[] face = faces[i];
                byte alpha = (byte)(48 + i % 3 * 8);
                UGL.DrawTriangle(vh, points[face[0]], points[face[1]], points[face[2]], new Color32(20, 68, 135, alpha));
            }

            HashSet<long> edges = new HashSet<long>();
            foreach (int[] face in envelopeHull)
            {
                DrawHullEdge(vh, points, edges, face[0], face[1]);
                DrawHullEdge(vh, points, edges, face[1], face[2]);
                DrawHullEdge(vh, points, edges, face[2], face[0]);
            }
        }

        private static void DrawHullEdge(VertexHelper vh, Vector3[] points, HashSet<long> edges, int a, int b)
        {
            int min = Mathf.Min(a, b);
            int max = Mathf.Max(a, b);
            long key = (long)min * int.MaxValue + max;
            if (!edges.Add(key)) return;
            UGL.DrawLine(vh, points[a], points[b], 1.05f, new Color32(18, 61, 128, 205));
        }

        private static Vector3 MapRomPoint(GridCoord3D grid, XAxis3D xAxis, YAxis3D yAxis, ZAxis3D zAxis,
            double xValue, double yValue, double zValue)
        {
            float x = xAxis.context.length * (float)(xValue / System.Math.Max(0.0001d, xAxis.max));
            float y = yAxis.context.length * (float)(yValue / System.Math.Max(0.0001d, yAxis.max));
            float z = zAxis.context.length * (float)(zValue / System.Math.Max(0.0001d, zAxis.max));
            return grid.context.pointA + xAxis.context.dire * x + yAxis.context.dire * y + zAxis.context.dire * z;
        }

        private static void DrawSortedFaces(VertexHelper vh, Vector3[] points, Color32 color, bool triangulate)
        {
            List<int[]> faces = new List<int[]>(VolumeFaces);
            faces.Sort((a, b) => AverageY(points, a).CompareTo(AverageY(points, b)));
            foreach (int[] face in faces)
            {
                UGL.DrawQuadrilateral(vh, points[face[0]], points[face[1]], points[face[2]], points[face[3]], color);
                if (triangulate)
                    UGL.DrawLine(vh, points[face[0]], points[face[2]], 0.65f, new Color32(25, 76, 148, 105));
            }
        }

        private static float AverageY(Vector3[] points, int[] face)
        {
            float sum = 0f;
            foreach (int index in face) sum += points[index].y;
            return sum / face.Length;
        }

        private void ConfigureAxes()
        {
            ConfigureAxis(AddChartComponent<XAxis3D>(), true);
            ConfigureAxis(AddChartComponent<YAxis3D>(), true);
            ConfigureAxis(AddChartComponent<ZAxis3D>(), false);
        }

        private static void ConfigureAxis(Axis axis, bool floorAxis)
        {
            axis.type = Axis.AxisType.Value;
            axis.minMaxType = Axis.AxisMinMaxType.Custom;
            axis.min = 0d;
            axis.max = 1d;
            axis.splitNumber = 4;
            axis.axisLabel.show = false;
            axis.axisName.show = false;
            axis.axisTick.show = true;
            axis.axisLine.show = true;
            axis.splitLine.show = true;
            if (floorAxis) axis.splitLine.showZLine = true;
        }

        private void UpdateAxisMaximum()
        {
            axisMaxima = new Vector3(NiceMaximum(extents.x), NiceMaximum(extents.y), NiceMaximum(extents.z));
            XAxis3D xAxis = GetChartComponent<XAxis3D>();
            YAxis3D yAxis = GetChartComponent<YAxis3D>();
            ZAxis3D zAxis = GetChartComponent<ZAxis3D>();
            if (xAxis != null) { xAxis.min = 0d; xAxis.max = axisMaxima.x; }
            if (yAxis != null) { yAxis.min = 0d; yAxis.max = axisMaxima.y; }
            if (zAxis != null) { zAxis.min = 0d; zAxis.max = axisMaxima.z; }
        }

        private static float NiceMaximum(float value)
        {
            float maximum = Mathf.Max(0f, value, 0.001f);
            float magnitude = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10(maximum)));
            float normalized = maximum / magnitude;
            float[] steps = { 1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f, 6f, 8f, 10f };
            foreach (float step in steps)
            {
                if (normalized <= step) return step * magnitude;
            }
            return 10f * magnitude;
        }

        private void UpdateCubeLayout()
        {
            GridCoord3D grid = GetChartComponent<GridCoord3D>();
            if (grid == null) return;
            float width = Mathf.Max(1f, rectTransform.rect.width);
            float height = Mathf.Max(1f, rectTransform.rect.height);
            float alpha = grid.viewControl.alpha * Mathf.Deg2Rad;
            float beta = grid.viewControl.beta * Mathf.Deg2Rad;
            float horizontal = Mathf.Abs(Mathf.Sin(alpha)) + Mathf.Abs(Mathf.Sin(beta));
            float vertical = Mathf.Abs(Mathf.Cos(alpha)) + Mathf.Abs(Mathf.Cos(beta)) + 1f;
            float edge = Mathf.Min((width - 40f) / Mathf.Max(0.01f, horizontal),
                (height - 20f) / Mathf.Max(0.01f, vertical));
            edge = Mathf.Max(40f, edge);
            float left = (width - edge * horizontal) * 0.5f;
            float bottom = (height - edge * vertical) * 0.5f + edge * Mathf.Abs(Mathf.Cos(alpha));
            grid.boxWidth = edge;
            grid.boxHeight = edge;
            grid.boxDepth = edge;
            grid.left = left;
            grid.right = Mathf.Max(0f, width - left - edge * horizontal);
            grid.bottom = bottom;
            grid.top = Mathf.Max(0f, height - bottom - edge * (Mathf.Abs(Mathf.Cos(beta)) + 1f));
            grid.UpdateRuntimeData(this);
        }

        private static Vector2 ToTopLeft(Vector3 point, Rect rect)
        {
            return new Vector2(point.x - rect.x, rect.height - (point.y - rect.y));
        }

        private void RestoreRomLabels()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("Rom")) child.gameObject.SetActive(true);
            }
        }
    }
}
