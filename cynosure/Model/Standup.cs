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
    }
}