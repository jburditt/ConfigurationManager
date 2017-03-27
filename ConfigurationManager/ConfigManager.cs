using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Arcteryx.Web
{
    // TODO Disable in prod. Security concern if someone had write access to Web folder (which would never happen)
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
                    filePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.FullName + $"\\Arcteryx.Web\\Local.{activeConfig}.config";
                    Load(filePath);
                }

                // global config
                filePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.FullName + "\\Arcteryx.Web\\Local.config";
                Load(filePath);
            }
            catch
            {
                // swallow error these are not critical
            }
        }

        public static string DebugHtml()
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();
            builder.ConnectionString = ConfigurationManager.ConnectionStrings["ArcteryxRepository"].ConnectionString;

            var html = $"<h3>Database</h3><br>Server = [{builder["Data Source"]}]<br>Database Name = {builder["Initial Catalog"]}<br><br><h3>AppSettings</h3><br>";

            foreach (var appSetting in ConfigurationManager.AppSettings)
                html += $"{appSetting} = {ConfigurationManager.AppSettings[appSetting+""]}<br>";

            return html;
        }

        public static bool Load(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                var config = XDocument.Load(filePath);

                if (config?.Root == null)
                    return false;

                if (config.Root.Element("appSettings") != null)
                    foreach (var element in config.Root.Element("appSettings").Elements("add"))
                    {
                        var key = element.Attribute("key")?.Value;
                        var value = element.Attribute("value")?.Value;

                        if (key != null && value != null)
                            ConfigurationManager.AppSettings[key] = value;
                    }

                if (config.Root.Element("connectionStrings") != null)
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
                                typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic)?
                                    .SetValue(ConfigurationManager.ConnectionStrings, false);

                                ConfigurationManager.ConnectionStrings.Add(new ConnectionStringSettings(name, connectionString, providerName));
                            }
                            else
                            {
                                typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(conn, false);

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
