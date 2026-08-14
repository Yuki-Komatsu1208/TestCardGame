using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewShieldBashEffect", menuName = "Card Game/Effects/Shield Bash")]
    public sealed class ShieldBashEffectSO : ActionEffectSO
    {
        [SerializeField, Min(0)] private int defaultDamage = 10;
        [SerializeField, Min(0)] private int defaultShieldValue = 15;
        [SerializeField, Min(1)] private int defaultMoveDistance = 1;
        [SerializeField, Min(0)] private int defaultForceMoveStunDamage = 10;

        public override string[] ParameterFields => new[] { "damage", "value", "distance", "forceMoveStunDamage" };

        public override string GetParameterDisplayName(string fieldName) => fieldName switch
        {
            "damage" => "HPダメージ",
            "value" => "シールド値",
            "distance" => "移動距離",
            "forceMoveStunDamage" => "FORCEMOVE基礎スタン耐性ダメージ",
            _ => base.GetParameterDisplayName(fieldName)
        };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.damage = defaultDamage;
            parameters.value = defaultShieldValue;
            parameters.distance = defaultMoveDistance;
            parameters.forceMoveStunDamage = defaultForceMoveStunDamage;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            return new eShieldBash(
                Mathf.Max(0, parameters?.damage ?? defaults.damage),
                Mathf.Max(0, parameters?.value ?? defaults.value),
                Mathf.Max(1, parameters?.distance ?? defaults.distance),
                Mathf.Max(0, parameters?.forceMoveStunDamage ?? defaults.forceMoveStunDamage));
        }
    }
}
