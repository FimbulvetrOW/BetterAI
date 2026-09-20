using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TenCrowns.AppCore;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace BetterAI
{
    public class BetterAIInfos : Infos
    {
        //line 437
        public BetterAIInfos(ModSettings pModSettings) : base(pModSettings)
        {
        }

        //lines 775-868
        protected override void ReadInfoListData(List<XmlDataListItemBase> items, bool deferredPass)
        {
            //using var profileScope = new UnityProfileScope("Infos.ReadInfoListData");

            bool isThreadSafe(XmlDataListItemBase item)
            {
                if (deferredPass)
                {
                    return false;
                }
                return !item.GetFlags().HasFlag(XmlDataListFlags.NonThreadSafe);
            }

            bool thisPass(XmlDataListItemBase item)
            {
                if (item.GetFlags().HasFlag(XmlDataListFlags.SeparatePassOnly))
                {
                    return items.Count == 1;
                }
                if (!mModSettings.ModPath.IsStrictMode())
                {
                    return !deferredPass;
                }
                return item.GetFlags().HasFlag(XmlDataListFlags.StrictModeDeferred) == deferredPass;
            }

            void read(XmlDataListItemBase item)
            {
                //using var profileScope = new UnityProfileScope("Infos.ReadInfoListData.Thread." + item.GetFileName());
                Type currentType = item.GetType().GenericTypeArguments[1];

                List<XmlNodeList> validationNodes = new List<XmlNodeList>();
                ReadContext ctx = new ReadContext(currentType, null);

                //base xml
                foreach (XmlDocument xmlDoc in getModdableBaseXML(item.GetFileName()))
                {
                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                    item.ReadData(nodes, this, ctx);
                    validationNodes.Add(nodes);
                }

                ctx.IsAddedData = true;

                //added xml
                foreach (XmlDocument xmlDoc in mModSettings.XMLLoader.GetModdedXML(item.GetFileName(), ModdedXMLType.ADD))
                {
                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                    item.ReadData(nodes, this, ctx);
                    validationNodes.Add(nodes);
                }

                foreach (XmlDocument xmlDoc in mModSettings.XMLLoader.GetModdedXML(item.GetFileName(), ModdedXMLType.ADD_ALWAYS))
                {
                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                    item.ReadData(nodes, this, ctx);
                }

                //change xml
                foreach (XmlDocument xmlDoc in mModSettings.XMLLoader.GetChangedXML(item.GetFileName()))
                {
                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                    item.ReadData(nodes, this, ctx);
                }

                /*####### Better Old World AI - Base DLL #######
                  ### modmod fix                       START ###
                  ##############################################*/
                //make mod load order the only significant factor for change and append
                //with this, you can -change, then a modmod can -append to that same item

                ////append xml
                //ctx.AppendLists = true;
                //foreach (XmlDocument xmlDoc in mModSettings.XMLLoader.GetModdedXML(item.GetFileName(), ModdedXMLType.APPEND))
                //{
                //    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                //    item.ReadData(nodes, this, ctx);
                //    validationNodes.Add(nodes);
                //}

                //ctx.AppendLists = false;

                ////change xml
                //foreach (XmlDocument xmlDoc in mModSettings.XMLLoader.GetModdedXML(item.GetFileName(), ModdedXMLType.CHANGE))
                //{
                //    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                //    item.ReadData(nodes, this, ctx);
                //}

                //append+change xml
                List<XmlDocument> appends = mModSettings.XMLLoader.GetModdedXML(item.GetFileName(), ModdedXMLType.APPEND);
                foreach (XmlDocument xmlDoc in mModSettings.XMLLoader.GetModdedXML(item.GetFileName(), ModdedXMLType.APPEND | ModdedXMLType.CHANGE))
                {
                    ctx.AppendLists = appends.Contains(xmlDoc);
                    XmlNodeList nodes = xmlDoc.SelectNodes("Root/Entry");
                    item.ReadData(nodes, this, ctx);
                    validationNodes.Add(nodes);
                }

                ctx.AppendLists = false;
                /*####### Better Old World AI - Base DLL #######
                  ### modmod fix                         END ###
                  ##############################################*/

                mModSettings.InfoValidator.EndReadValidation(validationNodes, item.GetFileName(), currentType, mTypeDictionary, mRemovedXMLTypes.Keys);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (XmlDataListItemBase item in items)
            {
                if (!isThreadSafe(item) && thisPass(item))
                {
                    read(item);
                }
            }
            Parallel.ForEach(items, item =>
            {
                if (isThreadSafe(item) && thisPass(item))
                {
                    read(item);
                }
            });
            stopwatch.Stop();
            Debug.Log($"Infos.ReadInfoListData complete in {stopwatch.ElapsedMilliseconds} ms");
        }


        //line 1196-1218
        protected override void init(bool resetDefaultXMLCache)
        {
            base.init(resetDefaultXMLCache);
            calculateDerivativeInfo();
        }

        public virtual void addAllUpgrades(UnitType eUnit)
        {
            BetterAIInfoUnit pInfoUnit = (BetterAIInfoUnit)unit(eUnit);

            foreach (UnitType eUpgradeUnit in (pInfoUnit.maeUpgradeUnit))
            {
                if (pInfoUnit.mseUpgradeUnitAccumulated.Add(eUpgradeUnit))
                {
                    //recursion
                    addAllUpgrades(eUpgradeUnit);

                    pInfoUnit.mseUpgradeUnitAccumulated.UnionWith(((BetterAIInfoUnit)unit(eUpgradeUnit)).mseUpgradeUnitAccumulated);
                }
            }

            return;
        }

        public virtual bool removeUnlockerEffectPlayerLoops(EffectPlayerType eEffectPlayer, ref HashSet<EffectPlayerType> previousUnlockers)
        {
            BetterAIInfoEffectPlayer pInfoEffectPlayer = (BetterAIInfoEffectPlayer)effectPlayer(eEffectPlayer);

            if (pInfoEffectPlayer.meEffectPlayer != EffectPlayerType.NONE)
            {
                if (previousUnlockers.Contains(pInfoEffectPlayer.meEffectPlayer))
                {
                    pInfoEffectPlayer.meEffectPlayer = EffectPlayerType.NONE;
                    return true;
                }
                else
                {
                    return removeUnlockerEffectPlayerLoops(pInfoEffectPlayer.meEffectPlayer, ref previousUnlockers);
                }
            }

            return false;
        }

        public virtual bool addAllUnlockerEffectPlayers(EffectPlayerType eEffectPlayer)
        {
            BetterAIInfoEffectPlayer pInfoEffectPlayer = (BetterAIInfoEffectPlayer)effectPlayer(eEffectPlayer);

            if (pInfoEffectPlayer.meEffectPlayer != EffectPlayerType.NONE)
            {
                BetterAIInfoEffectPlayer pInfoEffectPlayerUnlocked = (BetterAIInfoEffectPlayer)effectPlayer(pInfoEffectPlayer.meEffectPlayer);
                pInfoEffectPlayerUnlocked.mseGetsUnlockedByEffectPlayers.Add(eEffectPlayer);
                pInfoEffectPlayerUnlocked.mseGetsUnlockedByEffectPlayers.UnionWith(pInfoEffectPlayer.mseGetsUnlockedByEffectPlayers);

                addAllUnlockerEffectPlayers(pInfoEffectPlayer.meEffectPlayer);
            }

            return true;
        }

        public virtual void setAllAnyEffectPlayerEffectPlayers(EffectPlayerType eEffectPlayer)
        {
            BetterAIInfoEffectPlayer pInfoEffectPlayer = (BetterAIInfoEffectPlayer)effectPlayer(eEffectPlayer);

            pInfoEffectPlayer.bAnyEffectPlayerEffectPlayer = true;

            foreach (EffectPlayerType eUnlockedByEffectPlayer in pInfoEffectPlayer.mseGetsUnlockedByEffectPlayers)
            {
                //this should never happen. I expect things would get weird here - so I choose not to support it at all.
                BetterAIInfoEffectPlayer pUnlockedByInfoEffectPlayer = (BetterAIInfoEffectPlayer)effectPlayer(eUnlockedByEffectPlayer);

                if (pUnlockedByInfoEffectPlayer.meEffectPlayer == eEffectPlayer) //remove only direct unlocks
                {
                    UnityEngine.Debug.Log("A player effect unlocks another player effect, which together with yet another can add yet another player effect. A -> B; B + C -> D. All unlocks leading to this player effect will be removed: player effect unlocks (EffectPlayer) are allowed to cascade (A -> B -> C -> D) but may never branch out (EffectPlayerEffectPlayer). Avoid this by using 2x EffectPlayerEffectPlayer in the first place instead of 1x EffectPlayer and 1x EffectPlayerEffectPlayer: A + C -> D, B + C -> D. meEffectPlayer ("+ pInfoEffectPlayer.mzType + ") removed from " + pUnlockedByInfoEffectPlayer.mzType);

                    pUnlockedByInfoEffectPlayer.meEffectPlayer = EffectPlayerType.NONE;
                }
            }
        }


/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/
        public virtual void setEffectPlayerPermanance(EffectPlayerType eEffectPlayer)
        {
            BetterAIInfoEffectPlayer pInfoEffectPlayer = (BetterAIInfoEffectPlayer)effectPlayer(eEffectPlayer);
            if (pInfoEffectPlayer.mbPermanent)
            {
                if (pInfoEffectPlayer.meSourceTrait != TraitType.NONE || pInfoEffectPlayer.meSourceCouncil != CouncilType.NONE)
                {
                    pInfoEffectPlayer.mbPermanent = false;
                }
                else if (pInfoEffectPlayer.mseSourceTraitJobs.Count + pInfoEffectPlayer.mseSourceTraitTraits.Count > 0)
                {
                    pInfoEffectPlayer.mbPermanent = false;
                }
                else if (pInfoEffectPlayer.maeeSourceEffectPlayers.Count + pInfoEffectPlayer.mseGetsUnlockedByEffectPlayers.Count > 0)
                {
                    foreach ((EffectPlayerType, EffectPlayerType) eLoopEffectPlayerPair in pInfoEffectPlayer.maeeSourceEffectPlayers)
                    {
                        setEffectPlayerPermanance(eLoopEffectPlayerPair.Item1);
                        setEffectPlayerPermanance(eLoopEffectPlayerPair.Item2);
                        BetterAIInfoEffectPlayer pSourceInfoEffectPlayer1 = (BetterAIInfoEffectPlayer)effectPlayer(eLoopEffectPlayerPair.Item1);
                        BetterAIInfoEffectPlayer pSourceInfoEffectPlayer2 = (BetterAIInfoEffectPlayer)effectPlayer(eLoopEffectPlayerPair.Item2);
                        pInfoEffectPlayer.mbPermanent = pInfoEffectPlayer.mbPermanent && pSourceInfoEffectPlayer1.mbPermanent && pSourceInfoEffectPlayer2.mbPermanent;

                        if (!pInfoEffectPlayer.mbPermanent) return;
                    }

                    foreach (EffectPlayerType eLoopEffectPlayer in pInfoEffectPlayer.mseGetsUnlockedByEffectPlayers)
                    {
                        BetterAIInfoEffectPlayer pSourceInfoEffectPlayer = (BetterAIInfoEffectPlayer)effectPlayer(eLoopEffectPlayer);
                        setEffectPlayerPermanance(eLoopEffectPlayer);
                        pInfoEffectPlayer.mbPermanent = pInfoEffectPlayer.mbPermanent && pSourceInfoEffectPlayer.mbPermanent;

                        if (!pInfoEffectPlayer.mbPermanent) return;
                    }
                }

            }
        }

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/

        public virtual void getFactorsForExpectedMaxAge(int iAge, bool bGeneral, BetterAIInfoMortality pInfoMortality, out int aX10, out int bX1000, out int cX100000)
        {
            if (bGeneral)
            {
                //General job/life expectancy formula by age (x >= 20), respecting ill and severely ill, by mortality
                if (iAge >= pInfoMortality.miMaxAgeGenOld)
                {
                    aX10 = pInfoMortality.miMaxAgeGenOldaX10;
                    bX1000 = pInfoMortality.miMaxAgeGenOldbX1000;
                    cX100000 = pInfoMortality.miMaxAgeGenOldcX100000;
                }
                else
                {
                    aX10 = pInfoMortality.miMaxAgeGenYoungaX10;
                    bX1000 = pInfoMortality.miMaxAgeGenYoungbX1000;
                    cX100000 = pInfoMortality.miMaxAgeGenYoungcX100000;
                }
            }
            else
            {
                if (iAge >= pInfoMortality.miMaxAgeOld)
                {
                    aX10 = pInfoMortality.miMaxAgeOldaX10;
                    bX1000 = pInfoMortality.miMaxAgeOldbX1000;
                    cX100000 = pInfoMortality.miMaxAgeOldcX100000;
                }
                else
                {
                    aX10 = pInfoMortality.miMaxAgeYoungaX10;
                    bX1000 = pInfoMortality.miMaxAgeYoungbX1000;
                    cX100000 = pInfoMortality.miMaxAgeYoungcX100000;
                }
            }
        }

/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/


        protected virtual void calculateDerivativeInfo()
        {
            //UnityEngine.Debug.Log("Infos.calculateDerivativeInfo - Start");

/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
            for (EffectUnitType eLoopEffectUnit = 0; eLoopEffectUnit < effectUnitsNum(); eLoopEffectUnit++)
            {
                if (effectUnit(eLoopEffectUnit).maeUnitTraitZOC.Count > 0)
                {
                    for (UnitType eLoopUnit = 0; eLoopUnit < unitsNum(); eLoopUnit++)
                    {

                        bool bBlocksUnit = false;
                        foreach (UnitTraitType eLoopUnitTrait in effectUnit(eLoopEffectUnit).maeUnitTraitZOC)
                        {
                            if (unit(eLoopUnit).maeUnitTrait.Contains(eLoopUnitTrait))
                            {
                                bBlocksUnit = true;
                                break;
                            }

                        }
                        
                        if (bBlocksUnit)
                        {
                            ((BetterAIInfoUnit)unit(eLoopUnit)).maeBlockZOCEffectUnits.Add(eLoopEffectUnit);
                            ((BetterAIInfoUnit)unit(eLoopUnit)).bHasIngoreZOCBlocker = true;
                        }

                    }
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/

            for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < improvementClassesNum(); eLoopImprovementClass++)
            {
                BetterAIInfoImprovementClass pLoopImprovementClassInfo = ((BetterAIInfoImprovementClass)improvementClass(eLoopImprovementClass));
                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < improvementsNum(); eLoopImprovement++)
                {
                    if (eLoopImprovementClass == improvement(eLoopImprovement).meClass)
                    {
                        pLoopImprovementClassInfo.maeImprovementTypes.Add(eLoopImprovement);
                    }
                }
            }

            //cascading bonus adjacent improvments is forbidden
            for (ImprovementType eLoopImprovement = 0; eLoopImprovement < improvementsNum(); eLoopImprovement++)
            {
                BetterAIInfoImprovement pLoopImprovementInfo = ((BetterAIInfoImprovement)improvement(eLoopImprovement));
                if (pLoopImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                {
                    pLoopImprovementInfo.meBonusAdjacentImprovement = ImprovementType.NONE; //just to be sure
                    foreach (ImprovementType eClassInprovement in ((BetterAIInfoImprovementClass)improvementClass(pLoopImprovementInfo.meBonusAdjacentImprovementClass)).maeImprovementTypes)
                    {
                        ((BetterAIInfoImprovement)improvement(eClassInprovement)).meBonusAdjacentImprovement = ImprovementType.NONE;
                    }
                }
                else if (pLoopImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE)
                {
                    ((BetterAIInfoImprovement)improvement(pLoopImprovementInfo.meBonusAdjacentImprovement)).meBonusAdjacentImprovement = ImprovementType.NONE;
                }
            }

            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < effectCitiesNum(); eLoopEffectCity++)
            {
                BetterAIInfoEffectCity pLoopEffectCity = ((BetterAIInfoEffectCity)effectCity(eLoopEffectCity));
                foreach (ImprovementClassType epSkipTechImprovementClass in pLoopEffectCity.maeVoidTechPrereqImprovementClass)
                {
                    BetterAIInfoImprovementClass pSkipTechImprovementClassInfo = ((BetterAIInfoImprovementClass)improvementClass(epSkipTechImprovementClass));
                    pSkipTechImprovementClassInfo.maeVoidTechPrereqFromEffectCities.Add(eLoopEffectCity);
                }
            }


            for (UnitType eLoopUnit = 0; eLoopUnit < unitsNum(); eLoopUnit++)
            {
                BetterAIInfoUnit pLoopUnitInfo = (BetterAIInfoUnit)unit(eLoopUnit);
                if (pLoopUnitInfo.maeTribeUpgradeUnit.Count > 0)
                {
                    for (TribeType eLoopTribe = 0; eLoopTribe < tribesNum(); eLoopTribe++)
                    {
                        if (pLoopUnitInfo.maeTribeUpgradeUnit[eLoopTribe] != UnitType.NONE)
                        {
                            BetterAIInfoUnit pTribeUpgradeUnitInfo = (BetterAIInfoUnit)unit(pLoopUnitInfo.maeTribeUpgradeUnit[eLoopTribe]);
                            if (!pTribeUpgradeUnitInfo.maeTribeUpgradesFromAccumulated.Contains(eLoopUnit))
                            {
                                pTribeUpgradeUnitInfo.maeTribeUpgradesFromAccumulated.Add(eLoopUnit);
                            }
                        }
                    }
                }

                if (pLoopUnitInfo.maeUpgradeUnit.Count > 0 && pLoopUnitInfo.mseUpgradeUnitAccumulated.Count == 0) //no need to do this with units that have no upgrades, and we only need to look at eatch unit once
                {
                    addAllUpgrades(eLoopUnit); //recursive
                }

                if (pLoopUnitInfo.meEffectCityPrereq != EffectCityType.NONE)
                {
                    ResourceType ePrereqResource = effectCity(pLoopUnitInfo.meEffectCityPrereq).meSourceResource;
                    if (ePrereqResource != ResourceType.NONE)
                    {
                        if (((BetterAIInfoGlobals)Globals).dUnitsWithResourceRequirement.ContainsKey(ePrereqResource))
                        {
                            ((BetterAIInfoGlobals)Globals).dUnitsWithResourceRequirement[ePrereqResource].Add(eLoopUnit);
                        }
                        else
                        {
                            ((BetterAIInfoGlobals)Globals).dUnitsWithResourceRequirement.Add(ePrereqResource, new List<UnitType>() { eLoopUnit });
                        }
                    }
                }
            }

            for (UnitType eLoopUnit = 0; eLoopUnit < unitsNum(); eLoopUnit++)
            {
                //add direct upgrades too, no recursion here
                BetterAIInfoUnit pLoopUnitInfo = (BetterAIInfoUnit)unit(eLoopUnit);
                foreach (UnitType eDirectUpgradeUnit in pLoopUnitInfo.maeDirectUpgradeUnit)
                {
                    pLoopUnitInfo.mseUpgradeUnitAccumulated.Add(eDirectUpgradeUnit);
                }

                //cleanup in case of circular unit upgrade references
                pLoopUnitInfo.mseUpgradeUnitAccumulated.Remove(eLoopUnit);

                //categories
                {
                    for (int i = 0; i < (int)ClientUI.UnitListFilterType.NUM_TYPES; i++)
                    {
                        pLoopUnitInfo.maeUnitCategories.Add(false);
                    }

                    bool bMilitary = false;
                    if (pLoopUnitInfo.miRangeMax > 0)
                    {
                        //ranged
                        bMilitary = true;
                        pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_RANGED] = true;

                    }
                    else if (pLoopUnitInfo.mbMelee)
                    {
                        //melee
                        bMilitary = true;
                    }

                    if (bMilitary)
                    {
                        if (pLoopUnitInfo.meUnitCycle == UnitCycleType.MILITARY_SIEGE || pLoopUnitInfo.maeUnitTrait.Contains(Globals.SIEGE_TRAIT))
                        {
                            pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_SIEGE] = true;
                        }
                    }
                    else
                    {
                        //civilian
                        pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.CIVILIAN] = true;
                    }
                    

                    if (!pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_SIEGE]) //siege can't be water, mounted or infantry or scout
                    {
                        if (pLoopUnitInfo.meUnitCycle == UnitCycleType.SCOUT || pLoopUnitInfo.miReveal > 0) //military can be scout too
                        {
                            //scout
                            pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.SCOUT] = true;
                        }

                        if (pLoopUnitInfo.meUnitCycle == UnitCycleType.MILITARY_WATER || pLoopUnitInfo.maeUnitTrait.Contains(Globals.SHIP_TRAIT) || pLoopUnitInfo.mbWater)
                        {
                            pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_WATER] = true;
                        }
                        else if (pLoopUnitInfo.meUnitCycle == UnitCycleType.MILITARY_MOUNTED || pLoopUnitInfo.maeUnitTrait.Contains(Globals.MOUNTED_TRAIT))
                        {
                            pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_MOUNTED] = true;
                        }
                        else if (pLoopUnitInfo.meUnitCycle == UnitCycleType.MILITARY_INFANTRY || pLoopUnitInfo.maeUnitTrait.Contains(((BetterAIInfoGlobals)Globals).INFANTRY_TRAIT))
                        {
                            pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_INFANTRY] = true;
                        }
                    }


                    {
                        //public bool BAI_NO_MOUNTED_IS_RANGED = true;
                        //public bool BAI_NO_WATER_IS_RANGED = true;
                        //public bool BAI_NO_WATER_IS_SCOUT = true;
                        //public bool BAI_NO_WATER_IS_CIVILIAN = true;
                        //public bool BAI_ALL_CIVILIAN_AND_SCOUT_IS_INFANTRY = true;
                        if (pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_MOUNTED] && pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_RANGED])
                        {
                            ((BetterAIInfoGlobals)Globals).BAI_NO_MOUNTED_IS_RANGED = false;
                        }
                        else if (pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_WATER])
                        {
                            if (pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_RANGED])
                            {
                                ((BetterAIInfoGlobals)Globals).BAI_NO_WATER_IS_RANGED = false;
                            }
                            else
                            {
                                if (pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.SCOUT])
                                {
                                    ((BetterAIInfoGlobals)Globals).BAI_NO_WATER_IS_SCOUT = false;
                                }
                                else if (pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.CIVILIAN])
                                {
                                    ((BetterAIInfoGlobals)Globals).BAI_NO_WATER_IS_CIVILIAN = false;
                                }
                            }
                        }

                        if (pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.CIVILIAN] && !pLoopUnitInfo.maeUnitCategories[(int)ClientUI.UnitListFilterType.MILITARY_INFANTRY])
                        {
                            ((BetterAIInfoGlobals)Globals).BAI_ALL_CIVILIAN_AND_SCOUT_IS_INFANTRY = false;
                        }
                    }
                }
            }


            for (TraitType eLoopTrait = 0; eLoopTrait < traitsNum(); eLoopTrait++)
            {
                for (JobType eLoopJob = 0; eLoopJob < jobsNum(); eLoopJob++)
                {
                    BetterAIInfoTrait pLoopInfoTrait = ((BetterAIInfoTrait)trait(eLoopTrait));
                    EffectPlayerType eTraitJobEffectPlayer = pLoopInfoTrait.maeJobEffectPlayer[eLoopJob];
                    BetterAIInfoEffectPlayer pInfoEffectPlayer = ((BetterAIInfoEffectPlayer)effectPlayer(eTraitJobEffectPlayer));
                    BetterAIInfoJob pLoopInfoJob = ((BetterAIInfoJob)job(eLoopJob));

                    //For council jobs, this is the most elegant solution: using base game field maeCouncilEffectPlayer
                    if (pLoopInfoJob.meCouncil != CouncilType.NONE)
                    {
                        if (pLoopInfoTrait.maeCouncilEffectPlayer[pLoopInfoJob.meCouncil] != EffectPlayerType.NONE)
                        {
                            if (eTraitJobEffectPlayer != EffectPlayerType.NONE)
                            {
                                UnityEngine.Debug.Log("Trait " + pLoopInfoTrait.mzType + " has both job effect and council effect for the same job. council effect will be overwritten");
                                pLoopInfoTrait.maeCouncilEffectPlayer[pLoopInfoJob.meCouncil] = eTraitJobEffectPlayer;
                                pLoopInfoTrait.maeJobEffectPlayer[eLoopJob] = EffectPlayerType.NONE;
                                pInfoEffectPlayer.meSourceCouncil = pLoopInfoJob.meCouncil;
                            }
                            else
                            {
                                eTraitJobEffectPlayer = pLoopInfoTrait.maeCouncilEffectPlayer[pLoopInfoJob.meCouncil];
                            }
                        }
                        else if (eTraitJobEffectPlayer != EffectPlayerType.NONE)
                        {
                            pLoopInfoTrait.maeCouncilEffectPlayer[pLoopInfoJob.meCouncil] = eTraitJobEffectPlayer;
                            pInfoEffectPlayer.meSourceCouncil = pLoopInfoJob.meCouncil;
                        }
                    }

                    if (eTraitJobEffectPlayer != EffectPlayerType.NONE)
                    {
                        pLoopInfoJob.bAnyTraitEffectPlayer = true;

                        if (pLoopInfoJob.mdlEffectPlayerTraits.ContainsKey(eTraitJobEffectPlayer))
                        {
                            pLoopInfoJob.mdlEffectPlayerTraits[eTraitJobEffectPlayer].Add(eLoopTrait);
                        }
                        else
                        {
                            pLoopInfoJob.mdlEffectPlayerTraits.Add(eTraitJobEffectPlayer, new List<TraitType>() { eLoopTrait });
                        }

                        pInfoEffectPlayer.mseSourceTraitJobs.Add(eLoopJob);
                        pInfoEffectPlayer.mbPermanent = false;
                    }
                }
            }


            for (TraitType eOuterLoopTrait = 0; eOuterLoopTrait < traitsNum(); eOuterLoopTrait++)
            {
                for (TraitType eInnerLoopTrait = eOuterLoopTrait; eInnerLoopTrait < traitsNum(); eInnerLoopTrait++)
                {
                    if (eOuterLoopTrait == eInnerLoopTrait) //no trait can create a player effect by itself
                    {
                        ((BetterAIInfoTrait)trait(eOuterLoopTrait)).maeTraitEffectPlayer[eInnerLoopTrait] = EffectPlayerType.NONE;

                        //this is the same:
                        //((BetterAIInfoTrait)trait(eInnerLoopTrait)).maeTraitEffectPlayer[eOuterLoopTrait] = EffectPlayerType.NONE;

                        continue;
                    }

                    BetterAIInfoTrait pOuterLoopInfoTrait = ((BetterAIInfoTrait)trait(eOuterLoopTrait));
                    BetterAIInfoTrait pInnerLoopInfoTrait = ((BetterAIInfoTrait)trait(eInnerLoopTrait));

                    //can give deadly trait?
                    int iProb = pOuterLoopInfoTrait.maiTraitProb[eInnerLoopTrait];
                    if (iProb > 0)
                    {
                        pOuterLoopInfoTrait.maeAllTraitProbs.Add(eInnerLoopTrait);

                        if (Helpers.getTraitDieProb(eInnerLoopTrait, pGame: null) > 0)
                        {
                            pOuterLoopInfoTrait.maeDieTraitProbs.Add(eInnerLoopTrait);
                        }
                        if (pInnerLoopInfoTrait.mbNoJob)
                        {
                            pOuterLoopInfoTrait.maeNoJobTraitProbs.Add(eInnerLoopTrait);
                        }
                    }
                    iProb = pInnerLoopInfoTrait.maiTraitProb[eOuterLoopTrait];
                    if (iProb > 0)
                    {
                        pInnerLoopInfoTrait.maeAllTraitProbs.Add(eOuterLoopTrait);

                        if (Helpers.getTraitDieProb(eOuterLoopTrait, pGame: null) > 0)
                        {
                            pInnerLoopInfoTrait.maeDieTraitProbs.Add(eOuterLoopTrait);
                        }
                        if (pInnerLoopInfoTrait.mbNoJob
                            && (pInnerLoopInfoTrait.miRemoveTurns == 0 || pInnerLoopInfoTrait.miRemoveTurns >= 5) //ignore NoJob traits if they are gone in 4 turns or less
                            && (pInnerLoopInfoTrait.miRemoveProb == 0 || pInnerLoopInfoTrait.miRemoveProb < 40))
                        {
                            pOuterLoopInfoTrait.maeNoJobTraitProbs.Add(eInnerLoopTrait);
                        }
                    }

                    if (pOuterLoopInfoTrait.maeTraitEffectPlayer[eInnerLoopTrait] != EffectPlayerType.NONE)   //in this case eInnerLoopTrait is the job-like trait
                    {
                        EffectPlayerType eTraitTraitPlayerEffect = pOuterLoopInfoTrait.maeTraitEffectPlayer[eInnerLoopTrait];
                        if (pInnerLoopInfoTrait.maeTraitEffectPlayer[eOuterLoopTrait] != EffectPlayerType.NONE)
                        {
                            pInnerLoopInfoTrait.maeTraitEffectPlayer[eOuterLoopTrait] = EffectPlayerType.NONE;  //one direction only
                        }

                        pOuterLoopInfoTrait.bAnyTraitEffectPlayer = true;
                        pInnerLoopInfoTrait.bAnyTraitEffectPlayer = true;

                        if (pInnerLoopInfoTrait.mleEffectPlayerTraits.ContainsKey(eTraitTraitPlayerEffect))
                        {
                            pInnerLoopInfoTrait.mleEffectPlayerTraits[eTraitTraitPlayerEffect].Add(eInnerLoopTrait);
                        }
                        else
                        {
                            pInnerLoopInfoTrait.mleEffectPlayerTraits.Add(eTraitTraitPlayerEffect, new List<TraitType>() { eInnerLoopTrait });
                        }

                        BetterAIInfoEffectPlayer pInfoEffectPlayer = ((BetterAIInfoEffectPlayer)effectPlayer(eTraitTraitPlayerEffect));
                        pInfoEffectPlayer.mseSourceTraitTraits.Add(eInnerLoopTrait);
                        pInfoEffectPlayer.mbPermanent = false;

                    }
                    else if (pInnerLoopInfoTrait.maeTraitEffectPlayer[eOuterLoopTrait] != EffectPlayerType.NONE)  //in this case eOuterLoopTrait is the job-like trait
                    {
                        EffectPlayerType eTraitTraitPlayerEffect = pInnerLoopInfoTrait.maeTraitEffectPlayer[eOuterLoopTrait];
                        pOuterLoopInfoTrait.bAnyTraitEffectPlayer = true;
                        pInnerLoopInfoTrait.bAnyTraitEffectPlayer = true;

                        if (pOuterLoopInfoTrait.mleEffectPlayerTraits.ContainsKey(eTraitTraitPlayerEffect))
                        {
                            pOuterLoopInfoTrait.mleEffectPlayerTraits[eTraitTraitPlayerEffect].Add(eOuterLoopTrait);
                        }
                        else
                        {
                            pOuterLoopInfoTrait.mleEffectPlayerTraits.Add(eTraitTraitPlayerEffect, new List<TraitType>() { eOuterLoopTrait });
                        }

                        BetterAIInfoEffectPlayer pInfoEffectPlayer = ((BetterAIInfoEffectPlayer)effectPlayer(eTraitTraitPlayerEffect));
                        pInfoEffectPlayer.mseSourceTraitTraits.Add(eOuterLoopTrait);
                    }
                }
            }


            for (TraitType eLoopTrait = 0; eLoopTrait < traitsNum(); eLoopTrait++)
            {
                BetterAIInfoTrait pLoopInfoTrait = ((BetterAIInfoTrait)trait(eLoopTrait));
                if (pLoopInfoTrait.mbNoRemoveOnTraitProb || pLoopInfoTrait.miRemoveTurns > 0) continue;

                int iChances = 10000;
                foreach (TraitType eLoopProbTrait in pLoopInfoTrait.maeAllTraitProbs)
                {
                    iChances *= (100 - pLoopInfoTrait.maiTraitProb[eLoopProbTrait]);
                    iChances /= 100;
                }
                //example: 20% chance to get any trait from traitprob means average 5 turns to happen and remove the original trait
                //(10000 - iChances) = chance for any trait (100% = 10000). 100% / chance = average turns
                if (iChances < 10000)
                {
                    pLoopInfoTrait.maiRemoveTurnsFromTraitProbsX10 = (10000 * 10) / (10000 - iChances);
                }
            }

            //mortality from traits
            for (MortalityType eLoopMortality = 0; eLoopMortality < mortalitiesNum(); eLoopMortality++)
            {
                BetterAIInfoMortality pLoopInfoMortality = (BetterAIInfoMortality)mortality(eLoopMortality);
                int aX10, bX1000, cX100000;

                //General
                for (int iAge = pLoopInfoMortality.miMaxAgeGenYoungMinAge; iAge < Globals.GENERAL_RETIRE_AGE - 1; iAge++)
                {
                    getFactorsForExpectedMaxAge(iAge: iAge, bGeneral: true, pInfoMortality: pLoopInfoMortality, out aX10, out bX1000, out cX100000);
                    pLoopInfoMortality.maiMaxAgeGeneralX10[iAge] = ((((iAge * iAge * cX100000 + 50) / 100) + (iAge * bX1000) + 50) / 100) + aX10;

                }
                //starting from 1 year below retirement age: max 1 more year remaining, next year will trigger retirement
                for (int iAge = Globals.GENERAL_RETIRE_AGE - 1; iAge < pLoopInfoMortality.maiMaxAgeGeneralX10.Length; iAge++)
                {
                    pLoopInfoMortality.maiMaxAgeGeneralX10[iAge] = 10 * (iAge + 1);
                }
                //filling up afterwards
                for (int iAge = pLoopInfoMortality.miMaxAgeGenYoungMinAge - 1; iAge >= 0; iAge--)
                {
                    pLoopInfoMortality.maiMaxAgeGeneralX10[iAge] = pLoopInfoMortality.maiMaxAgeGeneralX10[iAge + 1];
                }

                //Non-General
                for (int iAge = pLoopInfoMortality.miMaxAgeYoungMinAge; iAge < pLoopInfoMortality.maiMaxAgeX10.Length; iAge++)
                {
                    getFactorsForExpectedMaxAge(iAge: iAge, bGeneral: false, pInfoMortality: pLoopInfoMortality, out aX10, out bX1000, out cX100000);
                    pLoopInfoMortality.maiMaxAgeX10[iAge] = ((((iAge * iAge * cX100000 + 50) / 100) + (iAge * bX1000) + 50) / 100) + aX10;

                }
                //filling up afterwards
                for (int iAge = pLoopInfoMortality.miMaxAgeYoungMinAge - 1; iAge >= 0; iAge--)
                {
                    pLoopInfoMortality.maiMaxAgeX10[iAge] = pLoopInfoMortality.maiMaxAgeX10[iAge + 1];
                }
            }


            //for (MortalityType eLoopMortality = 0; eLoopMortality < mortalitiesNum(); eLoopMortality++)
            //{
            //    BetterAIInfoMortality pLoopInfoMortality = (BetterAIInfoMortality)mortality(eLoopMortality);
            //    Debug.Log($"{pLoopInfoMortality.mzType}: General retirement {Globals.GENERAL_RETIRE_AGE}");
            //    for (int i = 0; i < pLoopInfoMortality.maiMaxAgeGeneralX10.Length; i++)
            //    {
            //        Debug.Log($"{i}: Life {pLoopInfoMortality.maiMaxAgeX10[i]}, General {pLoopInfoMortality.maiMaxAgeGeneralX10[i]} ");
            //    }
            //}





            HashSet<EffectPlayerType> previousUnlockers = new HashSet<EffectPlayerType>();
            for (EffectPlayerType eLoopEffectPlayer = 0; eLoopEffectPlayer < effectPlayersNum(); eLoopEffectPlayer++)
            {
                if (effectPlayer(eLoopEffectPlayer).meEffectPlayer != EffectPlayerType.NONE)
                {
                    if (removeUnlockerEffectPlayerLoops(eLoopEffectPlayer, ref previousUnlockers))
                    {
                        previousUnlockers.Clear();
                    }
                }
            }

            for (EffectPlayerType eLoopEffectPlayer = 0; eLoopEffectPlayer < effectPlayersNum(); eLoopEffectPlayer++)
            {
                addAllUnlockerEffectPlayers(eLoopEffectPlayer);
            }

            for (EffectPlayerType eOuterLoopEffectPlayer = 0; eOuterLoopEffectPlayer < effectPlayersNum(); eOuterLoopEffectPlayer++)
            {
                for (EffectPlayerType eInnerLoopEffectPlayer = eOuterLoopEffectPlayer; eInnerLoopEffectPlayer < effectPlayersNum(); eInnerLoopEffectPlayer++)
                {
                    if (((BetterAIInfoEffectPlayer)effectPlayer(eOuterLoopEffectPlayer)).maeEffectPlayerEffectPlayer[eInnerLoopEffectPlayer] != EffectPlayerType.NONE)
                    {
                        if (eOuterLoopEffectPlayer == eInnerLoopEffectPlayer)
                        {
                            ((BetterAIInfoEffectPlayer)effectPlayer(eOuterLoopEffectPlayer)).maeEffectPlayerEffectPlayer[eInnerLoopEffectPlayer] = EffectPlayerType.NONE;
                        }
                        else
                        {
                            if (((BetterAIInfoEffectPlayer)effectPlayer(eInnerLoopEffectPlayer)).maeEffectPlayerEffectPlayer[eOuterLoopEffectPlayer] != EffectPlayerType.NONE)
                            {
                                ((BetterAIInfoEffectPlayer)effectPlayer(eInnerLoopEffectPlayer)).maeEffectPlayerEffectPlayer[eOuterLoopEffectPlayer] = EffectPlayerType.NONE;
                            }
                            setAllAnyEffectPlayerEffectPlayers(eOuterLoopEffectPlayer);
                            setAllAnyEffectPlayerEffectPlayers(eInnerLoopEffectPlayer);
                            ((BetterAIInfoEffectPlayer)effectPlayer(eInnerLoopEffectPlayer)).maeeSourceEffectPlayers.Add((eInnerLoopEffectPlayer, eOuterLoopEffectPlayer));
                        }

                        if (((BetterAIInfoEffectPlayer)effectPlayer(eInnerLoopEffectPlayer)).maeEffectPlayerEffectPlayer[eOuterLoopEffectPlayer] != EffectPlayerType.NONE)
                        {
                            setAllAnyEffectPlayerEffectPlayers(eOuterLoopEffectPlayer);
                            setAllAnyEffectPlayerEffectPlayers(eInnerLoopEffectPlayer);
                            ((BetterAIInfoEffectPlayer)effectPlayer(eInnerLoopEffectPlayer)).maeeSourceEffectPlayers.Add((eOuterLoopEffectPlayer, eInnerLoopEffectPlayer));
                        }
                    }
                }
            }

            for (EffectPlayerType eLoopEffectPlayer = 0; eLoopEffectPlayer < effectPlayersNum(); eLoopEffectPlayer++)
            {
                setEffectPlayerPermanance(eLoopEffectPlayer);
            }


            if (((BetterAIInfoGlobals)Globals).BAI_NO_DELAY == 1)
            {
                foreach (InfoBonus pLoopBonus in bonuses())
                {
                    if (pLoopBonus.maiTraitProbDelay.Count > 0 && pLoopBonus.maiTraitProb.Count == 0)
                    {
                        foreach (KeyValuePair<TraitType, int> probTrait in pLoopBonus.maiTraitProbDelay)
                        {
                            pLoopBonus.maiTraitProb[probTrait.Key] = probTrait.Value;
                        }
                        pLoopBonus.maiTraitProbDelay.Clear();
                    }

                    if (pLoopBonus.maeRandomTraitDelay.Count > 0 && pLoopBonus.maeRandomTrait.Count == 0)
                    {
                        foreach (TraitType randTrait in pLoopBonus.maeRandomTraitDelay)
                        {
                            pLoopBonus.maeRandomTrait.Add(randTrait);
                        }
                        pLoopBonus.maeRandomTraitDelay.Clear();
                    }

                    if (pLoopBonus.maeRandomLeaderRelationshipDelay.Count > 0 && pLoopBonus.maeRandomLeaderRelationship.Count == 0)
                    {
                        foreach (RelationshipType randRelation in pLoopBonus.maeRandomLeaderRelationshipDelay)
                        {
                            pLoopBonus.maeRandomLeaderRelationship.Add(randRelation);
                        }
                        pLoopBonus.maeRandomLeaderRelationshipDelay.Clear();
                    }
                }
            }

            //UnityEngine.Debug.Log("Infos.calculateDerivativeInfo - End");
        }


        //compare line 1805-1866 readTypeIntListByType<T, U>(ReadContext ctx, string zText, ref SparseList2D<T, U, int> lliValues)
        // and line 1729-1769 readTypesByType<U, T>(ReadContext ctx, string zText, ref SparseList<U, T> leValues, T defaultVal)
        public virtual void readTypeTypeListByType<T, U, V>(ReadContext ctx, string zText, ref SparseList2D<T, U, V> lleValues, V defaultVal)
        {
            if (AddFieldType(zText, ctx, typeof(List<Tuple<T, Tuple<U, V>>>), false))
            {
                AddFieldType(zText + "/Pair", ctx, typeof(Tuple<T, Tuple<U, V>>), true);
                AddFieldType(zText + "/Pair/zIndex", ctx, typeof(T), false);
                AddFieldType(zText + "/Pair/SubPair", ctx, typeof(Tuple<U, V>), true);
                AddFieldType(zText + "/Pair/SubPair/zSubIndex", ctx, typeof(U), false);
                AddFieldType(zText + "/Pair/SubPair/zValue", ctx, typeof(V), false);
            }

            XmlNode entryNode = ctx.Node.FindChild(zText);
            if (entryNode != null)
            {
                if (!ctx.AppendLists)
                    lleValues.Clear();
                lleValues.Default = defaultVal;

                for (XmlNode child = entryNode.FirstChild; child != null; child = child.NextSibling)
                {
                    if (child.Name == "Pair")
                    {
                        ReadContext childCtx = new ReadContext(ctx, child);
                        string zType = readString(childCtx, "zIndex", true, true);
                        if (IsRemovedXMLType(zType))
                        {
                            continue;
                        }

                        T index = getType<T>(zType);
                        if (CastTo<int>.From(index) >= 0)
                        {
                            for (XmlNode subChild = child.FirstChild; subChild != null; subChild = subChild.NextSibling)
                            {
                                if (subChild.Name == "SubPair")
                                {
                                    ReadContext subChildCtx = new ReadContext(ctx, subChild);

                                    string zSubType = readString(subChildCtx, "zSubIndex", true, true);
                                    if (IsRemovedXMLType(zSubType))
                                    {
                                        continue;
                                    }

                                    U subIndex = getType<U>(zSubType);
                                    V value = getType<V>(readString(childCtx, "zValue", true, true));
                                    if (CastTo<int>.From(subIndex) >= 0)
                                    {
                                        lleValues[index, subIndex] = value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                lleValues.Default = defaultVal;
            }
        }


/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
        //line 283
        protected List<BetterAIInfoCourtier> maBetterAICourtiers;

        //line 2522
        public override InfoCourtier courtier(CourtierType eIndex) => maBetterAICourtiers.GetOrDefault((int)eIndex);
        public override CourtierType courtiersNum() => (CourtierType)maBetterAICourtiers.Count;
        public override List<InfoCourtier> courtiers() => new List<InfoCourtier>(maBetterAICourtiers);
        public virtual List<BetterAIInfoCourtier> BetterAIcourtiers() => maBetterAICourtiers;
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/

        
        protected List<BetterAIInfoEffectCity> maBetterAIEffectCities;

        public override InfoEffectCity effectCity(EffectCityType eIndex) => maBetterAIEffectCities.GetOrDefault((int)eIndex);
        public override EffectCityType effectCitiesNum() => (EffectCityType)maBetterAIEffectCities.Count;
        public override List<InfoEffectCity> effectCities() => new List<InfoEffectCity>(maBetterAIEffectCities);
        public virtual List<BetterAIInfoEffectCity> BetterAIeffectCities() => maBetterAIEffectCities;



/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ###  EffectPlayer combinations             ###
  ##############################################*/
        //line 294
        protected List<BetterAIInfoEffectPlayer> maBetterAIEffectPlayers;

        //line 2566
        public override InfoEffectPlayer effectPlayer(EffectPlayerType eIndex) => maBetterAIEffectPlayers.GetOrDefault((int)eIndex);
        public override EffectPlayerType effectPlayersNum() => (EffectPlayerType)maBetterAIEffectPlayers.Count;
        public override List<InfoEffectPlayer> effectPlayers() => new List<InfoEffectPlayer>(maBetterAIEffectPlayers);
        public virtual List<BetterAIInfoEffectPlayer> BetterAIeffectPlayers() => maBetterAIEffectPlayers;


/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ###  EffectPlayer combinations             ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
        //line 295
        protected List<BetterAIInfoEffectUnit> maBetterAIEffectUnits;

        //line 2574
        public override InfoEffectUnit effectUnit(EffectUnitType eIndex) => maBetterAIEffectUnits.GetOrDefault((int)eIndex);
        public override EffectUnitType effectUnitsNum() => (EffectUnitType)maBetterAIEffectUnits.Count;
        public override List<InfoEffectUnit> effectUnits() => new List<InfoEffectUnit>(maBetterAIEffectUnits);
        public virtual List<BetterAIInfoEffectUnit> BetterAIeffectUnits() => maBetterAIEffectUnits;
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
        //line 316
        protected List<BetterAIInfoImprovement> maBetterAIImprovements;

        //line 2654
        public override InfoImprovement improvement(ImprovementType eIndex) => maBetterAIImprovements.GetOrDefault((int)eIndex);
        public override ImprovementType improvementsNum() => (ImprovementType)maBetterAIImprovements.Count;
        public override List<InfoImprovement> improvements() => new List<InfoImprovement>(maBetterAIImprovements);
        public virtual List<BetterAIInfoImprovement> BetterAIimprovements() => maBetterAIImprovements;

        //line 317
        protected List<BetterAIInfoImprovementClass> maBetterAIImprovementClasses;

        //line 2658
        public override InfoImprovementClass improvementClass(ImprovementClassType eIndex) => maBetterAIImprovementClasses.GetOrDefault((int)eIndex);
        public override ImprovementClassType improvementClassesNum() => (ImprovementClassType)maBetterAIImprovementClasses.Count;
        public override List<InfoImprovementClass> improvementClasses() => new List<InfoImprovementClass>(maBetterAIImprovementClasses);
        public virtual List<BetterAIInfoImprovementClass> BetterAIimprovementClasses() => maBetterAIImprovementClasses;
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
        //line 318
        protected List<BetterAIInfoJob> maBetterAIJobs;

        //line 2670
        public override InfoJob job(JobType eJob) => maBetterAIJobs.GetOrDefault((int)eJob);
        public override JobType jobsNum() => (JobType)maBetterAIJobs.Count;
        public override List<InfoJob> jobs() => new List<InfoJob>(maBetterAIJobs);
        public virtual List<BetterAIInfoJob> BetterAIjobs() => maBetterAIJobs;
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/



/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/
        protected List<BetterAIInfoMortality> maBetterAIMortalities;

        //line 2670
        public override InfoMortality mortality(MortalityType eIndex) => maBetterAIMortalities.GetOrDefault((int)eIndex);
        public override MortalityType mortalitiesNum() => (MortalityType)maBetterAIMortalities.Count;
        public override List<InfoMortality> mortalities() => new List<InfoMortality>(maBetterAIMortalities);
        public virtual List<BetterAIInfoMortality> BetterAImortalities() => maBetterAIMortalities;

/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/



/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        //line 383
        protected List<BetterAIInfoTerrain> maBetterAITerrains;

        //line 2922
        public override InfoTerrain terrain(TerrainType eIndex) => maBetterAITerrains.GetOrDefault((int)eIndex);
        public override TerrainType terrainsNum() => (TerrainType)maBetterAITerrains.Count;
        public override List<InfoTerrain> terrains() => new List<InfoTerrain>(maBetterAITerrains);
        public virtual List<BetterAIInfoTerrain> BetterAIterrains() => maBetterAITerrains;
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
        //line 391
        protected List<BetterAIInfoTrait> maBetterAITraits;

        //line 2954
        public override InfoTrait trait(TraitType eIndex) => maBetterAITraits.GetOrDefault((int)eIndex);
        public override TraitType traitsNum() => (TraitType)maBetterAITraits.Count;
        public override List<InfoTrait> traits() => new List<InfoTrait>(maBetterAITraits);
        public virtual List<BetterAIInfoTrait> BetterAItraits() => maBetterAITraits;
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### Empty Sites Override             START ###
  ##############################################*/
        //line 393
        protected List<BetterAIInfoTribeLevel> maBetterAITribeLevels;

        //line 2962
        public override InfoTribeLevel tribeLevel(TribeLevelType eIndex) => maBetterAITribeLevels.GetOrDefault((int)eIndex);
        public override TribeLevelType tribeLevelsNum() => (TribeLevelType)maBetterAITribeLevels.Count;
        public override List<InfoTribeLevel> tribeLevels() => new List<InfoTribeLevel>(maBetterAITribeLevels);
        public virtual List<BetterAIInfoTribeLevel> BetterAItribeLevels() => maBetterAITribeLevels;

/*####### Better Old World AI - Base DLL #######
  ### Empty Sites Override               END ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
        //line 399
        protected List<BetterAIInfoUnit> maBetterAIUnits;

        //line 2986
        public override InfoUnit unit(UnitType eIndex) => maBetterAIUnits.GetOrDefault((int)eIndex);
        public override UnitType unitsNum() => (UnitType)maBetterAIUnits.Count;
        public override List<InfoUnit> units() => new List<InfoUnit>(maBetterAIUnits);
        public virtual List<BetterAIInfoUnit> BetterAIunits() => maBetterAIUnits;
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        protected List<InfoCityBiome> maCityBiomes;
        public virtual List<InfoCityBiome> cityBiomes() => maCityBiomes;
        public virtual InfoCityBiome cityBiome(CityBiomeType eIndex) => maCityBiomes.GetOrDefault((int)eIndex);
        public virtual CityBiomeType cityBiomesNum() => (CityBiomeType)maCityBiomes.Count;
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### [multiple]                       START ###
  ##############################################*/
        //line 491-646
        protected override void BuildListOfInfoFiles()
        {
            base.BuildListOfInfoFiles();

            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/courtier"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/effectCity"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/effectPlayer"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/effectUnit"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/improvement"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/improvementClass"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/job"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/mortality"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/terrain"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/trait"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/tribeLevel"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/unit"));

            mInfoList.Add(new XmlDataListItem<BetterAIInfoCourtier, CourtierType>("Infos/courtier", readInfoTypes<BetterAIInfoCourtier, CourtierType>, ref maBetterAICourtiers));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoEffectCity, EffectCityType>("Infos/effectCity", readInfoTypes<BetterAIInfoEffectCity, EffectCityType>, ref maBetterAIEffectCities));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoEffectPlayer, EffectPlayerType>("Infos/effectPlayer", readInfoTypes<BetterAIInfoEffectPlayer, EffectPlayerType>, ref maBetterAIEffectPlayers));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoEffectUnit, EffectUnitType>("Infos/effectUnit", readInfoTypes<BetterAIInfoEffectUnit, EffectUnitType>, ref maBetterAIEffectUnits));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoImprovement, ImprovementType>("Infos/improvement", readInfoTypes<BetterAIInfoImprovement, ImprovementType>, ref maBetterAIImprovements));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoImprovementClass, ImprovementClassType>("Infos/improvementClass", readInfoTypes<BetterAIInfoImprovementClass, ImprovementClassType>, ref maBetterAIImprovementClasses));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoJob, JobType>("Infos/job", readInfoTypes<BetterAIInfoJob, JobType>, ref maBetterAIJobs));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoMortality, MortalityType>("Infos/mortality", readInfoTypes<BetterAIInfoMortality, MortalityType>, ref maBetterAIMortalities));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoTerrain, TerrainType>("Infos/terrain", readInfoTypes<BetterAIInfoTerrain, TerrainType>, ref maBetterAITerrains));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoTrait, TraitType>("Infos/trait", readInfoTypes<BetterAIInfoTrait, TraitType>, ref maBetterAITraits));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoTribeLevel, TribeLevelType>("Infos/tribeLevel", readInfoTypes<BetterAIInfoTribeLevel, TribeLevelType>, ref maBetterAITribeLevels));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoUnit, UnitType>("Infos/unit", readInfoTypes<BetterAIInfoUnit, UnitType>, ref maBetterAIUnits));

            mInfoList.Add(new XmlDataListItem<InfoCityBiome, CityBiomeType>("Infos/cityBiome", readInfoTypes<InfoCityBiome, CityBiomeType>, ref maCityBiomes));
        }
