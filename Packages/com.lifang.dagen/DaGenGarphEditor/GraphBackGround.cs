using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DaGenGraph.Editor
{
    public static class GraphBackground
    {
        private const float MINOR_GRID_SIZE = 10f;
        private const float MAJOR_GRID_SIZE = 100f;

        private static readonly Color s_GridMinorColorDark = new Color(0f, 0f, 0f, 0.18f);
        private static readonly Color s_GridMajorColorDark = new Color(0f, 0f, 0f, 0.28f);
        private static readonly int s_HandleZTest = Shader.PropertyToID("_HandleZTest");

        public static void DrawGrid(Rect gridRect, float zoomLevel, Vector2 panOffset)
        {
            if (Event.current.type != EventType.Repaint) return;

            //draw background
            UnityEditor.Graphs.Styles.graphBackground.Draw(gridRect, false, false, false, false);

            HandleUtility.ApplyWireMaterial();
            GL.PushMatrix();
            GL.Begin(1);
            var t = Mathf.InverseLerp(0.1f, 1f, zoomLevel);
            DrawGridLines(gridRect, MINOR_GRID_SIZE * zoomLevel, Color.Lerp(Color.clear, s_GridMinorColorDark, t),
                panOffset);
            DrawGridLines(gridRect, MAJOR_GRID_SIZE * zoomLevel, Color.Lerp(s_GridMinorColorDark, s_GridMajorColorDark, t),
                panOffset);
            GL.End();
            GL.PopMatrix();
        }

        private static void DrawGridLines(Rect gridRect, float gridSize, Color gridColor, Vector2 panOffset)
        {
            //vertical lines
            GL.Color(gridColor);
            var scaledOffsetX = -panOffset.x + panOffset.x % gridSize;
            var x = gridRect.xMin - gridRect.xMin % gridSize + scaledOffsetX;
            while (x < (double) gridRect.xMax + scaledOffsetX)
            {
                DrawLine(new Vector2(x + panOffset.x, gridRect.yMin), new Vector2(x + panOffset.x, gridRect.yMax));
                x += gridSize;
            }
            //horizontal lines
            GL.Color(gridColor);
            var scaledOffsetY = -panOffset.y + panOffset.y % gridSize;
            var y = gridRect.yMin - gridRect.yMin % gridSize + scaledOffsetY;
            while (y < (double) gridRect.yMax + scaledOffsetY)
            {
                DrawLine(new Vector2(gridRect.xMin, y + panOffset.y), new Vector2(gridRect.xMax, y + panOffset.y));
                y += gridSize;
            }
        }

        private static void DrawLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }

        // Implementation from UnityEditor.HandleUtility
        private static class HandleUtility
        {
            private static Material s_HandleWireMaterial;
            private static Material s_HandleWireMaterial2D;

            internal static void ApplyWireMaterial(CompareFunction zTest = CompareFunction.Always)
            {
                var wireMaterial = HandleUtility.handleWireMaterial;
                wireMaterial.SetInt(s_HandleZTest, (int) zTest);
                wireMaterial.SetPass(0);
            }

            private static Material handleWireMaterial
            {
                get
                {
                    InitHandleMaterials();
                    return !Camera.current ? s_HandleWireMaterial2D : s_HandleWireMaterial;
                }
            }

            private static void InitHandleMaterials()
            {
                if (s_HandleWireMaterial) return;
                s_HandleWireMaterial = (Material) EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat");
                s_HandleWireMaterial2D = (Material) EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");
            }
        }
    }
}