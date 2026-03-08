using System.Collections.Generic;
using TestCardGame.Charactor.StatusVO;
using TestCardGame.CommonBase;

namespace TestCardGame.Charactor.ValueObjects
{
    /// <summary>
    /// キャラクターごとのIDを表すクラス。
    /// </summary>
    public class CharactorID:VOBase
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
        public CharactorID(int code,int viewKey)
        {
            Code = code;
            ViewKey = viewKey;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return new{Code, ViewKey};
        }
        /// <summary>デフォルトのプレイヤーID</summary>
        public static CharactorID DefaultPlayer = new CharactorID(1, 1);
        public static CharactorID DefaultEnemy = new CharactorID(2, 2);

        /// <summary>
        /// 全てのキャラクターIDを管理する辞書。キーはキャラクターコード、値はCharactorIDオブジェクト。
        /// </summary>
        public static Dictionary<int, CharactorID> charactorIDs = new Dictionary<int, CharactorID>
        {
            {DefaultPlayer.Code, DefaultPlayer},
            {DefaultEnemy.Code, DefaultEnemy},
            // 他のキャラクターIDもここに追加
        };
    }
}