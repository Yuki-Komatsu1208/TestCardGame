using System.Collections.Generic;
using TestCardGame.Character;
using TestCardGame.Character.ValueObjects;
using UnityEngine;
using System;

namespace TestCardGame.Controller.Services
{
    /// <summary>
    /// ユニットのモデル座標に合わせて、対応するViewをセル位置へ移動するサービス。
    /// </summary>
    public class ViewMoveService
    {
        private readonly IReadOnlyDictionary<Vector2Int, RectTransform> cellRects;
        private readonly IReadOnlyDictionary<UnitID, UnitView> viewByUnitId;
        private readonly IReadOnlyDictionary<UnitID, IUnit> unitsById;
        private UnitMoveService unitMoveService;

        public event Action<UnitID, Vector2Int, Vector2Int> ViewMoved;

        /// <summary>
        /// セル表示、ユニットView、ユニット実体の対応表を受け取ってサービスを作成する。
        /// </summary>
        public ViewMoveService(
            IReadOnlyDictionary<Vector2Int, RectTransform> cellRects,
            IReadOnlyDictionary<UnitID, UnitView> viewByUnitId,
            IReadOnlyDictionary<UnitID, IUnit> unitsById)
        {
            this.cellRects = cellRects;
            this.viewByUnitId = viewByUnitId;
            this.unitsById = unitsById;
        }

        /// <summary>
        /// 移動サービスの完了イベントを購読し、View移動を連動させる。
        /// </summary>
        public void Bind(UnitMoveService unitMoveService)
        {
            if (this.unitMoveService != null)
            {
                this.unitMoveService.MoveCompleted -= OnMoveCompleted;
            }

            this.unitMoveService = unitMoveService;
            if (this.unitMoveService != null)
            {
                this.unitMoveService.MoveCompleted += OnMoveCompleted;
            }
        }

        /// <summary>
        /// 全ユニットViewを現在のモデル座標へ同期する。
        /// </summary>
        public void SyncAllViewsFromModel()
        {
            if (unitsById == null)
            {
                return;
            }

            foreach (var unit in unitsById.Values)
            {
                MoveUnitView(unit);
            }
        }

        /// <summary>
        /// モデル移動完了時に該当ユニットのViewを更新する。
        /// </summary>
        private void OnMoveCompleted(UnitID unitId, Vector2Int from, Vector2Int to)
        {
            if (unitsById == null || !unitsById.TryGetValue(unitId, out var unit))
            {
                return;
            }

            MoveUnitView(unit);
            ViewMoved?.Invoke(unitId, from, to);
        }

        /// <summary>
        /// 指定ユニットのViewを現在位置のセルへ移動する。
        /// </summary>
        private void MoveUnitView(IUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            if (viewByUnitId == null || !viewByUnitId.TryGetValue(unit.ID, out var unitView))
            {
                return;
            }

            if (cellRects == null || !cellRects.TryGetValue(unit.Position, out var cellRect))
            {
                return;
            }

            unitView.MoveToCell(cellRect);
        }

        /// <summary>
        /// 購読中の移動イベントを解除する。
        /// </summary>
        public void Dispose()
        {
            if (unitMoveService != null)
            {
                unitMoveService.MoveCompleted -= OnMoveCompleted;
            }
        }
    }
}
