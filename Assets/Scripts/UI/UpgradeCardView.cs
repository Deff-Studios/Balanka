using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Balanka
{
    /// <summary>
    /// One upgrade card in the shop. Populated at runtime by ShopView.
    /// </summary>
    public class UpgradeCardView : MonoBehaviour
    {
        [Header("Labels")]
        public TextMeshProUGUI NameLabel;
        public TextMeshProUGUI DescriptionLabel;
        public TextMeshProUGUI CostLabel;

        [Header("Buy Button")]
        public Button BuyButton;

        [Header("Purchased State")]
        public Color PurchasedTint = new Color(0.4f, 0.4f, 0.4f, 0.6f);

        private Image _bg;

        private void Awake() => _bg = GetComponent<Image>();

        public void Populate(UpgradeDefinition def, System.Action onBuy)
        {
            if (NameLabel)        NameLabel.text        = def.UpgradeName;
            if (DescriptionLabel) DescriptionLabel.text = def.Description;
            if (CostLabel)        CostLabel.text        = $"{def.Cost} pts";

            if (BuyButton)
            {
                BuyButton.onClick.RemoveAllListeners();
                BuyButton.onClick.AddListener(() => onBuy?.Invoke());
            }
        }

        public void SetPurchased()
        {
            if (BuyButton) BuyButton.interactable = false;
            if (_bg)       _bg.color              = PurchasedTint;
        }
    }
}
