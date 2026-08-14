using System;

namespace TestCardGame.Run
{
    public interface IRandomService
    {
        int Range(int minInclusive, int maxExclusive);
    }

    public sealed class SystemRandomService : IRandomService
    {
        private readonly Random random;

        public SystemRandomService()
            : this(new Random())
        {
        }

        public SystemRandomService(Random random)
        {
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            return random.Next(minInclusive, maxExclusive);
        }
    }
}
