using DoubleQoL.Config;
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
        public bool IsExpanded { get; private set; } = ConfigManager.Instance.DefaultState_statusbar.Value;
        public bool lastState;

        public InfoTileExpended(UiBuilder builder, string icon) {
            _builder = builder;
            _container = builder.NewStackContainer(nameof(InfoTileExpended)).SetStackingDirection(StackContainer.Direction.TopToBottom).SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned);
            _expContainer = builder.NewStackContainer("ExpandableContainer").SetStackingDirection(StackContainer.Direction.TopToBottom).SetSizeMode(StackContainer.SizeMode.Dynamic);
            PopInfoTile = new InfoTile(builder, icon).OnRightClick(() => ToggleExpanded());
        }

        public void ToHideListener(bool hide) {
            if (hide) {
                lastState = IsExpanded;
                SetExpanded(false);
            }
            else SetExpanded(lastState);
        }

        public InfoTileExpended Build(Vector2? vector = null) {
            Size = vector ?? Size;
            PopInfoTile.AppendTo(_container, Size, ContainerPosition.LeftOrTop, Offset.Top(5f));
            _builder.NewPanel("divider").SetBackground(_builder.Style.Panel.Border.Color).AppendTo(_container, 5f);
            _expContainer.AppendTo(_container);
            _container.SetSize(Size);
            SetExpanded(IsExpanded);
            lastState = IsExpanded;
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

        public InfoTileExpended SetExpanded(bool expand) {
            try {//Lazy fix. For some reason GameObject is null sometimes!!
                _expContainer.SetVisibility(expand);
                IsExpanded = expand;
            }
            catch { }
            return this;
        }

        public InfoTileExpended ToggleExpanded() {
            SetExpanded(!IsExpanded);
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