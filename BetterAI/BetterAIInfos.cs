using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Enum = System.Enum;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using TenCrowns.AppCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;
using Constants = TenCrowns.GameCore.Constants;
using TenCrowns.ClientCore;
using static TenCrowns.ClientCore.ClientUI;

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

                mModSettings.XMLLoader.GetValidator().EndReadValidation(validationNodes, item.GetFileName(), currentType, mTypeDictionary, mRemovedXMLTypes.Keys);
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
            BetterAIInfoUnit pUnitInfo = (BetterAIInfoUnit)unit(eUnit);

            foreach (UnitType eUpgradeUnit in (pUnitInfo.maeUpgradeUnit))
            {
                if (pUnitInfo.mseUpgradeUnitAccumulated.Add(eUpgradeUnit))
                {
                    //recursion
                    addAllUpgrades(eUpgradeUnit);

                    pUnitInfo.mseUpgradeUnitAccumulated.UnionWith(((BetterAIInfoUnit)unit(eUpgradeUnit)).mseUpgradeUnitAccumulated);
                }
            }

            return;
        }

        protected virtual void calculateDerivativeInfo()
        {
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
                foreach (UnitType eDirectUpgradeUnit in ((BetterAIInfoUnit)unit(eLoopUnit)).maeDirectUpgradeUnit)
                {
                    ((BetterAIInfoUnit)unit(eLoopUnit)).mseUpgradeUnitAccumulated.Add(eDirectUpgradeUnit);
                }

                //cleanup in case of circular unit upgrade references
                ((BetterAIInfoUnit)unit(eLoopUnit)).mseUpgradeUnitAccumulated.Remove(eLoopUnit);
            }


            for (TraitType eLoopTrait = 0; eLoopTrait < traitsNum(); eLoopTrait++)
            {
                for (JobType eLoopJob = 0; eLoopJob < jobsNum(); eLoopJob++)
                {
                    if (((BetterAIInfoTrait)trait(eLoopTrait)).maeJobEffectPlayer[eLoopJob] != EffectPlayerType.NONE)
                    {
                        ((BetterAIInfoJob)job(eLoopJob)).bAnyTraitEffectPlayer = true;
                    }
                }
            }

            for (TraitType eOuterLoopTrait = 0; eOuterLoopTrait < traitsNum(); eOuterLoopTrait++)
            {
                for (TraitType eInnerLoopTrait = 0; eInnerLoopTrait < traitsNum(); eInnerLoopTrait++)
                {
                    if (((BetterAIInfoTrait)trait(eOuterLoopTrait)).maeTraitEffectPlayer[eInnerLoopTrait] != EffectPlayerType.NONE)
                    {
                        ((BetterAIInfoTrait)trait(eOuterLoopTrait)).bAnyTraitEffectPlayer = true;
                        ((BetterAIInfoTrait)trait(eInnerLoopTrait)).bAnyTraitEffectPlayer = true;
                    }
                }
            }

            for (EffectPlayerType eOuterLoopEffectPlayer = 0; eOuterLoopEffectPlayer < effectPlayersNum(); eOuterLoopEffectPlayer++)
            {
                for (EffectPlayerType eInnerLoopEffectPlayer = 0; eInnerLoopEffectPlayer < effectPlayersNum(); eInnerLoopEffectPlayer++)
                {
                    if (((BetterAIInfoEffectPlayer)effectPlayer(eOuterLoopEffectPlayer)).maeEffectPlayerEffectPlayer[eInnerLoopEffectPlayer] != EffectPlayerType.NONE)
                    {
                        ((BetterAIInfoEffectPlayer)effectPlayer(eOuterLoopEffectPlayer)).bAnyEffectPlayerEffectPlayer = true;
                        ((BetterAIInfoEffectPlayer)effectPlayer(eInnerLoopEffectPlayer)).bAnyEffectPlayerEffectPlayer = true;
                    }
                }
            }


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


