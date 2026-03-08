using System.Collections.Generic;
using TestCardGame.Charactor.StatusVO;
using TestCardGame.CommonBase;
using Unity.VisualScripting;

namespace TestCardGame.Charactor.ValueObjects
{
    /// <summary>
    /// フィールド上のユニットを表すID
    /// ユニットIDは、ユニットの種類を識別するためのCharactorIDと、
    /// フィールド上での個体識別のためのCodeを組み合わせたもの。
    /// </summary>
    public class UnitID : VOBase
    {
        public int Code { get; }
        public CharactorID CharaID { get; }

        public UnitID(int code, CharactorID charactorID )
        {
            Code = code;
            CharaID = charactorID;
        }
        
        public static UnitID defaultPlayerUnit = new UnitID(1, CharactorID.DefaultPlayer);
        public static UnitID defaultEnemyUnit = new UnitID(1, CharactorID.DefaultEnemy);
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return new { Code, CharaID };
        }
    }

}