/*####### Better Old World AI - Base DLL #######
  ### [multiple]                       START ###
  ##############################################*/
    }


/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
    //InfoBase.cs, line 1122
    public class BetterAIInfoCourtier : InfoCourtier
    {
        public List<TraitType> maeAdjectives = new List<TraitType>();
        public bool mbStateReligion = false; //will automatically get a Religion
        public bool mbNotRandomCourtier = false; //when true, a Courtier will never randonly get this type
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readTypes(ctx, "aeAdjectives", ref maeAdjectives);
            infos.readBool(ctx, "bStateReligion", ref mbStateReligion);
            infos.readBool(ctx, "bNotRandomCourtier", ref mbNotRandomCourtier);
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/


    public class BetterAIInfoEffectCity : InfoEffectCity
    {
        public bool mbEnablesGovernor = false;
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readBool(ctx, "bEnablesGovernor", ref mbEnablesGovernor);
        }
    }

    
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/
    //InfoBase.cs, line 6358
    public class BetterAIInfoEffectPlayer : InfoEffectPlayer
    {
        public SparseList<EffectPlayerType, EffectPlayerType> maeEffectPlayerEffectPlayer = new SparseList<EffectPlayerType, EffectPlayerType>();
        public bool bAnyEffectPlayerEffectPlayer = false;
        public HashSet<EffectPlayerType> mseGetsUnlockedByEffectPlayers = new HashSet<EffectPlayerType>();
        public HashSet<JobType> mseSourceTraitJobs = new HashSet<JobType>();
        public HashSet<TraitType> mseSourceTraitTraits = new HashSet<TraitType>();
        public bool mbPermanent = true;
        public HashSet<(EffectPlayerType, EffectPlayerType)> maeeSourceEffectPlayers = new HashSet<(EffectPlayerType, EffectPlayerType)>();
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readTypesByType(ctx, "aeEffectPlayerEffectPlayer", ref maeEffectPlayerEffectPlayer, EffectPlayerType.NONE);
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/

    //InfoBase.cs, line 1701
    public class BetterAIInfoEffectUnit : InfoEffectUnit
    {
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
        public bool mbAmphibiousEmbark = false;
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Enlist Replacement Attack Heal   START ###
  ##############################################*/
        public int miHealAttack = 0;
        //public int miHealKill = 0;
/*####### Better Old World AI - Base DLL #######
  ### Enlist Replacement Attack Heal     END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Tile-based Combat Modifiers      START ###
  ##############################################*/

            //not yet implemented
            //transform this into TerrainTarget form before implementing
            //public List<int> maiTerrainFromDefenseModifier = new List<int>();
            //public List<int> maiTerrainToAttackModifier = new List<int>();
            //public List<int> maiClearTerrainToAttackModifier = new List<int>();
            //public List<int> maiHeightFromDefenseModifier = new List<int>();
            //public List<int> maiHeightToAttackModifier = new List<int>();
            //public List<int> maiClearHeightToAttackModifier = new List<int>();
            //public List<int> maiVegetationFromDefenseModifier = new List<int>();
            //public List<int> maiVegetationToAttackModifier = new List<int>();
            //public List<int> maiImprovementFromModifier = new List<int>();
            //public List<int> maiImprovementFromDefenseModifier = new List<int>();

/*####### Better Old World AI - Base DLL #######
  ### Tile-based Combat Modifiers        END ###
  ##############################################*/

        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
            infos.readBool(ctx, "bAmphibiousEmbark", ref mbAmphibiousEmbark);
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Enlist Replacement Attack Heal   START ###
  ##############################################*/
            infos.readInt(ctx, "iHealAttack", ref miHealAttack);
            //heal on kill would require A LOT more changes for AI, so I'm scrapping this idea
            //infos.readInt(ctx, "iHealKill", ref miHealKill);
/*####### Better Old World AI - Base DLL #######
  ### Enlist Replacement Attack Heal     END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Tile-based Combat Modifiers      START ###
  ##############################################*/

                //infos.readIntsByType(ctx, "aiTerrainFromDefenseModifier", ref maiTerrainFromDefenseModifier, ((BetterAIInfos)infos).terrainsNum());
                //infos.readIntsByType(ctx, "aiTerrainToAttackModifier", ref maiTerrainToAttackModifier, ((BetterAIInfos)infos).terrainsNum());
                //infos.readIntsByType(ctx, "aiClearTerrainToAttackModifier", ref maiClearTerrainToAttackModifier, ((BetterAIInfos)infos).terrainsNum());
                //infos.readIntsByType(ctx, "aiHeightFromDefenseModifier", ref maiHeightFromDefenseModifier, ((BetterAIInfos)infos).heightsNum());
                //infos.readIntsByType(ctx, "aiHeightToAttackModifier", ref maiHeightToAttackModifier, ((BetterAIInfos)infos).heightsNum());
                //infos.readIntsByType(ctx, "aiClearHeightToAttackModifier", ref maiClearHeightToAttackModifier, ((BetterAIInfos)infos).heightsNum());
                //infos.readIntsByType(ctx, "aiVegetationFromDefenseModifier", ref maiVegetationFromDefenseModifier, ((BetterAIInfos)infos).vegetationNum());
                //infos.readIntsByType(ctx, "aiVegetationToAttackModifier", ref maiVegetationToAttackModifier, ((BetterAIInfos)infos).vegetationNum());
                //infos.readIntsByType(ctx, "aiImprovementFromModifier", ref maiImprovementFromModifier, ((BetterAIInfos)infos).improvementsNum());
                //infos.readIntsByType(ctx, "aiImprovementFromDefenseModifier", ref maiImprovementFromDefenseModifier, ((BetterAIInfos)infos).improvementsNum());

/*####### Better Old World AI - Base DLL #######
  ### Tile-based Combat Modifiers        END ###
  ##############################################*/
        }
    }

