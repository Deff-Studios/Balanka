using UnityEngine;

namespace Balanka
{
    /// <summary>
    /// Central singleton that holds references to all managers and drives
    /// top-level game-flow events (start run, restart, round won/lost).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Manager references (assign in Inspector) ──────────────────────
        [Header("Managers")]
        public RoundManager   RoundManager;
        public ShopManager    ShopManager;
        public UpgradeManager UpgradeManager;
        public AudioManager   AudioManager;

        // ── UI references ─────────────────────────────────────────────────
        [Header("UI")]
        public HUDView    HUDView;
        public ResultView ResultView;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            BoardRuntimeBuilder board = FindAnyObjectByType<BoardRuntimeBuilder>();
            if (board != null)
                board.Rebuild();

            StartRun();
        }

        // ── Public API ────────────────────────────────────────────────────

        public void StartRun()
        {
            ResultView.Hide();
            UpgradeManager.ResetRun();
            RoundManager.InitRun();
        }

        public void RestartRun() => StartRun();

        /// <summary>Called by RoundManager when the round target was reached.</summary>
        public void OnRoundWon()
        {
            AudioManager.PlayRoundWon();
            ResultView.ShowWin(RoundManager.Score, RoundManager.CurrentRound);
        }

        /// <summary>Called by RoundManager when shots ran out without reaching target.</summary>
        public void OnRoundLost()
        {
            AudioManager.PlayRoundLost();
            ResultView.ShowLoss(RoundManager.Score, RoundManager.RoundTargetScore);
        }

        /// <summary>Called by ResultView "Continue" button after a win.</summary>
        public void ContinueToNextRound()
        {
            ResultView.Hide();
            RoundManager.StartNextRound();
        }
    }
}
