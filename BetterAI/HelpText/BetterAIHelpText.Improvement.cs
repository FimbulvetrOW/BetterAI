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

        //lines 2230-2822
        public override void buildImprovementRequiresHelp(List<TextVariable> lRequirements, ImprovementType eImprovement, Game pGame, Player pActivePlayer, Tile pTile, Unit pUnit = null, bool bUpgradeImprovement = false)
        {
            //UnityEngine.Debug.Log("HelpText.buildImprovementRequiresHelp");

            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            {
                //here goes the copy-pasting
                //using (new UnityProfileScope("HelpText.buildImprovementRequiresHelp"))
                {
                    ImprovementClassType eImprovementClass = pImprovementInfo.meClass;

                    ReligionType eReligionPrereq = pImprovementInfo.meReligionPrereq;

                    BetterAICity pCityTerritory = ((pTile != null) ? (BetterAICity)pTile.cityTerritory() : null);
                    BetterAIInfoImprovementClass pInfoImprovementClass = (BetterAIInfoImprovementClass)infos().improvementClass(eImprovementClass);


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
                                if (pInfoImprovementClass.maeVoidTechPrereqFromEffectCities.Count == 0)
                                {
                                    //andList.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(ePrereqTech)), pActivePlayer != null));
                                    andList.AddItem(buildTechLinkVariable(ePrereqTech));
                                    bHasPrimaryUnlock = true;
                                }
                                else
                                {
                                    CommaListVariableGenerator orList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR, CommaListVariableGenerator.EncloseType.PARENTHESIS, TextManager);
                                    orList.AddItem(buildTechLinkVariable(ePrereqTech));

                                    foreach (EffectCityType eLoopEffectCity in pInfoImprovementClass.maeVoidTechPrereqFromEffectCities)
                                    {
                                        orList.AddItem(buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer, pTile));
                                    }
                                    andList.AddItem(orList.Finalize());
                                }
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

                        foreach (TerrainTargetType eTerrainTarget in infos().improvement(eImprovement).maeTerrainValid)
                        {
                            orList.AddItem(buildTerrainTargetLinkVariable(eTerrainTarget));
                        }

                        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                        {
                            if (infos().improvement(eLoopImprovement).maeAdjacentImprovementValid.Contains(eImprovement))
                            {
                                orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_HEIGHT", buildImprovementLinkVariable(eLoopImprovement, pGame, pTile)));
                            }
                        }

                        if (infos().improvement(eImprovement).mbRiverValid)
                        {
                            orList.AddItem(buildRiverLinkVariable(pTile));
                        }

                        if (infos().improvement(eImprovement).mbCityValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_CITY"));
                        }

                        if (infos().improvement(eImprovement).mbHolyCityValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_HOLY_CITY_ANY"));
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                if (infos().improvementClass(eImprovementClass).mabResourceValid[(int)eLoopResource])
                                {
                                    if (isContentEnabled(infos().resource(eLoopResource).meGameContentRequired, pGame))
                                    {
                                        orList.AddItem(buildResourceLinkVariable(eLoopResource, pTile));
                                    }
                                }
                            }
                        }

                        for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < mInfos.effectCitiesNum(); eLoopEffectCity++)
                        {
                            foreach ((TerrainTargetType, ImprovementType) pLoopPair in infos().effectCity(eLoopEffectCity).mlpTerrainImprovementValid)
                            {
                                if (pLoopPair.Item2 == eImprovement)
                                {
                                    orList.AddItem(buildSlashText(buildTerrainTargetLinkVariable(pLoopPair.Item1), buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor())));
                                }
                            }
                        }

                        if (orList.Count > 0)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", orList.Finalize()), ((pTile != null) && !(pTile.isImprovementValid(eImprovement, pCityTerritory)))));
                        }
                    }

                    {
                        int iRequiresLaws = pActivePlayer != null ? pActivePlayer.getImprovementLawsRequired(eImprovement) : infos().improvement(eImprovement).miPrereqLaws;

                        if (iRequiresLaws > 0)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_LAWS", iRequiresLaws), ((pActivePlayer != null) ? (pActivePlayer.countActiveLaws() < iRequiresLaws) : false)));
                        }
                    }

                    if (pTile == null && infos().improvement(eImprovement).mbNoAdjacentReligion)
                    {
                        lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT_RELIGION"));
                    }

                    if (pTile == null && eImprovementClass != ImprovementClassType.NONE && infos().improvementClass(eImprovementClass).mbNoAdjacent)
                    {
                        lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT", buildImprovementClassLinkVariable(eImprovementClass)));
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
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_IMPROVEMENT", buildImprovementLinkVariable(eImprovementPrereq, pGame, pTile)), ((pCityTerritory != null) ? (pCityTerritory.getActiveImprovementCount(eImprovementPrereq) == 0) : false)));
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        SpecialistType eAdjacentSpecialistPrereq = infos().improvement(eImprovement).meAdjacentSpecialistPrereq;

                        if (eAdjacentSpecialistPrereq != SpecialistType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_SPECIALIST", buildSpecialistLinkVariable(eAdjacentSpecialistPrereq, pGame, pTile)), ((pTile != null) ? !pTile.adjacentToSpecialist(eAdjacentSpecialistPrereq) : false)));
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        foreach (ImprovementClassType eAdjacentImprovementClassPrereq in infos().improvement(eImprovement).maeAdjacentImprovementClassAllPrereq)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_IMPROVEMENT_CLASS", buildImprovementClassLinkVariable(eAdjacentImprovementClassPrereq)), ((pTile != null) ? !pTile.adjacentToImprovementClassFinished(eAdjacentImprovementClassPrereq) : false)));
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


                    {
                        EffectPlayerType eEffectPlayerPrereq = infos().improvement(eImprovement).meEffectPlayerPrereq;

                        if (eEffectPlayerPrereq != EffectPlayerType.NONE && infos().Helpers.canEverHaveEffectPlayer(pActivePlayer, eEffectPlayerPrereq))
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildEffectPlayerSourceLinkVariable(eEffectPlayerPrereq, pGame, pActivePlayer)), ((pActivePlayer != null) ? (pActivePlayer.getEffectPlayerCount(eEffectPlayerPrereq) == 0) : false)));
                        }
                    }

                    if (pTile != null && pTile.hasVegetation() && infos().improvement(eImprovement).mbNoVegetation)
                    {
                        using (TextBuilder linkBuilder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            linkBuilder.AddTEXT("TEXT_HELPTEXT_TILE_TOOLTIP_REMOVE_VEGETATION", TEXTVAR_TYPE(pTile.vegetation().meName));

                            Player pTilePlayer = pTile.owner() ?? pActivePlayer;
                            if (pTile.vegetation().mbRequiresUnlock)
                            {
                                if (!pTilePlayer.isRemoveAllVegetationUnlock())
                                {
                                    using (buildWarningTextScope(linkBuilder))
                                    using (linkBuilder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_START_REQUIRES")))
                                    {
                                        for (EffectPlayerType eEffectPlayer = 0; eEffectPlayer < mInfos.effectPlayersNum(); ++eEffectPlayer)
                                        {
                                            if (mInfos.effectPlayer(eEffectPlayer).mbRemoveAllVegetation)
                                            {
                                                linkBuilder.Add(buildEffectPlayerSourceLinkVariable(eEffectPlayer, pGame, pActivePlayer));
                                            }
                                        }
                                    }
                                }
                                if (!pTile.hasRevealedCityTerritory(pActivePlayer?.getTeam() ?? TeamType.NONE))
                                {
                                    using (buildWarningTextScope(linkBuilder))
                                    {
                                        linkBuilder.AddTEXT("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_START_REQUIRES", buildConceptLinkVariable("CONCEPT_TERRITORY"));
                                    }
                                }
                            }

                            lRequirements.Add(linkBuilder.ToTextVariable());
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

                    
                    {
                        bool isTerrainCandidate(TerrainType eTerrain, bool bInvalid)
                        {
                            if (bInvalid != infos().Helpers.isImprovementTerrainInvalid(eImprovement, eTerrain))
                            {
                                return false;
                            }
                            if (infos().terrain(eTerrain).mbWater)
                            {
                                return false;
                            }
                            if (infos().terrain(eTerrain).mbUrban && !infos().improvement(eImprovement).mbUrban)
                            {
                                return false;
                            }
                            return true;
                        }
                        TerrainType eTerrainValid = TerrainType.NONE;
                        for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                        {
                            if (isTerrainCandidate(eLoopTerrain, bInvalid: false))
                            {
                                if (eTerrainValid != TerrainType.NONE)
                                {
                                    eTerrainValid = TerrainType.NONE;
                                    break;
                                }
                                eTerrainValid = eLoopTerrain;
                            }
                        }
                        if (eTerrainValid != TerrainType.NONE)
                        {
                            lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTerrainLinkVariable(eTerrainValid)));
                        }
                        else
                        {
                            for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                            {
                                if (isTerrainCandidate(eLoopTerrain, bInvalid: true) && (pTile == null || pTile.getTerrain() == eLoopTerrain))
                                {
                                    lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_TERRAIN", buildTerrainLinkVariable(eLoopTerrain))));
                                }
                            }
                        }
                    }

                    for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                    {
                        if (infos().Helpers.isImprovementHeightInvalid(eImprovement, eLoopHeight) && (pTile == null || pTile.getHeight() == eLoopHeight))
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
                                }
                            }

                            if (!bAdjacencyValid && infos().improvementClass(eImprovementClass).mbNoAdjacent && !(pTile.notAdjacentToImprovementClass(eImprovementClass)))
                            {
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT", buildImprovementClassLinkVariable(eImprovementClass))));
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
                                TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CULTURE_IMPROVEMENT_COUNT", TEXTVAR(iValue), buildImprovementClassLinkVariable(eImprovementClass), buildCultureLevelLinkVariable(pCityTerritory));
                                if (pCityTerritory != null)
                                {
                                    int maxImprovements = (int)pCityTerritory.getCulture() + (int)pCityTerritory.getCultureStep() + 1;
                                    reqItem = buildWarningTextVariable(reqItem, ((pCityTerritory != null) && maxImprovements <= pCityTerritory.getImprovementClassCount(eImprovementClass)));
                                    reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pCityTerritory.getImprovementClassCount(eImprovementClass)), TEXTVAR(maxImprovements));
                                }

                                req.AddItem(reqItem);
                            }

                            iValue = infos().improvementClass(eImprovementClass).miMaxCityCount;
                            if (iValue > 0)
                            {
                                if (pCityTerritory == null || !pCityTerritory.isNoImprovementClassMaxUnlock(eImprovementClass))
                                {
                                    //TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CITY_IMPROVEMENT_COUNT", TEXTVAR(iValue), buildImprovementClassLinkVariable(eImprovementClass));
                                    TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CITY_IMPROVEMENT_CLASS_COUNT", TEXTVAR(iValue), buildImprovementClassLinkVariable(eImprovementClass));

                                    if (pCityTerritory != null)
                                    {
                                        reqItem = buildWarningTextVariable(reqItem, iValue <= pCityTerritory.getImprovementClassCount(eImprovementClass));
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


                    if (pUnit != null)
                    {
                        if (infos().improvement(eImprovement).mbBuild && pUnit.canBuildImprovements() && pUnit.hasCooldown())
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                lRequirements.Add(buildCooldownWarning(subText, pUnit).ToTextVariable());
                            }
                        }
                    }

                }
            }
        }
    }
}