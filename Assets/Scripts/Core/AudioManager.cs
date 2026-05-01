using UnityEngine;

namespace Balanka
{
    /// <summary>
    /// Central audio hub. Assign AudioClips in the Inspector.
    /// Sound can stay empty for the first technical prototype.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [Header("Sound Effects")]
        public AudioClip ShotClip;
        public AudioClip ZoneHitClip;
        public AudioClip UpgradePurchasedClip;
        public AudioClip RoundWonClip;
        public AudioClip RoundLostClip;

        private AudioSource _source;

        private void Awake() => _source = GetComponent<AudioSource>();

        public void PlayShot()             => Play(ShotClip);
        public void PlayZoneHit()          => Play(ZoneHitClip);
        public void PlayUpgradePurchased() => Play(UpgradePurchasedClip);
        public void PlayRoundWon()         => Play(RoundWonClip);
        public void PlayRoundLost()        => Play(RoundLostClip);

        private void Play(AudioClip clip)
        {
            if (clip) _source.PlayOneShot(clip);
        }
    }
}
