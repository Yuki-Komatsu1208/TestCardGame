using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.Battle
{
    /// <summary>セルの永続状態と独立した一時Overlayを表示する。</summary>
    public sealed class CellView : MonoBehaviour
    {
        [SerializeField] private Image baseImage;
        [SerializeField] private Image moveCandidateBorder;
        [SerializeField] private Image playerActionBorder;
        [SerializeField] private Image enemyIntentBorder;

        public void SetPersistentColor(Color color) { if (baseImage != null) baseImage.color = color; }
        public void SetOverlay(CellOverlayKind kind, bool visible)
        {
            Image target = kind switch
            {
                CellOverlayKind.MoveCandidate => moveCandidateBorder,
                CellOverlayKind.PlayerActionRange => playerActionBorder,
                CellOverlayKind.EnemyIntentRange => enemyIntentBorder,
                _ => null
            };
            if (target != null) target.gameObject.SetActive(visible);
        }

        public bool ValidateReferences() => baseImage != null && moveCandidateBorder != null &&
            playerActionBorder != null && enemyIntentBorder != null;
    }
}
