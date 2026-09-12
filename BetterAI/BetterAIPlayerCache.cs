using Mohawk.SystemCore;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TenCrowns.GameCore;

namespace BetterAI
{
    public class BetterAIPlayerCache
    {
        //does not inherit from PlayerCache and is supposed to be used in addition to it, not instead

        protected ConcurrentDictionary<(YieldType, int), int> mdCityYieldSpecializationModifiers = new ConcurrentDictionary<(YieldType, int), int>();


        protected ConcurrentDictionary<int, int> mdCachedTurnsLeftGeneralX10 = new ConcurrentDictionary<int, int>();
        protected ConcurrentDictionary<int, int> mdCachedTurnsLeftAnyJobX10 = new ConcurrentDictionary<int, int>();
        protected ConcurrentDictionary<int, int> mdCachedTurnsLeftLifeX10 = new ConcurrentDictionary<int, int>();

        public virtual void clear()
        {
            mdCityYieldSpecializationModifiers.Clear();
            mdCachedTurnsLeftGeneralX10.Clear();
            mdCachedTurnsLeftAnyJobX10.Clear();
            mdCachedTurnsLeftLifeX10.Clear();
        }

        public BetterAIPlayerCache()
        {
        }

        public virtual bool getCityYieldSpecializationModifier(YieldType eYield, int iCityID, out int iModifier)
        {
            return mdCityYieldSpecializationModifiers.TryGetValue((eYield, iCityID), out iModifier);
        }


        public virtual bool getCharacterGeneralTurnsLeftX10(int iCharacterID, out int iTurnsLeft)
        {
            return mdCachedTurnsLeftGeneralX10.TryGetValue(iCharacterID, out iTurnsLeft);
        }
        public virtual bool getCharacterAnyJobTurnsLeftX10(int iCharacterID, out int iTurnsLeft)
        {
            return mdCachedTurnsLeftAnyJobX10.TryGetValue(iCharacterID, out iTurnsLeft);
        }
        public virtual bool getCharacterLifeTurnsLeftX10(int iCharacterID, out int iTurnsLeft)
        {
            return mdCachedTurnsLeftLifeX10.TryGetValue(iCharacterID, out iTurnsLeft);
        }

        public virtual void setCityYieldSpecializationModifier(YieldType eYield, int iCityID, int iModifier)
        {
            mdCityYieldSpecializationModifiers[(eYield, iCityID)] = iModifier;
        }


        public virtual void setCharacterGeneralTurnsLeftX10(int iCharacterID, int iTurnsLeft)
        {
            mdCachedTurnsLeftGeneralX10[iCharacterID]= iTurnsLeft;
        }
        public virtual void setCharacterAnyJobTurnsLeftX10(int iCharacterID, int iTurnsLeft)
        {
            mdCachedTurnsLeftAnyJobX10[iCharacterID] = iTurnsLeft;
        }
        public virtual void setCharacterLifeTurnsLeftX10(int iCharacterID, int iTurnsLeft)
        {
            mdCachedTurnsLeftLifeX10[iCharacterID] = iTurnsLeft;
        }

    }
}
