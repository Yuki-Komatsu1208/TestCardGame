using TestCardGame.Character.Enemies;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 敵召喚効果の設定を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewSummonEnemyEffect", menuName = "Card Game/Effects/Summon Enemy")]
    public class SummonEnemyEffectSO : ActionEffectSO
    {
        [SerializeField] private EnemyDefinitionSO enemyDefinition;

        public EnemyDefinitionSO EnemyDefinition => enemyDefinition;

        /// <summary>
        /// 召喚する敵定義を差し替える。
        /// </summary>
        public void Configure(EnemyDefinitionSO definition)
        {
            enemyDefinition = definition;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            return new eSummonEnemy(enemyDefinition);
        }
    }
}
