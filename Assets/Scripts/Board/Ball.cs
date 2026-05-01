using System.Collections;
using UnityEngine;

namespace Balanka
{
    /// <summary>
    /// Represents a single ball in flight.
    /// • Detects entry into ScoreZone / BallExitZone triggers.
    /// • Guards against double-scoring (only first trigger counts).
    /// • Detects when velocity drops below threshold and signals completion.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Ball : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────
        [Header("Stop Detection")]
        public float StopVelocityThreshold = 0.3f;
        public float StopConfirmDelay      = 0.2f;   // seconds below threshold before "stopped"
        public float MaxLifetime           = 12f;    // safety net

        [Header("Board Tilt")]
        public float BoardTiltDegrees       = 35f;
        public float GravityAcceleration    = 9.81f;

        // ── Events ────────────────────────────────────────────────────────
        /// <summary>Fired once when the ball has fully resolved. Carries earned score.</summary>
        public event System.Action<Ball, int> OnBallFinished;

        // ── Runtime ───────────────────────────────────────────────────────
        private Rigidbody2D _rb;
        private bool  _hasScored;
        private int   _earnedScore;
        private float _slowTimer;
        private bool  _finished;
        private bool  _hasLaunched;

        // ─────────────────────────────────────────────────────────────────

        private void Awake() => _rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            if (_finished || !_hasLaunched) return;

            if (_rb.linearVelocity.magnitude < StopVelocityThreshold)
            {
                _slowTimer += Time.deltaTime;
                if (_slowTimer >= StopConfirmDelay)
                    Finish();
            }
            else
            {
                _slowTimer = 0f;
            }
        }

        private void FixedUpdate()
        {
            if (_finished || !_hasLaunched) return;

            float downhillAcceleration = GravityAcceleration * Mathf.Sin(BoardTiltDegrees * Mathf.Deg2Rad);
            _rb.AddForce(Vector2.down * downhillAcceleration, ForceMode2D.Force);
        }

        // ── Public API ────────────────────────────────────────────────────

        public void Launch(Vector2 force)
        {
            _hasLaunched = true;
            _finished    = false;
            _hasScored   = false;
            _earnedScore = 0;
            _slowTimer   = 0f;
            StartCoroutine(LifetimeSafeguard());
            _rb.AddForce(force, ForceMode2D.Impulse);
        }

        // ── Trigger handling ──────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_finished) return;

            if (other.TryGetComponent<BallExitZone>(out var exit))
            {
                if (!_hasScored)
                    RegisterScore(exit.ExitScore);

                Finish();
                return;
            }

            if (_hasScored) return;

            if (other.TryGetComponent<ScoreZone>(out var zone))
            {
                RegisterScore(zone.GetScore());
                GameManager.Instance.AudioManager.PlayZoneHit();
                zone.PlayHitFeedback();
            }
        }

        private void RegisterScore(int score)
        {
            if (_hasScored) return;
            _hasScored   = true;
            _earnedScore = score;
            // Slow the ball down so it settles quickly after scoring
            _rb.linearVelocity *= 0.3f;
        }

        // ── Stop / Finish ─────────────────────────────────────────────────

        private void Finish()
        {
            if (_finished) return;
            _finished = true;
            OnBallFinished?.Invoke(this, _earnedScore);
            Destroy(gameObject, 0.05f);
        }

        private IEnumerator LifetimeSafeguard()
        {
            yield return new WaitForSeconds(MaxLifetime);
            Finish();
        }
    }
}
