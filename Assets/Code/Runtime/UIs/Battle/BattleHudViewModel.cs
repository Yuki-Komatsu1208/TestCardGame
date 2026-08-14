using System.Collections.Generic;
using TestCardGame.Character.ValueObjects;

namespace TestCardGame.UIs.Battle
{
    /// <summary>状態異常1件の表示専用データ。</summary>
    public sealed class StatusEffectHudViewModel
    {
        public StatusEffectHudViewModel(string label) => Label = label ?? string.Empty;
        public string Label { get; }
    }

    /// <summary>ユニットHUDの表示専用データ。</summary>
    public sealed class UnitHudViewModel
    {
        public UnitHudViewModel(UnitID unitId, string name, int currentHp, int maxHp, int currentStun,
            int maxStun, IReadOnlyList<StatusEffectHudViewModel> statuses, bool isDefeated, int currentMana = 0,
            int maxMana = 0)
        {
            UnitId = unitId;
            Name = name ?? string.Empty;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            CurrentStun = currentStun;
            MaxStun = maxStun;
            Statuses = statuses ?? new List<StatusEffectHudViewModel>();
            IsDefeated = isDefeated;
            CurrentMana = currentMana;
            MaxMana = maxMana;
        }

        public UnitID UnitId { get; }
        public string Name { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int CurrentStun { get; }
        public int MaxStun { get; }
        public IReadOnlyList<StatusEffectHudViewModel> Statuses { get; }
        public bool IsDefeated { get; }
        public int CurrentMana { get; }
        public int MaxMana { get; }
        public float HpRatio => MaxHp > 0 ? (float)CurrentHp / MaxHp : 0f;
        public float StunRatio => MaxStun > 0 ? (float)CurrentStun / MaxStun : 0f;
    }

    /// <summary>Battle常設HUD全体の表示専用データ。</summary>
    public sealed class BattleHudViewModel
    {
        public UnitHudViewModel Player { get; set; }
        public IReadOnlyList<UnitHudViewModel> Enemies { get; set; } = new List<UnitHudViewModel>();
        public string TurnLabel { get; set; } = string.Empty;
        public string ExpeditionLabel { get; set; } = string.Empty;
        public string StageLabel { get; set; } = string.Empty;
        public bool IsOverHunt { get; set; }
        public int OverHuntDepth { get; set; }
        public int PendingGold { get; set; }
        public bool CanEndTurn { get; set; }
        public bool CanOpenDeck { get; set; }
    }
}