/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
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
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/effectPlayer"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/effectUnit"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/improvement"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/improvementClass"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/job"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/terrain"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/trait"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/tribeLevel"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/unit"));

            mInfoList.Add(new XmlDataListItem<BetterAIInfoCourtier, CourtierType>("Infos/courtier", readInfoTypes<BetterAIInfoCourtier, CourtierType>, ref maBetterAICourtiers));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoEffectPlayer, EffectPlayerType>("Infos/effectPlayer", readInfoTypes<BetterAIInfoEffectPlayer, EffectPlayerType>, ref maBetterAIEffectPlayers));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoEffectUnit, EffectUnitType>("Infos/effectUnit", readInfoTypes<BetterAIInfoEffectUnit, EffectUnitType>, ref maBetterAIEffectUnits));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoImprovement, ImprovementType>("Infos/improvement", readInfoTypes<BetterAIInfoImprovement, ImprovementType>, ref maBetterAIImprovements));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoImprovementClass, ImprovementClassType>("Infos/improvementClass", readInfoTypes<BetterAIInfoImprovementClass, ImprovementClassType>, ref maBetterAIImprovementClasses));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoJob, JobType>("Infos/job", readInfoTypes<BetterAIInfoJob, JobType>, ref maBetterAIJobs));
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




/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
    //InfoBase.cs, line 6358
    public class BetterAIInfoEffectPlayer : InfoEffectPlayer
    {
        public SparseList<EffectPlayerType, EffectPlayerType> maeEffectPlayerEffectPlayer = new SparseList<EffectPlayerType, EffectPlayerType>();
        public bool bAnyEffectPlayerEffectPlayer = false;
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readTypesByType(ctx, "aeEffectPlayerEffectPlayer", ref maeEffectPlayerEffectPlayer, EffectPlayerType.NONE); //not implemented
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
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
    }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
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
  ##############################################*/
    //InfoBase.cs, line 6358
    public class BetterAIInfoTrait : InfoTrait
    {
        public SparseList<JobType, EffectPlayerType> maeJobEffectPlayer = new SparseList<JobType, EffectPlayerType>();
        public SparseList<TraitType, EffectPlayerType> maeTraitEffectPlayer = new SparseList<TraitType, EffectPlayerType>();
        public bool bAnyTraitEffectPlayer = false;
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readTypesByType(ctx, "aeJobEffectPlayer", ref maeJobEffectPlayer, EffectPlayerType.NONE);     //not implemented //Trait + Job = Player Effect
            infos.readTypesByType(ctx, "aeTraitEffectPlayer", ref maeTraitEffectPlayer, EffectPlayerType.NONE); //ToDo: HelpText //for non-job positions like Clergy: Trait + Trait = Player Effect
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
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

        public int AI_GROWTH_CITY_SPECIALIZATION_MODIFIER = 0;
        public int AI_CIVICS_CITY_SPECIALIZATION_MODIFIER = 0;
        public int AI_TRAINING_CITY_SPECIALIZATION_MODIFIER = 0;
        public int AI_FAMILY_OPINION_VALUE_PER = 0;
        public int AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE = 0;

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

            AI_GROWTH_CITY_SPECIALIZATION_MODIFIER = infos.getGlobalAI("AI_GROWTH_CITY_SPECIALIZATION_MODIFIER");
            AI_CIVICS_CITY_SPECIALIZATION_MODIFIER = infos.getGlobalAI("AI_CIVICS_CITY_SPECIALIZATION_MODIFIER");
            AI_TRAINING_CITY_SPECIALIZATION_MODIFIER = infos.getGlobalAI("AI_TRAINING_CITY_SPECIALIZATION_MODIFIER");
            AI_FAMILY_OPINION_VALUE_PER = infos.getGlobalAI("AI_FAMILY_OPINION_VALUE_PER");
            AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE = infos.getGlobalAI("AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE");
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### [multiple]                         END ###
  ##############################################*/

}
