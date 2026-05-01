using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Balanka
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BoardRuntimeBuilder : MonoBehaviour
    {
        private const string GeneratedRootName = "_GeneratedBoardShape";
        private const string ScoreZonesParentName = "ScoreZones";
        private const string GeneratedScoreSlotsName = "_GeneratedBasicPointSlots";
        private const string BoardWoodResource = "Sprites/board_wood_sprite";
        private const string BorderWoodResource = "Sprites/border_wood_sprite";

        private static readonly int[] DefaultBasicPointValues = { 5, 10, 15, 20, 15, 10, 5 };
        private static readonly int[] RecommendedTopNailRowCounts = { 1, 3, 5, 7, 9, 11, 13 };
        private static readonly int[] RecommendedMiddleNailRowCounts = { 12, 13 };
        private static readonly int[] RecommendedBottomNailRowCounts = { 12, 13, 12, 13 };

        [Header("Board Shape")]
        public float Width = 6f;
        public float StraightHeight = 7.5f;
        public float BottomY = -4.8f;
        public int ArcSegments = 24;

        [Header("Launch Lane")]
        public float LaneHalfWidth = 2.55f;
        public float LaneTopY = 2.35f;
        public float SpawnY = -4.55f;

        [Header("Visuals")]
        public float RailWidth = 0.16f;
        public float ColliderRadius = 0.06f;
        public Color RailColor = new(0.53f, 0.25f, 0.13f, 1f);
        public Color BoardColor = new(0.72f, 0.49f, 0.31f, 1f);
        public Color BoardFacetDarkColor = new(0.58f, 0.36f, 0.20f, 1f);
        public Color BoardFacetLightColor = new(0.80f, 0.58f, 0.39f, 1f);
        public Sprite BoardWoodSprite;
        public Sprite BorderWoodSprite;
        public Vector2 BoardTextureTiling = Vector2.one;
        public float BorderTextureTiling = 1.6f;

        [Header("Basic Point Slots")]
        public bool GenerateBasicPointSlots = true;
        public int[] BasicPointValues = { 5, 10, 15, 20, 15, 10, 5 };
        public float ScoreSlotHeight = 0.75f;
        public float ScoreTriggerHeight = 0.12f;
        public float ScoreSlotBottomPadding = 0.04f;
        public float ScoreDividerHeight = 0.45f;
        public float ScoreLabelYOffset = 0.22f;
        public float ScoreLabelFontSize = 3.8f;
        public Color ScoreLabelColor = new(0.23f, 0.12f, 0.06f, 1f);
        public Color ScoreLabelHighlightColor = new(0.93f, 0.69f, 0.39f, 0.55f);
        public Color ScoreLabelShadowColor = new(0.12f, 0.06f, 0.03f, 0.75f);
        public Color ScoreSlotInsetColor = new(0.18f, 0.08f, 0.03f, 0.13f);

        [Header("Nail Obstacles")]
        public bool GenerateNailObstacles = true;
        public bool UseRecommendedNailLayout = true;
        public int[] TopNailRowCounts = { 1, 3, 5, 7, 9, 11, 13 };
        public int[] MiddleNailRowCounts = { 12, 13, 12, 13, 12, 13, 12, 13 };
        public int[] BottomNailRowCounts = { 12, 13, 12, 13 };
        public float TopNailsTopY = 4.77f;
        public float TopNailsBottomY = 2.4f;
        public float BottomNailsTopY = -3.28f;
        public float BottomNailsBottomY = -4.05f;
        public float NailHorizontalSpacing = 0.37f;
        public float NailRadius = 0.075f;
        public float NailHeadRadius = 0.13f;
        public float NailRestitution = 0.72f;
        public float NailFriction = 0.02f;
        public Color NailShadowColor = new(0.10f, 0.06f, 0.03f, 0.45f);
        public Color NailRimColor = new(0.50f, 0.47f, 0.42f, 1f);
        public Color NailFaceColor = new(0.78f, 0.75f, 0.68f, 1f);
        public Color NailHighlightColor = new(1.00f, 0.96f, 0.82f, 0.90f);

#if UNITY_EDITOR
        private bool _editorRebuildQueued;
#endif

        private void OnEnable() => Rebuild();

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                QueueEditorRebuild();
                return;
            }
