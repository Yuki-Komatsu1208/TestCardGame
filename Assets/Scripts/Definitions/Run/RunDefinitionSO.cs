using System.Collections.Generic;
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
        public List<StageDefinitionSO> stages;
    }
}