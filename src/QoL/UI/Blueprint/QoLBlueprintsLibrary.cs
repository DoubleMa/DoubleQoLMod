using Mafi;
using Mafi.Collections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;
using System;
using System.Linq;

namespace DoubleQoL.QoL.UI.Blueprint {

    public class QoLBlueprintsLibrary : BlueprintsLibrary {
        private readonly Lyst<string> SavedRoot;
        private readonly Lyst<IBlueprintsFolder> SavedFolders;
        private readonly Lyst<IBlueprint> SavedBlueprints;

        public QoLBlueprintsLibrary(IFileSystemHelper fileSystemHelper, ConfigSerializationContext serializationContext) : base(fileSystemHelper, serializationContext) {
            SavedRoot = new Lyst<string>();
            SavedBlueprints = new Lyst<IBlueprint>();
            SavedFolders = new Lyst<IBlueprintsFolder>();
        }

        public void Clear() {
            foreach (var blueprint in Root.Blueprints.AsEnumerable().ToList()) DeleteItem(Root, blueprint);
            foreach (var subfolder in Root.Folders.AsEnumerable().ToList()) DeleteItem(Root, subfolder);
        }

        public void SaveCurrentRoot(IBlueprintsFolder root = null) {
            if (root == null) root = Root;
            SavedRoot.Clear();
            SavedFolders.Clear();
            SavedBlueprints.Clear();
            foreach (var subfolder in root.Folders) {
                SavedRoot.Add(ConvertToString(subfolder));
                SavedFolders.Add(subfolder);
            }
            foreach (var blueprint in root.Blueprints) {
                SavedRoot.Add(ConvertToString(blueprint));
                SavedBlueprints.Add(blueprint);
            }
        }

        public void UpdateRoot(Lyst<string> items) {
            Clear();
            foreach (var item in items) TryAddBlueprintFromString(Root, item, out _);
        }

        public void RestoreSavedRoot() => UpdateRoot(SavedRoot);

        public Lyst<string> SearchItems(string searchTerm) {
            var matchingItems = new Lyst<string>();
            SearchInItems(SavedBlueprints, searchTerm, matchingItems);
            SearchInItems(SavedFolders, searchTerm, matchingItems);
            return matchingItems;
        }

        public string ConvertItemToString(IBlueprintItem item) {
            if (item is IBlueprint blueprint) return ConvertToString(blueprint);
            return ConvertToString((IBlueprintsFolder)item);
        }

        private void SearchInItems<T>(IIndexable<T> items, string searchTerm, Lyst<string> matchingItems) where T : IBlueprintItem {
            foreach (var item in items.AsEnumerable()) {
                if (item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    item.Desc.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) {
                    matchingItems.Add(ConvertItemToString(item));
                }
                if (item is IBlueprintsFolder subfolder) {
                    SearchInItems(subfolder.Folders, searchTerm, matchingItems);
                    SearchInItems(subfolder.Blueprints, searchTerm, matchingItems);
                }
            }
        }
    }
}