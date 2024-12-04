using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod
{
    /// <summary>
    /// Class to keep track of items that don't exist in the world
    /// </summary>
    internal class DummyItem
    {
        public string QualifiedItemId { get; private set; }
        public string DisplayName { get; private set; }

        public DummyItem(string qualifiedItemId, string displayName)
        { 
            QualifiedItemId = qualifiedItemId;
            DisplayName = displayName;
        }

        public DummyItem(KeyValuePair<string, string> kv)
        {
            QualifiedItemId = kv.Value;
            DisplayName = kv.Key;
        }
    }
}
