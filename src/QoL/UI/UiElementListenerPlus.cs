using Mafi;
using Mafi.Unity.UiFramework;
using System;
using UnityEngine.EventSystems;

namespace DoubleQoL.QoL.UI {

    internal class UiElementListenerPlus : UiElementListener {
        public Option<Action> MiddleClickAction { get; set; }

        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);

            if (MiddleClickAction.HasValue && eventData.button == PointerEventData.InputButton.Middle) {
                MiddleClickAction.Value();
            }
        }
    }
}