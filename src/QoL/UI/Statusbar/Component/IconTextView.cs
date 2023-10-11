using DoubleQoL.Extensions;
using DoubleQoL.Global;
using Mafi;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Statusbar.Component {

    internal class IconTextView : IFixedSizeUiElement {
        private readonly Panel _container;
        public GameObject GameObject => _container.GameObject;
        public RectTransform RectTransform => _container.RectTransform;
        public Vector2 Size { get; private set; } = Static.InfoTileSize.Modify(y: 20f);
        public bool IgnoreHover { get; set; } = false;
        public ColorRgba NeutralClr => _builder.Style.Global.Text.Color;
        public ColorRgba PositiveClr => _builder.Style.Global.GreenForDark;
        public ColorRgba WarningClr => _builder.Style.Global.OrangeText;
        public ColorRgba CriticalClr => _builder.Style.Global.DangerClr;
        private readonly UiBuilder _builder;
        private readonly string _iconPath;
        private Txt _Txt;
        private IconContainer _icon;
        private Action onMouseEnter;
        private Action onMouseLeave;
        private readonly Func<float, string> _formatFunc;
        private readonly ColorRgba DefaultBgColor = 2565927;
        private readonly ColorRgba HoverBgColor = new ColorRgba(22, 22, 22);
        private float Value;
        private readonly string Text;

        public IconTextView(UiBuilder builder, string icon, string text, float value, Func<float, string> formatFunc = null) : base() {
            _container = builder.NewPanel(nameof(IconTextView)).SetBackground(DefaultBgColor);
            _builder = builder;
            _iconPath = icon;
            Text = text;
            Value = value;
            _formatFunc = formatFunc;
        }

        public IconTextView SetOnClick(Action onClick) {
            if (onClick != null) _container.OnClick(onClick);
            return this;
        }

        public IconTextView SetOnRightClick(Action onRightClick) {
            if (onRightClick != null) _container.OnRightClick(onRightClick);
            return this;
        }

        public IconTextView SetOnMouseEnterLeaveActions(Action onMouseEnter, Action onMouseLeave) {
            this.onMouseEnter = onMouseEnter;
            this.onMouseLeave = onMouseLeave;
            return this;
        }

        public IconTextView Build(Vector2? vector = null) {
            Size = vector ?? Size;
            float itemSpacing = 3f;
            StackContainer container = _builder.NewStackContainer("StackContainer").SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetItemSpacing(itemSpacing)
                .SetInnerPadding(Offset.LeftRight(itemSpacing))
                .PutToLeftTopOf(_container, Size, Offset.Zero);
            TextStyle textStyle = _builder.Style.Global.Text.Extend(fontSize: 10);
            _icon = _builder.NewIconContainer("Icon").SetIcon(_iconPath)
                .AppendTo(container, Size.Extend(x: 0.1f), ContainerPosition.MiddleOrCenter);
            _builder.NewTxt("Count").SetTextStyle(textStyle).SetAlignment(TextAnchor.MiddleLeft)
                .SetText(Text.AbbreviateWithDot(7))
                .AppendTo(container, Size.Extend(x: 0.40f), ContainerPosition.LeftOrTop);
            _Txt = _builder.NewTxt("Count").SetTextStyle(textStyle).SetAlignment(TextAnchor.MiddleLeft)
                .AppendTo(container, Size.Extend(x: 0.40f), ContainerPosition.LeftOrTop);
            _container.SetOnMouseEnterLeaveActions(OnMouseEnter, OnMouseLeave);
            RectTransform.anchoredPosition = Vector2.zero;
            RectTransform.sizeDelta = Vector2.zero;
            SetValue(Value);
            return this;
        }

        private void OnMouseEnter() {
            _container.SetBackground(HoverBgColor);
            if (IgnoreHover) return;
            onMouseEnter?.Invoke();
        }

        private void OnMouseLeave() {
            _container.SetBackground(DefaultBgColor);
            if (IgnoreHover) return;
            onMouseLeave?.Invoke();
        }

        private string FormatText(float value) => _formatFunc is null ? Value.AddSign() : _formatFunc.Invoke(value);

        public void SetValue(float value) {
            Value = value;
            _Txt.SetText(FormatText(value));
            SetColor();
        }

        private void SetColor() {
            if (Value == 0) {
                _icon.SetColor(WarningClr);
                _Txt.SetColor(WarningClr);
            }
            else if (Value > 0) {
                _icon.SetColor(PositiveClr);
                _Txt.SetColor(PositiveClr);
            }
            else {
                _icon.SetColor(CriticalClr);
                _Txt.SetColor(CriticalClr);
            }
        }

        public Vector2 GetSize() => Size;
    }
}