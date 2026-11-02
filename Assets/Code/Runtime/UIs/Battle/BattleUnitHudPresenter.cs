using System.Collections.Generic;
using TestCardGame.BoardManage;
using TestCardGame.Character;
using TestCardGame.Character.ValueObjects;
using TestCardGame.Controller;
using UnityEngine;

namespace TestCardGame.UIs.Battle
{
    /// <summary>既存HUDモデルをIDでUnitViewへ結び付け、Canvas上のバーと共有説明を更新する。</summary>
    public sealed class BattleUnitHudPresenter : MonoBehaviour
    {
        [SerializeField] private GameController controller;
        [SerializeField] private BattleUI battleUi;
        [SerializeField] private CellBuilder cellBuilder;
        [SerializeField] private RectTransform overlayRoot;
        [SerializeField] private RectTransform boardViewport;
        [SerializeField] private UnitOverheadBarsView barsPrefab;
        [SerializeField] private UnitHoverDetailsView hoverDetails;
        [SerializeField] private CommonUiController commonUi;
        [SerializeField] private TooltipView cellTooltip;

        private sealed class Entry
        {
            public UnitView Unit;
            public UnitHudHoverTarget Target;
            public UnitOverheadBarsView Bars;
            public UnitHudViewModel Model;
        }

        private readonly Dictionary<UnitID, Entry> entries = new();
        private readonly HashSet<UnitID> liveIds = new();
        private readonly List<UnitID> removedIds = new();
        private readonly Vector3[] corners = new Vector3[4];
        private UnitHudHoverTarget hovered;
        private bool cardPreviewActive;
        private Canvas canvas;

        /// <summary>既存のドラッグPreview通知だけを購読し、Legacy Inputを追加しない。</summary>
        private void OnEnable()
        {
            if (controller != null)
            {
                controller.CardPreviewStarted += OnPreviewStarted;
                controller.CardPreviewEnded += OnPreviewEnded;
            }
            if (hoverDetails != null) hoverDetails.Hide();
        }

        /// <summary>状態取得はBattleHudPresenterに任せ、通常・召喚・再構築の同じ対応表へ反映する。</summary>
        public void Render(BattleHudViewModel model)
        {
            if (model == null || cellBuilder == null || overlayRoot == null || barsPrefab == null) return;
            liveIds.Clear();
            Bind(model.Player, cellBuilder.PlayerUnitView);
            foreach (UnitHudViewModel enemy in model.Enemies)
                if (cellBuilder.EnemyUnitViews.TryGetValue(enemy.UnitId, out UnitView unit)) Bind(enemy, unit);
            removedIds.Clear();
            foreach (var pair in entries)
                if (!liveIds.Contains(pair.Key)) removedIds.Add(pair.Key);
            foreach (UnitID id in removedIds) Remove(id);
        }

        /// <summary>同じIDでもステージ再構築でViewが変わった場合は古い購読とバーを破棄する。</summary>
        private void Bind(UnitHudViewModel model, UnitView unit)
        {
            if (model == null || model.IsDefeated || unit == null || !unit.gameObject.activeInHierarchy) return;
            liveIds.Add(model.UnitId);
            if (entries.TryGetValue(model.UnitId, out Entry entry) && entry.Unit != unit)
            {
                Remove(model.UnitId);
                entry = null;
            }
            if (entry == null)
            {
                var target = unit.GetComponent<UnitHudHoverTarget>();
                if (target == null) target = unit.gameObject.AddComponent<UnitHudHoverTarget>();
                entry = new Entry { Unit = unit, Target = target, Bars = Instantiate(barsPrefab, overlayRoot) };
                entry.Bars.name = $"UnitBars_{model.UnitId.Code}";
                entries.Add(model.UnitId, entry);
                target.Configure(OnHovered);
            }
            entry.Model = model;
            entry.Bars.Render(model);
        }

