using System;

namespace MedievalCityBuilder.Roads
{
    /// <summary>
    /// The gold and stone cost of a road construction or upkeep operation.
    /// All arithmetic is whole-number (long) and checked; overflow throws OverflowException.
    /// </summary>
    public readonly record struct RoadCost(long Gold, long Stone)
    {
        /// <summary>
        /// A cost of zero gold and zero stone.
        /// </summary>
        public static readonly RoadCost Zero = new RoadCost(0, 0);

        /// <summary>
        /// Adds two RoadCost values with checked arithmetic.
        /// </summary>
        /// <exception cref="OverflowException">Thrown when either component overflows long.</exception>
        public static RoadCost Add(RoadCost a, RoadCost b)
        {
            long gold;
            long stone;
            checked
            {
                gold = a.Gold + b.Gold;
                stone = a.Stone + b.Stone;
            }
            return new RoadCost(gold, stone);
        }

        /// <summary>
        /// Adds two RoadCost values with checked arithmetic.
        /// </summary>
        /// <exception cref="OverflowException">Thrown when either component overflows long.</exception>
        public static RoadCost operator +(RoadCost a, RoadCost b)
        {
            return Add(a, b);
        }
    }
}