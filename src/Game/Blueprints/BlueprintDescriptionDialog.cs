using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Blueprints.BlueprintDescriptionDialog"/>.
    /// </summary>
    internal class BlueprintDescriptionDialog : DialogView {
        private readonly TxtField m_txtField;

        public BlueprintDescriptionDialog(UiBuilder builder, Action<string> onApplyChanges) : base(builder) {
            HideMessage();
            Width = 400f;
            Txt element = builder.NewTitle("Title").SetText(Tr.UpdateDescription__Title).SetAlignment(TextAnchor.MiddleLeft).SetTextStyle(builder.Style.Global.TitleBig);
            element.SetSize(element.GetPreferredSize(0, 0));
            AppendCustomElement(element);
            m_txtField = builder.NewTxtField("TxtField").SetStyle(builder.Style.Global.LightTxtFieldStyle).SetPlaceholderText(string.Format("{0} ...", Tr.UpdateDescription__Placeholder)).SetCharLimit(0).EnableSelectionOnFocus().MakeMultiline().SetHeight(250f);
            AppendCustomElement(m_txtField);
            AppendBtnPrimary(Tr.ApplyChanges).OnClick(() => {
                onApplyChanges(m_txtField.GetText());
                Hide();
            });
            AppendBtnGeneral(Tr.Cancel).OnClick(Hide);
            HighlightAsSettings();
        }

        public void ShowForEdit(string description) {
            m_txtField.SetText(description);
            Show();
        }
    }
}