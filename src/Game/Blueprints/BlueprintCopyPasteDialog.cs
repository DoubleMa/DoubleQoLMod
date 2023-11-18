using Mafi;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Blueprints.BlueprintCopyPasteDialog"/>.
    /// </summary>
    internal class BlueprintCopyPasteDialog : DialogView {

        private enum Mode {
            Export,
            Import,
            Upload
        }

        private readonly Txt m_title;
        private readonly Txt m_txt;
        private readonly Txt m_errorText;
        private readonly Btn m_importBtn;
        private readonly Btn m_uploadBtn;
        private readonly Btn m_exportBtn;
        private readonly UiBuilder m_builder;
        private readonly Func<string, bool> m_onImport;
        private string m_textToCopy;
        private Mode m_mode;

        public BlueprintCopyPasteDialog(UiBuilder builder, Func<string, bool> onImport) : base(builder) {
            m_textToCopy = "";
            m_builder = builder;
            m_onImport = onImport;
            HideMessage();
            Width = 500f;
            m_title = builder.NewTitle("Title").SetAlignment(TextAnchor.MiddleLeft).SetTextStyle(builder.Style.Global.TitleBig);
            m_title.SetSize(m_title.GetPreferredSize(0, 0));
            AppendCustomElement(m_title);
            Panel panel1 = builder.NewPanel("TxtPanel").SetBackground(3487029).SetHeight(30f);
            AppendCustomElement(panel1);
            m_txt = builder.NewTxt("Txt").SetTextStyle(builder.Style.Global.TextMedium).SetAlignment(TextAnchor.MiddleLeft).PutTo(panel1, Offset.LeftRight(5f));
            Panel panel2 = builder.NewPanel("ErrorPanel").SetHeight(20f);
            AppendCustomElement(panel2);
            m_errorText = builder.NewTxt("Error").SetTextStyle(builder.Style.Global.TextMediumBold).SetAlignment(TextAnchor.MiddleLeft).PutTo(panel2).Hide();
            string iconPath = Assets.Unity.UserInterface.General.Clipboard_svg;
            m_importBtn = AppendBtnPrimary(Tr.PasteString__Action, iconPath).AddToolTip(Tr.PasteString__Tooltip).OnClick(importString);
            m_uploadBtn = AppendBtnPrimary(new LocStrFormatted("Upload"), iconPath).AddToolTip(Tr.PasteString__Tooltip).OnClick(importString);
            m_exportBtn = AppendBtnPrimary(Tr.CopyString__Action, iconPath).AddToolTip(Tr.CopyString__Tooltip).OnClick(exportString);
            Btn closeBtn = AppendBtnGeneral(Tr.Close).OnClick(Hide);
            HighlightAsSettings();
        }

        private void importString() {
            string systemCopyBuffer = GUIUtility.systemCopyBuffer;
            setStringToPreview(systemCopyBuffer);
            m_errorText.Show();
            if (m_onImport(systemCopyBuffer)) {
                m_errorText.SetText(Tr.ImportBlueprint__Success);
                m_errorText.SetColor(m_builder.Style.Global.GreenForDark);
                m_importBtn.Hide();
            }
            else {
                m_errorText.SetText(Tr.ImportBlueprint__Fail);
                m_errorText.SetColor(m_builder.Style.Global.DangerClr);
            }
        }

        private void exportString() {
            GUIUtility.systemCopyBuffer = m_textToCopy;
            m_errorText.Show();
            m_errorText.SetText(Tr.CopyString__Success);
            m_errorText.SetColor(m_builder.Style.Global.GreenForDark);
        }

        public void OnUploadSuccess() {
            m_errorText.SetText("Blueprint uploaded");
            m_errorText.SetColor(m_builder.Style.Global.GreenForDark);
            Hide();
        }

        public void OnUploadFailed(string error = null) {
            m_errorText.SetText($"Failed to upload blueprint: {error ?? ""}");
            m_errorText.SetColor(m_builder.Style.Global.DangerClr);
            m_uploadBtn.Show();
        }

        private void uploadString() {
            string systemCopyBuffer = GUIUtility.systemCopyBuffer;
            setStringToPreview(systemCopyBuffer);
            m_errorText.Show();
            if (m_onImport(systemCopyBuffer)) {
                m_errorText.SetText("Uploading blueprint...");
                m_errorText.SetColor(m_builder.Style.Global.GreenForDark);
                m_uploadBtn.Hide();
            }
            else {
                m_errorText.SetText("Failed to parse or upload blueprint");
                m_errorText.SetColor(m_builder.Style.Global.DangerClr);
            }
        }

        public void ShowForStringImport() {
            m_mode = Mode.Import;
            m_title.SetText(Tr.ImportBlueprint__Title);
            m_txt.SetText("");
            SetBtnVisibility(m_exportBtn, false);
            SetBtnVisibility(m_importBtn, true);
            SetBtnVisibility(m_uploadBtn, false);
            m_errorText.Hide();
            Show();
        }

        public void ShowForStringExport(string text) {
            m_mode = Mode.Export;
            m_textToCopy = text;
            m_title.SetText(Tr.ExportBlueprint__Title);
            setStringToPreview(text);
            SetBtnVisibility(m_exportBtn, true);
            SetBtnVisibility(m_importBtn, false);
            SetBtnVisibility(m_uploadBtn, false);
            m_errorText.Hide();
            Show();
        }

        public void ShowForStringUpload() {
            m_mode = Mode.Upload;
            m_title.SetText("Upload BluePrint");
            m_txt.SetText("");
            SetBtnVisibility(m_exportBtn, false);
            SetBtnVisibility(m_importBtn, false);
            SetBtnVisibility(m_uploadBtn, true);
            m_errorText.Hide();
            Show();
        }

        private void setStringToPreview(string text) {
            int length = text.Length;
            string str = Tr.CharactersCount.Format(length).Value;
            m_txt.SetText("\"" + text.Substring(0, length.Min(26)) + "...\" (" + str + ")");
        }

        public bool InputUpdate() {
            switch (m_mode) {
                case Mode.Import:
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V)) {
                        importString();
                        return true;
                    }
                    break;

                case Mode.Export:
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C)) {
                        exportString();
                        return true;
                    }
                    break;

                case Mode.Upload:
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V)) {
                        uploadString();
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}