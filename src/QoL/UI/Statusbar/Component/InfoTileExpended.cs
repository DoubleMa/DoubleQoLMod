using DoubleQoL.Global;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Statusbar.Component {

    internal class InfoTileExpended : IFixedSizeUiElement {
        private readonly StackContainer _container;
        public GameObject GameObject => _container.GameObject;
        public RectTransform RectTransform => _container.RectTransform;
        public Vector2 Size { get; private set; } = Static.InfoTileSize;
        private readonly StackContainer _expContainer;
        private readonly UiBuilder _builder;
        public InfoTile PopInfoTile { get; private set; }

        private bool lastState = true;

        public InfoTileExpended(UiBuilder builder, string icon) {
            _builder = builder;
            _container = builder.NewStackContainer(nameof(InfoTileExpended)).SetStackingDirection(StackContainer.Direction.TopToBottom).SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned);
            _expContainer = builder.NewStackContainer(nameof(_expContainer)).SetStackingDirection(StackContainer.Direction.TopToBottom).SetSizeMode(StackContainer.SizeMode.Dynamic);
            PopInfoTile = new InfoTile(builder, icon).OnRightClick(() => ToggleExpend());
        }

        public void ToHideListener(bool hide) {
            if (hide) {
                lastState = IsExpended();
                SetExpended(false);
            }
            else SetExpended(lastState);
        }

        public InfoTileExpended Build(Vector2? vector = null) {
            Size = vector ?? Size;
            PopInfoTile.AppendTo(_container, Size, ContainerPosition.LeftOrTop, Offset.Top(5f));
            _builder.NewPanel("divider").SetBackground(_builder.Style.Panel.Border.Color).AppendTo(_container, 5f);
            _expContainer.AppendTo(_container);
            _container.SetSize(Size);
            return this;
        }

        public InfoTileExpended MakeAsSingleText() {
            PopInfoTile.MakeAsSingleText();
            return this;
        }

        public InfoTileExpended AddTooltip(string str) {
            PopInfoTile.AddTooltip(str);
            return this;
        }

        public InfoTileExpended OnClick(Action action) {
            PopInfoTile.OnClick(action);
            return this;
        }

        public InfoTileExpended OnRightClick(Action action) {
            PopInfoTile.OnRightClick(action);
            return this;
        }

        public bool IsExpended() => _expContainer.IsVisible();

        public InfoTileExpended SetExpended(bool expend) {
            _expContainer.SetVisibility(expend);
            return this;
        }

        public InfoTileExpended ToggleExpend() {
            _expContainer.ToggleVisibility();
            return this;
        }

        public InfoTileExpended Append(IFixedSizeUiElement element, Vector2 size) {
            element.AppendTo(_expContainer, size, ContainerPosition.LeftOrTop, Offset.Zero);
            return this;
        }

        public InfoTileExpended Append(IFixedSizeUiElement element) {
            element.AppendTo(_expContainer, element.Size, ContainerPosition.LeftOrTop, Offset.Zero);
            return this;
        }

        public InfoTileExpended ClearAll() {
            _expContainer.ClearAll(true);
            return this;
        }

        public InfoTileExpended StartBatchOperation() {
            _expContainer.StartBatchOperation();
            return this;
        }

        public InfoTileExpended FinishBatchOperation() {
            _expContainer.FinishBatchOperation();
            return this;
        }
    }
}