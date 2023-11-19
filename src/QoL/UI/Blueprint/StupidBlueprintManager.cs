using DoubleQoL.Config;
using Mafi.Core.Entities.Blueprints;
using System.Collections.Generic;

namespace DoubleQoL.QoL.UI.Blueprint {

    internal class StupidBlueprintManager {

        private class PlayerUpload {
            public IBlueprintItem Item { get; set; }
            public BlueprintService.BlueprintData Data { get; set; }
        }

        private class PlayerRoot : PlayerUpload {
            public readonly List<PlayerUpload> uploads = new List<PlayerUpload>();

            public void Add(PlayerUpload playerUpload) {
                uploads.Add(playerUpload);
                Data.Like_count += playerUpload.Data.Like_count;
                Data.Download_count += playerUpload.Data.Download_count;
            }

            public void Add(IBlueprintItem item, BlueprintService.BlueprintData data) => Add(new PlayerUpload() { Item = item, Data = data });

            public PlayerUpload Find(IBlueprintItem item) => uploads.Find(e => e.Item == item);

            public PlayerUpload Find(BlueprintService.BlueprintData data) => uploads.Find(e => e.Data.Id == data.Id);

            public void Remove(IBlueprintItem item) {
                var playerUpload = Find(item);
                if (playerUpload == null) return;
                uploads.Remove(playerUpload);
            }

            public void Remove(BlueprintService.BlueprintData data) {
                var playerUpload = Find(data);
                if (playerUpload == null) return;
                uploads.Remove(playerUpload);
            }

            public void Clear() => uploads.Clear();
        }

        private readonly List<PlayerRoot> roots_map;

        public StupidBlueprintManager() {
            roots_map = new List<PlayerRoot>();
        }

        public bool GetRootItem(int ownerId, out IBlueprintItem item) {
            item = roots_map.Find(e => e.Data.Owner_id == ownerId)?.Item;
            return item != null;
        }

        private PlayerRoot Find(int ownerId) => roots_map.Find(e => e.Data.Owner_id == ownerId);

        //private PlayerRoot Find(BlueprintService.BlueprintData rootData) => roots_map.Find(e => e.Data == rootData);

        private PlayerRoot Find(IBlueprintItem item) => roots_map.Find(e => e.Item == item);

        //private PlayerRoot FindRoot(int ownerId) => Find(ownerId);

        //private PlayerRoot FindRoot(IBlueprintItem item) => roots_map.Find(e => e.Find(item) != null);

        private PlayerRoot FindRoot(BlueprintService.BlueprintData itemData) => roots_map.Find(e => e.Find(itemData) != null);

        public bool IsRoot(IBlueprintItem item) => Find(item) != null;

        public bool IsUpload(IBlueprintItem item) => GetItemData(item, out _);

        public bool GetItemData(IBlueprintItem item, out BlueprintService.BlueprintData itemData) {
            itemData = null;
            if (item == null) return false;
            foreach (var root in roots_map) {
                if (root.Item == item) {
                    itemData = root.Data;
                    return true;
                }
                var find = root.Find(item);
                if (find == null) continue;
                itemData = find.Data;
                return true;
            }
            return false;
        }

        public bool IsOwned(IBlueprintItem item, ServerInfo server) {
            if (GetItemData(item, out var data)) return data.Owner_id + "" == server.UserId;
            return false;
        }

        public void AddRoot(IBlueprintItem item, BlueprintService.BlueprintData rootData) {
            if (FindRoot(rootData) is null) roots_map.Add(new PlayerRoot() { Item = item, Data = rootData });
        }

        public void AddRoot(IBlueprintItem item, int ownerId) => AddRoot(item, new BlueprintService.BlueprintData { Id = -1, Owner_id = ownerId, Download_count = 0, Like_count = 0 });

        public void AddItem(IBlueprintItem item, BlueprintService.BlueprintData data) {
            var parent = Find(data.Owner_id);
            if (parent is null) return;
            parent.Add(item, data);
        }

        public void Clear() => roots_map.Clear();
    }
}