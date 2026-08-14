using System.Collections.Generic;
using TestCardGame.Actions.Preview;
using TestCardGame.Cards.Core;
using TestCardGame.Character;
using UnityEngine;

namespace TestCardGame.Controller.Services
{
    /// <summary>カード内のPreview対応Effectをすべて合算する。</summary>
    public sealed class CardActionPreviewService
    {
        private readonly UnitMoveService moveService;

        public CardActionPreviewService(UnitMoveService moveService) => this.moveService = moveService;

        /// <summary>Preview可能な全Effectの選択可能セルと影響セルを和集合で返す。</summary>
        public ActionPreviewResult GetPreview(CardBase card, IUnit user, Vector2Int candidate)
        {
            if (card?.Effects == null || user == null || moveService == null) return ActionPreviewResult.None;
            var selectable = new HashSet<Vector2Int>();
            var moveCandidates = new HashSet<Vector2Int>();
            var affected = new HashSet<Vector2Int>();
            bool hasSpatialTarget = false;
            var context = new ActionPreviewContext(moveService, user, candidate);
            foreach (var effect in card.Effects)
            {
                if (effect is not IActionPreviewProvider provider) continue;
                ActionPreviewResult preview = provider.GetPreview(context);
                if (preview == null || !preview.HasSpatialTarget) continue;
                hasSpatialTarget = true;
                foreach (Vector2Int cell in preview.SelectableCells) selectable.Add(cell);
                foreach (Vector2Int cell in preview.MoveCandidateCells) moveCandidates.Add(cell);
                foreach (Vector2Int cell in preview.AffectedCells) affected.Add(cell);
            }
            return new ActionPreviewResult(hasSpatialTarget, new List<Vector2Int>(selectable),
                new List<Vector2Int>(affected), new List<Vector2Int>(moveCandidates));
        }
    }
}
