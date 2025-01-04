using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod.Decision_Trees
{
    internal class GrowCropAction : Action
    {
        public string QualifiedItemId { get; private set; }

        public GrowCropAction(string qualifiedItemId, ChangeDisplayNameDelegate changeDisplayNameMethod) : base(changeDisplayNameMethod)
        {
            QualifiedItemId = qualifiedItemId;
        }
    }
}
