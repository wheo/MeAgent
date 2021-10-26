using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeAgent.config
{
    internal class Settings
    {
        public String ip { get; set; }
        public int port { get; set; }
        public String id { get; set; }
        public String pw { get; set; }

        public String DatabaseName { get; set; }

        public double min_variance { get; set; } = 0;

        public string alarmpath { get; set; } = "";

        public int alarmsec { get; set; } = 10;

        public int limit_error_count { get; set; } = 30;

        public String configFileName { get; } = "config.json";

        public void Save()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(this.configFileName))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, this);
            }
        }
    }
}