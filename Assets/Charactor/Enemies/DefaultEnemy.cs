using TestCardGame.Charactor.ValueObjects;

namespace TestCardGame.Charactor.Enemies
{
    /// <summary>
    /// デフォルトの敵キャラクターを表すクラス。
    /// </summary>
    public class DefaultEnemy : IUnit
    {
        public static readonly DefaultEnemy defaultEnemy = 
            new DefaultEnemy(UnitID.defaultEnemyUnit, "Default Enemy", new StatusVO.HP(50), new UnityEngine.Vector2Int(0, 0));

        public UnitID ID { get; }
        public string Name { get; }
        public StatusVO.HP Hp { get; }
        public UnityEngine.Vector2Int Position { get; set; }

        /// <summary>
        /// デフォルトの敵キャラクターのコンストラクタ。ID、名前、HPを初期化する。
        /// </summary>
        public DefaultEnemy(UnitID id, string name, StatusVO.HP hp, UnityEngine.Vector2Int position)
        {
            ID = id;
            Name = name;
            Position = position;
            Hp = hp;
        }


        public void MoveTo(int x, int y)
        {
            Position = new UnityEngine.Vector2Int(x, y);
        }
    }
}