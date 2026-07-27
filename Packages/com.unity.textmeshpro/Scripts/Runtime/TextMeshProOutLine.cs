using UnityEngine;
using UnityEngine.Serialization;

namespace TMPro
{
    public enum OutlineDirection
    {
        Centered = 0,  // 向外描边：描边在字形外侧
        Inward = 1,   // 向内描边：描边在字形内侧
        Outward = 2, // 中间描边：描边居中于字形边缘
    }

    [RequireComponent(typeof(TMP_Text))]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class TextMeshProOutLine : MonoBehaviour
    {
        [Range(0, 5)]
        public float faceDilate;
        [Range(0, 5)]
        public float outlineWidth;
        
        public Color32 outlineColor = Color.black;
        public OutlineDirection outlineDirection = OutlineDirection.Centered;

        [Range(0, 1)]
        public float underlayDilate = 0f;
        [Range(-1, 1)]
        public float underlayOffsetX = 0f;
        [Range(-1, 1)]
        public float underlayOffsetY = 0f;

        private const int k_Samples = 32;
        private TMP_Text _mainText;
        private int _lastOrigVC = -1; // unused, kept for potential future use
        private bool _needsMeshUpdate;
        private bool _isProcessing; // reentrancy guard
#if UNITY_EDITOR
        private bool _lastShellState;
#endif
        public void Awake()
        {
            _mainText = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            _mainText = GetComponent<TMP_Text>();
            if (_mainText != null)
            {
                _mainText.OnPreRenderText += OnTMPPreRender;
            }
        }

        private void Start()
        {
            // Refresh after full activation (Awake/OnEnable may run before Canvas recognizes active state)
            Refresh();
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (_mainText == null) return;
            bool currentShell = _mainText.fontSharedMaterial != null && _mainText.fontSharedMaterial.IsKeywordEnabled("OUTLINE_SHELL_ON");
            if (currentShell != _lastShellState)
            {
                _lastShellState = currentShell;
                Refresh();
            }
        }
#endif
        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (_mainText == null) return;
            bool shell = _mainText.fontSharedMaterial != null && _mainText.fontSharedMaterial.IsKeywordEnabled("OUTLINE_SHELL_ON");
#if UNITY_EDITOR
            _lastShellState = false; // force refresh on first Update
#endif
            bool hasOutline = shell && outlineWidth > 0.001f;

            if (hasOutline)
            {
                _mainText.outlineWidth = 0;

                // faceDilate controls inward portion of outline
                // Outward: faceDilate = 0 (face unchanged, copies extend outward)
                // Inward:  faceDilate = -outlineWidth (face shrinks inward, copies at original position fill gap)
                // Centered: faceDilate = -outlineWidth * 0.5 (half inward + half outward)
                float shellFaceDilate;
                switch (outlineDirection)
                {
                    case OutlineDirection.Inward:
                        shellFaceDilate = -outlineWidth * 0.2f;
                        break;
                    case OutlineDirection.Centered:
                        shellFaceDilate = -outlineWidth * 0.1f;
                        break;
                    default:
                        shellFaceDilate = 0f;
                        break;
                }
                _mainText.faceDilate = shellFaceDilate;
            }
            else
            {
                // No shell: use normal SDF outline with direction support
                // SDF outline is centered by default (half inward, half outward)
                // Outward: faceDilate pushes face outward, making outline fully external
                // Inward: negative faceDilate shrinks face inward, making outline fully internal
                // Centered: standard SDF behavior
                switch (outlineDirection)
                {
                    case OutlineDirection.Outward:
                        _mainText.faceDilate = faceDilate + outlineWidth * 0.2f;
                        break;
                    case OutlineDirection.Inward:
                        _mainText.faceDilate = faceDilate - outlineWidth * 0.2f;
                        break;
                    default:
                        _mainText.faceDilate = faceDilate;
                        break;
                }
                _mainText.outlineWidth = outlineWidth;
            }

            _mainText.underlayOffsetX = underlayOffsetX;
            _mainText.underlayOffsetY = underlayOffsetY;
            _mainText.underlayDilate = underlayDilate;
            // Outline color goes through tangent channel (effectColorFloat)
            _mainText.effectColorFloat = new Vector4(
                outlineColor.r / 255f, outlineColor.g / 255f, outlineColor.b / 255f, outlineColor.a / 255f);

            if (_isProcessing) return;
            _isProcessing = true;
            _mainText.ForceMeshUpdate();
            _isProcessing = false;

            // Push expanded mesh immediately (don't wait for LateUpdate)
            if (_needsMeshUpdate)
            {
                _needsMeshUpdate = false;
                PushMeshToRenderer();
            }

            // If shell is off but mesh was previously expanded, push clean mesh to renderer
            if (!shell && _mainText is TextMeshProUGUI && _mainText.textInfo.meshInfo.Length > 0)
            {
                var mi = _mainText.textInfo.meshInfo[0];
                if (mi.mesh != null)
                {
                    mi.mesh.MarkDynamic();
                    mi.mesh.vertices = mi.vertices;
                    mi.mesh.uv = mi.uvs0;
                    mi.mesh.uv2 = mi.uvs2;
                    mi.mesh.uv3 = mi.uvs3;
                    mi.mesh.uv4 = mi.uvs4;
                    mi.mesh.colors32 = mi.colors32;
                    mi.mesh.tangents = mi.tangents;
                    mi.mesh.triangles = mi.triangles;
                    mi.mesh.RecalculateBounds();
                    (_mainText as TextMeshProUGUI).canvasRenderer.SetMesh(mi.mesh);
                }
            }
        }

