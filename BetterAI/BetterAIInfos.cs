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
using System.Xml;

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


        public override void ReadData(XmlNode node, Infos infos)
        {
            base.ReadData(node, infos);

            infos.readType(node, "CityBiomePrereq", ref meCityBiomePrereq);
            infos.readType(node, "SecondaryUnlockTechPrereq", ref meSecondaryUnlockTechPrereq);
            infos.readType(node, "SecondaryUnlockCulturePrereq", ref meSecondaryUnlockCulturePrereq);
            infos.readInt(node, "iSecondaryUnlockPopulationPrereq", ref miSecondaryUnlockPopulationPrereq);
            infos.readType(node, "SecondaryUnlockEffectCityPrereq", ref meSecondaryUnlockEffectCityPrereq);
            infos.readType(node, "TertiaryUnlockFamilyClassPrereq", ref meTertiaryUnlockFamilyClassPrereq);
            infos.readBool(node, "bTertiaryUnlockSeatOnly", ref mbTertiaryUnlockSeatOnly);
            infos.readType(node, "TertiaryUnlockTechPrereq", ref meTertiaryUnlockTechPrereq);
            infos.readType(node, "TertiaryUnlockCulturePrereq", ref meTertiaryUnlockCulturePrereq);
            infos.readType(node, "TertiaryUnlockEffectCityPrereq", ref meTertiaryUnlockEffectCityPrereq);
        }
    }

    //InfoBase.cs, line 2963
    public class BetterAIInfoImprovementClass : InfoImprovementClass
    {
        //new stuff here
        public int miMaxCityCount = 0;
        public override void ReadData(XmlNode node, Infos infos)
        {
            base.ReadData(node, infos);
            infos.readInt(node, "iMaxCityCount", ref miMaxCityCount);

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
        public List<int> maiBiomePoints = new List<int>();
        public override void ReadData(XmlNode node, Infos infos)
        {
            base.ReadData(node, infos);
            infos.readIntsByType(node, "aiBiomePoints", ref maiBiomePoints, ((BetterAIInfos)infos).cityBiomesNum());
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
        public bool mbAmphibious = false;
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

        public override void ReadData(XmlNode node, Infos infos)
        {
            base.ReadData(node, infos);
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
            infos.readBool(node, "bAmphibious", ref mbAmphibious);
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Tile-based Combat Modifiers      START ###
  ##############################################*/

                //infos.readIntsByType(node, "aiTerrainFromDefenseModifier", ref maiTerrainFromDefenseModifier, ((BetterAIInfos)infos).terrainsNum());
                //infos.readIntsByType(node, "aiTerrainToAttackModifier", ref maiTerrainToAttackModifier, ((BetterAIInfos)infos).terrainsNum());
                //infos.readIntsByType(node, "aiClearTerrainToAttackModifier", ref maiClearTerrainToAttackModifier, ((BetterAIInfos)infos).terrainsNum());
                //infos.readIntsByType(node, "aiHeightFromDefenseModifier", ref maiHeightFromDefenseModifier, ((BetterAIInfos)infos).heightsNum());
                //infos.readIntsByType(node, "aiHeightToAttackModifier", ref maiHeightToAttackModifier, ((BetterAIInfos)infos).heightsNum());
                //infos.readIntsByType(node, "aiClearHeightToAttackModifier", ref maiClearHeightToAttackModifier, ((BetterAIInfos)infos).heightsNum());
                //infos.readIntsByType(node, "aiVegetationFromDefenseModifier", ref maiVegetationFromDefenseModifier, ((BetterAIInfos)infos).vegetationNum());
                //infos.readIntsByType(node, "aiVegetationToAttackModifier", ref maiVegetationToAttackModifier, ((BetterAIInfos)infos).vegetationNum());
                //infos.readIntsByType(node, "aiImprovementFromModifier", ref maiImprovementFromModifier, ((BetterAIInfos)infos).improvementsNum());
                //infos.readIntsByType(node, "aiImprovementFromDefenseModifier", ref maiImprovementFromDefenseModifier, ((BetterAIInfos)infos).improvementsNum());

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
        public override void ReadData(XmlNode node, Infos infos)
        {
            base.ReadData(node, infos);
            infos.readTypes(node, "aeAdjectives", ref maeAdjectives);
            infos.readBool(node, "bStateReligion", ref mbStateReligion);
            infos.readBool(node, "bNotRandomCourtier", ref mbNotRandomCourtier);
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
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/
    }


/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
    public class InfoCityBiome : InfoBase
    {
        public CityBiomeType meType { get { return (CityBiomeType)miType; } }
        public TextType mName = TextType.NONE;
        public override void ReadData(XmlNode node, Infos infos)
        {
            infos.readType(node, "Name", ref mName);
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
        public int BAI_MINOR_CITY_IGNORE_IMPASSABLE = 0;
        public int BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS = 0;
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
            BAI_MINOR_CITY_IGNORE_IMPASSABLE = infos.getGlobalInt("BAI_MINOR_CITY_IGNORE_IMPASSABLE");
            BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS = infos.getGlobalInt("BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS");
        }
    }
/*####### Better Old World AI - Base DLL #######
  ### [multiple]                         END ###
  ##############################################*/

}
