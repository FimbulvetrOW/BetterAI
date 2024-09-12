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
using TenCrowns.ClientCore;
using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using static BetterAI.BetterAIHelpText.CommaListVariableGenerator;

namespace BetterAI
{
    public class BetterAIHelpText : HelpText
    {
        //lines 44-48
        public BetterAIHelpText(TextManager txtMgr) : base(txtMgr)
        {
        }

/*####### Better Old World AI - Base DLL #######
  ### Rel. Improvements in City list   START ###
  ##############################################*/
        public virtual TextVariable buildCityLinkVariableWithReligiousImprovements(Game pGame, City pCity, Player pActivePlayer, ReligionType eReligion, List<ImprovementType> ImprovementTypes, Dictionary<ImprovementType, string> ImprovementShortNames, bool bCrest = true, bool bAddColor = true, bool bSelect = true)
        {
            if (pCity == null)
            {
                return TEXTVAR(false);
            }

            TextVariable returnText = base.buildCityLinkVariable(pCity, pActivePlayer, bCrest, bAddColor, bSelect);

            if (eReligion != ReligionType.NONE && ImprovementTypes != null)
            {
                if (ImprovementTypes.Count > 0)
                {
                    CommaListVariableGenerator religiousImprovements = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, EncloseType.PARENTHESIS, TextManager);
                    foreach (ImprovementType eLoopImprovement in ImprovementTypes)
                    {
                        if (pCity.getImprovementCount(eLoopImprovement) > 0)
                        {
                            string name = ImprovementShortNames[eLoopImprovement];

                            //counts
                            if (pCity.getFinishedImprovementCount(eLoopImprovement) > 0)
                            {
                                if (pCity.getFinishedImprovementCount(eLoopImprovement) > 1)
                                {
                                    name = pCity.getFinishedImprovementCount(eLoopImprovement).ToStringCached() + name;
                                }
                                if (pCity.getImprovementCount(eLoopImprovement) > pCity.getFinishedImprovementCount(eLoopImprovement))
                                {
                                    int iUnfinishedImprovements = pCity.getImprovementCount(eLoopImprovement) - pCity.getFinishedImprovementCount(eLoopImprovement);
                                    name = name + "(" + iUnfinishedImprovements.ToStringCached() + ")";
                                }
                            }
                            else
                            {
                                if (pCity.getImprovementCount(eLoopImprovement) > 1)
                                {
                                    name = pCity.getImprovementCount(eLoopImprovement).ToStringCached() + name;
                                }
                                name = "(" + name + ")";
                            }

                            religiousImprovements.AddItem(buildLinkTextVariable(TEXTVAR(name), ItemType.HELP_LINK, nameof(LinkType.HELP_IMPROVEMENT), infos().improvement(eLoopImprovement).SafeTypeString(), "-1"));

                        }
                    }

                    if (religiousImprovements.Count > 0)
                    {
                        returnText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", returnText, religiousImprovements.Finalize());
                    }
                }            
            }

            return returnText;
        }
/*####### Better Old World AI - Base DLL #######
  ### Rel. Improvements in City list     END ###
  ##############################################*/

        //lines 2998-3012
        public override TextVariable buildHappinessLevelLinkVariable(City pCity, bool bShort = false)
        {
            //using (new UnityProfileScope("HelpText.buildHappinessLevelLinkVariable"))
            {
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
                if (bShort)
                {
                    //return buildLinkTextVariable(concatenateSpace(TEXTVAR_TYPE(infos().yield(pCity.getHappinessLevel() >= 0 ? infos().Globals.HAPPINESS_YIELD : infos().Globals.DISCONTENT_YIELD).meName), 
                    //    TEXTVAR(Math.Abs(pCity.getHappinessLevel()))), ItemType.HELP_LINK, nameof(LinkType.HELP_HAPPINESS_LEVEL), pCity.getID().ToStringCached());
                    return buildLinkTextVariable(concatenateSpace(TEXTVAR_TYPE(infos().yield(!(((BetterAICity)pCity).isDiscontent()) ? infos().Globals.HAPPINESS_YIELD : infos().Globals.DISCONTENT_YIELD).meName),
                        TEXTVAR(Math.Abs(pCity.getHappinessLevel()))), ItemType.HELP_LINK, nameof(LinkType.HELP_HAPPINESS_LEVEL), pCity.getID().ToStringCached());
                }
                else
                {
                    //return buildLinkTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_HAPPINESS", TEXTVAR(Math.Abs(pCity.getHappinessLevel())), TEXTVAR(pCity.getHappinessLevel() >= 0)), ItemType.HELP_LINK, nameof(LinkType.HELP_HAPPINESS_LEVEL), pCity.getID().ToStringCached());
                    return buildLinkTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_HAPPINESS", TEXTVAR(Math.Abs(pCity.getHappinessLevel())), TEXTVAR(!(((BetterAICity)pCity).isDiscontent()))), ItemType.HELP_LINK, nameof(LinkType.HELP_HAPPINESS_LEVEL), pCity.getID().ToStringCached());
                }
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                  END ###
  ##############################################*/
            }
        }

/*####### Better Old World AI - Base DLL #######
  ### ZOC ignore exceptions            START ###
  ##############################################*/
        //lines 3044-3047
        //public virtual TextVariable buildIgnoreZOCLinkVariable()
        //{
        //    return buildConceptLinkVariable("CONCEPT_IGNORES_ZOC");
        //}
        public virtual TextVariable buildIgnoreZOCWithExceptionsLinkVariable(UnitType eUnit)
        {
            CommaListVariableGenerator blockerList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);
            BetterAIInfoUnit pUnitInfo = (BetterAIInfoUnit)infos().unit(eUnit);
            foreach (EffectUnitType ZOCBlockerEffect in pUnitInfo.maeBlockZOCEffectUnits)
            {
                blockerList.AddItem(buildEffectUnitLinkVariable(ZOCBlockerEffect));
            }
            return TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", buildIgnoreZOCLinkVariable(), TEXTVAR_TYPE("TEXT_HELPTEXT_ZOC_IGNORE_BLOCKER_EFFECT_UNITS", blockerList.Finalize()));
        }
