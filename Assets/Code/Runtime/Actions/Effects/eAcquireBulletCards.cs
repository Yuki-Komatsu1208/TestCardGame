using System;
using System.Collections.Generic;
using TestCardGame.Actions.Core;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.VOs;
using TestCardGame.Character.Player;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 設定された弾丸候補から、使用者と同じレベルの弾丸をランダムに獲得する。
    /// </summary>
    public sealed class eAcquireBulletCards : ActionEffect
    {
        private readonly IReadOnlyList<CardDefinitionSO> bulletDefinitions;
        private readonly int count;
        private readonly int level;

        public eAcquireBulletCards(IReadOnlyList<CardDefinitionSO> bulletDefinitions, int count, int level)
        {
            this.bulletDefinitions = bulletDefinitions ?? Array.Empty<CardDefinitionSO>();
            this.count = Mathf.Max(1, count);
            this.level = Mathf.Clamp(level, 1, 3);
        }

        public override bool CanExecute(ActionContext context)
        {
            return context?.User is PlayerUnit && bulletDefinitions.Count > 0;
        }

        public override void Execute(ActionContext context)
        {
            if (context?.User is not PlayerUnit player || bulletDefinitions.Count == 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                CardDefinitionSO definition = bulletDefinitions[UnityEngine.Random.Range(0, bulletDefinitions.Count)];
                if (definition != null)
                {
                    player.Cards.Add(new CardBase(definition, new CardLevel(level)));
                }
            }
        }
    }
}
