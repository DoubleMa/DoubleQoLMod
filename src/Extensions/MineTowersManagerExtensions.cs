using Mafi;
using Mafi.Collections;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DoubleQoL.Extensions
{
    public static class MineTowersManagerExtensions
    {
        public static Lyst<MineTower> getTowers(this MineTowersManager mineTowersManager)
        {
            List<MineTower> result = mineTowersManager.Towers.Select(x => x).ToList();
            result.Sort((t1, t2) => t1.GetTitle().CompareTo(t2.GetTitle()));
            return result.ToLyst();
        }
    }
}