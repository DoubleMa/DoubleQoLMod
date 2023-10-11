using Mafi.Unity.UiFramework;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Statusbar.Component {

    internal interface IFixedSizeUiElement : IUiElement {
        Vector2 Size { get; }
    }
}