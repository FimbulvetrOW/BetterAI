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
using System;

namespace BetterAI
{
    public  class BetterAIClientManager : ClientManager
    {
        public BetterAIClientManager(ModSettings modSettings, Game gameClient, GameInterfaces gameInterfaces, IClientNetwork clientNetwork)
            : base(modSettings, gameClient, gameInterfaces, clientNetwork)
        {
        }

        //lines 1016-1023
        public override void sendBuildQueue(City pCity, int iOldSlot, int iNewSlot)
        {
/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                START ###
  ##############################################*/
            //can queued items even be dragged to position 0? idk, but better safe than sorry
            //intercepting invalid drag actions here, because I can't mod CityQueueList directly, since that class is in Assembly-CSharp
            if (iOldSlot == iNewSlot) return;
            if (((BetterAIInfoGlobals)Infos.Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1)
            {
                if (iNewSlot == 0)
                {
                    if (pCity != null && (pCity.getBuildThreshold(pCity.getBuildQueueNode(0)) == pCity.getBuildQueueNode(0).miProgress) && (pCity.getBuildQueueNode(0).miProgress > 0))
                    {
                        iNewSlot = 1;
                        if (iOldSlot == iNewSlot) return;
                    }
                }
                else if (iOldSlot == 0)
                {
                    if (pCity != null && (pCity.getBuildThreshold(pCity.getBuildQueueNode(0)) == pCity.getBuildQueueNode(0).miProgress) && (pCity.getBuildQueueNode(0).miProgress > 0))
                    {
                        return;
                    }
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                  END ###
  ##############################################*/

            ActionData actionData = ModSettings.Factory.CreateActionData(ActionType.BUILD_QUEUE, getActivePlayer());
            actionData.addValue(pCity.getID());
            actionData.addValue(iOldSlot);
            actionData.addValue(iNewSlot);
            sendAction(actionData);
        }
    }
}
