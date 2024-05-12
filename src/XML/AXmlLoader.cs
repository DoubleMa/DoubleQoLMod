using System;
using System.Linq;
using System.Xml.Linq;

namespace DoubleQoL.XML {

    internal abstract class AXmlLoader<X> : IXmlLoader where X : AXmlLoader<X>, new() {
        private static X instance;
        public static X Instance => GetInstance();

        private static X GetInstance() {
            try { if (instance == null) instance = new X(); }
            catch (Exception e) { Logging.Log.Exception(e, e.Message); }
            return instance;
        }

        public abstract string path { get; }
        public abstract string defaultSection { get; }
        protected readonly XElement root;

        public AXmlLoader() {
            root = GetOrCreateConfigFile();
        }

        protected XElement GetOrCreateConfigFile() {
            try {
                return XElement.Load(path);
            }
            catch {
                return new XElement(defaultSection);
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

        public string GetValueOrCreate<T>(XKeyWithComment<T> x) where T : IComparable, IConvertible {
            XElement xSection = x.SectionWithComment.Element;
            if (xSection == null) return x.DefaultValue.ToString();
            try {
                XElement keyElement = xSection.Elements(x.Tag).FirstOrDefault(e => e.Attribute("key")?.Value == x.Key);
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
            if (x.Comment != null) xSection.Add(new XComment(x.Comment));
            xSection.Add(new XElement(x.Tag, new XAttribute("key", x.Key), new XAttribute("value", x.DefaultValue)));
            Save();
            return x.DefaultValue.ToString();
        }

        public bool ReplaceKey(string tag, string toReplace, string newKey) {
            try {
                var toModify = root.Descendants(tag).Where(e => e.Attribute("key")?.Value == toReplace).ToList();
                if (toModify.Count == 0) return false;
                foreach (var e in toModify) e.Attribute("key").Value = newKey;
                Save();
                return true;
            }
            catch { }
            return false;
        }

        public void Save() => root?.Save(path);
    }
}