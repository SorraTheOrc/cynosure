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
            String summary = "DONE:\n\n";
            foreach (var item in Done)
                summary += item + "\n\n";
            summary += "\n\n\n\nFOCUSING ON:\n\n";
            foreach (var item in Committed)
                summary += item + "\n\n";
            summary += "\n\n\n\nBARRIERS:\n\n";
            foreach (var item in Issues)
                summary += item + "\n\n";
            return summary;
        }
    }
}