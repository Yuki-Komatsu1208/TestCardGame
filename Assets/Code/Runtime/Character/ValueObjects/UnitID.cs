using System.Collections.Generic;
using TestCardGame.Character.StatusVO;
using TestCardGame.CommonBase;
using Unity.VisualScripting;

namespace TestCardGame.Character.ValueObjects
{
    /// <summary>
    /// フィールド上のユニットを表すID
    /// ユニットIDは、ユニットの種類を識別するためのCharacterIDと、
    /// フィールド上での個体識別のためのCodeを組み合わせたもの。
    /// </summary>
    public class UnitID : VOBase
    {
        public int Code { get; }
        public CharacterID CharaID { get; }

        public UnitID(int code, CharacterID characterID )
        {
            Code = code;
            CharaID = characterID;
        }
        
        public static UnitID defaultPlayerUnit = new UnitID(1, CharacterID.DefaultPlayer);
        public static UnitID slimeUnit = new UnitID(1, CharacterID.Slime);
        public static UnitID fireSlimeUnit = new UnitID(1, CharacterID.FireSlime);
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return new { Code, CharaID };
        }
    }

}
