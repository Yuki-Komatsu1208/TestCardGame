using System;
using System.Collections.Generic;
using TestCardGame.Cards;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;
using TestCardGame.Character;
using TestCardGame.Character.StatusVO;
using TestCardGame.Character.ValueObjects;
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

