using System.Collections;
using UnityEngine;

namespace Balanka
{
    /// <summary>
    /// Manages rounds, shots, score accumulation and win/loss detection.
    /// State machine: ReadyToShoot → BallInFlight → Shopping → (next shot or RoundEnd)
    /// </summary>
    public class RoundManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────
        [Header("Round Settings")]
        public int StartingShots          = 5;
        public int RoundTargetScore       = 1000;
        public int TargetIncreasePerRound = 500;

        [Header("References")]
        public ShotController ShotController;
        public ShopManager    ShopManager;

        // ── Runtime State ─────────────────────────────────────────────────
        public int  Score        { get; private set; }
        public int  ShotsLeft    { get; set; }
        public int  CurrentRound { get; private set; }
        public bool IsRunning    { get; private set; }

        // ── Events ────────────────────────────────────────────────────────
        public event System.Action<int> OnScoreChanged;   // new score
        public event System.Action<int> OnShotsChanged;   // shots remaining
        public event System.Action<int> OnRoundChanged;   // current round

        // ─────────────────────────────────────────────────────────────────

        public void InitRun()
        {
            CurrentRound       = 1;
            Score              = 0;
            RoundTargetScore   = int.MaxValue; // reset from inspector default handled in StartNextRound
            IsRunning          = true;
            StartRound();
        }

        public void StartNextRound()
        {
            CurrentRound++;
            RoundTargetScore += TargetIncreasePerRound;
            StartRound();
        }

        private void StartRound()
        {
            ShotsLeft = StartingShots
                        + GameManager.Instance.UpgradeManager.BonusShots;

            GameManager.Instance.UpgradeManager.BonusShots = 0; // consume bonus

            NotifyAll();
            ReadyNextShot();
        }

        // ── Shot flow ─────────────────────────────────────────────────────

        public void ReadyNextShot()
        {
            ShotController.EnableInput(true);
        }

        /// <summary>Called by ShotController when ball(s) have been launched.</summary>
        public void OnShotFired()
        {
            ShotController.EnableInput(false);
        }

        /// <summary>Called by Ball(s) when all active balls have finished.</summary>
        public void OnAllBallsFinished(int totalShotScore)
        {
            AddScore(totalShotScore);
            ShotsLeft--;
            OnShotsChanged?.Invoke(ShotsLeft);

            if (ShotsLeft > 0)
            {
                ShopManager.OpenShop();
            }
            else
            {
                EvaluateRound();
            }
        }

        /// <summary>Called by ShopManager when player closes shop (buy or skip).</summary>
        public void OnShopClosed()
        {
            ReadyNextShot();
        }

        // ── Score ─────────────────────────────────────────────────────────

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        /// <summary>Spend score as currency. Returns false if not enough.</summary>
        public bool SpendScore(int amount)
        {
            if (Score < amount) return false;
            Score -= amount;
            OnScoreChanged?.Invoke(Score);
            return true;
        }

        // ── Evaluation ────────────────────────────────────────────────────

        private void EvaluateRound()
        {
            if (Score >= RoundTargetScore)
                GameManager.Instance.OnRoundWon();
            else
                GameManager.Instance.OnRoundLost();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void NotifyAll()
        {
            OnScoreChanged?.Invoke(Score);
            OnShotsChanged?.Invoke(ShotsLeft);
            OnRoundChanged?.Invoke(CurrentRound);
        }
    }
}
