using DoubleQoL.QoL.UI.Statusbar;
using Mafi;
using Mafi.Core.Prototypes;

namespace DoubleQoL.Game.Patcher.Helper {

    //[GlobalDependency(RegistrationMode.AsEverything)]
    [DependencyRegisteredManually("Should wait for StatusBarView")]
    internal class PatcherHelper {
        public static PatcherHelper Instance { get; private set; }
        public ProtosDb ProtosDb { get; }
        public LogisticsStatusBarView LogisticsStatusBarView { get; }
        public PopulationStatusBarView PopulationStatusBarView { get; }
        public UnityStatusBarView UnityStatusBarView { get; }

        public PatcherHelper(ProtosDb protosDb, LogisticsStatusBarView logisticsStatusBarView, PopulationStatusBarView populationStatusBarView, UnityStatusBarView unityStatusBarView) {
            ProtosDb = protosDb;
            LogisticsStatusBarView = logisticsStatusBarView;
            PopulationStatusBarView = populationStatusBarView;
            UnityStatusBarView = unityStatusBarView;
            Instance = this;
            InitPatchers();
        }

        private void InitPatchers() {
            MineTowerPatcher.Instance.Init();
            VehiclePatcher.Instance.Init();
            CollapsePatcher.Instance.Init();
            TerrainDesignationsPatcher.Instance.Init();
            LogisticsStatusBarPatcher.Instance.Init();
            PopulationStatusBarPatcher.Instance.Init();
            ResearchPopupControllerPatcher.Instance.Init();
        }
    }
}