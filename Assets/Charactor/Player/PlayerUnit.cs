using TestCardGame.Charactor.StatusVO;
using TestCardGame.Charactor.ValueObjects;
using UnityEngine;

namespace TestCardGame.Charactor.Player
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
        public Vector2Int Position { get; set; }
        public void MoveTo(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }

        /// <summary>
        /// プレイヤーのコンストラクタ。ID、名前、HPを初期化する。
        /// </summary>
        public PlayerUnit(UnitID id, string name, HP hp, Vector2Int position)
        {
            ID = id;
            Name = name;
            Position = position;
            Hp = hp;
        }
    }
}