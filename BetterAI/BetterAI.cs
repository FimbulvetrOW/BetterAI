using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TenCrowns.AppCore;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using UnityEngine.UI;
using static TenCrowns.ClientCore.ClientUI;
using static TenCrowns.GameCore.Game;
using static TenCrowns.GameCore.Player;
using static TenCrowns.GameCore.Text.TextExtensions;
using Constants = TenCrowns.GameCore.Constants;
using Enum = System.Enum;


namespace BetterAI
{
    public class BetterAI : ModEntryPointAdapter
    {
        public override void Initialize(ModSettings modSettings)
        {
            //I still call this mod "BetterAI" because improving AI was my intention.
            //My failure to deliver much on that side so far does not mean that my goals changed.
            Assembly BAI_Assembly = Assembly.GetExecutingAssembly();
            string version = BAI_Assembly.GetName().Version?.ToString();
            string initMsg = "Better Old World AI Base DLL initializing (";

            if (!string.IsNullOrEmpty(version))
            {
                initMsg += "v" + version;
            }
            initMsg += ")";

            Debug.Log(initMsg);
            base.Initialize(modSettings);
            modSettings.Factory = new BetterAIGameFactory();
        }
    }
}
