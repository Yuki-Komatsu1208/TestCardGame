using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.PostBattle
{
    /// <summary>帰還とOverHunt続行の入力を操作IDとして通知する。</summary>
    public sealed class RouteChoicePanelView : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private TMP_Text continueLabelText;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button continueButton;

        public event Action<RouteChoiceAction> ActionSelected;

        private void Awake()
        {
            returnButton?.onClick.AddListener(RequestReturn);
            continueButton?.onClick.AddListener(RequestContinue);
        }

        private void OnDestroy()
        {
            returnButton?.onClick.RemoveListener(RequestReturn);
            continueButton?.onClick.RemoveListener(RequestContinue);
        }

        public void Show(RouteChoiceViewModel model)
        {
            if (titleText != null) titleText.text = model.Title;
            if (statusText != null)
                statusText.text = $"現在HP  {model.CurrentHp}    OverHunt深度  {model.OverHuntDepth}\n保留ゴールド  {model.PendingGold} G";
            if (warningText != null) warningText.text = "続行中に死亡すると、このRUNは終了します。";
            if (continueLabelText != null) continueLabelText.text = model.ContinueLabel;
            if (continueButton != null) continueButton.gameObject.SetActive(model.CanContinue);
            panelRoot?.SetActive(true);
        }

        public void Hide() => panelRoot?.SetActive(false);

        private void RequestReturn() => ActionSelected?.Invoke(RouteChoiceAction.ReturnToOutpost);
        private void RequestContinue() => ActionSelected?.Invoke(RouteChoiceAction.ContinueOverHunt);

        public bool ValidateReferences() => panelRoot != null && titleText != null && statusText != null
            && warningText != null && continueLabelText != null && returnButton != null && continueButton != null;
    }
}
