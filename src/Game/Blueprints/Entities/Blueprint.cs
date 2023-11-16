//using DoubleQoL.Extensions;
//using Mafi;
//using Mafi.Collections;
//using Mafi.Collections.ImmutableCollections;
//using Mafi.Core.Entities;
//using Mafi.Core.Factory.Transports;
//using Mafi.Core.Factory.Zippers;
//using Mafi.Core.Prototypes;
//using Mafi.Serialization;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace DoubleQoL.Game.Blueprints.Entities {
//    internal class Blueprint : IBlueprint, IBlueprintItem, IBlueprintItemFriend {
//        public string GameVersion { get; }

//        public int SaveVersion { get; }

//        public string Name { get; private set; }

//        public string Desc { get; private set; }

//        public ImmutableArray<EntityConfigData> Items { get; private set; }

//        public Option<string> ProtosThatFailedToLoad { get; private set; }

//        public ImmutableArray<KeyValuePair<Proto, int>> MostFrequentProtos { get; private set; }

//        public Set<Proto> AllDistinctProtos { get; }

//        public Blueprint(string name, ImmutableArray<EntityConfigData> items) {
//            this.MostFrequentProtos = (ImmutableArray<KeyValuePair<Proto, int>>)ImmutableArray.Empty;
//            this.GameVersion = "0.5.5d";
//            this.SaveVersion = 132;
//            this.Name = name;
//            this.Desc = "";
//            this.Items = items;
//            this.MostFrequentProtos = this.getMostFrequentProtos(this.Items);
//            this.AllDistinctProtos = this.getDistinctProtos();
//        }

//        private Blueprint(
//          string gameVersion,
//          int saveVersion,
//          string name,
//          string desc,
//          ImmutableArray<EntityConfigData> items) {
//            this.MostFrequentProtos = (ImmutableArray<KeyValuePair<Proto, int>>)ImmutableArray.Empty;
//            this.GameVersion = gameVersion;
//            this.SaveVersion = saveVersion;
//            this.Name = name;
//            this.Desc = desc;
//            this.Items = items;
//            this.MostFrequentProtos = this.getMostFrequentProtos(this.Items);
//            this.AllDistinctProtos = this.getDistinctProtos();
//            StringBuilder stringBuilder = (StringBuilder)null;
//            Set<string> set = (Set<string>)null;
//            foreach (EntityConfigData entityConfigData in this.Items) {
//                if (entityConfigData.Prototype.IsNone) {
//                    if (set == null)
//                        set = new Set<string>();
//                    if (stringBuilder == null)
//                        stringBuilder = new StringBuilder();
//                    Option<string> option = entityConfigData.ProtoModName;
//                    string str1 = option.ValueOrNull ?? "Core";
//                    option = entityConfigData.InvokeGetter<Option<string>>("ProtoId");
//                    string str2 = option.ValueOrNull ?? "";
//                    if (!set.Contains(str2)) {
//                        set.Add(str2);
//                        stringBuilder.AppendLine(str2 + " (" + str1 + ")");
//                    }
//                }
//            }
//            if (stringBuilder == null)
//                return;
//            this.ProtosThatFailedToLoad = (Option<string>)stringBuilder.ToString();
//        }

//        private Set<Proto> getDistinctProtos() => this.Items.Where((Func<EntityConfigData, bool>)(x => x.Prototype.HasValue)).Select<EntityConfigData, Proto>((Func<EntityConfigData, Proto>)(x => x.Prototype.Value)).ToSet<Proto>();

//        [MustUseReturnValue]
//        private ImmutableArray<KeyValuePair<Proto, int>> getMostFrequentProtos(
//          ImmutableArray<EntityConfigData> items) {
//            Dict<Proto, int> dict = new Dict<Proto, int>();
//            bool flag1 = false;
//            bool flag2 = false;
//            foreach (EntityConfigData entityConfigData in items) {
//                Proto valueOrNull = entityConfigData.Prototype.ValueOrNull;
//                if (!(valueOrNull == (Proto)null) && !(valueOrNull is MiniZipperProto)) {
//                    dict.IncOrInsert1<Proto>(entityConfigData.Prototype.Value);
//                    bool flag3 = valueOrNull is TransportProto || valueOrNull is ZipperProto;
//                    flag1 |= !flag3;
//                    flag2 = ((flag2 ? 1 : 0) | (flag3 ? 0 : (valueOrNull.Id.Value.Contains("SmokeStack") ? 1 : 0))) != 0;
//                }
//            }
//            if (flag1)
//                dict.RemoveKeys((Predicate<Proto>)(x => {
//                    bool mostFrequentProtos;
//                    switch (x) {
//                        case TransportProto _:
//                        case ZipperProto _:
//                            mostFrequentProtos = true;
//                            break;

//                        default:
//                            mostFrequentProtos = false;
//                            break;
//                    }
//                    return mostFrequentProtos;
//                }));
//            if (flag2)
//                dict.RemoveKeys((Predicate<Proto>)(x => x.Id.Value.Contains("SmokeStack")));
//            return dict.OrderByDescending<KeyValuePair<Proto, int>, int>((Func<KeyValuePair<Proto, int>, int>)(x => x.Value)).ToImmutableArray<KeyValuePair<Proto, int>>();
//        }

//        public void SetName(string name) => this.Name = name;

//        public void SetDescription(string desc) => this.Desc = desc;

//        public IBlueprint CreateCopyForSave() => (IBlueprint)new Blueprint(this.GameVersion, this.SaveVersion, this.Name, this.Desc, this.Items);

//        internal void SerializeForBlueprints(BlobWriter writer) {
//            writer.WriteString(this.GameVersion);
//            writer.WriteIntNotNegative(this.SaveVersion);
//            writer.WriteString(this.Name);
//            writer.WriteString(this.Desc);
//            writer.WriteIntNotNegative(this.Items.Length);
//            foreach (EntityConfigData entityConfigData in this.Items)
//                entityConfigData.InvokeMethod("SerializeForBlueprints", writer);
//        }

//        internal static Blueprint DeserializeForBlueprints(BlobReader reader, ConfigSerializationContext context, int libraryVersion) {
//            string gameVersion = reader.ReadString();
//            int saveVersion = reader.ReadIntNotNegative();
//            string name = reader.ReadString();
//            string desc = reader.ReadString();
//            int length = reader.ReadIntNotNegative();
//            ImmutableArrayBuilder<EntityConfigData> immutableArrayBuilder = new ImmutableArrayBuilder<EntityConfigData>(length);
//            for (int i = 0; i < length; ++i)
//                immutableArrayBuilder[i] = typeof(EntityConfigData).InvokeStaticMethod<EntityConfigData>("DeserializeForBlueprints", reader, context);
//            ImmutableArray<EntityConfigData> immutableArrayAndClear = immutableArrayBuilder.GetImmutableArrayAndClear();
//            return new Blueprint(gameVersion, saveVersion, name, desc, immutableArrayAndClear);
//        }
//    }
//}