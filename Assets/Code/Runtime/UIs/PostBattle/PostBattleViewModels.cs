using System;

namespace TestCardGame.UIs.PostBattle
{
    public enum RewardDisplayKind
    {
        LevelUp,
        Heal,
        Modifier
    }

    public enum RouteChoiceAction
    {
        ReturnToOutpost,
        ContinueOverHunt
    }

    public enum RunEndKind
    {
        Victory,
        Death,
        FatalError
    }

    /// <summary>報酬枠へ渡す、ゲーム状態を含まない表示専用データ。</summary>
    public sealed class RewardChoiceViewModel
    {
        public RewardChoiceViewModel(int choiceId, string title, string description,
            RewardDisplayKind kind, bool isSelectable, string unavailableReason)
        {
            ChoiceId = choiceId;
            Title = title ?? string.Empty;
            Description = description ?? string.Empty;
            Kind = kind;
            IsSelectable = isSelectable;
            UnavailableReason = unavailableReason ?? string.Empty;
        }

        public int ChoiceId { get; }
        public string Title { get; }
        public string Description { get; }
        public RewardDisplayKind Kind { get; }
        public bool IsSelectable { get; }
        public string UnavailableReason { get; }
    }

    /// <summary>帰還またはOverHunt続行画面へ渡す表示専用データ。</summary>
    public sealed class RouteChoiceViewModel
    {
        public RouteChoiceViewModel(string title, int currentHp, int overHuntDepth,
            int pendingGold, bool canContinue, string continueLabel)
        {
            Title = title ?? string.Empty;
            CurrentHp = currentHp;
            OverHuntDepth = overHuntDepth;
            PendingGold = pendingGold;
            CanContinue = canContinue;
            ContinueLabel = continueLabel ?? string.Empty;
        }

        public string Title { get; }
        public int CurrentHp { get; }
        public int OverHuntDepth { get; }
        public int PendingGold { get; }
        public bool CanContinue { get; }
        public string ContinueLabel { get; }
    }

    /// <summary>RUN終了画面へ渡す表示専用データ。</summary>
    public sealed class RunEndViewModel
    {
        public RunEndViewModel(RunEndKind kind, string message, string buttonLabel)
        {
            Kind = kind;
            Message = message ?? string.Empty;
            ButtonLabel = buttonLabel ?? string.Empty;
        }

        public RunEndKind Kind { get; }
        public string Message { get; }
        public string ButtonLabel { get; }
    }
}
