using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Balanka
{
    /// <summary>
    /// Controls the Shop panel: show/hide, card instantiation, error feedback.
    /// </summary>
    public class ShopView : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject ShopPanel;

        [Header("Card Container")]
        public Transform           CardsContainer;
        public UpgradeCardView     CardPrefab;

        [Header("Buttons")]
        public Button RefreshButton;
        public Button SkipButton;

        [Header("Feedback")]
        public TextMeshProUGUI FeedbackLabel;   // "Not enough points!" etc.

        private readonly List<UpgradeCardView> _cards = new();
        private ShopManager _shop;

        private void Awake()
        {
            _shop = GameManager.Instance.ShopManager;
            if (RefreshButton) RefreshButton.onClick.AddListener(() => _shop.RefreshShop());
            if (SkipButton)    SkipButton.onClick.AddListener(()    => _shop.SkipShop());
        }

        // ── Public API called by ShopManager ─────────────────────────────

        public void Show(List<UpgradeDefinition> offers)
        {
            ShopPanel.SetActive(true);
            ClearCards();
            BuildCards(offers);
            ClearFeedback();
        }

        public void Hide()
        {
            ShopPanel.SetActive(false);
        }

        public void UpdateOffers(List<UpgradeDefinition> offers)
        {
            ClearCards();
            BuildCards(offers);
            ClearFeedback();
        }

        public void OnPurchaseConfirmed(int index)
        {
            // Grey out the purchased card; player can still skip
            if (index >= 0 && index < _cards.Count)
                _cards[index].SetPurchased();

            // After max purchases the skip button effectively ends the shop
            if (SkipButton) SkipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
        }

        public void ShowAffordError()   => ShowFeedback("Not enough points!");
        public void ShowMaxPurchaseInfo() => ShowFeedback("Only one upgrade per shot.");

        // ── Internal ──────────────────────────────────────────────────────

        private void BuildCards(List<UpgradeDefinition> offers)
        {
            for (int i = 0; i < offers.Count; i++)
            {
                UpgradeCardView card = Instantiate(CardPrefab, CardsContainer);
                int capturedIndex    = i;
                card.Populate(offers[i], () => _shop.PurchaseUpgrade(capturedIndex));
                _cards.Add(card);
            }
        }

        private void ClearCards()
        {
            foreach (var c in _cards) if (c) Destroy(c.gameObject);
            _cards.Clear();
        }

        private void ShowFeedback(string msg)
        {
            if (FeedbackLabel)
            {
                FeedbackLabel.text = msg;
                FeedbackLabel.gameObject.SetActive(true);
            }
        }

        private void ClearFeedback()
        {
            if (FeedbackLabel) FeedbackLabel.gameObject.SetActive(false);
        }
    }
}
