using System.Collections.Generic;
using System.Linq;

namespace TestCardGame.CommonBase
{
    /// <summary>
    /// ValueObjectの基底クラス
    /// </summary>
    public  abstract class VOBase
    {
        protected abstract IEnumerable<object?> GetEqualityComponents();
        public override bool Equals(object? obj)
        {
            if (obj is not VOBase other)
            {
                return false;
            }

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }
        public static bool operator ==(VOBase a,VOBase b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }
        public static bool operator !=(VOBase a,VOBase b) => !(a == b);
        public override string ToString()
        {
            var typeName = GetType().Name;
            var properties = GetType().GetProperties();
            var propertyValues = properties.Select(p => $"{p.Name}: {p.GetValue(this)}");
            return $"{typeName} {{ {string.Join(", ", propertyValues)} }}";
        }
    }
}
