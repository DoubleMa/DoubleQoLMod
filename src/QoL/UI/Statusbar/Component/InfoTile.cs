using DoubleQoL.Global;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Statusbar.Component {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Levelling.PopulationStatusBarView.PopInfoTile"/>.
    /// </summary>
    internal class InfoTile : IFixedSizeUiElement {
        private readonly Btn m_container;
        public GameObject GameObject => m_container.GameObject;
        public RectTransform RectTransform => m_container.RectTransform;
        public ColorRgba CritricalClr => m_builder.Style.Global.DangerClr;
        public Vector2 Size { get; private set; } = Static.InfoTileSize;
        private readonly UiBuilder m_builder;
        private readonly Txt m_text;
        private readonly Txt m_subText;
        private readonly ColorRgba m_standardClr;
        public readonly ColorRgba SuccessClr;
        public readonly ColorRgba WarningClr;
        private readonly IconContainer m_icon;

        public InfoTile(UiBuilder builder, string iconPath) : base() {
            SuccessClr = new ColorRgba(2211126);
            WarningClr = new ColorRgba(15176704);
            m_builder = builder;
            TextStyle text = builder.Style.Global.Text;
            m_standardClr = text.Color;
            m_container = new Btn(builder, "PopulationStatusBarView").SetButtonStyle(builder.Style.Panel.HeaderButton);
            m_icon = builder.NewIconContainer("PopIcon").SetIcon(iconPath).SetColor(text.Color).PutToCenterTopOf(m_container, new Vector2(40f, 40f));
            m_text = builder.NewTxt("PopTileText").SetTextStyle(text.Extend(null, FontStyle.Bold, 14)).BestFitEnabled(14).SetAlignment(TextAnchor.MiddleCenter)
                .PutToTopOf(m_container, 22f, Offset.Top(40f));
            m_subText = builder.NewTxt("PopTileSubText").SetTextStyle(text.Extend(null, FontStyle.Bold, 12)).BestFitEnabled(12).SetAlignment(TextAnchor.MiddleCenter)
                .PutToTopOf(m_container, 20f, Offset.Top(60f));
        }

        public InfoTile AddCustomSubTextWithIcon(TextWithIcon txt) {
            m_subText.Hide();
            txt.PutToTopOf(m_container, 20f, Offset.Top(60f));
            return this;
        }

        public InfoTile AddTooltip(LocStrFormatted text) => AddTooltip(text.Value);

        public InfoTile AddTooltip(string text) {
            m_builder.AddTooltipFor(m_container).SetText(text);
            return this;
        }

        public InfoTile OnClick(Action onClick) {
            m_container.OnClick(onClick);
            return this;
        }

        public InfoTile OnRightClick(Action onRightClick) {
            m_container.OnRightClick(onRightClick);
            return this;
        }

        public InfoTile MakeAsSingleText() {
            m_text.SetAlignment(TextAnchor.UpperCenter).PutToTopOf(m_container, 40f, Offset.Top(40f) + Offset.LeftRight(5f));
            return this;
        }

        public InfoTile SetText(string text) {
            m_text.SetText(text);
            return this;
        }

        public InfoTile SetText(LocStrFormatted text) {
            m_text.SetText(text);
            return this;
        }

        public InfoTile SetSubText(string text, ColorRgba? color = null) {
            m_subText.SetText(text);
            if (color.HasValue) m_subText.SetColor(color.Value);
            return this;
        }

        public InfoTile SetWarningColor() {
            m_icon.SetColor(WarningClr);
            m_text.SetColor(WarningClr);
            m_subText.SetColor(WarningClr);
            return this;
        }

        public InfoTile SetCriticalColor() {
            m_icon.SetColor(CritricalClr);
            m_text.SetColor(CritricalClr);
            m_subText.SetColor(CritricalClr);
            return this;
        }

        public InfoTile SetStandardColor() {
            m_icon.SetColor(m_standardClr);
            m_text.SetColor(m_standardClr);
            m_subText.SetColor(m_standardClr);
            return this;
        }
    }
}