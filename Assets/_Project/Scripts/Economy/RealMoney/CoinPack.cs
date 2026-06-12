namespace Simcity.Economy.RealMoney
{
    /// <summary>A purchasable bundle of Coins. Coins bought this way are PURCHASED Coins —
    /// spendable in-world but never cashable (GAME_DESIGN §6.3/§6.4). Prices here are
    /// placeholder sandbox values; real pricing + the actual charge happen on the web /
    /// through the store via the payments provider.</summary>
    public struct CoinPack
    {
        public readonly string id;
        public readonly string label;
        public readonly int coins;
        public readonly int priceUsdCents;

        public CoinPack(string id, string label, int coins, int priceUsdCents)
        {
            this.id = id;
            this.label = label;
            this.coins = coins;
            this.priceUsdCents = priceUsdCents;
        }

        public string PriceLabel => $"${priceUsdCents / 100f:0.00}";
    }

    public static class CoinPackCatalog
    {
        public static readonly CoinPack[] Packs =
        {
            new CoinPack("starter", "Starter", 500, 499),
            new CoinPack("plus",    "Plus",    1200, 999),
            new CoinPack("pro",     "Pro",     3000, 1999),
        };
    }
}
