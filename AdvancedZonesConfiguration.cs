using Rocket.API;
using System.Collections.Generic;

namespace Game4Freak.AdvancedZones
{
    public class AdvancedZonesConfiguration : IRocketPluginConfiguration
    {
        public List<int> BlockedBuildables;
        public List<int> BlockedEquiptables;
        public List<string> ZoneNames;
        public List<List<float[]>> ZoneNodes;
        public List<List<int>> ZoneFlags;

        public void LoadDefaults()
        {
            BlockedBuildables = new List<int>();
            BlockedEquiptables = new List<int>();
            ZoneNames = new List<string>();
            ZoneNodes = new List<List<float[]>>();
            ZoneFlags = new List<List<int>>();
        }
    }
}
