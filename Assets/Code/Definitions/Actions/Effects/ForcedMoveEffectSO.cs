using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>衝突規則を持つ強制移動効果を生成するアセット。</summary>
    [CreateAssetMenu(fileName = "NewForcedMoveEffect", menuName = "Card Game/Effects/Forced Move")]
    public class ForcedMoveEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultDistance = 1;
        [SerializeField] private ForceMoveDirection defaultDirection = ForceMoveDirection.AwayFromUser;
        [SerializeField, Min(0)] private int defaultStunDamage = 10;

        public override string[] ParameterFields => new[] { "distance", "forceMoveDirection", "forceMoveStunDamage" };

        public override string GetParameterDisplayName(string fieldName) => fieldName switch
        {
            "distance" => "移動距離",
            "forceMoveDirection" => "方向",
            "forceMoveStunDamage" => "基礎スタン耐性ダメージ",
            _ => base.GetParameterDisplayName(fieldName)
        };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.distance = defaultDistance;
            parameters.forceMoveDirection = defaultDirection;
            parameters.forceMoveStunDamage = defaultStunDamage;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            return new eForcedMove(
                Mathf.Max(1, parameters?.distance ?? defaults.distance),
                parameters?.forceMoveDirection ?? defaults.forceMoveDirection,
                Mathf.Max(0, parameters?.forceMoveStunDamage ?? defaults.forceMoveStunDamage));
        }
    }
}
