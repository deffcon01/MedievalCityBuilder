using System;
using System.Collections.Generic;

namespace MedievalCityBuilder.Zoning
{
    /// <summary>
    /// Immutable identifier for a zone type. Id 0 represents "None" (no zone).
    /// </summary>
    public readonly record struct ZoneTypeId(int Value)
    {
        public static ZoneTypeId None => new ZoneTypeId(0);

        public bool IsNone => Value == 0;

        public override string ToString() => $"Z{Value}";
    }

    /// <summary>
    /// Category of a zoning definition.
    /// </summary>
    public enum ZoneCategory
    {
        Residential,
        Commercial,
        Industrial,
        MixedUse
    }

    /// <summary>
    /// Definition of a zone type.
    /// </summary>
    public sealed record ZoneDefinition
    {
        public ZoneTypeId Id { get; }
        public string Name { get; }
        public ZoneCategory Category { get; }

        public ZoneDefinition(ZoneTypeId id, string name, ZoneCategory category)
        {
            if (id.IsNone)
                throw new ArgumentException("Id must not be None.", nameof(id));
            if (name == null || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must not be null, empty, or whitespace.", nameof(name));

            Id = id;
            Name = name;
            Category = category;
        }
    }

    /// <summary>
    /// Registry of zone definitions. Backed by a SortedDictionary for ordered lookup.
    /// </summary>
    public sealed class ZoneRegistry
    {
        private readonly SortedDictionary<int, ZoneDefinition> _byId = new();
        private readonly Dictionary<string, ZoneDefinition> _byName = new(StringComparer.OrdinalIgnoreCase);

        public ZoneRegistry()
        {
        }

        /// <summary>
        /// Registers a zone definition.
        /// </summary>
        public ZoneRegistry Register(ZoneDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (_byId.ContainsKey(definition.Id.Value))
                throw new ArgumentException($"Duplicate Id: {definition.Id.Value}.", nameof(definition));

            if (_byName.ContainsKey(definition.Name))
                throw new ArgumentException($"Duplicate Name: {definition.Name}.", nameof(definition));

            _byId[definition.Id.Value] = definition;
            _byName[definition.Name] = definition;
            return this;
        }

        public ZoneDefinition Get(ZoneTypeId id)
        {
            if (id.IsNone)
                throw new ArgumentException($"Unknown zone id: {id.Value}.", nameof(id));
            if (!_byId.TryGetValue(id.Value, out var def))
                throw new ArgumentException($"Unknown zone id: {id.Value}.", nameof(id));
            return def;
        }

        public bool TryGet(ZoneTypeId id, out ZoneDefinition definition)
        {
            if (id.IsNone)
            {
                definition = null!;
                return false;
            }
            ZoneDefinition? temp = null;
            bool found = _byId.TryGetValue(id.Value, out temp);
            definition = temp!;
            return found;
        }

        public bool IsRegistered(ZoneTypeId id)
        {
            if (id.IsNone) return false;
            return _byId.ContainsKey(id.Value);
        }

        public IReadOnlyList<ZoneDefinition> Definitions
        {
            get
            {
                var list = new List<ZoneDefinition>(_byId.Count);
                foreach (var def in _byId.Values)
                    list.Add(def);
                return list;
            }
        }

        public int Count => _byId.Count;

        /// <summary>
        /// Creates a registry with the starting zone definitions from DESIGN.md section 5.
        /// This list is a placeholder until zone definitions load from data files.
        /// </summary>
        public static ZoneRegistry CreateStartingRegistry()
        {
            var registry = new ZoneRegistry();
            registry.Register(new ZoneDefinition(new ZoneTypeId(1), "Small Residential", ZoneCategory.Residential));
            registry.Register(new ZoneDefinition(new ZoneTypeId(2), "Multifamily Residential", ZoneCategory.Residential));
            registry.Register(new ZoneDefinition(new ZoneTypeId(3), "Small Open Market", ZoneCategory.Commercial));
            registry.Register(new ZoneDefinition(new ZoneTypeId(4), "Light Commercial", ZoneCategory.Commercial));
            registry.Register(new ZoneDefinition(new ZoneTypeId(5), "Light Industry", ZoneCategory.Industrial));
            registry.Register(new ZoneDefinition(new ZoneTypeId(6), "Residential-Agriculture", ZoneCategory.MixedUse));
            registry.Register(new ZoneDefinition(new ZoneTypeId(7), "Residential-Industry", ZoneCategory.MixedUse));
            registry.Register(new ZoneDefinition(new ZoneTypeId(8), "Residential-Commercial", ZoneCategory.MixedUse));
            return registry;
        }
    }
}
