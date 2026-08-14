using TestCardGame.Run;

namespace TestCardGame.UIs
{
    public readonly struct InnDisplayModel
    {
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int HealedHp { get; }
        public int Cost { get; }
        public bool IsAvailable { get; }
        public string UnavailableReason { get; }
        public InnDisplayModel(int currentHp, int maxHp, int healedHp, int cost, bool isAvailable, string unavailableReason)
        { CurrentHp = currentHp; MaxHp = maxHp; HealedHp = healedHp; Cost = cost; IsAvailable = isAvailable; UnavailableReason = unavailableReason ?? string.Empty; }
    }

    public readonly struct ShopCardDisplayModel
    {
        public int Index { get; }
        public string Name { get; }
        public int Price { get; }
        public bool IsOnSale { get; }
        public bool IsSoldOut { get; }
        public bool IsSelectable { get; }
        public ShopCardDisplayModel(int index, string name, int price, bool isOnSale, bool isSoldOut, bool isSelectable)
        { Index = index; Name = name ?? string.Empty; Price = price; IsOnSale = isOnSale; IsSoldOut = isSoldOut; IsSelectable = isSelectable; }
    }

    public readonly struct KeystoneDisplayModel
    {
        public KeystoneId Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Price { get; }
        public bool IsSoldOut { get; }
        public bool IsUnavailable { get; }
        public bool IsSelectable { get; }
        public KeystoneDisplayModel(KeystoneId id, string name, string description, int price, bool isSoldOut, bool isUnavailable, bool isSelectable)
        { Id = id; Name = name ?? string.Empty; Description = description ?? string.Empty; Price = price; IsSoldOut = isSoldOut; IsUnavailable = isUnavailable; IsSelectable = isSelectable; }
    }
}
