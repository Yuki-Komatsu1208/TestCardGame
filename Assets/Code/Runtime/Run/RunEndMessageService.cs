using System;

namespace TestCardGame.Run
{
    /// <summary>RUN終了画面で表示中に固定する短文を、注入された乱数から選ぶ。</summary>
    public sealed class RunEndMessageService
    {
        private static readonly string[] DeathButtonLabels =
        {
            "死亡した",
            "目の前が暗くなる",
            "立ち上がれないようだ……"
        };

        private const string VictoryButtonLabel = "新たな傭兵を迎える";

        private readonly IRandomService random;

        public RunEndMessageService(IRandomService randomService)
        {
            random = randomService ?? throw new ArgumentNullException(nameof(randomService));
        }

        public string ChooseDeathButtonLabel() => Choose(DeathButtonLabels);
        public string ChooseVictoryButtonLabel() => VictoryButtonLabel;

        private string Choose(string[] candidates)
        {
            return candidates[random.Range(0, candidates.Length)];
        }
    }
}
