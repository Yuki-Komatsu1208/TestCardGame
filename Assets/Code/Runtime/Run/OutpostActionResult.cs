namespace TestCardGame.Run
{
    public enum OutpostFailureReason
    {
        None,
        InvalidRunState,
        InvalidDefinition,
        InvalidProduct,
        SoldOut,
        KeystoneAlreadyOwned,
        FullHealth,
        InsufficientGold,
        NoExpedition
    }

    /// <summary>Outpost操作の成否とプレイヤー向け理由を返す不変値。</summary>
    public readonly struct OutpostActionResult
    {
        public bool Succeeded { get; }
        public OutpostFailureReason FailureReason { get; }
        public string PlayerMessage { get; }
        public int Cost { get; }

        private OutpostActionResult(bool succeeded, OutpostFailureReason reason, string message, int cost)
        { Succeeded = succeeded; FailureReason = reason; PlayerMessage = message ?? string.Empty; Cost = cost; }

        public static OutpostActionResult Success(int cost = 0) => new(true, OutpostFailureReason.None, string.Empty, cost);
        public static OutpostActionResult Failure(OutpostFailureReason reason, string message, int cost = 0) => new(false, reason, message, cost);
    }
}
