using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace RodizioSmartRestuarant.Helpers
{
    /// <summary>
    /// Alternative for environment variables storage
    /// </summary>
    public static class Secrets
    {
        private static readonly string environmentVariablesPath = Path
            .Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            @"EnvironmentVariables.json");

        private static JObject environment = environmentVariables();

        private static JObject environmentVariables()
        {
            using (StreamReader r = new StreamReader(environmentVariablesPath))
            {
                string json = r.ReadToEnd();

                return JsonConvert.DeserializeObject<JObject>(json);
            }
        }

        #if DEBUG
        public static string GetVariable(string variable)
        {
            return environment["Development"][variable].ToString();
        }
        #else
        public static string GetVariable(string variable)
        {
            return environment["Production"][variable].ToString();
        }
        #endif
    }
}
