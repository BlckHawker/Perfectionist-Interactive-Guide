using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod.Decision_Trees
{
    internal class GetItemAction : Action
    {
        public string QualifiedItemId { get; private set; }

        public GetItemAction(string qualifiedItemId, ChangeDisplayNameDelegate changeDisplayNameMethod) : base(changeDisplayNameMethod)
        {
            QualifiedItemId = qualifiedItemId;
        }
    }
}
