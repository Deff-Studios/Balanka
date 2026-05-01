using System.Collections.Generic;
using UnityEngine;

namespace Balanka
{
    /// <summary>
    /// Handles player input for charging shot power and spawning the ball(s).
    /// Hold Space (or left mouse button) → power charges up.
    /// Release → ball(s) launched along the launch lane.
    /// </summary>
    public class ShotController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────
        [Header("Launch Settings")]
        public float MaxForce        = 18f;
        public float ChargeSpeed     = 0.6f;  // fraction of MaxForce per second
        public float MinChargeToFire = 0.05f;
        /// <summary>Direction the ball travels when leaving the launch lane.</summary>
        public Vector2 LaunchDirection = Vector2.up;

        [Header("References")]
        public Transform  BallSpawnPoint;
        public GameObject BallPrefab;

        [Header("UI")]
        public ShotPowerView ShotPowerView;

        // ── Runtime ───────────────────────────────────────────────────────
        private float _charge;          // 0 .. 1
        private bool  _charging;
        private bool  _inputEnabled;

        // Tracks active balls for this shot so we know when all are done
        private readonly List<Ball> _activeBalls = new();
        private int _pendingBalls;
        private int _accumulatedShotScore;
        private GameObject _readyBallPreview;

        // ─────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (BallPrefab != null && BallPrefab.scene.IsValid())
            {
                _readyBallPreview = BallPrefab;
            }
        }

        private void Update()
        {
            if (!_inputEnabled) return;

            bool holdInput = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

            if (holdInput && !_charging)
            {
                _charging = true;
                _charge   = 0f;
            }

            if (_charging && holdInput)
            {
                _charge = Mathf.MoveTowards(_charge, 1f, ChargeSpeed * Time.deltaTime);
                ShotPowerView.SetPower(_charge);
            }

            if (_charging && !holdInput)
            {
                Fire(_charge);
                _charge   = 0f;
                _charging = false;
                ShotPowerView.SetPower(0f);
            }
        }

        // ── Public API ────────────────────────────────────────────────────

        public void EnableInput(bool enabled)
        {
            _inputEnabled = enabled;
            if (enabled)
            {
                ShowReadyBall();
            }
            else
            {
                _charging = false;
                _charge   = 0f;
                ShotPowerView.SetPower(0f);
            }
        }

        // ── Internal ──────────────────────────────────────────────────────

        private void Fire(float charge)
        {
            if (charge < MinChargeToFire) return;
            if (BallPrefab == null) return;

            int ballCount = GameManager.Instance.UpgradeManager.ConsumePendingDoubleBall() ? 2 : 1;

            _pendingBalls        = ballCount;
            _accumulatedShotScore = 0;
            _activeBalls.Clear();

            Vector3 baseSpawnPos = GetSpawnPosition();

            for (int i = 0; i < ballCount; i++)
            {
                // slight horizontal offset for double-ball so they don't perfectly overlap
                Vector3 spawnPos = baseSpawnPos + new Vector3(i * 0.15f, 0f, 0f);
                GameObject go    = Instantiate(BallPrefab, spawnPos, Quaternion.identity);
                go.SetActive(true);
                Ball ball        = go.GetComponent<Ball>();

                ball.OnBallFinished += HandleBallFinished;
                _activeBalls.Add(ball);

                Vector2 force = LaunchDirection.normalized * (charge * MaxForce);
                ball.Launch(force);
            }

            HideReadyBall();
            GameManager.Instance.AudioManager.PlayShot();
            GameManager.Instance.RoundManager.OnShotFired();
        }

        private void HandleBallFinished(Ball ball, int score)
        {
            ball.OnBallFinished -= HandleBallFinished;
            _accumulatedShotScore += score;
            _pendingBalls--;

            if (_pendingBalls <= 0)
            {
                // All balls done → inform RoundManager
                GameManager.Instance.RoundManager.OnAllBallsFinished(_accumulatedShotScore);
            }
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 spawnPos = BallSpawnPoint.position;
            Camera mainCamera = Camera.main;

            if (!mainCamera) return spawnPos;

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(spawnPos);
            bool isInCameraView = viewportPos.z > 0f
                && viewportPos.x >= 0f && viewportPos.x <= 1f
                && viewportPos.y >= 0f && viewportPos.y <= 1f;

            if (isInCameraView) return spawnPos;

            float cameraDistance = Mathf.Abs(mainCamera.transform.position.z - spawnPos.z);
            Vector3 fallback = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.15f, cameraDistance));
            fallback.z = spawnPos.z;
            return fallback;
        }

        private void ShowReadyBall()
        {
            if (BallPrefab == null) return;

            if (_readyBallPreview == null)
            {
                _readyBallPreview = Instantiate(BallPrefab);
            }

            _readyBallPreview.transform.position = GetSpawnPosition();
            _readyBallPreview.transform.rotation = Quaternion.identity;
            ResetReadyBallPhysics(_readyBallPreview);
            _readyBallPreview.SetActive(true);
        }

        private void HideReadyBall()
        {
            if (_readyBallPreview != null)
                _readyBallPreview.SetActive(false);
        }

        private static void ResetReadyBallPhysics(GameObject ballObject)
        {
            if (!ballObject.TryGetComponent<Rigidbody2D>(out var rb)) return;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
