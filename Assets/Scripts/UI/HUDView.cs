using UnityEngine;
using TMPro;

namespace Balanka
{
    /// <summary>
    /// Reads events from RoundManager and updates the on-screen HUD labels.
    /// </summary>
    public class HUDView : MonoBehaviour
    {
        [Header("Labels")]
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI TargetText;
        public TextMeshProUGUI ShotsText;
        public TextMeshProUGUI RoundText;

        private RoundManager _rm;

        private void Start()
        {
            _rm = GameManager.Instance.RoundManager;
            _rm.OnScoreChanged += OnScore;
            _rm.OnShotsChanged += OnShots;
            _rm.OnRoundChanged += OnRound;
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_rm == null) return;
            _rm.OnScoreChanged -= OnScore;
            _rm.OnShotsChanged -= OnShots;
            _rm.OnRoundChanged -= OnRound;
        }

        private void OnScore(int v) => SetText(ScoreText,  $"Score: {v} / {_rm.RoundTargetScore}");
        private void OnShots(int v) => SetText(ShotsText,  $"Shots: {v}");
        private void OnRound(int v) => SetText(RoundText,  $"Round: {v}");

        private void RefreshAll()
        {
            OnScore(_rm.Score);
            OnShots(_rm.ShotsLeft);
            OnRound(_rm.CurrentRound);
            SetText(TargetText, $"Target: {_rm.RoundTargetScore}");
        }

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label) label.text = text;
        }
    }
}
