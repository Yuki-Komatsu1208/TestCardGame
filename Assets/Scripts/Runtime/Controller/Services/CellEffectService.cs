using TestCardGame.BoardManage;
using TestCardGame.Character.StatusEffects;
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
        public void TickCellEffects(Board board, StatusEffectService statusEffectService)
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

                    // 炎上しているマスにユニットが滞在している場合、OnFire状態異常を付与（または更新）する。
                    if (cell.IsOnFire && cell.Occupant != null)
                    {
                        statusEffectService?.ApplyBurn(cell.Occupant, cell.FireTurns, cell.FireDamage);
                        Debug.Log($"盤面効果：炎上マス（{cell.X}, {cell.Y}）に滞在しているため、{cell.Occupant.Name}にOnFire状態異常（{cell.FireTurns}ターン、ダメージ: {cell.FireDamage}）を適用しました。");
                    }

                    cell.TickFire(out int damage);
                }
            }
        }
    }
}