/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ### Bonus adjacent Improvement             ###
  ##############################################*/
    //corresponding classes are in InfoBase.cs
    //InfoBase.cs, line 2724
    public class BetterAIInfoImprovement : InfoImprovement
    {
        //new stuff here
        public int miRangeChange = 0;
        public EffectUnitType meApplyEffectUnit = EffectUnitType.NONE; //not implemented.
        public CityBiomeType meCityBiomePrereq = CityBiomeType.NONE;
        public TechType meSecondaryUnlockTechPrereq = TechType.NONE;
        public CultureType meSecondaryUnlockCulturePrereq = CultureType.NONE;
        public int miSecondaryUnlockPopulationPrereq = 0;
        public EffectCityType meSecondaryUnlockEffectCityPrereq = EffectCityType.NONE;
        public virtual bool isAnySecondaryPrereq()
        {
            return (meSecondaryUnlockTechPrereq != TechType.NONE || 
                meSecondaryUnlockCulturePrereq != CultureType.NONE ||
                miSecondaryUnlockPopulationPrereq > 0 ||
                meSecondaryUnlockEffectCityPrereq != EffectCityType.NONE);
        }

        public FamilyClassType meTertiaryUnlockFamilyClassPrereq = FamilyClassType.NONE;
        public bool mbTertiaryUnlockSeatOnly = false;
        public TechType meTertiaryUnlockTechPrereq = TechType.NONE;
        public CultureType meTertiaryUnlockCulturePrereq = CultureType.NONE;
        public EffectCityType meTertiaryUnlockEffectCityPrereq = EffectCityType.NONE;
        public virtual bool isAnyTertiaryPrereq()
        {
            return (meTertiaryUnlockFamilyClassPrereq != FamilyClassType.NONE ||
                meTertiaryUnlockTechPrereq != TechType.NONE ||
                meTertiaryUnlockCulturePrereq != CultureType.NONE ||
                meTertiaryUnlockEffectCityPrereq != EffectCityType.NONE);
        }
        //public BonusType meBonusCitiesExtra = BonusType.NONE;
        public ImprovementType meBonusAdjacentImprovement = ImprovementType.NONE;
        public ImprovementClassType meBonusAdjacentImprovementClass = ImprovementClassType.NONE;
        public bool mbMakesAdjacentPassableLandTileValidForBonusImprovement = false;

        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);

            infos.readInt(ctx, "iRangeChange", ref miRangeChange);
            infos.readType(ctx, "ApplyEffectUnit", ref meApplyEffectUnit);
            infos.readType(ctx, "CityBiomePrereq", ref meCityBiomePrereq);
            infos.readType(ctx, "SecondaryUnlockTechPrereq", ref meSecondaryUnlockTechPrereq);
            infos.readType(ctx, "SecondaryUnlockCulturePrereq", ref meSecondaryUnlockCulturePrereq);
            infos.readInt(ctx, "iSecondaryUnlockPopulationPrereq", ref miSecondaryUnlockPopulationPrereq);
            infos.readType(ctx, "SecondaryUnlockEffectCityPrereq", ref meSecondaryUnlockEffectCityPrereq);
            infos.readType(ctx, "TertiaryUnlockFamilyClassPrereq", ref meTertiaryUnlockFamilyClassPrereq);
            infos.readBool(ctx, "bTertiaryUnlockSeatOnly", ref mbTertiaryUnlockSeatOnly);
            infos.readType(ctx, "TertiaryUnlockTechPrereq", ref meTertiaryUnlockTechPrereq);
            infos.readType(ctx, "TertiaryUnlockCulturePrereq", ref meTertiaryUnlockCulturePrereq);
            infos.readType(ctx, "TertiaryUnlockEffectCityPrereq", ref meTertiaryUnlockEffectCityPrereq);
            //infos.readType(ctx, "BonusCitiesExtra", ref meBonusCitiesExtra);
            infos.readType(ctx, "BonusAdjacentImprovement", ref meBonusAdjacentImprovement);
            infos.readType(ctx, "BonusAdjacentImprovementClass", ref meBonusAdjacentImprovementClass);
            infos.readBool(ctx, "bMakesAdjacentPassableLandTileValidForBonusImprovement", ref mbMakesAdjacentPassableLandTileValidForBonusImprovement);
        }
    }

    //InfoBase.cs, line 2963
    public class BetterAIInfoImprovementClass : InfoImprovementClass
    {
        public List<ImprovementType> maeImprovementTypes = new List<ImprovementType>();
        public List<EffectCityType> maeVoidTechPrereqFromEffectCities = new List<EffectCityType>();
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);

        }
    }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ### Bonus adjacent Improvement             ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
    //InfoBase.cs, line 3517
    public class BetterAIInfoJob : InfoJob
    {
        public bool bAnyTraitEffectPlayer = false;
        public Dictionary<EffectPlayerType, List<TraitType>> mdlEffectPlayerTraits = new Dictionary<EffectPlayerType, List<TraitType>>();
    }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/



