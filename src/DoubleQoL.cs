using DoubleQoL.Game.Patcher.Helper;
using DoubleQoL.Game.Prototypes;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using System;

namespace DoubleQoL {

    public sealed class DoubleQoL : IMod {
        public string Name => "DoubleQoL";
        public int Version => 1;
        public bool IsUiOnly => false;

        private static Version GetVersion() => new Version(1, 2, 1);

        public void Initialize(DependencyResolver resolver, bool gameWasLoaded) {
            var version = GetVersion();
            Logging.Log.Info($"Current {Name} mod version v{version.Major}.{version.Minor}.{version.Build}");
            //resolver.EnsureResolved(typeof(PatcherHelper), typeof(PatcherHelper));
            resolver.Instantiate<PatcherHelper>();
        }

        public void ChangeConfigs(Lyst<IConfig> configs) {
        }

        public void RegisterPrototypes(ProtoRegistrator registrator) {
            PrototypeHelper.Instance.Init(registrator);
        }

        public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool wasLoaded) {
        }
    }
}