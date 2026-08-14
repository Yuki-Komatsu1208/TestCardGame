using UnityEngine;

namespace TestCardGame.UIs.PostBattle
{
    /// <summary>戦闘後UIの排他的な表示切替を担当する。</summary>
    public sealed class PostBattleRootView : MonoBehaviour
    {
        [SerializeField] private RewardChoicePanelView rewardPanel;
        [SerializeField] private RouteChoicePanelView routePanel;
        [SerializeField] private RunEndPanelView runEndPanel;

        public RewardChoicePanelView RewardPanel => rewardPanel;
        public RouteChoicePanelView RoutePanel => routePanel;
        public RunEndPanelView RunEndPanel => runEndPanel;

        public void HideAll()
        {
            rewardPanel?.Hide();
            routePanel?.Hide();
            runEndPanel?.Hide();
        }

        public bool ValidateReferences() => rewardPanel != null && routePanel != null && runEndPanel != null
            && rewardPanel.ValidateReferences() && routePanel.ValidateReferences() && runEndPanel.ValidateReferences();
    }
}
