using DoubleQoL.Config;
using Mafi.Core.Entities.Blueprints;
using System;
using System.Collections.Generic;

namespace DoubleQoL.QoL.UI.Blueprint {

    internal class StupidBlueprintManager {

        internal class PlayerUpload : IComparable<PlayerUpload> {
            public IBlueprintItem Item { get; set; }
            public BlueprintService.BlueprintData Data { get; set; }

            int IComparable<PlayerUpload>.CompareTo(PlayerUpload other) => other == null ? 1 : Data.CompareTo(other.Data);

            public override string ToString() {
                return Data.ToString();
            }
        }

        public enum ItemType {
            Unknown,
            Root,
            Upload
        }

        internal class PlayerRoot : PlayerUpload {
            public readonly List<PlayerUpload> uploads = new List<PlayerUpload>();

            public void Add(PlayerUpload playerUpload) {
                uploads.Add(playerUpload);
                Data.Like_count += playerUpload.Data.Like_count;
                Data.Download_count += playerUpload.Data.Download_count;
            }

            public void Add(IBlueprintItem item, BlueprintService.BlueprintData data) => Add(new PlayerUpload() { Item = item, Data = data });

            public PlayerUpload Find(IBlueprintItem item) => uploads.Find(e => e.Item == item);

            public PlayerUpload Find(string data) => uploads.Find(e => e.Data.Data == data);

            public PlayerUpload Find(BlueprintService.BlueprintData data) => uploads.Find(e => e.Data.Equals(data));

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

            public int Like(PlayerUpload playerUpload, int count) {
                if (playerUpload == null) return 0;
                playerUpload.Data.Like_count += count;
                Data.Like_count += count;
                return playerUpload.Data.Like_count;
            }

            public void Like(PlayerUpload playerUpload) => Like(playerUpload, 1);

            public void Unlike(PlayerUpload playerUpload) => Like(playerUpload, -1);

            public void Clear() => uploads.Clear();
        }

        private readonly List<PlayerRoot> roots_map;
        private readonly Func<IBlueprintItem, string> ConvertItemToString;

        public StupidBlueprintManager(Func<IBlueprintItem, string> convertItemToString) {
            roots_map = new List<PlayerRoot>();
            ConvertItemToString = convertItemToString;
        }

        public bool GetRootItem(int ownerId, out IBlueprintItem item) {
            item = roots_map.Find(e => e.Data.Owner_id == ownerId)?.Item;
            return item != null;
        }

        public PlayerRoot FindRoot(int ownerId) => roots_map.Find(e => e.Data.Owner_id == ownerId);

        private PlayerRoot FindRoot(IBlueprintItem item) => roots_map.Find(e => e.Find(item) != null);

        public PlayerRoot FindRoot(BlueprintService.BlueprintData data) => roots_map.Find(e => e.Data.Equals(data));

        //public bool IsRoot(BlueprintService.BlueprintData data) => IsRoot(data, out _);

        //public bool IsUpload(BlueprintService.BlueprintData data) => IsUpload(data, out _);

        //public bool IsRoot(BlueprintService.BlueprintData data, out BlueprintService.BlueprintData itemData) => GetItemData(data.Data, out itemData) == ItemType.Root && itemData != null;

        //public bool IsUpload(BlueprintService.BlueprintData data, out BlueprintService.BlueprintData itemData) => GetItemData(data.Data, out itemData) == ItemType.Upload && itemData != null;

        //public bool GetKnownData(BlueprintService.BlueprintData data, out BlueprintService.BlueprintData itemData) => GetItemData(data.Data, out itemData) != ItemType.Unknown && itemData != null;

        public bool IsRoot(IBlueprintItem item) => IsRoot(item, out _);

        public bool IsUpload(IBlueprintItem item) => IsUpload(item, out _);

        public bool IsRoot(IBlueprintItem item, out BlueprintService.BlueprintData itemData) => GetItemData(item, out itemData) == ItemType.Root && itemData != null;

        public bool IsUpload(IBlueprintItem item, out BlueprintService.BlueprintData itemData) => GetItemData(item, out itemData) == ItemType.Upload && itemData != null;

        public bool GetKnownData(IBlueprintItem item, out BlueprintService.BlueprintData itemData) => GetItemData(item, out itemData) != ItemType.Unknown && itemData != null;

        public ItemType GetItemData(IBlueprintItem item, out BlueprintService.BlueprintData itemData) {
            itemData = null;
            if (item == null) return ItemType.Unknown;
            foreach (var root in roots_map) {
                if (root.Item == item) {
                    itemData = root.Data;
                    return ItemType.Root;
                }
                var find = root.Find(item);
                if (find == null) continue;
                itemData = find.Data;
                return ItemType.Upload;
            }
            return ItemType.Unknown;
        }

        //public ItemType GetItemData(string data, out BlueprintService.BlueprintData itemData) {
        //    var t = GetItemData2(data, out itemData);
        //    Logging.Log.Warning("GetItemData: " + t);
        //    return t;
        //}

        //public ItemType GetItemData2(string data, out BlueprintService.BlueprintData itemData) {
        //    itemData = null;
        //    if (string.IsNullOrEmpty(data)) return ItemType.Unknown;
        //    foreach (var root in roots_map) {
        //        if (root.Data.Data == data) {
        //            itemData = root.Data;
        //            return ItemType.Root;
        //        }
        //        var find = root.Find(data);
        //        if (find == null) continue;
        //        itemData = find.Data;
        //        return ItemType.Upload;
        //    }
        //    return ItemType.Unknown;
        //}

        public int Like(IBlueprintItem item, int count) {
            if (!IsUpload(item)) return 0;
            PlayerRoot root = FindRoot(item);
            if (root == null) return 0;
            return root.Like(root.Find(item), count);
        }

        public void Like(IBlueprintItem item) => Like(item, 1);

        public void Unlike(IBlueprintItem item) => Like(item, -1);

        public bool IsOwned(IBlueprintItem item, ServerInfo server) => GetKnownData(item, out var data) && data.Owner_id + "" == server.UserId;

        public void AddRoot(IBlueprintItem item, BlueprintService.BlueprintData rootData) {
            if (FindRoot(rootData.Owner_id) is null) roots_map.Add(new PlayerRoot() { Item = item, Data = rootData });
        }

        public void AddRoot(IBlueprintItem item, int ownerId) => AddRoot(item, new BlueprintService.BlueprintData { Id = -1, Owner_id = ownerId, Download_count = 0, Like_count = 0, Data = ConvertItemToString(item) });

        public void AddItem(IBlueprintItem item, BlueprintService.BlueprintData data) {
            var parent = FindRoot(data.Owner_id);
            if (parent is null) return;
            parent.Data.Data = ConvertItemToString(parent.Item);
            parent.Add(item, data);
        }

        public void Clear() => roots_map.Clear();
    }
}