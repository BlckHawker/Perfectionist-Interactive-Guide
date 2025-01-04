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
        public int DesiredAmount;

        public GrowCropAction(string qualifiedItemId, int desiredAmount, ChangeDisplayNameDelegate changeDisplayNameMethod) : base(changeDisplayNameMethod)
        {
            DesiredAmount = desiredAmount;
            QualifiedItemId = qualifiedItemId;
        }
    }
}
