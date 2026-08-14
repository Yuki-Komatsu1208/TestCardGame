using System.Collections.Generic;
using System.Linq;

namespace TestCardGame.Run
{
    public enum KeystoneId { None, MagicCrystal, KnightShield, Musket }
    public enum BuildTag { Magic, Knight, Gunner }

    /// <summary>キーストーンによる候補の出現重みを一箇所に集約する。</summary>
    public static class BuildWeightService
    {
        public const int FavoredTagWeight = 3;

        public static int GetWeight(IEnumerable<BuildTag> candidateTags, RunState runState)
        {
            if (runState?.favoredBuildTags == null || candidateTags == null) return 1;
            return candidateTags.Any(tag => runState.favoredBuildTags.Contains(tag)) ? FavoredTagWeight : 1;
        }
    }
}
