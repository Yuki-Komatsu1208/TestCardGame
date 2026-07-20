using System;
using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.Character.Player;
using TestCardGame.Stage;
using UnityEngine;

namespace TestCardGame.Run
{
    [CreateAssetMenu(fileName = "NewRunDefinition", menuName = "Card Game/Run Definition")]
    public class RunDefinitionSO : ScriptableObject
    {
        public string runName;
        public PlayerDefinitionSO playerDefinition;
        [Tooltip("RUN開始時に街で0G購入できるキーストーン候補。")]
        public List<KeystoneDefinition> keystones = new();
        [Tooltip("RUNを構成する遠征。各遠征の最後のステージがボス戦で、OverHunt候補は遠征ごとに設定する。")]
        public List<ExpeditionDefinition> expeditions = new();

        // 既存の報酬プール参照と旧アセット互換のために保持する。
        public List<StageDefinitionSO> stages;
    }

    [Serializable]
    public class ExpeditionDefinition
    {
        public string expeditionName;
        public List<StageDefinitionSO> stages = new();
        public List<StageDefinitionSO> overhuntStages = new();
    }

    [Serializable]
    public class KeystoneDefinition
    {
        public KeystoneId id;
        public string displayName;
        [TextArea] public string description;
        public List<BuildTag> favoredTags = new();
        public List<CardDefinitionSO> initialCards = new();
    }
}
