using DoubleQoL.Global;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;

namespace DoubleQoL.QoL.Controllers {

    internal abstract class AController : IUnityUi {
        protected abstract bool IsEnabled { get; }

        protected readonly StatusBar _statusBar;
        protected readonly ShortcutsManager _shortcutsManager;

        public AController(StatusBar statusBar, ShortcutsManager shortcutsManager) {
            _statusBar = statusBar;
            _shortcutsManager = shortcutsManager;
        }

        public void RegisterUi(UiBuilder builder) {
            if (IsEnabled) BuildUi(builder);
        }

        protected abstract void BuildUi(UiBuilder builder);

        public ControllerConfig Config => Static.StandAloneControllerConfig;
    }
}