using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Balanka
{
    /// <summary>
    /// Tracks and applies upgrade effects.
    /// Stateless effects (ExtraShot, DoubleBall) are stored as flags/counters
    /// here so ShotController / RoundManager can query them.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Board reference for DoubleRandomZone")]
        public Transform ScoreZonesParent;   // parent of all ScoreZone children

        // ── Persistent flags (survive multiple shots within a run) ─────────
        public int  BonusShots            { get; set; }   // consumed by RoundManager at round start
        private bool _doubleBallNextShot;

        // ── History for stacking / UI later ──────────────────────────────
        private readonly List<UpgradeType> _purchasedHistory = new();

        // ── Public API ────────────────────────────────────────────────────

        public void ResetRun()
        {
            BonusShots         = 0;
            _doubleBallNextShot = false;
            _purchasedHistory.Clear();
            ResetAllZoneMultipliers();
        }

        public void ApplyUpgrade(UpgradeDefinition def)
        {
            _purchasedHistory.Add(def.Type);

            switch (def.Type)
            {
                case UpgradeType.ExtraShot:
                    // Add a shot immediately to the current round
                    GameManager.Instance.RoundManager.ShotsLeft++;
                    // Also fire the event so HUD updates
                    break;

                case UpgradeType.DoubleBallNextShot:
                    _doubleBallNextShot = true;
                    break;

                case UpgradeType.DoubleRandomZone:
                    DoubleRandomZone();
                    break;
            }
        }

        /// <summary>
        /// ShotController calls this before spawning balls.
        /// Returns true (and resets the flag) if next shot should use 2 balls.
        /// </summary>
        public bool ConsumePendingDoubleBall()
        {
            if (!_doubleBallNextShot) return false;
            _doubleBallNextShot = false;
            return true;
        }

        // ── Internal helpers ──────────────────────────────────────────────

        private void DoubleRandomZone()
        {
            if (!ScoreZonesParent) return;

            ScoreZone[] zones = ScoreZonesParent.GetComponentsInChildren<ScoreZone>();
            if (zones.Length == 0) return;

            ScoreZone target = zones[Random.Range(0, zones.Length)];
            target.ApplyMultiplier(2f);
        }

        private void ResetAllZoneMultipliers()
        {
            if (!ScoreZonesParent) return;
            foreach (var z in ScoreZonesParent.GetComponentsInChildren<ScoreZone>())
                z.ResetMultiplier();
        }
    }
}