/*####### Better Old World AI - Base DLL #######
  ### ZOC ignore exceptions              END ###
  ##############################################*/

        //lines 8684-13513
        public override TextBuilder buildWidgetHelp(TextBuilder builder, WidgetData pWidget, ClientManager pManager, bool bIncludeEncyclopediaFooter = true)
        {
            if (pWidget.GetWidgetType() == ItemType.CREATE_AGENT_NETWORK)
            {
                Player pActivePlayer = pManager.activePlayer();
                Game pGame = pManager.GameClient;

                Unit pUnit = pGame.unit(pWidget.GetDataInt(0));
                if (pUnit != null)
                {
                    //Player pPlayer = pUnit.player();
                    City pCity = pGame.city(pWidget.GetDataInt(1));
/*####### Better Old World AI - Base DLL #######
  ### Agent Network Cost Scaling       START ###
  ##############################################*/
                    builder.AddTEXT("TEXT_HELPTEXT_WIDGET_CREATE_AGENT_NETWORK", buildAgentNetworkLinkVariable(), buildMoneyWarningTextVariable(((BetterAIUnit)pUnit).getAgentNetworkCost(pCity), pActivePlayer), buildCityLinkVariable(pCity, pActivePlayer));
/*####### Better Old World AI - Base DLL #######
  ### Agent Network Cost Scaling         END ###
  ##############################################*/
                    buildAgentReturnText(builder, pUnit, pCity, pGame, pActivePlayer);

                    buildAgentUnlockText(builder, pUnit.player(), pGame, pActivePlayer);

                    buildCanActText(builder, 0, pGame, pActivePlayer, pUnit);
                }

                if (bIncludeEncyclopediaFooter && builder.HasContent && !pManager.App.UserInterface.IsPopupActive("POPUP_HELP") &&
                    hasEncyclopediaEntry(pWidget, pManager.GameClient) && hasHotkey(pManager.Interfaces.Hotkeys, infos().Globals.HOTKEY_HELP_SCREEN))
                {
                    buildDividerText(builder);
                    builder.AddTEXT("TEXT_HELPTEXT_LINK_OPEN_ENCYCLOPEDIA", buildPressHotkeyVariableOrFalse(pManager.Interfaces.Hotkeys, infos().Globals.HOTKEY_HELP_SCREEN));
                }

                return builder;
            }
            else
            {
                return base.buildWidgetHelp(builder, pWidget, pManager, bIncludeEncyclopediaFooter);
            }
        }


        //lines 19118-19200
        public override TextBuilder buildTileDebugText(TextBuilder builder, Tile pTile, ClientManager pManager)
        {
            //using (new UnityProfileScope("buildTileDebugText"))
            {
                TeamType eActiveTeam = pManager.getActiveTeam();

                using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                {
                    for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                    {
                        if (pTile.getTradeNetwork(eActiveTeam, eLoopDirection) != -1)
                        {
                            builder.Add(buildColonSpaceOne(TEXTVAR(eLoopDirection.ToStringCached()), pTile.getTradeNetwork(eActiveTeam, eLoopDirection)));
                        }
                    }
                }

                using (builder.BeginScope("TEXT_HELPTEXT_CONCAT_SPACE_TWO"))
                {
                    builder.Add(pTile.ToTextVariable());
                    builder.Add(QUICKTEXTVAR("A{0}", pTile.getArea()));
                    builder.Add(QUICKTEXTVAR("S{0}", pTile.getLandSection()));
                    builder.Add(QUICKTEXTVAR("L{0}", pTile.getLatitude()));
                    builder.Add(QUICKTEXTVAR("H{0}", (int)pTile.getHeight()));

                    if (pTile.getTerrainStamp() != TerrainStampType.NONE)
                    {
                        builder.Add(QUICKTEXTVAR("S{0}", (int)pTile.getTerrainStamp()));
                    }

                    if (pTile.isBoundary())
                    {
                        builder.Add(QUICKTEXTVAR("BND"));
                    }
                    builder.Add("*", skipSeparator: true);
                }

                if (pTile.getNumTags() > 0)
                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_DEBUG_TAGS")))
                    {
                        for (int i = 0; i < pTile.getNumTags(); ++i)
                        {
                            builder.Add(QUICKTEXTVAR(infos().tileTag(pTile.getTagAt(i)).mzType));
                        }
                    }
                    builder.Add("*", skipSeparator: true);
                }

                if (pTile.hasMapGeneratorData())
                {
                    builder.Add(QUICKTEXTVAR("GT:{0}", QUICKTEXTVAR(pTile.getMapGeneratorData())));
                }

/*####### Better Old World AI - Base DLL #######
  ### No BestImprovement               START ###
  ### because Nullref                        ###
  ##############################################*/
                ////if (!((BetterAIPlayer.BetterAIPlayerAI)pManager.activePlayer().AI).bHumanCachingDone)
                ////{
                ////    ((BetterAIPlayer.BetterAIPlayerAI)pManager.activePlayer().AI).cacheImprovementValuesHuman();
                ////}
                //
                //ImprovementType eBestImprovement = pManager.activePlayer().AI.getBestImprovementCached(pTile, pTile.cityTerritory());;
                //if (eBestImprovement != ImprovementType.NONE)
                //{
                //    long iAIValue = pManager.activePlayer().AI.improvementValueTile(eBestImprovement, pTile, pTile.cityTerritory(), true, true, true);
                //    builder.Add(QUICKTEXTVAR(TEXT("TEXT_HELPTEXT_AI_VALUE") + ": {0} ({1})*", iAIValue, TEXTVAR_TYPE(mInfos.improvement(eBestImprovement).mName)));
                //}
/*####### Better Old World AI - Base DLL #######
  ### No BestImprovement                 END ###
  ### because Nullref                        ###
  ##############################################*/

                if (pManager.Selection.isSelectedUnit())
                {
                    Unit pUnit = pManager.Selection.getSelectedUnit();
                    if (pUnit != null && pUnit.getPlayer() == pManager.getActivePlayer())
                    {
                        builder.Add(QUICKTEXTVAR(TEXT("TEXT_HELPTEXT_AI_VALUE") + ": {0} ({1})*", pUnit.AI.exploreValue(pTile), TEXTVAR("Explore")));
                    }
                }

                if (pTile.isValidFoundLocation(pManager.getActivePlayer(), TeamType.NONE, false))
                {
                    FamilyType eBestFamily = pManager.activePlayer().AI.getBestFoundFamily(pTile);
                    if (eBestFamily != FamilyType.NONE)
                    {
                        builder.Add(QUICKTEXTVAR(TEXT("TEXT_HELPTEXT_BEST_FAMILY") + ": {0}", TEXTVAR_TYPE(mInfos.family(eBestFamily).meName)));
                    }
                }

                return builder;
            }
        }

        //500 lines of copy-paste START
        //lines 19806-20253
        public override TextBuilder buildImprovementBreakdown(TextBuilder builder, ImprovementType eImprovement, SpecialistType eSpecialist, Tile pTile, ClientManager pManager)
        {
            Game pGame = pManager.GameClient;
            Player pActivePlayer = pManager.activePlayer();

            ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

            ResourceType eResource = pTile.getResource();
            City pCityTerritory = pTile.cityTerritory();

            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
            {
                int iTotalOutput = pTile.yieldOutputModified(eImprovement, eSpecialist, eLoopYield, bCityEffects: true);
                if (iTotalOutput != 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_PER_YEAR", buildYieldValueIconLinkVariable(eLoopYield, iTotalOutput, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));

                    bool bShowModifiers = false;

                    {
                        int iValue = 0;
                        infos().Helpers.yieldOutputImprovement(eImprovement, eLoopYield, ResourceType.NONE, pGame, ref iValue);

/*####### Better Old World AI - Base DLL #######
  ### TerrainYield fix                 START ###
  ##############################################*/
                        int iValueTerrain = infos().improvement(eImprovement).maaiTerrainYieldOutput[pTile.getTerrain(), eLoopYield];
                        int iValueSum = iValue + iValueTerrain;
                        if (iValueSum != iTotalOutput && iValueSum != 0)
                        { 
                            if ((iValue != 0))
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER), buildImprovementLinkVariable(eImprovement, pGame));
                                }
                            }
                            if (iValueTerrain != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValueTerrain, false, Constants.YIELDS_MULTIPLIER), buildTerrainLinkVariable(pTile.getTerrain(), pTile));
                                }
                            }

                            bShowModifiers = true;
                        }
                        else if (iValueTerrain != 0)
                        {
                            using (buildSecondaryTextScope(builder))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValueTerrain, false, Constants.YIELDS_MULTIPLIER), buildTerrainLinkVariable(pTile.getTerrain(), pTile));
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### TerrainYield fix                   END ###
  ##############################################*/
                    }

                    for (int i = 0; i < pGame.getNumOccurrences(); ++i)
                    {
                        OccurrenceData pLoopData = pGame.getOccurrenceDataAt(i);
                        if (pGame.isOccurrenceActive(pLoopData.miID))
                        {
                            InfoOccurrence occurrence = infos().occurrence(pLoopData.meType);
                            int iValue = occurrence.miBaseYieldCoastModifier;
                            if (iValue != 0 && (pTile.isSaltCoastLand() || pTile.isSaltCoastWater()))
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType, pLoopData));
                                }
                            }
                            iValue = occurrence.miBaseYieldRiverModifier;
                            if (iValue != 0 && pTile.isRiver())
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType, pLoopData));
                                }
                            }
                            iValue = occurrence.miTileBaseYieldModifier;
                            if (iValue != 0 && pLoopData.isAffectedTile(pTile.getID()))
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType, pLoopData));
                                }
                            }
                            iValue = occurrence.miTileBaseYieldModifierAdjacent;
                            if (iValue != 0)
                            {
                                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                {
                                    Tile pAdjacentTile = pTile.tileAdjacent(eLoopDirection);
                                    if (pAdjacentTile != null && pLoopData.isAffectedTile(pAdjacentTile.getID()))
                                    {
                                        using (buildSecondaryTextScope(builder))
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType, pLoopData));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //{
                    //    int iValue = infos().improvement(eImprovement).maaiTerrainYieldOutput[pTile.getTerrain(), eLoopYield];
                    //    if (iValue != 0)
                    //    {
                    //        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER), buildTerrainLinkVariable(pTile.getTerrain(), pTile));
                    //    }
                    //}

                    {
                        int iValue = infos().improvement(eImprovement).maiAdjacentWonderYieldOutput[eLoopYield];
                        if (iValue != 0)
                        {
                            int iCount = pTile.countTeamAdjacentWonders();
                            if (iCount > 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT_WONDER", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER));
                                }

                                bShowModifiers = true;
                            }
                        }
                    }

                    {
                        int iValue = infos().improvement(eImprovement).maiAdjacentResourceYieldOutput[eLoopYield];
                        if (iValue != 0)
                        {
                            int iCount = pTile.countTeamAdjacentResources();
                            if (iCount > 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT_RESOURCE", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER));
                                }

                                bShowModifiers = true;
                            }
                        }
                    }

                    {
                        int iValue = infos().improvement(eImprovement).maiTradeNetworkYieldOutput[eLoopYield];
                        if (iValue != 0)
                        {
                            if (pCityTerritory != null)
                            {
                                if (pTile.tradeNetworkTestImprovement(pCityTerritory.getPlayer(), eImprovement))
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_TRADE_NETWORK", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER));
                                    }

                                    bShowModifiers = true;
                                }
                            }
                        }
                    }

                    for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                    {
                        int iValue = infos().improvement(eImprovement).maaiAdjacentImprovementClassYield[eLoopImprovementClass, eLoopYield];
                        if (iValue != 0)
                        {
                            int iCount = pTile.countTeamAdjacentImprovementClassFinished(eLoopImprovementClass);
                            if (iCount > 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER), buildImprovementClassLinkVariable(eLoopImprovementClass));
                                }

                                bShowModifiers = true;
                            }
                        }
                    }

                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        if (eResource != ResourceType.NONE)
                        {
                            int iValue = 0;
                            infos().Helpers.yieldOutputResource(eImprovementClass, eLoopYield, eResource, ref iValue);
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildResourceLinkVariable(eResource));
                                }

                                bShowModifiers = true;
                            }
                        }

                        {
                            int iValue = infos().improvementClass(eImprovementClass).maiAdjacentResourceYieldOutput[eLoopYield];
                            if (iValue != 0)
                            {
                                int iCount = pTile.countTeamAdjacentResources();
                                if (iCount > 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT_RESOURCE", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER));
                                    }

                                    bShowModifiers = true;
                                }
                            }
                        }

                        {
                            ReligionType eReligionPrereq = infos().improvement(eImprovement).meReligionPrereq;

                            if (eReligionPrereq != ReligionType.NONE)
                            {
                                for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                                {
                                    if (pGame.isReligionTheology(eReligionPrereq, eLoopTheology))
                                    {
                                        int iValue = infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput[eLoopTheology, eLoopYield];
                                        if (iValue != 0)
                                        {
                                            using (buildSecondaryTextScope(builder))
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildTheologyLinkVariable(eLoopTheology, eReligionPrereq, true));
                                            }

                                            bShowModifiers = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (pCityTerritory != null)
                        {
                            foreach (EffectCityType eLoopEffectCity in pCityTerritory.getActiveEffectCity())
                            {
                                int iCount = pCityTerritory.getEffectCityCount(eLoopEffectCity);
                                int iValue = infos().effectCity(eLoopEffectCity).maaiImprovementYield[eImprovement, eLoopYield] + infos().effectCity(eLoopEffectCity).maaiImprovementClassYield[eImprovementClass, eLoopYield];
                                if (iValue != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_X",
                                            buildSignedTextVariable((iCount * iValue), false, Constants.YIELDS_MULTIPLIER),
                                            buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory.governor(), pGame, pActivePlayer),
                                            (iCount > 1 ? TEXTVAR(iCount) : TEXTVAR(false)));
                                    }

                                    bShowModifiers = true;
                                }
                            }
                        }
                    }

                    if (bShowModifiers)
                    {
                        if (pCityTerritory != null)
                        {
                            foreach (EffectCityType eLoopEffectCity in pCityTerritory.getActiveEffectCity())
                            {
                                int iCount = pCityTerritory.getEffectCityCount(eLoopEffectCity);
                                int iValue = pTile.getTileEffectCityModifier(eLoopEffectCity, eImprovement);

                                if (iValue != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_X",
                                            buildSignedTextVariable((iCount * iValue), true),
                                            buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory.governor(), pGame, pActivePlayer),
                                            (iCount > 1 ? TEXTVAR(iCount) : TEXTVAR(false)));
                                    }
                                }
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).maaiTerrainYieldModifier[pTile.getTerrain(), eLoopYield];
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true), buildTerrainLinkVariable(pTile.getTerrain(), pTile));
                                }
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).maaiHeightYieldModifier[pTile.getHeight(), eLoopYield];
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true), buildHeightLinkVariable(pTile.getHeight(), pTile));
                                }
                            }
                        }

                        if (pTile.isFreshWaterAccess())
                        {
                            int iValue = infos().improvement(eImprovement).miFreshWaterModifier + infos().improvement(eImprovement).maiYieldFreshWaterModifier[eLoopYield];
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true), buildFreshWaterLinkVariable(pTile));
                                }
                            }
                        }

                        if (pTile.isRiver())
                        {
                            int iValue = infos().improvement(eImprovement).miRiverModifier + infos().improvement(eImprovement).maiYieldRiverModifier[eLoopYield];
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true), buildRiverLinkVariable(pTile));
                                }
                            }
                        }

                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            Tile pAdjacentTile = pTile.tileAdjacent(eLoopDirection, true);

                            if (pAdjacentTile != null)
                            {
                                {
                                    int iValue = infos().improvement(eImprovement).maaiAdjacentHeightYieldModifier[pAdjacentTile.getHeight(), eLoopYield];
                                    if (iValue != 0)
                                    {
                                        using (buildSecondaryTextScope(builder))
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT", buildSignedTextVariable(iValue, true), buildHeightLinkVariable(pAdjacentTile.getHeight()));
                                        }
                                    }
                                }

                                if (pCityTerritory != null)
                                {
                                    if (pAdjacentTile.getTeam() == pCityTerritory.getTeam())
                                    {
                                        if (pAdjacentTile.hasImprovementFinished())
                                        {
                                            int iValue = pAdjacentTile.improvement().maiAdjacentImprovementModifier[eImprovement];
                                            if (pAdjacentTile.hasImprovementClass() && (eImprovementClass != ImprovementClassType.NONE))
                                            {
                                                iValue += pAdjacentTile.improvement().maiAdjacentImprovementClassModifier[eImprovementClass];
                                                iValue += pAdjacentTile.improvementClass().maiAdjacentImprovementClassModifier[eImprovementClass];
                                            }
                                            if (iValue != 0)
                                            {
                                                using (buildSecondaryTextScope(builder))
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT", buildSignedTextVariable(iValue, true), buildImprovementLinkVariable(pAdjacentTile.getImprovement(), pGame, pAdjacentTile));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (eSpecialist != SpecialistType.NONE)
                        {
                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                int iValue = infos().specialist(eSpecialist).maiImprovementClassModifier[eImprovementClass];
                                if (iValue != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_PERCENT_FROM", buildValueTextVariable((iValue + 100)), buildSpecialistLinkVariable(eSpecialist, pGame));
                                    }
                                }
                            }
                        }
                    }

                    if (pCityTerritory != null)
                    {
                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvement(eImprovement).meEffectCity, false, eLoopYield, pCityTerritory, pManager, iTotalOutput);

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvementClass(eImprovementClass).meEffectCity, false, eLoopYield, pCityTerritory, pManager, iTotalOutput);

                            if (eResource != ResourceType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvementClass(eImprovementClass).maeResourceCityEffect[eResource], true, eLoopYield, pCityTerritory, pManager);
                            }

                            {
                                ReligionType ePrereqReligion = infos().improvement(eImprovement).meReligionPrereq;

                                if (ePrereqReligion != ReligionType.NONE)
                                {
                                    for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                                    {
                                        if (pGame.isReligionTheology(ePrereqReligion, eLoopTheology))
                                        {
                                            EffectCityType eEffectCity = mInfos.improvementClass(eImprovementClass).maeTheologyCityEffect[eLoopTheology];

                                            if (eEffectCity != EffectCityType.NONE)
                                            {
                                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        {
                            EffectPlayerType eEffectPlayer = infos().improvement(eImprovement).meEffectPlayer;

                            if (eEffectPlayer != EffectPlayerType.NONE)
                            {
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                {
                                    EffectCityType eEffectCityExtra = infos().effectPlayer(eEffectPlayer).meEffectCityExtra;

                                    if (eEffectCityExtra != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCityExtra, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                if (pCityTerritory.isCapital())
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meCapitalEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                if (pCityTerritory.isConnected())
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meConnectedEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                if (pCityTerritory.hasPlayer() && pCityTerritory.player().hasStateReligion())
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meStateReligionEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        ReligionType eReligion = pCityTerritory.player().stateReligion().meType;

                                        if (eReligion != ReligionType.NONE && pCityTerritory.isReligion(eReligion))
                                        {
                                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                        }
                                    }
                                }
                            }
                        }

                        if (eSpecialist != SpecialistType.NONE)
                        {
                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialist(eSpecialist).meEffectCity, true, eLoopYield, pCityTerritory, pManager);
                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialist(eSpecialist).meEffectCityExtra, true, eLoopYield, pCityTerritory, pManager);
                            if (infos().specialist(eSpecialist).meClass != SpecialistClassType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialistClass(infos().specialist(eSpecialist).meClass).meEffectCity, true, eLoopYield, pCityTerritory, pManager);
                            }

                            if (eResource != ResourceType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialistClass(infos().specialist(eSpecialist).meClass).maeResourceCityEffect[eResource], true, eLoopYield, pCityTerritory, pManager);
                            }
                        }
                    }
                }
            }
            return builder;
        }
        //copy-paste END

        //1k lines of copy-paste START
        //lines 20296-21274
        public override TextBuilder buildImprovementHelp(TextBuilder builder, ImprovementType eImprovement, Tile pTile, ClientManager pManager, bool bName = true, bool bCosts = true, bool bDetails = true, bool bEncyclopedia = false, TextBuilder.ScopeType scopeType = TextBuilder.ScopeType.NONE)
        {
            //using (new UnityProfileScope("HelpText.buildImprovementHelp"))
            using (var effectListScoped = CollectionCache.GetListScoped<EffectCityType>())
            {
                //bool bInclucePotentialBonusesTitle = true;

                Game pGame = pManager.GameClient;

                Player pPlayer = ((pTile != null) ? pTile.owner() : null);

                Player pActivePlayer = pManager.activePlayer();
                if (pPlayer == null)
                {
                    pPlayer = pActivePlayer;
                }

                City pCityTerritory = ((pTile != null) ? pTile.cityTerritory() : null);

                ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

                ReligionType eReligionPrereq = infos().improvement(eImprovement).meReligionPrereq;

                getImprovementEffectCityList(eImprovement, pManager, effectListScoped.Value);

                using (builder.BeginScope(scopeType))
                {
                    if (bName)
                    {
                        builder.Add(buildTitleVariable(TEXTVAR_TYPE(mInfos.improvement(eImprovement).mName)));
                    }

                    if ((pTile != null) && pTile.isInfoVisible(pActivePlayer.getTeam(), pManager))
                    {
                        buildImprovementBreakdown(builder, eImprovement, SpecialistType.NONE, pTile, pManager);
                    }

                    using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                    {
                        {
                            int iValue = infos().improvement(eImprovement).miDefenseModifier;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEFENSE_MODIFIER", buildDefenseValueLinkVariable(iValue, true));
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miDefenseModifierFriendly;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEFENSE_MODIFIER_FRIENDLY", buildDefenseValueLinkVariable(iValue, true));
                            }
                        }

                        if (infos().improvement(eImprovement).mbFreshWaterSource)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_FRESH_WATER_SOURCE", buildFreshWaterLinkVariable(pTile));
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miVP;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_VP", buildSignedTextVariable(iValue));
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miVisionChange;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_VISION_CHANGE", buildSignedTextVariable(iValue));
                            }
                        }

                        if (infos().improvement(eImprovement).mbTribe)
                        {
                            if ((pTile != null) && pTile.isTribeSite())
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_PRODUCES_TRIBE_UNITS", buildTribeLinkVariable(pTile.getTribeSite(), pGame));
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_PRODUCES_GENERIC_TRIBE_UNITS");
                            }
                        }

                        if (infos().improvement(eImprovement).mbBonus)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_BONUS");
                        }

                        if ((pTile == null) || !(pTile.isInfoVisible(pActivePlayer.getTeam(), pManager)))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_OUTPUT_YIELDS", buildTurnScaleName(pGame)), scopeKey: "YIELD-LIST"))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = 0;
                                    infos().Helpers.yieldOutputImprovement(eImprovement, eLoopYield, ResourceType.NONE, pGame, ref iOutput);
                                    if (iOutput != 0)
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }

                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_CONSUMPTION_YIELDS", buildTurnScaleName(pGame)), scopeKey: "YIELD-LIST"))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvement(eImprovement).maiYieldConsumption[eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }

                        foreach (EffectCityType eEffectCity in effectListScoped.Value)
                        {
                            buildEffectCityHelpYields(builder, eEffectCity, pGame, null, pActivePlayer);
                        }

                        foreach (EffectCityType eEffectCity in effectListScoped.Value)
                        {
                            buildEffectCityHelpNoYields(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), pActivePlayer);
                        }

                        {
                            ImprovementType eAdjacentImprovement = infos().improvement(eImprovement).meAdjacentImprovementSpecialist;
                            if (eAdjacentImprovement != ImprovementType.NONE)
                            {
                                SpecialistType eSpecialist = infos().improvement(eAdjacentImprovement).meSpecialist;
                                if (eSpecialist != SpecialistType.NONE)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_BONUS_FREE_IMPROVEMENT_SPECIALIST_ADJACENT", buildSpecialistLinkVariable(eSpecialist, pGame), buildImprovementLinkVariable(eAdjacentImprovement, pGame));
                                }
                            }
                        }

                        if (pGame?.isCharacters() ?? true)
                        {
                            int iValue = infos().improvement(eImprovement).miLegitimacy;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_LEGITIMACY", buildSignedTextVariable(iValue));
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miUnitHeal;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_HEALS_UNIT", buildSignedTextVariable(iValue));
                            }
                        }

                        {
                            using (var hashSetScope = CollectionCache.GetHashSetScoped<UnitTraitType>())
                            {
                                HashSet<UnitTraitType> seUnitTraitsAdded = hashSetScope.Value;

                                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                                {
                                    if (!(seUnitTraitsAdded.Contains(eLoopUnitTrait)))
                                    {
                                        int iValue = infos().improvement(eImprovement).maiUnitTraitHeal[eLoopUnitTrait];
                                        if (iValue != 0)
                                        {
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_UNIT_TRAIT_HEALS_UNIT", buildSignedTextVariable(iValue))))
                                            {
                                                {
                                                    builder.Add(buildUnitTraitLinkVariable(eLoopUnitTrait));
                                                    seUnitTraitsAdded.Add(eLoopUnitTrait);
                                                }

                                                for (UnitTraitType eOtherUnitTrait = 0; eOtherUnitTrait < infos().unitTraitsNum(); eOtherUnitTrait++)
                                                {
                                                    if (eOtherUnitTrait != eLoopUnitTrait)
                                                    {
                                                        if (infos().improvement(eImprovement).maiUnitTraitHeal[eOtherUnitTrait] == iValue)
                                                        {
                                                            builder.Add(buildUnitTraitLinkVariable(eOtherUnitTrait));
                                                            seUnitTraitsAdded.Add(eOtherUnitTrait);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        {
                            using (var hashSetScope = CollectionCache.GetHashSetScoped<UnitTraitType>())
                            {
                                HashSet<UnitTraitType> seUnitTraitsAdded = hashSetScope.Value;

                                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                                {
                                    if (!(seUnitTraitsAdded.Contains(eLoopUnitTrait)))
                                    {
                                        int iValue = infos().improvement(eImprovement).maiUnitTraitXP[eLoopUnitTrait];
                                        if (iValue != 0)
                                        {
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_UNIT_TRAIT_XP", buildSignedTextVariable(iValue), buildTurnScaleName(pGame))))
                                            {
                                                {
                                                    builder.Add(buildUnitTraitLinkVariable(eLoopUnitTrait));
                                                    seUnitTraitsAdded.Add(eLoopUnitTrait);
                                                }

                                                for (UnitTraitType eOtherUnitTrait = 0; eOtherUnitTrait < infos().unitTraitsNum(); eOtherUnitTrait++)
                                                {
                                                    if (eOtherUnitTrait != eLoopUnitTrait)
                                                    {
                                                        if (infos().improvement(eImprovement).maiUnitTraitXP[eOtherUnitTrait] == iValue)
                                                        {
                                                            builder.Add(buildUnitTraitLinkVariable(eOtherUnitTrait));
                                                            seUnitTraitsAdded.Add(eOtherUnitTrait);
                                                        }
                                                    }
                                                }
                                            }
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_CITY_HELP_SPAWN_POINT_UNIT_TRAIT")))
                                            {
                                                foreach (UnitTraitType eXpTrait in seUnitTraitsAdded)
                                                {
                                                    builder.Add(buildUnitTraitLinkVariable(eXpTrait));
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            {
                                EffectPlayerType eEffectPlayer = infos().improvement(eImprovement).meEffectPlayer;

                                if (eEffectPlayer != EffectPlayerType.NONE)
                                {
                                    buildEffectPlayerHelp(builder, eEffectPlayer, pGame, pPlayer, pActivePlayer, bAllCities: true);
                                }
                            }

                            {
                                BonusType eBonus = infos().improvement(eImprovement).meBonus;

                                if (eBonus != BonusType.NONE)
                                {
                                    buildBonusHelp(builder, eBonus, pGame, pPlayer, pActivePlayer, pTile, bName: false, startLineVariable: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_BONUS_ON_COMPLETION"));
                                }
                            }

                            {
                                BonusType eBonusCities = infos().improvement(eImprovement).meBonusCities;

                                if (eBonusCities != BonusType.NONE)
                                {
                                    buildBonusHelp(builder, eBonusCities, pGame, pPlayer, pActivePlayer, bShowCity: false, startLineVariable: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_BONUS_ALL_CITIES"));
                                }
                            }

                            if (bDetails)
                            {
                                {
                                    ReligionType eReligionSpread = pGame?.getImprovementReligionSpread(eImprovement) ?? infos().improvement(eImprovement).meReligionSpread;

                                    if (eReligionSpread != ReligionType.NONE)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_SPREADS_RELIGION", buildReligionLinkVariable(eReligionSpread, pGame, pActivePlayer));

                                        {
                                            EffectCityType eEffectCity = infos().religion(eReligionSpread).meEffectCity;

                                            if (eEffectCity != EffectCityType.NONE)
                                            {
                                                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                                {
                                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                                    {
                                                        buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), true, pActivePlayer, bSkipImpossible: !bEncyclopedia);
                                                    }

                                                    if (subText.HasContent)
                                                    {
                                                        builder.AddWithParenthesis(subText.ToTextVariable());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                {
                                    int iValue = infos().improvement(eImprovement).miOpinionReligion;
                                    if (iValue != 0)
                                    {
                                        if (pGame?.isCharacters() ?? true)
                                        {
                                            ReligionType eReligionSpread = pGame?.getImprovementReligionSpread(eImprovement) ?? infos().improvement(eImprovement).meReligionSpread;

                                            if ((eReligionSpread != ReligionType.NONE) || (eReligionPrereq != ReligionType.NONE))
                                            {
                                                if (eReligionSpread == eReligionPrereq)
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_OPINION_RELIGION", buildSignedTextVariable(iValue), buildReligionLinkVariable(eReligionSpread, pGame, pActivePlayer));
                                                }
                                                else
                                                {
                                                    if (eReligionSpread != ReligionType.NONE)
                                                    {
                                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_OPINION_RELIGION", buildSignedTextVariable(iValue), buildReligionLinkVariable(eReligionSpread, pGame, pActivePlayer));
                                                    }
                                                    if (eReligionPrereq != ReligionType.NONE)
                                                    {
                                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_OPINION_RELIGION", buildSignedTextVariable(iValue), buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

/*####### Better Old World AI - Base DLL #######
  ### leaving in some info             START ###
  ##############################################*/
                            if (bEncyclopedia && infos().improvement(eImprovement).mbNoAdjacentReligion)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT_RELIGION");
                            }
/*####### Better Old World AI - Base DLL #######
  ### leaving in some info               END ###
  ##############################################*/

                            for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                            {
                                if (infos().unit(eLoopUnit).meImprovementPrereq == eImprovement)
                                {
                                    if ((pPlayer != null) ? pPlayer.canEverBuildUnit(eLoopUnit) : true)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_UNLOCKS_UNIT", buildUnitTypeLinkVariable(eLoopUnit, pGame, pCity: pCityTerritory));
                                    }
                                }
                            }

                            if (infos().improvement(eImprovement).mbHeal)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HEAL");
                            }

                            if (infos().improvement(eImprovement).mbRemovePillage)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_REMOVE_PILLAGE");
                            }

                            if (!(infos().improvement(eImprovement).mbTribe))
                            {
                                int iValue = infos().improvement(eImprovement).miUnitTurns;
                                if (iValue > 0)
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_CREATES_UNITS", iValue, buildTurnScaleName(pGame, iValue))))
                                    {
                                        for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                                        {
                                            if (infos().improvement(eImprovement).maiUnitDie[eLoopUnit] > 0)
                                            {
                                                builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, pCity: pCityTerritory));
                                            }
                                        }
                                    }
                                }
                            }

                            {
                                SpecialistType eSpecialist = infos().improvement(eImprovement).meSpecialist;

                                if (eSpecialist != SpecialistType.NONE)
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_ENABLES", buildSpecialistLinkVariable(eSpecialist, pGame).SetKey("LINK"));

                                        if (pTile != null)
                                        {
                                            if (pTile.hasResource())
                                            {
                                                EffectCityType eEffectCity = infos().specialistClass(infos().specialist(eSpecialist).meClass).maeResourceCityEffect[pTile.getResource()];

                                                if (eEffectCity != EffectCityType.NONE)
                                                {
                                                    buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), true, pActivePlayer, bSkipImpossible: !bEncyclopedia);
                                                }
                                            }
                                        }
                                    }
/*####### Better Old World AI - Base DLL #######
  ### show Specialist Prod Discount    START ###
  ##############################################*/
                                    {
                                        int iSpecialistCostExtra = infos().improvement(eImprovement).miSpecialistCost;
                                        if (iSpecialistCostExtra != 0)
                                        {
                                            SpecialistClassType eSpecialistClass = infos().specialist(eSpecialist).meClass;
                                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_SPECIALIST_COST", buildYieldValueIconLinkVariable(infos().Globals.CIVICS_YIELD, iSpecialistCostExtra), (eSpecialistClass != SpecialistClassType.NONE) ? buildSpecialistClassLinkVariable(eSpecialistClass, pTile) : buildSpecialistLinkVariable(eSpecialist, pGame, pTile));
                                        }
                                    }
/*####### Better Old World AI - Base DLL #######
  ### show Specialist Prod Discount      END ###
  ##############################################*/
                                }
                            }

                            if (bDetails)
                            {
                                using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_ENABLES"), scopeKey: "LINK"))
                                {
                                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                    {
                                        if (infos().improvement(eLoopImprovement).meImprovementPrereq == eImprovement)
                                        {
                                            builder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                        }

                                        if (infos().improvement(eLoopImprovement).meEffectCityPrereq != EffectCityType.NONE && infos().improvement(eImprovement).meEffectPlayer != EffectPlayerType.NONE)
                                        {
                                            if (infos().improvement(eLoopImprovement).meEffectCityPrereq == infos().effectPlayer(infos().improvement(eImprovement).meEffectPlayer).meEffectCity)
                                            {
                                                builder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                            }
                                        }
                                    }
                                }
                            }

                            using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementType>())
                            {
                                HashSet<ImprovementType> seImprovementsAdded = hashSetScope.Value;
                                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                {
                                    if (!(seImprovementsAdded.Contains(eLoopImprovement)) && (eLoopImprovement != eImprovement))
                                    {
                                        if ((pPlayer != null) ? pPlayer.isNationImprovement(eLoopImprovement) : true)
                                        {
                                            int iValue = infos().improvement(eImprovement).maiAdjacentImprovementModifier[eLoopImprovement];
                                            if (iValue != 0)
                                            {
                                                using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_FOR_ADJACENT_IMPROVEMENT_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                {
                                                    builder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                                    seImprovementsAdded.Add(eLoopImprovement);

                                                    for (ImprovementType eOtherImprovement = 0; eOtherImprovement < infos().improvementsNum(); eOtherImprovement++)
                                                    {
                                                        if (eOtherImprovement != eLoopImprovement)
                                                        {
                                                            if ((pPlayer != null) ? pPlayer.isNationImprovement(eOtherImprovement) : true)
                                                            {
                                                                if (infos().improvement(eImprovement).maiAdjacentImprovementModifier[eOtherImprovement] == iValue)
                                                                {
                                                                    builder.Add(buildImprovementLinkVariable(eOtherImprovement, pGame));
                                                                    seImprovementsAdded.Add(eOtherImprovement);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementClassType>())
                            {
                                HashSet<ImprovementClassType> seImprovementClassessAdded = hashSetScope.Value;
                                for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                                {
                                    if (!(seImprovementClassessAdded.Contains(eLoopImprovementClass)) && (eLoopImprovementClass != eImprovementClass))
                                    {
                                        int iValue = infos().improvement(eImprovement).maiAdjacentImprovementClassModifier[eLoopImprovementClass];
                                        if (iValue != 0)
                                        {
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_FOR_ADJACENT_IMPROVEMENT_CLASS_MODIFIER", buildSignedTextVariable(iValue, true))))
                                            {
                                                builder.Add(buildImprovementClassLinkVariable(eLoopImprovementClass));
                                                seImprovementClassessAdded.Add(eLoopImprovementClass);

                                                for (ImprovementClassType eOtherImprovementClass = 0; eOtherImprovementClass < infos().improvementClassesNum(); eOtherImprovementClass++)
                                                {
                                                    if (eOtherImprovementClass != eLoopImprovementClass)
                                                    {
                                                        if (infos().improvement(eImprovement).maiAdjacentImprovementClassModifier[eOtherImprovementClass] == iValue)
                                                        {
                                                            builder.Add(buildImprovementClassLinkVariable(eOtherImprovementClass));
                                                            seImprovementClassessAdded.Add(eOtherImprovementClass);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }



                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementClassType>())
                                {
                                    HashSet<ImprovementClassType> seImprovementClassessAdded = hashSetScope.Value;
                                    for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                                    {
                                        if (!(seImprovementClassessAdded.Contains(eLoopImprovementClass)) && (eLoopImprovementClass != eImprovementClass))
                                        {
                                            int iValue = infos().improvementClass(eImprovementClass).maiAdjacentImprovementClassModifier[eLoopImprovementClass];
                                            if (iValue != 0)
                                            {
                                                using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_FOR_ADJACENT_IMPROVEMENT_CLASS_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                {
                                                    builder.Add(buildImprovementClassLinkVariable(eLoopImprovementClass));
                                                    seImprovementClassessAdded.Add(eLoopImprovementClass);

                                                    for (ImprovementClassType eOtherImprovementClass = 0; eOtherImprovementClass < infos().improvementClassesNum(); eOtherImprovementClass++)
                                                    {
                                                        if (eOtherImprovementClass != eLoopImprovementClass)
                                                        {
                                                            if (infos().improvementClass(eImprovementClass).maiAdjacentImprovementClassModifier[eOtherImprovementClass] == iValue)
                                                            {
                                                                builder.Add(buildImprovementClassLinkVariable(eOtherImprovementClass));
                                                                seImprovementClassessAdded.Add(eOtherImprovementClass);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (infos().improvementClass(eImprovementClass).maeResourceCityEffect.Count > 0 && pPlayer != null && pTile != null)
                                {
                                    for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                    {
                                        if (pTile.getResource() == eLoopResource)
                                        {
                                            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < infos().effectCitiesNum(); eLoopEffectCity++)
                                            {
                                                using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                                                {
                                                    if (infos().improvementClass(eImprovementClass).maeResourceCityEffect[eLoopResource] == eLoopEffectCity)
                                                    {
                                                        for (EffectCityType eOtherEffectCity = 0; eOtherEffectCity < infos().effectCitiesNum(); eOtherEffectCity++)
                                                        {
                                                            EffectCityType eEffectCity = infos().effectCity(eOtherEffectCity).maeEffectCityEffectCity[eLoopEffectCity];
                                                            if (eEffectCity != EffectCityType.NONE && infos().effectCity(eOtherEffectCity).meSourceNation == pPlayer.getNation())
                                                            {
                                                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_NO_YIELDS_EFFECT_CITY_EFFECT_CITY", buildEffectCityLinkVariable(eLoopEffectCity, null, null), buildEffectCityLinkVariable(eEffectCity, null, null));
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if ((pTile == null) || (pTile.getImprovement() != eImprovement))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                            {
                                ImprovementType eDevelopImprovement = mInfos.Helpers.getDevelopImprovement(eImprovement, pGame?.getTribeLevel() ?? TribeLevelType.NONE);

                                if (eDevelopImprovement != ImprovementType.NONE)
                                {
                                    TextVariable developImprovement = buildImprovementLinkVariable(eDevelopImprovement, pGame);

                                    int iDevelopTurns = infos().improvement(eImprovement).miDevelopTurns;

                                    if (infos().improvement(eImprovement).miDevelopRand != 0)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEVELOP_TURNS_RANDOM", developImprovement, TEXTVAR(iDevelopTurns - infos().improvement(eImprovement).miDevelopRand), TEXTVAR(iDevelopTurns));
                                    }
                                    else
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEVELOP_TURNS", developImprovement, TEXTVAR(iDevelopTurns), buildTurnScaleName(pGame, iDevelopTurns));
                                    }
                                }
                            }

                            if (bCosts)
                            {
                                buildImprovementCostsHelp(builder, eImprovement, pTile, pManager, true);
                            }

                            if (bDetails)
                            {
                                if (eReligionPrereq != ReligionType.NONE)
                                {
                                    using (TextBuilder subCommaText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subCommaText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                                            {
                                                if (infos().unit(eLoopUnit).meBuildReligion == eReligionPrereq)
                                                {
                                                    subCommaText.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame));
                                                }
                                            }
                                        }

                                        if (subCommaText.HasContent)
                                        {
                                            builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_BUILT_BY", subCommaText.ToTextVariable()));
                                        }
                                    }
                                }
                            }
                        }
                    }

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/
                    BetterAIInfoImprovement pImprovementInfo = ((BetterAIInfoImprovement)(infos().improvement(eImprovement)));

                    if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE)
                    {
                        builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ADDS_ADJACENT_BONUS_IMPROVEMENT", TEXTVAR_TYPE(infos().improvement(pImprovementInfo.meBonusAdjacentImprovement).mName)));
                    }
                    else if (pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                    {
                        builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ADDS_ADJACENT_BONUS_IMPROVEMENT", TEXTVAR_TYPE(infos().improvementClass(pImprovementInfo.meBonusAdjacentImprovementClass).mName)));
                    }
/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

                    if (infos().improvement(eImprovement).mbWonder)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_WONDER");
                    }

                    if (bDetails)
                    {
                        if (infos().improvement(eImprovement).mbUrban)
                        {
                            if (!(infos().improvement(eImprovement).mbRequiresUrban))
                            {
                                TextVariable urbanVariable = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_URBAN_BUILDING_ANYWHERE", buildTerrainLinkVariable(infos().Globals.URBAN_TERRAIN));
                                builder.Add(urbanVariable);
                            }
                        }
                        else if (infos().improvement(eImprovement).mbSpreadsBorders)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_SPREADS_BORDERS");
                        }

                        if (infos().improvement(eImprovement).mbPermanent)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_PERMANENT_BUILDING");
                        }
                    }
                }

                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                using (TextBuilder potentialText = TextBuilder.GetTextBuilder(TextManager))
                {
                    using (subText.BeginScope(scopeType))
                    using (subText.BeginScope(TextBuilder.ScopeType.BULLET))
                    {
                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_WONDER", buildTurnScaleName(pGame))))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiAdjacentWonderYieldOutput[eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_RESOURCE", buildTurnScaleName(pGame))))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiAdjacentResourceYieldOutput[eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_TRADE_NETWORK")))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiTradeNetworkYieldOutput[eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_IMPROVEMENTCLASS", buildTurnScaleName(pGame), buildImprovementClassLinkVariable(eLoopImprovementClass))))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvement(eImprovement).maaiAdjacentImprovementClassYield[eLoopImprovementClass, eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
/*####### Better Old World AI - Base DLL #######
  ### leaving in some info             START ###
  ##############################################*/
                            if (pTile == null)
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: buildColonSpaceOne(buildResourceLinkVariable(eLoopResource), null))) // arg1 for buildColonSpaceOne will be filled with list
                                    {
                                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                        {
                                            int iValue = 0;
                                            infos().Helpers.yieldOutputResource(eImprovementClass, eLoopYield, eLoopResource, ref iValue);
                                            if (iValue != 0)
                                            {
                                                subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                            }
                                        }
/*####### Better Old World AI - Base DLL #######
  ### show abNoBaseOutput              START ###
  ##############################################*/
                                        if (infos().improvement(eImprovement).mabNoBaseOutput[(int)eLoopResource])
                                        {
                                            subText.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_REPLACES_BASE_IMPROVEMENT_OUTPUT"));
                                        }
/*####### Better Old World AI - Base DLL #######
  ### show abNoBaseOutput                END ###
  ##############################################*/
                                    }
                                }
                            }
/*####### Better Old World AI - Base DLL #######
  ### leaving in some info               END ###
  ##############################################*/

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_RESOURCE", buildTurnScaleName(pGame))))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvementClass(eImprovementClass).maiAdjacentResourceYieldOutput[eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }

                            for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: buildColonSpaceOne(buildTerrainLinkVariable(eLoopTerrain), null)))
                                {
                                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                    {
                                        int iOutput = infos().improvement(eImprovement).maaiTerrainYieldOutput[eLoopTerrain, eLoopYield];
                                        if (iOutput != 0)
                                        {
                                            subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                        }
                                    }
                                }
                            }

                            for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iValue = infos().improvement(eImprovement).maaiTerrainYieldModifier[eLoopTerrain, eLoopYield];
                                    if (iValue != 0)
                                    {
                                        subText.Add(buildColonSpaceOne(buildTerrainLinkVariable(eLoopTerrain), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true)));
                                    }
                                }
                            }

                            for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iValue = infos().improvement(eImprovement).maaiHeightYieldModifier[eLoopHeight, eLoopYield];
                                    if (iValue != 0)
                                    {
                                        subText.Add(buildColonSpaceOne(buildHeightLinkVariable(eLoopHeight), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true)));
                                    }
                                }
                            }
                        }

                        for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iValue = infos().improvement(eImprovement).maaiAdjacentHeightYieldModifier[eLoopHeight, eLoopYield];
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_ADJACENT_COLON", buildHeightLinkVariable(eLoopHeight), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true));
                                }
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miFreshWaterModifier;
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildFreshWaterLinkVariable(), buildSignedTextVariable(iValue, true)));
                            }
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            int iValue = infos().improvement(eImprovement).maiYieldFreshWaterModifier[eLoopYield];
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildFreshWaterLinkVariable(), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true)));
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miRiverModifier;
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildRiverLinkVariable(), buildSignedTextVariable(iValue, true)));
                            }
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            int iValue = infos().improvement(eImprovement).maiYieldRiverModifier[eLoopYield];
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildRiverLinkVariable(), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true)));
                            }
                        }

                        if (bDetails)
                        {
                            foreach (EffectCityType eEffectCity in effectListScoped.Value)
                            {
                                buildEffectCityHelpYieldsPotential(subText, eEffectCity, pGame, null, pActivePlayer);
                            }

                            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < mInfos.effectCitiesNum(); eLoopEffectCity++)
                            {
                                if ((infos().effectCity(eLoopEffectCity).maaiImprovementYield.Count > 0) || (infos().effectCity(eLoopEffectCity).maaiImprovementClassYield.Count > 0))
                                {
                                    ImprovementType eSourceImprovement = infos().effectCity(eLoopEffectCity).meSourceImprovement; 
                                    ProjectType eSourceProject = infos().effectCity(eLoopEffectCity).meSourceProject;
                                    bool contentOk = eSourceImprovement == ImprovementType.NONE || (pGame != null ? pGame.checkGameContent(eSourceImprovement) : App.CheckContentOwnership(infos().improvement(eSourceImprovement).meGameContentRequired, infos(), null));
                                    contentOk &= eSourceProject == ProjectType.NONE || (pGame != null ? pGame.checkGameContent(eSourceProject) : App.CheckContentOwnership(infos().project(eSourceProject).meGameContentRequired, infos(), null));
                                    if (!contentOk)
                                    {
                                        continue;
                                    }
                                    CommaListVariableGenerator yieldsList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);

                                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                    {
                                        int iValue = infos().effectCity(eLoopEffectCity).maaiImprovementYield[eImprovement, eLoopYield];
                                        if (eImprovementClass != ImprovementClassType.NONE)
                                        {
                                            iValue += infos().effectCity(eLoopEffectCity).maaiImprovementClassYield[eImprovementClass, eLoopYield];
                                        }
                                        if (iValue != 0)
                                        {
                                            if (bEncyclopedia || (pActivePlayer == null || pActivePlayer.canEverHaveEffectCity(eLoopEffectCity)))
                                            {
                                                yieldsList.AddItem(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                            }
                                        }
                                    }

                                    if (yieldsList.Count > 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_YIELDS_EFFECT_CITY_YIELD_RATE", buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer), yieldsList.Finalize(), TEXTVAR(false), TEXTVAR(!(infos().effectCity(eLoopEffectCity).mbSingle)));
                                    }
                                }
                            }

                            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < mInfos.effectCitiesNum(); eLoopEffectCity++)
                            {
                                if ((infos().effectCity(eLoopEffectCity).maiImprovementModifier.Count > 0) || (infos().effectCity(eLoopEffectCity).maiImprovementClassModifier.Count > 0))
                                {
                                    if ((infos().effectCity(eLoopEffectCity).meSourceLaw != LawType.NONE) ||
                                        (infos().effectCity(eLoopEffectCity).meSourceImprovement != ImprovementType.NONE) ||
                                        (infos().effectCity(eLoopEffectCity).meSourceImprovementClass != ImprovementClassType.NONE))
                                    {
                                        ImprovementType eSourceImprovement = infos().effectCity(eLoopEffectCity).meSourceImprovement;
                                        if (eSourceImprovement != ImprovementType.NONE && !infos().modSettings().App.CheckContentOwnership(infos().improvement(eSourceImprovement).meGameContentRequired, infos(), null))
                                        {
                                            continue;
                                        }
                                        int iValue = infos().effectCity(eLoopEffectCity).maiImprovementModifier[eImprovement];
                                        if (eImprovementClass != ImprovementClassType.NONE)
                                        {
                                            iValue += infos().effectCity(eLoopEffectCity).maiImprovementClassModifier[eImprovementClass];
                                        }
                                        if (iValue != 0)
                                        {
                                            if (bEncyclopedia || (pActivePlayer == null || pActivePlayer.canEverHaveEffectCity(eLoopEffectCity)))
                                            {
                                                subText.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_YIELDS_EFFECT_CITY_YIELD_MODIFIER", buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer), buildSignedTextVariable(iValue, true), TEXTVAR(!(infos().effectCity(eLoopEffectCity).mbSingle)));
                                            }
                                        }
                                    }
                                }
                            }

                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                if (eReligionPrereq != ReligionType.NONE)
                                {
                                    for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                                    {
                                        if (infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput.Count > 0)
                                        {
                                            CommaListVariableGenerator yieldsList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);

                                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                            {
                                                int iValue = infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput[eLoopTheology, eLoopYield];
                                                if (iValue != 0)
                                                {
                                                    yieldsList.AddItem(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                                }
                                            }

                                            if (yieldsList.Count > 0)
                                            {
                                                subText.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildTheologyLinkVariable(eLoopTheology, eReligionPrereq), yieldsList.Finalize());
                                            }
                                        }
                                        if (infos().improvementClass(eImprovementClass).maeTheologyCityEffect[eLoopTheology] != EffectCityType.NONE)
                                        {
                                            using (TextBuilder effectCityText = TextBuilder.GetTextBuilder(TextManager))
                                            {
                                                using (effectCityText.BeginScope(TextBuilder.ScopeType.COMMA))
                                                {
                                                    buildEffectCityHelp(effectCityText, infos().improvementClass(eImprovementClass).maeTheologyCityEffect[eLoopTheology], pGame, pPlayer, false, pActivePlayer);
                                                }
                                                subText.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildTheologyLinkVariable(eLoopTheology, eReligionPrereq), effectCityText.ToTextVariable());
                                            }
                                        }

                                    }
                                }
                            }

                            using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementType>())
                            {
                                HashSet<ImprovementType> seImprovementsAdded = hashSetScope.Value;
                                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                {
                                    if (!(seImprovementsAdded.Contains(eLoopImprovement)))
                                    {
                                        if (((pPlayer != null) ? pPlayer.isNationImprovement(eLoopImprovement) : true) && infos().improvement(eLoopImprovement).mbBuild)
                                        {
                                            int iValue = infos().improvement(eLoopImprovement).maiAdjacentImprovementModifier[eImprovement];
                                            if (iValue != 0)
                                            {
                                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_PER_ADJACENT_IMPROVEMENT_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                {
                                                    subText.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                                    seImprovementsAdded.Add(eLoopImprovement);

                                                    for (ImprovementType eOtherImprovement = 0; eOtherImprovement < infos().improvementsNum(); eOtherImprovement++)
                                                    {
                                                        if (eOtherImprovement != eLoopImprovement)
                                                        {
                                                            if (((pPlayer != null) ? pPlayer.isNationImprovement(eOtherImprovement) : true) && infos().improvement(eOtherImprovement).mbBuild)
                                                            {
                                                                if (infos().improvement(eOtherImprovement).maiAdjacentImprovementModifier[eImprovement] == iValue)
                                                                {
                                                                    subText.Add(buildImprovementLinkVariable(eOtherImprovement, pGame));
                                                                    seImprovementsAdded.Add(eOtherImprovement);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementType>())
                                {
                                    HashSet<ImprovementType> seImprovementsAdded = hashSetScope.Value;
                                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                    {
                                        if (!(seImprovementsAdded.Contains(eLoopImprovement)))
                                        {
                                            if (((pPlayer != null) ? pPlayer.isNationImprovement(eLoopImprovement) : true) && infos().improvement(eLoopImprovement).mbBuild)
                                            {
                                                int iValue = infos().improvement(eLoopImprovement).maiAdjacentImprovementClassModifier[eImprovementClass];
                                                if (iValue != 0)
                                                {
                                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_PER_ADJACENT_IMPROVEMENT_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                    {
                                                        subText.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                                        seImprovementsAdded.Add(eLoopImprovement);

                                                        for (ImprovementType eOtherImprovement = 0; eOtherImprovement < infos().improvementsNum(); eOtherImprovement++)
                                                        {
                                                            if (eOtherImprovement != eLoopImprovement)
                                                            {
                                                                if (((pPlayer != null) ? pPlayer.isNationImprovement(eOtherImprovement) : true) && infos().improvement(eOtherImprovement).mbBuild)
                                                                {
                                                                    if (infos().improvement(eOtherImprovement).maiAdjacentImprovementClassModifier[eImprovementClass] == iValue)
                                                                    {
                                                                        subText.Add(buildImprovementLinkVariable(eOtherImprovement, pGame));
                                                                        seImprovementsAdded.Add(eOtherImprovement);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementClassType>())
                                {
                                    HashSet<ImprovementClassType> seImprovementClassessAdded = hashSetScope.Value;
                                    for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                                    {
                                        if (!(seImprovementClassessAdded.Contains(eLoopImprovementClass)))
                                        {
                                            int iValue = infos().improvementClass(eLoopImprovementClass).maiAdjacentImprovementClassModifier[eImprovementClass];
                                            if (iValue != 0)
                                            {
                                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_PER_ADJACENT_IMPROVEMENT_CLASS_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                {
                                                    subText.Add(buildImprovementClassLinkVariable(eLoopImprovementClass));
                                                    seImprovementClassessAdded.Add(eLoopImprovementClass);

                                                    for (ImprovementClassType eOtherImprovementClass = 0; eOtherImprovementClass < infos().improvementClassesNum(); eOtherImprovementClass++)
                                                    {
                                                        if (eOtherImprovementClass != eLoopImprovementClass)
                                                        {
                                                            if (infos().improvementClass(eOtherImprovementClass).maiAdjacentImprovementClassModifier[eImprovementClass] == iValue)
                                                            {
                                                                subText.Add(buildImprovementClassLinkVariable(eOtherImprovementClass));
                                                                seImprovementClassessAdded.Add(eOtherImprovementClass);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (infos().improvementClass(eImprovementClass).maeResourceCityEffect.Count > 0)
                                {
                                    if (((pPlayer != null) ? pPlayer.isNationImprovement(eImprovement) : true))
                                    {
                                        using (TextBuilder effectCityBuilder = TextBuilder.GetTextBuilder(TextManager))
                                        {
                                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                            {
                                                for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < infos().effectCitiesNum(); eLoopEffectCity++)
                                                {
                                                    if (infos().improvementClass(eImprovementClass).maeResourceCityEffect[eLoopResource] == eLoopEffectCity)
                                                    {
                                                        for (EffectCityType eOtherEffectCity = 0; eOtherEffectCity < infos().effectCitiesNum(); eOtherEffectCity++)
                                                        {
                                                            EffectCityType eEffectCity = infos().effectCity(eOtherEffectCity).maeEffectCityEffectCity[eLoopEffectCity];
                                                            if (eEffectCity != EffectCityType.NONE)
                                                            {
                                                                effectCityBuilder.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_NO_YIELDS_EFFECT_CITY_EFFECT_CITY", buildEffectCityLinkVariable(eLoopEffectCity, null, null), buildEffectCityLinkVariable(eEffectCity, null, null));
                                                                subText.Add(buildColonSpaceOne(buildEffectCityLinkVariable(eOtherEffectCity, null, null), effectCityBuilder.ToTextVariable()));
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (subText.HasContent)
                    {
                        if (bDetails)
                        {
                            buildDividerText(builder);
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_POTENTIAL_BONUSES");
                        }

                        builder.Add(subText.ToTextVariable());
                    }
                }


                {
                    TextType eDescription = infos().improvement(eImprovement).meDescription;

                    if (eDescription != TextType.NONE)
                    {
                        builder.AddTEXT(eDescription);
                    }
                }

                using (TextBuilder extraText = TextBuilder.GetTextBuilder(TextManager))
                {
                    extraText.AddTEXT(infos().improvement(eImprovement).mExtraHelp);

                    if (extraText.HasContent)
                    {
                        buildDividerText(builder);
                        builder.Add(extraText.ToTextVariable());
                    }
                }

                return builder;
            }
        }
        //1k lines of copy-paste END

        //lines 21309-21715
        public override void buildImprovementRequiresHelp(List<TextVariable> lRequirements, ImprovementType eImprovement, Game pGame, Player pActivePlayer, Tile pTile, bool bUpgradeImprovement = false)
        {
            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            {
                //here goes the copy-pasting
                //using (new UnityProfileScope("HelpText.buildImprovementRequiresHelp"))
                {
                    ImprovementClassType eImprovementClass = pImprovementInfo.meClass;

                    ReligionType eReligionPrereq = pImprovementInfo.meReligionPrereq;

                    BetterAICity pCityTerritory = ((pTile != null) ? (BetterAICity)pTile.cityTerritory() : null);


                    //if (eImprovementClass != ImprovementClassType.NONE)
                    //{
                    //    TechType ePrereqTech = infos().improvementClass(eImprovementClass).meTechPrereq;

                    //    if (ePrereqTech != TechType.NONE)
                    //    {
                    //        if (pActivePlayer == null || !(pActivePlayer.isTechAcquired(ePrereqTech)))
                    //        {
                    //            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(ePrereqTech)), pActivePlayer != null));
                    //        }
                    //    }
                    //}
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
                    {
                        bool bHasPrimaryUnlock = false;
                        CommaListVariableGenerator andList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);
                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            TechType ePrereqTech = infos().improvementClass(eImprovementClass).meTechPrereq;

                            if (ePrereqTech != TechType.NONE)
                            {
                                //andList.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(ePrereqTech)), pActivePlayer != null));
                                andList.AddItem(buildTechLinkVariable(ePrereqTech));
                                bHasPrimaryUnlock = true;
                            }
                        }

                        CultureType eCulturePrereq = pImprovementInfo.meCulturePrereq;

                        if (eCulturePrereq != CultureType.NONE)
                        {
                            //andList.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(eCulturePrereq, pCityTerritory)), ((pCityTerritory != null) ? (pCityTerritory.getCulture() < eCulturePrereq) : false)));
                            andList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(eCulturePrereq, pCityTerritory)));
                            bHasPrimaryUnlock = true;
                        }
                        if (bHasPrimaryUnlock)
                        {
                            if (pImprovementInfo.isAnySecondaryPrereq() || pImprovementInfo.isAnyTertiaryPrereq())
                            {
                                CommaListVariableGenerator orLineBreakList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR_LINEBREAK, CommaListVariableGenerator.EncloseType.PARENTHESIS, TextManager);
                                orLineBreakList.AddItem(andList.Finalize());
                                if (pImprovementInfo.isAnySecondaryPrereq())
                                {
                                    CommaListVariableGenerator andList2 = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);
                                    
                                    if (pImprovementInfo.meSecondaryUnlockTechPrereq != TechType.NONE)
                                    {
                                        //andList2.AddItem(buildWarningTextVariable(buildTechLinkVariable(pImprovementInfo.meSecondaryUnlockTechPrereq), pActivePlayer != null));
                                        andList2.AddItem(buildTechLinkVariable(pImprovementInfo.meSecondaryUnlockTechPrereq));
                                    }
                                    if (pImprovementInfo.meSecondaryUnlockCulturePrereq != CultureType.NONE)
                                    {
                                        //andList2.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(pImprovementInfo.meSecondaryUnlockCulturePrereq, pCityTerritory)), ((pCityTerritory != null) ? (pCityTerritory.getCulture() < pImprovementInfo.meSecondaryUnlockCulturePrereq) : false)));
                                        andList2.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(pImprovementInfo.meSecondaryUnlockCulturePrereq, pCityTerritory)));
                                    }
                                    if (pImprovementInfo.miSecondaryUnlockPopulationPrereq > 0)
                                    {
                                        TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildConceptLinkVariable("CONCEPT_POPULATION"), TEXTVAR_TYPE("TEXT_HELPTEXT_MIN", pImprovementInfo.miSecondaryUnlockPopulationPrereq));
                                        if (pCityTerritory != null)
                                        {
                                            //reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pCityTerritory.getPopulation()) , TEXTVAR(pImprovementInfo.miSecondaryUnlockPopulationPrereq));
                                            //andList2.AddItem(buildWarningTextVariable(reqItem, (pCityTerritory.getPopulation() < pImprovementInfo.miSecondaryUnlockPopulationPrereq)));
                                            andList2.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pCityTerritory.getPopulation()), TEXTVAR(pImprovementInfo.miSecondaryUnlockPopulationPrereq)));
                                        }
                                        else
                                        {
                                            //andList2.AddItem(buildWarningTextVariable(reqItem, false));
                                            andList2.AddItem(reqItem);
                                        }
                                    }
                                    if (pImprovementInfo.meSecondaryUnlockEffectCityPrereq != EffectCityType.NONE)
                                    {
                                        //andList2.AddItem(buildWarningTextVariable(buildEffectCitySourceLinkVariable(pImprovementInfo.meSecondaryUnlockEffectCityPrereq, pCityTerritory, pGame, pActivePlayer), ((pCityTerritory != null) ? (pCityTerritory.getEffectCityCount(pImprovementInfo.meSecondaryUnlockEffectCityPrereq) == 0) : false)));
                                        andList2.AddItem(buildEffectCitySourceLinkVariable(pImprovementInfo.meSecondaryUnlockEffectCityPrereq, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer));
                                    }
                                    orLineBreakList.AddItem(andList2.Finalize());
                                }

                                if (pImprovementInfo.isAnyTertiaryPrereq())
                                {
                                    CommaListVariableGenerator andList3 = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);

                                    if (pImprovementInfo.meTertiaryUnlockFamilyClassPrereq != FamilyClassType.NONE)
                                    {
                                        TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildConceptLinkVariable("CONCEPT_FAMILY_CLASS"), TEXTVAR(infos().familyClass(pImprovementInfo.meTertiaryUnlockFamilyClassPrereq).meName, TextManager));
                                        //reqItem = buildWarningTextVariable(reqItem, ((pCityTerritory != null) ? (pCityTerritory.getFamilyClass() != pImprovementInfo.meTertiaryUnlockFamilyClassPrereq) : false));
                                        if (pImprovementInfo.mbTertiaryUnlockSeatOnly)
                                        {
                                            //reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", reqItem, buildWarningTextVariable(buildConceptLinkVariable("CONCEPT_FAMILY_SEAT"), ((pCityTerritory != null) ? pCityTerritory.isFamilySeat() : false)));
                                            reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", reqItem, buildConceptLinkVariable("CONCEPT_FAMILY_SEAT"));
                                        }
                                        andList3.AddItem(reqItem);
                                    }

                                    if (pImprovementInfo.meTertiaryUnlockTechPrereq != TechType.NONE)
                                    {
                                        //andList3.AddItem(buildWarningTextVariable(buildTechLinkVariable(pImprovementInfo.meTertiaryUnlockTechPrereq), pActivePlayer != null));
                                        andList3.AddItem(buildTechLinkVariable(pImprovementInfo.meTertiaryUnlockTechPrereq));
                                    }

                                    if (pImprovementInfo.meTertiaryUnlockCulturePrereq != CultureType.NONE)
                                    {
                                        //andList3.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(pImprovementInfo.meTertiaryUnlockCulturePrereq, pCityTerritory)), ((pCityTerritory != null) ? (pCityTerritory.getCulture() < pImprovementInfo.meTertiaryUnlockCulturePrereq) : false)));
                                        andList3.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(pImprovementInfo.meTertiaryUnlockCulturePrereq, pCityTerritory)));
                                    }

                                    if (pImprovementInfo.meTertiaryUnlockEffectCityPrereq != EffectCityType.NONE)
                                    {
                                        //andList3.AddItem(buildWarningTextVariable(buildEffectCitySourceLinkVariable(pImprovementInfo.meTertiaryUnlockEffectCityPrereq, pCityTerritory, pGame, pActivePlayer), ((pCityTerritory != null) ? (pCityTerritory.getEffectCityCount(pImprovementInfo.meTertiaryUnlockEffectCityPrereq) == 0) : false)));
                                        andList3.AddItem(buildEffectCitySourceLinkVariable(pImprovementInfo.meTertiaryUnlockEffectCityPrereq, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer));
                                    }

                                    orLineBreakList.AddItem(andList3.Finalize());
                                }
                                //lRequirements.Add(orLineBreakList.Finalize());
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", orLineBreakList.Finalize()), ((pCityTerritory != null) ? !(pCityTerritory.isImprovementUnlockedInCity(eImprovement)) : false)));

                            }
                            else
                            {
                                //lRequirements.Add(andList.Finalize());
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", andList.Finalize()), ((pCityTerritory != null) ? !(pCityTerritory.isImprovementUnlockedInCity(eImprovement)) : false)));
                            }
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

                    //mbHolyCity is for specific religions, so it was moved down to religion check in version 1.0.62422
                    //for any Holy City: mbHolyCityValid
                    //if (pImprovementInfo.mbHolyCity)
                    //{
                    //    if (eReligionPrereq != ReligionType.NONE)
                    //    {
                    //        if (pTile != null)
                    //        {
                    //            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildHolyCityLinkVariable(pCityTerritory)), ((pCityTerritory != null) && !(pCityTerritory.isReligionHolyCity(eReligionPrereq)))));
                    //        }
                    //        else
                    //        {
                    //            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_HOLY_CITY", buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer)), ((pCityTerritory != null) && !(pCityTerritory.isReligionHolyCityAny()))));
                    //        }
                    //    }
                    //}

                    {
                        CommaListVariableGenerator orList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR, TextManager);

                        if (pImprovementInfo.mbFreshWaterValid)
                        {
                            orList.AddItem(buildFreshWaterLinkVariable(pTile));
                        }

                        if (pImprovementInfo.mbRiverValid)
                        {
                            orList.AddItem(buildRiverLinkVariable(pTile));
                        }

                        if (pImprovementInfo.mbCoastLandValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_COAST_LAND"));
                        }

                        if (pImprovementInfo.mbCoastWaterValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_COAST_WATER"));
                        }

                        if (pImprovementInfo.mbCityValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_CITY"));
                        }

                        if (pImprovementInfo.mbHolyCityValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_HOLY_CITY_ANY"));
                        }

                        for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                        {
                            if (pImprovementInfo.mabTerrainValid[(int)eLoopTerrain])
                            {
                                orList.AddItem(buildTerrainLinkVariable(eLoopTerrain));
                            }
                        }

                        for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                        {
                            if (pImprovementInfo.mabHeightValid[(int)eLoopHeight])
                            {
                                orList.AddItem(buildHeightLinkVariable(eLoopHeight));
                            }
                        }

                        for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                        {
                            if (pImprovementInfo.mabHeightAdjacentValid[(int)eLoopHeight])
                            {
                                orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_HEIGHT", buildHeightLinkVariable(eLoopHeight)));
                            }
                        }

                        for (VegetationType eLoopVegetation = 0; eLoopVegetation < infos().vegetationNum(); eLoopVegetation++)
                        {
                            if (pImprovementInfo.mabVegetationValid[eLoopVegetation])
                            {
                                orList.AddItem(buildVegetationLinkVariable(eLoopVegetation));
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                if (infos().improvementClass(eImprovementClass).mabResourceValid[(int)eLoopResource])
                                {
                                    orList.AddItem(buildResourceLinkVariable(eLoopResource, pTile));
                                }
                            }
                        }

                        for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < mInfos.effectCitiesNum(); eLoopEffectCity++)
                        {
                            foreach ((TerrainType, ImprovementType) pLoopPair in infos().effectCity(eLoopEffectCity).mlpTerrainImprovementValid)
                            {
                                if (pLoopPair.Item2 == eImprovement)
                                {
                                    orList.AddItem(buildSlashText(buildTerrainLinkVariable(pLoopPair.Item1, pTile), buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor())));
                                }
                            }
                        }

                        if (orList.Count > 0)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", orList.Finalize()), ((pTile != null) && !(pTile.isImprovementValid(eImprovement, pCityTerritory)))));
                        }
                    }

                    {
                        int iRequiresLaws = pImprovementInfo.miPrereqLaws;

                        if (iRequiresLaws > 0)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_LAWS", iRequiresLaws), ((pActivePlayer != null) ? (pActivePlayer.countActiveLaws() < iRequiresLaws) : false)));
                        }
                    }

                    if (infos().improvement(eImprovement).mbNoAdjacentReligion)
                    {
                        lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT_RELIGION"));
                    }

                    if (eReligionPrereq != ReligionType.NONE)
                    {
                        if (infos().improvement(eImprovement).mbHolyCity)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_HOLY_CITY", buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer)), ((pCityTerritory != null) && !(pCityTerritory.isReligionHolyCity(eReligionPrereq)))));
                        }
                        else
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_RELIGION", buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer)), ((pCityTerritory != null) ? !(pCityTerritory.isReligion(eReligionPrereq)) : false)));
                        }
                    }

                    //culture is now part of new code, with tech
                    //{
                    //    CultureType eCulturePrereq = pImprovementInfo.meCulturePrereq;

                    //    if (eCulturePrereq != CultureType.NONE)
                    //    {
                    //        lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CULTURE", buildCultureLinkVariable(eCulturePrereq, pCityTerritory)), ((pCityTerritory != null) ? (pCityTerritory.getCulture() < eCulturePrereq) : false)));
                    //    }
                    //}

                    {
                        FamilyType eFamilyPrereq = infos().improvement(eImprovement).meFamilyPrereq;

                        if (eFamilyPrereq != FamilyType.NONE)
                        {
                            if (pGame != null)
                            {
                                if (pGame.isCharacters())
                                {
                                    lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_FAMILY", buildFamilyLinkVariable(eFamilyPrereq, pGame)), (pCityTerritory != null) && (pCityTerritory.getFamily() != eFamilyPrereq)));
                                }
                            }
                            else
                            {
                                lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_FAMILY", buildFamilyLinkVariable(eFamilyPrereq, pGame)));
                            }
                        }
                    }

                    {
                        ImprovementType eImprovementPrereq = pImprovementInfo.meImprovementPrereq;

                        if (eImprovementPrereq != ImprovementType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_IMPROVEMENT", buildImprovementLinkVariable(eImprovementPrereq, pGame, pTile)), ((pCityTerritory != null) ? (pCityTerritory.getFinishedImprovementCount(eImprovementPrereq) == 0) : false)));
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementType eAdjacentImprovementPrereq = pImprovementInfo.meAdjacentImprovementPrereq;

                        if (eAdjacentImprovementPrereq != ImprovementType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_IMPROVEMENT", buildImprovementLinkVariable(eAdjacentImprovementPrereq, pGame)), ((pTile != null) ? !pTile.adjacentToImprovementFinished(eAdjacentImprovementPrereq) : false)));
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementClassType eAdjacentImprovementClassPrereq = pImprovementInfo.meAdjacentImprovementClassPrereq;

                        if (eAdjacentImprovementClassPrereq != ImprovementClassType.NONE)
                        {
                            //this is already fixed by giving classes an actual name instead of sometimes using the lvl1 improvement name for the entire improvement class
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_IMPROVEMENT", buildImprovementClassLinkVariable(eAdjacentImprovementClassPrereq)), ((pTile != null) ? !pTile.adjacentToImprovementClassFinished(eAdjacentImprovementClassPrereq) : false)));
                        }
                    }

                    {
                        EffectCityType eEffectCityPrereq = pImprovementInfo.meEffectCityPrereq;

                        if (eEffectCityPrereq != EffectCityType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildEffectCitySourceLinkVariable(eEffectCityPrereq, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer)), ((pCityTerritory != null) ? (pCityTerritory.getEffectCityCount(eEffectCityPrereq) == 0) : false)));
                        }
                    }

                    {
                        if (pImprovementInfo.maeEffectCityAnyPrereq.Count > 0)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES")))
                                {
                                    foreach (EffectCityType eLoopEffectCity in pImprovementInfo.maeEffectCityAnyPrereq)
                                    {
                                        subText.Add(buildWarningTextVariable(buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer)), ((pCityTerritory != null) ? (pCityTerritory.getEffectCityCount(eLoopEffectCity) == 0) : false));
                                    }
                                }

                                lRequirements.Add(subText.ToTextVariable());
                            }
                        }
                    }

/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
                    {
                        CityBiomeType eCityBiomeType = pImprovementInfo.meCityBiomePrereq;
                        if (eCityBiomeType != CityBiomeType.NONE)
                        {
                            TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildConceptLinkVariable("CONCEPT_CITY_BIOME"), TEXTVAR(((BetterAIInfos)infos()).cityBiome(eCityBiomeType).mName, TextManager));
                            if (pCityTerritory != null)
                            {

                                reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", reqItem, TEXTVAR(((BetterAIInfos)infos()).cityBiome(pCityTerritory.getCityBiome()).mName, TextManager));
                            }
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", reqItem), ((pCityTerritory != null) ? (pCityTerritory.getCityBiome() != eCityBiomeType) : false)));
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/


                    for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                    {
                        if (!infos().terrain(eLoopTerrain).mbWater) // don't clutter the help text with obvious requirements
                        {
                            if (pImprovementInfo.mabTerrainInvalid[(int)eLoopTerrain] && (pTile == null || pTile.getTerrain() == eLoopTerrain))
                            {
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_TERRAIN", buildTerrainLinkVariable(eLoopTerrain))));
                            }
                        }
                    }

                    for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                    {
                        if (pImprovementInfo.mabHeightInvalid[(int)eLoopHeight] && (pTile == null || pTile.getHeight() == eLoopHeight))
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_TERRAIN", buildHeightLinkVariable(eLoopHeight))));
                        }
                    }

                    bool bAdjacencyValid = false;
                    if (pTile != null)
                    {
                        if (pImprovementInfo.mbTerritoryOnly)
                        {
                            if (pActivePlayer != null)
                            {
                                if (pActivePlayer.getTeam() != pTile.getTeam())
                                {
                                    lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_TEAM")));
                                }
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            if (infos().improvementClass(eImprovementClass).mbAdjacentValid)
                            {
                                if (pTile.adjacentToImprovementClassFinished(eImprovementClass))
                                {
                                    bAdjacencyValid = true;
                                    lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENTCLASS_ADJACENCY_VALID", buildImprovementClassLinkVariable(eImprovementClass)));
                                }

                            }

                            if (infos().improvementClass(eImprovementClass).mbNoAdjacent)
                            {
                                TextVariable textvar = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT", buildImprovementClassLinkVariable(eImprovementClass));
                                if (!bAdjacencyValid && !(pTile.notAdjacentToImprovementClass(eImprovementClass)))
                                {
                                    lRequirements.Add(buildWarningTextVariable(textvar));
                                }
                                else
                                {
                                    lRequirements.Add(textvar);
                                }
                            }
                        }

                        if (eReligionPrereq != ReligionType.NONE)
                        {
                            if (pImprovementInfo.mbNoAdjacentReligion)
                            {
                                TextVariable textvar = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT_RELIGION");
                                if (!bAdjacencyValid && pTile.adjacentToOtherImprovementReligion(eReligionPrereq))
                                {
                                    lRequirements.Add(buildWarningTextVariable(textvar));
                                }
                                else
                                {
                                    lRequirements.Add(textvar);
                                }
                            }
                        }
                    }

                    if (pImprovementInfo.mbRequiresBorder)
                    {
                        TextVariable textvar = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_REQUIRES_BORDER");
                        if (!bAdjacencyValid && !pTile.isBorder())
                        {
                            lRequirements.Add(buildWarningTextVariable(textvar));
                        }
                        else
                        {
                            lRequirements.Add(textvar);
                        }
                    }

                    if (pImprovementInfo.mbRequiresUrban)
                    {
                        TextVariable textvar = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_URBAN_BUILDING", buildUrbanLinkVariable());
                        if (!bAdjacencyValid && pTile != null && !pTile.urbanEligible())
                        {
                            lRequirements.Add(buildWarningTextVariable(textvar));
                        }
                        else
                        {
                            lRequirements.Add(textvar);
                        }
                    }

                    {
                        CommaListVariableGenerator req = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);

                        {
                            int iCount = ((pCityTerritory != null) ? pCityTerritory.getImprovementCount(eImprovement) : 0);
                            int iValue = pImprovementInfo.miMaxCityCount;

                            if (iValue > 0)
                            {
                                TextVariable reqItem = buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CITY_IMPROVEMENT_COUNT", TEXTVAR(iValue), buildImprovementLinkVariable(eImprovement, pGame, pTile)), (iCount >= iValue));

                                if (pCityTerritory != null)
                                {
                                    reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(iCount), TEXTVAR(iValue));
                                }

                                req.AddItem(reqItem);
                            }
                        }

                        {
                            int iValue = pImprovementInfo.miMaxFamilyCount;
                            if (iValue > 0)
                            {
                                TextVariable reqItem = buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_FAMILY_IMPROVEMENT_COUNT", TEXTVAR(iValue)), ((pCityTerritory != null) && (iValue <= pGame.countFamilyImprovements(pCityTerritory.getFamily(), eImprovement))));

                                if (pCityTerritory != null)
                                {
                                    if (pCityTerritory.hasFamily())
                                    {
                                        reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pGame.countFamilyImprovements(pCityTerritory.getFamily(), eImprovement)), TEXTVAR(iValue));
                                    }
                                }

                                req.AddItem(reqItem);
                            }
                        }

                        {
                            int iValue = pImprovementInfo.miMaxPlayerCount;
                            if (iValue > 0)
                            {
                                if ((eImprovementClass != ImprovementClassType.NONE) ? ((pCityTerritory == null) || !(pCityTerritory.isNoImprovementClassMaxUnlock(eImprovementClass))) : true)
                                {
                                    TextVariable reqItem = buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NATION_IMPROVEMENT_COUNT", TEXTVAR(iValue)), ((pActivePlayer != null) && (iValue <= pActivePlayer.getImprovementCount(eImprovement))));

                                    if (pActivePlayer != null)
                                    {
                                        reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pActivePlayer.getImprovementCount(eImprovement)), TEXTVAR(iValue));
                                    }

                                    req.AddItem(reqItem);
                                }
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            int iValue = infos().improvementClass(eImprovementClass).miMaxCultureCount;
                            if (iValue > 0)
                            {
                                if (pCityTerritory != null)
                                {
                                    TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CULTURE_IMPROVEMENT_COUNT", TEXTVAR(iValue), buildImprovementClassLinkVariable(eImprovementClass), buildCultureLevelLinkVariable(pCityTerritory));
                                    int maxImprovements = (int)pCityTerritory.getCulture() + (int)pCityTerritory.getCultureStep() + 1;
                                    reqItem = buildWarningTextVariable(reqItem, ((pCityTerritory != null) && maxImprovements <= pCityTerritory.getImprovementClassCount(eImprovementClass)));

                                    if (pCityTerritory != null)
                                    {
                                        reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pCityTerritory.getImprovementClassCount(eImprovementClass)), TEXTVAR(maxImprovements));
                                    }

                                    req.AddItem(reqItem);
                                }
                            }

                            iValue = infos().improvementClass(eImprovementClass).miMaxCityCount;
                            if (iValue > 0 && pCityTerritory != null)
                            {
                                if (!pCityTerritory.isNoImprovementClassMaxUnlock(eImprovementClass))
                                {
                                    //TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CITY_IMPROVEMENT_COUNT", TEXTVAR(iValue), buildImprovementClassLinkVariable(eImprovementClass));
                                    TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CITY_IMPROVEMENT_CLASS_COUNT", TEXTVAR(iValue), buildImprovementClassLinkVariable(eImprovementClass));
                                    reqItem = buildWarningTextVariable(reqItem, ((pCityTerritory != null) && iValue <= pCityTerritory.getImprovementClassCount(eImprovementClass)));

                                    if (pCityTerritory != null)
                                    {
                                        reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pCityTerritory.getImprovementClassCount(eImprovementClass)), TEXTVAR(iValue));
                                    }

                                    req.AddItem(reqItem);
                                }
                            }

                            if (infos().improvementClass(eImprovementClass).mbContiguous)
                            {
                                lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENTCLASS_CONTIGUOUS", buildImprovementClassLinkVariable(eImprovementClass)));
                            }

                            if (!bAdjacencyValid && infos().improvementClass(eImprovementClass).mbAdjacentValid)
                            {
                                lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENTCLASS_ADJACENCY_VALID", buildImprovementClassLinkVariable(eImprovementClass)));
                            }


                        }

                        if (req.Count > 0)
                        {
                            lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_MAX_LIST", req.Finalize()));
                        }
                    }

                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        if (pCityTerritory != null)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    foreach (EffectCityType eLoopEffectCity in infos().improvementClass(eImprovementClass).maeEffectCityDisabled)
                                    {
                                        if (pCityTerritory.getEffectCityCount(eLoopEffectCity) > 0)
                                        {
                                            subText.Add(buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory.governor()));
                                        }
                                    }
                                }

                                if (subText.HasContent)
                                {
                                    lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_DISABLED_BY", subText.ToTextVariable())));
                                }
                            }
                        }
                    }
                }
                //end of copy-paste region
            }
        }

        //copy-paste START
        //lines 21717-22072
        public override TextBuilder buildUnitTypeHelp(TextBuilder builder, UnitType eUnit, City pCity, Player pPlayer, TribeType eTribe, Game pGame, Player pActivePlayer, bool bName = true, bool bCosts = true, bool bStats = true, bool bDetails = true)
        {
            //using (new UnityProfileScope("HelpText.buildUnitTypeHelp"))
            {
                if (bName)
                {
                    builder.Add(buildTitleVariable(buildUnitNameVariable(eUnit, pGame)));
                }

                buildUnitTraitsHelp(builder, eUnit);

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
                        ReligionType eReligion = infos().unit(eUnit).meRequiresReligion;

                        if (eReligion != ReligionType.NONE)
                        {
                            foreach (UnitTraitType eLoopUnitTrait in infos().unit(eUnit).maeUnitTrait)
                            {
                                if (infos().effectUnit(infos().unitTrait(eLoopUnitTrait).meEffectUnit).mbSpreadReligion)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_SPREADS_RELIGION", buildReligionLinkVariable(eReligion, pGame, null));
                                    break;
                                }
                            }
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

                    if (infos().unit(eUnit).mbHarvest)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_HARVEST_RESOURCE", buildHarvestResourceLinkVariable());
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
                                        improvementsScope.Value.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
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

                    if (infos().unit(eUnit).mbTestudo)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_TESTUDO");
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
                                if (infos().unit(eUnit).mbPromote)
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

                                    using (var hashSetScope = CollectionCache.GetHashSetScoped<PromotionType>())
                                    {
                                        HashSet<PromotionType> sePromotions = hashSetScope.Value;

                                        foreach (EffectCityType eLoopEffectCity in pCity.getActiveEffectCity())
                                        {
                                            foreach (PromotionType eLoopPromotion in infos().effectCity(eLoopEffectCity).maeFreePromotion)
                                            {
                                                if (pGame.isEffectUnitValid(eUnit, infos().promotion(eLoopPromotion).meEffectUnit))
                                                {
                                                    sePromotions.Add(eLoopPromotion);
                                                }
                                            }

                                            foreach (UnitTraitType eLoopUnitTrait in infos().unit(eUnit).maeUnitTrait)
                                            {
                                                PromotionType ePromotion = infos().effectCity(eLoopEffectCity).maeTraitPromotion[eLoopUnitTrait];

                                                if (ePromotion != PromotionType.NONE)
                                                {
                                                    sePromotions.Add(ePromotion);
                                                }
                                            }
                                        }

                                        foreach (PromotionType eLoopPromotion in sePromotions)
                                        {
                                            builder.Add(buildPromotionLinkVariable(eLoopPromotion));
                                        }
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

                    if (infos().Helpers.canDamage(eUnit) && !(infos().unit(eUnit).mbPromote))
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
                                builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_POPULATION_COST", TEXTVAR(iCost), buildCitizenLinkVariable(null));
                            }
                        }
                    }

                    // yields
                    {
                        TextVariable production = null;

                        if (pPlayer != null)
                        {
                            using (var costsScoped = CollectionCache.GetDictionaryScoped<YieldType, int>())
                            {
                                pPlayer.getUnitYieldCost(eUnit, pCity, costsScoped.Value);
                                buildYieldCostText(builder, costsScoped.Value, ((pPlayer == pActivePlayer) ? pPlayer : null));
                            }

                            int iProduction = pPlayer.getUnitBuildCost(eUnit, pCity);
                            if (iProduction > 0)
                            {
                                production = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_PRODUCTION", TEXTVAR((pPlayer?.getUnitBuildCost(eUnit, pCity) ?? iProduction)), buildYieldIconLinkVariable(infos().unit(eUnit).meProductionType));
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
                                    builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
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
        //copy-paste END

        //copy-paste START
        //lines 22242-22506
        public override void buildUnitTooltip(Unit pUnit, ClientManager pManager, UIUnitTooltipData outUnitData)
        {
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

                if (pUnit.getTestudoBonus() > 0)
                {
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_TESTUDO_LABEL"), TEXTVAR("TEXT_HELPTEXT_EFFECT_UNIT_HELP_VS_UNIT_TRAIT", TextManager, buildUnitTraitLinkVariable(infos().Globals.RANGED_TRAIT), buildDefenseValueLinkVariable(pUnit.getTestudoBonus(), true), TEXTVAR(true)));
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
                            if (pUnit.hasZOC())
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_HAS_ZOC", buildZOCLinkVariable());
                            }

                            if (pUnit.hasIgnoreZOC(null))
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

                            outUnitData.AddStat(TextManager, buildEffectUnitClassLinkVariable(eLoopEffectUnitClass, pUnit), builder.ToTextVariable());
                        }
                    }
                }

                foreach (EffectUnitType eLoopEffect in pUnit.getEffectUnits())
                {
                    if (infos().effectUnit(eLoopEffect).meClass == EffectUnitClassType.NONE)
                    {
                        if (!(pUnit.isGeneralEffectUnit(eLoopEffect)))
                        {
                            if (!pUnit.isEffectUnitSource(eLoopEffect, SourceEffectUnitType.UNIT))
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                    {
                                        buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                    }

                                    outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(eLoopEffect, pUnit), builder.ToTextVariable());
                                }
                            }
                            else if (bExpand)
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                    outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(eLoopEffect, pUnit), builder.ToTextVariable());
                                }
                            }
                        }
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
                                    outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(eLoopEffect, pUnit), builder.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                if (pUnit.info().mbPromote)
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
                        int iValue = pUnit.attackValue(eLoopAttack);
                        if (iValue > 0)
                        {
                            TextVariable valueOrFalse = iValue > 1 ? TEXTVAR(iValue) : TEXTVAR(false);

                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_ATTACK_LABEL"), TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_ATTACK_VALUE_ITEM", buildAttackLinkVariable(eLoopAttack), valueOrFalse, TEXTVAR(pUnit.attackPercent(eLoopAttack))));
                        }
                    }

                    if (pUnit.getTurnSteps() > 0)
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
        //copy-paste END

        //lines 23001-23650
        public override TextBuilder buildEffectUnitHelp(TextBuilder builder, EffectUnitType eEffectUnit, Game pGame, bool bSkipIcons = false, bool bRightJustify = false, bool bIncludeIndirect = true)
        {
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

        //lines 27316-28221
        public override TextBuilder buildEffectPlayerHelp(TextBuilder builder, EffectPlayerType eEffectPlayer, Game pGame, Player pPlayer, Player pActivePlayer, ReligionType eStateReligion = ReligionType.NONE, bool bAllCities = false, TextBuilder.ScopeType effectCityScopeType = TextBuilder.ScopeType.COMMA)
        {
            
            using (var ignoreEffectPlayerListScoped = CollectionCache.GetListScoped<EffectPlayerType>())
            {
                List<EffectPlayerType> effectPlayerIgnore = ignoreEffectPlayerListScoped.Value;
                return buildEffectPlayerHelp(builder, eEffectPlayer, pGame, pPlayer, pActivePlayer, ref effectPlayerIgnore, eStateReligion, bAllCities, effectCityScopeType);
            }
        }

        public virtual TextBuilder buildEffectPlayerHelp(TextBuilder builder, EffectPlayerType eEffectPlayer, Game pGame, Player pPlayer, Player pActivePlayer, ref List<EffectPlayerType> effectPlayerIgnore, ReligionType eStateReligion = ReligionType.NONE, bool bAllCities = false, TextBuilder.ScopeType effectCityScopeType = TextBuilder.ScopeType.COMMA)

        {
            //using (new UnityProfileScope("HelpText.buildEffectPlayerHelp"))

            BetterAIInfoEffectPlayer pInfoEffectPlayer = (BetterAIInfoEffectPlayer)infos().effectPlayer(eEffectPlayer);
            {
                City pCapitalCity = pPlayer?.capitalCity();

                {
                    EffectPlayerType eEffectPlayerUnlock = pInfoEffectPlayer.meEffectPlayer;

                    if (eEffectPlayerUnlock != EffectPlayerType.NONE && !effectPlayerIgnore.Contains(eEffectPlayerUnlock))
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_UNLOCKS_EFFECT", buildEffectPlayerLinkVariable(eEffectPlayerUnlock));
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

                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_START_BONUS", subText.ToTextVariable());
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

                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_FOUND_BONUS", subText.ToTextVariable());
                        }
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

                if (pInfoEffectPlayer.mbRemoveAllVegetation)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CAN_REMOVE_VEGETATION_ALL");
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
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_OUTSIDE_UNIT_CONSUMPTION", buildUnitConsumptionOutsideLinkVariable());
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
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_LEGITIMACY_ORDERS", buildLegitimacyLinkVariable());
                }

                if (pInfoEffectPlayer.mbOrdersScience)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_ORDERS_SCIENCE");
                }

                if (pInfoEffectPlayer.mbRecruitMercenaries)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_RECRUIT_MERCENARIES", buildLegitimacyLinkVariable());
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
                    }

                    foreach ((DiplomacyType eDiplomacy, YieldType eYield, int iAmount) pLoopTriple in pInfoEffectPlayer.mlpTeamDiplomacyYields)
                    {
                        if (pLoopTriple.eYield == eLoopYield)
                        {
                            if (pLoopTriple.iAmount != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_DIPLOMACY_YIELD", buildYieldValueIconLinkVariable(eLoopYield, pLoopTriple.iAmount, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame), buildDiplomacyLinkVariable(pLoopTriple.eDiplomacy, false));
                            }
                        }
                    }

                    foreach ((DiplomacyType eDiplomacy, YieldType eYield, int iAmount) pLoopTriple in pInfoEffectPlayer.mlpTribeDiplomacyYields)
                    {
                        if (pLoopTriple.eYield == eLoopYield)
                        {
                            if (pLoopTriple.iAmount != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TRIBE_DIPLOMACY_YIELD", buildYieldValueIconLinkVariable(eLoopYield, pLoopTriple.iAmount, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame), buildDiplomacyLinkVariable(pLoopTriple.eDiplomacy, true));
                            }
                        }
                    }
                }

                {
                    CommaListVariableGenerator yieldsList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);

                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                    {
                        int iValue = pInfoEffectPlayer.maiYieldRate[eLoopYield];
                        if (iValue != 0)
                        {
                            yieldsList.AddItem(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                        }
                    }

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
                    }
                }

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = pInfoEffectPlayer.maiYieldRateGenerals[eLoopYield];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_YIELD_RATE_GENERALS", TEXTVAR(bAllCities), buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));
                    }
                }

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = pInfoEffectPlayer.maiYieldUpkeep[eLoopYield];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_YIELD_UPKEEP", buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));
                    }
                }

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                {
                    int iValue = pInfoEffectPlayer.maiUnitTraitConsumptionModifier[eLoopUnitTrait];
                    if (iValue != 0)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_UNIT_TRAIT_CONSUMPTION_MODIFIER", buildUnitTraitLinkVariable(eLoopUnitTrait), buildSignedTextVariable(iValue, true));
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

                foreach (YieldType eLoopYield in pInfoEffectPlayer.maeTradeYield)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_TRADE_YIELD", buildYieldLinkVariable(eLoopYield));
                }

                foreach (YieldType eLoopYield in pInfoEffectPlayer.maeNoSellPenaltyYield)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_NO_SELL_PENALTY_YIELD", buildYieldLinkVariable(eLoopYield));
                }

                foreach (YieldType eLoopYield in pInfoEffectPlayer.maeConnectedForeign)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CONNECTED_FOREIGN", buildYieldLinkVariable(eLoopYield));
                    if (pGame != null)
                    {
                        using (TextBuilder subBuilder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            for (TeamType eLoopTeam = 0; eLoopTeam < pGame.getNumTeams(); eLoopTeam++)
                            {
                                int iFromConnection = pPlayer.getConnectedForeignYieldTotal(eLoopYield, eLoopTeam, bPreview: !pPlayer.isConnectedForeignUnlock(eLoopYield));
                                if (iFromConnection != 0)
                                {
                                    TextVariable yieldVariable = buildColorTextOptionalVariable(buildYieldValueIconLinkVariable(eLoopYield, iFromConnection, bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER), !(infos().Helpers.yieldWarning(eLoopYield, iFromConnection)));
                                    TextVariable previewVariable = pPlayer.isConnectedForeignUnlock(eLoopYield) ? QUICKTEXTVAR("icon(bullet)") : TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CONNECTED_FOREIGN_WILL_GAIN");
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
                                buildEffectCityHelpYieldsAll(subText, eEffectCity, pGame, pPlayer, null, pActivePlayer);
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
                                buildEffectCityHelpYieldsPotentialAll(subText, eEffectCity, pGame, pPlayer, null, pActivePlayer);
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
                                buildEffectCityHelpYieldsAll(subText, eEffectCityExtra, pGame, pPlayer, null, pActivePlayer);
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
                                buildEffectCityHelpYieldsPotentialAll(subText, eEffectCityExtra, pGame, pPlayer, null, pActivePlayer);
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
                                buildEffectCityHelp(subText, eCapitalEffectCity, pGame, pCapitalCity, pCapitalCity?.governor(), false, pActivePlayer);
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
                                buildEffectCityHelpAll(subText, eConnectedEffectCity, pGame, pPlayer, x => x.isConnected() ? 1 : 0, false, pActivePlayer);
                            }

                            builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_CONNECTED_EFFECT", subText.ToTextVariable());
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
                                    buildEffectCityHelpAll(subText, eStateReligionEffectCity, pGame, pPlayer, x => x.isReligion(eStateReligion) ? 1 : 0, false, pActivePlayer);
                                }
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_RELIGIOUS_CITIES_EFFECT", buildReligionLinkVariable(eStateReligion, pGame, pActivePlayer, false), subText.ToTextVariable());
                            }
                            else
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    buildEffectCityHelp(subText, eStateReligionEffectCity, pGame, pPlayer, false, pActivePlayer);
                                }
                                builder.AddTEXT("TEXT_HELPTEXT_EFFECT_PLAYER_HELP_STATE_RELIGION_CITIES_EFFECT", subText.ToTextVariable());
                            }
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
                            buildBonusHelp(subText, pLoopPair.eBonus, pGame, pPlayer, pActivePlayer, bName: false, bShowCity: false, startLineVariable: TEXTVAR(""));
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
                                subText.Add(buildLawLinkVariable(eLoopLaw));
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
                                    buildEffectPlayerHelp(builder, pInfoEffectPlayer.maeEffectPlayerEffectPlayer[eLoopEffectPlayer], pGame, pPlayer, pActivePlayer, ref effectPlayerIgnore, eStateReligion, bAllCities, effectCityScopeType);
                                }

                                if (subText.HasContent)
                                {
                                    builder.AddWithParenthesis(TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_CITY_HELP_NO_YIELDS_EFFECT_CITY_EFFECT_CITY", 
                                        buildEffectPlayerLinkVariable(eLoopEffectPlayer), subText.ToTextVariable()));
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



        //120 lines copy&paste START
        //lines 37481-37607
        public override TextBuilder buildResourceHelp(TextBuilder builder, ResourceType eResource, Game pGame, Player pPlayer, Tile pTile, Player pActivePlayer, bool bName = true)
        {
            //using (new UnityProfileScope("HelpText.buildResourceHelp"))
            {
                City pCityTerritory = pTile?.cityTerritory();

                if (bName)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_NAME", TEXTVAR_TYPE(infos().resource(eResource).mName), TEXTVAR((pTile != null) ? pTile.isHarvested() : false));
                }

                if (pPlayer != null)
                {
                    for (FamilyType eLoopFamily = 0; eLoopFamily < infos().familiesNum(); eLoopFamily++)
                    {
                        if (pPlayer.isFamilyStarted(eLoopFamily))
                        {
                            int iValue = pGame.familyClass(eLoopFamily).maiLuxuryMissingOpinion[eResource];
                            if (iValue != 0)
                            {
                                builder.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_MISSING", buildFamilyLinkVariable(eLoopFamily, pGame), TEXTVAR(iValue)), !pPlayer.isFamilyLuxury(eLoopFamily, eResource)));
                            }
                        }
                    }
                }


                using (TextBuilder commaList = TextBuilder.GetTextBuilder(TextManager))
                {
                    using (commaList.BeginScope(TextBuilder.ScopeType.COMMA))
                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            int iValue = infos().resource(eResource).maiYieldNoImprovement[eLoopYield];
                            if (iValue > 0)
                            {
                                commaList.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                            }
                        }
                    }

                    if (commaList.HasContent)
                    {
                        builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_NO_IMPROVEMENT", commaList.ToTextVariable(), buildTurnScaleName(pGame)));
                    }
                }

                for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_FROM_LIST", buildImprovementClassLinkVariable(eLoopImprovementClass))))
                    {
                        bool bAdded = false;

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            int iValue = 0;
                            infos().Helpers.yieldOutputResource(eLoopImprovementClass, eLoopYield, eResource, ref iValue);
                            if (iValue != 0)
                            {
                                builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                bAdded = true;
                            }
                        }