/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/
    public class BetterAIInfoMortality : InfoMortality
    {
        public int miMaxAgeGenOldaX10 = 0;
        public int miMaxAgeGenOldbX1000 = 0;
        public int miMaxAgeGenOldcX100000 = 0;
        public int miMaxAgeGenOld = 0;  //older than this will use Old calc

        public int miMaxAgeGenYoungaX10 = 0;
        public int miMaxAgeGenYoungbX1000 = 0;
        public int miMaxAgeGenYoungcX100000 = 0;
        public int miMaxAgeGenYoungMinAge = 0;

        public int miMaxAgeOldaX10 = 0;
        public int miMaxAgeOldbX1000 = 0;
        public int miMaxAgeOldcX100000 = 0;
        public int miMaxAgeOld = 0;  //older than this will use Old calc

        public int miMaxAgeYoungaX10 = 0;
        public int miMaxAgeYoungbX1000 = 0;
        public int miMaxAgeYoungcX100000 = 0;
        public int miMaxAgeYoungMinAge = 0;

        public int[] maiMaxAgeGeneralX10 = new int[128];
        public int[] maiMaxAgeX10 = new int[128];


        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);

            infos.readInt(ctx, "iMaxAgeGenOldaX10", ref miMaxAgeGenOldaX10);
            infos.readInt(ctx, "iMaxAgeGenOldbX1000", ref miMaxAgeGenOldbX1000);
            infos.readInt(ctx, "iMaxAgeGenOldcX100000", ref miMaxAgeGenOldcX100000);
            infos.readInt(ctx, "iMaxAgeGenOld", ref miMaxAgeGenOld);

            infos.readInt(ctx, "iMaxAgeGenYoungaX10", ref miMaxAgeGenYoungaX10);
            infos.readInt(ctx, "iMaxAgeGenYoungbX1000", ref miMaxAgeGenYoungbX1000);
            infos.readInt(ctx, "iMaxAgeGenYoungcX100000", ref miMaxAgeGenYoungcX100000);
            infos.readInt(ctx, "iMaxAgeGenYoungMinAge", ref miMaxAgeGenYoungMinAge);

            infos.readInt(ctx, "iMaxAgeOldaX10", ref miMaxAgeOldaX10);
            infos.readInt(ctx, "iMaxAgeOldbX1000", ref miMaxAgeOldbX1000);
            infos.readInt(ctx, "iMaxAgeOldcX100000", ref miMaxAgeOldcX100000);
            infos.readInt(ctx, "iMaxAgeOld", ref miMaxAgeOld);

            infos.readInt(ctx, "iMaxAgeYoungaX10", ref miMaxAgeYoungaX10);
            infos.readInt(ctx, "iMaxAgeYoungbX1000", ref miMaxAgeYoungbX1000);
            infos.readInt(ctx, "iMaxAgeYoungcX100000", ref miMaxAgeYoungcX100000);
            infos.readInt(ctx, "iMaxAgeYoungMinAge", ref miMaxAgeYoungMinAge);
        }
    }

