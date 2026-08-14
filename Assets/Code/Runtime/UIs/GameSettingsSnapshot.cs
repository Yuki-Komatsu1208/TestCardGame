namespace TestCardGame.UIs
{
    /// <summary>設定画面へ渡すプレイヤー設定の読み取り専用スナップショット。</summary>
    public readonly struct GameSettingsSnapshot
    {
        public float MasterVolume { get; }
        public float BgmVolume { get; }
        public float SeVolume { get; }
        public int ResolutionWidth { get; }
        public int ResolutionHeight { get; }
        public bool IsFullScreen { get; }
        public bool IsVSyncEnabled { get; }

        public GameSettingsSnapshot(float masterVolume, float bgmVolume, float seVolume,
            int resolutionWidth, int resolutionHeight, bool isFullScreen, bool isVSyncEnabled)
        {
            MasterVolume = masterVolume;
            BgmVolume = bgmVolume;
            SeVolume = seVolume;
            ResolutionWidth = resolutionWidth;
            ResolutionHeight = resolutionHeight;
            IsFullScreen = isFullScreen;
            IsVSyncEnabled = isVSyncEnabled;
        }
    }

    /// <summary>設定画面で選択できる幅と高さだけの解像度表示データ。</summary>
    public readonly struct DisplayResolutionOption
    {
        public int Width { get; }
        public int Height { get; }
        public string Label => $"{Width} × {Height}";

        public DisplayResolutionOption(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
