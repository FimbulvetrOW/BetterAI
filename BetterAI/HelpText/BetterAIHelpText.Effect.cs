using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using static TenCrowns.GameCore.Text.TextExtensions;

namespace BetterAI
{
    public partial class BetterAIHelpText : HelpText
    {

        //lines 244-331
        public override TextVariable buildEffectPlayerSourceLinkVariable(EffectPlayerType eEffectPlayer, Game pGame, Player pPlayer = null)
        {
            //UnityEngine.Debug.Log("HelpText.buildEffectPlayerSourceLinkVariable");

            //this method assumes that there is only one source for the player effect
            TextVariable result = base.buildEffectPlayerSourceLinkVariable(eEffectPlayer, pGame, pPlayer);

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
            BetterAIInfoEffectPlayer pInfoEffectPlayer = ((BetterAIInfoEffectPlayer)infos().effectPlayer(eEffectPlayer));
            if (result == null)
            {
                CommaListVariableGenerator jobs = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR_LINEBREAK, CommaListVariableGenerator.EncloseType.NONE, bEncloseSingleItem: false, TextManager);
                if (pInfoEffectPlayer.mseSourceTraitJobs.Count > 0)
                {
                    foreach (JobType eLoopJob in pInfoEffectPlayer.mseSourceTraitJobs)
                    {
                        CommaListVariableGenerator traits = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR, CommaListVariableGenerator.EncloseType.PARENTHESIS, bEncloseSingleItem: false, TextManager);
                        BetterAIInfoJob pLoopInfoJob = ((BetterAIInfoJob)infos().job(eLoopJob));
                        foreach(TraitType eLoopTrait in pLoopInfoJob.mdlEffectPlayerTraits[eEffectPlayer])
                        {
                            traits.AddItem(buildTraitLinkVariable(eLoopTrait));
                        }

                        jobs.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", traits.Finalize(), buildJobLinkVariable(eLoopJob)));
                    }
                }

                if (pInfoEffectPlayer.mseSourceTraitTraits.Count > 0)
                {
                    foreach (TraitType eLoopTrait in pInfoEffectPlayer.mseSourceTraitTraits)
                    {
                        CommaListVariableGenerator traits = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR, CommaListVariableGenerator.EncloseType.PARENTHESIS, bEncloseSingleItem: false, TextManager);
                        BetterAIInfoTrait pLoopInfoTrait = ((BetterAIInfoTrait)infos().trait(eLoopTrait));
                        foreach (TraitType eInnerLoopTrait in pLoopInfoTrait.mleEffectPlayerTraits[eEffectPlayer])
                        {
                            traits.AddItem(buildTraitLinkVariable(eInnerLoopTrait));
                        }

                        jobs.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", traits.Finalize(), buildTraitLinkVariable(eLoopTrait)));
                    }

                }

