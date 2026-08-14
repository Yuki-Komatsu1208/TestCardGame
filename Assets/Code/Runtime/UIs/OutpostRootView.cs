using System;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    public enum OutpostPanelKind { Facilities, Inn, Shop, Preparing }

    /// <summary>Outpostの事前配置パネルをactive切替し、施設入力だけを通知する。</summary>
    public sealed class OutpostRootView : MonoBehaviour
    {
        [SerializeField] private GameObject facilitiesPanel;
        [SerializeField] private GameObject innPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject preparingPanel;
        [SerializeField] private Button innButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button workshopButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button departButton;
        [SerializeField] private Button[] backButtons = Array.Empty<Button>();
        [SerializeField] private OutpostInnView inn;
        [SerializeField] private OutpostShopView shop;
        [SerializeField] private OutpostPreparingView preparing;
        public OutpostInnView Inn => inn;
        public OutpostShopView Shop => shop;
        public OutpostPreparingView Preparing => preparing;
        public void BindFacilityActions(UnityEngine.Events.UnityAction openInn, UnityEngine.Events.UnityAction openShop, UnityEngine.Events.UnityAction openWorkshop, UnityEngine.Events.UnityAction openQuest, UnityEngine.Events.UnityAction depart) { innButton.onClick.AddListener(openInn); shopButton.onClick.AddListener(openShop); workshopButton.onClick.AddListener(openWorkshop); questButton.onClick.AddListener(openQuest); departButton.onClick.AddListener(depart); }
        public void BindBackActions(UnityEngine.Events.UnityAction back) { foreach (var button in backButtons) button.onClick.AddListener(back); }
        public void ClearListeners() { innButton.onClick.RemoveAllListeners(); shopButton.onClick.RemoveAllListeners(); workshopButton.onClick.RemoveAllListeners(); questButton.onClick.RemoveAllListeners(); departButton.onClick.RemoveAllListeners(); foreach (var button in backButtons) button.onClick.RemoveAllListeners(); }
        public void Show(OutpostPanelKind panel) { facilitiesPanel.SetActive(panel == OutpostPanelKind.Facilities); innPanel.SetActive(panel == OutpostPanelKind.Inn); shopPanel.SetActive(panel == OutpostPanelKind.Shop); preparingPanel.SetActive(panel == OutpostPanelKind.Preparing); }
        public bool ValidateReferences() { bool valid = facilitiesPanel != null && innPanel != null && shopPanel != null && preparingPanel != null && innButton != null && shopButton != null && workshopButton != null && questButton != null && departButton != null && inn != null && shop != null && preparing != null && backButtons != null && backButtons.Length == 3; if (!valid) { Debug.LogError($"{name}: Outpost ViewのInspector参照が不足しています。", this); return false; } return inn.ValidateReferences() && shop.ValidateReferences() && preparing.ValidateReferences(); }
    }
}
