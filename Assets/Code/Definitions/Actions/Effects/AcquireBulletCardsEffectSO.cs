using System.Collections.Generic;
using TestCardGame.Cards.Core;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewAcquireBulletCardsEffect", menuName = "Card Game/Effects/Acquire Bullet Cards")]
    public sealed class AcquireBulletCardsEffectSO : ActionEffectSO
    {
        [SerializeField] private List<CardDefinitionSO> bulletDefinitions = new();

        public override string[] ParameterFields => new[] { "count" };

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            int count = Mathf.Max(1, parameters?.count ?? 1);
            return new eAcquireBulletCards(bulletDefinitions, count, level);
        }
    }
}
