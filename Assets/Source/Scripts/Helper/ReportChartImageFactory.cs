using System.Collections.Generic;
using UnityEngine;

namespace LB
{
    public static class ReportChartImageFactory
    {
        private const int RomCellWidth = 600;
        private const int RomCellHeight = 300;

        public static byte[] CreateRomComposite(ReportDocumentModel model)
        {
            if (model == null || model.History == null || model.History.Count == 0) return null;
            Texture2D composite = new Texture2D(RomCellWidth * 2, RomCellHeight * 2, TextureFormat.RGBA32, false);
            FillWhite(composite);

            for (int i = 0; i < 4 && i < model.History.Count; i++)
            {
                ReportHistoryItem item = model.History[i];
                RehabRomMetrics rom = item != null ? item.Rom : null;
                if (rom == null || !rom.HasData) continue;
                byte[] png = CreateRomChart(rom, item.RomPoints);
                if (png == null) continue;
                Texture2D chart = LoadTexture(png);
                if (chart == null) continue;
                int x = (i % 2) * RomCellWidth;
                int y = (1 - i / 2) * RomCellHeight;
                CopyTexture(composite, chart, x, y, RomCellWidth, RomCellHeight);
                Object.DestroyImmediate(chart);
            }

            byte[] result = composite.EncodeToPNG();
            Object.DestroyImmediate(composite);
            return result;
        }

        public static byte[] CreateStrengthRadar(ReportDocumentModel model)
        {
            if (model == null || model.History == null) return null;
            List<float[]> series = new List<float[]>();
            foreach (ReportHistoryItem item in model.History)
            {
                RehabStrengthMetrics strength = item != null ? item.Strength : null;
                if (strength == null || !strength.HasData) continue;
                series.Add(new[] { strength.Flexion, strength.Extension, strength.Up, strength.Right, strength.Left, strength.Down });
            }
            if (series.Count == 0) return null;

            return Capture(393, 224, parent =>
            {
                GameObject go = CreateChild(parent, "StrengthRadar", 393, 224);
                ReportChartGraphic chart = go.AddComponent<ReportChartGraphic>();
                chart.Kind = ReportChartGraphic.ChartKind.StrengthRadar;
                chart.SetStrengthData(series.ToArray());
                chart.SetVerticesDirty();
                return chart;
            });
        }

        public static byte[] CreateBaselineLine(ReportDocumentModel model)
        {
            if (model == null || model.History == null) return null;
            List<float> participation = new List<float>();
            List<float> trackingError = new List<float>();
            List<float> deviation = new List<float>();
            foreach (ReportHistoryItem item in model.History)
            {
                RehabBaselineMetrics baseline = item != null ? item.Baseline : null;
                if (baseline == null || !baseline.HasData || baseline.Trajectory == null || !baseline.Trajectory.HasData) continue;
                participation.Add(baseline.ActiveParticipationPercent);
                trackingError.Add(baseline.Trajectory.TrajectoryTrackingErrorMm);
                deviation.Add(baseline.Trajectory.TrajectoryDeviationMm);
            }
            if (participation.Count == 0) return null;

            return Capture(1105, 224, parent =>
            {
                GameObject go = CreateChild(parent, "BaselineLine", 1105, 224);
                ReportChartGraphic chart = go.AddComponent<ReportChartGraphic>();
                chart.Kind = ReportChartGraphic.ChartKind.BaselineLine;
                chart.SetBaselineData(participation.ToArray(), trackingError.ToArray(), deviation.ToArray());
                chart.SetVerticesDirty();
                return chart;
            });
        }

        private static byte[] CreateRomChart(RehabRomMetrics rom, List<Vector3> points)
        {
            return Capture(RomCellWidth, RomCellHeight, parent =>
            {
                GameObject go = CreateChild(parent, "RomRangeChart", RomCellWidth, RomCellHeight);
                RomRangeChart chart = go.AddComponent<RomRangeChart>();
                chart.SetRomData(rom.WidthCm, rom.HeightCm, rom.DepthCm, points);
                chart.RefreshChart();
                SetLayerRecursive(chart.gameObject, LayerMask.NameToLayer("UI"));
                return chart;
            });
        }

        private static byte[] Capture(int width, int height, System.Func<RectTransform, UnityEngine.UI.Graphic> build)
        {
            GameObject cameraGo = new GameObject("T7_ReportChartCaptureCamera");
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.white;
            camera.orthographic = true;
            camera.aspect = (float)width / height;
            camera.orthographicSize = height * 0.5f;
            camera.cullingMask = ~0;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = renderTexture;

            GameObject canvasGo = new GameObject("T7_ReportChartCaptureCanvas");
            canvasGo.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.position = new Vector3(0f, 0f, -9f);
            canvasRect.rotation = Quaternion.identity;
            canvasRect.sizeDelta = new Vector2(width, height);
            canvasRect.localScale = Vector3.one;

            UnityEngine.UI.Graphic graphic = build(canvasRect);
            Canvas.ForceUpdateCanvases();
            RomRangeChart romChart = graphic as RomRangeChart;
            if (romChart != null)
            {
                romChart.RefreshChart();
                RestoreRomPainterScales(romChart);
            }
            else if (graphic != null) graphic.SetVerticesDirty();
            Canvas.ForceUpdateCanvases();
            camera.Render();

            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            texture.Apply();
            byte[] png = texture.EncodeToPNG();

            RenderTexture.active = null;
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(canvasGo);
            camera.targetTexture = null;
            Object.DestroyImmediate(renderTexture);
            Object.DestroyImmediate(cameraGo);
            return png;
        }

        private static void RestoreRomPainterScales(RomRangeChart chart)
        {
            if (chart == null) return;
            SetLocalScale(chart.transform, "painter_b");
            SetLocalScale(chart.transform, "painter_t");
            SetLocalScale(chart.transform, "painter_u");
        }

        private static void SetLocalScale(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null) child.localScale = Vector3.one;
        }

        private static void SetLayerRecursive(GameObject root, int layer)
        {
            if (root == null) return;
            root.layer = layer;
            foreach (Transform child in root.transform) SetLayerRecursive(child.gameObject, layer);
        }

        private static GameObject CreateChild(RectTransform parent, string name, int width, int height)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
            return go;
        }

        private static Texture2D LoadTexture(byte[] png)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(png))
            {
                Object.DestroyImmediate(texture);
                return null;
            }
            return texture;
        }

        private static void FillWhite(Texture2D texture)
        {
            Color32[] pixels = new Color32[texture.width * texture.height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply();
        }

        private static void CopyTexture(Texture2D target, Texture2D source, int x, int y, int width, int height)
        {
            Color32[] sourcePixels = source.GetPixels32();
            Color32[] targetPixels = target.GetPixels32();
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int sourceIndex = row * source.width + column;
                    int targetIndex = (y + row) * target.width + x + column;
                    if (sourceIndex >= 0 && sourceIndex < sourcePixels.Length && targetIndex >= 0 && targetIndex < targetPixels.Length)
                        targetPixels[targetIndex] = sourcePixels[sourceIndex];
                }
            }
            target.SetPixels32(targetPixels);
            target.Apply();
        }
    }
}
