using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenCrowns.AppCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;
using Constants = TenCrowns.GameCore.Constants;
using Enum = System.Enum;
using TenCrowns.ClientCore;
using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using UnityEngine;
using UnityEngine.UI;
using static TenCrowns.GameCore.Game;
using static TenCrowns.GameCore.Player;
using static TenCrowns.ClientCore.ClientUI;


namespace BetterAI
{
    public class BetterAI : ModEntryPointAdapter
    {
        public override void Initialize(ModSettings modSettings)
        {
            Debug.Log((object)"Better Old World AI Base DLL initializing");
            base.Initialize(modSettings);
            modSettings.Factory = new BetterAIGameFactory();
        }
    }
}
