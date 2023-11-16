using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;
using System.Linq;

namespace DoubleQoL.QoL.UI.Blueprint {

    public class QoLBlueprintsLibrary : BlueprintsLibrary {

        public QoLBlueprintsLibrary(IFileSystemHelper fileSystemHelper, ConfigSerializationContext serializationContext) : base(fileSystemHelper, serializationContext) {
        }

        public void Clear() {
            foreach (var blueprint in Root.Blueprints.AsEnumerable().ToList()) DeleteItem(Root, blueprint);
            foreach (var subfolder in Root.Folders.AsEnumerable().ToList()) DeleteItem(Root, subfolder);
        }
    }
}