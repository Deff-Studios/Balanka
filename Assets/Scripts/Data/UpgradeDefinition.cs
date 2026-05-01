using UnityEngine;

namespace Balanka
{
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Balanka/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Display")]
        public string UpgradeName = "Upgrade";
        [TextArea] public string Description = "";

        [Header("Economy")]
        public int Cost = 100;

        [Header("Effect")]
        public UpgradeType Type;
    }
}
