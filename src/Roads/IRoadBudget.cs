namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// Budget for road construction.
    /// </summary>
    public interface IRoadBudget
    {
        /// <summary>
        /// Checks if the budget can afford the given cost.
        /// </summary>
        bool CanAfford(RoadCost cost);

        /// <summary>
        /// Attempts to spend the given cost. Deducts and returns true only if the full cost is affordable.
        /// Otherwise returns false and changes nothing.
        /// A null budget in the controller means roads are free.
        /// </summary>
        bool TrySpend(RoadCost cost);
    }
}
