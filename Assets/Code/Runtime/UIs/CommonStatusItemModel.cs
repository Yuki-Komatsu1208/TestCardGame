namespace TestCardGame.UIs
{
    /// <summary>上部ステータス欄へ表示する一項目分の表示専用データ。</summary>
    public readonly struct CommonStatusItemModel
    {
        public string Label { get; }
        public string Value { get; }

        public CommonStatusItemModel(string label, string value)
        {
            Label = label ?? string.Empty;
            Value = value ?? string.Empty;
        }
    }
}
