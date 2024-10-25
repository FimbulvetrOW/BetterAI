using System;
using System.Collections.Generic;
using UnityEngine;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;
using Mohawk.SystemCore;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml;
using Random = Mohawk.SystemCore.Random;
using BinaryReader = Mohawk.SystemCore.BinaryReader;
using BinaryWriter = Mohawk.SystemCore.BinaryWriter;

namespace BetterAI
{
    public class BetterAIGame : Game
    {
        public BetterAIGame(ModSettings pModSettings, IApplication pApp, bool bShowGame) : base(pModSettings, pApp, bShowGame)
        {
        }

/*####### Better Old World AI - Base DLL #######
  ### Better bounce tile search        START ###
  ##############################################*/
        //restored old version of this method with Predicate
        //lines 9232-9359
        public virtual Tile findUnitTileNearby(UnitType eUnit, Tile pTile, PlayerType ePlayer, TribeType eTribe, TeamType eTeamAvoid, bool bTestTile, bool bSpecialTile, int iRequiresArea, City pRequiresCity, Predicate<Tile> predicate = null, Tile pAvoidTile = null, Unit pIgnoreUnit = null)
        {
            TeamType eTeam = getPlayerTeam(ePlayer);
            bool bPredicateTrue = predicate?.Invoke(pTile) ?? false;

            int compareTiles(int iTile1, int iTile2)
            {
                Tile pTile1 = tile(iTile1);
                Tile pTile2 = tile(iTile2);

                if (pTile.isWater())
                {
                    bool bSameWater1 = pTile1.isWater() && pTile1.getArea() == pTile.getArea();
                    bool bSameWater2 = pTile2.isWater() && pTile2.getArea() == pTile.getArea();
                    if (bSameWater1 != bSameWater2)
                    {
                        return bSameWater1 ? -1 : 1;
                    }
                }
                else
                {
                    bool bSameLandSection1 = pTile1.getLandSection() == pTile.getLandSection();
                    bool bSameLandSection2 = pTile2.getLandSection() == pTile.getLandSection();
                    if (bSameLandSection1 != bSameLandSection2)
                    {
                        return bSameLandSection1 ? -1 : 1;
                    }
                }

                bool bSamePlayer1 = pTile1.getOwner() == ePlayer;
                bool bSamePlayer2 = pTile2.getOwner() == ePlayer;
                if (bSamePlayer1 != bSamePlayer2)
                {
                    return bSamePlayer1 ? -1 : 1;
                }

                bool bSameTeam1 = pTile1.getTeam() == eTeam;
                bool bSameTeam2 = pTile2.getTeam() == eTeam;
                if (bSameTeam1 != bSameTeam2)
                {
                    return bSameTeam1 ? -1 : 1;
                }

                if (ePlayer != PlayerType.NONE)
                {
                    bool bPathToCity1 = player(ePlayer).hasPathToCity(pTile1);
                    bool bPathToCity2 = player(ePlayer).hasPathToCity(pTile2);
                    if (bPathToCity1 != bPathToCity2)
                    {
                        return bPathToCity1 ? -1 : 1;
                    }
                }

                bool bAdjacentHostile1 = pTile1.adjacentToHostileUnit(eTeam, eTribe, TeamType.NONE);
                bool bAdjacentHostile2 = pTile2.adjacentToHostileUnit(eTeam, eTribe, TeamType.NONE);
                if (bAdjacentHostile1 != bAdjacentHostile2)
                {
                    return bAdjacentHostile2 ? -1 : 1;
                }

                bool bDamageTile1 = pTile1.terrain().miUnitDamage > 0;
                bool bDamageTile2 = pTile2.terrain().miUnitDamage > 0;
                if (bDamageTile1 != bDamageTile2)
                {
                    return bDamageTile2 ? -1 : 1;
                }

                return pTile1.getID().CompareTo(pTile2.getID());
            }
            bool isValidTile(Tile pLoopTile)
            {
                if (pLoopTile == pAvoidTile)
                {
                    return false;
                }
                if (!pLoopTile.canPlaceUnit(eUnit, ePlayer, eTribe, eTeamAvoid, pTile, bSpecialTile, iRequiresArea, pRequiresCity, pIgnoreUnit))
                {
                    return false;
                }
                if (bPredicateTrue && !predicate(pLoopTile))
                {
                    return false;
                }
                if (pLoopTile.hasImprovement() && pLoopTile.improvement().mbBonus)
                {
                    return false;
                }
                return true;
            }

            if (bTestTile)
            {
                if (pTile.canPlaceUnit(eUnit, ePlayer, eTribe, eTeamAvoid, pTile, bSpecialTile, iRequiresArea, pRequiresCity, pIgnoreUnit))
                {
                    return pTile;
                }
            }

            for (int iRange = 1; iRange < maxDistance(); ++iRange)
            {
                if (bPredicateTrue)
                {
                    if (iRange > (infos().unit(eUnit).miMovement * infos().unit(eUnit).miFatigue))
                    {
                        return null;
                    }
                }

                using (var tilesScoped = CollectionCache.GetListScoped<int>())
                {
                    List<int> liTiles = tilesScoped.Value;
                    pTile.getTilesAtDistance(iRange, liTiles);

                    for (int iIndex = liTiles.Count - 1; iIndex >= 0; --iIndex)
                    {
                        if (!isValidTile(tile(liTiles[iIndex])))
                        {
                            liTiles.RemoveAt(iIndex);
                        }
                    }

                    if (liTiles.Count > 0)
                    {
                        return tile(liTiles[liTiles.GetMinValueIndex(compareTiles)]);
                    }
                }
            }
            return null;
        }
/*####### Better Old World AI - Base DLL #######
  ### Better bounce tile search          END ###
  ##############################################*/