        /// <summary>移動・攻撃アニメーションとCanvasサイズ変更後の実寸へ毎フレーム追従する。</summary>
        private void LateUpdate()
        {
            if (overlayRoot == null || boardViewport == null || hoverDetails == null) return;
            canvas ??= overlayRoot.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            boardViewport.GetWorldCorners(corners);
            Vector2 bottom = ToLocal(corners[0], camera);
            Vector2 top = ToLocal(corners[2], camera);
            Rect boardBounds = Rect.MinMaxRect(bottom.x, bottom.y, top.x, top.y);
            bool blocked = cardPreviewActive || battleUi == null || battleUi.IsUnitHoverBlocked
                || (commonUi != null && commonUi.SettingsModal != null && commonUi.SettingsModal.IsOpen);
            if (blocked) CloseHover();
            Entry hoverEntry = null;
            foreach (Entry entry in entries.Values)
            {
                bool visible = entry.Unit != null && entry.Unit.gameObject.activeInHierarchy && !entry.Model.IsDefeated;
                entry.Bars.gameObject.SetActive(visible);
                if (!visible)
                {
                    if (entry.Target == hovered) CloseHover();
                    continue;
                }
                ((RectTransform)entry.Unit.transform).GetWorldCorners(corners);
                Vector2 left = ToLocal(corners[0], camera);
                Vector2 right = ToLocal(corners[3], camera);
                float width = Mathf.Clamp(right.x - left.x, 30f, Mathf.Max(30f, boardBounds.width - 8f));
                Vector2 position = new Vector2((left.x + right.x) * 0.5f, Mathf.Min(left.y, right.y) - 12f);
                position.x = Mathf.Clamp(position.x, boardBounds.xMin + width * 0.5f + 2f, boardBounds.xMax - width * 0.5f - 2f);
                position.y = Mathf.Clamp(position.y, boardBounds.yMin + 10f, boardBounds.yMax - 10f);
                entry.Bars.Place(position, width);
                if (!blocked && entry.Target == hovered) hoverEntry = entry;
            }
            if (hoverEntry == null) { hoverDetails.Hide(); return; }
            cellTooltip?.Hide();
            string intent = BuildIntent(hoverEntry);
            ((RectTransform)hoverEntry.Unit.transform).GetWorldCorners(corners);
            Vector2 anchor = ToLocal(corners[2], camera);
            hoverDetails.Show(hoverEntry.Model, intent, anchor, overlayRoot.rect);
        }

        /// <summary>Canvasの描画方式に対応した画面座標から共通オーバーレイ座標へ変換する。</summary>
        private Vector2 ToLocal(Vector3 world, Camera camera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRoot,
                RectTransformUtility.WorldToScreenPoint(camera, world), camera, out Vector2 local);
            return local;
        }

        /// <summary>既存の副作用なしの敵予告から、公開済み情報だけを説明する。</summary>
        private string BuildIntent(Entry entry)
        {
            if (!cellBuilder.EnemyUnitViews.ContainsKey(entry.Model.UnitId)) return string.Empty;
            var preview = entry.Unit.GetComponent<EnemyIntentView>()?.Preview;
            if (preview == null) return "未設定";
            if (!preview.CanExecute) return string.IsNullOrEmpty(preview.UnavailableReason) ? "実行不可" : preview.UnavailableReason;
            string kind = preview.Kind switch
            {
                TestCardGame.Character.Enemies.EnemyIntentKind.Attack => "攻撃",
                TestCardGame.Character.Enemies.EnemyIntentKind.StatusEffect => "状態異常",
                _ => "その他"
            };
            return $"{kind} / 対象 ({preview.TargetPosition.x}, {preview.TargetPosition.y}) / 範囲 {preview.AffectedCells.Count}マス";
        }

        /// <summary>EventSystemの出入りだけで対象を切り替え、セル説明との重複を抑える。</summary>
        private void OnHovered(UnitHudHoverTarget target, bool entered)
        {
            if (entered) { hovered = target; cellTooltip?.Hide(); }
            else if (hovered == target) CloseHover();
        }

        private void OnPreviewStarted() { cardPreviewActive = true; CloseHover(); cellTooltip?.Hide(); }
        private void OnPreviewEnded() => cardPreviewActive = false;
        private void CloseHover() { hovered = null; if (hoverDetails != null) hoverDetails.Hide(); }

        /// <summary>破棄・死亡したユニットのコールバックと表示を解除する。</summary>
        private void Remove(UnitID id)
        {
            Entry entry = entries[id];
            if (entry.Target == hovered) CloseHover();
            if (entry.Target != null) entry.Target.Configure(null);
            if (entry.Bars != null) Destroy(entry.Bars.gameObject);
            entries.Remove(id);
        }

        /// <summary>退出時にすべての購読と動的表示を残さず解除する。</summary>
        private void OnDisable()
        {
            if (controller != null)
            {
                controller.CardPreviewStarted -= OnPreviewStarted;
                controller.CardPreviewEnded -= OnPreviewEnded;
            }
            CloseHover();
            foreach (Entry entry in entries.Values)
            {
                if (entry.Target != null) entry.Target.Configure(null);
                if (entry.Bars != null) Destroy(entry.Bars.gameObject);
            }
            entries.Clear();
            cardPreviewActive = false;
        }

        public bool ValidateReferences() => controller != null && battleUi != null && cellBuilder != null
            && overlayRoot != null && boardViewport != null && barsPrefab != null && barsPrefab.ValidateReferences()
            && hoverDetails != null && hoverDetails.ValidateReferences() && commonUi != null;
    }
}
