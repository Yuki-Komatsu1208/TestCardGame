using System;
using System.Collections.Generic;
using TestCardGame.Actions.Core;
using TestCardGame.Character;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Actions.Preview
{
    /// <summary>副作用なしで行動の選択可能セルと影響セルを問い合わせる契約。</summary>
    public interface IActionPreviewProvider
    {
        ActionPreviewResult GetPreview(ActionPreviewContext context);
    }

    /// <summary>行動プレビューに必要な読み取り専用参照を保持する。</summary>
    public sealed class ActionPreviewContext
    {
        public UnitMoveService MoveService { get; }
        public IUnit User { get; }
        public Vector2Int CandidatePosition { get; }
        public StatusEffectService StatusEffectService { get; }

        public ActionPreviewContext(UnitMoveService moveService, IUnit user, Vector2Int candidatePosition,
            StatusEffectService statusEffectService = null)
        {
            MoveService = moveService;
            User = user;
            CandidatePosition = candidatePosition;
            StatusEffectService = statusEffectService;
        }

        /// <summary>既存の実行可否判定へ渡す読み取り用Contextを作る。</summary>
        public ActionContext ToActionContext() => new(MoveService, User, CandidatePosition, StatusEffectService);
    }

    /// <summary>空間対象の有無、選択可能セル、現在候補の影響セルを返す。</summary>
    public sealed class ActionPreviewResult
    {
        private static readonly IReadOnlyList<Vector2Int> EmptyCells = Array.Empty<Vector2Int>();
        public static ActionPreviewResult None { get; } = new(false, EmptyCells, EmptyCells);

        public bool HasSpatialTarget { get; }
        public IReadOnlyList<Vector2Int> SelectableCells { get; }
        public IReadOnlyList<Vector2Int> MoveCandidateCells { get; }
        public IReadOnlyList<Vector2Int> AffectedCells { get; }

        public ActionPreviewResult(bool hasSpatialTarget, IReadOnlyList<Vector2Int> selectableCells,
            IReadOnlyList<Vector2Int> affectedCells, IReadOnlyList<Vector2Int> moveCandidateCells = null)
        {
            HasSpatialTarget = hasSpatialTarget;
            SelectableCells = selectableCells ?? EmptyCells;
            MoveCandidateCells = moveCandidateCells ?? EmptyCells;
            AffectedCells = affectedCells ?? EmptyCells;
        }
    }
}
