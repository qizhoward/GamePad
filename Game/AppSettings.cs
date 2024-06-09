using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [Serializable]
    public class AppSettings
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string Note { get; set; }
        public bool Setting3 { get; set; }
    }
}
