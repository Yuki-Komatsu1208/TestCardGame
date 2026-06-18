using System.Collections.Generic;
using TestCardGame.Character.StatusVO;
using TestCardGame.CommonBase;

namespace TestCardGame.Character.ValueObjects
{
    /// <summary>
    /// キャラクターごとのIDを表すクラス。
    /// </summary>
    public class CharacterID:VOBase
    {
        /// <summary>
        /// キャラクターコード。各キャラクターに一意のコードを割り当てるための整数値。
        /// </summary>
        public int Code { get; }
        /// <summary>
        /// キャラクタービューのキー。キャラクターの見た目を管理するための識別子。
        /// </summary>
        public int ViewKey { get; }
        /// <summary>
        /// キャラクターIDのコンストラクタ。キャラクターコードとキャラクタービューのキーを初期化する。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="viewKey"></param>
        public CharacterID(int code,int viewKey)
        {
            Code = code;
            ViewKey = viewKey;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return new{Code, ViewKey};
        }
        /// <summary>デフォルトのプレイヤーID</summary>
        public static CharacterID DefaultPlayer = new CharacterID(1, 1);
        public static CharacterID Slime = new CharacterID(2, 2);
        public static CharacterID FireSlime = new CharacterID(3, 2);

        /// <summary>
        /// 全てのキャラクターIDを管理する辞書。キーはキャラクターコード、値はCharacterIDオブジェクト。
        /// </summary>
        public static Dictionary<int, CharacterID> characterIDs = new Dictionary<int, CharacterID>
        {
            {DefaultPlayer.Code, DefaultPlayer},
            {Slime.Code, Slime},
            {FireSlime.Code, FireSlime},
            // 他のキャラクターIDもここに追加
        };
    }
}
