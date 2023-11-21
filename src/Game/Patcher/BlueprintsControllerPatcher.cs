using DoubleQoL.Config;
using System;
using System.Linq;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    internal class BlueprintsControllerPatcher : APatcher<BlueprintsControllerPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.Blueprint_Servers.Count() > 0;
        private static Type Typ;

        public BlueprintsControllerPatcher() : base("BlueprintsViewPatcher") {
            Typ = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.Blueprints.BlueprintsController");
            AddMethod(Typ, "RegisterUi", PostfixEmpty);
        }
    }
}