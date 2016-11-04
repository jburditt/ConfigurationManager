using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace ConfigManager
{
    public class ConfigManager
    {
        public static void Initialize()
        {
            try
            {
                // active config
                var activeConfig = ConfigurationManager.AppSettings["BuildConfiguration"];
                string filePath;
                if (!string.IsNullOrEmpty(activeConfig))
                {
                    filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.FullName +
                               $"\\Local.{activeConfig}.config";
                    Load(filePath);
                }

                // global config
                filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.FullName + "\\Local.config";
                Load(filePath);
            } catch { }
        }

        public static bool Load(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                var config = XDocument.Load(filePath);

                foreach (var element in config.Root.Element("appSettings").Elements("add"))
                {
                    var key = element.Attribute("key")?.Value;
                    var value = element.Attribute("value")?.Value;

                    if (key != null && value != null)
                        ConfigurationManager.AppSettings[key] = value;
                }

                foreach (var element in config.Root.Element("connectionStrings").Elements("add"))
                {
                    var name = element.Attribute("name")?.Value;
                    var connectionString = element.Attribute("connectionString")?.Value;
                    var providerName = element.Attribute("providerName")?.Value;

                    if (name != null && connectionString != null)
                    {
                        var conn = ConfigurationManager.ConnectionStrings[name];
                        if (conn == null)
                        {
                            typeof (ConfigurationElementCollection).GetField("bReadOnly",
                                BindingFlags.Instance | BindingFlags.NonPublic)?
                                .SetValue(ConfigurationManager.ConnectionStrings, false);

                            ConfigurationManager.ConnectionStrings.Add(
                                new ConnectionStringSettings(name, connectionString, providerName));
                        }
                        else
                        {
                            typeof (ConfigurationElement).GetField("_bReadOnly",
                                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(conn, false);

                            conn.ConnectionString = connectionString;
                            if (providerName != null)
                                conn.ProviderName = providerName;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}