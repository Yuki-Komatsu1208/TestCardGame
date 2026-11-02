using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestCardGame.UIs.Battle
{
    /// <summary>ユニットへのEventSystemホバー通知を表示担当へ伝える。</summary>
    public sealed class UnitHudHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Action<UnitHudHoverTarget, bool> hovered;
        public void Configure(Action<UnitHudHoverTarget, bool> callback) => hovered = callback;
        public void OnPointerEnter(PointerEventData eventData) => hovered?.Invoke(this, true);
        public void OnPointerExit(PointerEventData eventData) => hovered?.Invoke(this, false);
        private void OnDisable() => hovered?.Invoke(this, false);
    }
}
