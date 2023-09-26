using System.Linq;
using System.Xml.Linq;

namespace DoubleQoL.Config
{
    internal class ConfigLoader
    {
        public static readonly ConfigLoader Instance = new ConfigLoader();
        private readonly string path;
        private readonly XElement root;

        private ConfigLoader()
        {
            path = GetType().Assembly.Location + ".config";
            root = GetOrCreateConfigFile();
        }

        public void Save() => root?.Save(path);

        private XElement GetOrCreateConfigFile()
        {
            try
            {
                return XElement.Load(path);
            }
            catch
            {
                return new XElement("configuration");
            }
        }

        public XElement GetSectionOrCreate(XSectionWithComment x)
        {
            if (root == null) return null;
            try
            {
                XElement xSection = root.Element(x.tag);
                if (xSection == null)
                {
                    xSection = new XElement(x.tag);
                    if (x.comment != null) xSection.Add(new XComment(x.comment));
                    root.Add(xSection);
                    Save();
                }
                return xSection;
            }
            catch { }
            return null;
        }

        public string GetValueOrCreate(XKeyWithComment x)
        {
            XElement xSection = x.xSectionWithComment.xElement;
            if (xSection == null) return x.defaultValue;
            try
            {
                XElement keyElement = xSection.Elements("add").FirstOrDefault(e => e.Attribute("key")?.Value == x.key);
                if (keyElement != null)
                {
                    var value = keyElement.Attribute("value");
                    if (value == null || value.Value == null)
                    {
                        keyElement.SetAttributeValue("value", x.defaultValue);
                        Save();
                        return x.defaultValue;
                    }
                    else return value.Value;
                }
            }
            catch { }
            xSection.Add(new XComment(x.comment ?? "This was auto generated ¯\\_(ツ)_/¯"));
            xSection.Add(new XElement("add", new XAttribute("key", x.key), new XAttribute("value", x.defaultValue)));
            Save();
            return x.defaultValue;
        }
    }
}