using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System;
using System.Collections.Generic;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using static TenCrowns.GameCore.Text.TextExtensions;

namespace BetterAI
{
    public partial class BetterAIHelpText : HelpText
    {

        //lines 1064-1078
        public override TextVariable buildHappinessLevelLinkVariable(City pCity, bool bShort = false)
        {
            //UnityEngine.Debug.Log("HelpText.buildHappinessLevelLinkVariable");

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
        //public virtual TextVariable buildIgnoreZOCLinkVariable()
        //{
        //    return buildConceptLinkVariable("CONCEPT_IGNORES_ZOC");
        //}
        //lines 1110-1113
        public virtual TextVariable buildIgnoreZOCWithExceptionsLinkVariable(UnitType eUnit)
        {
            //UnityEngine.Debug.Log("HelpText.buildIgnoreZOCWithExceptionsLinkVariable");

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

        //lines 5385-5466
        public override TextBuilder buildTileDebugText(TextBuilder builder, Tile pTile, ClientManager pManager)
        {
            //UnityEngine.Debug.Log("HelpText.buildTileDebugText");

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
                //    builder.Add(QUICKTEXTVAR(TEXT("TEXT_HELPTEXT_AI_VALUE") + ": {0} ({1})*", iAIValue, TEXTVAR_TYPE(mInfos.Helpers.getImprovementName(eBestImprovement, pTile.game()))));
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

                if (pTile.isValidFoundLocation(pManager.getActiveTeam(), TeamType.NONE, false))
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

        //lines 12816-12956
        public override TextBuilder buildResourceHelp(TextBuilder builder, ResourceType eResource, Game pGame, Player pPlayer, Tile pTile, Player pActivePlayer, bool bName = true, bool bEncyclopedia = false)
        {
            //UnityEngine.Debug.Log("HelpText.buildResourceHelp");

            //using (new UnityProfileScope("HelpText.buildResourceHelp"))
            {
                City pCityTerritory = pTile?.cityTerritory();
                bool bShow = (pGame?.checkGameContent(eResource) ?? mInfos.modSettings().App.CheckContentOwnership(infos().resource(eResource).meGameContentRequired, mInfos, null));

                if (bName)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_NAME", TEXTVAR_TYPE(infos().resource(eResource).mName), TEXTVAR((pTile != null) ? pTile.isHarvested() : false));
                }

                if (bShow)
                {

                    if (pPlayer != null)
                    {
                        for (FamilyType eLoopFamily = 0; eLoopFamily < infos().familiesNum(); eLoopFamily++)
                        {
                            if (pPlayer.isFamilyStarted(eLoopFamily))
                            {
                                int iValue = pGame.familyClass(eLoopFamily).maiLuxuryMissingOpinion[eResource];
                                if (iValue != 0)
                                {
                                    builder.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_MISSING_OPINION", buildFamilyLinkVariable(eLoopFamily, pGame), TEXTVAR(iValue)), !pPlayer.isFamilyLuxury(eLoopFamily, eResource)));
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

                            if (pCityTerritory != null)
                            {
                                EffectCityType eLoopEffectCity = infos().improvementClass(eLoopImprovementClass).maeResourceCityEffect[eResource];
                                if (eLoopEffectCity != EffectCityType.NONE)
                                {
                                    foreach (EffectCityType eOtherEffectCity in pCityTerritory.getActiveEffectCity())
                                    {
                                        EffectCityType eEffectCity = infos().effectCity(eOtherEffectCity).maeEffectCityEffectCity[eLoopEffectCity];
                                        if (eEffectCity != EffectCityType.NONE)
                                        {
                                            builder.Add(buildEffectCityLinkVariable(eEffectCity, pCityTerritory, pCityTerritory?.governor()));
                                        }
                                    }
                                }
                            }

                            {
                                EffectCityType eEffectCity = infos().improvementClass(eLoopImprovementClass).maeResourceCityEffect[eResource];
                                if (eEffectCity != EffectCityType.NONE)
                                {
                                    buildEffectCityHelp(builder, eEffectCity, pCityTerritory, pActivePlayer, bSkipImpossible: !bEncyclopedia, bShowTotal: true, bSkipZero: !bEncyclopedia);
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
                                        if (pCityTerritory == null || !pCityTerritory.isVoidTechPrereqUnlock(eLoopImprovementClass))
                                        {
                                            builder.AddWithParenthesis(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(ePrereqTech))));
                                        }
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
                                buildEffectCityHelp(builder, eEffectCity, pCityTerritory, pActivePlayer, bSkipImpossible: !bEncyclopedia, bShowTotal: true, bSkipZero: !bEncyclopedia);
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
                }


                for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < infos().effectCitiesNum(); eLoopEffectCity++)
                {
                    if (infos().effectCity(eLoopEffectCity).maeLuxuryResources.Contains(eResource) && infos().Helpers.canEverHaveEffectCity(pPlayer, eLoopEffectCity))
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_FROM", buildEffectCitySourceLinkVariable(eLoopEffectCity, null, null, pGame, pActivePlayer, null, true));
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### show total Resource count on map START ###
  ##############################################*/
                if (pGame != null && bShow && ( (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT + ((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT + ((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES) > 0 ) )
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
                                            TextVariable name;
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

        //lines 8238-8507
        public override TextBuilder buildLawHelp(TextBuilder builder, LawType eLaw, Game pGame, Player pPlayer, Player pActivePlayer, bool bName = true, bool bDetails = true, bool bEncyclopedia = false, TextBuilder.ScopeType scopeType = TextBuilder.ScopeType.NONE)
        {
            //UnityEngine.Debug.Log("HelpText.buildLawHelp");

            //using (new UnityProfileScope("HelpText.buildLawHelp"))
            {
                LawClassType eLawClass = infos().law(eLaw).meLawClass;

                if (bEncyclopedia)
                {
                    TextType eDescription = infos().law(eLaw).meDescription;

                    if (eDescription != TextType.NONE)
                    {
                        builder.AddTEXT(eDescription);
                        buildDividerText(builder);
                    }
                }

                if (bName)
                {
                    TextVariable showCurrent = TEXTVAR(bDetails && pPlayer != null && pPlayer.getActiveLaw(eLawClass) == eLaw);
                    builder.AddTEXT("TEXT_HELPTEXT_LAW_HELP_NAME", buildTitleVariable(TEXTVAR_TYPE(infos().law(eLaw).mName)), showCurrent);
                }

                using (builder.BeginScope(scopeType))
                {
                    {
                        SuccessionOrderType eSuccessionOrder = infos().law(eLaw).meSuccessionOrder;
                        SuccessionGenderType eSuccessionGender = infos().law(eLaw).meSuccessionGender;

                        using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                        {
                            if (eSuccessionOrder != SuccessionOrderType.NONE)
                            {
                                builder.Add(TEXT(infos().successionOrder(eSuccessionOrder).mHelp));
                            }

                            if (eSuccessionGender != SuccessionGenderType.NONE)
                            {
                                builder.Add(TEXT(infos().successionGender(eSuccessionGender).mHelp));
                            }
                        }

                        if (pPlayer != null)
                        {
                            if ((eSuccessionOrder != SuccessionOrderType.NONE) || (eSuccessionGender != SuccessionGenderType.NONE))
                            {
                                SuccessionOrderType eTestSuccessionOrder = ((eSuccessionOrder != SuccessionOrderType.NONE) ? eSuccessionOrder : pPlayer.getSuccessionOrder());
                                SuccessionGenderType eTestSuccessionGender = ((eSuccessionGender != SuccessionGenderType.NONE) ? eSuccessionGender : pPlayer.getSuccessionGender());

                                buildSuccessionHelp(builder, pGame, pPlayer, eTestSuccessionOrder, eTestSuccessionGender, pActivePlayer);
                            }
                        }
                    }

                    {
                        LawType ePrereqLaw = infos().law(eLaw).meLawPrereq;
                        if (ePrereqLaw != LawType.NONE)
                        {
                            using (buildWarningTextScope(builder, pActivePlayer.isActiveLaw(ePrereqLaw)))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_REQUIRES", buildLawLinkVariable(ePrereqLaw, pPlayer));
                            }
                        }
                    }

                    {
                        EffectPlayerType eEffectPlayer = infos().law(eLaw).meEffectPlayer;

                        if (eEffectPlayer != EffectPlayerType.NONE)
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                            {
                                buildEffectPlayerHelp(builder, eEffectPlayer, pGame, pPlayer, pActivePlayer, eStateReligion: pPlayer?.getStateReligion() ?? ReligionType.NONE, bShowTotal: bDetails);
                            }
                        }
                    }

                    if (bDetails)
                    {
                        {
                            EffectPlayerType eEffectPlayerUpkeep = infos().law(eLaw).meEffectPlayerUpkeep;

                            if (eEffectPlayerUpkeep != EffectPlayerType.NONE)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LAW_HELP_EFFECT_UPKEEP", buildEffectPlayerLinkVariable(eEffectPlayerUpkeep, pPlayer));

                                if (pPlayer != null && pPlayer == pActivePlayer)
                                {
                                    using (TextBuilder andList = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (andList.BeginScope(TextBuilder.ScopeType.COMMA_AND))
                                        {
                                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                            {
                                                int iValue = infos().effectPlayer(eEffectPlayerUpkeep).maiYieldUpkeep[eLoopYield];
                                                iValue = infos().utils().modify(iValue, pPlayer.getYieldUpkeepModifier());
                                                if (iValue != 0)
                                                {
                                                    andList.AddTEXT("TEXT_HELPTEXT_YIELD_PER_YEAR", buildYieldValueIconLinkVariable(eLoopYield, (iValue * pPlayer.getNumCities()), iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));
                                                }
                                            }
                                        }

                                        if (andList.HasContent)
                                        {
                                            builder.AddWithParenthesis(andList.ToTextVariable());
                                        }
                                    }
                                }
                            }
                        }

                        if (pActivePlayer == null || pActivePlayer.isPlayerOption(infos().Globals.ADVANCED_HELP))
                        {
                            for (TraitType eLoopTrait = 0; eLoopTrait < infos().traitsNum(); eLoopTrait++)
                            {
                                int iValue = infos().trait(eLoopTrait).maiLawOpinion[eLaw];
                                if (iValue != 0)
                                {
                                    builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_OPINION_COLON", buildTraitLinkVariable(eLoopTrait), buildSignedTextVariable(iValue)));
                                }
                            }

                            for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                            {
                                int iValue = infos().religion(eLoopReligion).maiLawOpinion[eLaw];
                                if (iValue != 0)
                                {
                                    builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_OPINION_COLON", buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer), buildSignedTextVariable(iValue)));
                                }
                            }

                            for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                            {
                                int iValue = infos().theology(eLoopTheology).maiLawOpinion[eLaw];
                                if (iValue != 0)
                                {
                                    TextVariable theologyText = TEXTVAR_TYPE("TEXT_HELPTEXT_OPINION_COLON", buildTheologyLinkVariable(eLoopTheology, ReligionType.NONE, pPlayer), buildSignedTextVariable(iValue));

                                    if (pGame != null && pPlayer != null && pPlayer == pActivePlayer)
                                    {
                                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                        {
                                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                            {
                                                for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                                                {
                                                    if (pGame.isReligionTheology(eLoopReligion, eLoopTheology))
                                                    {
                                                        subText.Add(buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer));
                                                    }
                                                }
                                            }

                                            if (subText.HasContent)
                                            {
                                                theologyText = buildEnclosedParenthesis(theologyText, subText.ToTextVariable());
                                            }
                                        }
                                    }

                                    builder.Add(theologyText);
                                }
                            }

                            if (pActivePlayer != null)
                            {
                                foreach (FamilyType eLoopFamily in pActivePlayer.getFamilies())
                                {
                                    int iValue = pActivePlayer.getFamilyLawOpinion(eLoopFamily, eLaw) + pGame.familyClass(eLoopFamily).maiLawOpinion[eLaw];
                                    if (iValue != 0)
                                    {
                                        builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_OPINION_COLON", buildFamilyLinkVariable(eLoopFamily, pGame), buildSignedTextVariable(iValue)));
                                    }
                                }
                            }
                            else
                            {
                                for (FamilyClassType eLoopFamilyClass = 0; eLoopFamilyClass < infos().familyClassesNum(); eLoopFamilyClass++)
                                {
                                    int iValue = infos().familyClass(eLoopFamilyClass).maiLawOpinion[eLaw];
                                    if (iValue != 0)
                                    {
                                        builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_OPINION_COLON", buildFamilyClassLinkVariable(eLoopFamilyClass), buildSignedTextVariable(iValue)));
                                    }
                                }
                            }
                        }

                        if (pPlayer != null && pPlayer == pActivePlayer)
                        {
                            {
                                int iCost = pPlayer.getStartLawCost(eLaw);

                                using (buildWarningTextScope(builder, (pPlayer.getYieldStockpileWhole(infos().Globals.CIVICS_YIELD) < iCost)))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LAW_HELP_COST", buildYieldValueIconLinkVariable(infos().Globals.CIVICS_YIELD, iCost));
                                }
                            }

                            if ((infos().law(eLaw).meSuccessionOrder != SuccessionOrderType.NONE) ||
                                (infos().law(eLaw).meSuccessionGender != SuccessionGenderType.NONE))
                            {
                                if (!(pPlayer.isLeaderAlive()))
                                {
                                    using (buildWarningTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_LAW_HELP_SUCCESSION_LEADER_DEAD");
                                    }
                                }
                            }
                        }

                        {
                            TechType eTechPrereq = infos().lawClass(eLawClass).meTechPrereq;

                            if (eTechPrereq != TechType.NONE)
                            {
                                if ((pPlayer != null) && !(pPlayer.isTechAcquired(eTechPrereq)) && pPlayer == pActivePlayer)
                                {

/*####### Better Old World AI - Base DLL #######
  ### Law: remaining Techs to research START ###
  ##############################################*/
                                    
                                    bool bUnavailable = false;
                                    int iRemainingTechsToUnlockCount = 0;
                                    int iRemainingScience = 0;

                                    if (pPlayer.isTechTrashed(eTechPrereq) || pPlayer.isTechLocked(eTechPrereq))
                                    {
                                        bUnavailable = true;
                                    }
                                    else
                                    {
                                        if (!pPlayer.isTechAnyDeck(eTechPrereq))
                                        {
                                            //look for other techs
                                            using (var hashSetScope = CollectionCache.GetHashSetScoped<TechType>())
                                            {
                                                HashSet<TechType> seRequiredTechs = hashSetScope.Value;

                                                seRequiredTechs.Add(eTechPrereq);
                                                ((BetterAIPlayer)pPlayer).getUnlockTechCount(eTechPrereq, ref seRequiredTechs, ref bUnavailable);

                                                iRemainingTechsToUnlockCount = seRequiredTechs.Count - 1;

                                                foreach (TechType eLoopTech in seRequiredTechs)
                                                {
                                                    iRemainingScience += pPlayer.getTechCostWhole(eLoopTech) - pPlayer.getTechProgress(eLoopTech);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            iRemainingScience = pPlayer.getTechCostWhole(eTechPrereq) - pPlayer.getTechProgress(eTechPrereq);
                                        }
                                    }

                                    TextVariable extraText;
                                    if (bUnavailable)
                                    {
                                        extraText = TEXTVAR_TYPE("TEXT_HELPTEXT_TECH_UNAVAILABLE");
                                    }
                                    else if (iRemainingTechsToUnlockCount <= 0)
                                    {
                                        if (pPlayer.getTechResearching() == eTechPrereq)
                                        {
                                            extraText = TEXTVAR_TYPE("TEXT_HELPTEXT_TECH_RESEARCHING", iRemainingScience);
                                        }
                                        else if (pPlayer.isTechAvailable(eTechPrereq))
                                        {
                                            extraText = TEXTVAR_TYPE("TEXT_HELPTEXT_TECH_IN_HAND", iRemainingScience);
                                        }
                                        else if (pPlayer.canMakeTechAvailable(eTechPrereq))
                                        {
                                            extraText = TEXTVAR_TYPE("TEXT_HELPTEXT_TECH_DRAW_PILE", iRemainingScience);
                                        }
                                        else // if (pPlayer.isTechPassed(eTechPrereq))
                                        {
                                            extraText = TEXTVAR_TYPE("TEXT_HELPTEXT_TECH_DISCARD_PILE", iRemainingScience);
                                        }
                                    }
                                    else // if(iRemainingTechsToUnlockCount > 0)
                                    {
                                        extraText = TEXTVAR_TYPE("TEXT_HELPTEXT_TECH_MORE_TECHS_REQUIRED", iRemainingTechsToUnlockCount, iRemainingScience);
                                    }

                                    using (buildWarningTextScope(builder))
                                    {
                                        //builder.AddTEXT("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(eTechPrereq));
                                        builder.AddTEXT("TEXT_HELPTEXT_CONCAT_SPACE_TWO", TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(eTechPrereq)), 
                                            TEXTVAR_TYPE("TEXT_HELPTEXT_ENCLOSED_PARENTHESIS", extraText));
                                    }

/*####### Better Old World AI - Base DLL #######
  ### Law: remaining Techs to research   END ###
  ##############################################*/
                                }
                                else
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(eTechPrereq));
                                }
                            }

                            if (pPlayer != null && pPlayer == pActivePlayer)
                            {
                                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                    {
                                        foreach (EffectPlayerType eLoopEffectPlayer in infos().law(eLaw).maeEffectPlayerDisabled)
                                        {
                                            if (pPlayer.getEffectPlayerCount(eLoopEffectPlayer) > 0)
                                            {
                                                subText.Add(buildEffectPlayerLinkVariable(eLoopEffectPlayer, pPlayer));
                                            }
                                        }
                                    }

                                    if (subText.HasContent)
                                    {
                                        using (buildWarningTextScope(builder))
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_DISABLED_BY", subText.ToTextVariable());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (pGame != null)
                    {
                        if (pGame.manager().activePlayer().isPlayerOption(infos().Globals.DEBUG_HELP))
                        {
                            buildLawDebugText(builder, eLaw, pGame.manager());
                        }
                    }
                }

                return builder;
            }
        }

    }
}