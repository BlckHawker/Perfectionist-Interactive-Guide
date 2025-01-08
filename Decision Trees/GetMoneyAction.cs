using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stardew_100_Percent_Mod.Decision_Trees
{
    internal class GetMoneyAction : Action
    {
        public int MoneyRequired { get; private set; }
        /// <summary>
        /// An action to see how much money the player needs
        /// </summary>
        /// <param name="changeDisplayNameDelegate"></param>
        public GetMoneyAction(ChangeDisplayNameDelegate changeDisplayNameDelegate) : base(changeDisplayNameDelegate)
        {
            
        }
    }
}
