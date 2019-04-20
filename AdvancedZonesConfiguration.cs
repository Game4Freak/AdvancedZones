using Rocket.API;
using System.Collections.Generic;

namespace Game4Freak.AdvancedZones
{
    public class AdvancedZonesConfiguration : IRocketPluginConfiguration
    {
        public string version;
        // Buildables
        public List<string> BlockedBuildablesListNames;
        public List<List<int>> BlockedBuildables;
        // Equip
        public List<string> BlockedEquipListNames;
        public List<List<int>> BlockedEquip;
        // Zones
        public List<string> ZoneNames;
        public List<List<float[]>> ZoneNodes;
        public List<List<int>> ZoneFlags;
        public List<List<string>> ZoneBlockedBuildables;
        public List<List<string>> ZoneBlockedEquip;
        public List<List<string>> ZoneEnterAddGroups;
        public List<List<string>> ZoneEnterRemoveGroups;
        public List<List<string>> ZoneLeaveAddGroups;
        public List<List<string>> ZoneLeaveRemoveGroups;
        public List<List<string>> ZoneEnterMessages;
        public List<List<string>> ZoneLeaveMessages;

        public void LoadDefaults()
        {
            //version = AdvancedZones.VERSION;
            version = "";
            // Buildables
            BlockedBuildablesListNames = new List<string> { "ALL", "VanillaStructures", "VanillaBarricades", "Ignore_Book" };
            BlockedBuildables = new List<List<int>> { new List<int>(), new List<int> { 31, 32, 33, 34, 35, 36, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 316, 317, 318, 319, 320, 321, 322, 323, 324, 369, 370, 371, 372, 373, 374, 375, 376, 377, 442,
                    443, 444, 445, 446 ,447, 449, 450, 452, 453, 454, 1210, 1211, 1212, 1213, 1214, 1215, 1216, 1262, 1263, 1264, 1265, 1266, 1267, 1268, 1269, 1414, 1415, 1416, 1417, 1418 },
                new List<int> { 29, 30, 45, 46, 47, 48, 281, 282, 283, 284, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 325, 326, 237, 331, 359, 360, 361, 362, 365, 378, 379, 451, 455, 456, 456, 459, 1050, 1058, 1059, 1060, 1061, 1062, 1063,
                    1064, 1065, 1066, 1067, 1068, 1069, 1070, 1071, 1072, 1073, 1074, 1075, 1083, 1084, 1085, 1086, 1087, 1088, 1089, 1090, 1091, 1092, 1093, 1094, 1095, 1096, 1097, 1098, 1099, 1111, 1112, 1144, 1145, 1146, 1147, 1148, 1149, 1150,
                    1151, 1152, 1153, 1154, 1155, 1158, 1208, 1217, 1218, 1219, 1222, 1223, 1224, 1225, 1226, 1227, 1231, 1232, 1233, 1234, 1235, 1236, 1237, 1238, 1239, 1243, 1250, 1255, 1256, 1261, 1282, 1284, 1285, 1286, 1287, 1288, 1289, 1290,
                    1291, 1292, 1293, 1294, 1295, 1296, 1297, 1298, 1299, 1303, 1304, 1305, 1306, 1307, 1308, 1309, 1310, 1311, 1312, 1313, 1314, 1315, 1316, 1317, 1318, 1319, 1320, 1327, 1328, 1329, 1330, 1331, 1332, 1345, 1396, 1397, 1408, 1409,
                    1466, 1467, 1468, 1469, 1470, 1500 },
                new List<int> { 1327 } };
            // Equip
            BlockedEquipListNames = new List<string> { "ALL", "VanillaWeapons", "Ignore_Falcon" };
            BlockedEquip = new List<List<int>> { new List<int>(), new List<int> { 4, 18, 97, 99, 101, 107, 109, 112, 116, 122, 126, 129, 132, 297, 300, 346, 353, 355, 356, 357, 363, 380, 474, 479, 480, 484, 488, 519, 1000, 1018, 1021, 1024, 1027,
                    1037, 1039, 1041, 1143, 1165, 1300, 1337, 1360, 1362, 1364, 1366,  1369, 1375, 1377, 1379, 1382, 1394, 1436, 1441, 1447, 1471, 1476, 1477, 1480, 1481, 1484, 15036, 15039, 15041, 15044, 15048, 15050, 15053, 23001, 2316 },
                new List<int> { 488 } };
            // Zones
            ZoneNames = new List<string>();
            ZoneNodes = new List<List<float[]>>();
            ZoneFlags = new List<List<int>>();
            ZoneBlockedBuildables = new List<List<string>>();
            ZoneBlockedEquip = new List<List<string>>();
            ZoneEnterAddGroups = new List<List<string>>();
            ZoneEnterRemoveGroups = new List<List<string>>();
            ZoneLeaveAddGroups = new List<List<string>>();
            ZoneLeaveRemoveGroups = new List<List<string>>();
            ZoneEnterMessages = new List<List<string>>();
            ZoneLeaveMessages = new List<List<string>>();
        }
    }
}
