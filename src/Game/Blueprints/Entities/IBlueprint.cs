//using Mafi;
//using Mafi.Collections;
//using Mafi.Collections.ImmutableCollections;
//using Mafi.Core.Entities;
//using Mafi.Core.Prototypes;
//using System.Collections.Generic;

//namespace DoubleQoL.Game.Blueprints.Entities {
//    public interface IBlueprint : IBlueprintItem {
//        string GameVersion { get; }

//        int SaveVersion { get; }

//        ImmutableArray<EntityConfigData> Items { get; }

//        Option<string> ProtosThatFailedToLoad { get; }

//        ImmutableArray<KeyValuePair<Proto, int>> MostFrequentProtos { get; }

//        Set<Proto> AllDistinctProtos { get; }
//    }
//}