using System;
using System.Collections.Generic;
using TestCardGame.Cards;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;
using TestCardGame.Character;
using TestCardGame.Character.StatusVO;
using TestCardGame.Character.ValueObjects;
using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Character.Player
{
    /// <summary>
    /// プレイヤーを表すクラス。
    /// </summary>
    public class PlayerUnit : IUnit
    {
        public static readonly PlayerUnit defaultPlayer = 
            new PlayerUnit(UnitID.defaultPlayerUnit, "Player", new HP(100), new Vector2Int(0, 0));

        public UnitID ID { get; } 
        public string Name { get; } 
        public HP Hp { get; } 
        public int Mana { get; set; }
        public int MaxMana { get; set; }
        public Vector2Int Position { get; set; }
        private readonly List<StatusEffectInstance> statusEffects = new();
        public IReadOnlyList<StatusEffectInstance> StatusEffects => statusEffects;

        /// <summary>
        /// 状態異常を付与する。同種の状態異常がある場合は既存効果へマージする。
        /// </summary>
        public void ApplyStatusEffect(StatusEffectInstance effect)
        {
            if (effect == null) return;

            var existing = statusEffects.Find(e => e.Definition.EffectId == effect.Definition.EffectId);
            if (existing != null)
            {
                existing.Merge(effect);
                Debug.Log($"{Name}の既存の状態異常 {effect.Definition.DisplayName} が更新されました（残り持続: {existing.RemainingTurns}ターン）。");
            }
            else
            {
                statusEffects.Add(effect);
                Debug.Log($"{Name}に状態異常 {effect.Definition.DisplayName} が付与されました（持続: {effect.RemainingTurns}ターン）。");
            }
        }

        /// <summary>
        /// 持続ターンが切れた状態異常を削除する。
        /// </summary>
        public void CleanExpiredStatusEffects()
        {
            for (int i = statusEffects.Count - 1; i >= 0; i--)
            {
                var effect = statusEffects[i];
                if (effect.IsExpired)
                {
                    statusEffects.RemoveAt(i);
                    Debug.Log($"{Name}の状態異常 {effect.Definition.DisplayName} が終了しました。");
                }
            }
        }

        /// <summary>
        /// 付与中の状態異常をもとに、現在行動できるかを返す。
        /// </summary>
        public bool CanAct
        {
            get
            {
                foreach (var effect in statusEffects)
                {
                    if (!effect.CanAct(this))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// プレイヤーの座標を直接更新する。
        /// </summary>
        public void MoveTo(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }
        public List<CardBase> Cards {get;private set;}

        /// <summary>
        /// プレイヤーのコンストラクタ。ID、名前、HPを初期化する。
        /// </summary>
        public PlayerUnit(UnitID id, string name, HP hp, Vector2Int position)
        {
            ID = id;
            Name = name;
            Position = position;
            Hp = hp;
            Mana = 3;
            MaxMana = 5;
            Cards = new List<CardBase>();
        }

        /// <summary>
        /// 実行時カード一覧を指定してプレイヤーを作成する。
        /// </summary>
        public PlayerUnit(UnitID id, string name, HP hp, Vector2Int position, List<CardBase> cards)
        {
            ID = id;
            Name = name;
            Position = position;
            Hp = hp;
            Mana = 3;
            MaxMana = 5;
            Cards = cards ?? new List<CardBase>();
        }

        /// <summary>
        /// カード定義一覧からレベル1カードを作成してプレイヤーを初期化する。
        /// </summary>
        public PlayerUnit(UnitID id, string name, HP hp, Vector2Int position, List<CardDefinitionSO> cardDefinitions)
        {
            ID = id;
            Name = name;
            Position = position;
            Hp = hp;
            Mana = 3;
            MaxMana = 5;
            Cards = new List<CardBase>();
            if (cardDefinitions != null)
            {
                foreach (var def in cardDefinitions)
                {
                    if (def != null)
                    {
                        Cards.Add(new CardBase(def, CardLevel.one));
                    }
                }
            }
        }

        /// <summary>
        /// PlayerDefinitionSOの初期設定からプレイヤーを作成する。
        /// </summary>
        public PlayerUnit(UnitID id, PlayerDefinitionSO definition, Vector2Int position)
        {
            ID = id;
            Name = definition != null ? definition.playerName : "Player";
            Position = position;
            Hp = new HP(definition != null ? definition.maxHp : 100);
            Mana = definition != null ? definition.initialMana : 3;
            MaxMana = definition != null ? definition.maxMana : 5;
            Cards = new List<CardBase>();
            if (definition != null && definition.initialCards != null)
            {
                foreach (var entry in definition.initialCards)
                {
                    if (entry == null || entry.card == null)
                    {
                        continue;
                    }

                    var level = Mathf.Clamp(entry.level, 1, 3);
                    Cards.Add(new CardBase(entry.card, new CardLevel(level)));
                }
            }
        }
    }
}
