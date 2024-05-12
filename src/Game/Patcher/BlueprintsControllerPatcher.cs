using DoubleQoL.XML.config;
using System.Linq;

namespace DoubleQoL.Game.Patcher {

    internal class BlueprintsControllerPatcher : APatcher<BlueprintsControllerPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.Blueprint_Servers.Count() > 0;

        public BlueprintsControllerPatcher() : base("BlueprintsViewPatcher") {
            //AddMethod(Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.Blueprints.BlueprintsController"), "RegisterUi", PostfixEmpty);
        }
    }
}