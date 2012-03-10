using System.Collections.Generic;

namespace TestRecorder.Core
{
    public class Settings
    {
        public static List<string> GetFindPattern()
        {
            var pattern = new List<string> {"id", "name", "title", "href", "url", "src", "value", "style", "text"};
            return pattern;
        }

        public static string GetConfigSettingString(string name)
        {
            return System.Configuration.ConfigurationManager.AppSettings[name];
        }
    }
}
