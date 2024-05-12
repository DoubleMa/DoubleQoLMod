using Mafi.Localization;

namespace DoubleQoL.XML.lang {

    internal class LanguageManager {
        public static readonly LanguageManager Instance = new LanguageManager();
        private readonly string CurrentLang;
        private readonly XSectionWithComment LangSection;

        public XKeyWithComment<string> tr_prioritize { get; }
        public XKeyWithComment<string> tr_select { get; }
        public XKeyWithComment<string> tr_count { get; }
        public XKeyWithComment<string> tr_idle { get; }
        public XKeyWithComment<string> tr_prioritize_resource { get; }
        public XKeyWithComment<string> tr_prioritize_product { get; }
        public XKeyWithComment<string> tr_cancel_jobs { get; }
        public XKeyWithComment<string> tr_vehicles_tool { get; }
        public XKeyWithComment<string> tr_vehicles_tool_tt { get; }
        public XKeyWithComment<string> tr_select_move { get; }
        public XKeyWithComment<string> tr_select_vehicles { get; }
        public XKeyWithComment<string> tr_clear_cargo_name { get; }
        public XKeyWithComment<string> tr_clear_cargo_tt { get; }
        public XKeyWithComment<string> tr_recover_vehicle_name { get; }
        public XKeyWithComment<string> tr_recover_vehicle_tt { get; }

        public void INIT() {
        }

        private LanguageManager() {
            var tag = "Entry";
            CurrentLang = LocalizationManager.CurrentLangInfo.LanguageTitle;
            var loader = LanguageLoader.Instance;
            LangSection = new XSectionWithComment(loader, CurrentLang);
            tr_prioritize = new XKeyWithComment<string>(loader, LangSection, "prioritize", "prioritize", null, tag);
            tr_select = new XKeyWithComment<string>(loader, LangSection, "select", "select", null, tag);
            tr_count = new XKeyWithComment<string>(loader, LangSection, "count", "count", null, tag);
            tr_idle = new XKeyWithComment<string>(loader, LangSection, "idle", "idle", null, tag);
            tr_prioritize_resource = new XKeyWithComment<string>(loader, LangSection, "prioritize_resource", "Prioritize resource for all exavators", "Title for the mine tower", tag);
            tr_prioritize_product = new XKeyWithComment<string>(loader, LangSection, "prioritize_product", "Prioritize the product to all the excavators", "Tooltip for the prioritize button in the mine tower", tag);
            tr_cancel_jobs = new XKeyWithComment<string>(loader, LangSection, "cancel_jobs", "Cancel all jobs", "Tooltip for the cancel jobs button, in the behicle window view", tag);
            tr_vehicles_tool = new XKeyWithComment<string>(loader, LangSection, "vehicles_tool", "Vehicle Tool", "Name of the vehicle tool", tag);
            tr_vehicles_tool_tt = new XKeyWithComment<string>(loader, LangSection, "vehicles_tool_tt", "Select One or multiple vehicles to move them", "Tooltip for the vehicle tool", tag);
            tr_select_vehicles = new XKeyWithComment<string>(loader, LangSection, "select_vehicles", "Select and move vehicles", "Tooltip for the select button, in the vehicle tool", tag);
            tr_select_move = new XKeyWithComment<string>(loader, LangSection, "select_move", "Select / Move", "Tooltip for the select button, in the vehicle tool", tag);
            tr_clear_cargo_name = new XKeyWithComment<string>(loader, LangSection, "clear_cargo_name", "Clear truck cargo", "Name of the clear cargo button, in the vehicle tool", tag);
            tr_clear_cargo_tt = new XKeyWithComment<string>(loader, LangSection, "clear_cargo_tt", "Try to clear the cargo of selected trucks", "Tooltip for the clear cargo button, in the vehicle tool", tag);
            tr_recover_vehicle_name = new XKeyWithComment<string>(loader, LangSection, "recover_vehicle_name", "Recover vehicle", "Name of the recover vehicle button, in the vehicle tool", tag);
            tr_recover_vehicle_tt = new XKeyWithComment<string>(loader, LangSection, "recover_vehicle_tt", "Recover the selected vehicles", "Tooltip for the recover vehicle button, in the vehicle tool", tag);
        }
    }
}