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
using static TenCrowns.ClientCore.ClientUI;
using static BetterAI.BetterAIInfos;

namespace BetterAI
{
    public class BetterAICharacter : Character
    {

        //lines 7677-7701
        public override void generateRatingsCourtier(CourtierType eCourtier)
        {
            BetterAIInfoCourtier eInfoCourtier = ((BetterAIInfoCourtier)infos().courtier(eCourtier));
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
            //forced Traits
            if (eInfoCourtier.maeAdjectives.Count > 0)
            {
                foreach (TraitType eLoopTrait in eInfoCourtier.maeAdjectives)
                {
                    addTrait(eLoopTrait);
                }
            }
            //Regligion: State Religion
            if (eInfoCourtier.mbStateReligion)
            {
                Player pPlayer = player();
                if (pPlayer != null)
                {
                    ReligionType eReligion = pPlayer.getStateReligion();
                    if (eReligion != ReligionType.NONE)
                    {
                        setReligion(eReligion);
                    }
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/
            base.generateRatingsCourtier(eCourtier);
        }
    }
}
