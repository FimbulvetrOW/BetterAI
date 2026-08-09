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

        ConcurrentDictionary<(YieldType, int), int> mdCityYieldSpecializationModifiers = new ConcurrentDictionary<(YieldType, int), int>();

        public virtual void clear()
        {
            mdCityYieldSpecializationModifiers.Clear();
        }

        public BetterAIPlayerCache()
        {
        }

        public virtual bool getCityYieldSpecializationModifier(YieldType eYield, int iCityID, out int iModifier)
        {
            return mdCityYieldSpecializationModifiers.TryGetValue((eYield, iCityID), out iModifier);
        }

        public virtual void setCityYieldSpecializationModifier(YieldType eYield, int iCityID, int iModifier)
        {
            mdCityYieldSpecializationModifiers[(eYield, iCityID)] = iModifier;
        }

    }
}
