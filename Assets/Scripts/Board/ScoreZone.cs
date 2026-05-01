using UnityEngine;
using TMPro;

namespace Balanka
{
    /// <summary>
    /// A trigger area on the board that awards points when a ball enters.
    /// Supports a multiplier (used by the DoubleRandomZone upgrade).
    /// </summary>
    public class ScoreZone : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────
        [Header("Identity")]
        public string ZoneId   = "Z01";
        public string ZoneName = "Low Left";

        [Header("Scoring")]
        public int   BaseScore   = 50;
        public float Multiplier  = 1f;

        [Header("References – optional")]
        public TMP_Text ValueLabel;
        public SpriteRenderer ZoneSprite;

        [Header("Feedback")]
        public Color NormalColor   = new Color(0.2f, 0.6f, 1f, 0.6f);
        public Color HitColor      = new Color(1f, 0.9f, 0.2f, 0.9f);
        public Color DoubledColor  = new Color(0.2f, 1f, 0.5f, 0.8f);

        // ─────────────────────────────────────────────────────────────────

        private void Start() => RefreshLabel();

        // ── Public API ────────────────────────────────────────────────────

        public int GetScore() => Mathf.RoundToInt(BaseScore * Multiplier);

        /// <summary>Apply a multiplier (e.g. x2 from DoubleRandomZone upgrade).</summary>
        public void ApplyMultiplier(float factor)
        {
            Multiplier *= factor;
            RefreshLabel();
            if (ZoneSprite) ZoneSprite.color = DoubledColor;
        }

        public void ResetMultiplier()
        {
            Multiplier = 1f;
            RefreshLabel();
            if (ZoneSprite) ZoneSprite.color = NormalColor;
        }

        public void PlayHitFeedback()
        {
            if (ZoneSprite)
                StartCoroutine(FlashRoutine());
        }

        // ── Internal ──────────────────────────────────────────────────────

        private void RefreshLabel()
        {
            if (ValueLabel)
            {
                ValueLabel.text = GetScore().ToString();
                foreach (TMP_Text labelLayer in ValueLabel.transform.parent.GetComponentsInChildren<TMP_Text>(true))
                    labelLayer.text = ValueLabel.text;
            }
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            if (!ZoneSprite) yield break;
            Color original = ZoneSprite.color;
            ZoneSprite.color = HitColor;
            yield return new UnityEngine.WaitForSeconds(0.25f);
            ZoneSprite.color = original;
        }
    }
}