        private void OnDisable()
        {
            if (_mainText != null)
                _mainText.OnPreRenderText -= OnTMPPreRender;
        }

        private void OnTMPPreRender(TMP_TextInfo textInfo)
        {
            // Skip if outlineWidth is 0
            if (outlineWidth <= 0.001f)
            {
                ClearMeshTriangles(textInfo);
                _needsMeshUpdate = true;
                return;
            }

            Material mat = _mainText.fontSharedMaterial;
            if (mat == null || !mat.IsKeywordEnabled("OUTLINE_SHELL_ON"))
            {
                ResetExpandedMesh(textInfo);
                ClearMeshTriangles(textInfo);
                _needsMeshUpdate = true;
                return;
            }

            // Shell mode: expand vertices. TMP will push expanded data to mesh after this callback.
            DuplicateQuadsForOutline(textInfo);
            _needsMeshUpdate = true;
        }

        private void ClearMeshTriangles(TMP_TextInfo textInfo)
        {
            if (textInfo == null || textInfo.meshInfo == null) return;
            for (int m = 0; m < textInfo.meshInfo.Length; m++)
            {
                var meshInfo = textInfo.meshInfo[m];
                if (meshInfo.mesh == null) continue;
                int vc = meshInfo.vertexCount;

                // Build correct-sized arrays based on vertexCount (not array.Length)
                // This fixes the struct copy issue where Array.Resize doesn't propagate
                if (vc > 0 && meshInfo.vertices != null && meshInfo.vertices.Length >= vc)
                {
                    var verts = new Vector3[vc];
                    var uvs0 = new Vector2[vc];
                    var uvs2 = new Vector2[vc];
                    var uvs3 = new Vector2[vc];
                    var uvs4 = new Vector2[vc];
                    var colors = new Color32[vc];
                    var tangents = new Vector4[vc];
                    var tris = new int[(vc / 4) * 6];

                    System.Array.Copy(meshInfo.vertices, verts, vc);
                    System.Array.Copy(meshInfo.uvs0, uvs0, vc);
                    if (meshInfo.uvs2 != null) System.Array.Copy(meshInfo.uvs2, uvs2, vc);
                    if (meshInfo.uvs3 != null) System.Array.Copy(meshInfo.uvs3, uvs3, vc);
                    if (meshInfo.uvs4 != null) System.Array.Copy(meshInfo.uvs4, uvs4, vc);
                    System.Array.Copy(meshInfo.colors32, colors, vc);
                    if (meshInfo.tangents != null) System.Array.Copy(meshInfo.tangents, tangents, vc);

                    for (int i = 0; i < vc / 4; i++)
                    {
                        int b = i * 4;
                        int t = i * 6;
                        tris[t] = b;     tris[t+1] = b+1; tris[t+2] = b+2;
                        tris[t+3] = b+2; tris[t+4] = b+3; tris[t+5] = b;
                    }

                    meshInfo.vertices = verts;
                    meshInfo.uvs0 = uvs0;
                    meshInfo.uvs2 = uvs2;
                    meshInfo.uvs3 = uvs3;
                    meshInfo.uvs4 = uvs4;
                    meshInfo.colors32 = colors;
                    meshInfo.tangents = tangents;
                    meshInfo.triangles = tris;
                    textInfo.meshInfo[m] = meshInfo;
                }
            }
        }