/*####### Better Old World AI - Base DLL #######
  ### show abNoBaseOutput              START ###
  ##############################################*/
                        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                        {
                            if (infos().improvement(eLoopImprovement).meClass == eLoopImprovementClass)
                            {
                                if (infos().improvement(eLoopImprovement).mabNoBaseOutput[(int)eResource])
                                {
                                    builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_REPLACES_BASE_IMPROVEMENT_OUTPUT"));
                                }

                                //assumption: mabNoBaseOutput is the same for all improvements in improvement class, so I only need to check the first
                                break;
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### show abNoBaseOutput                END ###
  ##############################################*/

                        if (pPlayer != null)
                        {
                            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < infos().effectCitiesNum(); eLoopEffectCity++)
                            {
                                if (infos().improvementClass(eLoopImprovementClass).maeResourceCityEffect[eResource] == eLoopEffectCity)
                                {
                                    for (EffectCityType eOtherEffectCity = 0; eOtherEffectCity < infos().effectCitiesNum(); eOtherEffectCity++)
                                    {
                                        EffectCityType eEffectCity = infos().effectCity(eOtherEffectCity).maeEffectCityEffectCity[eLoopEffectCity];
                                        if (eEffectCity != EffectCityType.NONE && infos().effectCity(eOtherEffectCity).meSourceNation == pPlayer.getNation())
                                        {
                                            builder.Add(buildEffectCityLinkVariable(eEffectCity, null, null));
                                        }
                                    }
                                }
                            }
                        }

                        {
                            EffectCityType eEffectCity = infos().improvementClass(eLoopImprovementClass).maeResourceCityEffect[eResource];
                            if (eEffectCity != EffectCityType.NONE)
                            {
                                buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), false, pActivePlayer);
                                bAdded = true;
                            }
                        }

                        if (bAdded)
                        {
                            TechType ePrereqTech = infos().improvementClass(eLoopImprovementClass).meTechPrereq;

                            if (ePrereqTech != TechType.NONE)
                            {
                                if ((pActivePlayer != null) && !(pActivePlayer.isTechAcquired(ePrereqTech)))
                                {
                                    builder.AddWithParenthesis(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(ePrereqTech))));
                                }
                            }
                        }
                    }
                }

                for (SpecialistClassType eLoopSpecialistClass = 0; eLoopSpecialistClass < infos().specialistClassesNum(); eLoopSpecialistClass++)
                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_FROM_LIST", buildSpecialistClassLinkVariable(eLoopSpecialistClass, pTile))))
                    {
                        EffectCityType eEffectCity = infos().specialistClass(eLoopSpecialistClass).maeResourceCityEffect[eResource];
                        if (eEffectCity != EffectCityType.NONE)
                        {
                            buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), false, pActivePlayer);
                        }
                    }
                }

                {
                    TextVariable harvestText = buildResourceHarvestHelpVariable(pPlayer, pTile, pActivePlayer);

                    if (!harvestText.IsNullOrEmpty())
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_HARVEST", harvestText);
                    }
                }

                for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < infos().effectCitiesNum(); eLoopEffectCity++)
                {
                    if (infos().effectCity(eLoopEffectCity).maeLuxuryResources.Contains(eResource))
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_FROM", buildEffectCitySourceLinkVariable(eLoopEffectCity, null, null, pGame, pActivePlayer));
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### show total Resource count on map START ###
  ##############################################*/
                if (pGame != null && ( (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT + ((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT + ((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES) > 0 ) )
                {
                    if (pGame.getResourceCount(eResource) <= 0)
                    {
                        if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT > 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_COUNT_ON_MAP", pGame.getResourceCount(eResource));
                        }
                    }
                    else if (pPlayer != null)
                    {
                        int iTotal = 0;
                        int iPlayerTerritory = 0;
                        int iTeamTerritory = 0; //not player, other team member
                        int iRevealed = 0;
                        int iFog = 0;

                        CommaListVariableGenerator tileTerritoryList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.NONE, TextManager);
                        CommaListVariableGenerator tileTeamList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.PARENTHESIS, true, TextManager);
                        CommaListVariableGenerator tileRevealedList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.PARENTHESIS, true, TextManager);
                        CommaListVariableGenerator tileUnrevealedList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.PARENTHESIS_OUTER, true, TextManager);
                        for (int iI = 0; iI < pGame.getNumTiles(); ++iI)
                        {
                            Tile pLoopTile = pGame.tile(iI);
                            if (pLoopTile != null)
                            {
                                if (pLoopTile.getResource() == eResource)
                                {
                                    if (pLoopTile.getTeam() == pPlayer.getTeam())
                                    {
                                        
                                        pCityTerritory = pLoopTile.cityTerritory();
                                        if (pCityTerritory == null) //this should not be possible
                                        {
                                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES > 0)
                                            {
                                                tileTerritoryList.AddItem(buildLocationLinkVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_TILE", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY())), pLoopTile));
                                            }
                                        }
                                        else
                                        {
                                            TextVariable name = null;
                                            if (pCityTerritory.getNameType() != CityNameType.NONE)
                                            {
                                                name = TEXTVAR(infos().cityName(pCityTerritory.getNameType()).mName, TextManager);
                                            }
                                            else
                                            {
                                                name = TEXTVAR(pCityTerritory.getName());
                                            }
                                            if (name == null) name = TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_TILE", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY()));

                                            //1. a Specialist could be in build queue
                                            TextVariable CityResource = null;
                                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES > 0)
                                            { 
                                                SpecialistType eTileSpecialist = SpecialistType.NONE;
                                                bool bSpecialistQueued = false;
                                                bool bSpecialistBuilding = false;
                                                ImprovementType eTileImprovement = ImprovementType.NONE;
                                                bool bImprovementBuilding = false;
                                                bool bImprovementPillaged = false;
                                                int iTurnsLeft = 0;
                                                if (pCityTerritory.getBuildCount() > 0)
                                                {
                                                    for (int i = pCityTerritory.getBuildCount() - 1; i >= 0; i--)
                                                    {
                                                        CityQueueData data = pCityTerritory.getBuildQueueNode(i);
                                                        if (data.meBuild == infos().Globals.SPECIALIST_BUILD && data.miData == pLoopTile.getID())
                                                        {
                                                            eTileSpecialist = (SpecialistType)(data.miType);
                                                            if (i == 0)
                                                            {
                                                                bSpecialistBuilding = true;
                                                                iTurnsLeft = pCityTerritory.getBuildTurnsLeft(data);
                                                            }
                                                            else
                                                            {
                                                                bSpecialistQueued = true;
                                                            }

                                                            break;
                                                        }
                                                    }
                                                }
                                                if (!bSpecialistQueued && !bSpecialistBuilding)
                                                {
                                                    //2. a Specialist could already be there
                                                    if (pLoopTile.getSpecialist() != SpecialistType.NONE)
                                                    {
                                                        eTileSpecialist = pLoopTile.getSpecialist();
                                                    }
                                                }
                                                if (eTileSpecialist == SpecialistType.NONE)
                                                {
                                                    //3. there could at least be an improvement
                                                    eTileImprovement = pLoopTile.getImprovement();
                                                    if (eTileImprovement != ImprovementType.NONE)
                                                    {
                                                        if (pLoopTile.isPillaged())
                                                        {
                                                            bImprovementPillaged = true;
                                                            iTurnsLeft = pLoopTile.getImprovementPillageTurns();
                                                        }
                                                        else if (pLoopTile.getImprovementBuildTurnsLeft() > 0)
                                                        {
                                                            bImprovementBuilding = true;
                                                            iTurnsLeft = pLoopTile.getImprovementBuildTurnsLeft();
                                                        }
                                                    }
                                                }

                                                //now the fun part: building the text
                                                if (bSpecialistQueued)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_IN_QUEUE");
                                                }
                                                else if (bSpecialistBuilding)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_SPECIALIST_IN_TRAINING");
                                                }
                                                else if (bImprovementBuilding)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_UNDER_CONSTRUCTION");
                                                }
                                                else if (bImprovementPillaged)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_PILLAGED_IMPROVEMENT");
                                                }

                                                if (iTurnsLeft > 0)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", CityResource, TEXTVAR_TYPE("TEXT_HELPTEXT_TURNS_TEXT_SHORT", TEXTVAR(iTurnsLeft), TEXTVAR(true), pGame.turnScaleNameShort(iTurnsLeft)));
                                                }

                                                if (eTileSpecialist != SpecialistType.NONE)
                                                {
                                                    if (CityResource != null)
                                                    {
                                                        CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", TEXTVAR_TYPE(infos().specialist(eTileSpecialist).meName), CityResource);
                                                    }
                                                    else
                                                    {
                                                        CityResource = TEXTVAR_TYPE(infos().specialist(eTileSpecialist).meName);
                                                    }
                                                }
                                                else if (eTileImprovement != ImprovementType.NONE)
                                                {
                                                    if (CityResource != null)
                                                    {
                                                        CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", TEXTVAR_TYPE(infos().improvement(eTileImprovement).mName), CityResource);
                                                    }
                                                    else
                                                    {
                                                        CityResource = TEXTVAR_TYPE(infos().improvement(eTileImprovement).mName); ;
                                                    }
                                                }

                                                if (CityResource == null)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_COMMA_SPACE_TWO", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY()));
                                                }

                                            }

                                            if (pCityTerritory.getPlayer() == pPlayer.getPlayer())
                                            {
                                                iPlayerTerritory++;
                                                if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 1)
                                                {
                                                    tileTerritoryList.AddItem(buildLocationLinkVariable((TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_TILE_IN_CITY", CityResource, name)), pLoopTile));
                                                }
                                            }
                                            else 
                                            {
                                                iTeamTerritory++;
                                                if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 2)
                                                {
                                                    tileTeamList.AddItem(buildLocationLinkVariable((TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_TILE_IN_CITY", CityResource, name)), pLoopTile));
                                                }
                                            }
                                        }

                                    }
                                    else if (pLoopTile.isRevealed(pPlayer.getTeam()))
                                    {
                                        iRevealed++;
                                        if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 3)
                                        {
                                            tileRevealedList.AddItem(buildLocationLinkVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_COMMA_SPACE_TWO", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY())), pLoopTile));
                                        }
                                    }
                                    else
                                    {
                                        iFog++;
                                        if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 4)
                                        {
                                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 5)
                                            {
                                                tileRevealedList.AddItem(buildLocationLinkVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_COMMA_SPACE_TWO", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY())), pLoopTile));
                                            }
                                            else
                                            {
                                                tileUnrevealedList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_COMMA_SPACE_TWO", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY())));
                                            }
                                        }
                                    }

                                    iTotal++;
                                    if (iTotal == pGame.getResourceCount(eResource))
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        CommaListVariableGenerator countList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.NONE, TextManager);
                        //if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT > 0)
                        //{
                        //    countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_ON_MAP", pGame.getResourceCount(eResource)));
                        //}

                        if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT >= 1)
                        {
                            countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_PLAYER", iPlayerTerritory));

                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT >= 2)
                            {
                                if (iTeamTerritory > 0) countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_TEAM", iTeamTerritory));

                                if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT >= 3)
                                {
                                    if (iRevealed > 0) countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_REVEALED", iRevealed));

                                    if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT >= 4)
                                    {
                                        if (iFog > 0) countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_FOG", iFog));
                                    }
                                }
                            }
                        }
                        if (countList.Count > 0)
                        {
                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT > 0)
                            {
                                builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_ON_MAP", pGame.getResourceCount(eResource)), countList.Finalize()));
                            }
                            else
                            {
                                builder.Add(countList.Finalize());
                            }
                        }
                        else if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT > 0)
                        {
                            builder.Add((TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_ON_MAP", pGame.getResourceCount(eResource))));
                        }


                        CommaListVariableGenerator tileList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.NONE, TextManager);
                        if (tileTerritoryList.Count > 0)
                        {
                            tileList.AddItem(tileTerritoryList.Finalize());
                        }
                        if (tileRevealedList.Count > 0)
                        {
                            tileList.AddItem(tileRevealedList.Finalize());
                        }
                        if (tileUnrevealedList.Count > 0)
                        {
                            tileList.AddItem(tileUnrevealedList.Finalize());
                        }
                        if (tileList.Count > 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_TILES_ON_MAP", tileList.Finalize());
                        }
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### show total Resource count on map   END ###
  ##############################################*/

                return builder;
            }
        }
        //120 line copy paste END

        //copy-paste START
        //lines 39607-39647
        public override TextBuilder buildUrbanHelp(TextBuilder builder)
        {
            using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
            {
                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_URBAN");

/*####### Better Old World AI - Base DLL #######
  ### Urban restrictions Explained     START ###
  ##############################################*/
                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_URBAN_RESTRICTIONS");
/*####### Better Old World AI - Base DLL #######
  ### Urban restrictions Explained       END ###
  ##############################################*/

                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_URBAN_SPREADS_BORDERS");

                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_URBAN_TRADE_NETWORK");

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                {
                    EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                    if (eEffectUnit != EffectUnitType.NONE)
                    {
                        int iValue = infos().effectUnit(eEffectUnit).miUrbanAttackModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildUnitTraitLinkVariable(eLoopUnitTrait), buildAttackValueLinkVariable(iValue, true));
                        }
                    }
                }

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                {
                    EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                    if (eEffectUnit != EffectUnitType.NONE)
                    {
                        int iValue = infos().effectUnit(eEffectUnit).miUrbanDefenseModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildUnitTraitLinkVariable(eLoopUnitTrait), buildDefenseValueLinkVariable(iValue, true));
                        }
                    }
                }
            }

            return builder;
        }
        //copy-paste END

        //copy-paste START
        //lines 39783-40305
        public override TextBuilder buildCityYieldNetHelp(TextBuilder builder, City pCity, Character pGovernor, YieldType eYield, ClientManager pManager, bool bNetOnly = false, bool bReverseSign = false)
        {
            Game pGame = pManager.GameClient;
            Player pActivePlayer = pManager.activePlayer();

            if (pCity != null)
            {
                using (var effectCityCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
                {
                    Dictionary<EffectCityType, int> dEffectCityCounts = effectCityCountsScoped.Value;

                    pCity.getEffectCityCountsForGovernor(pGovernor, dEffectCityCounts);

                    Player pPlayer = pCity.player();

                    int iRate = pCity.calculateCurrentYield(eYield);
                    int iBaseYield = pCity.getBaseYieldNetForGovernor(eYield, pGovernor);
                    int iModifiedYield = pCity.calculateModifiedYieldGovernor(eYield, pGovernor);

                    if (!bNetOnly)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildYieldLinkVariable(eYield), buildIconTextVariable(infos().yield(eYield).meType));

                        if (pCity.isYieldBuildCurrent(eYield) && pCity.getCurrentBuild().mdYieldCosts.TryGetValue(eYield, out int value) && value > 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_USED_FOR",
                                buildColonSpaceOne(TEXTVAR(pCity.getName()), buildYieldValueIconLinkVariable(eYield, iRate, iMultiplier: Constants.YIELDS_MULTIPLIER)),
                                getQueueLinkVariable(pCity.getCurrentBuild(), pCity, pGame, true, false));
                        }
                        else
                        {
                            YieldType eDisplayYield = eYield;
                            int iDisplayRate = iRate;
                            getDisplayYield(ref eDisplayYield, ref iDisplayRate);

                            builder.Add(buildColonSpaceOne(TEXTVAR(pCity.getName()), buildYieldValueIconLinkVariable(eDisplayYield, iDisplayRate, iMultiplier: Constants.YIELDS_MULTIPLIER)));
                        }
                        builder.Add(getCityYieldProgress(pCity, eYield), skipSeparator: true);

                        int iTileBase = pCity.calculateUnmodifiedTileYield(eYield, pGovernor);
                        if (iTileBase != 0 && iTileBase != iBaseYield)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TILE_BASE_YIELD", buildYieldTextVariable(iTileBase, iMultiplier: Constants.YIELDS_MULTIPLIER));
                        }
                        // base yield included in subtotal, below
                        //if (iBaseYield != 0 && iBaseYield != iRate)
                        //{
                        //    builder.AddTEXT("TEXT_HELPTEXT_CITY_BASE_YIELD", buildYieldTextVariable(iBaseYield, iMultiplier: Constants.YIELDS_MULTIPLIER));
                        //}

                        buildDividerText(builder);
                    }

                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); ++eLoopYield)
                    {
                        if (infos().yield(eLoopYield).meSubtractFromYield == eYield)
                        {
                            builder.Add(buildYieldIconNameLinkVariable(eYield));
                            break;
                        }
                    }

                    for (int iPass = 0; iPass < 2; iPass++)
                    {
                        SetList<EffectCityType> seGovernorCityEffects = new SetList<EffectCityType>();

                        if (pGovernor != null)
                        {
                            foreach (TraitType eLoopTrait in pGovernor.getTraits())
                            {
                                EffectCityType eEffectCity = infos().trait(eLoopTrait).meGovernorEffectCity;
                                EffectCityType eEffectCityState = infos().trait(eLoopTrait).meStateReligionEffectCity;

                                if (eEffectCity != EffectCityType.NONE)
                                {
                                    int iValue = pCity.getEffectCityYieldRate(eEffectCity, eYield, pGovernor);
                                    if (bReverseSign)
                                    {
                                        iValue = -(iValue);
                                    }
                                    if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_CONCAT_COLON_ENCLOSED_PARENTHESIS",
                                            buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER),
                                            buildCharacterLinkVariable(pGovernor, pActivePlayer),
                                            buildTraitLinkVariable(eLoopTrait, pGovernor));
                                    }

                                    seGovernorCityEffects.Add(eEffectCity);
                                }

                                if (eEffectCityState != EffectCityType.NONE && pCity.hasStateReligion())
                                {
                                    int iValue = pCity.getEffectCityYieldRate(eEffectCityState, eYield, pGovernor);
                                    if (bReverseSign)
                                    {
                                        iValue = -(iValue);
                                    }
                                    if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_CONCAT_COLON_ENCLOSED_PARENTHESIS",
                                            buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER),
                                            buildCharacterLinkVariable(pGovernor, pActivePlayer),
                                            buildTraitLinkVariable(eLoopTrait, pGovernor));
                                    }

                                    seGovernorCityEffects.Add(eEffectCityState);
                                }
                            }
                        }

                        foreach (KeyValuePair<EffectCityType, int> p in dEffectCityCounts)
                        {
                            EffectCityType eLoopEffectCity = p.Key;
                            int iCount = p.Value;

                            if (seGovernorCityEffects.Contains(eLoopEffectCity))
                            {
                                iCount--;
                            }

                            if (iCount > 0)
                            {
                                int iValue = pCity.getEffectCityYieldRate(eLoopEffectCity, eYield, pGovernor);
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    TextVariable yieldEffectText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_COLON_X_COUNT",
                                        buildYieldTextVariable((iCount * iValue), true, false, Constants.YIELDS_MULTIPLIER),
                                        buildEffectCitySourceLinkVariable(eLoopEffectCity, pCity, pGovernor, pGame, pActivePlayer),
                                        (iCount > 1 ? TEXTVAR(iCount) : TEXTVAR(false)));

                                    if (infos().effectCity(eLoopEffectCity).maaiEffectCityYieldRate.Count > 0)
                                    {
                                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                        {
                                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: buildEnclosedParenthesisIf(yieldEffectText, null), emptyScopeText: TEXTVAR(false)))
                                            {
                                                foreach (KeyValuePair<EffectCityType, int> q in dEffectCityCounts)
                                                {
                                                    EffectCityType eOtherEffectCity = q.Key;
                                                    int iOtherCount = q.Value;
                                                    int iSubValue = infos().effectCity(eLoopEffectCity).maaiEffectCityYieldRate[eOtherEffectCity, eYield];
                                                    if (iSubValue != 0)
                                                    {
                                                        subText.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_YIELD_FROM_X",
                                                            buildSignedTextVariable((iCount * iSubValue), false, Constants.YIELDS_MULTIPLIER),
                                                            buildEffectCitySourceLinkVariable(eOtherEffectCity, pCity, pGovernor, pGame, pActivePlayer),
                                                            ((iOtherCount > 1) ? TEXTVAR(iOtherCount) : TEXTVAR(false))
                                                            );
                                                    }
                                                }
                                            }

                                            builder.Add(subText.ToTextVariable());
                                        }
                                    }
                                    else
                                    {
                                        builder.Add(yieldEffectText);
                                    }
                                }
                            }
                        }

                        using (var resourceMapScoped = CollectionCache.GetDictionaryScoped<ResourceType, int>())
                        using (var improvementMapScoped = CollectionCache.GetDictionaryScoped<ImprovementType, int>())
                        using (var specialistMapScoped = CollectionCache.GetDictionaryScoped<SpecialistType, int>())
                        {
                            Dictionary<ResourceType, int> dResourceYields = resourceMapScoped.Value;
                            Dictionary<ImprovementType, int> dImprovementYields = improvementMapScoped.Value;
                            Dictionary<SpecialistType, int> dSpecialistYields = specialistMapScoped.Value;
                            foreach (int iTileID in pCity.getTerritoryTiles())
                            {
                                Tile pTile = pGame.tile(iTileID);
                                if (pTile.hasImprovementFinished())
                                {
                                    ImprovementType eImprovement = pTile.getImprovement();
                                    SpecialistType eSpecialist = pTile.getSpecialist();

                                    int iBase = pTile.yieldOutputModified(pTile.getImprovement(), SpecialistType.NONE, eYield);
                                    if (iBase != 0)
                                    {
                                        if (!dImprovementYields.ContainsKey(eImprovement))
                                        {
                                            dImprovementYields.Add(eImprovement, iBase);
                                        }
                                        else
                                        {
                                            dImprovementYields[eImprovement] += iBase;
                                        }
                                    }

                                    if (eSpecialist != SpecialistType.NONE)
                                    {
                                        int iSpecialist = (pTile.yieldOutputModified(pTile.getImprovement(), eSpecialist, eYield) - iBase);
                                        if (iSpecialist != 0)
                                        {
                                            if (!dSpecialistYields.ContainsKey(eSpecialist))
                                            {
                                                dSpecialistYields.Add(eSpecialist, iSpecialist);
                                            }
                                            else
                                            {
                                                dSpecialistYields[eSpecialist] += iSpecialist;
                                            }
                                        }
                                    }
                                }
                                else if (pTile.hasResource())
                                {
                                    ResourceType eResource = pTile.getResource();

                                    int iValue = pTile.yieldOutputModified(ImprovementType.NONE, SpecialistType.NONE, eYield, bCityEffects: true);
                                    if (iValue != 0)
                                    {
                                        if (!dResourceYields.ContainsKey(eResource))
                                        {
                                            dResourceYields.Add(eResource, iValue);
                                        }
                                        else
                                        {
                                            dResourceYields[eResource] += iValue;
                                        }
                                    }
                                }
                            }
                            foreach (KeyValuePair<ResourceType, int> pPair in dResourceYields)
                            {
                                int iValue = pPair.Value;
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_YIELD_FROM_RESOURCE", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildResourceLinkVariable(pPair.Key));
                                }
                            }
                            foreach (KeyValuePair<SpecialistType, int> pPair in dSpecialistYields)
                            {
                                int iValue = pPair.Value;
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_YIELD_FROM_IMPROVEMENT", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildSpecialistLinkVariable(pPair.Key, pGame));
                                }
                            }
                            foreach (KeyValuePair<ImprovementType, int> pPair in dImprovementYields)
                            {
                                int iValue = pPair.Value;
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.Add(buildColonSpaceOne(buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildImprovementLinkVariable(pPair.Key, pGame)));
                                }
                            }
                        }

                        if (pGovernor != null && pGovernor.isLeader())
                        {
                            int iValue = infos().yield(eYield).miLeaderGovernor;
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_GOVERNOR_IS_LEADER", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER));
                            }
                        }

                        if (pCity.getTeam() != pCity.getFirstTeam() && pCity.getFirstPlayer() != PlayerType.NONE)
                        {
                            int iValue = infos().yield(eYield).miForeignPopulation;
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_FOREIGN_POPULATION", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildNationLinkVariable(pCity.firstNation().meType, pCity.getFirstPlayer()));
                            }
                        }

                        {
                            int iValue = pCity.calculateYieldRateDefending(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_DEFENDING", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), TEXTVAR(pCity.hasFamily()));
                            }
                        }

                        {
                            int iValue = pCity.calculateImprovementYield(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                int iCount = pCity.getFinishedImprovementCountAll();
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_IMPROVEMENT", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), ((iCount > 1) ? TEXTVAR(iCount) : TEXTVAR(false)), buildUrbanLinkVariable());
                            }
                        }

                        {
                            int iValue = pCity.calculateCapitalDistanceYield(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_CAPITAL_DISTANCE", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildCapitalLinkVariable(pPlayer.capitalCity()));
                            }
                        }

                        if (pGovernor != null)
                        {
                            int iValue = pCity.calculateGovernorOpinionYield(eYield, pGovernor);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_GOVERNOR_OPINION", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildOpinionCharacterLinkVariable(pPlayer.getCharacterOpinion(pGovernor), pGovernor, pPlayer.getPlayer()), buildCharacterLinkVariable(pGovernor, pActivePlayer));
                            }
                        }

                        if (pPlayer != null)
                        {
                            {
                                int iValue = pPlayer.calculateMissingFamilyYield(eYield);
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_MISSING_FAMILY", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER));
                                }
                            }

                            for (CouncilType eLoopCouncil = 0; eLoopCouncil < infos().councilsNum(); eLoopCouncil++)
                            {
                                Character pCouncilCharacter = pPlayer.councilCharacter(eLoopCouncil);

                                if (pCouncilCharacter != null)
                                {
                                    for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                    {
                                        int iValue = pCouncilCharacter.getRatingYieldRateCouncilCity(eLoopRating, eYield, eLoopCouncil);
                                        if (bReverseSign)
                                        {
                                            iValue = -(iValue);
                                        }
                                        if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_CONCAT_COLON_ENCLOSED_PARENTHESIS",
                                                buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER),
                                                buildCharacterLinkVariable(pCouncilCharacter, pActivePlayer, bCognomen: false, bSkipNation: true),
                                                buildCharacterRatingLinkVariable(pCouncilCharacter, eLoopRating, true));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (iBaseYield != iModifiedYield)
                    {
                        if (bReverseSign)
                        {
                            iBaseYield *= -1;
                        }
                        builder.AddTEXT("TEXT_HELPTEXT_SUBTOTAL", buildSignedTextVariable(iBaseYield, iMultiplier: Constants.YIELDS_MULTIPLIER));
                    }

                    for (int iPass = 0; iPass < 2; iPass++)
                    {
                        {
                            int iValue = pCity.getHappinessLevelYieldModifier(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.Add(buildColonSpaceOne(buildSignedTextVariable(iValue, true), buildHappinessLevelLinkVariable(pCity)));
                            }
                        }

                        {
                            int iValue = pCity.getDamageYieldModifier(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_DAMAGE_YIELD_MODIFIER", buildSignedTextVariable(iValue, true), TEXTVAR(pCity.getHP()), TEXTVAR(pCity.getHPMax()));
                            }
                        }

                        {
                            int iValue = pCity.getAssimilateYieldModifier(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_ASSIMILATE_YIELD_MODIFIER", buildSignedTextVariable(iValue, true));
                            }
                        }

                        foreach (KeyValuePair<EffectCityType, int> p in dEffectCityCounts)
                        {
                            EffectCityType eLoopEffectCity = p.Key;
                            int iCount = p.Value;
                            //int iValue = (infos().effectCity(eLoopEffectCity).maiYieldModifier[eYield] * iCount);
                            int iValue = (pCity.getEffectCityYieldModifier(eLoopEffectCity, eYield) * iCount); //includes maaiEffectCityYieldModifier
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_YIELD_FROM_X",
                                    buildSignedTextVariable(iValue, true),
                                    buildEffectCitySourceLinkVariable(eLoopEffectCity, pCity, pGovernor, pGame, pActivePlayer),
                                    (iCount > 1 ? TEXTVAR(iCount) : TEXTVAR(false)));
                            }
                        }

                        if (pGovernor != null)
                        {
                            for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                            {
                                int iValue = pGovernor.getRatingYieldModifierGovernor(eLoopRating, eYield);
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_GOVERNOR_YIELD_MODIFIER", buildSignedTextVariable(iValue, true), buildCharacterLinkVariable(pGovernor, pActivePlayer), buildCharacterRatingLinkVariable(pGovernor, eLoopRating));
                                }
                            }
                        }

                        if (pCity.hasBuild())
                        {
                            CityQueueData pCurrentBuild = pCity.getCurrentBuild();

                            if (pCurrentBuild.meBuild == infos().Globals.PROJECT_BUILD)
                            {
                                ProjectType eProject = (ProjectType)(pCurrentBuild.miType);
                                int iValue = infos().project(eProject).maiYieldModifier[eYield];
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_PROJECT_YIELD_MODIFIER", buildSignedTextVariable(infos().utils().modify(pCity.getBaseYieldNet(eYield), iValue - 100), false, Constants.YIELDS_MULTIPLIER), buildProjectLinkVariable(eProject, pCity));
                                }
                            }
                        }
                    }

                    for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                    {
                        int iValue = pCity.calculateUnitImprovementYield(eYield, eLoopUnit);
                        if (iValue != 0)
                        {
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            builder.AddTEXT("TEXT_HELPTEXT_YIELD_NET_UNIT_IMPROVEMENT_YIELD", buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER), buildUnitTypeLinkVariable(eLoopUnit, pGame));
                        }
                    }

                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); ++eLoopYield)
                    {
                        if (infos().yield(eLoopYield).meSubtractFromYield == eYield)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TOTAL_YIELD", buildYieldLinkVariable(eYield), buildSignedTextVariable(iModifiedYield, iMultiplier: Constants.YIELDS_MULTIPLIER));
                            break;
                        }
                    }

                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); ++eLoopYield)
                        {
                            if (infos().yield(eLoopYield).meSubtractFromYield == eYield)
                            {
                                buildDividerText(builder);
                                builder.Add(buildYieldIconNameLinkVariable(eLoopYield));
/*####### Better Old World AI - Base DLL #######
  ### don't reverse sign               START ###
  ##############################################*/
                                //buildCityYieldNetHelp(builder, pCity, eLoopYield, pManager, bNetOnly: true, bReverseSign: true);
                                //builder.AddTEXT("TEXT_HELPTEXT_TOTAL_YIELD", buildYieldLinkVariable(eLoopYield), buildSignedTextVariable(-(pCity.calculateModifiedYieldBase(eLoopYield)), iMultiplier: Constants.YIELDS_MULTIPLIER));
                                buildCityYieldNetHelp(builder, pCity, pGovernor, eLoopYield, pManager, bNetOnly: true, bReverseSign: false);
                                builder.AddTEXT("TEXT_HELPTEXT_TOTAL_YIELD", buildYieldLinkVariable(eLoopYield), buildSignedTextVariable(pCity.calculateModifiedYield(eLoopYield), iMultiplier: Constants.YIELDS_MULTIPLIER));
/*####### Better Old World AI - Base DLL #######
  ### don't reverse sign                 END ###
  ##############################################*/
                            }
                        }
                    }

                    if (!bNetOnly && pActivePlayer != null && pActivePlayer.isPlayerOption(infos().Globals.DEBUG_HELP))
                    {
                        buildYieldDebugText(builder, eYield, pManager);
                    }
                }
            }

            return builder;
        }
        //copy-paste END

        //lines 42813-43676
        public override TextBuilder buildTraitHelp(TextBuilder builder, TraitType eTrait, Game pGame, Player pActivePlayer, Character pCharacter = null, bool bName = false, bool bInvalidTraits = false, bool bRestrictions = false, bool bDetails = false, TextBuilder.ScopeType scopeType = TextBuilder.ScopeType.BULLET)
        {
            //using (new UnityProfileScope("HelpText.buildTraitHelp"))
            {
                BetterAIInfoTrait pInfoTrait = (BetterAIInfoTrait)infos().trait(eTrait);

                if (bName)
                {
                    InfoTrait trait = infos().trait(eTrait);
                    TextType traitName = getGenderedTraitName(trait, pCharacter?.getGender() ?? GenderType.NONE);
                    builder.AddTEXT("TEXT_HELPTEXT_TRAIT_TITLE",
                    string.IsNullOrEmpty(trait.mzIconName) ? TEXTVAR(false) : mSpriteRepository?.GetInlineIconVariable(eTrait) ?? TEXTVAR(false),
                    TEXTVAR_TYPE(traitName),
                    TEXTVAR(trait.mbArchetype),
                    TEXTVAR(trait.mbItem));
                }

                using (builder.BeginScope(scopeType))
                {
                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                    {
                        buildTraitJobs(subText, eTrait, pCharacter);
                        if (subText.HasContent)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_JOB_PREREQ", subText.ToTextVariable());
                        }
                    }

                    {
                        if (infos().trait(eTrait).mbClergy)
                        {
                            if (infos().religion(infos().trait(eTrait).meReligion).meType == ReligionType.NONE)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_CLERGY_TRAIT");
                            }
                            else if (infos().religion(infos().trait(eTrait).meReligion).mePaganNation != NationType.NONE)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_PAGAN_CLERGY_TRAIT");
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_WORLD_CLERGY_TRAIT");
                            }
                        }
                    }

                    {
                        EffectCityType eEffectCity = infos().trait(eTrait).meGovernorEffectCity;

                        if (eEffectCity != EffectCityType.NONE)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                City pCityGovernor = ((pCharacter != null) ? pCharacter.cityGovernor() : null);

                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    buildEffectCityHelp(subText, eEffectCity, pGame, pCityGovernor, pCharacter, false, pActivePlayer);
                                }

                                if (subText.HasContent)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GOVERNOR", subText.ToTextVariable());
                                }
                            }
                        }
                    }

                    {
                        EffectCityType eEffectCity = infos().trait(eTrait).meStateReligionEffectCity;

                        if (eEffectCity != EffectCityType.NONE)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                City pCityGovernor = ((pCharacter != null) ? pCharacter.cityGovernor() : null);

                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    buildEffectCityHelp(subText, eEffectCity, pGame, pCityGovernor, pCharacter, false, pActivePlayer);
                                }

                                if (subText.HasContent)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GOVERNOR_STATE_RELIGION", subText.ToTextVariable());
                                }
                            }
                        }
                    }

                    {
                        EffectUnitType eEffectUnit = infos().trait(eTrait).meGeneralEffectUnit;

                        if (eEffectUnit != EffectUnitType.NONE)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    buildEffectUnitHelp(subText, eEffectUnit, pGame);
                                }

                                if (subText.HasContent)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GENERAL", subText.ToTextVariable(), TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                                }
                            }
                        }
                    }

                    {
                        EffectUnitType eEffectUnit = infos().trait(eTrait).meLeaderEffectUnit;

                        if (eEffectUnit != EffectUnitType.NONE)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    buildEffectUnitHelp(subText, eEffectUnit, pGame);
                                }

                                if (subText.HasContent)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GENERAL_LEADER", subText.ToTextVariable(), TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                                }
                            }
                        }
                    }

                    {
                        EffectPlayerType eEffectPlayer = infos().trait(eTrait).meLeaderEffectPlayer;

                        if (eEffectPlayer != EffectPlayerType.NONE)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    Player pCharacterPlayer = pCharacter != null ? pCharacter.player() : null;
                                    buildEffectPlayerHelp(subText, eEffectPlayer, pGame, pCharacterPlayer, pActivePlayer);
                                }

                                if (subText.HasContent)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_TRAIT_LEADER", subText.ToTextVariable());
                                }
                            }

                            for (CouncilType eLoopCouncil = 0; eLoopCouncil < infos().councilsNum(); eLoopCouncil++)
                            {
                                if (!(infos().council(eLoopCouncil).mbDisable))
                                {
                                    if (infos().council(eLoopCouncil).meEffectPlayerPrereq == eEffectPlayer)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_TECH_UNLOCKS_GENERIC", buildCouncilLinkVariable(eLoopCouncil));
                                    }
                                }
                            }
                        }
                    }

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
                    {
                        if (pInfoTrait.maeJobEffectPlayer.Count > 0)
                        {
                            for (JobType eLoopJob = 0; eLoopJob < infos().jobsNum(); eLoopJob++)
                            {
                                if (pGame != null ? pGame.checkGameContent(eLoopJob) : App.CheckContentOwnership(infos().job(eLoopJob).meGameContentRequired, infos(), null))
                                {
                                    EffectPlayerType eEffectPlayer = pInfoTrait.maeJobEffectPlayer[eLoopJob];
                                    if (eEffectPlayer != EffectPlayerType.NONE)
                                    {
                                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                        {
                                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                            {
                                                Player pCharacterPlayer = pCharacter != null ? pCharacter.player() : null;
                                                buildEffectPlayerHelp(subText, eEffectPlayer, pGame, pCharacterPlayer, pActivePlayer);
                                            }

                                            if (subText.HasContent)
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_JOB", buildJobLinkVariable(eLoopJob, pCharacter), subText.ToTextVariable());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    {
                        if (pInfoTrait.bAnyTraitEffectPlayer)
                        {
                            for (TraitType eLoopTrait = 0; eLoopTrait < infos().traitsNum(); eLoopTrait++)
                            {
                                EffectPlayerType eEffectPlayer = pInfoTrait.maeTraitEffectPlayer[eLoopTrait];
                                if (eEffectPlayer == EffectPlayerType.NONE)
                                {
                                    BetterAIInfoTrait pLoopInfoTrait = (BetterAIInfoTrait)infos().trait(eLoopTrait);
                                    eEffectPlayer = pLoopInfoTrait.maeTraitEffectPlayer[eTrait];
                                }

                                if (eEffectPlayer != EffectPlayerType.NONE)
                                {
                                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            Player pCharacterPlayer = pCharacter != null ? pCharacter.player() : null;
                                            buildEffectPlayerHelp(subText, eEffectPlayer, pGame, pCharacterPlayer, pActivePlayer);
                                        }

                                        if (subText.HasContent)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_TRAIT", buildTraitLinkVariable(eLoopTrait, pCharacter), subText.ToTextVariable());
                                        }
                                    }
                                }
                            }
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/

                    for (MissionType eLoopMission = 0; eLoopMission < infos().missionsNum(); eLoopMission++)
                    {
                        void addMission(TextBuilder builder, MissionType eMission, SubjectType eSubject, Character pCharacter)
                        {
                            if (eSubject != SubjectType.NONE)
                            {
                                if (infos().subject(eSubject).meTraitPrereq == eTrait)
                                {
                                    if (infos().subject(eSubject).meCharacter == CharacterType.NONE || (pCharacter != null && infos().subject(eSubject).meCharacter == pCharacter.getCharacter()))
                                    {
                                        if (infos().subject(eSubject).mbLeader)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_LEADER_MISSION", buildMissionLinkVariable(eMission));
                                        }
                                        else
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_MISSION", buildMissionLinkVariable(eMission));
                                        }
                                    }
                                }
                                if (infos().subject(eSubject).maeTraitAny.Count > 0)
                                {
                                    foreach (TraitType eLoopTrait in infos().subject(eSubject).maeTraitAny)
                                    {
                                        if (eTrait == eLoopTrait)
                                        {
                                            if (infos().subject(eSubject).meCharacter == CharacterType.NONE || (pCharacter != null && infos().subject(eSubject).meCharacter == pCharacter.getCharacter()))
                                            {
                                                if (infos().subject(eSubject).mbLeader)
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_TRAIT_LEADER_MISSION", buildMissionLinkVariable(eMission));
                                                }
                                                else
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_TRAIT_MISSION", buildMissionLinkVariable(eMission));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        addMission(builder, eLoopMission, infos().mission(eLoopMission).meSubjectCharacter, pCharacter);

                        foreach (SubjectType eLoopSubject in infos().mission(eLoopMission).maeSubjectCharacterOn)
                        {
                            if (eLoopSubject != infos().mission(eLoopMission).meSubjectCharacter)
                            {
                                addMission(builder, eLoopMission, eLoopSubject, pCharacter);
                            }
                        }
                    }

                    {
                        int iValue = infos().trait(eTrait).miXPTurn;
                        if (iValue > 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_XP_PER_YEAR", buildSignedTextVariable(iValue), buildTurnScaleName(pGame));

                            if (pCharacter != null)
                            {
                                builder.AddWithParenthesis(buildSlashText(pCharacter.getXP(), pCharacter.getUpgradeXPThreshold()));
                            }
                        }
                    }

                    {
                        int iValue = infos().trait(eTrait).miStrengthLimitModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_STRENGTH_MODIFIER", buildSignedTextVariable(iValue));
                        }
                    }

                    {
                        int iValue = infos().trait(eTrait).miWeaknessLimitModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_WEAKNESS_MODIFIER", buildSignedTextVariable(iValue));
                        }
                    }

                    if (infos().trait(eTrait).meReligionAgent != ReligionType.NONE)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_AGENT", buildAgentLinkVariable(), buildReligionLinkVariable(infos().trait(eTrait).meReligionAgent, pGame, pActivePlayer));
                    }

                    {
                        int iValue = infos().trait(eTrait).miAgentModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_AGENT_MODIFIER", buildSignedTextVariable(iValue, true));
                        }
                    }

                    if (pCharacter != null)
                    {
                        using (TextBuilder subCommaText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            bool bRatingValue = false;

                            using (subCommaText.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                TraitType eArchetype = pCharacter.getArchetype();

                                for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                {
                                    int iValue = infos().trait(eTrait).maiRating[eLoopRating];

                                    if (infos().trait(eTrait).mbArchetype)
                                    {
                                        if ((eArchetype != TraitType.NONE) && (eArchetype != eTrait))
                                        {
                                            iValue -= infos().trait(eArchetype).maiRating[eLoopRating];
                                        }
                                    }

                                    foreach (TraitType eOtherTrait in infos().trait(eTrait).maeTraitReplaces)
                                    {
                                        if (pCharacter.isTrait(eOtherTrait) && (eArchetype != eOtherTrait))
                                        {
                                            iValue -= infos().trait(eOtherTrait).maiRating[eLoopRating];
                                        }
                                    }

                                    if (iValue != 0)
                                    {
                                        TextVariable ratingValueText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildSignedTextVariable(iValue), buildRatingLinkVariable(eLoopRating, true));

                                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                        {
                                            int iNewValue = (pCharacter.getRating(eLoopRating) + ((pCharacter.isTrait(eTrait)) ? 0 : iValue));
                                            int iOldValue = (pCharacter.getRating(eLoopRating) - ((pCharacter.isTrait(eTrait)) ? iValue : 0));

                                            buildRatingHelp(subText, eLoopRating, iNewValue, iOldValue, pGame, pActivePlayer, pCharacter, pCharacter.getPlayerOpinionCharacter(), pCharacter.isLeader(), pCharacter.isLeaderSpouse(), pCharacter.isSuccessor(), pCharacter.isCourtier(), pCharacter.isClergy(), pCharacter.getCouncil(), pCharacter.isCityGovernor(), pCharacter.cityGovernor(), pCharacter.isCityAgent(), pCharacter.cityAgent(), pCharacter.isUnitGeneral(), pCharacter.unitGeneral());
                                            if (subText.HasContent)
                                            {
                                                ratingValueText = buildEnclosedParenthesis(ratingValueText, subText.ToTextVariable());
                                                bRatingValue = true;
                                            }
                                        }

                                        if (bRatingValue)
                                        {
                                            builder.Add(ratingValueText);
                                        }
                                        else
                                        {
                                            subCommaText.AddTEXT("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildSignedTextVariable(iValue), buildRatingLinkVariable(eLoopRating, true));
                                        }
                                    }
                                }
                            }

                            if (subCommaText.HasContent)
                            {
                                builder.Add(subCommaText.ToTextVariable());
                            }
                        }
                    }
                    else
                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                            {
                                int iValue = infos().trait(eTrait).maiRating[eLoopRating];
                                if (iValue != 0)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildSignedTextVariable(iValue), buildRatingLinkVariable(eLoopRating, true));
                                }
                            }
                        }
                    }

                    if (bRestrictions)
                    {
                        if (infos().trait(eTrait).mbNoMarry)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_MARRY");
                        }

                        if (infos().trait(eTrait).mbNoBirth)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_BIRTH");
                        }

                        if (infos().trait(eTrait).mbNoSuccession)
                        {
                            if ((pCharacter != null) && pCharacter.isLeader())
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_FORFEITS");
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_HEIR", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                            }
                        }

                        if (infos().trait(eTrait).mbNoSuccessionChildren)
                        {
                            if ((pCharacter != null) && pCharacter.isLeader())
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_FORFEITS");
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_HEIR_CHILDREN");
                            }
                        }

                        if (infos().trait(eTrait).mbNoJob)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_JOB_LIST", buildAllJobsLinkVariable(pCharacter, pGame));
                        }

                        if (infos().trait(eTrait).mbNoCouncil)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_COUNCIL");
                        }

                        if (infos().trait(eTrait).mbNoGeneral)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_GENERAL", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoGovernor)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_GOVERNOR", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoCourtier)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_COURTIER", buildCourtierLinkVariable(eGender: pCharacter != null ? pCharacter.getGender() : GenderType.NONE), TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoFamilyHead)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_FAMILY_HEAD");
                        }

                        if (infos().trait(eTrait).mbNoReligionHeadNew)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_RELIGION_HEAD_NEW", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoReligion)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_RELIGION");
                        }

                        if (infos().trait(eTrait).mbNoEvents)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_EVENTS", buildTraitLinkVariable(eTrait, pCharacter));
                        }

                        if (infos().trait(eTrait).mbGiveBirth)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_GIVE_BIRTH");
                        }

                        if (infos().trait(eTrait).mbStrength)
                        {
                            if (pCharacter != null)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_STRENGTH_COUNT", pCharacter.countStrengths());
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_STRENGTH");
                            }
                        }

                        if (infos().trait(eTrait).mbWeakness)
                        {
                            if (pCharacter != null)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_WEAKNESS_COUNT", pCharacter.countWeaknesses());
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_WEAKNESS");
                            }
                        }
                    }
                }

                if (bDetails)
                {
                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (subText.BeginScope(scopeType))
                        {
                            int iDeathProb = infos().utils().modify(infos().Helpers.getTraitDieProb(eTrait, pGame), infos().trait(eTrait).miDieModifier);
                            {
                                if (iDeathProb > 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_DEATH_CHANCE", iDeathProb);
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miDieModifier;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_DIE_MODIFIER", buildSignedTextVariable(iValue, false));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miRemoveTurns;
                                if (iValue > 0)
                                {
                                    if ((pCharacter != null) && pCharacter.isTrait(eTrait))
                                    {
                                        int iTurns = (iValue - pCharacter.getTraitTurnLength(eTrait));
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_REMOVE_TURNS_CHARACTER", iTurns, buildTurnScaleName(pGame, iTurns));
                                    }
                                    else
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_REMOVE_TURNS", iValue, buildTurnScaleName(pGame, iValue));
                                    }
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miBirthModifier;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_BIRTH_CHANCE", buildSignedTextVariable(iValue));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miFamilyHeadModifier;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_FAMILY_HEAD_CHANCE", buildSignedTextVariable(iValue));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miReligionHeadModifier;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_HEAD_CHANCE", buildSignedTextVariable(iValue));
                                }
                            }

                            int iReplaceProbability = 0;
                            for (TraitType eLoopTrait = 0; eLoopTrait < infos().traitsNum(); eLoopTrait++)
                            {
                                int iValue = infos().utils().modify(infos().trait(eTrait).maiTraitProb[eLoopTrait], -iDeathProb);
                                if (iValue > 0 && (pCharacter == null || !pCharacter.isTrait(eLoopTrait)))
                                {
                                    iReplaceProbability += infos().trait(eTrait).maiTraitProb[eLoopTrait];
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OTHER_TRAIT_CHANCE", TEXTVAR(iValue), buildTraitLinkVariable(eLoopTrait, pCharacter));
                                }
                            }

                            {
                                int iValue = infos().utils().modify(infos().trait(eTrait).miRemoveProb, -iDeathProb);
                                iValue = infos().utils().modify(iValue, -iReplaceProbability);
                                if (iValue > 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_REMOVE_CHANCE", iValue, buildTraitLinkVariable(eTrait, pCharacter));
                                }
                            }

                            for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                            {
                                int iValue = infos().trait(eTrait).maiReligionOpinion[eLoopReligion];
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_OPINION", buildSignedTextVariable(iValue), buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer));
                                }
                            }

                            for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                            {
                                int iValue = infos().trait(eTrait).maiReligionOpinionWeighted[eLoopReligion];
                                if (iValue != 0)
                                {
                                    if (pCharacter != null)
                                    {
                                        int iChange = infos().Helpers.getReligionOpinionWeightedTrait(eLoopReligion, eTrait, pCharacter.getPlayerOpinionCharacter());
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_OPINION_WEIGHTED_CHARACTER", buildSignedTextVariable(iChange), buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer), buildSignedTextVariable(iValue));
                                    }
                                    else
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_OPINION_WEIGHTED", buildSignedTextVariable(iValue), buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer));
                                    }
                                }
                            }

                            for (FamilyClassType eLoopFamilyClass = 0; eLoopFamilyClass < infos().familyClassesNum(); eLoopFamilyClass++)
                            {
                                int iValue = infos().trait(eTrait).maiFamilyClassOpinion[eLoopFamilyClass];
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_FAMILY_CLASS_OPINION", buildSignedTextVariable(iValue), buildFamilyClassLinkVariable(eLoopFamilyClass));
                                }
                            }

                            for (LawType eLoopLaw = 0; eLoopLaw < infos().lawsNum(); eLoopLaw++)
                            {
                                int iValue = infos().trait(eTrait).maiLawOpinion[eLoopLaw];
                                if (iValue > 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_LAW_OPINION", buildSignedTextVariable(iValue), buildLawLinkVariable(eLoopLaw));
                                }
                            }

                            for (JobType eLoopJob = 0; eLoopJob < infos().jobsNum(); eLoopJob++)
                            {
                                if (pGame != null ? pGame.checkGameContent(eLoopJob) : App.CheckContentOwnership(infos().job(eLoopJob).meGameContentRequired, infos(), null))
                                {
                                    int iValue = infos().trait(eTrait).maiJobOpinion[eLoopJob];
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_JOB_OPINION", buildSignedTextVariable(iValue), buildJobLinkVariable(eLoopJob));
                                    }
                                }
                            }

                            for (TraitType eLoopTrait = 0; eLoopTrait < infos().traitsNum(); eLoopTrait++)
                            {
                                int iValue = infos().trait(eTrait).maiTraitOpinion[eLoopTrait];
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OTHER_TRAIT_OPINION", buildSignedTextVariable(iValue), buildTraitLinkVariable(eLoopTrait, pCharacter));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miOpinionSame;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OTHER_TRAIT_OPINION", buildSignedTextVariable(iValue), buildTraitLinkVariable(eTrait, pCharacter));
                                }
                            }

                            if ((pCharacter != null) ? !(pCharacter.isLeader()) : true)
                            {
                                int iValue = infos().trait(eTrait).miOpinion;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION", buildSignedTextVariable(iValue));
                                }
                            }

                            if ((pCharacter != null) ? pCharacter.hasFamily() : true)
                            {
                                int iValue = infos().trait(eTrait).miOpinionFamily;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_FAMILY", buildSignedTextVariable(iValue));
                                }
                            }

                            if ((pCharacter != null) ? pCharacter.isLeader() : true)
                            {
                                {
                                    int iValue = infos().trait(eTrait).miOpinionReligion;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_RELIGION", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionProximity;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_PROXIMITY", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionStrength;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_STRENGTH", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionKnowledge;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_KNOWLEDGE", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionGenerals;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_GENERALS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionGovernors;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_GOVERNORS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionWonders;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_WONDERS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionLaws;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_LAWS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionCognomen;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_COGNOMEN", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionTrades;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_TRADES", buildSignedTextVariable(iValue));
                                    }
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miReligionPaganOpinion;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_PAGAN_OPINION", buildSignedTextVariable(iValue));
                                }
                            }

                            {
                                using (TextBuilder ratingText = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                    {
                                        int iValue = infos().trait(eTrait).maiRatingFallback[eLoopRating];
                                        if (iValue != 0)
                                        {
                                            TextVariable ratingValueText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildSignedTextVariable(iValue), buildRatingLinkVariable(eLoopRating, true));
                                            ratingText.Add(ratingValueText);
                                        }
                                    }

                                    if (ratingText.HasContent)
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_RATING_FALLBACK_LIST")))
                                        {
                                            subText.Add(ratingText.ToTextVariable());
                                        }
                                    }
                                }
                            }

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TRAIT_INCLUDED_IN")))
                            {
                                for (SubjectType eLoopSubject = 0; eLoopSubject < infos().subjectsNum(); eLoopSubject++)
                                {
                                    if (infos().subject(eLoopSubject).maeTraitAny.Contains(eTrait) && !infos().subject(eLoopSubject).mbHidden)
                                    {
                                        subText.Add(buildSubjectTraitAnyLinkVariable(eLoopSubject, pCharacter != null ? pCharacter.getGender() : GenderType.NONE));
                                    }
                                }
                            }

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TRAIT_EXCUDED_FROM")))
                            {
                                for (SubjectType eLoopSubject = 0; eLoopSubject < infos().subjectsNum(); eLoopSubject++)
                                {
                                    if (infos().subject(eLoopSubject).maeTraitNone.Contains(eTrait) && !infos().subject(eLoopSubject).mbHidden)
                                    {
                                        subText.Add(buildSubjectTraitNoneLinkVariable(eLoopSubject, pCharacter != null ? pCharacter.getGender() : GenderType.NONE));
                                    }
                                }
                            }

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TRAIT_COMMON_FAMILYCLASS")))
                            {
                                if (pActivePlayer == null || !pActivePlayer.hasNation())
                                {
                                    for (FamilyClassType eLoopFamilyClass = 0; eLoopFamilyClass < infos().familyClassesNum(); ++eLoopFamilyClass)
                                    {
                                        int iValue = infos().familyClass(eLoopFamilyClass).maiTraitDie[eTrait];
                                        if (iValue > 1)
                                        {
                                            subText.Add(buildFamilyClassLinkVariable(eLoopFamilyClass));
                                        }
                                    }
                                }
                                else if (pGame != null)
                                {
                                    for (FamilyType eLoopFamily = 0; eLoopFamily < infos().familiesNum(); ++eLoopFamily)
                                    {
                                        if (infos().family(eLoopFamily).mabNation[(int)(pActivePlayer.getNation())])
                                        {
                                            FamilyClassType eFamilyClass = pGame.getFamilyClass(eLoopFamily);
                                            if (eFamilyClass != FamilyClassType.NONE)
                                            {
                                                int iValue = infos().familyClass(eFamilyClass).maiTraitDie[eTrait];
                                                if (iValue > 1)
                                                {
                                                    subText.Add(buildFamilyLinkVariable(eLoopFamily, pGame));
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            {
                                if (infos().trait(eTrait).meEncyclopediaCharacter != CharacterType.NONE)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_UNIQUE_CHARACTER", buildCharacterPresetLinkVariable(infos().trait(eTrait).meEncyclopediaCharacter, pActivePlayer));
                                }
                            }

                            if (pGame != null)
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.INDENTED_BULLET, insertBeforeScopeText: TEXTVAR_TYPE("TEXT_HELPTEXT_NOTABLE_CHARACTERS")))
                                {
                                    using (var hashSetScope = CollectionCache.GetHashSetScoped<Character>())
                                    using (var charListScoped = CollectionCache.GetListScoped<Character>())
                                    {
                                        HashSet<Character> spCharactersAdded = hashSetScope.Value;

                                        pGame.getActiveCharacters(charListScoped.Value);

                                        for (int iPass = 0; iPass < InfoHelpers.NOTABLE_CHARACTER_PASSES; iPass++)
                                        {
                                            foreach (Character pLoopCharacter in charListScoped.Value)
                                            {
                                                if (!(spCharactersAdded.Contains(pLoopCharacter)))
                                                {
                                                    if (pLoopCharacter.hasContact(pActivePlayer.getTeam()))
                                                    {
                                                        if (infos().Helpers.isNotableCharacter(pLoopCharacter, iPass, pActivePlayer))
                                                        {
                                                            if (pLoopCharacter.isTrait(eTrait))
                                                            {
                                                                subText.Add(buildCharacterLinkVariable(pLoopCharacter, pActivePlayer));
                                                            }

                                                            spCharactersAdded.Add(pLoopCharacter);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (subText.HasContent)
                        {
                            buildDividerText(builder);
                            builder.Add(subText.ToTextVariable());
                        }
                    }
                }

                if (infos().trait(eTrait).meDescription != TextType.NONE)
                {
                    buildDividerText(builder);

                    using (buildColorTextPositiveScope(builder))
                    {
                        builder.AddTEXT(infos().trait(eTrait).meDescription);
                    }
                }

                return builder;
            }
        }




        /*####### Better Old World AI - Base DLL #######
          ### misc                             START ###
          ##############################################*/
        //public struct BetterAICommaListVariableGenerator : CommaListVariableGenerator
        //{
        //}
        //fuck me, structs can't inherit
        //I can't overrride a struct but at least inside the derived BetterAIHelpText class I can call this for proper japanese list generation

        //lines 44213-44336
        new public struct CommaListVariableGenerator
        {
            public enum ListType
            {
                NONE = 0,
                OR,
                AND,
                LINEBREAK,
                OR_LINEBREAK,
                AND_LINEBREAK
            }
            public enum EncloseType
            {
                NONE = 0,
                PARENTHESIS,
                PARENTHESIS_OUTER
            }

            private ListType listType;
            private EncloseType encloseType;
            private int numItems;
            private bool bEncloseSingleItem;
            private TextVariable result;
            private TextVariable prevItem;
            private TextManager textManager;
            private string firstItemTextType;
            private string separatorTextType;
            private string lastItemSeparatorTextType;

            public int Count { get { return numItems; } }

            public CommaListVariableGenerator(string firstItemTextType, string listSeparatorTextType, string lastItemSeparatorTextType, TextManager textManager)
            {
                this.listType = ListType.NONE;
                this.encloseType = EncloseType.NONE;
                this.textManager = textManager;
                this.bEncloseSingleItem = false;

                numItems = 0;
                prevItem = null;
                result = null;
                this.firstItemTextType = firstItemTextType;
                separatorTextType = listSeparatorTextType;
                this.lastItemSeparatorTextType = lastItemSeparatorTextType;
            }
            public CommaListVariableGenerator(string firstItemTextType, string listSeparatorTextType, TextManager textManager)
            {
                this.listType = ListType.NONE;
                this.encloseType = EncloseType.NONE;
                this.textManager = textManager;
                this.bEncloseSingleItem = false;

                numItems = 0;
                prevItem = null;
                result = null;
                this.firstItemTextType = firstItemTextType;
                separatorTextType = listSeparatorTextType;
                this.lastItemSeparatorTextType = separatorTextType;
            }

            public CommaListVariableGenerator(ListType listType, EncloseType encloseType, bool bEncloseSingleItem, TextManager textManager)
            {
                this.listType = listType;
                this.encloseType = encloseType;
                this.textManager = textManager;
                this.bEncloseSingleItem = bEncloseSingleItem;

                numItems = 0;
                prevItem = null;
                result = null;
                firstItemTextType = null;
                separatorTextType = "TEXT_HELPTEXT_COMMA_SPACE_TWO";
                lastItemSeparatorTextType = separatorTextType;
            }

            public CommaListVariableGenerator(ListType listType, EncloseType encloseType, TextManager textManager)
            {
                this.listType = listType;
                this.encloseType = encloseType;
                this.textManager = textManager;
                this.bEncloseSingleItem = false;

                numItems = 0;
                prevItem = null;
                result = null;
                firstItemTextType = null;
                separatorTextType = "TEXT_HELPTEXT_COMMA_SPACE_TWO";
                lastItemSeparatorTextType = separatorTextType;
            }

            public CommaListVariableGenerator(ListType listType, TextManager textManager)
            {
                this.listType = listType;
                this.encloseType = EncloseType.NONE;
                this.textManager = textManager;
                this.bEncloseSingleItem = false;

                numItems = 0;
                prevItem = null;
                result = null;
                firstItemTextType = null;
                separatorTextType = "TEXT_HELPTEXT_COMMA_SPACE_TWO";
                lastItemSeparatorTextType = separatorTextType;
            }

            public void AddItem(TextVariable item)
            {
                if (prevItem != null)
                {
                    if (numItems == 1)
                    {
                        if (encloseType != EncloseType.NONE)
                        {
                            switch (encloseType)
                            {
                                case EncloseType.PARENTHESIS: firstItemTextType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                                case EncloseType.PARENTHESIS_OUTER: firstItemTextType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                                default: firstItemTextType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            }
                        }

                        if (string.IsNullOrEmpty(firstItemTextType))
                        {
                            result = prevItem;

                        }
                        else
                        {
                            result = TEXTVAR(firstItemTextType, textManager, prevItem);
                        }
                    }
                    else
                    {
                        //new code for fixed japanese lists
                        string textType;
                        if (encloseType != EncloseType.NONE)
                        {
                            switch (encloseType)
                            {
                                case EncloseType.PARENTHESIS: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                                case EncloseType.PARENTHESIS_OUTER: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                                default: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            }
                            prevItem = TEXTVAR(textType, textManager, prevItem);
                        }
    
                        switch (listType)
                        {
                            case ListType.AND: textType = "TEXT_HELPTEXT_AND_MIDLIST"; break;
                            case ListType.OR: textType = "TEXT_HELPTEXT_OR_MIDLIST"; break;
                            case ListType.AND_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_AND_MIDLIST"; break;
                            case ListType.OR_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_OR_MIDLIST"; break;
                            case ListType.LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_TWO"; break;
                            default: textType = separatorTextType; break;
                        }
                        result = TEXTVAR(textType, textManager, result, prevItem);
                        //end of new code

                        //old code
                        //result = TEXTVAR(separatorTextType, textManager, result, prevItem);
                    }
                }

                prevItem = item;
                numItems++;
            }

            public TextVariable Finalize()
            {
                if (numItems == 0)
                    return TEXTVAR("");

                if (numItems == 1)
                {
                    if (string.IsNullOrEmpty(firstItemTextType) && !bEncloseSingleItem)
                    {
                        result = prevItem;
                        //no enclosing if there's just 1 item
                    }
                    else
                    {
                        if (encloseType != EncloseType.NONE)
                        {
                            string textType;
                            switch (encloseType)
                            {
                                case EncloseType.PARENTHESIS: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                                case EncloseType.PARENTHESIS_OUTER: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                                default: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            }
                            result = TEXTVAR(textType, textManager, prevItem);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(firstItemTextType)) //implies bEcloseSingleItem
                            {
                                result = prevItem; //no EncloseType chosen, no firstItemTextType set
                            }
                            else
                            {
                                result = TEXTVAR(firstItemTextType, textManager, prevItem);
                            }
                        }
                    }
                }
                else if (numItems == 2)
                {
                    string textType;

                    if (encloseType != EncloseType.NONE)
                    {
                        switch (encloseType)
                        {
                            case EncloseType.PARENTHESIS: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            case EncloseType.PARENTHESIS_OUTER: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                            default: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                        }
                        prevItem = TEXTVAR(textType, textManager, prevItem);
                    }

                    switch (listType)
                    {
                        case ListType.AND: textType = "TEXT_HELPTEXT_AND_TWO"; break;
                        case ListType.OR: textType = "TEXT_HELPTEXT_OR_TWO"; break;
                        case ListType.AND_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_AND_TWO"; break;
                        case ListType.OR_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_OR_TWO"; break;
                        case ListType.LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_TWO"; break;
                        default: textType = separatorTextType; break;
                    }
                    result = TEXTVAR(textType, textManager, result, prevItem);
                }
                else
                {
                    string textType;
                    if (encloseType != EncloseType.NONE)
                    {
                        switch (encloseType)
                        {
                            case EncloseType.PARENTHESIS: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            case EncloseType.PARENTHESIS_OUTER: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                            default: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                        }
                        prevItem = TEXTVAR(textType, textManager, prevItem);
                    }

                    switch (listType)
                    {
                        case ListType.AND: textType = "TEXT_HELPTEXT_COMMA_AND_TWO"; break;
                        case ListType.OR: textType = "TEXT_HELPTEXT_COMMA_OR_TWO"; break;
                        case ListType.AND_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_AND_TWO"; break;
                        case ListType.OR_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_OR_TWO"; break;
                        case ListType.LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_TWO"; break;
                        default: textType = lastItemSeparatorTextType; break;
                    }
                    result = TEXTVAR(textType, textManager, result, prevItem);
                }

                return result;
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### misc                               END ###
  ##############################################*/

    }
}