/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
    //InfoBase.cs, line 5518
    public class BetterAIInfoTerrain : InfoTerrain
    {
        public SparseList<CityBiomeType, int> maiBiomePoints = new SparseList<CityBiomeType, int>();
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readIntsByType(ctx, "aiBiomePoints", ref maiBiomePoints);
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

    
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/
    //InfoBase.cs, line 6358
    public class BetterAIInfoTrait : InfoTrait
    {
        public SparseList<JobType, EffectPlayerType> maeJobEffectPlayer = new SparseList<JobType, EffectPlayerType>();
        public SparseList<TraitType, EffectPlayerType> maeTraitEffectPlayer = new SparseList<TraitType, EffectPlayerType>();
        public bool bAnyTraitEffectPlayer = false;
        public Dictionary<EffectPlayerType, List<TraitType>> mleEffectPlayerTraits = new Dictionary<EffectPlayerType, List<TraitType>>();

        public List<TraitType> maeDieTraitProbs = new List<TraitType>();
        public List<TraitType> maeNoJobTraitProbs = new List<TraitType>();
        public List<TraitType> maeAllTraitProbs = new List<TraitType>();
        public int maiRemoveTurnsFromTraitProbsX10 = 0;
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readTypesByType(ctx, "aeJobEffectPlayer", ref maeJobEffectPlayer, EffectPlayerType.NONE);     //Trait + Job = Player Effect
            infos.readTypesByType(ctx, "aeTraitEffectPlayer", ref maeTraitEffectPlayer, EffectPlayerType.NONE); //for non-job positions like Clergy: Trait + Trait = Player Effect
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### Empty Sites Override             START ###
  ##############################################*/
    public class BetterAIInfoTribeLevel : InfoTribeLevel
    {
        public int miEmptySites = -1;

        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readInt(ctx, "iEmptySites", ref miEmptySites);
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### Empty Sites Override               END ###
  ##############################################*/

    //InfoBase.cs, line 6106
    public class BetterAIInfoUnit : InfoUnit
    {
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
        public bool bHasIngoreZOCBlocker = false;
        public List<EffectUnitType> maeBlockZOCEffectUnits = new List<EffectUnitType>();
        public List<UnitType> maeTribeUpgradesFromAccumulated = new List<UnitType>();
        public HashSet<UnitType> mseUpgradeUnitAccumulated = new HashSet<UnitType>();  //ToDo: make the AI use this too
        public List<bool> maeUnitCategories = new List<bool>();
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/
    }


/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
    public class InfoCityBiome : InfoBase<CityBiomeType>
    {
        public TextType mName = TextType.NONE;
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            infos.readType(ctx, "Name", ref mName);
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### [multiple]                       START ###
  ##############################################*/
    //InfoBase.cs, line 7360
    public class BetterAIInfoGlobals : InfoGlobals
    {
        public int BAI_WORKERLIST_EXTRA = 0; //for Worker Improvement Valid List Mod
        public int BAI_HURRY_COST_REDUCED = 0; //for activating Alternative Hurry
        public int BAI_EMBARKING_COST_EXTRA = 0;
        public int BAI_HARBOR_OR_AMPHIBIOUS_EMBARKING_DISCOUNT = 0;
        public int BAI_AMPHIBIOUS_RIVER_CROSSING_DISCOUNT = 0;
        public int BAI_AMPHIBIOUS_ZOC_CROSSES_RIVER = 0;
        public int BAI_TEAM_TERRITORY_ROAD_RIVER_CROSSING_DISCOUNT = 0;
        public int BAI_AGENT_NETWORK_COST_PER_CULTURE_LEVEL = 0;
        public int BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT = 0;
        public int BAI_SHOW_RESOURCE_TILE_COUNT = 0;
        public int BAI_SHOW_RESOURCE_TILE_COORDINATES = 0;
        public int BAI_ENLIST_NO_FAMILY = 0;
        public int BAI_DISCONTENT_LEVEL_ZERO = 0;
        public int BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS = 0;
        public int BAI_PROPER_REGENT_LEGITIMACY_DECAY = 0;
        public int BAI_ASSUMED_AVERAGE_REIGN_TURNS = 0;
        public int BAI_EXTRA_LEGITIMACY_DECAY_TURNS_PER_LEADER = 0;
        public int BAI_RANGED_UNIT_ROUTING_REQUIRES_MELEE_RANGE = 0;
        public int BAI_PRECISE_COLLATERAL_DAMAGE = 0;
        public int BAI_MIN_UPGRADE_RATINGS_OPTIONS = 0;
        public int BAI_USE_TRIANGLE_IN_COMPETITIVE = 0;
        public int BAI_COMPETITIVE_COURT_YIELD_MODIFIER = 0;
        public int BAI_BETTER_BOUNCE = 0;
        public int BAI_PLAYEREVENT_STAT_BONUS_GOES_TO_PRIMARY_STAT_PERCENT = 0;
        public int BAI_PLAYER_MAX_EXTRA_DEVELOPMENT_CITIES_PERCENT = 100;
        public int BAI_NUM_IMPROVEMENT_FINISHED_UNITS = 1;
        public int BAI_ALT_CHARACTER_SORT = 0;
        public int BAI_NO_DELAY = 0;

        public int AI_GROWTH_CITY_SPECIALIZATION_MODIFIER = 0;
        public int AI_CIVICS_CITY_SPECIALIZATION_MODIFIER = 0;
        public int AI_TRAINING_CITY_SPECIALIZATION_MODIFIER = 0;
        public int AI_FAMILY_OPINION_VALUE_PER = 0;
        public int AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE = 0;
        public int AI_CITY_GOVERNOR_VALUE = 0;
        public int AI_DEATHTRAIT_PROB_EVAL_DEPTH = 0;

        public bool BAI_NO_MOUNTED_IS_RANGED = true; //to be set to false
        public bool BAI_NO_WATER_IS_RANGED = true;
        public bool BAI_NO_WATER_IS_SCOUT = true;
        public bool BAI_NO_WATER_IS_CIVILIAN = true;
        public bool BAI_ALL_CIVILIAN_AND_SCOUT_IS_INFANTRY = true;

        public UnitTraitType INFANTRY_TRAIT = UnitTraitType.NONE;
        public ColorType COLOR_RIVER_EDGE = ColorType.NONE;


        public Dictionary<ResourceType, List<UnitType>> dUnitsWithResourceRequirement = new Dictionary<ResourceType, List<UnitType>>();
        //public List<UnitType> WorkerUnits = new List<UnitType>();
        //override for more variables
        public override void ReadData(Infos infos)
        {
            base.ReadData(infos);
            BAI_WORKERLIST_EXTRA = infos.getGlobalInt("BAI_WORKERLIST_EXTRA"); //for Worker Improvement Valid List Mod
            BAI_HURRY_COST_REDUCED = infos.getGlobalInt("BAI_HURRY_COST_REDUCED"); //for activating Alternative Hurry

            //for Land Unit Water Movement
            BAI_EMBARKING_COST_EXTRA = infos.getGlobalInt("BAI_EMBARKING_COST_EXTRA");
            BAI_HARBOR_OR_AMPHIBIOUS_EMBARKING_DISCOUNT = infos.getGlobalInt("BAI_HARBOR_OR_AMPHIBIOUS_EMBARKING_DISCOUNT");
            BAI_AMPHIBIOUS_RIVER_CROSSING_DISCOUNT = infos.getGlobalInt("BAI_HARBOR_OR_AMPHIBIOUS_EMBARKING_DISCOUNT");
            BAI_TEAM_TERRITORY_ROAD_RIVER_CROSSING_DISCOUNT = infos.getGlobalInt("BAI_TEAM_TERRITORY_ROAD_RIVER_CROSSING_DISCOUNT");
            BAI_AMPHIBIOUS_ZOC_CROSSES_RIVER = infos.getGlobalInt("BAI_AMPHIBIOUS_ZOC_CROSSES_RIVER");

            BAI_AGENT_NETWORK_COST_PER_CULTURE_LEVEL = infos.getGlobalInt("BAI_AGENT_NETWORK_COST_PER_CULTURE_LEVEL");

            BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT = infos.getGlobalInt("BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT");
            BAI_SHOW_RESOURCE_TILE_COUNT = infos.getGlobalInt("BAI_SHOW_RESOURCE_TILE_COUNT");
            BAI_SHOW_RESOURCE_TILE_COORDINATES = infos.getGlobalInt("BAI_SHOW_RESOURCE_TILE_COORDINATES");

            BAI_ENLIST_NO_FAMILY = infos.getGlobalInt("BAI_ENLIST_NO_FAMILY");
            BAI_DISCONTENT_LEVEL_ZERO = infos.getGlobalInt("BAI_DISCONTENT_LEVEL_ZERO");
            BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS = infos.getGlobalInt("BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS");

            BAI_PROPER_REGENT_LEGITIMACY_DECAY = infos.getGlobalInt("BAI_PROPER_REGENT_LEGITIMACY_DECAY");
            BAI_ASSUMED_AVERAGE_REIGN_TURNS = infos.getGlobalInt("BAI_ASSUMED_AVERAGE_REIGN_TURNS");
            BAI_EXTRA_LEGITIMACY_DECAY_TURNS_PER_LEADER = infos.getGlobalInt("BAI_EXTRA_LEGITIMACY_DECAY_TURNS_PER_LEADER");

            BAI_RANGED_UNIT_ROUTING_REQUIRES_MELEE_RANGE = infos.getGlobalInt("BAI_RANGED_UNIT_ROUTING_REQUIRES_MELEE_RANGE");
            BAI_PRECISE_COLLATERAL_DAMAGE = infos.getGlobalInt("BAI_PRECISE_COLLATERAL_DAMAGE");
            BAI_MIN_UPGRADE_RATINGS_OPTIONS = infos.getGlobalInt("BAI_MIN_UPGRADE_RATINGS_OPTIONS");
            BAI_USE_TRIANGLE_IN_COMPETITIVE = infos.getGlobalInt("BAI_USE_TRIANGLE_IN_COMPETITIVE");
            BAI_COMPETITIVE_COURT_YIELD_MODIFIER = infos.getGlobalInt("BAI_COMPETITIVE_COURT_YIELD_MODIFIER");

            BAI_BETTER_BOUNCE = infos.getGlobalInt("BAI_BETTER_BOUNCE");
            BAI_PLAYEREVENT_STAT_BONUS_GOES_TO_PRIMARY_STAT_PERCENT = infos.getGlobalInt("BAI_PLAYEREVENT_STAT_BONUS_GOES_TO_PRIMARY_STAT_PERCENT");
            BAI_PLAYER_MAX_EXTRA_DEVELOPMENT_CITIES_PERCENT = infos.getGlobalInt("BAI_PLAYER_MAX_EXTRA_DEVELOPMENT_CITIES_PERCENT");
            BAI_NUM_IMPROVEMENT_FINISHED_UNITS = infos.getGlobalInt("BAI_NUM_IMPROVEMENT_FINISHED_UNITS");
            BAI_ALT_CHARACTER_SORT = infos.getGlobalInt("BAI_ALT_CHARACTER_SORT");
            BAI_NO_DELAY = infos.getGlobalInt("BAI_NO_DELAY");

            AI_GROWTH_CITY_SPECIALIZATION_MODIFIER = infos.getGlobalAI("AI_GROWTH_CITY_SPECIALIZATION_MODIFIER");
            AI_CIVICS_CITY_SPECIALIZATION_MODIFIER = infos.getGlobalAI("AI_CIVICS_CITY_SPECIALIZATION_MODIFIER");
            AI_TRAINING_CITY_SPECIALIZATION_MODIFIER = infos.getGlobalAI("AI_TRAINING_CITY_SPECIALIZATION_MODIFIER");
            AI_FAMILY_OPINION_VALUE_PER = infos.getGlobalAI("AI_FAMILY_OPINION_VALUE_PER");
            AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE = infos.getGlobalAI("AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE");
            AI_CITY_GOVERNOR_VALUE = infos.getGlobalAI("AI_CITY_GOVERNOR_VALUE");
            AI_DEATHTRAIT_PROB_EVAL_DEPTH = infos.getGlobalAI("AI_DEATHTRAIT_PROB_EVAL_DEPTH");

            INFANTRY_TRAIT = infos.getGlobalType<UnitTraitType>("INFANTRY_TRAIT");

            COLOR_RIVER_EDGE = infos.getType<ColorType>("COLOR_RIVER_EDGE");
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### [multiple]                         END ###
  ##############################################*/

}
