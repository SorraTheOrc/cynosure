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
        public List<string> Backlog { get; set; }

        public Standup()
        {
            Done = new List<string>();
            Committed = new List<string>();
            Issues = new List<string>();
            Backlog = new List<string>();
        }

        public string Summary()
        {
            string summary = ItemsSummary("DONE", Done);
            summary += ItemsSummary("FOCUSING ON", Committed);
            summary += ItemsSummary("BARRIERS", Issues);
            summary += ItemsSummary("BACKLOG", Backlog);
            return summary;
        }

        public enum ItemSummaryFormat {
            List,
            NumberedList
        };

        public static string ItemsSummary(string prefix, List<string> items, ItemSummaryFormat format = ItemSummaryFormat.NumberedList)
        {
            string separator = "\n\n";
            int idx = 1;
            String summary = prefix + separator;
            if (items.Any())
            {
                var last = items.Last();
                foreach (var item in items)
                {
                    if (format == ItemSummaryFormat.NumberedList)
                    {
                        summary += idx + ". ";
                        idx++;
                    }

                    if (!item.Equals(last))
                    {
                        summary += item + separator;
                    }
                    else
                    {
                        summary += item;
                    }
                }
            } else
            {
                summary += "None";
            }
            summary += "\n\n";
            return summary;
        }
    }
}