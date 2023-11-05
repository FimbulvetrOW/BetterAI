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
using System.Runtime.Remoting.Messaging;

namespace BetterAI
{
    public class BetterAICharacter : Character
    {
        //lines 4670-4673
/*####### Better Old World AI - Base DLL #######
  ### Leader yield preview fix         START ###
  ##############################################*/
        public override int getRatingYieldRateLeader(RatingType eRating, YieldType eYield)
        {
            return infos().Helpers.getRatingYieldRateCourt(eRating, eYield, getRating(eRating), OpinionCharacterType.NONE, game());
        }
/*####### Better Old World AI - Base DLL #######
  ### Leader yield preview fix           END ###
  ##############################################*/

        public override int getLegitimacy(int iIndex, int iNumLeaders)
        {
            if (hasCognomen() && hasPlayer() && isOrWasLeader())
            {
                if (iIndex != -1)
                {
                    if (((BetterAIInfoGlobals)infos().Globals).BAI_ASSUMED_AVERAGE_REIGN_TURNS == 0 && ((BetterAIInfoGlobals)infos().Globals).BAI_EXTRA_LEGITIMACY_DECAY_TURNS_PER_LEADER == 0)
                    {
                        return (cognomen().miLegitimacy / Math.Max(1, iNumLeaders - iIndex + ((!isLeader() && isOrWasRegent()) ? ((BetterAIInfoGlobals)infos().Globals).BAI_PROPER_REGENT_LEGITIMACY_DECAY : 0) ));
                    }
                    else
/*####### Better Old World AI - Base DLL #######
  ### Time-based cognomen legitimacy decaySTART#
  ##############################################*/
                    {
                        if (isLeader())
                        {
                            return cognomen().miLegitimacy;
                        }
                        else
                        {
                            int iAssumedAverageReignTurns = ((BetterAIInfoGlobals)infos().Globals).BAI_ASSUMED_AVERAGE_REIGN_TURNS;
                            int iExtraDecayTurnsPerLeader = ((BetterAIInfoGlobals)infos().Globals).BAI_EXTRA_LEGITIMACY_DECAY_TURNS_PER_LEADER;
                            int iEndOfReignTurn = ((isAbdicated()) ? getAbdicateTurn() : getDeathTurn());
                            int iTurnsSinceEnd = game().getTurn() - iEndOfReignTurn;
                            int iExtraDecay = iExtraDecayTurnsPerLeader * Math.Max(1, iNumLeaders - iIndex + ((!isLeader() && isOrWasRegent()) ? ((BetterAIInfoGlobals)infos().Globals).BAI_PROPER_REGENT_LEGITIMACY_DECAY : 0));
                            int iTotalDecayTurns = Math.Max(iAssumedAverageReignTurns, iTurnsSinceEnd + iExtraDecay);
                            return ((cognomen().miLegitimacy * iAssumedAverageReignTurns) / (iAssumedAverageReignTurns + iTotalDecayTurns));
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Time-based cognomen legitimacy decay END #
  ##############################################*/
                }
            }
            return 0;
        }

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
