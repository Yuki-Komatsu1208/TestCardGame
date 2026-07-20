using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;

namespace TestCardGame.Run
{
    public enum RunProgressPhase
    {
        Expedition,
        AwaitingNormalRewardResolution,
        AwaitingReturnOrOverhuntChoice,
        OverHunt,
        AwaitingOverhuntDecision,
        Town,
        Completed,
        Failed
    }

    /// <summary>
    /// ラン中に継続するプレイヤーの状態。
    /// </summary>
    [Serializable]
    public class RunState
    {
        public List<CardBase> playerDeck = new();
        public int currentExpeditionIndex;
        public int currentStageIndex;
        public bool hasStartedCurrentExpedition;
        public int currentHp;
        public int ownedGold;
        // アイテム実装の共通所持状態。遠征遷移では再生成せず、RUN終了まで保持する。
        public List<string> ownedItemIds = new();
        public KeystoneId selectedKeystone = KeystoneId.None;
        public List<BuildTag> favoredBuildTags = new();
        public int pendingGold;
        public int overhuntDepth;
        public bool isInOverhunt;
        public RunProgressPhase phase;
    }
}
