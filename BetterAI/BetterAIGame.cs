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

        //lines 11904-11983
        public override ReligionType doReligionFound(PlayerType ePlayer, bool bTestPrereq, City pCity = null)
        {
            ReligionType eBestReligion = ReligionType.NONE;
            City pBestCity = null;
            int iBestValue = int.MinValue;

            void checkCityValue(City pTestCity)
            {
                for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); ++eLoopReligion)
                {
                    if (!infos().religion(eLoopReligion).mbHidden && isWorldReligion(eLoopReligion) && pTestCity.canFoundReligion(eLoopReligion, bTestPrereq))
                    {
                        int iValue = 0;

/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion       START ###
  ##############################################*/
                        if (!(pTestCity.isReligionHolyCityAny()))
                        {
                            //iValue += 64000;
                            iValue += 128000;
                        }

                        if (pTestCity.player().countWorldReligionsWithHolyCities() == 0)
                        {
                            //iValue += 32000;
                            iValue += 64000;
                        }

                        if (!(pTestCity.isCapital()))
                        {
                            //iValue += 16000;
                            iValue += 32000;

                        }

                        //Dynasty preference > isHuman()
                        if (pTestCity.isHuman())
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
                        if (pTestCity.getPlayer() != PlayerType.NONE && pTestCity.player().hasDynasty() && pTestCity.player().dynasty().mePreferredReligion != ReligionType.NONE)
                        {
                            if (pTestCity.player().dynasty().mePreferredReligion == eLoopReligion)
                            {
                                iValue += 8000;
                            }
                            else
                            {
                                iValue -= 8000;
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion         END ###
  ##############################################*/

                        if (!bTestPrereq)
                        {
                            if (pTestCity.canFoundReligion(eLoopReligion, true))
                            {
                                iValue += 4000;
                            }
                            else if (canFoundReligion(eLoopReligion, true))
                            {
                                iValue += 2000;
                            }
                        }

                        iValue += randomNext(1000);

                        if (iValue > iBestValue)
                        {
                            eBestReligion = eLoopReligion;
                            pBestCity = pTestCity;
                            iBestValue = iValue;
                        }
                    }
                }
            }

            if (pCity != null)
            {
                checkCityValue(pCity);
            }
            else if (ePlayer != PlayerType.NONE)
            {
                foreach (int iCityID in player(ePlayer).getCities())
                {
                    checkCityValue(city(iCityID));
                }
            }
            else
            {
                foreach (City pLoopCity in getCities())
                {
                    checkCityValue(pLoopCity);
                }
            }

            if (pBestCity != null)
            {
                pBestCity.foundReligion(eBestReligion, bTestPrereq);
            }

            return eBestReligion;
        }

    }
}
