//using Mafi;
//using Mafi.Collections;
//using Mafi.Collections.ImmutableCollections;
//using Mafi.Collections.ReadonlyCollections;
//using Mafi.Core.Entities;
//using Mafi.Core.Prototypes;
//using Mafi.Serialization;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace DoubleQoL.Game.Blueprints.Entities {
//    internal class BlueprintsFolder : IBlueprintsFolder, IBlueprintItem, IBlueprintItemFriend {
//        private readonly Lyst<IBlueprintsFolder> m_folders;
//        private readonly Lyst<IBlueprint> m_blueprints;

//        public bool IsEmpty => this.m_folders.IsEmpty && this.m_blueprints.IsEmpty;

//        public string Name { get; private set; }

//        public string Desc { get; private set; }

//        public Option<IBlueprintsFolder> ParentFolder { get; private set; }

//        public IIndexable<IBlueprintsFolder> Folders => (IIndexable<IBlueprintsFolder>)this.m_folders;

//        public IIndexable<IBlueprint> Blueprints => (IIndexable<IBlueprint>)this.m_blueprints;

//        public Lyst<Proto> PreviewProtos { get; private set; }

//        public BlueprintsFolder(string name) {
//            this.PreviewProtos = new Lyst<Proto>();
//            this.Name = name;
//            this.Desc = "";
//            this.m_folders = new Lyst<IBlueprintsFolder>();
//            this.m_blueprints = new Lyst<IBlueprint>();
//        }

//        private BlueprintsFolder(
//          string name,
//          string desc,
//          Lyst<IBlueprintsFolder> folders,
//          Lyst<IBlueprint> blueprints) {
//            this.PreviewProtos = new Lyst<Proto>();
//            this.Name = name;
//            this.Desc = desc;
//            this.m_folders = folders;
//            this.m_blueprints = blueprints;
//            foreach (BlueprintsFolder folder in folders)
//                folder.ParentFolder = (Option<IBlueprintsFolder>)this;
//            this.updatePreviewProtos(this.m_blueprints);
//        }

//        internal bool TryAdd(IBlueprint blueprint, bool doNotUpdatePreviews = false) {
//            if (this.Blueprints.Contains<IBlueprint>(blueprint))
//                return false;
//            this.m_blueprints.Add(blueprint);
//            if (!doNotUpdatePreviews)
//                this.updatePreviewProtos(this.m_blueprints);
//            return true;
//        }

//        internal bool Remove(IBlueprint blueprint) {
//            if (!this.m_blueprints.Remove(blueprint))
//                return false;
//            this.updatePreviewProtos(this.m_blueprints);
//            return true;
//        }

//        /// <summary>
//        /// NOTE: newIndex is index in a list that contains all the folders and then all the blueprints.
//        /// </summary>
//        internal bool TryReorderItem(IBlueprintItem blueprint, int newIndex) {
//            switch (blueprint) {
//                case BlueprintsFolder blueprintsFolder:
//                    int index1 = this.Folders.IndexOf<IBlueprintsFolder>((IBlueprintsFolder)blueprintsFolder);
//                    if (index1 < 0 || newIndex < 0 || index1 == newIndex)
//                        return false;
//                    if (index1 < newIndex)
//                        newIndex = (newIndex - 1).Max(0);
//                    this.m_folders.RemoveAt(index1);
//                    if (newIndex >= this.Folders.Count)
//                        this.m_folders.Add((IBlueprintsFolder)blueprintsFolder);
//                    else
//                        this.m_folders.Insert(newIndex, (IBlueprintsFolder)blueprintsFolder);
//                    return true;

//                case Blueprint blueprint1:
//                    newIndex -= this.Folders.Count;
//                    int index2 = this.Blueprints.IndexOf<IBlueprint>((IBlueprint)blueprint1);
//                    if (index2 < 0 || newIndex < 0 || index2 == newIndex)
//                        return false;
//                    if (index2 < newIndex)
//                        newIndex = (newIndex - 1).Max(0);
//                    this.m_blueprints.RemoveAt(index2);
//                    if (newIndex >= this.Blueprints.Count)
//                        this.m_blueprints.Add((IBlueprint)blueprint1);
//                    else
//                        this.m_blueprints.Insert(newIndex, (IBlueprint)blueprint1);
//                    return true;

//                default:
//                    return false;
//            }
//        }

//        internal bool TryAdd(IBlueprintsFolder newFolder) {
//            if (this.Folders.Contains<IBlueprintsFolder>(newFolder))
//                return false;
//            this.m_folders.Add(newFolder);
//            ((BlueprintsFolder)newFolder).SetParentFolder((Option<IBlueprintsFolder>)this);
//            return true;
//        }