        //lines 12234-12274
        public override ReligionType doReligionFound(PlayerType ePlayer, bool bTestPrereq)
        {
/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion       START ###
  ##############################################*/
            int iBestValue = 1000;
/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion         END ###
  ##############################################*/
            ReligionType eBestReligion = ReligionType.NONE;
            City pBestCity = null;

            void checkBestValue(City pLoopCity, ReligionType eLoopReligion)
            {
                int iValue = getReligionCityFoundValue(eLoopReligion, pLoopCity, bTestPrereq);
                if (iValue > iBestValue)
                {
                    eBestReligion = eLoopReligion;
                    iBestValue = iValue;
                    pBestCity = pLoopCity;
                }
            }

            for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); ++eLoopReligion)
            {
                if (ePlayer != PlayerType.NONE)
                {
                    foreach (int iCityID in player(ePlayer).getCities())
                    {
                        checkBestValue(city(iCityID), eLoopReligion);
                    }
                }
                else
                {
                    foreach (City pLoopCity in getCities())
                    {
                        checkBestValue(pLoopCity, eLoopReligion);
                    }
                }
            }
            if (pBestCity != null)
            {
                pBestCity.foundReligion(eBestReligion, bTestPrereq);
            }

            return eBestReligion;
        }

        //lines 12275-12297
        public override ReligionType doReligionFoundCity(City pCity, bool bTestPrereq)
        {
/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion       START ###
  ##############################################*/
            int iBestValue = 1000;
/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion         END ###
  ##############################################*/
            ReligionType eBestReligion = ReligionType.NONE;

            for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); ++eLoopReligion)
            {
                int iValue = getReligionCityFoundValue(eLoopReligion, pCity, bTestPrereq);

                if (iValue > iBestValue)
                {
                    eBestReligion = eLoopReligion;
                    iBestValue = iValue;
                }
            }

            if (eBestReligion != ReligionType.NONE)
            {
                pCity.foundReligion(eBestReligion, bTestPrereq);
            }

            return eBestReligion;
        }


        //lines 12073-12117
        protected override int getReligionCityFoundValue(ReligionType eReligion, City pCity, bool bTestPrereq)
        {
            int iValue = 0;

            if (!infos().religion(eReligion).mbHidden && isWorldReligion(eReligion) && pCity.canFoundReligion(eReligion, bTestPrereq))
            {
/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion       START ###
  ##############################################*/
                if (!(pCity.isReligionHolyCityAny()))
                {
                    //iValue += 64000;
                    iValue += 128000;
                }

                if (pCity.player().countWorldReligionsWithHolyCities() == 0)
                {
                    //iValue += 32000;
                    iValue += 64000;
                }

                if (!(pCity.isCapital()))
                {
                    //iValue += 16000;
                    iValue += 32000;

                }

                //Dynasty preference > isHuman()
                if (pCity.isHuman())
                {
                    //iValue += 8000;
                    iValue += 1000;
                }


                //if (pTestCity.player().hasDynasty() && pTestCity.player().dynasty().mePreferredReligion == eLoopReligion)
                //{
                //    iValue += 4000;
                //}

                //Dynasty has no preference: +8000
                //Dynasty has preference, Religion is preference: +16000
                //Dynasty has preference, Religion is not preference: +0
                iValue += 8000;
                if (pCity.getPlayer() != PlayerType.NONE && pCity.player().hasDynasty() && pCity.player().dynasty().mePreferredReligion != ReligionType.NONE)
                {
                    if (pCity.player().dynasty().mePreferredReligion == eReligion)
                    {
                        iValue += 8000;
                    }
                    else
                    {
                        iValue -= 8000;
                    }
                }

                if (!bTestPrereq)
                {
                    if (pCity.canFoundReligion(eReligion, true))
                    {
                        //iValue += 2000;
                        iValue += 4000;
                    }
                    else if (canFoundReligion(eReligion, true))
                    {
                        //iValue += 1000;
                        iValue += 2000;
                    }
                }

                iValue += randomNext(1000);
/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion         END ###
  ##############################################*/

            }
            return iValue;
        }

    }
}
