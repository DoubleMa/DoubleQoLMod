using Mafi.Core.GameLoop;
using Mafi.Core.Input;
using Mafi.Core.Maintenance;
using Mafi.Core.Products;
using Mafi.Core.Simulation;
using Mafi.Unity.InputControl.RecipesBook;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;

namespace DoubleQoL.QoL.UI {

    //[GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    internal class ProductsDisplaysController : IUnityUi {
        private readonly RecipesBookController m_recipesBookController;

        public ProductsDisplaysController(
          IProductsManager productsManager,
          IGameLoopEvents gameLoop,
          ICalendar calendar,
          IInputScheduler inputScheduler,
          PinnedProductsManager pinnedProductsManager,
          MaintenanceManager maintenanceManager,

          RecipesBookController recipesBook) : base() {
            //ProductsDisplaysController displaysController = this;
            //this.m_recipesBookController = recipesBook;
            //this.m_displayPanel = new ProductDisplayPanel(calendar, gameLoop, productsManager, inputScheduler, pinnedProductsManager, maintenanceManager, new Action(onPanelClick), new Action(onMaintenanceClick), new Action<ProductProto>(onProductRightClick));

            //void onProductRightClick(ProductProto product) => displaysController.m_recipesBookController.OpenForProduct(product);

            //void onPanelClick() => statsController.OpenAndShowProducts();

            //void onMaintenanceClick() => statsMaintenanceController.OpenAndShowMaintenance();
        }

        public void RegisterUi(UiBuilder builder) {
            //this.m_displayPanel.BuildUi(builder);
            //Panel panel = builder.NewPanel("Container").OnClick(new Action(this.m_recipesBookController.Open)).SetBackground(builder.Style.Global.PanelsBg);
            //this.m_displayPanel.AddBottomPanel((IUiElement)panel, 48.Vector2());
            //builder.NewBtn("btn").OnClick(new Action(this.m_recipesBookController.Open)).SetButtonStyle(builder.Style.Toolbar.ButtonOff).SetIcon("Assets/Unity/UserInterface/Toolbar/Recipes.svg").PutToCenterMiddleOf<Btn>((IUiElement)panel, 38.Vector2());
        }
    }
}