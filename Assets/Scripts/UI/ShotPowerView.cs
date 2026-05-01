using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Balanka
{
    /// <summary>
    /// Displays the current shot charge as a slider fill and optional label.
    /// </summary>
    public class ShotPowerView : MonoBehaviour
    {
        [Header("UI Elements")]
        public Slider          PowerSlider;
        public TextMeshProUGUI PowerLabel;   // optional: "47 %"
        public bool            ShowPercentLabel;
        public Vector2         BarSize = new(34f, 240f);

        [Header("Colors")]
        public Color LowColor  = Color.green;
        public Color HighColor = Color.red;
        public Color TickColor = new(1f, 1f, 1f, 0.75f);

        private Image _fillImage;
        private RectTransform _fillTransform;

        private void Awake()
        {
            if (PowerSlider && PowerSlider.fillRect)
            {
                _fillImage = PowerSlider.fillRect.GetComponent<Image>();
                _fillTransform = PowerSlider.fillRect;
            }
            else
            {
                BuildFallbackBar();
            }

            if (PowerLabel)
                PowerLabel.gameObject.SetActive(ShowPercentLabel);
        }

        /// <param name="power">0..1</param>
        public void SetPower(float power)
        {
            power = Mathf.Clamp01(power);

            if (PowerSlider) PowerSlider.value = power;
            if (_fillImage)  _fillImage.color  = Color.Lerp(LowColor, HighColor, power);
            if (_fillTransform) _fillTransform.anchorMax = new Vector2(1f, power);
            if (PowerLabel && ShowPercentLabel) PowerLabel.text = $"{Mathf.RoundToInt(power * 100)} %";
        }

        private void BuildFallbackBar()
        {
            RectTransform root = PowerSlider
                ? PowerSlider.GetComponent<RectTransform>()
                : GetComponent<RectTransform>();

            if (!root) return;

            root.sizeDelta = BarSize;

            Image background = root.gameObject.GetComponent<Image>();
            if (!background)
                background = root.gameObject.AddComponent<Image>();

            background.color = new Color(0f, 0f, 0f, 0.55f);

            GameObject fillObject = new GameObject("PowerFill", typeof(RectTransform), typeof(Image));
            RectTransform fill = fillObject.GetComponent<RectTransform>();
            fill.SetParent(root, false);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(1f, 0f);
            fill.offsetMin = new Vector2(2f, 2f);
            fill.offsetMax = new Vector2(-2f, -2f);

            _fillTransform = fill;
            _fillImage = fillObject.GetComponent<Image>();

            if (PowerSlider)
                PowerSlider.fillRect = fill;

            CreateQuarterTicks(root);
        }

        private void CreateQuarterTicks(RectTransform root)
        {
            for (int i = 1; i < 4; i++)
            {
                GameObject tickObject = new GameObject($"QuarterTick{i}", typeof(RectTransform), typeof(Image));
                RectTransform tick = tickObject.GetComponent<RectTransform>();
                tick.SetParent(root, false);

                float y = i * 0.25f;
                tick.anchorMin = new Vector2(0f, y);
                tick.anchorMax = new Vector2(1f, y);
                tick.pivot = new Vector2(0.5f, 0.5f);
                tick.sizeDelta = new Vector2(0f, 2f);
                tick.anchoredPosition = Vector2.zero;

                Image image = tickObject.GetComponent<Image>();
                image.color = TickColor;
            }
        }
    }
}
