using TestCardGame.BoardManage;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>
    /// 盤面セルに付与された継続効果を進行させるサービス。
    /// </summary>
    public class CellEffectService
    {
        /// <summary>
        /// すべてのセル効果を1ターン分進め、必要ならユニットへダメージを与える。
        /// </summary>
        public void TickCellEffects(Board board)
        {
            if (board == null)
            {
                return;
            }

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var cell = board.GetCell(x, y);
                    cell.TickFire(out int damage);
                    if (damage <= 0 || cell.Occupant == null)
                    {
                        continue;
                    }

                    cell.Occupant.Hp.TakeDamage(damage);
                    Debug.Log($"{cell.Occupant.Name} took {damage} fire damage! Current HP: {cell.Occupant.Hp.CurrentValue}");
                }
            }
        }
    }
}
