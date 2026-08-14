using System;
using System.Collections.Generic;
using TestCardGame.Character.ValueObjects;
using UnityEngine;

namespace TestCardGame.Character.Enemies
{
    /// <summary>
    /// UIへ公開する、副作用なしで解決済みの敵次行動情報。
    /// </summary>
    public sealed class EnemyIntentPreview
    {
        public UnitID EnemyId { get; }
        public EnemyIntentKind Kind { get; }
        public Vector2Int TargetPosition { get; }
        public IReadOnlyList<Vector2Int> AffectedCells { get; }
        public bool CanExecute { get; }
        public string UnavailableReason { get; }

        private EnemyIntentPreview(UnitID enemyId, EnemyIntentKind kind, Vector2Int targetPosition,
            IReadOnlyList<Vector2Int> affectedCells, bool canExecute, string unavailableReason)
        {
            EnemyId = enemyId;
            Kind = kind;
            TargetPosition = targetPosition;
            AffectedCells = affectedCells ?? Array.Empty<Vector2Int>();
            CanExecute = canExecute;
            UnavailableReason = unavailableReason ?? string.Empty;
        }

        public static EnemyIntentPreview Available(UnitID enemyId, EnemyIntentKind kind,
            Vector2Int targetPosition, IReadOnlyList<Vector2Int> affectedCells)
            => new(enemyId, kind, targetPosition, affectedCells, true, string.Empty);

        public static EnemyIntentPreview Unavailable(UnitID enemyId, string reason)
            => new(enemyId, EnemyIntentKind.Unset, default, Array.Empty<Vector2Int>(), false, reason);
    }
}
