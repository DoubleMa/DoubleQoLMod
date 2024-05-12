using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleQoL.XML.config {

    internal class ConfigLoader : AXmlLoader<ConfigLoader> {
        public override string path => typeof(ConfigLoader).Assembly.Location + ".config";
        public override string defaultSection => "configuration";

        public ConfigLoader() {
        }

        public IEnumerable<ServerInfo> GetAllServers() {
            try {
                GetSectionOrCreate(new XSectionWithComment(this, "BlueprintServers", "Add a private or public server for the BlueprintQoL"));
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