using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeAgent.model
{
    internal class Logitem
    {
        public Logitem()
        {
        }

        public string StartAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string ip { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
}