        private void LateUpdate()
        {
            if (!_needsMeshUpdate) return;
            _needsMeshUpdate = false;
            PushMeshToRenderer();
        }

        private void PushMeshToRenderer()
        {
            if (_mainText == null || _mainText.textInfo == null) return;
            for (int m = 0; m < _mainText.textInfo.meshInfo.Length; m++)
            {
                var meshInfo = _mainText.textInfo.meshInfo[m];
                if (meshInfo.mesh == null) continue;

                int vc = meshInfo.vertexCount;
                if (vc <= 0 || meshInfo.vertices == null) continue;

                // Rebuild correctly-sized arrays based on vertexCount
                // (array may be larger than vertexCount due to struct copy semantics)
                var verts = new Vector3[vc];
                var uvs0 = new Vector2[vc];
                var uvs2 = new Vector2[vc];
                var uvs3 = new Vector2[vc];
                var uvs4 = new Vector2[vc];
                var colors = new Color32[vc];
                var tangents = new Vector4[vc];
                var tris = new int[(vc / 4) * 6];

                System.Array.Copy(meshInfo.vertices, verts, vc);
                System.Array.Copy(meshInfo.uvs0, uvs0, vc);
                if (meshInfo.uvs2 != null) System.Array.Copy(meshInfo.uvs2, uvs2, vc);
                if (meshInfo.uvs3 != null) System.Array.Copy(meshInfo.uvs3, uvs3, vc);
                if (meshInfo.uvs4 != null) System.Array.Copy(meshInfo.uvs4, uvs4, vc);
                System.Array.Copy(meshInfo.colors32, colors, vc);
                if (meshInfo.tangents != null) System.Array.Copy(meshInfo.tangents, tangents, vc);

                for (int i = 0; i < vc / 4; i++)
                {
                    int b = i * 4;
                    int t = i * 6;
                    tris[t] = b;     tris[t+1] = b+1; tris[t+2] = b+2;
                    tris[t+3] = b+2; tris[t+4] = b+3; tris[t+5] = b;
                }

                meshInfo.mesh.Clear();
                meshInfo.mesh.MarkDynamic();
                meshInfo.mesh.vertices = verts;
                meshInfo.mesh.uv = uvs0;
                meshInfo.mesh.uv2 = uvs2;
                meshInfo.mesh.uv3 = uvs3;
                meshInfo.mesh.uv4 = uvs4;
                meshInfo.mesh.colors32 = colors;
                meshInfo.mesh.tangents = tangents;
                meshInfo.mesh.triangles = tris;
                meshInfo.mesh.RecalculateBounds();

                if (_mainText is TextMeshProUGUI)
                {
                    var cr = (_mainText as TextMeshProUGUI).canvasRenderer;
                    if (cr != null)
                        cr.SetMesh(meshInfo.mesh);
                }
            }
        }

