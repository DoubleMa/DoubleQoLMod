using DoubleQoL.Extensions;
using DoubleQoL.Global;
using DoubleQoL.QoL.UI.Statusbar.Component;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Statusbar.Logistics {

    internal class VehicleCountView : IFixedSizeUiElement {
        private readonly Panel _container;
        public bool IgnoreHover { get; set; } = false;
        public GameObject GameObject => _container.GameObject;
        public RectTransform RectTransform => _container.RectTransform;
        public Vector2 Size { get; private set; } = Static.InfoTileSize.Extend(y: 0.5f);
        public Proto.ID ProtoId { get; }
        private readonly UiBuilder _builder;
        private readonly string _icon;
        private Txt _countTxt;
        private Txt _idleCount;
        private Action onMouseEnter;
        private Action onMouseLeave;
        private readonly ColorRgba DefaultBgColor = 2565927;
        private readonly ColorRgba HoverBgColor = new ColorRgba(22, 22, 22);

        public VehicleCountView(UiBuilder builder, Proto.ID protoId) : this(builder, protoId.GetVehicleIconPath()) {
            ProtoId = protoId;
        }

        public VehicleCountView(UiBuilder builder, string icon) : base() {
            _container = builder.NewPanel(nameof(VehicleCountView)).SetBackground(DefaultBgColor);
            _builder = builder;
            _icon = icon;
        }

        public VehicleCountView OnClick(Action onClick) {
            if (onClick != null) _container.OnClick(onClick);
            return this;
        }

        public VehicleCountView OnRightClick(Action onRightClick) {
            if (onRightClick != null) _container.OnRightClick(onRightClick);
            return this;
        }

        public VehicleCountView SetOnMouseEnterLeaveActions(Action onMouseEnter, Action onMouseLeave) {
            this.onMouseEnter = onMouseEnter;
            this.onMouseLeave = onMouseLeave;
            return this;
        }

        public VehicleCountView Build(Vector2? vector = null) {
            Size = vector ?? Size;
            float itemSpacing = 5f;
            float iconX = Size.x * 0.28f;
            float textX = Size.x - iconX - itemSpacing * 3;
            Vector2 textSize = new Vector2(textX, 11f);
            float textSpacing = 5f;
            TextStyle textStyle = _builder.Style.Global.Text.Extend(fontSize: 11);
            StackContainer container = _builder.NewStackContainer("StackContainer").SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetItemSpacing(itemSpacing)
                .SetInnerPadding(Offset.LeftRight(itemSpacing))
                .PutToCenterMiddleOf(_container, Size, Offset.Zero);
            IconContainer icon = _builder.NewIconContainer("Icon").SetIcon(_icon)
                .AppendTo(container, Size.Modify(x: iconX), ContainerPosition.MiddleOrCenter);
            StackContainer textPanel = _builder.NewStackContainer("textPanel").SetStackingDirection(StackContainer.Direction.TopToBottom)
                .SetItemSpacing(textSpacing)
                .SetInnerPadding(Offset.TopBottom((Size.y - textSize.y * 2 - textSpacing) / 2))
                .AppendTo(container, textSize.Extend(y: 2), ContainerPosition.LeftOrTop);
            _countTxt = _builder.NewTxt("Count").SetTextStyle(textStyle).SetAlignment(TextAnchor.MiddleLeft)
                .AppendTo(textPanel, textSize, ContainerPosition.MiddleOrCenter);
            _idleCount = _builder.NewTxt("Idle").SetTextStyle(textStyle).SetAlignment(TextAnchor.MiddleLeft)
                .AppendTo(textPanel, textSize, ContainerPosition.MiddleOrCenter);
            _container.SetOnMouseEnterLeaveActions(OnMouseEnter, OnMouseLeave);
            SetCounts(0, 0);
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

        public void SetCount(int count) {
            //this.SetVisibility(count > 0);
            _countTxt.SetText($"Count: {count}");
        }

        public void SetIdleCount(int count) => _idleCount.SetText($"Idle: {count}").SetColor(count > 0 ? 15048741 : 14935011);

        public void SetCounts(int count, int idle) {
            SetCount(count);
            SetIdleCount(idle);
        }
    }
}