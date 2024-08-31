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
