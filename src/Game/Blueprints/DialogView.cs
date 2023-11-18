using Mafi;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Styles;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using UnityEngine;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.UiFramework.Components.DialogView"/>.
    /// </summary>
    internal class DialogView {
        public float Width;
        private readonly UiBuilder m_builder;
        private readonly StackContainer m_itemsContainer;
        private readonly StackContainer m_btnsContainer;
        private readonly PanelWithShadow m_container;
        private readonly Txt m_text;
        private readonly Panel m_btnsHolder;

        protected StackContainer ItemsContainer => m_itemsContainer;

        public DialogView(UiBuilder builder) {
            Width = 450f;
            m_builder = builder;
            UiStyle style = m_builder.Style;
            m_container = m_builder.NewPanelWithShadow("Dialog", m_builder.GameOverlaySuper).SetBackground(2236962).AddShadowRightBottom();
            m_container.Hide();
            m_itemsContainer = m_builder.NewStackContainer("Container").SetStackingDirection(StackContainer.Direction.TopToBottom).SetSizeMode(StackContainer.SizeMode.Dynamic).SetInnerPadding(Offset.All(25f)).SetItemSpacing(10f).PutToTopOf(m_container, 0.0f);
            m_text = m_builder.NewTxt("Text").SetText("").SetTextStyle(style.Global.TextBig).SetAlignment(TextAnchor.MiddleCenter).EnableRichText().AppendTo(m_itemsContainer, 35f, Offset.Bottom(10f));
            m_btnsHolder = m_builder.NewPanel("BtnsHolder").AppendTo(m_itemsContainer, 35f);
            m_btnsContainer = m_builder.NewStackContainer("Dialog").SetStackingDirection(StackContainer.Direction.LeftToRight).SetSizeMode(StackContainer.SizeMode.Dynamic).SetItemSpacing(20f).PutToCenterOf(m_btnsHolder, 0.0f);
        }

        public void HighlightAsDanger() => m_container.SetBorderStyle(new BorderStyle(m_builder.Style.Global.DangerClr, 2f));

        public void HighlightAsSuccess() => m_container.SetBorderStyle(new BorderStyle(2523182, 2f));

        public void HighlightAsGeneral() => m_container.SetBorderStyle(new BorderStyle(10132122, 2f));

        public void HighlightAsSettings() {
            m_container.SetBorderStyle(new BorderStyle(0, 2f));
            m_container.SetBackground(m_builder.Style.Global.PanelsBg);
        }

        public bool IsVisible() => m_container.IsVisible();

        public void SetMessage(LocStrFormatted message) {
            m_text.SetText(message);
            m_itemsContainer.UpdateItemHeight(m_text, m_text.GetPreferedHeight(Width - 80).Max(35f));
        }

        protected void HideMessage() => m_itemsContainer.HideItem(m_text);

        public void SetLargeMessageOnce(LocStrFormatted message) {
            Width = 700f;
            m_itemsContainer.Remove(m_text);
            m_text.SetAlignment(TextAnchor.UpperLeft).SetText(message).SetHeight(m_text.GetPreferedHeight((float)(Width - 80)));
            m_builder.NewScrollableContainer("ScrollableTitles").AddVerticalScrollbar().AppendTo(m_itemsContainer, m_text.GetHeight().Clamp(40f, 400f)).AddItemTop(m_text);
        }

        protected void AppendCustomElement(IUiElement element) {
            m_itemsContainer.StartBatchOperation();
            m_itemsContainer.Remove(m_btnsHolder);
            m_itemsContainer.Append(element, element.GetHeight());
            m_btnsHolder.AppendTo(m_itemsContainer, 35f);
            m_itemsContainer.FinishBatchOperation();
        }

        protected void SetCustomItemVisibility(IUiElement element, bool isVisible) => m_itemsContainer.SetItemVisibility(element, isVisible);

        public Btn AppendBtnPrimary(LocStrFormatted text, Option<string> iconPath = default) {
            Btn objectToPlace = m_builder.NewBtnPrimaryBig("BtnPrimary").SetText(text).DropShadow();
            if (iconPath.HasValue) objectToPlace.SetIcon(iconPath.Value, 16.Vector2());
            return objectToPlace.AppendTo(m_btnsContainer, objectToPlace.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
        }

        public Btn AppendBtnDanger(LocStrFormatted text, Option<string> iconPath = default) {
            Btn objectToPlace = m_builder.NewBtnDangerBig("BtnDanger").SetText(text);
            if (iconPath.HasValue) objectToPlace.SetIcon(iconPath.Value, 16.Vector2());
            return objectToPlace.AppendTo(m_btnsContainer, objectToPlace.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
        }

        public Btn AppendBtnGeneral(LocStrFormatted text, Option<string> iconPath = default) {
            Btn objectToPlace = m_builder.NewBtnGeneralBig("BtnPrimary").SetText(text).DropShadow();
            if (iconPath.HasValue) objectToPlace.SetIcon(iconPath.Value, 16.Vector2());
            return objectToPlace.AppendTo(m_btnsContainer, objectToPlace.GetOptimalSize(), ContainerPosition.MiddleOrCenter);
        }

        public void SetBtnVisibility(Btn btn, bool isVisible) => m_btnsContainer.SetItemVisibility(btn, isVisible);

        public void Show() {
            m_builder.GameOverlaySuper.Show();
            m_container.PutToCenterMiddleOf(m_builder.GameOverlaySuper, new Vector2(Width, m_itemsContainer.GetDynamicHeight())).Show();
        }

        public void Hide() {
            m_container.Hide();
            m_builder.GameOverlaySuper.Hide();
        }

        public void ShowInCustomOverlay(IUiElement overlay) {
            overlay.Show();
            m_container.PutToCenterMiddleOf(overlay, new Vector2(Width, m_itemsContainer.GetDynamicHeight())).Show();
        }

        public void HideFromCustomOverlay(IUiElement overlay) {
            m_container.Hide();
            overlay.Hide();
        }
    }
}