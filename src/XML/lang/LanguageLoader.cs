using DoubleQoL.XML.config;

namespace DoubleQoL.XML.lang {

    internal class LanguageLoader : AXmlLoader<LanguageLoader> {
        public override string path => typeof(ConfigLoader).Assembly.Location + ".lang";
        public override string defaultSection => "language";

        public LanguageLoader() : base() {
        }
    }
}