                if (pInfoEffectPlayer.mseSourceTraitJobs.Count + pInfoEffectPlayer.mseSourceTraitTraits.Count > 0)
                {
                    result = TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_EFFECT_PLAYER_SOURCE_TRAIT", jobs.Finalize());
                }
            }

            if (result == null)
            {
                if (pInfoEffectPlayer.maeeSourceEffectPlayers.Count > 0)
                {
                    CommaListVariableGenerator effectPlayers = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR, CommaListVariableGenerator.EncloseType.PARENTHESIS, bEncloseSingleItem: false, TextManager);
                    foreach ((EffectPlayerType, EffectPlayerType) p in pInfoEffectPlayer.maeeSourceEffectPlayers)
                    {
                        if (pPlayer == null || (pPlayer.canEverHaveEffectPlayer(p.Item1) && pPlayer.canEverHaveEffectPlayer(p.Item2)))
                        {
                            effectPlayers.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_AND_TWO", buildEffectPlayerLinkVariable(p.Item1, pPlayer), buildEffectPlayerLinkVariable(p.Item2, pPlayer)));
                        }

                    }
                    result = effectPlayers.Finalize();
                }
            }

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/
            return result;
        }

        //lines 1740-2525
        public override TextBuilder buildEffectCityHelpNoYields(TextBuilder builder, EffectCityType eEffectCity, Game pGame, List<(City, Character, int)> apCities, Player pPlayer, Player pActivePlayer)
        {
            //UnityEngine.Debug.Log("HelpText.buildEffectCityHelpNoYields");

            builder = base.buildEffectCityHelpNoYields(builder, eEffectCity, pGame, apCities, pPlayer, pActivePlayer);

/*####### Better Old World AI - Base DLL #######
  ### mbEnablesGovernor restoration    START ###
  ##############################################*/
            if (((BetterAIInfoEffectCity)infos().effectCity(eEffectCity)).mbEnablesGovernor)
            {
                if (pGame?.isCharacters() ?? true)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_NO_YIELDS_ENABLES_GOVERNOR", buildGovernorLinkVariable());
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### mbEnablesGovernor restoration      END ###
  ##############################################*/

            return builder;
        }

        //lines 2859-3998
        public override TextBuilder buildEffectPlayerHelp(TextBuilder builder, EffectPlayerType eEffectPlayer, Game pGame, Player pPlayer, Player pActivePlayer, ReligionType eStateReligion = ReligionType.NONE, bool bAllCities = false, bool bShowTotal = false, TextBuilder.ScopeType effectCityScopeType = TextBuilder.ScopeType.COMMA)
        {
            //UnityEngine.Debug.Log("HelpText.buildEffectPlayerHelp");

            using (var ignoreEffectPlayerListScoped = CollectionCache.GetListScoped<EffectPlayerType>())
            {
                List<EffectPlayerType> effectPlayerIgnore = ignoreEffectPlayerListScoped.Value;
                return buildEffectPlayerHelp(builder, eEffectPlayer, pGame, pPlayer, pActivePlayer, ref effectPlayerIgnore, eStateReligion, bAllCities, bShowTotal, effectCityScopeType);
            }
        }

        public virtual TextBuilder buildEffectPlayerHelp(TextBuilder builder, EffectPlayerType eEffectPlayer, Game pGame, Player pPlayer, Player pActivePlayer, ref List<EffectPlayerType> effectPlayerIgnore, ReligionType eStateReligion = ReligionType.NONE, bool bAllCities = false, bool bShowTotal = false, TextBuilder.ScopeType effectCityScopeType = TextBuilder.ScopeType.COMMA)
        {
            //UnityEngine.Debug.Log("HelpText.buildEffectPlayerHelp");

            //using (new UnityProfileScope("HelpText.buildEffectPlayerHelp"))

            if (pPlayer == null || (pActivePlayer != null && (pActivePlayer.getTeam() != pPlayer.getTeam() || !pActivePlayer.isPlayerOption(infos().Globals.ADVANCED_HELP))))
            {
                bShowTotal = false;
            }

            BetterAIInfoEffectPlayer pInfoEffectPlayer = (BetterAIInfoEffectPlayer)infos().effectPlayer(eEffectPlayer);
            {
                City pCapitalCity = pPlayer?.capitalCity();

                {
                    EffectPlayerType eEffectPlayerUnlock = pInfoEffectPlayer.meEffectPlayer;

                    if (eEffectPlayerUnlock != EffectPlayerType.NONE && !effectPlayerIgnore.Contains(eEffectPlayerUnlock))
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_UNLOCKS_EFFECT", buildEffectPlayerLinkVariable(eEffectPlayerUnlock, pPlayer));
                    }
                }

                {
                    BonusType eStartBonus = pInfoEffectPlayer.meStartBonus;

                    if (eStartBonus != BonusType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildBonusHelp(subText, eStartBonus, pGame, pPlayer, pActivePlayer, bName: false, bShowCity: false, startLineVariable: TEXTVAR(""));
                            }

                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_START_BONUS", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    BonusType eFoundBonus = pInfoEffectPlayer.meFoundBonus;

                    if (eFoundBonus != BonusType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildBonusHelp(subText, eFoundBonus, pGame, pPlayer, pActivePlayer, bName: false, bShowCity: false, startLineVariable: TEXTVAR(""));
                            }

                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_FOUND_BONUS", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    int iValue = infos().effectPlayer(eEffectPlayer).miLegitimacy;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_LEGITIMACY", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miVP;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_VPS", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miMaxOrders;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_MAX_ACTIONS", TEXTVAR(iValue), buildTurnScaleNamePlural(pGame));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miMaxCities;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_MAX_CITIES", TEXTVAR(iValue), buildTurnScaleNamePlural(pGame));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miXPAllTurn;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ALL_UNIT_XP", buildSignedTextVariable(iValue), buildTurnScaleName(pGame));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miXPIdleTurn;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_IDLE_UNIT_XP", buildSignedTextVariable(iValue), buildTurnScaleName(pGame), buildIdleLinkVariable());
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miXPModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_XP_COMBAT_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miHarvestModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_HARVEST_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miSellPenaltyModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_SELL_PENALTY_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miConsumptionModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CONSUMPTION_MODIFIER", buildSignedTextVariable(iValue, true));

                        if (bShowTotal)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                    {
                                        int iBase = 0;
                                        for (int i = 0; i < pPlayer.getNumUnits(); ++i)
                                        {
                                            Unit pUnit = pPlayer.unitAt(i);
                                            if (pUnit != null)
                                            {
                                                iBase += pUnit.info().maiYieldConsumption[eLoopYield];
                                            }
                                        }
                                        if (iBase != 0)
                                        {
                                            iBase *= Constants.YIELDS_MULTIPLIER;
                                            subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iBase - infos().utils().modify(iBase, iValue), iMultiplier: Constants.YIELDS_MULTIPLIER));
                                        }
                                    }
                                }
                                if (subText.HasContent)
                                {
                                    builder.AddWithParenthesis(subText.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miWonderModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_WONDER_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miRepairModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_REPAIR_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miMissionModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_MISSION_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miStartLawModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_START_LAW_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miSwitchLawModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_SWITCH_LAW_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miSwitchLawMaximum;
                    if (iValue > 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_SWITCH_LAW_MAXIMUM", buildYieldValueIconLinkVariable(infos().Globals.CIVICS_YIELD, iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miGovernorCostModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_GOVERNOR_COST_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miYieldUpkeepModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_YIELD_UPKEEP_MODIFIER", buildPercentTextValue(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miTrainingOrderModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TRAINING_MODIFIER", buildSignedTextVariable(iValue, true), buildYieldIconLinkVariable(infos().Globals.TRAINING_YIELD), buildYieldTextVariable(1, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miBuildModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_BUILD_TURN_MODIFIER", buildSignedTextVariable(iValue, true), buildTurnScaleName(pGame, iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miBuildTurnChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_BUILD_TURN_CHANGE", buildSignedTextVariable(iValue), buildTurnScaleName(pGame, iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miVisionChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_VISION_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miTribeFatigueChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TRIBE_FATIGUE_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miTechCostModifier;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TECH_COST_MODIFIER", buildSignedTextVariable(iValue, true));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miTechsAvailableChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TECH_AVAILABLE_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miStateReligionSpread;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_STATE_RELIGION_SPREAD_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = infos().effectPlayer(eEffectPlayer).miWorldReligionSpread;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_WORLD_RELIGION_SPREAD_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miLeaderOpinionChange;
                    if (iValue != 0)
                    {
                        if ((pGame != null) && pGame.originalAllHumanTeams())
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HEAD_OPINION_CHANGE", buildSignedTextVariable(iValue));
                        }
                        else
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_LEADER_OPINION_CHANGE", buildSignedTextVariable(iValue));
                        }
                    }
                }

                {
                    int iValue = infos().effectPlayer(eEffectPlayer).miLeaderDescendantOpinionChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_LEADER_DESCENDANT_OPINION_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miTribeLeaderOpinionChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_TRIBE_LEADER_OPINION_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miReligionOpinionChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_RELIGION_OPINION_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miStateReligionOpinionChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_STATE_RELIGION_OPINION_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miLeaderReligionOpinionChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_LEADER_RELIGION_OPINION_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                {
                    int iValue = pInfoEffectPlayer.miFamilyOpinionChange;
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_FAMILY_OPINION_CHANGE", buildSignedTextVariable(iValue));
                    }
                }

                if (pInfoEffectPlayer.mbStartMusic)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_START_MUSIC");
                }

                if (pInfoEffectPlayer.mbRedrawTechs)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CAN_REDRAW_TECHS");
                }

                if (pInfoEffectPlayer.mbAddRoad)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CAN_ADD_ROAD", buildRoadLinkVariable());
                }

                if (pInfoEffectPlayer.mbAddUrban)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CAN_ADD_URBAN", buildUrbanLinkVariable());
                }

                if (infos().effectPlayer(eEffectPlayer).mbRemoveAllVegetation)
                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_CAN_CHOP_TERRITORY")))
                    {
                        for (VegetationType eLoopVegetation = 0; eLoopVegetation < infos().vegetationNum(); ++eLoopVegetation)
                        {
                            if (infos().vegetation(eLoopVegetation).mbRequiresUnlock)
                            {
                                builder.Add(buildVegetationLinkVariable(eLoopVegetation, true));
                            }
                        }
                    }
                }

                if (pInfoEffectPlayer.mbUpgradeImprovement)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CAN_UPGRADE_IMPROVEMENT");
                }

                if (pInfoEffectPlayer.mbMultipleWorkers)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_MULTIPLER_WORKERS", buildConnectedLinkVariable());
                }

                if (pInfoEffectPlayer.mbMoveAlliedUnits)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_MOVE_ALLIED_UNITS");
                }

                if (pInfoEffectPlayer.mbAgent)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CAN_AGENT", buildAgentNetworkLinkVariable());
                }

                if (pInfoEffectPlayer.mbRiverMovement)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_RIVER_MOVEMENT_BONUS", buildRiverLinkVariable());
                }

                if (pInfoEffectPlayer.mbRiverBridging)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_RIVER_BRIDGING_BONUS", buildRiverLinkVariable());
                }

                if (pInfoEffectPlayer.mbNoSellPenalty)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_SELL_PENALTY");
                }

                if (pInfoEffectPlayer.mbNoOutsideConsumption)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_OUTSIDE_UNIT_CONSUMPTION", buildUnitConsumptionOutsideLinkVariable(pPlayer));
                }

                if (pInfoEffectPlayer.mbPurgeReligions)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_PURGE_RELIGIONS");
                }

                if (pInfoEffectPlayer.mbBuildAllReligions)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_BUILD_ALL_RELIGIONS");
                }

                if (pInfoEffectPlayer.mbPaganStateReligion)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_PAGAN_STATE_RELIGION");
                }

                if (pInfoEffectPlayer.mbLegitimacyOrders)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_LEGITIMACY_ORDERS", buildLegitimacyLinkVariable(pPlayer));
                }

                if (pInfoEffectPlayer.mbOrdersScience)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ORDERS_SCIENCE");
                }

                if (pInfoEffectPlayer.mbRecruitMercenaries)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_RECRUIT_MERCENARIES", buildLegitimacyLinkVariable(pPlayer));
                }

                if (pInfoEffectPlayer.mbHireMercenaries)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_HIRE_MERCENARIES");
                }

                if (pInfoEffectPlayer.mbNoStartWars)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_START_WARS");
                }

                if (pInfoEffectPlayer.mbNoEndWars)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_END_WARS");
                }

                if (pInfoEffectPlayer.mbAutomateWorkers)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_AUTOMATE_WORKERS");
                }

                if (pInfoEffectPlayer.mbAutomateScouts)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_AUTOMATE_SCOUTS");
                }

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = pInfoEffectPlayer.maiWarYield[eLoopYield];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_WAR_YIELD", buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));

                        if (bShowTotal)
                        {
                            int iNumWars = pPlayer.countTeamWars() + pPlayer.countTribeWars();
                            if (iNumWars > 0)
                            {
                                builder.AddWithParenthesis(buildYieldValueIconLinkVariable(eLoopYield, iValue * iNumWars, iMultiplier: Constants.YIELDS_MULTIPLIER));
                            }
                        }
                    }

                    foreach ((DiplomacyType eDiplomacy, YieldType eYield, int iAmount) in infos().effectPlayer(eEffectPlayer).mlpTeamDiplomacyYields)
                    {
                        if (eYield == eLoopYield)
                        {
                            if (iAmount != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_DIPLOMACY_YIELD", buildYieldValueIconLinkVariable(eLoopYield, iAmount, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame), buildDiplomacyLinkVariable(eDiplomacy, false));

                                if (bShowTotal)
                                {
                                    int iNumDiplomacy = pPlayer.countTeamDiplomacy(eDiplomacy);
                                    if (iNumDiplomacy > 0)
                                    {
                                        builder.AddWithParenthesis(buildYieldValueIconLinkVariable(eLoopYield, iValue * iNumDiplomacy, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }
                    }

                    foreach ((DiplomacyType eDiplomacy, YieldType eYield, int iAmount) in infos().effectPlayer(eEffectPlayer).mlpTribeDiplomacyYields)
                    {
                        if (eYield == eLoopYield)
                        {
                            if (iAmount != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TRIBE_DIPLOMACY_YIELD", buildYieldValueIconLinkVariable(eLoopYield, iAmount, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame), buildDiplomacyLinkVariable(eDiplomacy, true));

                                if (bShowTotal)
                                {
                                    int iNumDiplomacy = pPlayer.countTribeDiplomacy(eDiplomacy);
                                    if (iNumDiplomacy > 0)
                                    {
                                        builder.AddWithParenthesis(buildYieldValueIconLinkVariable(eLoopYield, iValue * iNumDiplomacy, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }
                    }
                }

                {
                    CommaListVariableGenerator yieldsList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);

                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                    {
                        int iValue = infos().effectPlayer(eEffectPlayer).maiMissionYieldCostModifier[eLoopYield];
                        if (iValue != 0)
                        {
                            yieldsList.AddItem(buildYieldValueIconLinkVariable(eLoopYield, iValue, bPercent: true));
                        }
                    }

                    if (yieldsList.Count > 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_MISSION_COST_YIELD", TEXTVAR(bAllCities), yieldsList.Finalize(), buildTurnScaleName(pGame));
                    }
                }

                {
                    TenCrowns.GameCore.HelpText.CommaListVariableGenerator yieldsList = getYieldsList(infos().effectPlayer(eEffectPlayer).maiYieldRate);

                    if (yieldsList.Count > 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_YIELD_RATE", TEXTVAR(bAllCities), yieldsList.Finalize(), buildTurnScaleName(pGame));
                    }
                }

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = pInfoEffectPlayer.maiYieldRateLaws[eLoopYield];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_YIELD_RATE_LAWS", TEXTVAR(bAllCities), buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));

                        if (bShowTotal)
                        {
                            int iNumLaws = pPlayer.countActiveLaws();
                            if (iNumLaws > 0)
                            {
                                builder.AddWithParenthesis(buildYieldValueIconLinkVariable(eLoopYield, iValue * iNumLaws, iMultiplier: Constants.YIELDS_MULTIPLIER));
                            }
                        }
                    }
                }

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = pInfoEffectPlayer.maiYieldRateGenerals[eLoopYield];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_YIELD_RATE_GENERALS", TEXTVAR(bAllCities), buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));

                        if (bShowTotal)
                        {
                            int iNumGenerals = pPlayer.countGenerals();
                            if (iNumGenerals > 0)
                            {
                                builder.AddWithParenthesis(buildYieldValueIconLinkVariable(eLoopYield, iValue * iNumGenerals, iMultiplier: Constants.YIELDS_MULTIPLIER));
                            }
                        }
                    }
                }

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = pInfoEffectPlayer.maiYieldUpkeep[eLoopYield];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_YIELD_UPKEEP", buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));

                        if (bShowTotal)
                        {
                            int iNumCities = pPlayer.getNumCities();
                            if (iNumCities > 0)
                            {
                                builder.AddWithParenthesis(buildYieldValueIconLinkVariable(eLoopYield, infos().utils().modify(iValue * iNumCities, pPlayer.getYieldUpkeepModifier()), iMultiplier: Constants.YIELDS_MULTIPLIER));
                            }
                        }
                    }
                }

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                {
                    int iValue = pInfoEffectPlayer.maiUnitTraitConsumptionModifier[eLoopUnitTrait];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_UNIT_TRAIT_CONSUMPTION_MODIFIER", buildUnitTraitLinkVariable(eLoopUnitTrait), buildSignedTextVariable(iValue, true));


                        if (bShowTotal)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                    {
                                        int iBase = 0;
                                        for (int i = 0; i < pPlayer.getNumUnits(); ++i)
                                        {
                                            Unit pUnit = pPlayer.unitAt(i);
                                            if (pUnit != null && pUnit.info().maeUnitTrait.Contains(eLoopUnitTrait))
                                            {
                                                iBase += pUnit.info().maiYieldConsumption[eLoopYield];
                                            }
                                        }
                                        if (iBase != 0)
                                        {
                                            iBase *= Constants.YIELDS_MULTIPLIER;
                                            subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iBase - infos().utils().modify(iBase, iValue), iMultiplier: Constants.YIELDS_MULTIPLIER));
                                        }
                                    }
                                }
                                if (subText.HasContent)
                                {
                                    builder.AddWithParenthesis(subText.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                for (JobType eLoopJob = 0; eLoopJob < infos().jobsNum(); eLoopJob++)
                {
                    int iValue = pInfoEffectPlayer.maiJobOpinionRate[eLoopJob];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_OPINION_COLON", buildJobLinkVariable(eLoopJob), buildSignedTextVariable(iValue));
                    }
                }

                for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                {
                    int iValue = infos().effectPlayer(eEffectPlayer).maiImprovementClassPrereqLawsChange[eLoopImprovementClass];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_IMPROVEMENT_LAWS_REQUIRED_CHANGE", buildSignedTextVariable(iValue), buildImprovementClassLinkVariable(eLoopImprovementClass));
                    }
                }

                foreach (YieldType eLoopYield in pInfoEffectPlayer.maeTradeYield)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TRADE_YIELD", buildYieldLinkVariable(eLoopYield));
                }

                foreach (YieldType eLoopYield in infos().effectPlayer(eEffectPlayer).maeNoBuyYield)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_BUY_YIELD", buildYieldLinkVariable(eLoopYield));
                }


                foreach (YieldType eLoopYield in pInfoEffectPlayer.maeNoSellPenaltyYield)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_SELL_PENALTY_YIELD", buildYieldLinkVariable(eLoopYield));
                }

                foreach (YieldType eLoopYield in pInfoEffectPlayer.maeConnectedForeign)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CONNECTED_FOREIGN", buildYieldLinkVariable(eLoopYield));
                    if (bShowTotal)
                    {
                        using (TextBuilder subBuilder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            for (TeamType eLoopTeam = 0; eLoopTeam < pGame.getNumTeams(); eLoopTeam++)
                            {
                                int iFromConnection = pPlayer.getConnectedForeignYieldTotal(eLoopYield, eLoopTeam, bPreview: !pPlayer.isConnectedForeignUnlock(eLoopYield));
                                if (iFromConnection != 0)
                                {
                                    TextVariable yieldVariable = buildColorTextOptionalVariable(buildYieldValueIconLinkVariable(eLoopYield, iFromConnection, bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER), !(infos().Helpers.yieldWarning(eLoopYield, iFromConnection)));
                                    TextVariable previewVariable = pPlayer.isConnectedForeignUnlock(eLoopYield) ? TEXTVAR("") : TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CONNECTED_FOREIGN_WILL_GAIN");
                                    buildBonusLine(subBuilder, previewVariable, TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CONNECTED_FOREIGN_FROM_TEAM", buildTeamLinkVariable(eLoopTeam, pGame, pActivePlayer, true), yieldVariable));
                                }
                            }

                            if (subBuilder.HasContent)
                            {
                                builder.Add(subBuilder.ToTextVariable());
                            }
                        }
                    }
                }

                foreach (YieldType eLoopYield in pInfoEffectPlayer.maeBuyTile)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CAN_BUY_TILES", buildBuyTilesLinkVariable(), buildYieldLinkVariable(eLoopYield));
                }

                foreach (UnitType eLoopUnit in pInfoEffectPlayer.maeWaterUnit)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_WATER_UNIT", buildUnitTypeLinkVariable(eLoopUnit, pGame));
                }

                foreach (UnitType eLoopUnit in pInfoEffectPlayer.maeInvisibleUnit)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_INVISIBLE_UNIT", buildUnitTypeLinkVariable(eLoopUnit, pGame));
                }

                foreach (ImprovementType eLoopImprovement in pInfoEffectPlayer.maeImprovementSpreadBorders)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_IMPROVEMENT_SPREAD_BORDER", buildImprovementLinkVariable(eLoopImprovement, pGame));
                }

                foreach (JobType eLoopJob in pInfoEffectPlayer.maeNoFamilyRestrictionJob)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_NO_FAMILY_RESTRICTION_JOB", buildJobLinkVariable(eLoopJob));
                }

                foreach (FamilyType eLoopFamily in pInfoEffectPlayer.maeForceFamily)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_FORCE_FAMILY", buildFamilyLinkVariable(eLoopFamily, pGame));
                }

                {
                    for (TechType eLoopTech = 0; eLoopTech < infos().techsNum(); eLoopTech++)
                    {
                        if (infos().tech(eLoopTech).meEffectPlayerPrereq == eEffectPlayer)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_UNLOCKS_EFFECT", buildTechLinkVariable(eLoopTech));
                        }
                    }
                }

                {
                    for (ProjectType eLoopProject = 0; eLoopProject < infos().projectsNum(); eLoopProject++)
                    {
                        if (infos().project(eLoopProject).meEffectPlayerPrereq == eEffectPlayer)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_UNLOCKS_PROJECT", buildProjectLinkVariable(eLoopProject, null));
                        }
                    }
                }

                {
                    for (ProjectType eLoopProject = 0; eLoopProject < infos().projectsNum(); eLoopProject++)
                    {
                        if (infos().project(eLoopProject).meCapitalEffectPlayerPrereq == eEffectPlayer)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_UNLOCKS_PROJECT_CAPITAL", buildCapitalLinkVariable(pCapitalCity), buildProjectLinkVariable(eLoopProject, null));
                        }
                    }
                }

                {
                    EffectCityType eEffectCity = pInfoEffectPlayer.meEffectCity;

                    if (eEffectCity != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelpYieldsAll(subText, eEffectCity, pGame, pPlayer, null, pActivePlayer, bShowTotal);
                            }

                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ALL_CITIES_EFFECT", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    EffectCityType eEffectCity = pInfoEffectPlayer.meEffectCity;

                    if (eEffectCity != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelpNoYieldsAll(subText, eEffectCity, pGame, pPlayer, null, pActivePlayer);
                            }

                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ALL_CITIES_EFFECT", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    EffectCityType eEffectCity = pInfoEffectPlayer.meEffectCity;

                    if (eEffectCity != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelpYieldsPotentialAll(subText, eEffectCity, pGame, pPlayer, null, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);
                            }

                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ALL_CITIES_EFFECT", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    EffectCityType eEffectCityExtra = pInfoEffectPlayer.meEffectCityExtra;

                    if (eEffectCityExtra != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelpYieldsAll(subText, eEffectCityExtra, pGame, pPlayer, null, pActivePlayer, bShowTotal);
                            }

                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ALL_CITIES_EFFECT", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    EffectCityType eEffectCityExtra = pInfoEffectPlayer.meEffectCityExtra;

                    if (eEffectCityExtra != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelpNoYieldsAll(subText, eEffectCityExtra, pGame, pPlayer, null, pActivePlayer);
                            }

                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ALL_CITIES_EFFECT", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    EffectCityType eEffectCityExtra = pInfoEffectPlayer.meEffectCityExtra;

                    if (eEffectCityExtra != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelpYieldsPotentialAll(subText, eEffectCityExtra, pGame, pPlayer, null, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);
                            }

                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ALL_CITIES_EFFECT", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    EffectCityType eCapitalEffectCity = pInfoEffectPlayer.meCapitalEffectCity;

                    if (eCapitalEffectCity != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelp(subText, eCapitalEffectCity, pCapitalCity, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);
                            }

                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CAPITAL_EFFECT", subText.ToTextVariable());
                        }
                    }
                }

                {
                    EffectCityType eConnectedEffectCity = pInfoEffectPlayer.meConnectedEffectCity;

                    if (eConnectedEffectCity != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelpAll(subText, eConnectedEffectCity, pGame, pPlayer, x => x.isConnected() ? 1 : 0, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);
                            }

                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CONNECTED_EFFECT", subText.ToTextVariable());
                        }
                    }
                }

                {
                    EffectCityType eNoGovernorEffectCity = infos().effectPlayer(eEffectPlayer).meNoGovernorEffectCity;

                    if (eNoGovernorEffectCity != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                buildEffectCityHelpAll(subText, eNoGovernorEffectCity, pGame, pPlayer, x => x.hasGovernor() ? 0 : 1, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);
                            }

                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_GOVERNOR_EFFECT", subText.ToTextVariable());
                        }
                    }
                }

                {
                    EffectCityType eStateReligionEffectCity = pInfoEffectPlayer.meStateReligionEffectCity;

                    if (eStateReligionEffectCity != EffectCityType.NONE)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            if (eStateReligion != ReligionType.NONE)
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    buildEffectCityHelpAll(subText, eStateReligionEffectCity, pGame, pPlayer, x => x.isReligion(eStateReligion) ? 1 : 0, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);
                                }
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_RELIGIOUS_CITIES_EFFECT", buildReligionLinkVariable(eStateReligion, pGame, pActivePlayer, false), subText.ToTextVariable());
                            }
                            else
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    buildEffectCityHelp(subText, eStateReligionEffectCity, pGame, pPlayer, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);
                                }
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_STATE_RELIGION_CITIES_EFFECT", subText.ToTextVariable());
                            }
                        }
                    }
                }

                {
                    //foreach ((FamilyClassType eFamilyClass, EffectCityType eFamilyEffect) in infos().effectPlayer(eEffectPlayer).maeFamilyClassEffectCity)
                    foreach (KeyValuePair<FamilyClassType, EffectCityType> p in infos().effectPlayer(eEffectPlayer).maeFamilyClassEffectCity)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                //buildEffectCityHelpAll(subText, eFamilyEffect, pGame, pPlayer, x => (x.getFamilyClass() == eFamilyClass) ? 1 : 0, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);
                                buildEffectCityHelpAll(subText, p.Value, pGame, pPlayer, x => (x.getFamilyClass() == p.Key) ? 1 : 0, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowTotal, bSkipZero: false);

                            }

                            //builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_FAMILY_CLASS_CITIES_EFFECT", buildFamilyClassLinkVariable(eFamilyClass), subText.ToTextVariable());
                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_FAMILY_CLASS_CITIES_EFFECT", buildFamilyClassLinkVariable(p.Key), subText.ToTextVariable());
                        }
                    }
                }

                {
                    EffectUnitType eEffectUnit = pInfoEffectPlayer.meEffectUnit;

                    if (eEffectUnit != EffectUnitType.NONE)
                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ALL_UNITS_EFFECT")))
                        {
                            buildEffectUnitHelp(builder, eEffectUnit, pGame);
                        }
                    }
                }

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                {
                    EffectUnitType eEffectUnit = pInfoEffectPlayer.maeEffectUnitTrait[eLoopUnitTrait];

                    if (eEffectUnit != EffectUnitType.NONE)
                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TRAIT_UNITS_EFFECT", buildUnitTraitLinkVariable(eLoopUnitTrait))))
                        {
                            buildEffectUnitHelp(builder, eEffectUnit, pGame);
                        }
                    }
                }

                for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                {
                    EffectUnitType eEffectUnit = pInfoEffectPlayer.maeEffectUnit[eLoopUnit];

                    if (eEffectUnit != EffectUnitType.NONE)
                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_UNITS_EFFECT", buildUnitTypeLinkVariable(eLoopUnit, pGame))))
                        {
                            buildEffectUnitHelp(builder, eEffectUnit, pGame);
                        }
                    }
                }

                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                {
                    if (infos().improvement(eLoopImprovement).meEffectPlayerPrereq == eEffectPlayer)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_TECH_UNLOCKS_IMPROVEMENT", buildImprovementLinkVariable(eLoopImprovement, pGame));
                    }
                }

                foreach ((ReligionType eReligion, BonusType eBonus) pLoopPair in pInfoEffectPlayer.mlpReligionSpreadBonus)
                {
                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            buildBonusHelp(subText, pLoopPair.eBonus, pGame, pPlayer, pActivePlayer, bName: false, bShowCity: false, startLineVariable: TEXTVAR(""));
                        }

                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_RELIGION_SPREAD_BONUS", buildReligionLinkVariable(pLoopPair.eReligion, pGame, pActivePlayer), subText.ToTextVariable());
                    }
                }

                foreach ((StatType eStat, BonusType eBonus) pLoopPair in pInfoEffectPlayer.mlpStatBonus)
                {
                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            buildBonusHelp(subText, pLoopPair.eBonus, pGame, pPlayer, pActivePlayer, bName: false, bShowCity: false, startLineVariable: TEXTVAR(""), bDetails: false);
                        }

                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_STAT_BONUS", TEXTVAR_TYPE(infos().stat(pLoopPair.eStat).mName, TEXTVAR(1)), subText.ToTextVariable());
                    }
                }

                foreach ((MissionResultType eMissionResult, BonusType eBonus) pLoopPair in pInfoEffectPlayer.mlpMissionPlayerBonus)
                {
                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            buildBonusHelp(subText, pLoopPair.eBonus, pGame, pPlayer, pActivePlayer, bName: false, bShowCity: false, startLineVariable: TEXTVAR(""));
                        }

                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_MISSION_PLAYER_BONUS", TEXTVAR_TYPE(infos().missionResult(pLoopPair.eMissionResult).meName), subText.ToTextVariable());
                    }
                }

                foreach ((MissionResultType eMissionResult, BonusType eBonus) pLoopPair in pInfoEffectPlayer.mlpMissionTargetBonus)
                {
                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            buildBonusHelp(subText, pLoopPair.eBonus, pGame, pPlayer, pActivePlayer, bName: false, bShowCity: false, startLineVariable: TEXTVAR(""));
                        }

                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_MISSION_TARGET_BONUS", TEXTVAR_TYPE(infos().missionResult(pLoopPair.eMissionResult).meName), subText.ToTextVariable());
                    }
                }

                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                {
                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                    {
                        for (LawType eLoopLaw = 0; eLoopLaw < infos().lawsNum(); eLoopLaw++)
                        {
                            if (infos().law(eLoopLaw).maeEffectPlayerDisabled.Contains(eEffectPlayer))
                            {
                                subText.Add(buildLawLinkVariable(eLoopLaw, pPlayer));
                            }
                        }
                    }

                    if (subText.HasContent)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_DISABLED_LAWS", subText.ToTextVariable());
                    }
                }

                {
                    TextType eDescription = pInfoEffectPlayer.meDescription;

                    if (eDescription != TextType.NONE)
                    {
                        builder.AddTEXT(eDescription);
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ###  EffectPlayer combinations             ###
  ##############################################*/
                if (pInfoEffectPlayer.bAnyEffectPlayerEffectPlayer && !effectPlayerIgnore.Contains(eEffectPlayer))
                {
                    effectPlayerIgnore.Add(eEffectPlayer);

                    EffectPlayerType eEffectPlayerUnlock = pInfoEffectPlayer.meEffectPlayer;
                    if (eEffectPlayerUnlock != EffectPlayerType.NONE)
                    {
                        effectPlayerIgnore.Add(eEffectPlayerUnlock);
                    }

                    for (EffectPlayerType eLoopEffectPlayer = 0; eLoopEffectPlayer < infos().effectPlayersNum(); eLoopEffectPlayer++)
                    {
                        if (pInfoEffectPlayer.maeEffectPlayerEffectPlayer[eLoopEffectPlayer] != EffectPlayerType.NONE)
                        {
                            effectPlayerIgnore.Add(eLoopEffectPlayer);

                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    buildEffectPlayerHelp(builder, pInfoEffectPlayer.maeEffectPlayerEffectPlayer[eLoopEffectPlayer], pGame, pPlayer, pActivePlayer, ref effectPlayerIgnore, eStateReligion, bAllCities, bShowTotal, effectCityScopeType);
                                }

                                if (subText.HasContent)
                                {
                                    builder.AddWithParenthesis(TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_CITY_HELP_NO_YIELDS_EFFECT_CITY_EFFECT_CITY", 
                                        buildEffectPlayerLinkVariable(eLoopEffectPlayer, pPlayer), subText.ToTextVariable()));
                                }
                            }

                            effectPlayerIgnore.Remove(eLoopEffectPlayer);
                        }

                    }

                    effectPlayerIgnore.Remove(eEffectPlayer);

                    if (eEffectPlayerUnlock != EffectPlayerType.NONE)
                    {
                        effectPlayerIgnore.Remove(eEffectPlayerUnlock);
                    }

                }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ###  EffectPlayer combinations             ###
  ##############################################*/

                return builder;
            }
        }

    }
}