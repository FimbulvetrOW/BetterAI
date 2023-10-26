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
    public partial class BetterAIPlayer : Player
    {
        public partial class BetterAIPlayerAI : BetterAIPlayer.PlayerAI
        {

            //lines 8034-8058
            protected override long getHurryCostValue(City pCity, CityBuildHurryType eHurry)
            {
                long iValue = 0;
                if (player == null) return iValue;

                switch (eHurry)
                {
                    case CityBuildHurryType.CIVICS:
                        iValue = pCity.getHurryCivicsCost() * yieldValue(infos.Globals.CIVICS_YIELD);
                        break;
                    case CityBuildHurryType.TRAINING:
                        iValue = pCity.getHurryTrainingCost() * yieldValue(infos.Globals.TRAINING_YIELD);
                        break;
                    case CityBuildHurryType.MONEY:
                        iValue = pCity.getHurryMoneyCost() * yieldValue(infos.Globals.MONEY_YIELD);
                        break;
                    case CityBuildHurryType.ORDERS:
                        iValue = pCity.getHurryOrdersCost() * yieldValue(infos.Globals.ORDERS_YIELD);
                        break;
                    case CityBuildHurryType.POPULATION:
                        iValue = pCity.getHurryPopulationCost() * citizenValue(pCity, true);
                        break;
                }

/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                //fix discontent value
                //no use calculating the effect of AI_YIELD_TURNS (100 turns) of discontent, when it only happens once

                //iValue += cityYieldValue(infos.Globals.DISCONTENT_YIELD, pCity) * pCity.getHurryDiscontent() / Constants.YIELDS_MULTIPLIER;
                iValue += discontentValue(pCity) * pCity.getHurryDiscontent() / Constants.YIELDS_MULTIPLIER;
/*####### Better Old World AI - Base DLL #######
  ### AI fix                             END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                START ###
  ##############################################*/
                //hurry cost reduced, but no overflow: needs to be reflected in AI evaluation
                YieldType eBuildYield = (pCity.getBuildYieldType(pCity.getCurrentBuild()));
                int iCityYieldLost = pCity.calculateCurrentYield(eBuildYield);  //whether or not next Turns production reduced the cost of Hurrying, it's not going to City Production next turn
                int iStockpileYieldGained = iCityYieldLost;                     //but if not it's going to the stockpile instead
                iCityYieldLost += pCity.getYieldOverflow(eBuildYield);          //whether or not Overflow reduced the cost of Hurrying, it's gone afterwards
                iValue += (iCityYieldLost * cityYieldValue(eBuildYield, pCity)) / (Constants.YIELDS_MULTIPLIER);
                if (((BetterAIInfoGlobals)infos.Globals).BAI_HURRY_COST_REDUCED < 3)
                {
                    iValue -= (iStockpileYieldGained * yieldValue(eBuildYield)) / (Constants.YIELDS_MULTIPLIER);
                }

                return iValue;
            }

/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
            //calculateCityYieldValue: lines 4141-4240
            public virtual long discontentValue(City pCity)
            {
                if (player == null) return 0;

                YieldType eYield = infos.Globals.HAPPINESS_YIELD;

                //copy-pasted from calculateCityValue START
                //if (eYield == infos.Globals.DISCONTENT_YIELD)
                {
                    //int iModifier = 0;
                    long iValue = yieldValue(eYield);
                    int iNextThreshold = pCity.getYieldThreshold(eYield);

                    if (pCity.hasFamily())
                    {
/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                        //iValue += getFamilyOpinionValue(pCity.getFamily(), infos.Globals.DISLOYALTY_OPINION) * AI_YIELD_TURNS * 5 / (100 * iNextThreshold);
                        iValue -= getFamilyOpinionValue(pCity.getFamily(), infos.Globals.DISLOYALTY_OPINION) * 5 / 100; //without AI_YIELD_TURNS, division by iNextThreshold moved down
/*####### Better Old World AI - Base DLL #######
  ### AI fix                             END ###
  ##############################################*/
                    }

                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                    {
                        if (eLoopYield != eYield && eLoopYield != infos.Globals.DISCONTENT_YIELD)
                        {
                            int iExtraYieldInNextLevel = 0;
                            int iCityYield = cityYield(eLoopYield, pCity);
                            //iExtraYieldInNextLevel += infos.Helpers.getHappinessLevelYieldModifier(pCity.getHappinessLevel() + 1, eLoopYield) * iCityYield / 100;
                            //iExtraYieldInNextLevel -= infos.Helpers.getHappinessLevelYieldModifier(pCity.getHappinessLevel(), eLoopYield) * iCityYield / 100;
                            //more interesting to look at one level below instead, just as the original method did

                            iExtraYieldInNextLevel -= infos.Helpers.getHappinessLevelYieldModifier(pCity.getHappinessLevel() - 1, eLoopYield) * iCityYield / 100;
                            iExtraYieldInNextLevel += infos.Helpers.getHappinessLevelYieldModifier(pCity.getHappinessLevel(), eLoopYield) * iCityYield / 100;

                            if (iExtraYieldInNextLevel != 0)
                            {
/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                                //iValue += iExtraYieldInNextLevel * cityYieldValue(eLoopYield, pCity) * AI_YIELD_TURNS / iNextThreshold;
                                iValue += (long)iExtraYieldInNextLevel * cityYieldValue(eLoopYield, pCity); //without AI_YIELD_TURNS, division by iNextThreshold moved down
/*####### Better Old World AI - Base DLL #######
  ### AI fix                             END ###
  ##############################################*/
                            }
                        }
                    }

                    //value derivate from base yield, unmodified
                    //iValue = infos.utils().modify(iValue, pCity.calculateTotalYieldModifier(eYield));

                    iValue /= iNextThreshold;
                    //return infos.utils().modify(iValue, infos.yield(eYield).mbNegative ? -iModifier : iModifier);
                    return iValue;
                }
                //copy-paste END
            }

            //lines 4141-4240
            public override long calculateCityYieldValue(YieldType eYield, City pCity)
            {
                long iValue = base.calculateCityYieldValue(eYield, pCity);

/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                if (eYield != infos.Globals.GROWTH_YIELD && eYield != infos.Globals.CIVICS_YIELD && eYield != infos.Globals.TRAINING_YIELD)
                {
                    //why is yieldmodifier ignored in all but 3 cases?
                    iValue = infos.utils().modify(iValue, pCity.calculateTotalYieldModifier(eYield));
                }
/*####### Better Old World AI - Base DLL #######
  ### AI fix                             END ###
  ##############################################*/
                return iValue;
            }

            //lines 6951-6973
            protected override bool shouldRespectCitySiteOwnership(Player pOtherPlayer)
            {
                if (player == null)
                {
                    return false;
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't treat humans differently START #
  ##############################################*/
                /*
                if (!pOtherPlayer.isHuman())
                {
                    return false;
                }
                */
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't treat humans differently END ###
  ##############################################*/
                if (game.isHostile(Team, Tribe, pOtherPlayer.getTeam(), TribeType.NONE))
                {
                    return false;
                }
                if (pOtherPlayer.getTeam() != Team)
                {
                    if (game.isGameOption(infos.Globals.GAMEOPTION_PLAY_TO_WIN))
                    {
                        return false;
                    }
                    if (isOtherPlayerSneaky(pOtherPlayer))
                    {
                        return false;
                    }
                }
                return true;
            }

            //lines 6975-6993
            //now obsolete, removed part was also removed in base game
            //protected override bool shouldLeaveTileForOtherPlayer(Tile pTile)

            //lines 7025-7082
            protected override bool shouldClaimCitySite(Tile pTile)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.shouldClaimCitySite");

                if (player == null)
                {
                    return false;
                }

                if (!canEverSettle())
                {
                    return false;
                }

                if (!pTile.isCitySiteActive()) // slight cheat, but humans can usually tell if the site has been settled by other means (score, surrounding tiles, etc)
                {
                    return false;
                }

                if (!player.canFoundCity())
                {
                    return false;
                }

                if (!((BetterAIPlayer)player).getStartingTiles().Contains(pTile.getID()))
                {
                    if (!isFoundCitySafe(pTile))
                    {
                        return false;
                    }

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much     START ###
  ##############################################*/
                    //if you clear a camp and then don't guard it, that's on you.
                    /*
                    if (shouldLeaveTileForOtherPlayer(pTile))
                    {
                        return false;
                    }
                    */
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much       END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much     START ###
  ##############################################*/
                    //this part can stay, to make sure we are only checking not-yet-used starting tiles
                    if (pTile.getCitySite() == CitySiteType.ACTIVE_RESERVED)
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much       END ###
  ##############################################*/
                    {
                        // don't claim reserved sites if we already started with extra cities
                        if (!player.isHuman() && game.hasDevelopmentCities())
                        {
                            // at least for a grace period
                            if (game.getTurn() < ((game.isGameOption(infos.Globals.GAMEOPTION_PLAY_TO_WIN)) ? AI_PLAY_TO_WIN_GRACE_TURNS : AI_GRACE_TURNS))
                            {

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much     START ###
  ##############################################*/
                                //this section is now in the base game (1.0.65965)
                                //grace turns only apply for non-sneaky players, so the owner needs to be found and checked
                                for (PlayerType eLoopPlayer = 0; eLoopPlayer < game.getNumPlayers(); ++eLoopPlayer)
                                {
                                    BetterAIPlayer pLoopPlayer = (BetterAIPlayer)game.player(eLoopPlayer);

                                    if (eLoopPlayer != getPlayer())
                                    {
                                        if (pTile.isCitySiteActive(player.getTeam()) && pLoopPlayer.getStartingTiles().Contains(pTile.getID()))
                                        {
                                            if (!isOtherPlayerSneaky(pLoopPlayer))
                                            {
                                                return false;
                                            }
                                            break;
                                        }
                                    }
                                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much       END ###
  ##############################################*/
                                
                            }
                        }
                    }
                }

                return true;
            }

            //lines 11463-11855
            protected override long calculateEffectCityValue(EffectCityType eEffectCity, City pCity, bool bRemove)
            {
                if (player == null)
                {
                    return 0;
                }

                long iValue = base.calculateEffectCityValue(eEffectCity, pCity, bRemove);
/*####### Better Old World AI - Base DLL #######
  ### self-aaiEffectCityYieldRate      START ###
  ##############################################*/
                foreach (var zTriple in infos.effectCity(eEffectCity).maaiEffectCityYieldRate)
                {
                    if (zTriple.First == eEffectCity)
                    {
                        int iCount = pCity.getEffectCityCount(zTriple.First);
                        if (iCount == 0) continue; //unless effects can be part of multiple zTriples, I can probably break here

                        //iCount++; //(x+1)^2 - x^2 - x = x+1 //but for the value of removing a current effect, I'd need to do iCount--;
                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                        {
                            long iSubValue = 0;
                            if (zTriple.Second == eLoopYield)
                            {
                                iSubValue += zTriple.Third * iCount;
                                iValue += ((iSubValue * cityYieldValue(eLoopYield, pCity) * AI_YIELD_TURNS) / Constants.YIELDS_MULTIPLIER);
                            }
                        }

                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### self-aaiEffectCityYieldRate        END ###
  ##############################################*/
                return iValue;
            }

            //lines 11966-13545
            protected override long bonusValue(BonusType eBonus, ref BonusParameters zParameters)
            {
                if (player == null)
                {
                    return 0;
                }
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
                //reducing Courtier values if they can't work by 2/3
                long iPlayerValue = 0;
                foreach (KeyValuePair<CourtierType, GenderType> pair in infos.bonus(eBonus).maeAddCourtier)
                {
                    if (pair.Key != CourtierType.NONE)
                    {
                        BetterAIInfoCourtier eInfoCourtier = (BetterAIInfoCourtier)infos.courtier(pair.Key);
                        if (eInfoCourtier.maeAdjectives.Count > 0)
                        {
                            foreach (TraitType eLoopTrait in eInfoCourtier.maeAdjectives)
                            {
                                if (eLoopTrait != TraitType.NONE)
                                {
                                    if (infos.trait(eLoopTrait).mbNoJob)
                                    {
                                        iPlayerValue -= adjustForInflation(2 * AI_COURTIER_VALUE / 3);
                                        break;
                                    }
                                }
                            }
                        }



                        // XXX
                        //GoalData pGoalData = getActiveStatGoal(infos.Globals.COURTIER_ADDED_STAT);
                        //if (pGoalData != null)
                        //{
                        //    iPlayerValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                        //}
                    }
                }

                foreach (KeyValuePair<CourtierType, GenderType> pair in infos.bonus(eBonus).maeAddCourtierOther)
                {
                    if (pair.Key != CourtierType.NONE)
                    {
                        BetterAIInfoCourtier eInfoCourtier = (BetterAIInfoCourtier)infos.courtier(pair.Key);

                        if (eInfoCourtier.maeAdjectives.Count > 0)
                        {
                            foreach (TraitType eLoopTrait in eInfoCourtier.maeAdjectives)
                            {
                                if (eLoopTrait != TraitType.NONE)
                                {
                                    if (infos.trait(eLoopTrait).mbNoJob)
                                    {
                                        iPlayerValue -= adjustForInflation(2 * AI_COURTIER_VALUE / 3);
                                        break;
                                    }
                                }
                            }
                        }


                        //GoalData pGoalData = getActiveStatGoal(infos.Globals.COURTIER_ADDED_STAT);
                        //if (pGoalData != null)
                        //{
                        //    iPlayerValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                        //}
                    }
                }

                if (infos.bonus(eBonus).mbHolyCityAgents)
                {
                    iPlayerValue -= adjustForInflation(AI_HOLY_CITY_AGENT_VALUE); //subtract all of it, then add the modified number

                    //slight cheat because the player doesn't know when Pagan Religions get founded, but this should at least
                    // make Egypt pick another Wonder on Turn 1. Hopefully.
                    int iPossibleWorldReligions = 0;
                    int iWorldReligionsWithHolyCity = 0;
                    int iPossiblePaganReligions = 0;
                    int iPaganReligionsWithHolyCity = 0;
                    int iHolyCities = 0;
                    int iPossibleReligions = 0;

                    HashSet<int> nationSet = new HashSet<int>();

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < game.getNumPlayers(); ++eLoopPlayer)
                    {
                        if (game.player(eLoopPlayer).isAlive() && game.player(eLoopPlayer).hasNation())
                        {
                            nationSet.Add((int)(game.player(eLoopPlayer).getNation()));
                        }
                    }

                    for (ReligionType eLoopReligion = 0; eLoopReligion < infos.religionsNum(); eLoopReligion++)
                    {
                        if (game.hasReligionHolyCity(eLoopReligion))
                        {
                            iHolyCities++;
                            iPossibleReligions++;
                            if (infos.religion(eLoopReligion).mePaganNation == NationType.NONE)
                            {
                                iWorldReligionsWithHolyCity++;
                                iPossibleWorldReligions++;
                            }
                            else
                            {
                                iPaganReligionsWithHolyCity++;
                                iPossiblePaganReligions++;
                            }
                        }
                        else if (!(game.isReligionFounded(eLoopReligion)))
                        {
                            if (infos.religion(eLoopReligion).mePaganNation == NationType.NONE)
                            {
                                iPossibleReligions++;
                                iPossibleWorldReligions++;
                            }
                            else if (nationSet.Contains((int)(infos.religion(eLoopReligion).mePaganNation)))
                            {
                                iPossibleReligions++;
                                iPossiblePaganReligions++;
                            }
                        }

                    }

                    if ((iPossibleReligions) != 0)
                    {
                        //World Religions contribute from 0 to 0.25 * AI_HOLY_CITY_AGENT_VALUE
                        //Pagan Religions contribute from -0.5 * AI_HOLY_CITY_AGENT_VALUE to +0.75 * AI_HOLY_CITY_AGENT_VALUE
                        //Total value therefore ranges from -0.5 * AI_HOLY_CITY_AGENT_VALUE when no Religion is founded, to AI_HOLY_CITY_AGENT_VALUE when they all have Holy Cities
                        iPlayerValue += (adjustForInflation(AI_HOLY_CITY_AGENT_VALUE) * (5 * iPaganReligionsWithHolyCity - 2 * iPossiblePaganReligions)) / (4 * iPossiblePaganReligions);
                        iPlayerValue += (adjustForInflation(AI_HOLY_CITY_AGENT_VALUE) * iWorldReligionsWithHolyCity) / (4 * iPossibleWorldReligions);
                    }
                }


                //if (pOtherPlayer.getTeam() == player.getTeam())
                //{
                //    iValue += iPlayerValue;
                //}
                //else
                //{
                //    iValue -= (iPlayerValue / 2);
                //}
                //translated to
                if (iPlayerValue != 0)
                {
                    Player pOtherPlayer = zParameters.eTargetPlayer != PlayerType.NONE ? game.player(zParameters.eTargetPlayer) : player;
                    if (pOtherPlayer.getTeam() != player.getTeam())
                    {
                        iPlayerValue /= 2;
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/
                return iPlayerValue + base.bonusValue(eBonus, ref zParameters);
            }

            //copy-paste START
            //lines 17008-17183
            // Chance that we will declare war on them
            public override int getWarOfferPercent(PlayerType eOtherPlayer, bool bDeclare)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.getWarOfferPercent");

                if (player == null)
                {
                    return 0;
                }

                BetterAIPlayer pOtherPlayer = (BetterAIPlayer)game.player(eOtherPlayer);
                PlayerType eThisPlayer = getPlayer();

                if (!(player.canDeclareWar(pOtherPlayer)))
                {
                    return 0;
                }

                if (pOtherPlayer.isGivingTributeToPlayer(eThisPlayer, AI_TRIBUTE_NO_WAR_TURNS))
                {
                    return 0;
                }

                if (player.countPlayerMemories(infos.Globals.PLAYER_WAR_MEMORY) > 0)
                {
                    return 0;
                }

                if (!isPlayerCityReachable(eOtherPlayer))
                {
                    return 0;
                }

                ProximityType eProximity = pOtherPlayer.calculateProximityPlayer(eThisPlayer);
                PowerType eStrength = pOtherPlayer.calculatePowerOf(eThisPlayer);

                bool bPlayToWin = game.isPlayToWinVs(eOtherPlayer);

                if (!bPlayToWin)
                {
                    if (game.isPlayToWinAny())
                    {
                        return 0;
                    }

                    if (bDeclare)
                    {
                        if (game.getTurn() < (20 + ((pOtherPlayer.isHuman()) ? game.opponentLevel().miStartWarMinTurn : 0)))
                        {
                            return 0;
                        }

                        if (!(pOtherPlayer.playerOpinionOfUs(eThisPlayer).mbDeclareWar))
                        {
                            return 0;
                        }

                        if (!(infos.proximity(eProximity).mbDeclareWar))
                        {
                            return 0;
                        }

                        if (!(infos.power(eStrength).mbDeclareWar))
                        {
                            return 0;
                        }

                        if (player.countTeamWars() > 1)
                        {
                            return 0;
                        }

                        if (!areAllCitiesDefended())
                        {
                            return 0;
                        }
                    }
                }

                int iPercent = pOtherPlayer.playerOpinionOfUs(eThisPlayer).miWarPercent;

/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer      START ###
  ##############################################*/
                int iMulti = 640;
                int iMultiHalf = 320;
                iPercent *= iMulti;
/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer        END ###
  ##############################################*/

                if (bPlayToWin)
                {
                    if (pOtherPlayer.isCloseToWinning(5))
                    {
                        iPercent = infos.utils().modify(iPercent, 50);
                    }
                    else if (pOtherPlayer.isCloseToWinning(10))
                    {
                        iPercent = infos.utils().modify(iPercent, 40);
                    }
                    else if (pOtherPlayer.isCloseToWinning(15))
                    {
                        iPercent = infos.utils().modify(iPercent, 30);
                    }
                    else if (pOtherPlayer.isCloseToWinning(20))
                    {
                        iPercent = infos.utils().modify(iPercent, 20);
                    }
                    else
                    {
                        iPercent = infos.utils().modify(iPercent, 10);
                    }
                }

                iPercent = infos.utils().modify(iPercent, infos.proximity(eProximity).miWarModifier);
                iPercent = infos.utils().modify(iPercent, infos.power(eStrength).miWarModifier);

                iPercent = infos.utils().modify(iPercent, game.teamDiplomacy(eThisPlayer, eOtherPlayer).miWarModifier);

                {
                    Character pLeader = player.leader();

                    if (pLeader != null)
                    {
                        foreach (TraitType eLoopTrait in pLeader.getTraits())
                        {
                            iPercent = infos.utils().modify(iPercent, infos.trait(eLoopTrait).miWarModifier);
                        }
                    }
                }

                if (pOtherPlayer.isHuman())
                {
                    iPercent = infos.utils().modify(iPercent, game.opponentLevel().miWarModifier);

                    if (game.getTurn() > 20)
                    {
                        int iScore = game.getTeamWarScoreTotalAll(pOtherPlayer.getTeam());

                        if (iScore < 10)
                        {
                            iPercent *= 5;
                            iPercent /= 4;
                        }
                        else if (iScore > 100)
                        {
                            if (!bPlayToWin)
                            {
                                iPercent *= 4;
                                iPercent /= 5;
                            }
                        }
                    }
                }

                iPercent *= (5 + pOtherPlayer.countTeamWars());
                iPercent /= (5 + 0);

                iPercent /= (player.countTeamWars() + 1);

                if (game.hasTeamAlliance(pOtherPlayer.getTeam()))
                {
                    iPercent *= 4;
                    iPercent /= 5;
                }

                if (!bPlayToWin)
                {
                    if (bDeclare)
                    {
                        iPercent *= 2;
                        iPercent /= 3;
                    }

                    for (TeamType eLoopTeam = 0; eLoopTeam < game.getNumTeams(); eLoopTeam++)
                    {
                        if ((eLoopTeam != Team) && (eLoopTeam != pOtherPlayer.getTeam()))
                        {
                            if (game.isTeamAlive(eLoopTeam))
                            {
                                if (game.teamDiplomacy(eLoopTeam, Team).mbHostile && game.teamDiplomacy(eLoopTeam, pOtherPlayer.getTeam()).mbHostile)
                                {
                                    iPercent /= 2;
                                }
                            }
                        }
                    }
                }


/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer      START ###
  ##############################################*/
                iPercent = (iPercent + iMultiHalf) / iMulti; //rounding
/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer        END ###
  ##############################################*/
                return infos.utils().range(iPercent, 0, 100);
            }
            //copy-paste END

        }
    }

}
