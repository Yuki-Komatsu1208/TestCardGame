using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.Battle
{
    /// <summary>HPとスタン耐性の細い2段バーを表示する。</summary>
    public sealed class UnitOverheadBarsView : MonoBehaviour
    {
        [SerializeField] private Image hpFill;
        [SerializeField] private Image stunFill;
        private RectTransform rect;

        /// <summary>既存の表示モデルから0～1へ制限した残量を反映する。</summary>
        public void Render(UnitHudViewModel model)
        {
            if (model == null) { gameObject.SetActive(false); return; }
            if (hpFill != null) hpFill.fillAmount = Mathf.Clamp01(model.HpRatio);
            if (stunFill != null) stunFill.fillAmount = Mathf.Clamp01(model.StunRatio);
            gameObject.SetActive(!model.IsDefeated);
        }

        /// <summary>Canvas上のユニット下端へ位置と幅を合わせる。</summary>
        public void Place(Vector2 position, float width)
        {
            rect ??= GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        /// <summary>画像未設定で残量表示が効かなくなる不具合を検出する。</summary>
        public bool ValidateReferences() => ValidFill(hpFill) && ValidFill(stunFill);
        private static bool ValidFill(Image image) => image != null && image.sprite != null
            && image.type == Image.Type.Filled && image.fillMethod == Image.FillMethod.Horizontal
            && image.fillOrigin == (int)Image.OriginHorizontal.Left && !image.raycastTarget;
    }
}
