using TestCardGame.Character;

namespace TestCardGame.Character.Enemies
{
    /// <summary>
    /// 敵ユニットが持つ最小限の共通契約。
    /// 具体的なステータスや行動内容は各敵クラスに閉じ込める。
    /// </summary>
    public interface IEnemy : IUnit
    {
        /// <summary>
        /// 敵のターン行動を実行する。
        /// </summary>
        void ExecuteTurn(EnemyTurnContext context);
    }
}
