using UnityEngine;

namespace Balanka
{
    /// <summary>
    /// Trigger at the bottom of the board. Ball that reaches here without
    /// hitting a ScoreZone receives a low consolation score.
    /// </summary>
    public class BallExitZone : MonoBehaviour
    {
        [Header("Scoring")]
        public int ExitScore = 25;
    }
}
