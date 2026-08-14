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
using ManaStatus = TestCardGame.Character.StatusVO.Mana;

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
        public STUNResist StunResist { get; }
        public ManaStatus Mana { get; }
        public Vector2Int Position { get; set; }
        private readonly StatusEffectCollection statusEffects = new();
        public IReadOnlyList<StatusEffectInstance> StatusEffects => statusEffects.Effects;

        /// <summary>
        /// 状態異常を付与する。同種の状態異常がある場合は既存効果へマージする。
        /// </summary>
        public void ApplyStatusEffect(StatusEffectInstance effect)
        {
            statusEffects.Apply(this, effect);
        }

        /// <summary>
        /// 持続ターンが切れた状態異常を削除する。
        /// </summary>
        public void CleanExpiredStatusEffects()
        {
            statusEffects.CleanExpired(this);
        }

        /// <summary>
        /// 付与中の状態異常をもとに、現在行動できるかを返す。
        /// </summary>
        public bool CanAct
        {
            get { return statusEffects.CanAct(this); }
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
            StunResist = new STUNResist(100, 100);
            Mana = new ManaStatus(ManaStatus.DefaultCurrentValue, ManaStatus.DefaultMaxValue, ManaStatus.DefaultRecoveryPerTurn);
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
            StunResist = new STUNResist(100, 100);
            Mana = new ManaStatus(ManaStatus.DefaultCurrentValue, ManaStatus.DefaultMaxValue, ManaStatus.DefaultRecoveryPerTurn);
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
            StunResist = new STUNResist(100, 100);
            Mana = new ManaStatus(ManaStatus.DefaultCurrentValue, ManaStatus.DefaultMaxValue, ManaStatus.DefaultRecoveryPerTurn);
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
            int maxStunResist = definition != null ? definition.maxStunResist : 100;
            StunResist = new STUNResist(maxStunResist, maxStunResist);
            Mana = new ManaStatus(
                definition != null ? definition.initialMana : ManaStatus.DefaultCurrentValue,
                definition != null ? definition.maxMana : ManaStatus.DefaultMaxValue,
                definition != null ? definition.manaRecoveryPerTurn : ManaStatus.DefaultRecoveryPerTurn);
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

        /// <summary>
        /// RUNで維持されたカード・スタン耐性を指定してプレイヤーを作成する。
        /// </summary>
        public PlayerUnit(UnitID id, PlayerDefinitionSO definition, Vector2Int position, List<CardBase> cards, int currentStunResist)
            : this(id, definition, position)
        {
            Cards = cards ?? new List<CardBase>();
            StunResist = new STUNResist(currentStunResist > 0 ? currentStunResist : StunResist.MaxValue, StunResist.MaxValue);
        }
    }
}
