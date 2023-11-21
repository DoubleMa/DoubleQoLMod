using System;
using System.Collections.Generic;
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

        public string GetValueOrCreate<T>(XKeyWithComment<T> x) where T : IComparable, IConvertible {
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

        public bool ReplaceKey(string toReplace, string newKey) {
            try {
                var toModify = root.Descendants("add").Where(e => e.Attribute("key")?.Value == toReplace).ToList();
                if (toModify.Count == 0) return false;
                foreach (var e in toModify) e.Attribute("key").Value = newKey;
                Save();
                return true;
            }
            catch { }
            return false;
        }

        public IEnumerable<ServerInfo> GetAllServers() {
            try {
                GetSectionOrCreate(new XSectionWithComment("BlueprintServers", "Add a private or public server for the BlueprintQoL"));
                var servers = root.Descendants("server").Select(serverElement => {
                    var serverInfo = new ServerInfo {
                        Name = serverElement.Elements("add").FirstOrDefault(e => e.Attribute("key")?.Value == "name")?.Attribute("value")?.Value,
                        Url = serverElement.Elements("add").FirstOrDefault(e => e.Attribute("key")?.Value == "url")?.Attribute("value")?.Value,
                        AdditionalData = new Dictionary<string, string>()
                    };

                    foreach (var dataElement in serverElement.Elements("add").Where(e => e.Attribute("key")?.Value != "url" && e.Attribute("key")?.Value != "name")) {
                        var key = dataElement.Attribute("key")?.Value;
                        var value = dataElement.Attribute("value")?.Value;

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value)) {
                            serverInfo.AdditionalData[key] = value;
                        }
                    }

                    return serverInfo;
                });

                return servers;
            }
            catch (Exception ex) {
                Logging.Log.Warning($"Error while reading servers: {ex.Message}");
                return Enumerable.Empty<ServerInfo>();
            }
        }
    }
}