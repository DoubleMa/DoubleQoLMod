using System;
using System.Linq;
using System.Xml.Linq;

namespace DoubleQoL.Config {

    internal class ConfigLoader {
        public static readonly ConfigLoader Instance = new ConfigLoader();
        private readonly string path;
        private readonly XElement root;

        private ConfigLoader() {
            path = GetType().Assembly.Location + ".config";
            root = GetOrCreateConfigFile();
        }

        public void Save() => root?.Save(path);

        private XElement GetOrCreateConfigFile() {
            try {
                return XElement.Load(path);
            }
            catch {
                return new XElement("configuration");
            }
        }

        public XElement GetSectionOrCreate(XSectionWithComment x) {
            if (root == null) return null;
            try {
                XElement xSection = root.Element(x.Tag);
                if (xSection == null) {
                    xSection = new XElement(x.Tag);
                    if (x.Comment != null) xSection.Add(new XComment(x.Comment));
                    root.Add(xSection);
                    Save();
                }
                return xSection;
            }
            catch { }
            return null;
        }

        public string GetValueOrCreate<T>(XKeyWithComment<T> x) where T : struct, IComparable, IConvertible {
            XElement xSection = x.SectionWithComment.Element;
            if (xSection == null) return x.DefaultValue.ToString();
            try {
                XElement keyElement = xSection.Elements("add").FirstOrDefault(e => e.Attribute("key")?.Value == x.Key);
                if (keyElement != null) {
                    var value = keyElement.Attribute("value");
                    if (value == null || value.Value == null) {
                        keyElement.SetAttributeValue("value", x.DefaultValue);
                        Save();
                        return x.DefaultValue.ToString();
                    }
                    else return value.Value;
                }
            }
            catch { }
            xSection.Add(new XComment(x.Comment ?? "This was auto generated ¯\\_(ツ)_/¯"));
            xSection.Add(new XElement("add", new XAttribute("key", x.Key), new XAttribute("value", x.DefaultValue)));
            Save();
            return x.DefaultValue.ToString();
        }
    }
}