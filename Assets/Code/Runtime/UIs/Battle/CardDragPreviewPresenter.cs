using UnityEngine;

namespace TestCardGame.UIs.Battle
{
    /// <summary>カードドラッグ入力を盤面プレビュー処理へ渡す契約。</summary>
    public interface ICardDragPreviewController
    {
        void BeginPreview(int cardIndex);
        void UpdatePreview(int cardIndex, Vector2 screenPosition);
        bool Commit(int cardIndex, Vector2 screenPosition);
        void Cancel(int cardIndex);
    }
}
