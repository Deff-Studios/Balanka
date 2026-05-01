using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Balanka
{
    /// <summary>
    /// Displays win / loss result and routes button actions back to GameManager.
    /// </summary>
    public class ResultView : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject ResultPanel;

        [Header("Labels")]
        public TextMeshProUGUI TitleLabel;
        public TextMeshProUGUI DetailsLabel;

        [Header("Buttons")]
        public Button ContinueButton;   // next round (win only)
        public Button RestartButton;    // new run

        private void Awake()
        {
            if (ContinueButton) ContinueButton.onClick.AddListener(
                () => GameManager.Instance.ContinueToNextRound());

            if (RestartButton)  RestartButton.onClick.AddListener(
                () => GameManager.Instance.RestartRun());
        }

        public void ShowWin(int score, int round)
        {
            ResultPanel.SetActive(true);
            TitleLabel.text   = "Round Clear!";
            DetailsLabel.text = $"Score: {score}\nProceeding to round {round + 1}…";
            if (ContinueButton) ContinueButton.gameObject.SetActive(true);
        }

        public void ShowLoss(int score, int target)
        {
            ResultPanel.SetActive(true);
            TitleLabel.text   = "Run Over";
            DetailsLabel.text = $"Score: {score}  /  Target: {target}";
            if (ContinueButton) ContinueButton.gameObject.SetActive(false);
        }

        public void Hide()
        {
            if (ResultPanel) ResultPanel.SetActive(false);
        }
    }
}
