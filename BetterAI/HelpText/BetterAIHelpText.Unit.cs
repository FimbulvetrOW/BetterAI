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

        //lines 747-1871
        public override TextBuilder buildEffectUnitHelp(TextBuilder builder, EffectUnitType eEffectUnit, Game pGame, bool bSkipIcons = false, bool bRightJustify = false, bool bIncludeIndirect = true, bool bEncyclopedia = false, bool bLine = false)
        {
            //UnityEngine.Debug.Log("HelpText.buildEffectUnitHelp");

            //ToDo: group maiImprovementToModifier effects by improvementClasses, like specialist improvement prereqs

            builder = base.buildEffectUnitHelp(builder, eEffectUnit, pGame, bSkipIcons, bRightJustify);

/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
            BetterAIInfoEffectUnit pEffectUnitInfo = (BetterAIInfoEffectUnit)infos().effectUnit(eEffectUnit);
            if (pEffectUnitInfo.mbAmphibiousEmbark)
            {
                if (((BetterAIInfoGlobals)infos().Globals).BAI_AMPHIBIOUS_RIVER_CROSSING_DISCOUNT > 0 && ((BetterAIInfoGlobals)infos().Globals).RIVER_CROSSING_COST_EXTRA > 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_UNIT_AMPHIBIOUS_RIVER_CROSSING");
                }
                if (((BetterAIInfoGlobals)infos().Globals).BAI_AMPHIBIOUS_ZOC_CROSSES_RIVER == 1)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_UNIT_AMPHIBIOUS_ZOC_CROSSES_RIVER");
                }
                if (((BetterAIInfoGlobals)infos().Globals).BAI_EMBARKING_COST_EXTRA > 0 && ((BetterAIInfoGlobals)infos().Globals).BAI_HARBOR_OR_AMPHIBIOUS_EMBARKING_DISCOUNT > 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_UNIT_AMPHIBIOUS_EMBARKING");
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Enlist Replacement Attack Heal   START ###
  ##############################################*/
            {
                int iValue = pEffectUnitInfo.miHealAttack;
                if (iValue > 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_UNIT_HELP_HEAL_ATTACK", buildSignedTextVariable(iValue), TEXTVAR(bRightJustify));
                }
                else if (iValue < 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_UNIT_HELP_HURT_ATTACK", buildSignedTextVariable(iValue), TEXTVAR(bRightJustify));
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Enlist Replacement Attack Heal     END ###
  ##############################################*/

            return builder;
        }

        //lines 1906-2385
        public override TextBuilder buildUnitTypeHelp(TextBuilder builder, UnitType eUnit, City pCity, Player pPlayer, TribeType eTribe, Game pGame, Player pActivePlayer, bool bName = true, bool bCosts = true, bool bStats = true, bool bDetails = true)
        {
            //UnityEngine.Debug.Log("HelpText.buildUnitTypeHelp");

            //using (new UnityProfileScope("HelpText.buildUnitTypeHelp"))
            {
                if (bName)
                {
                    builder.Add(buildTitleVariable(buildUnitNameVariable(eUnit, pGame)));

                    buildUnitTraitsHelp(builder, eUnit);
                }

                using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                {
                    if (bStats)
                    {
                        InfoUnit unit = infos().unit(eUnit);
                        int iRangeMin = unit.miRangeMin;
                        int iRangeMax = unit.miRangeMax;

                        using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                if (infos().unit(eUnit).miHPMax > 0)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_HP_MAX", TEXTVAR(unit.miHPMax));
                                }

                                if (infos().Helpers.canDamage(eUnit))
                                {
                                    builder.Add(buildUnitStrengthValueLinkVariable(unit.miStrength, false));
                                }
                                else
                                {
                                    builder.Add(buildDefenseValueLinkVariable(unit.miStrength, false));
                                }

                                builder.Add(buildMovementLinkVariable(unit.miMovement, bSigned: false));

                                if (!(infos().unit(eUnit).mbMelee))
                                {
                                    if (iRangeMax > 0)
                                    {
                                        if (iRangeMin == 0)
                                        {
                                            if (infos().unit(eUnit).mbRangeFlat)
                                            {
                                                builder.Add(buildRangeFixedLinkVariable(iRangeMax, false));
                                            }
                                            else
                                            {
                                                builder.Add(buildRangeLinkVariable(iRangeMax, false));
                                            }
                                        }
                                        else if (iRangeMin > 0)
                                        {
                                            builder.Add(buildRangeMinMaxLinkVariable(iRangeMin, iRangeMax));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (infos().unit(eUnit).mbRangeFlat)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_RANGE_FIXED");
                        }
                    }

                    {
                        int iValue = infos().unit(eUnit).miReveal;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_REVEAL_RANGE", buildSignedTextVariable(iValue));
                        }
                    }

                    {
                        int iValue = (infos().unit(eUnit).miFatigue - infos().Globals.UNIT_FATIGUE_LIMIT);
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_FATIGUE_LIMIT", buildSignedTextVariable(iValue));
                        }
                    }

                    {
                        ReligionType eReligion = infos().Helpers.getSpreadReligion(eUnit);
                        if (eReligion != ReligionType.NONE)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_SPREADS_RELIGION", buildReligionLinkVariable(eReligion, pGame, null));
                        }
                    }

                    if (infos().unit(eUnit).mbAmphibious)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_AMPHIBIOUS");
                    }

                    if (infos().unit(eUnit).mbCaravan)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_CARAVAN_MISSION");
                    }

                    if (infos().unit(eUnit).mbUnlimber)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_MUST_UNLIMBER");
                    }

                    if (infos().unit(eUnit).mbFound)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_FOUND_CITY");
                    }

                    if (infos().unit(eUnit).mbBuild)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_BUILD_IMPROVEMENTS");
                    }

                    {
                        ReligionType eBuildReligion = infos().unit(eUnit).meBuildReligion;

                        if (eBuildReligion != ReligionType.NONE)
                        {
                            using (var improvementsScope = CollectionCache.GetListScoped<TextVariable>())
                            {
                                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                {
                                    if (infos().improvement(eLoopImprovement).meReligionPrereq == eBuildReligion)
                                    {
                                        improvementsScope.Value.Add(buildImprovementLinkVariable(eLoopImprovement, pGame, pCity?.tile()));
                                    }
                                }

                                if (improvementsScope.Value.Count > 0)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_BUILD_RELIGION_IMPROVEMENT");

                                    using (builder.BeginScope(TextBuilder.ScopeType.INDENTED_BULLET))
                                    {
                                        foreach (TextVariable loopText in improvementsScope.Value)
                                        {
                                            builder.Add(loopText);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (bDetails)
                    {
                        if (infos().unit(eUnit).mbZOC)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_HAS_ZOC", buildZOCLinkVariable());
                        }

/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
                        if (infos().unit(eUnit).mbIgnoreZOC)
                        {
                            if (!((BetterAIInfoUnit)(infos().unit(eUnit))).bHasIngoreZOCBlocker)
                            {
                                builder.Add(buildIgnoreZOCLinkVariable());
                            }
                            else
                            {
                                builder.Add(buildIgnoreZOCWithExceptionsLinkVariable(eUnit));
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/
                    }

                    if (infos().unit(eUnit).mbFortify)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_FORTIFY");
                    }

                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_STARTS_WITH")))
                        {
                            foreach (EffectUnitType eLoopEffectUnit in infos().unit(eUnit).maeEffectUnit)
                            {
                                builder.Add(buildEffectUnitLinkVariable(eLoopEffectUnit));
                            }

                            if (pCity != null)
                            {
                                if (infos().Helpers.isPromotable(eUnit))
                                {
                                    {
                                        int iValue = pCity.getBuildUnitLevels(eUnit);
                                        if (iValue > 0)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_START_LEVELS", buildSignedTextVariable(iValue));
                                        }
                                    }

                                    {
                                        int iValue = pCity.getBuildUnitXP(eUnit);
                                        if (iValue > 0)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_START_XP", buildSignedTextVariable(iValue));
                                        }
                                    }

                                }

                                using (var hashSetScope = CollectionCache.GetHashSetScoped<EffectUnitType>())
                                {
                                    HashSet<EffectUnitType> seEffectUnits = hashSetScope.Value;

                                    foreach (EffectCityType eLoopEffectCity in pCity.getActiveEffectCity())
                                    {
                                        foreach (EffectUnitType eLoopEffectUnit in infos().effectCity(eLoopEffectCity).maeFreeEffectUnit)
                                        {
                                            if (pGame.isEffectUnitValid(eUnit, eLoopEffectUnit))
                                            {
                                                seEffectUnits.Add(eLoopEffectUnit);
                                            }
                                        }

                                        foreach (UnitTraitType eLoopUnitTrait in infos().unit(eUnit).maeUnitTrait)
                                        {
                                            EffectUnitType eEffectUnit = infos().effectCity(eLoopEffectCity).maeTraitEffectUnit[eLoopUnitTrait];

                                            if (eEffectUnit != EffectUnitType.NONE)
                                            {
                                                seEffectUnits.Add(eEffectUnit);
                                            }
                                        }
                                    }

                                    foreach (EffectUnitType eLoopEffectUnit in seEffectUnits)
                                    {
                                        builder.Add(buildEffectUnitLinkVariable(eLoopEffectUnit));
                                    }
                                }
                                if (infos().Helpers.isPromotable(eUnit))
                                {
                                    int iRandomPromotions = 0;
                                    foreach (EffectCityType eLoopEffectCity in pCity.getActiveEffectCity())
                                    {
                                        iRandomPromotions += infos().effectCity(eLoopEffectCity).miRandomPromotions;
                                    }
                                    if (iRandomPromotions != 0)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_RANDOM_PROMOTIONS", buildSignedTextVariable(iRandomPromotions));
                                    }
                                }
                                foreach (EffectCityType eLoopEffectCity in pCity.getActiveEffectCity())
                                {
                                    if (infos().effectCity(eLoopEffectCity).meNewUnitReligion != ReligionType.NONE && infos().unit(eUnit).meBuildReligion == ReligionType.NONE)
                                    {
                                        builder.Add(buildReligionLinkVariable(infos().effectCity(eLoopEffectCity).meNewUnitReligion, pGame, pActivePlayer));
                                    }
                                }
                            }
                        }
                    }

                    if (bDetails)
                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_CONSUMES")))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iValue = (infos().unit(eUnit).maiYieldConsumption[eLoopYield] * Constants.YIELDS_MULTIPLIER);
                                if (iValue != 0)
                                {
                                    builder.Add(buildYieldValueIconLinkVariable(eLoopYield, -(iValue), bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_IMPROVEMENT_YIELD", buildImprovementClassLinkVariable(eLoopImprovementClass), pGame?.turnScaleName() ?? TEXTVAR("y")))) //TODO: needs to be changed when mzScaleShort localization is added
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iValue = infos().unit(eUnit).maaiImprovementClassYieldRate[eLoopImprovementClass, eLoopYield];
                                    if (iValue != 0)
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }

                        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_IMPROVEMENT_YIELD", buildImprovementLinkVariable(eLoopImprovement, pGame), pGame?.turnScaleName() ?? TEXTVAR("y")))) //TODO: needs to be changed when mzScaleShort localization is added
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iValue = infos().unit(eUnit).maaiImprovementYieldRate[eLoopImprovement, eLoopYield];
                                    if (iValue != 0)
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }
                    }

                    if (infos().Helpers.canDamage(eUnit) && !infos().Helpers.isPromotable(eUnit))
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_NO_TRAIN_OR_PROMOTE");
                    }
                } // end BULLET

                if (bCosts)
                {
                    // population
                    {
                        int iCost = infos().unit(eUnit).miPopulationCost;
                        if (iCost > 0)
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                            using (buildWarningTextScope(builder, ((pCity != null) ? (pCity.getCitizens() < iCost) : false)))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_POPULATION_COST", TEXTVAR(iCost), buildCitizenLinkVariable());
                            }
                        }
                    }

                    // yields
                    {
                        using (var costsScoped = CollectionCache.GetDictionaryScoped<YieldType, int>())
                        {
                            if (pPlayer != null)
                            {
                                pPlayer.getUnitYieldCost(eUnit, pCity, costsScoped.Value);
                            }
                            else
                            {
                                infos().Helpers.getUnitYieldCost(eUnit, costsScoped.Value);
                            }
                            buildYieldCostText(builder, costsScoped.Value, ((pPlayer == pActivePlayer) ? pPlayer : null));
                        }

                        TextVariable production = null;
                        if (pPlayer != null && pCity != null)
                        {
                            int iProduction = pPlayer.getUnitBuildCost(eUnit, pCity);

                            if (iProduction > 0)
                            {
                                int iProgress = pCity.getYieldOverflow(infos().unit(eUnit).meProductionType);
                                CityQueueData build = pCity.getCurrentBuild();
                                if (build != null && pCity.getBuildYieldType(build) == infos().unit(eUnit).meProductionType)
                                {
                                    iProgress = 0;
                                }
                                production = TEXTVAR_TYPE("TEXT_HELPTEXT_BUILD_PRODUCTION", TEXTVAR(iProduction), buildTurnsLeftTextVariable(pGame, iProduction, iProgress, pCity.getBuildRate(infos().Globals.UNIT_BUILD, (int)eUnit), true), buildYieldIconNameLinkVariable(infos().unit(eUnit).meProductionType));
                            }
                        }
                        else
                        {
                            int iValue = infos().unit(eUnit).miProduction;
                            if (iValue > 0)
                            {
                                production = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_PRODUCTION", TEXTVAR(iValue), buildYieldIconLinkVariable(infos().unit(eUnit).meProductionType));
                            }
                        }
                        if (production != null)
                        {
                            //fixed in base game :)
                            int iModifier = pPlayer?.getUnitBuildCostModifier(eUnit, pCity) ?? 0;
                            TextVariable output = production;
                            if (infos().unit(eUnit).miProductionCity != 0)
                            {
                                TextVariable perCity = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_PRODUCTION_PER_CITY", TEXTVAR(infos().utils().modify(infos().unit(eUnit).miProductionCity, iModifier), NumberFormatOptions.SHOW_SIGN), buildYieldIconLinkVariable(infos().unit(eUnit).meProductionType));
                                output = buildCommaSpaceSeparator(output, perCity);
                            }
                            if (infos().unit(eUnit).miProductionPer != 0)
                            {
                                TextVariable perGlobal = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_PRODUCTION_PER_GLOBAL", TEXTVAR(infos().utils().modify(infos().unit(eUnit).miProductionPer, iModifier), NumberFormatOptions.SHOW_SIGN), buildYieldIconLinkVariable(infos().unit(eUnit).meProductionType));
                                output = buildCommaSpaceSeparator(output, perGlobal);
                            }
                            builder.Add(output);
                        }
                    }
                }

                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_UPGRADES_FROM")))
                    {
                        for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                        {
                            if (pPlayer == null || pPlayer.canEverBuildUnit(eLoopUnit))
                            {
                                if (infos().unit(eLoopUnit).maeUpgradeUnit.Contains(eUnit))
                                {
                                    if (isContentEnabled(infos().unit(eLoopUnit).meGameContentRequired, pGame))
                                    {
                                        builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                                    }
                                }
                            }
                            if (eTribe != TribeType.NONE)
                            {
                                if (infos().unit(eLoopUnit).maeTribeUpgradeUnit[eTribe] == eUnit)
                                {
                                    builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                                }
                            }
                            else
                            {
                                //old
                                //if (infos().unit(eLoopUnit).maeTribeUpgradeUnit.Contains(eUnit))
                                //{
                                //    builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                                //}

                                //new
                                //foreach (var p in infos().unit(eLoopUnit).maeTribeUpgradeUnit)
                                //{
                                //    if (p.Value == eUnit)
                                //    {
                                //        builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                                //        break;
                                //    }
                                //}

                                //using derivative info instead
                            }
                        }
                        if (eTribe == TribeType.NONE)
                        {
                            foreach (UnitType eLoopUnit in ((BetterAIInfoUnit)infos().unit(eUnit)).maeTribeUpgradesFromAccumulated)
                            {
                                builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                            }
                        }


                    }
                }

                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_UPGRADES_TO")))
                    using (var unitTypesScoped = CollectionCache.GetHashSetScoped<UnitType>())
                    {
                        HashSet<UnitType> seUpgrades = unitTypesScoped.Value;

                        foreach (UnitType eLoopUnit in infos().unit(eUnit).maeUpgradeUnit)
                        {
                            if (pPlayer == null || pPlayer.canEverBuildUnit(eLoopUnit))
                            {
                                seUpgrades.Add(eLoopUnit);
                            }
                        }
                        if (eTribe != TribeType.NONE)
                        {
                            UnitType eUpgrade = infos().unit(eUnit).maeTribeUpgradeUnit[eTribe];
                            if (eUpgrade != UnitType.NONE)
                            {
                                seUpgrades.Add(eUpgrade);
                            }
                        }
                        else
                        {
                            //foreach (UnitType eLoopUnit in infos().unit(eUnit).maeTribeUpgradeUnit)
                            //{
                            //    if (eLoopUnit != UnitType.NONE)
                            //    {
                            //        seUpgrades.Add(eLoopUnit);
                            //    }
                            //}
                            
                            foreach (KeyValuePair<TribeType, UnitType> TribeUpgradeUnit in infos().unit(eUnit).maeTribeUpgradeUnit)
                            {
                                if (TribeUpgradeUnit.Value != UnitType.NONE)
                                {
                                    seUpgrades.Add(TribeUpgradeUnit.Value);
                                }
                            }

                        }
                        foreach (UnitType eLoopUnit in seUpgrades)
                        {
                            builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                        }
                    }
                }


                if (infos().unit(eUnit).mltModVariables.Count > 0)
                {
                    foreach ((string First, string Second, TextType Third) modVariable in infos().unit(eUnit).mltModVariables)
                    {
                        if (modVariable.Third != TextType.NONE)
                        {
                            builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", TEXTVAR_TYPE(modVariable.Third), TEXTVAR(modVariable.Second)));
                        }
                    }
                }

                {
                    TextType eDescription = infos().unit(eUnit).meDescription;

                    if (eDescription != TextType.NONE)
                    {
                        builder.AddTEXT(eDescription);
                    }
                }

                return builder;
            }
        }

        //lines 2595-2948
        public override void buildUnitTooltip(Unit pUnit, ClientManager pManager, UIUnitTooltipData outUnitData)
        {
            //UnityEngine.Debug.Log("HelpText.buildUnitTooltip");

            //using (new UnityProfileScope("HelpText.buildUnitTooltip"))
            {
                Player pActivePlayer = pManager.activePlayer();
                Game pGame = pManager.GameClient;

                // Important to note that outUnitData is cleared before passed into this method!

                if (pUnit == null)
                {
                    TextManager.TEXT(outUnitData.Title, buildTitleVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_DEAD")));
                    return;
                }

                bool bExpand = pManager.Interfaces.Hotkeys.IsHotkeyPressed(mInfos.Globals.HOTKEY_EXPAND_TOOLTIP);

                outUnitData.UnitID = pUnit.getID();

                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager, outUnitData.Title))
                {
                    builder.Add(buildUnitTypeLinkVariable(pUnit.getType(), pGame, pUnit));
                }

                for (TeamType eLoopTeam = 0; eLoopTeam < pGame.getNumTeams(); eLoopTeam++)
                {
                    if (pUnit.isRaidTeam(eLoopTeam))
                    {
                        outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_RAIDING"), buildTeamLinkVariable(eLoopTeam, pGame, pActivePlayer));
                    }
                }

                if (pUnit.hasOriginalTribe() && pUnit.getOriginalTribe() != pUnit.getTribe())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_ORIGINAL_TRIBE"), buildTribeLinkVariable(pUnit.getOriginalTribe(), pGame));
                }

                if (pUnit.hasOriginalPlayer() && pUnit.getOriginalPlayer() != pUnit.getPlayer())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_ORIGINAL_TRIBE"), buildPlayerLinkVariable(pGame.player(pUnit.getOriginalPlayer()), pActivePlayer));
                }

                if (pUnit.hasRebelPlayer())
                {
                    if (pUnit.rebelPlayer().isAlive())
                    {
                        outUnitData.AddStat(TextManager, buildTribeLinkVariable(infos().Globals.REBELS_TRIBE, pGame), buildPlayerLinkVariable(pUnit.rebelPlayer(), pActivePlayer));
                    }
                }

                if (pUnit.hasCaravanMissionTarget())
                {
                    City pCity = pUnit.caravanMissionTarget().capitalCity();

                    if (pCity != null)
                    {
                        outUnitData.AddStat(TextManager, buildCaravanMissionLinkVariable(), buildCityLinkVariable(pCity, pActivePlayer));
                    }
                }

                if (pUnit.isDamaged())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_HEALTH_LABEL"), buildSlashText(pUnit.getHP(), pUnit.getHPMax()));
                }

                if (pUnit.getFortifyBonus() > 0)
                {
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_FORTIFIED_LABEL"), buildDefenseValueLinkVariable(pUnit.getFortifyBonus(), true));
                }

                if (pUnit.getFormationBonus() > 0 && pUnit.getCurrentFormation() != EffectUnitType.NONE)
                {
                    UnitTraitType eUnitTrait = infos().effectUnit(pUnit.getCurrentFormation()).mzUnitTraitFormation.Item1;
                    if (eUnitTrait != UnitTraitType.NONE)
                    {
                        outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(pUnit.getCurrentFormation(), pUnit, false), TEXTVAR("TEXT_HELPTEXT_EFFECT_UNIT_HELP_VS_UNIT_TRAIT", TextManager, buildUnitTraitLinkVariable(eUnitTrait), buildDefenseValueLinkVariable(pUnit.getFormationBonus(), true), TEXTVAR(true)));
                    }
                }

                if (pUnit.isAnchored())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR(""), buildAnchoredLinkVariable());
                }

                if (pUnit.isUnlimbered())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR(""), buildUnlimberedLinkVariable());
                }

                if (pUnit.isTribe() && pUnit.AI.hasCentralAI())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR(""), buildCentralAILinkVariable());
                }

                if (bExpand)
                {
                    using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            if (pUnit.hasZOC(UnitType.NONE))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_HAS_ZOC", buildZOCLinkVariable());
                            }

                            if (pUnit.hasIgnoreZOC())
                            {
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
                                if (!((BetterAIInfoUnit)pUnit.info()).bHasIngoreZOCBlocker)
                                {
                                    builder.Add(buildIgnoreZOCLinkVariable());
                                }
                                else
                                {
                                    builder.Add(buildIgnoreZOCWithExceptionsLinkVariable(pUnit.getType()));
                                }
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/
                            }
                        }

                        if (builder.HasContent)
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR(""), builder.ToTextVariable());
                        }
                    }
                }


                using var effectUnitsScoped = CollectionCache.GetListScoped<(TextVariable, TextVariable)>();
                List<(TextVariable, TextVariable)> effectUnits = effectUnitsScoped.Value;
                for (EffectUnitClassType eLoopEffectUnitClass = 0; eLoopEffectUnitClass < infos().effectUnitClassesNum(); eLoopEffectUnitClass++)
                {
                    if (pUnit.hasEffectUnitClass(eLoopEffectUnitClass))
                    {
                        using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                for (EffectUnitType eLoopEffectUnit = 0; eLoopEffectUnit < infos().effectUnitsNum(); eLoopEffectUnit++)
                                {
                                    if (infos().effectUnit(eLoopEffectUnit).meClass == eLoopEffectUnitClass)
                                    {
                                        if (pUnit.hasEffectUnit(eLoopEffectUnit))
                                        {
                                            buildEffectUnitHelp(builder, eLoopEffectUnit, pGame, bRightJustify: true, bIncludeIndirect: false);
                                        }
                                    }
                                }
                            }

                            effectUnits.Add((buildEffectUnitClassLinkVariable(eLoopEffectUnitClass, pUnit), builder.ToTextVariable()));
                        }
                    }
                }


                if (pUnit.hasEffectUnitAny())
                {
                    foreach (EffectUnitType eLoopEffect in pUnit.getEffectUnits())
                    {
                        if (infos().effectUnit(eLoopEffect).meClass == EffectUnitClassType.NONE)
                        {
                            if (!(pUnit.isGeneralEffectUnit(eLoopEffect)) && !(pUnit.isExplorerEffectUnit(eLoopEffect)))
                            {
                                if (!pUnit.isEffectUnitSource(eLoopEffect, SourceEffectUnitType.UNIT))
                                {
                                    using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                        }

                                        effectUnits.Add((buildEffectUnitLinkVariable(eLoopEffect, pUnit, bIcon: false), builder.ToTextVariable()));
                                    }
                                }
                                else if (bExpand)
                                {
                                    using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                        effectUnits.Add((buildEffectUnitLinkVariable(eLoopEffect, pUnit, bIcon: false), builder.ToTextVariable()));
                                    }
                                }
                            }
                        }
                    }
                }

                if (effectUnits.Count > 0)
                {
                    int CompareEffectUnits((TextVariable, TextVariable) first, (TextVariable, TextVariable) second)
                    {
                        using var nameFirst = CollectionCache.GetStringBuilderScoped();
                        using var nameSecond = CollectionCache.GetStringBuilderScoped();

                        TextManager.TEXT(nameFirst.Value, first.Item1);
                        TextManager.TEXT(nameSecond.Value, second.Item1);

                        for (int i = 0; i < nameFirst.Value.Length; i++)
                        {
                            if (i >= nameSecond.Value.Length)
                                return 1;

                            if (nameFirst.Value[i] != nameSecond.Value[i])
                                return nameFirst.Value[i] - nameSecond.Value[i];
                        }

                        return nameFirst.Value.Length - nameSecond.Value.Length;
                    }

                    effectUnits.Sort(CompareEffectUnits);
                    foreach (var pair in effectUnits)
                    {
                        outUnitData.AddStat(TextManager, pair.Item1, pair.Item2);
                    }
                }

                if (pUnit.hasGeneral())
                {
                    Character pGeneral = pUnit.general();

                    outUnitData.AddStat(TextManager, buildCharacterLinkVariable(pGeneral, pActivePlayer, false), buildGeneralRatingsAllHelpVariable(pGeneral));

                    foreach (EffectUnitType eLoopEffect in pUnit.getEffectUnits())
                    {
                        if (infos().effectUnit(eLoopEffect).meClass == EffectUnitClassType.NONE)
                        {
                            if (pUnit.isGeneralEffectUnit(eLoopEffect))
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                        buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                    outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(eLoopEffect, pUnit, bIcon: false), builder.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                if (pUnit.hasExplorer())
                {
                    Character pExplorer = pUnit.explorer();

                    outUnitData.AddStat(TextManager, buildCharacterLinkVariable(pExplorer, pActivePlayer, false), buildExplorerRatingsAllHelpVariable(pExplorer));

                    foreach (EffectUnitType eLoopEffect in pUnit.getEffectUnits())
                    {
                        if (infos().effectUnit(eLoopEffect).meClass == EffectUnitClassType.NONE)
                        {
                            if (pUnit.isExplorerEffectUnit(eLoopEffect))
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                        buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                    outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(eLoopEffect, pUnit, bIcon: false), builder.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                if (pUnit.isPromotable())
                {
                    TextVariable levelXPVariable;
                    if (pUnit.isLevelPromotionMax())
                    {
                        levelXPVariable = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_LEVEL_MAX", TEXTVAR(pUnit.getLevelPromotionString()));
                    }
                    else
                    {
                        levelXPVariable = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_LEVEL", TEXTVAR(pUnit.getLevelPromotionString()), TEXTVAR(pUnit.getXP()), TEXTVAR(pUnit.getXPThreshold()), TEXTVAR(pUnit.getLevelPromotion() < pUnit.getLevel()));
                    }
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_LEVEL_LABEL"), levelXPVariable);
                }

                if (bExpand)
                {
                    if (pUnit.hasPlayer())
                    {
                        if (pUnit.hasFamily())
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_FAMILY_LABEL"), buildFamilyLinkVariable(pUnit.getFamily(), pGame));
                        }
                        else
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR(""), buildMercenaryLinkVariable());
                        }

                        if (pUnit.hasOriginalTribe())
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_TRIBE_LABEL"), buildTribeLinkVariable(pUnit.getOriginalTribe(), pGame));
                        }
                    }

                    using (TextBuilder traitsBuilder = TextBuilder.GetTextBuilder(TextManager))
                    {
                        buildUnitTraitsHelp(traitsBuilder, pUnit.getType());
                        if (traitsBuilder.HasContent)
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_TRAITS_LABEL"), traitsBuilder.ToTextVariable());
                        }
                    }

                    for (AttackType eLoopAttack = 0; eLoopAttack < infos().attacksNum(); eLoopAttack++)
                    {
                        int iValue = pUnit.getAttackValue(eLoopAttack);
                        if (iValue > 0)
                        {
                            TextVariable valueOrFalse = iValue > 1 ? TEXTVAR(iValue) : TEXTVAR(false);

                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_ATTACK_LABEL"), TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_ATTACK_VALUE_ITEM", buildAttackLinkVariable(eLoopAttack), valueOrFalse, TEXTVAR(pUnit.attackPercent(eLoopAttack))));
                        }
                    }

                    if (pUnit.hasMoved())
                    {
                        int iFatigueSteps = pUnit.getFatigueLimit() - pUnit.getStepsToFatigue();
                        outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_FATIGUE_LABEL"), buildSlashText(iFatigueSteps, pUnit.getFatigueLimit()));
                    }

                    if (pUnit.hasCooldown())
                    {
                        outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_COOLDOWN", TEXTVAR_TYPE(pUnit.cooldown().mName)), buildTurnsTextVariable(pUnit.getCooldownTurns(), pGame));
                    }

                    using (TextBuilder yieldListBuilder = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (yieldListBuilder.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iValue = pUnit.yieldConsumption(eLoopYield);
                                if (iValue != 0)
                                {
                                    yieldListBuilder.Add(buildUnitConsumesLinkVariable(buildYieldIconTextVariable(eLoopYield, -(iValue), bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER), pUnit));
                                }
                            }
                        }

                        if (yieldListBuilder.HasContent)
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_CONSUMPTION"), yieldListBuilder.ToTextVariable());
                        }
                    }


                    if (pUnit.isModVariables())
                    {
                        foreach ((string First, string Second, TextType Third) mvType in infos().unit(pUnit.getType()).mltModVariables)
                        {
                            if (mvType.Third != TextType.NONE)
                            {
                                string sVariable = pUnit.getModVariable(mvType.First);
                                if (!string.IsNullOrWhiteSpace(sVariable))
                                {
                                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE(mvType.Third), TEXTVAR(sVariable));
                                }
                            }
                        }
                    }
                }
            }
        }


    }
}