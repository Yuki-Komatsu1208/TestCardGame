using System.Collections.Generic;
using TestCardGame.Character;
using TestCardGame.Character.ValueObjects;
using UnityEngine;
using System;

namespace TestCardGame.Controller.Services
{
    public class ViewMoveService
    {
        private readonly IReadOnlyDictionary<Vector2Int, RectTransform> cellRects;
        private readonly IReadOnlyDictionary<UnitID, UnitView> viewByUnitId;
        private readonly IReadOnlyDictionary<UnitID, IUnit> unitsById;
        private UnitMoveService unitMoveService;

        public event Action<UnitID, Vector2Int, Vector2Int> ViewMoved;

        public ViewMoveService(
            IReadOnlyDictionary<Vector2Int, RectTransform> cellRects,
            IReadOnlyDictionary<UnitID, UnitView> viewByUnitId,
            IReadOnlyDictionary<UnitID, IUnit> unitsById)
        {
            this.cellRects = cellRects;
            this.viewByUnitId = viewByUnitId;
            this.unitsById = unitsById;
        }

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

        private void OnMoveCompleted(UnitID unitId, Vector2Int from, Vector2Int to)
        {
            if (unitsById == null || !unitsById.TryGetValue(unitId, out var unit))
            {
                return;
            }

            MoveUnitView(unit);
            ViewMoved?.Invoke(unitId, from, to);
        }

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

        public void Dispose()
        {
            if (unitMoveService != null)
            {
                unitMoveService.MoveCompleted -= OnMoveCompleted;
            }
        }
    }
}
