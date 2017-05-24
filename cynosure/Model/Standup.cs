using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cynosure.Model
{
    [Serializable]
    public class Standup
    {
        public List<string> Done { get; set; }
        public List<string> Committed { get; set; }
        public List<string> Issues { get; set; }

        public Standup()
        {
            Done = new List<string>();
            Committed = new List<string>();
            Issues = new List<string>();
        }

        public string Summary()
        {
            string summary = ItemsSummary("DONE", Done);
            summary += ItemsSummary("FOCUSING ON", Committed);
            summary += ItemsSummary("BARRIERS", Issues);
            return summary;
        }

        public static string ItemsSummary(string prefix, List<string> items)
        {
            String summary = prefix + "\n\n";
            if (items.Any())
            {
                foreach (var item in items)
                    summary += item + "\n\n";
            } else
            {
                summary += "None" + "\n\n";
            }
            return summary;
        }
    }
}