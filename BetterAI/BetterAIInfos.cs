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
        //line 382
        public BetterAIInfos(ModSettings pModSettings) : base(pModSettings)
        {
        }

        //line 1249-1274
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

        }



        private List<string> strictModeDeferredItems = new List<string> { "Infos/achievement", "Infos/bonus", "Infos/eventStory", "Infos/eventOption",
            "Infos/subject", "Infos/assetVariation", "Infos/subjectRelation",
            "Infos/audio", "Infos/subjectTextVar", "Infos/borderPattern",
            "Infos/goal", "Infos/characterPortrait", "Infos/effectCity", "Infos/effectPlayer",
            "Infos/characterPortraitAgeInterpolation", "Infos/characterPortraitFeaturePoints", "Infos/characterPortraitOpinion"};

        protected override void ReadInfoListData(List<XmlDataListItemBase> items, bool deferredPass)
        {
            //using var profileScope = new UnityProfileScope("Infos.ReadInfoListData");

            bool isThreadSafe(XmlDataListItemBase item)
            {
                if (deferredPass) return false;
                return !(item.GetFileName() == "Infos/preload-text" || item.GetFileName() == "Infos/preload-concept" || item.GetFileName() == "Infos/language");
            }

            bool thisPass(XmlDataListItemBase item)
            {
                if (!mModSettings.ModPath.IsStrictMode()) return !deferredPass;
                return strictModeDeferredItems.Contains(item.GetFileName()) ? deferredPass : !deferredPass;
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
            foreach (var item in items)
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

/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
        //line 231
        protected List<BetterAIInfoCourtier> maBetterAICourtiers;

        //line 2464
        public override InfoCourtier courtier(CourtierType eIndex) => maBetterAICourtiers.GetOrDefault((int)eIndex);
        public override CourtierType courtiersNum() => (CourtierType)maBetterAICourtiers.Count;
        public override List<InfoCourtier> courtiers() => new List<InfoCourtier>(maBetterAICourtiers);
        public virtual List<BetterAIInfoCourtier> BetterAIcourtiers() => maBetterAICourtiers;
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
        //line 243
        protected List<BetterAIInfoEffectUnit> maBetterAIEffectUnits;

        //line 2512
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
        //line 264
        protected List<BetterAIInfoImprovement> maBetterAIImprovements;

        //line 2596
        public override InfoImprovement improvement(ImprovementType eIndex) => maBetterAIImprovements.GetOrDefault((int)eIndex);
        public override ImprovementType improvementsNum() => (ImprovementType)maBetterAIImprovements.Count;
        public override List<InfoImprovement> improvements() => new List<InfoImprovement>(maBetterAIImprovements);
        public virtual List<BetterAIInfoImprovement> BetterAIimprovements() => maBetterAIImprovements;

        //line 265
        protected List<BetterAIInfoImprovementClass> maBetterAIImprovementClasses;

        //line 2600
        public override InfoImprovementClass improvementClass(ImprovementClassType eIndex) => maBetterAIImprovementClasses.GetOrDefault((int)eIndex);
        public override ImprovementClassType improvementClassesNum() => (ImprovementClassType)maBetterAIImprovementClasses.Count;
        public override List<InfoImprovementClass> improvementClasses() => new List<InfoImprovementClass>(maBetterAIImprovementClasses);
        public virtual List<BetterAIInfoImprovementClass> BetterAIimprovementClasses() => maBetterAIImprovementClasses;
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        //line 329
        protected List<BetterAIInfoTerrain> maBetterAITerrains;

        //line 2856
        public override InfoTerrain terrain(TerrainType eIndex) => maBetterAITerrains.GetOrDefault((int)eIndex);
        public override TerrainType terrainsNum() => (TerrainType)maBetterAITerrains.Count;
        public override List<InfoTerrain> terrains() => new List<InfoTerrain>(maBetterAITerrains);
        public virtual List<BetterAIInfoTerrain> BetterAIterrains() => maBetterAITerrains;
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
        //line 345
        protected List<BetterAIInfoUnit> maBetterAIUnits;

        //line 2920
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

            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/improvement"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/improvementClass"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/terrain"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/effectUnit"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/courtier"));
            mInfoList.RemoveAt(mInfoList.FindIndex(x => x.GetFileName() == "Infos/unit"));

            mInfoList.Add(new XmlDataListItem<BetterAIInfoImprovement, ImprovementType>("Infos/improvement", readInfoTypes<BetterAIInfoImprovement, ImprovementType>, ref maBetterAIImprovements));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoImprovementClass, ImprovementClassType>("Infos/improvementClass", readInfoTypes<BetterAIInfoImprovementClass, ImprovementClassType>, ref maBetterAIImprovementClasses));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoTerrain, TerrainType>("Infos/terrain", readInfoTypes<BetterAIInfoTerrain, TerrainType>, ref maBetterAITerrains));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoEffectUnit, EffectUnitType>("Infos/effectUnit", readInfoTypes<BetterAIInfoEffectUnit, EffectUnitType>, ref maBetterAIEffectUnits));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoCourtier, CourtierType>("Infos/courtier", readInfoTypes<BetterAIInfoCourtier, CourtierType>, ref maBetterAICourtiers));
            mInfoList.Add(new XmlDataListItem<BetterAIInfoUnit, UnitType>("Infos/unit", readInfoTypes<BetterAIInfoUnit, UnitType>, ref maBetterAIUnits));

            mInfoList.Add(new XmlDataListItem<InfoCityBiome, CityBiomeType>("Infos/cityBiome", readInfoTypes<InfoCityBiome, CityBiomeType>, ref maCityBiomes));
        }
/*####### Better Old World AI - Base DLL #######
  ### [multiple]                       START ###
  ##############################################*/
    }

/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
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
        public virtual bool isAnythingNew()
        {
            return ((meCityBiomePrereq != CityBiomeType.NONE) || isAnySecondaryPrereq() || isAnyTertiaryPrereq());
        }


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
        }
    }

    //InfoBase.cs, line 2963
    public class BetterAIInfoImprovementClass : InfoImprovementClass
    {
        //new stuff here
        public int miMaxCityCount = 0;
        public List<ImprovementType> maeImprovementTypes = new List<ImprovementType>();
        public override void Read(Infos infos, Infos.ReadContext ctx)
        {
            base.Read(infos, ctx);
            infos.readInt(ctx, "iMaxCityCount", ref miMaxCityCount);

        }
    }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
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
    //InfoBase.cs, line 6556
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
        public int BAI_SWAP_UNIT_FATIGUE_COST = 0;
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

            BAI_SWAP_UNIT_FATIGUE_COST = infos.getGlobalInt("BAI_SWAP_UNIT_FATIGUE_COST");
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
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### [multiple]                         END ###
  ##############################################*/

}
