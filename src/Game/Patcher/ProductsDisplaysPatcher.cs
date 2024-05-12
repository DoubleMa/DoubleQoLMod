using DoubleQoL.Extensions;
using DoubleQoL.Game.UI;
using Mafi.Unity;
using Mafi.Unity.InputControl.RecipesBook;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;
using System;
using System.Reflection;
using UnityEngine;

namespace DoubleQoL.Game.Patcher
{

    internal class ProductsDisplaysPatcher : APatcher<ProductsDisplaysPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => true;
        private static Type Typ;
        public static bool left = false;

        public ProductsDisplaysPatcher() : base("ProductsDisplaysPatcher") {
            Typ = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.TopStatusBar.ProductsDisplays.ProductsDisplaysController");
            Type Typ2 = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.TopStatusBar.ProductsDisplays.ProductDisplayPanel");
            AddMethod(Typ, "RegisterUi", this.GetHarmonyMethod(nameof(MyPostfix)));
            AddMethod(Typ2, "BuildUi", this.GetHarmonyMethod(nameof(MyPostfix2)), true);
            AddMethod(Typ2, "refreshProducts", this.GetHarmonyMethod(nameof(MyPostfix2)), true);
        }

        private static void MyPostfix(IUnityUi __instance, UiBuilder builder) {
            RecipesBookController m_recipesBookController = __instance.GetField<RecipesBookController>("m_recipesBookController");
            View m_displayPanel = __instance.GetField<View>("m_displayPanel");
            m_displayPanel.BuildUi(builder);
            Mafi.Unity.UiFramework.Components.Panel panel = builder.NewPanel("Container").OnClick(new Action(m_recipesBookController.Open)).SetBackground(builder.Style.Global.PanelsBg);
            float width = m_displayPanel.RectTransform.sizeDelta.x;
            m_displayPanel.InvokeMethod("AddBottomPanel", panel, new Vector2(width + 10, 48));
            builder.NewBtn("btn").OnClick(new Action(m_recipesBookController.Open)).SetButtonStyle(builder.Style.Toolbar.ButtonOff).SetIcon("Assets/Unity/UserInterface/Toolbar/Recipes.svg").PutToRightMiddleOf(panel, 38.Vector2());
            builder.NewBtn("btn2").OnClick(() => {
                left = !left;
                SetPosition(m_displayPanel.RectTransform, panel.RectTransform);
            }).SetButtonStyle(builder.Style.Toolbar.ButtonOff).SetIcon(Assets.Unity.UserInterface.General.ArrowLeft128_png).PutToLeftMiddleOf(panel, 28.Vector2());
        }

        private static void MyPostfix2(View __instance) {
            SetPosition(__instance.RectTransform);
        }

        private static void SetPosition(RectTransform parent, RectTransform btnPanel = null) {
            if (left) {
                parent.position = new Vector2(parent.sizeDelta.x, parent.position.y);
                if (btnPanel != null) {
                    btnPanel.localScale = new Vector2(-1, 1);
                    btnPanel.position = new Vector3(0, btnPanel.position.y);
                }
            }
            else {
                parent.position = new Vector2(Screen.width, parent.position.y);
                if (btnPanel != null) {
                    btnPanel.localScale = new Vector2(1, 1);
                    btnPanel.position = new Vector3(Screen.width, btnPanel.position.y);
                }
            }
        }
    }
}