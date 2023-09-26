using DoubleQoL.Config;
using Mafi.Unity.InputControl;

namespace DoubleQoL.Game.Shortcuts
{
    public class DoubleQoLShortcutsMap : ShortcutsMap
    {
        public static readonly DoubleQoLShortcutsMap Instance = new DoubleQoLShortcutsMap();
        public const KbCategory DoubleMaModKB = new KbCategory();

        [Kb(DoubleMaModKB, "IncSpeed", "Increase Game Speed", null, true, false)]
        public KeyBindings IncSpeedKB { get; set; }

        [Kb(DoubleMaModKB, "DecSpeed", "Decrease Game Speed", null, true, false)]
        public KeyBindings DecSpeedKb { get; set; }

        [Kb(DoubleMaModKB, "FreeCamera", "Disable/Enable Free Camera", null, true, false)]
        public KeyBindings FreeCameraKb { get; set; }

        [Kb(DoubleMaModKB, "DisableFog", "Disable/Enable Fog", null, true, false)]
        public KeyBindings DisableFogKb { get; set; }

        [Kb(DoubleMaModKB, "DisableCollapse", "Disable/Enable Collapse", null, true, false)]
        public KeyBindings DisableCollapseKb { get; set; }

        [Kb(DoubleMaModKB, "DevMode", "Disable/Enable Dev Mode", null, true, false)]
        public KeyBindings DevKeyKb { get; set; }

        public DoubleQoLShortcutsMap() : base()
        {
            IncSpeedKB = KeyBindings.FromKey(DoubleMaModKB, ConfigManager.Instance.KeyCodes_incSpeed.getKeyCodeValue());
            DecSpeedKb = KeyBindings.FromKey(DoubleMaModKB, ConfigManager.Instance.KeyCodes_decSpeed.getKeyCodeValue());
            FreeCameraKb = KeyBindings.FromKey(DoubleMaModKB, ConfigManager.Instance.KeyCodes_freeCamera.getKeyCodeValue());
            DisableFogKb = KeyBindings.FromKey(DoubleMaModKB, ConfigManager.Instance.KeyCodes_fog.getKeyCodeValue());
            DisableCollapseKb = KeyBindings.FromKey(DoubleMaModKB, ConfigManager.Instance.KeyCodes_collapse.getKeyCodeValue());
            DevKeyKb = KeyBindings.FromKey(DoubleMaModKB, ConfigManager.Instance.KeyCodes_Dev.getKeyCodeValue());
        }
    }
}