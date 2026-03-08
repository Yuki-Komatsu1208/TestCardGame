using TestCardGame.Charactor.ValueObjects;
using Unity.VisualScripting;
using UnityEngine;

namespace TestCardGame.Controller
{
    public class MoveController : MonoBehaviour
    {
        private void OnMoveStarted(UnitID unitId, Vector2Int from, Vector2Int to)
        {
            // 移動開始時の処理（例: アニメーション開始）
        }

        private void OnMoveCompleted(UnitID unitId, Vector2Int from, Vector2Int to)
        {
            // 移動完了時の処理（例: アニメーション終了）
        }

        private void OnMoveRejected(UnitID unitId, Vector2Int from, string reason)
        {
            // 移動拒否時の処理（例: エラーメッセージ表示）
        }
    }

    public class MoveCommand
    {
        public Vector2Int Direction { get; }
        public int MoveCount{get;}
        public MoveCommand(Vector2Int direction, int moveCount)
        {
            Direction = direction;
            MoveCount = moveCount;
        }
    }
}