//        internal bool Remove(IBlueprintsFolder folder) {
//            if (!this.m_folders.Remove(folder))
//                return false;
//            ((BlueprintsFolder)folder).SetParentFolder((Option<IBlueprintsFolder>)Option.None);
//            return true;
//        }

//        private void updatePreviewProtos(Lyst<IBlueprint> blueprints) {
//            this.PreviewProtos.Clear();
//            Dict<Proto, int> source = new Dict<Proto, int>();
//            Lyst<IBlueprint>.Enumerator enumerator = blueprints.GetEnumerator();
//        label_4:
//            while (enumerator.MoveNext()) {
//                IBlueprint current = enumerator.Current;
//                int num1 = 0;
//                while (true) {
//                    int num2 = num1;
//                    ImmutableArray<KeyValuePair<Proto, int>> mostFrequentProtos = current.MostFrequentProtos;
//                    int num3 = mostFrequentProtos.Length.Min(1);
//                    if (num2 < num3) {
//                        Dict<Proto, int> dict = source;
//                        mostFrequentProtos = current.MostFrequentProtos;
//                        Proto key = mostFrequentProtos[0].Key;
//                        dict.IncOrInsert1<Proto>(key);
//                        ++num1;
//                    }
//                    else
//                        goto label_4;
//                }
//            }
//            this.PreviewProtos.AddRange(source.OrderByDescending<KeyValuePair<Proto, int>, int>((Func<KeyValuePair<Proto, int>, int>)(x => x.Value)).Take<KeyValuePair<Proto, int>>(source.Count.Min(3)).Select<KeyValuePair<Proto, int>, Proto>((Func<KeyValuePair<Proto, int>, Proto>)(x => x.Key)));
//        }

//        public void SetName(string name) => this.Name = name;

//        public void SetDescription(string desc) => this.Desc = desc;

//        internal void MergeAllItemsFrom(BlueprintsFolder folderToMergeFrom) {
//            foreach (IBlueprintsFolder folder in folderToMergeFrom.m_folders)
//                Assert.That<bool>(this.TryAdd(folder)).IsTrue();
//            foreach (IBlueprint blueprint in folderToMergeFrom.m_blueprints)
//                Assert.That<bool>(this.TryAdd(blueprint, true)).IsTrue();
//            folderToMergeFrom.m_folders.Clear();
//            folderToMergeFrom.m_blueprints.Clear();
//        }

//        internal void SetParentFolder(Option<IBlueprintsFolder> parent) => this.ParentFolder = parent;

//        public IBlueprintsFolder CreateCopyForSave() => (IBlueprintsFolder)new BlueprintsFolder(this.Name, this.Desc, ((IEnumerable<IBlueprintsFolder>)this.m_folders.Select<IBlueprintsFolder>((Func<IBlueprintsFolder, IBlueprintsFolder>)(x => ((BlueprintsFolder)x).CreateCopyForSave()))).ToLyst<IBlueprintsFolder>(), ((IEnumerable<IBlueprint>)this.m_blueprints.Select<IBlueprint>((Func<IBlueprint, IBlueprint>)(x => ((Blueprint)x).CreateCopyForSave()))).ToLyst<IBlueprint>());

//        internal void SerializeForBlueprints(BlobWriter writer) {
//            writer.WriteString(this.Name);
//            writer.WriteString(this.Desc);
//            writer.WriteIntNotNegative(this.Folders.Count);
//            foreach (BlueprintsFolder folder in this.Folders)
//                folder.SerializeForBlueprints(writer);
//            writer.WriteIntNotNegative(this.Blueprints.Count);
//            foreach (Blueprint blueprint in this.Blueprints)
//                blueprint.SerializeForBlueprints(writer);
//        }

//        internal static BlueprintsFolder DeserializeForBlueprints(
//          BlobReader reader,
//          ConfigSerializationContext context,
//          int libraryVersion) {
//            string name = reader.ReadString();
//            string desc = reader.ReadString();
//            int capacity1 = reader.ReadIntNotNegative();
//            Lyst<IBlueprintsFolder> folders = new Lyst<IBlueprintsFolder>(capacity1);
//            for (int index = 0; index < capacity1; ++index) {
//                BlueprintsFolder blueprintsFolder = DeserializeForBlueprints(reader, context, libraryVersion);
//                folders.Add((IBlueprintsFolder)blueprintsFolder);
//            }
//            int capacity2 = reader.ReadIntNotNegative();
//            Lyst<IBlueprint> blueprints = new Lyst<IBlueprint>(capacity2);
//            for (int index = 0; index < capacity2; ++index) {
//                Blueprint blueprint = Blueprint.DeserializeForBlueprints(reader, context, libraryVersion);
//                blueprints.Add((IBlueprint)blueprint);
//            }
//            return new BlueprintsFolder(name, desc, folders, blueprints);
//        }
//    }
//}