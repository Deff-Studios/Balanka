using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Balanka
{
    /// <summary>
    /// Opens after every shot, presents upgrade cards, handles purchase /
    /// refresh / skip logic.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────
        [Header("Shop Settings")]
        public int CardsShown         = 3;
        public int RefreshCost        = 100;
        public int MaxPurchasesPerVisit = 1;

        [Header("Upgrade Pool (ScriptableObjects)")]
        public List<UpgradeDefinition> UpgradePool = new();

        [Header("References")]
        public ShopView ShopView;

        // ── Runtime ───────────────────────────────────────────────────────
        private List<UpgradeDefinition> _currentOffers = new();
        private int _purchasesThisVisit;

        // ── Public API ────────────────────────────────────────────────────

        public void OpenShop()
        {
            _purchasesThisVisit = 0;
            GenerateOffers();

            if (_currentOffers.Count == 0)
            {
                ShopView.Hide();
                GameManager.Instance.RoundManager.OnShopClosed();
                return;
            }

            ShopView.Show(_currentOffers);
        }

        public void RefreshShop()
        {
            if (!GameManager.Instance.RoundManager.SpendScore(RefreshCost))
            {
                ShopView.ShowAffordError();
                return;
            }
            GenerateOffers();
            ShopView.UpdateOffers(_currentOffers);
        }

        public void PurchaseUpgrade(int offerIndex)
        {
            if (_purchasesThisVisit >= MaxPurchasesPerVisit)
            {
                ShopView.ShowMaxPurchaseInfo();
                return;
            }

            if (offerIndex < 0 || offerIndex >= _currentOffers.Count) return;
            UpgradeDefinition def = _currentOffers[offerIndex];

            if (!GameManager.Instance.RoundManager.SpendScore(def.Cost))
            {
                ShopView.ShowAffordError();
                return;
            }

            GameManager.Instance.UpgradeManager.ApplyUpgrade(def);
            GameManager.Instance.AudioManager.PlayUpgradePurchased();
            _purchasesThisVisit++;
            ShopView.OnPurchaseConfirmed(offerIndex);
        }

        public void SkipShop()
        {
            ShopView.Hide();
            GameManager.Instance.RoundManager.OnShopClosed();
        }

        // ── Internal ──────────────────────────────────────────────────────

        private void GenerateOffers()
        {
            _currentOffers.Clear();
            List<UpgradeDefinition> pool = new List<UpgradeDefinition>(UpgradePool);

            int count = Mathf.Min(CardsShown, pool.Count);
            for (int i = 0; i < count; i++)
            {
                int idx  = Random.Range(0, pool.Count);
                _currentOffers.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
        }
    }
}
