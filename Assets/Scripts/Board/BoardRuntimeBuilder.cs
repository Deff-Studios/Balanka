using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

        private void OnEnable() => Rebuild();
        private void OnValidate() => Rebuild();

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
                    DestroyGenerated(child);
            }

            Transform scoreZones = transform.Find(ScoreZonesParentName);
            if (scoreZones)
                DestroyGenerated(scoreZones.Find(GeneratedScoreSlotsName));
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
            line.sortingOrder = 3;

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
