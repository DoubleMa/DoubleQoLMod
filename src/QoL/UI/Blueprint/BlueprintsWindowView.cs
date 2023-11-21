using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Game.Blueprints;
using Mafi;
using Mafi.Core;
using Mafi.Core.Input;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Blueprint {

    [GlobalDependency(RegistrationMode.AsEverything, false, false)]
    internal class BlueprintsWindowView : WindowView {
        private readonly DependencyResolver _dependencyResolver;
        public TabsContainer _tabsContainer;
        public BlueprintsLocalView BlueprintsLocalView { get; private set; }

        internal BlueprintsWindowView(DependencyResolver dependencyResolver) : base("Blueprints") {
            _dependencyResolver = dependencyResolver;
            ShowAfterSync = true;
        }

        protected override void BuildWindowContent() {
            Vector2 size = BlueprintsView.SIZE.Modify(y: BlueprintsView.SIZE.y + 40f);
            SetTitle(Tr.Blueprints);
            SetContentSize(size);
            PositionSelfToCenter();
            MakeMovable();
            _tabsContainer = Builder.NewTabsContainer(size.x.RoundToInt(), size.y.RoundToInt()).PutTo(GetContentPanel());
            BlueprintsLocalView = _dependencyResolver.Instantiate<BlueprintsLocalView>();
            _tabsContainer.AddTab("Local", null, BlueprintsLocalView);
            foreach (var server in ConfigManager.Instance.Blueprint_Servers) {
                var t = _dependencyResolver.Instantiate<BlueprintsStoreView>();
                t.SetServer(server);
                _tabsContainer.AddTab(server.Name, null, t);
            }
        }

        public override void RenderUpdate(GameTime gameTime) {
            _tabsContainer.SyncUpdate(gameTime);
            base.RenderUpdate(gameTime);
        }

        public override void SyncUpdate(GameTime gameTime) {
            _tabsContainer.RenderUpdate(gameTime);
            base.SyncUpdate(gameTime);
        }

        internal bool InputUpdate(IInputScheduler inputScheduler) => ((IInputUpdate)_tabsContainer.ActiveTab.Value).InputUpdate(inputScheduler);

        public bool IsTabActive(Tab tab) => _tabsContainer.ActiveTab.Value == tab;
    }
}