        private void DuplicateQuadsForOutline(TMP_TextInfo textInfo)
        {
            if (textInfo == null || textInfo.meshInfo == null) return;

            float lossyScale = _mainText.transform.lossyScale.y;
            float canvasScale = 1f;
            var canvas = _mainText.canvas;
            if (canvas != null)
                canvasScale = canvas.scaleFactor;

            float localPerPixel;
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                localPerPixel = lossyScale / canvasScale;
            else
                localPerPixel = lossyScale;

            float offset = outlineWidth * 2f * localPerPixel;

            // Determine offset multipliers based on direction
            // copyMul: how much to shift outline copies
            // origMul: how much to shift original quads (opposite direction reveals outline)
            float copyMul, origMul;
            switch (outlineDirection)
            {
                case OutlineDirection.Inward:
                    copyMul = 0f;      // copies stay at glyph position
                    origMul = 0f;      // original stays, faceDilate handles inward
                    break;
                case OutlineDirection.Centered:
                    copyMul = 0.5f;    // copies shifted half outward
                    origMul = 0f;      // original stays, faceDilate handles half inward
                    break;
                default: // Outward
                    copyMul = 1f;      // copies shifted full outward
                    origMul = 0f;      // original stays
                    break;
            }

            for (int m = 0; m < textInfo.meshInfo.Length; m++)
            {
                var meshInfo = textInfo.meshInfo[m];
                if (meshInfo.vertices == null || meshInfo.vertexCount == 0) continue;

                // Detect if already expanded from previous frame (TMP_MeshInfo is a struct,
                // Clear(false) may not reset vertexCount on the array element)
                int origVC = meshInfo.vertexCount;
                if (origVC > 4 && origVC % (k_Samples + 1) == 0)
                {
                    // Already expanded — compute original and truncate arrays
                    int possibleOrig = origVC / (k_Samples + 1);
                    if (possibleOrig > 0 && possibleOrig * 4 <= textInfo.characterCount * 4 + 16)
                    {
                        origVC = possibleOrig;
                        System.Array.Resize(ref meshInfo.vertices, origVC);
                        System.Array.Resize(ref meshInfo.uvs0, origVC);
                        if (meshInfo.uvs2 != null) System.Array.Resize(ref meshInfo.uvs2, origVC);
                        if (meshInfo.uvs3 != null) System.Array.Resize(ref meshInfo.uvs3, origVC);
                        if (meshInfo.uvs4 != null) System.Array.Resize(ref meshInfo.uvs4, origVC);
                        System.Array.Resize(ref meshInfo.colors32, origVC);
                        if (meshInfo.tangents != null) System.Array.Resize(ref meshInfo.tangents, origVC);
                        meshInfo.vertexCount = origVC;
                    }
                }
                int origQuadCount = origVC / 4;
                int newVC = origVC * (k_Samples + 1);
                int newTriCount = origQuadCount * (k_Samples + 1);

                var newVerts = new Vector3[newVC];
                var newUVs0 = new Vector2[newVC];
                var newUVs2 = new Vector2[newVC];
                var newUVs3 = new Vector2[newVC];
                var newUVs4 = new Vector2[newVC];
                var newColors = new Color32[newVC];
                var newTangents = new Vector4[newVC];
                var newTris = new int[newTriCount * 6];

                int vIdx = 0;
                int tIdx = 0;

                for (int s = 0; s < k_Samples; s++)
                {
                    float angle = (float)s / k_Samples * Mathf.PI * 2f;
                    Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * offset;

                    for (int q = 0; q < origVC; q += 4)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int src = q + j;
                            newVerts[vIdx] = meshInfo.vertices[src] + dir * copyMul;
                            newUVs0[vIdx] = meshInfo.uvs0[src];
                            if (meshInfo.uvs2 != null && src < meshInfo.uvs2.Length) newUVs2[vIdx] = meshInfo.uvs2[src];
                            if (meshInfo.uvs3 != null && src < meshInfo.uvs3.Length) newUVs3[vIdx] = meshInfo.uvs3[src];
                            if (meshInfo.uvs4 != null && src < meshInfo.uvs4.Length) newUVs4[vIdx] = meshInfo.uvs4[src];
                            newColors[vIdx] = outlineColor;
                            if (meshInfo.tangents != null && src < meshInfo.tangents.Length) newTangents[vIdx] = meshInfo.tangents[src];
                            vIdx++;
                        }
                        int b = vIdx - 4;
                        newTris[tIdx++] = b;     newTris[tIdx++] = b + 1; newTris[tIdx++] = b + 2;
                        newTris[tIdx++] = b + 2; newTris[tIdx++] = b + 3; newTris[tIdx++] = b;
                    }
                }