#endif
            Rebuild();
        }

        public void Rebuild()
        {
            if (!isActiveAndEnabled) return;

            ClearGeneratedShape();
            DisablePlaceholderBorders();

            var root = new GameObject(GeneratedRootName).transform;
            root.SetParent(transform, false);

            LoadSpriteTextures();
            CreateBackdrop(root);
            CreateRail(root, "Outer U rail", BuildOuterUPoints(), true);
            CreateRail(root, "Left launch lane rail", new[] { new Vector2(-LaneHalfWidth, BottomY), new Vector2(-LaneHalfWidth, LaneTopY) }, true);
            CreateRail(root, "Right launch lane rail", new[] { new Vector2(LaneHalfWidth, BottomY), new Vector2(LaneHalfWidth, LaneTopY) }, true);
            CreateRail(root, "Bottom visual rail", new[] { new Vector2(-Width * 0.5f, BottomY), new Vector2(Width * 0.5f, BottomY) }, true);
            CreateNailObstacles(root);
            CreateBasicPointSlots();
            PositionSpawnPoint();
        }

        private Vector2[] BuildOuterUPoints()
        {
            float radius = Width * 0.5f;
            float sideTopY = BottomY + StraightHeight;
            var points = new List<Vector2> { new(-radius, BottomY), new(-radius, sideTopY) };

            for (int i = 1; i < ArcSegments; i++)
            {
                float t = i / (float)ArcSegments;
                float angle = Mathf.Lerp(180f, 0f, t) * Mathf.Deg2Rad;
                points.Add(new Vector2(Mathf.Cos(angle) * radius, sideTopY + Mathf.Sin(angle) * radius));
            }

            points.Add(new Vector2(radius, sideTopY));
            points.Add(new Vector2(radius, BottomY));
            return points.ToArray();
        }

        private void ClearGeneratedShape()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name == GeneratedRootName)
                {
                    ClearEditorSelectionIfInside(child);
                    DestroyGenerated(child);
                }
            }

            Transform scoreZones = transform.Find(ScoreZonesParentName);
            if (scoreZones)
            {
                ClearEditorSelectionIfInside(scoreZones.Find(GeneratedScoreSlotsName));
                DestroyGenerated(scoreZones.Find(GeneratedScoreSlotsName));
            }
        }

        private void DisablePlaceholderBorders()
        {
            var names = new HashSet<string>
            {
                "LaneLeftWall",
                "LaneRightWall",
                "LeftWall",
                "RightOuter",
                "TopArcLeft",
                "TopArcRight",
                "BottomWall",
            };

            foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
            {
                if (names.Contains(collider.gameObject.name))
                    collider.enabled = false;
            }

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (names.Contains(renderer.gameObject.name))
                    renderer.enabled = false;
            }
        }

        private void CreateRail(Transform parent, string name, IReadOnlyList<Vector2> points, bool hasCollider)
        {
            var rail = new GameObject(name);
            rail.transform.SetParent(parent, false);

            if (hasCollider)
            {
                var edge = rail.AddComponent<EdgeCollider2D>();
                edge.points = ToArray(points);
                edge.edgeRadius = ColliderRadius;
            }

            var line = rail.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = points.Count;
            line.widthMultiplier = RailWidth;
            line.numCapVertices = 3;
            line.numCornerVertices = 3;
            line.textureMode = LineTextureMode.Tile;
            line.material = CreateSpriteMaterial(BorderWoodSprite, RailColor, new Vector2(BorderTextureTiling, 1f), true);
            line.startColor = Color.white;
            line.endColor = Color.white;
            line.sortingOrder = 20;

            for (int i = 0; i < points.Count; i++)
                line.SetPosition(i, points[i]);
        }

        private void CreateBackdrop(Transform parent)
        {
            var backdrop = new GameObject("Board surface");
            backdrop.transform.SetParent(parent, false);
            backdrop.transform.localPosition = new Vector3(0f, 0f, 0.2f);

            var meshFilter = backdrop.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateLowPolyBoardMesh();

            var renderer = backdrop.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateSpriteMaterial(BoardWoodSprite, BoardColor, Vector2.one, false);
            renderer.sortingOrder = -2;
        }

        private void LoadSpriteTextures()
        {
            if (!BoardWoodSprite)
                BoardWoodSprite = Resources.Load<Sprite>(BoardWoodResource);

            if (!BorderWoodSprite)
                BorderWoodSprite = Resources.Load<Sprite>(BorderWoodResource);
        }

        private static Material CreateSpriteMaterial(Sprite sprite, Color fallbackColor, Vector2 textureScale, bool repeat)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            Texture2D texture = sprite ? sprite.texture : null;
            material.color = texture ? Color.white : fallbackColor;
            material.mainTextureScale = textureScale;

            if (texture)
            {
                texture.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
                material.mainTexture = texture;
            }

            return material;
        }

        private Mesh CreateLowPolyBoardMesh()
        {
            Vector2[] outline = BuildOuterUPoints();
            var vertices = new Vector3[outline.Length + 1];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[outline.Length * 3];
            float sideTopY = BottomY + StraightHeight;
            float topY = sideTopY + Width * 0.5f;

            vertices[0] = new Vector3(0f, sideTopY * 0.38f + BottomY * 0.62f, 0f);
            uvs[0] = BoardUv(vertices[0], topY);

            for (int i = 0; i < outline.Length; i++)
            {
                vertices[i + 1] = outline[i];
                uvs[i + 1] = BoardUv(outline[i], topY);

                int triangleIndex = i * 3;
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i == outline.Length - 1 ? 1 : i + 2;
            }

            var mesh = new Mesh { name = "Low Poly Wood Board Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
            return mesh;
        }

        private Vector2 BoardUv(Vector3 position, float topY)
        {
            float u = Mathf.InverseLerp(-Width * 0.5f, Width * 0.5f, position.x) * BoardTextureTiling.x;
            float v = Mathf.InverseLerp(BottomY, topY, position.y) * BoardTextureTiling.y;
            return new Vector2(u, v);
        }

        private void CreateBasicPointSlots()
        {
            if (!GenerateBasicPointSlots) return;

            int[] values = BasicPointValues != null && BasicPointValues.Length > 0
                ? BasicPointValues
                : DefaultBasicPointValues;
            Transform scoreZones = GetOrCreateChild(ScoreZonesParentName, transform);
            var root = new GameObject(GeneratedScoreSlotsName).transform;
            root.SetParent(scoreZones, false);

            float leftX = -LaneHalfWidth;
            float slotWidth = (LaneHalfWidth * 2f) / values.Length;
            float slotBottomY = BottomY + ScoreSlotBottomPadding;
            float slotCenterY = slotBottomY + ScoreSlotHeight * 0.5f;

            for (int i = 0; i < values.Length; i++)
            {
                float slotCenterX = leftX + slotWidth * (i + 0.5f);
                CreatePointSlot(root, i, values[i], slotCenterX, slotCenterY, slotWidth);
            }

            for (int i = 1; i < values.Length; i++)
            {
                float x = leftX + slotWidth * i;
                CreateRail(root, $"Score divider {i}", new[] { new Vector2(x, BottomY), new Vector2(x, BottomY + ScoreDividerHeight) }, true);
            }
        }

        private void CreatePointSlot(Transform parent, int index, int value, float x, float y, float width)
        {
            var slot = new GameObject($"PointSlot_{index + 1}_{value}");
            slot.transform.SetParent(parent, false);
            slot.transform.localPosition = new Vector3(x, y, 0f);

            var collider = slot.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            float triggerHeight = Mathf.Clamp(ScoreTriggerHeight, 0.04f, ScoreSlotHeight);
            collider.size = new Vector2(width, triggerHeight);
            collider.offset = new Vector2(0f, -ScoreSlotHeight * 0.5f + triggerHeight * 0.5f);

            var sprite = slot.AddComponent<SpriteRenderer>();
            sprite.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            sprite.color = ScoreSlotInsetColor;
            sprite.sortingOrder = -2;

            var zone = slot.AddComponent<ScoreZone>();
            zone.ZoneId = $"B{index + 1:00}";
            zone.ZoneName = $"Basic {value}";
            zone.BaseScore = value;
            zone.ZoneSprite = sprite;
            zone.NormalColor = ScoreSlotInsetColor;

            var label = new GameObject("PointLabel");
            label.transform.SetParent(slot.transform, false);
            float labelYOffset = Mathf.Clamp(ScoreLabelYOffset, 0.16f, 0.32f);
            label.transform.localPosition = new Vector3(0f, -ScoreSlotHeight * 0.5f + labelYOffset, -0.05f);

            float labelFontSize = Mathf.Max(ScoreLabelFontSize, 3.8f);
            Vector2 labelSize = new(width * 3f, labelFontSize * 0.45f);
            CreateScoreLabelLayer(label.transform, "EngraveHighlight", value.ToString(), labelFontSize, labelSize, ScoreLabelHighlightColor, new Vector3(-0.012f, 0.016f, 0.01f), -1);
            CreateScoreLabelLayer(label.transform, "EngraveShadow", value.ToString(), labelFontSize, labelSize, ScoreLabelShadowColor, new Vector3(0.018f, -0.018f, 0.02f), -1);
            var text = CreateScoreLabelLayer(label.transform, "EngraveFace", value.ToString(), labelFontSize, labelSize, ScoreLabelColor, Vector3.zero, -1);

            zone.ValueLabel = text;
        }

        private void CreateNailObstacles(Transform parent)
        {
            if (!GenerateNailObstacles) return;

            int[] topRows = UseRecommendedNailLayout ? RecommendedTopNailRowCounts : TopNailRowCounts;
            int[] middleRows = UseRecommendedNailLayout ? RecommendedMiddleNailRowCounts : MiddleNailRowCounts;
            float nailRadius = UseRecommendedNailLayout ? 0.05f : Mathf.Max(0.001f, NailRadius);
            float nailHeadRadius = UseRecommendedNailLayout ? 0.05f : Mathf.Max(nailRadius, NailHeadRadius);
            float spacing = Mathf.Max(nailHeadRadius * 2.1f, UseRecommendedNailLayout ? 0.37f : NailHorizontalSpacing);
            float maxHalfWidth = Mathf.Max(nailHeadRadius, LaneHalfWidth - ColliderRadius - nailRadius * 0.6f);
            float topY = UseRecommendedNailLayout ? 4.6f : TopNailsTopY;
            float bottomY = UseRecommendedNailLayout ? -4.05f : BottomNailsBottomY;
            int[] fieldRows = BuildContinuousNailRows(topRows, middleRows, topY, bottomY, spacing);
            float fieldBottomY = topY - spacing * (fieldRows.Length - 1);

            var root = new GameObject("Nail obstacles");
            root.transform.SetParent(parent, false);

            PhysicsMaterial2D nailMaterial = CreateNailPhysicsMaterial();
            CreateNailCluster(root.transform, "Continuous", fieldRows, topY, fieldBottomY, spacing, maxHalfWidth, nailRadius, nailHeadRadius, nailMaterial);
        }

        private static int[] BuildContinuousNailRows(int[] topRows, int[] repeatingRows, float topY, float bottomY, float rowSpacing)
        {
            int topRowCount = topRows != null ? topRows.Length : 0;
            int repeatingRowCount = repeatingRows != null ? repeatingRows.Length : 0;
            int totalRows = Mathf.Max(topRowCount, Mathf.FloorToInt(Mathf.Abs(topY - bottomY) / Mathf.Max(0.01f, rowSpacing)) + 1);
            var rows = new int[totalRows];

            for (int i = 0; i < totalRows; i++)
            {
                if (i < topRowCount)
                {
                    rows[i] = Mathf.Max(1, topRows[i]);
                    continue;
                }

                rows[i] = repeatingRowCount > 0
                    ? Mathf.Max(1, repeatingRows[(i - topRowCount) % repeatingRowCount])
                    : i % 2 == 0 ? 13 : 12;
            }

            return rows;
        }

        private void CreateNailCluster(
            Transform parent,
            string clusterName,
            int[] rowCounts,
            float topY,
            float bottomY,
            float spacing,
            float maxHalfWidth,
            float nailRadius,
            float nailHeadRadius,
            PhysicsMaterial2D material)
        {
            if (rowCounts == null || rowCounts.Length == 0) return;

            var cluster = new GameObject($"{clusterName} nail cluster");
            cluster.transform.SetParent(parent, false);

            int rows = rowCounts.Length;
            for (int row = 0; row < rows; row++)
            {
                int rowColumns = Mathf.Max(1, rowCounts[row]);
                float y = rows == 1 ? Mathf.Lerp(bottomY, topY, 0.5f) : Mathf.Lerp(topY, bottomY, row / (float)(rows - 1));
                float rowHalfWidth = GetNailRowHalfWidth(rowColumns, spacing, maxHalfWidth, nailRadius);
                CreateNailRow(cluster.transform, row, rowColumns, y, rowHalfWidth, nailRadius, nailHeadRadius, material);
            }
        }

        private static float GetNailRowHalfWidth(int rowColumns, float spacing, float maxHalfWidth, float nailRadius)
        {
            if (rowColumns >= 13)
                return maxHalfWidth;

            if (rowColumns == 12)
                return Mathf.Max(0f, maxHalfWidth - spacing * 0.5f);

            return Mathf.Min(((rowColumns - 1) * spacing) * 0.5f, maxHalfWidth);
        }

        private void CreateNailRow(
            Transform parent,
            int row,
            int rowColumns,
            float y,
            float rowHalfWidth,
            float nailRadius,
            float nailHeadRadius,
            PhysicsMaterial2D material)
        {
            for (int column = 0; column < rowColumns; column++)
            {
                float x = rowColumns == 1 ? 0f : Mathf.Lerp(-rowHalfWidth, rowHalfWidth, column / (float)(rowColumns - 1));
                CreateNail(parent, row, column, new Vector2(x, y), nailRadius, nailHeadRadius, material);
            }
        }

        private void CreateNail(Transform parent, int row, int column, Vector2 position, float colliderRadius, float headRadius, PhysicsMaterial2D material)
        {
            var nail = new GameObject($"Nail_{row + 1:00}_{column + 1:00}");
            nail.transform.SetParent(parent, false);
            nail.transform.localPosition = new Vector3(position.x, position.y, -0.04f);

            var collider = nail.AddComponent<CircleCollider2D>();
            collider.radius = colliderRadius;
            collider.sharedMaterial = material;

            CreateDisc(nail.transform, "Shadow", headRadius * 1.08f, NailShadowColor, new Vector3(headRadius * 0.42f, -headRadius * 0.42f, 0.03f), 0);
            CreateDisc(nail.transform, "Rim", headRadius, NailRimColor, Vector3.zero, 1);
            CreateDisc(nail.transform, "Face", headRadius * 0.76f, NailFaceColor, new Vector3(-0.006f, 0.006f, -0.01f), 2);
            CreateDisc(nail.transform, "Highlight", headRadius * 0.28f, NailHighlightColor, new Vector3(-headRadius * 0.24f, headRadius * 0.28f, -0.02f), 3);
        }

        private PhysicsMaterial2D CreateNailPhysicsMaterial()
        {
            return new PhysicsMaterial2D("Nail obstacle material")
            {
                friction = Mathf.Max(0f, NailFriction),
                bounciness = Mathf.Clamp01(NailRestitution)
            };
        }

        private void CreateDisc(Transform parent, string name, float radius, Color color, Vector3 localOffset, int sortingOrder)
        {
            var disc = new GameObject(name);
            disc.transform.SetParent(parent, false);
            disc.transform.localPosition = localOffset;

            var meshFilter = disc.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateDiscMesh(radius, 18);

            var renderer = disc.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateColorMaterial(color);
            renderer.sortingOrder = sortingOrder;
        }

        private static Material CreateColorMaterial(Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }

        private static Mesh CreateDiscMesh(float radius, int segments)
        {
            segments = Mathf.Max(8, segments);
            var vertices = new Vector3[segments + 1];
            var triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;

            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

                int triangleIndex = i * 3;
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i == segments - 1 ? 1 : i + 2;
            }

            var mesh = new Mesh { name = "Disc Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static TextMeshPro CreateScoreLabelLayer(Transform parent, string name, string value, float fontSize, Vector2 size, Color color, Vector3 offset, int sortingOrder)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(parent, false);
            layer.transform.localPosition = offset;

            var text = layer.AddComponent<TextMeshPro>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.CenterGeoAligned;
            text.color = color;
            text.sortingOrder = sortingOrder;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.rectTransform.sizeDelta = size;
            return text;
        }

        private static Transform GetOrCreateChild(string name, Transform parent)
        {
            Transform child = parent.Find(name);
            if (child) return child;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void DestroyGenerated(Transform existing)
        {
            if (!existing) return;

            if (Application.isPlaying)
                Destroy(existing.gameObject);
            else
                DestroyImmediate(existing.gameObject);
        }

        private void QueueEditorRebuild()
        {
#if UNITY_EDITOR
            if (_editorRebuildQueued) return;

            _editorRebuildQueued = true;
            EditorApplication.delayCall += () =>
            {
                _editorRebuildQueued = false;
                if (!this || !isActiveAndEnabled || Application.isPlaying) return;
                Rebuild();
            };
#endif
        }

        private static void ClearEditorSelectionIfInside(Transform root)
        {
#if UNITY_EDITOR
            if (!root) return;

            foreach (Object selected in Selection.objects)
            {
                if (selected is GameObject gameObject && gameObject.transform.IsChildOf(root))
                {
                    Selection.activeObject = root.parent ? root.parent.gameObject : null;
                    return;
                }
            }
#endif
        }

        private void PositionSpawnPoint()
        {
            Transform spawnPoint = transform.Find("BallSpawnPoint");
            if (spawnPoint)
            {
                float outerRightX = Width * 0.5f;
                float rightLaneRailX = LaneHalfWidth;
                float rightCenterX = (outerRightX + rightLaneRailX) * 0.5f;
                spawnPoint.localPosition = new Vector3(rightCenterX, SpawnY, 0f);
            }
        }

        private static Vector2[] ToArray(IReadOnlyList<Vector2> points)
        {
            var result = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
                result[i] = points[i];
            return result;
        }
    }
}
