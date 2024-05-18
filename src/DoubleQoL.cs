using DoubleQoL.Game.Patcher;
using DoubleQoL.XML.lang;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Game;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using System;

namespace DoubleQoL {

    public sealed class DoubleQoL : IMod {
        public static Version ModVersion = new Version(1, 6, 2);
        public string Name => "DoubleQoL";
        public int Version => 1;
        public bool IsUiOnly => false;

        public Option<IConfig> ModConfig { get; }

        public void Initialize(DependencyResolver resolver, bool gameWasLoaded) {
            Logging.Log.Info($"Current {Name} version v{ModVersion.ToString(3)}");
            InitializePatchers(resolver);
        }

        public void ChangeConfigs(Lyst<IConfig> configs) {
        }

        public void RegisterPrototypes(ProtoRegistrator registrator) {
        }

        public void RegisterDependencies(DependencyResolverBuilder depBuilder, ProtosDb protosDb, bool wasLoaded) {
        }

        private void InitializePatchers(DependencyResolver resolver) {
            LanguageManager.Instance.INIT();
            MineTowerPatcher.Instance?.Init(resolver);
            VehiclePatcher.Instance?.Init(resolver);
            EdgeSizeLimitPatcher.Instance?.Init(resolver);
            LogisticsStatusBarPatcher.Instance?.Init(resolver);
            PopulationStatusBarPatcher.Instance?.Init(resolver);
            ResearchPopupControllerPatcher.Instance?.Init(resolver);
            ProductsDisplaysPatcher.Instance?.Init(resolver);
            VehicleTypeAssignerViewPatcher.Instance?.Init(resolver);
            //BlueprintsControllerPatcher.Instance?.Init(resolver);
        }

        public void EarlyInit(DependencyResolver resolver) {
        }
    }
}