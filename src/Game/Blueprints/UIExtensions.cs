using DoubleQoL.Extensions;
using Mafi;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoubleQoL.Game.Blueprints {

    public static class UIExtensions {

        internal static Btn NewBtn(this UiBuilder builder, string name, IUiElement parent = null) => new Btn(builder, name, parent?.GameObject);

        internal static Btn NewBtnGeneralBig(this UiBuilder builder, string name, IUiElement parent = null) =>
            builder.NewBtn(name, parent).SetButtonStyle(builder.Style.Global.GeneralBtn.Extend(builder.Style.Global.GeneralBtn.Text.Extend(fontSize: 14)));

        internal static Btn NewBtnGeneralWide(this UiBuilder builder, string name, IUiElement parent = null) =>
            builder.NewBtn(name, parent).SetButtonStyle(builder.Style.Global.GeneralBtn.Extend(builder.Style.Global.GeneralBtn.Text.Extend(fontSize: 14), sidePaddings: 20));

        internal static Btn NewBtnPrimaryBig(this UiBuilder builder, string name, IUiElement parent = null) =>
            builder.NewBtn(name, parent).SetButtonStyle(builder.Style.Global.PrimaryBtn.Extend(builder.Style.Global.PrimaryBtn.Text.Extend(fontSize: 14)));

        internal static Btn NewBtnPrimaryWide(this UiBuilder builder, string name, IUiElement parent = null) =>
            builder.NewBtn(name, parent).SetButtonStyle(builder.Style.Global.PrimaryBtn.Extend(builder.Style.Global.PrimaryBtn.Text.Extend(fontSize: 14), sidePaddings: 20));

        internal static Btn NewBtnDangerBig(this UiBuilder builder, string name, IUiElement parent = null) =>
            builder.NewBtn(name, parent).SetButtonStyle(builder.Style.Global.DangerBtn.Extend(builder.Style.Global.DangerBtn.Text.Extend(fontSize: 14)));

        internal static Panel NewPanel(this UiBuilder builder, string name, IUiElement parent) => new Panel(builder, name, parent?.GameObject);

        internal static PanelWithShadow NewPanelWithShadow(this UiBuilder builder, string name, IUiElement parent) => new PanelWithShadow(builder, name, parent?.GameObject);

        internal static Txt NewTxt(this UiBuilder builder, string name, IUiElement parent) => new Txt(builder, name, parent?.GameObject);

        internal static Txt NewTitle(this UiBuilder builder, string name, IUiElement parent) => new Txt(builder, name, parent?.GameObject).SetTextStyle(builder.Style.Global.Title).SetAlignment(TextAnchor.MiddleLeft);

        internal static Txt NewTitleBigCentered(this UiBuilder builder, IUiElement parent) => new Txt(builder, "Title", parent?.GameObject).SetTextStyle(builder.Style.Global.TitleBig).SetAlignment(TextAnchor.MiddleCenter);

        internal static Txt NewTitle(this UiBuilder builder, LocStr name, IUiElement parent) => new Txt(builder, "title", parent?.GameObject).SetText((LocStrFormatted)name).SetTextStyle(builder.Style.Global.Title).SetAlignment(TextAnchor.MiddleLeft);

        internal static IconContainer NewIconContainer(this UiBuilder builder, string name, IUiElement parent) => new IconContainer(builder, name, parent.GameObject);

        internal static TxtField NewTxtField(this UiBuilder builder, string name, IUiElement parent) => new TxtField(builder, name, parent?.GameObject);

        internal static TxtField EnableSelectionOnFocus(this TxtField txtField) {
            txtField.GetField<TMP_InputField>("m_inputField").onFocusSelectAll = true;
            return txtField;
        }

        internal static IconContainer AddOutline(this IconContainer iconContainer) {
            iconContainer.GameObject.AddComponent<Outline>();
            return iconContainer;
        }

        internal static Txt AddOutline(this Txt txt) {
            txt.GetField<TextMeshProUGUI>("m_text").fontSharedMaterial = txt.GetField<UiBuilder>("m_builder").AssetsDb.GetSharedMaterial(Assets.Unity.TextMeshPro.Fonts.Main_Regular.Roboto_Outline_mat);
            return txt;
        }

        internal static BtnStyle ListMenuBtnDarker(this GlobalUiStyle globalUiStyle) => globalUiStyle.InvokeGetter<BtnStyle>("ListMenuBtnDarker");

        internal static BtnStyle IconBtnWhite(this GlobalUiStyle globalUiStyle) => globalUiStyle.InvokeGetter<BtnStyle>("IconBtnWhite");

        internal static BtnStyle IconBtnOrange(this GlobalUiStyle globalUiStyle) => globalUiStyle.InvokeGetter<BtnStyle>("IconBtnOrange");

        internal static BtnStyle IconBtnBrightOrange(this GlobalUiStyle globalUiStyle) {
            TextStyle? text = new TextStyle(globalUiStyle.OrangeText);
            ColorRgba? normalMaskClr = 0xFDD017FF;
            ColorRgba? hoveredMaskClr = ColorRgba.White;
            return new BtnStyle(text, null, null, normalMaskClr, hoveredMaskClr);
        }

        internal static Btn SetDragEnabled(this Btn btn, bool isEnabled) {
            Option<DragDropHandlerMb> m_dragListener = btn.GetField<Option<DragDropHandlerMb>>("m_dragListener");
            if (m_dragListener.HasValue) m_dragListener.Value.enabled = isEnabled;
            return btn;
        }
    }
}