using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TenCrowns.ClientCore;

namespace BetterAI
{
    public  class BetterAIClientManager : ClientManager
    {
        public BetterAIClientManager(GameInterfaces gameInterfaces)
            : base(gameInterfaces)
        {
        }

        //lines 1011-1018
        public override void sendBuildQueue(City pCity, int iOldSlot, int iNewSlot)
        {
/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                START ###
  ##############################################*/
            //can queued items even be dragged to position 0? idk, but better safe than sorry
            //intercepting invalid drag actions here, because I can't mod CityQueueList directly, since that class is in Assembly-CSharp
            if (iOldSlot == iNewSlot) return;
            if (((BetterAIInfoGlobals)Infos.Globals).BAI_HURRY_COST_REDUCED > 0)
            {
                if (iNewSlot == 0)
                {
                    if (pCity != null && pCity.getBuildQueueNode(0).mbHurried && (pCity.getBuildQueueNode(0).miProgress > 0))
                    {
                        iNewSlot = 1;
                        if (iOldSlot == iNewSlot) return;
                    }
                }
                else if (iOldSlot == 0)
                {
                    if (pCity != null && pCity.getBuildQueueNode(0).mbHurried && (pCity.getBuildQueueNode(0).miProgress > 0))
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
