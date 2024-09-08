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
        //lines 4427-4438
        public override int getLegitimacy(int iIndex, int iNumLeaders)
        {
            if (hasCognomen() && hasPlayer() && isOrWasLeader())
            {
                if (iIndex != -1)
                {
/*####### Better Old World AI - Base DLL #######
  ### Time-based cognomen legitimacy decaySTART#
  ##############################################*/
                    if (((BetterAIInfoGlobals)infos().Globals).BAI_ASSUMED_AVERAGE_REIGN_TURNS == 0 && ((BetterAIInfoGlobals)infos().Globals).BAI_EXTRA_LEGITIMACY_DECAY_TURNS_PER_LEADER == 0)
                    {
                        return (cognomen().miLegitimacy / Math.Max(1, iNumLeaders - iIndex)); //normal legitimacy based on number of leaders since then
                    }
                    else
                    {
                        if (isLeader())
                        {
                            return cognomen().miLegitimacy;
                        }
                        else
                        {
                            int iAssumedAverageReignTurns = ((BetterAIInfoGlobals)infos().Globals).BAI_ASSUMED_AVERAGE_REIGN_TURNS;
                            int iExtraDecayTurnsPerLeader = ((BetterAIInfoGlobals)infos().Globals).BAI_EXTRA_LEGITIMACY_DECAY_TURNS_PER_LEADER;
                            int iEndOfReignTurn = (isAbdicated() ? getAbdicateTurn() : (isDead() ? getDeathTurn() : 0));
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

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/

        //lines 5982-5993
        public override void resetTraitEffectPlayer(TraitType eTrait, int iChange)
        {
            base.resetTraitEffectPlayer(eTrait, iChange);
            if (isJob())
            {
                EffectPlayerType eEffectPlayer = ((BetterAIInfoTrait)(infos().trait(eTrait))).maeJobEffectPlayer[getJob()];

                if (eEffectPlayer != EffectPlayerType.NONE)
                {
                    player().changeEffectPlayerCount(eEffectPlayer, iChange);
                }
            }

            if (((BetterAIInfoTrait)(infos().trait(eTrait))).bAnyTraitEffectPlayer)
            {
                foreach (TraitType eLoopTrait in getTraits())
                {
                    if (eTrait != eLoopTrait)
                    {
                        EffectPlayerType eEffectPlayer = ((BetterAIInfoTrait)(infos().trait(eTrait))).maeTraitEffectPlayer[eLoopTrait];
                        if (eEffectPlayer != EffectPlayerType.NONE)
                        {
                            player().changeEffectPlayerCount(eEffectPlayer, iChange);
                        }

                        eEffectPlayer = ((BetterAIInfoTrait)(infos().trait(eLoopTrait))).maeTraitEffectPlayer[eTrait];
                        if (eEffectPlayer != EffectPlayerType.NONE)
                        {
                            player().changeEffectPlayerCount(eEffectPlayer, iChange);
                        }
                    }
                }
            }
        }

        public virtual void resetJobTraitEffectPlayer(JobType eJob, int iChange)
        {
            foreach (TraitType eLoopTrait in getTraits())
            {
                EffectPlayerType eEffectPlayer = ((BetterAIInfoTrait)infos().trait(eLoopTrait)).maeJobEffectPlayer[eJob];
                if (eEffectPlayer != EffectPlayerType.NONE)
                {
                    player().changeEffectPlayerCount(eEffectPlayer, iChange);
                }
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/


        //lines 7677-7701
        public override void generateRatingsCourtier(CourtierType eCourtier)
        {
            BetterAIInfoCourtier pCourtierInfo = ((BetterAIInfoCourtier)infos().courtier(eCourtier));
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
            //forced Traits
            if (pCourtierInfo.maeAdjectives.Count > 0)
            {
                foreach (TraitType eLoopTrait in pCourtierInfo.maeAdjectives)
                {
                    addTrait(eLoopTrait);
                }
            }
            //Regligion: State Religion
            if (pCourtierInfo.mbStateReligion)
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

        public override void doUpgradeEvent()
        {

            if (!(player().doEventTrigger(infos().Globals.UPGRADE_CHARACTER_EVENTTRIGGER, this)))
            {
                using (var ratingsListScoped = CollectionCache.GetListScoped<RatingType>())
                using (var traitListScoped = CollectionCache.GetListScoped<TraitType>())
                {
                    List<RatingType> aeRatings = ratingsListScoped.Value;
                    List<TraitType> aeTraits = traitListScoped.Value;

                    int iBestIndependentTraitValue = -1;
                    TraitType eIndependentTrait = TraitType.NONE;

                    for (TraitType eLoopTrait = 0; eLoopTrait < infos().traitsNum(); eLoopTrait++)
                    {
                        if (isValidUpgradeTrait(eLoopTrait, bGeneral: false, bGovernor: false))
                        {
                            if ((isUnitGeneral() || isCityGovernor()) && isValidUpgradeTrait(eLoopTrait, isUnitGeneral(), isCityGovernor()))
                            {
                                aeTraits.Add(eLoopTrait);
                            }
                            else if (isLeaderOrHeir())
                            {
                                int iValue = game().randomNext(1000);
                                if (iValue > iBestIndependentTraitValue)
                                {
                                    iBestIndependentTraitValue = iValue;
                                    eIndependentTrait = eLoopTrait;
                                }
                            }
                        }
                    }
                    aeTraits.Shuffle(nextRandomSeed());

/*####### Better Old World AI - Base DLL #######
  ### Min Ratings Options on upgrade   START ###
  ##############################################*/
                    //int iExtraTraits = aeTraits.Count - infos().Globals.UPGRADES_AVAILABLE;
                    int iExtraTraits = aeTraits.Count - infos().Globals.UPGRADES_AVAILABLE + ((BetterAIInfoGlobals)infos().Globals).BAI_MIN_UPGRADE_RATINGS_OPTIONS;
                    if (iExtraTraits > 0)
                    {
                        aeTraits.RemoveRange(0, iExtraTraits);
                    }
                    //else if (iExtraTraits < 0)
                    if (aeTraits.Count < infos().Globals.UPGRADES_AVAILABLE)
/*####### Better Old World AI - Base DLL #######
  ### Min Ratings Options on upgrade     END ###
  ##############################################*/
                    {
                        for (RatingType eRating = 0; eRating < infos().ratingsNum(); ++eRating)
                        {
                            aeRatings.Add(eRating);
                        }
                        aeRatings.Shuffle(nextRandomSeed());

                        int iExtraRatings = aeRatings.Count + aeTraits.Count - infos().Globals.UPGRADES_AVAILABLE;
                        if (iExtraRatings > 0)
                        {
                            aeRatings.RemoveRange(0, iExtraRatings);
                        }
                    }

                    if (eIndependentTrait != TraitType.NONE)
                    {
                        aeTraits.Add(eIndependentTrait);
                    }

                    if (aeTraits.Count + aeRatings.Count > 0)
                    {
                        UpgradeCharacterDecision pDecision = new UpgradeCharacterDecision(player().nextDecisionID(), infos(), getID(), !(player().isProcessingTurn()));
                        for (int i = 0; i < aeTraits.Count; ++i)
                        {
                            pDecision.setTrait(i, aeTraits[i]);
                        }
                        for (int i = 0; i < aeRatings.Count; ++i)
                        {
                            pDecision.setRating(aeTraits.Count + i, aeRatings[i]);
                        }
                        player().pushDecisionDataNext(pDecision);
                    }
                }
            }
        }

    }
}