                // Original quads on top
                for (int q = 0; q < origVC; q += 4)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        int src = q + j;
                        newVerts[vIdx] = meshInfo.vertices[src];
                        newUVs0[vIdx] = meshInfo.uvs0[src];
                        if (meshInfo.uvs2 != null && src < meshInfo.uvs2.Length) newUVs2[vIdx] = meshInfo.uvs2[src];
                        if (meshInfo.uvs3 != null && src < meshInfo.uvs3.Length) newUVs3[vIdx] = meshInfo.uvs3[src];
                        if (meshInfo.uvs4 != null && src < meshInfo.uvs4.Length) newUVs4[vIdx] = meshInfo.uvs4[src];
                        newColors[vIdx] = meshInfo.colors32[src];
                        if (meshInfo.tangents != null && src < meshInfo.tangents.Length) newTangents[vIdx] = meshInfo.tangents[src];
                        vIdx++;
                    }
                    int b = vIdx - 4;
                    newTris[tIdx++] = b;     newTris[tIdx++] = b + 1; newTris[tIdx++] = b + 2;
                    newTris[tIdx++] = b + 2; newTris[tIdx++] = b + 3; newTris[tIdx++] = b;
                }

                // Only modify meshInfo arrays — TMP's UpdateVertexData will push vertices/UVs/colors
                meshInfo.vertices = newVerts;
                meshInfo.uvs0 = newUVs0;
                meshInfo.uvs2 = newUVs2;
                meshInfo.uvs3 = newUVs3;
                meshInfo.uvs4 = newUVs4;
                meshInfo.colors32 = newColors;
                meshInfo.tangents = newTangents;
                meshInfo.triangles = newTris;
                meshInfo.vertexCount = newVC;

                // TMP_MeshInfo is a struct — must write back to the array
                textInfo.meshInfo[m] = meshInfo;
            }
        }

        private void ResetExpandedMesh(TMP_TextInfo textInfo)
        {
            if (textInfo == null || textInfo.meshInfo == null) return;

            for (int m = 0; m < textInfo.meshInfo.Length; m++)
            {
                var meshInfo = textInfo.meshInfo[m];
                if (meshInfo.vertices == null || meshInfo.vertexCount == 0) continue;

                int vc = meshInfo.vertexCount;

                // Check if already expanded (vertexCount is multiple of k_Samples+1)
                if (vc > 4 && vc % (k_Samples + 1) == 0)
                {
                    int possibleOrig = vc / (k_Samples + 1);
                    if (possibleOrig > 0 && possibleOrig * 4 <= textInfo.characterCount * 4 + 16)
                    {
                        // Truncate arrays back to original size
                        System.Array.Resize(ref meshInfo.vertices, possibleOrig);
                        System.Array.Resize(ref meshInfo.uvs0, possibleOrig);
                        if (meshInfo.uvs2 != null) System.Array.Resize(ref meshInfo.uvs2, possibleOrig);
                        if (meshInfo.uvs3 != null) System.Array.Resize(ref meshInfo.uvs3, possibleOrig);
                        if (meshInfo.uvs4 != null) System.Array.Resize(ref meshInfo.uvs4, possibleOrig);
                        System.Array.Resize(ref meshInfo.colors32, possibleOrig);
                        if (meshInfo.tangents != null) System.Array.Resize(ref meshInfo.tangents, possibleOrig);

                        // Reset triangles to standard TMP pattern
                        int origQuadCount = possibleOrig / 4;
                        var tris = new int[origQuadCount * 6];
                        for (int i = 0; i < origQuadCount; i++)
                        {
                            int b = i * 4;
                            int t = i * 6;
                            tris[t] = b;     tris[t+1] = b+1; tris[t+2] = b+2;
                            tris[t+3] = b+2; tris[t+4] = b+3; tris[t+5] = b;
                        }
                        meshInfo.triangles = tris;
                        meshInfo.vertexCount = possibleOrig;

                        // Don't write to mesh here — TMP will handle it after callback
                        textInfo.meshInfo[m] = meshInfo;
                    }
                }
            }
        }

        private void OnValidate()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (_mainText != null)
                _mainText.OnPreRenderText -= OnTMPPreRender;
        }
    }
}
