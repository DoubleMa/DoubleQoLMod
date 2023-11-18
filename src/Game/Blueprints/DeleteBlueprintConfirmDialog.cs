using Mafi.Unity;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Blueprints.DeleteBlueprintConfirmDialog"/>.
    /// </summary>
    internal class DeleteBlueprintConfirmDialog : DialogView {
        private readonly Action OnConfirm;

        public DeleteBlueprintConfirmDialog(UiBuilder builder, Action onConfirm) : base(builder) {
            OnConfirm = onConfirm;
            AppendBtnDanger(Tr.BlueprintDelete__Action).OnClick(Confirm);
            AppendBtnGeneral(Tr.Cancel).OnClick(Hide);
            HighlightAsDanger();
        }

        public void SetNameAndShow(string itemName) {
            SetMessage(Tr.BlueprintDelete__Confirmation.Format(itemName));
            Show();
        }

        private void Confirm() {
            Hide();
            OnConfirm();
        }

        public bool InputUpdate() {
            if (Input.GetKeyDown(KeyCode.Return)) {
                Confirm();
                return true;
            }
            return false;
        }
    }
}