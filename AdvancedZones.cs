using Rocket.API;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Game4Freak.AdvancedZones
{
    public class AdvancedZones : RocketPlugin<AdvancedZonesConfiguration>
    {
        public static AdvancedZones Instance;
        public const string VERSION = "0.5.0.0";
        private int frame;
        private Dictionary<string, Vector3> lastPosition;

        /**
         * TODO:
         * Message on Enter / Leave (custom messages)
         * noZombie Flag
         * Translations
         * Node visuals (brick pillar, maybe map-nodes)
         * 
         * NOTES:
         * "IGNORE" in Buildables List --> all other listed ids - ignored ids
         * "ALL" in Buildables List --> all barricades + all structures
         * permissions with zonenames for custom override (eg. advancedzones.override.build.testZone for building in the zone testZone)
         * DEBUG: foreach (var i in Provider.clients) UnturnedChat.Say(UnturnedPlayer.FromSteamPlayer(i), "");
         **/

        protected override void Load()
        {
            Instance = this;
            Logger.Log("AdvancedZones v" + VERSION);

            // Update config
            if (Configuration.Instance.version != VERSION)
            {
                updateConfig();
                Configuration.Instance.version = VERSION;
            }

            lastPosition = new Dictionary<string, Vector3>();
            foreach (var splayer in Provider.clients)
            {
                onPlayerConnection(UnturnedPlayer.FromSteamPlayer(splayer));
            }
            
            // Enter / Leave
            U.Events.OnPlayerConnected += onPlayerConnection;
            U.Events.OnPlayerDisconnected += onPlayerDisconnection;
            // Block Damage
            BarricadeManager.onDamageBarricadeRequested += onBarricadeDamage;
            StructureManager.onDamageStructureRequested += onStructureDamage;
            VehicleManager.onVehicleLockpicked += onVehicleLockpick;
            VehicleManager.onDamageVehicleRequested += onVehicleDamage;
            DamageTool.playerDamaged += onPlayerDamage;
            VehicleManager.onDamageTireRequested += onTireDamage;
            // Block Buildable
            BarricadeManager.onDeployBarricadeRequested += onBarricadeDeploy;
            StructureManager.onDeployStructureRequested += onStructureDepoly;
        }

        protected override void Unload()
        {
            lastPosition.Clear();

            // Enter / Leave
            U.Events.OnPlayerConnected -= onPlayerConnection;
            U.Events.OnPlayerDisconnected -= onPlayerDisconnection;
            // Block Damage
            BarricadeManager.onDamageBarricadeRequested -= onBarricadeDamage;
            StructureManager.onDamageStructureRequested -= onStructureDamage;
            VehicleManager.onVehicleLockpicked -= onVehicleLockpick;
            VehicleManager.onDamageVehicleRequested -= onVehicleDamage;
            DamageTool.playerDamaged -= onPlayerDamage;
            VehicleManager.onDamageTireRequested -= onTireDamage;
            // Block Buildable
            BarricadeManager.onDeployBarricadeRequested -= onBarricadeDeploy;
            StructureManager.onDeployStructureRequested -= onStructureDepoly;
        }

        private void updateConfig()
        {
            Logger.Log("Updating plugin config");
            if (Configuration.Instance.ZoneNodes.Count < Configuration.Instance.ZoneNames.Count)
            {
                for (int i = 0; i < (Configuration.Instance.ZoneNames.Count - Configuration.Instance.ZoneNodes.Count); i++)
                {
                    Configuration.Instance.ZoneNodes.Add(new List<float[]>());
                }
            }
            if (Configuration.Instance.ZoneFlags.Count < Configuration.Instance.ZoneNames.Count)
            {
                for (int i = 0; i < (Configuration.Instance.ZoneNames.Count - Configuration.Instance.ZoneFlags.Count); i++)
                {
                    Configuration.Instance.ZoneFlags.Add(new List<int>());
                }
            }
            if (Configuration.Instance.ZoneBlockedBuildables.Count < Configuration.Instance.ZoneNames.Count)
            {
                for (int i = 0; i < (Configuration.Instance.ZoneNames.Count - Configuration.Instance.ZoneBlockedBuildables.Count); i++)
                {
                    Configuration.Instance.ZoneBlockedBuildables.Add(new List<string>());
                }
            }
            if (Configuration.Instance.ZoneBlockedEquip.Count < Configuration.Instance.ZoneNames.Count)
            {
                for (int i = 0; i < (Configuration.Instance.ZoneNames.Count - Configuration.Instance.ZoneBlockedEquip.Count); i++)
                {
                    Configuration.Instance.ZoneBlockedEquip.Add(new List<string>());
                }
            }
            if (Configuration.Instance.ZoneEnterAddGroups.Count < Configuration.Instance.ZoneNames.Count)
            {
                for (int i = 0; i < (Configuration.Instance.ZoneNames.Count - Configuration.Instance.ZoneEnterAddGroups.Count); i++)
                {
                    Configuration.Instance.ZoneEnterAddGroups.Add(new List<string>());
                }
            }
            if (Configuration.Instance.ZoneEnterRemoveGroups.Count < Configuration.Instance.ZoneNames.Count)
            {
                for (int i = 0; i < (Configuration.Instance.ZoneNames.Count - Configuration.Instance.ZoneEnterRemoveGroups.Count); i++)
                {
                    Configuration.Instance.ZoneEnterRemoveGroups.Add(new List<string>());
                }
            }
            if (Configuration.Instance.ZoneLeaveAddGroups.Count < Configuration.Instance.ZoneNames.Count)
            {
                for (int i = 0; i < (Configuration.Instance.ZoneNames.Count - Configuration.Instance.ZoneLeaveAddGroups.Count); i++)
                {
                    Configuration.Instance.ZoneLeaveAddGroups.Add(new List<string>());
                }
            }
            if (Configuration.Instance.ZoneLeaveRemoveGroups.Count < Configuration.Instance.ZoneNames.Count)
            {
                for (int i = 0; i < (Configuration.Instance.ZoneNames.Count - Configuration.Instance.ZoneLeaveRemoveGroups.Count); i++)
                {
                    Configuration.Instance.ZoneLeaveRemoveGroups.Add(new List<string>());
                }
            }
        }

        private void Update()
        {
            // TODO: set with command
            frame++;
            if (frame % 10 != 0) return;

            foreach (var splayer in Provider.clients)
            {
                Vector3 lastPos;
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(splayer);
                // Enter / Leave region
                if (!lastPosition.ContainsKey(player.Id))
                {
                    onPlayerConnection(player);
                }
                else
                {
                    lastPosition.TryGetValue(player.Id, out lastPos);
                    if (!lastPos.Equals(player.Position))
                    {
                        List<string> lastZoneNames = new List<string>();
                        foreach (var zone in getPositionZones(lastPos))
                        {
                            lastZoneNames.Add(zone.getName());
                        }
                        List<string> currentZoneNames = new List<string>();
                        foreach (var zone in getPositionZones(player.Position))
                        {
                            currentZoneNames.Add(zone.getName());
                        }
                        foreach (var zoneName in lastZoneNames.Except(currentZoneNames))
                        {
                            // Leaving
                            onZoneLeave(player, getZoneByName(zoneName), lastPos);
                        }
                        foreach (var zoneName in currentZoneNames.Except(lastZoneNames))
                        {
                            // Entering
                            onZoneEnter(player, getZoneByName(zoneName), lastPos);
                        }
                    }
                    lastPosition[player.Id] = player.Position;
                }

                // Player Equip
                if (player.Player.equipment.isSelected && playerInZoneType(player, Zone.noItemEquip))
                {
                    onPlayerEquiped(player.Player, player.Player.equipment);
                }
            }
            /*List<Zombie> zombies = new List<Zombie>();
            ZombieManager.getZombiesInRadius(new Vector3(0, 0, 0), 1000000, result: zombies);
            foreach (var zombie in zombies)
            {
                ZombieManager.sendZombieDead(zombie, zombie.transform.position);
                zombie.shirt = 166;
            }*/
        }

        private void onZoneLeave(UnturnedPlayer player, Zone zone, Vector3 lastPos)
        {
            if (zone.hasFlag(Zone.noLeave))
            {
                if (!player.HasPermission("advancedzones.override.noleave") && !player.HasPermission("advancedzones.override.noleave." + zone.getName().ToLower()))
                {
                    player.Teleport(new Vector3(lastPos.x, lastPos.y - (float)0.6, lastPos.z), player.Rotation);
                    return;
                }
            }
            if (zone.hasFlag(Zone.leaveMessage))
            {
                UnturnedChat.Say(player, "Now leaving the zone: " + zone.getName(), UnityEngine.Color.green);
            }
            if (zone.hasFlag(Zone.leaveRemoveGroup))
            {
                foreach (var group in zone.getLeaveRemoveGroups())
                {
                    R.Permissions.RemovePlayerFromGroup(group, player);
                }
            }
            if (zone.hasFlag(Zone.leaveAddGroup))
            {
                foreach (var group in zone.getLeaveAddGroups())
                {
                    R.Permissions.RemovePlayerFromGroup(group, player);
                }
            }
        }

        private void onZoneEnter(UnturnedPlayer player, Zone zone, Vector3 lastPos)
        {
            if (zone.hasFlag(Zone.noEnter))
            {
                if (!player.HasPermission("advancedzones.override.noenter") && !player.HasPermission("advancedzones.override.noenter." + zone.getName().ToLower()))
                {
                    player.Teleport(new Vector3(lastPos.x, lastPos.y - (float)0.6, lastPos.z), player.Rotation);
                    return;
                }
            }
            if (zone.hasFlag(Zone.enterMessage))
            {
                UnturnedChat.Say(player, "Now entering the zone: " + zone.getName(), UnityEngine.Color.green);
            }
            if (zone.hasFlag(Zone.enterRemoveGroup))
            {
                foreach (var group in zone.getEnterRemoveGroups())
                {
                    R.Permissions.RemovePlayerFromGroup(group, player);
                }
            }
            if (zone.hasFlag(Zone.enterAddGroup))
            {
                foreach (var group in zone.getEnterAddGroups())
                {
                    R.Permissions.AddPlayerToGroup(group, player);
                }
            }

        }

        private void onPlayerDisconnection(UnturnedPlayer player)
        {
            lastPosition.Remove(player.Id);
            foreach (var zone in getPositionZones(player.Position))
            {
                onZoneLeave(player, zone, player.Position);
            }
        }

        private void onPlayerConnection(UnturnedPlayer player)
        {
            lastPosition.Add(player.Id, player.Position);
            foreach (var zone in getPositionZones(player.Position))
            {
                onZoneEnter(player, zone, player.Position);
            }
        }

        private void onTireDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, int tireIndex, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (transformInZoneType(vehicle.transform, Zone.noTireDamage) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.tiredamage"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.noTireDamage) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.tiredamage." + zone.getName()).ToLower()))
                    {
                        shouldAllow = false;
                    }
                }
            }
            else
            {
                shouldAllow = true;
            }
        }

        private void onPlayerEquiped(Player player, PlayerEquipment equipment)
        {
            if (!UnturnedPlayer.FromPlayer(player).HasPermission("advancedzones.override.equip"))
            {
                List<Zone> currentZones = getPositionZones(player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.noItemEquip))
                    {
                        if (!UnturnedPlayer.FromPlayer(player).HasPermission(("advancedzones.override.equip." + zone.getName()).ToLower()))
                        {
                            List<string> currentBlockedEquip = zone.getBlockedEquips();
                            List<string> currentIgnoredBlockedEquip = new List<string>();
                            for (int i = 0; i < currentBlockedEquip.Count; i++)
                            {
                                if (currentBlockedEquip.ElementAt(i).ToLower().Contains("ignore"))
                                {
                                    currentIgnoredBlockedEquip.Add(currentBlockedEquip.ElementAt(i));
                                    currentBlockedEquip.Remove(currentBlockedEquip.ElementAt(i));
                                }
                            }

                            if (currentIgnoredBlockedEquip.Count > 0)
                            {
                                foreach (var ignoredBlockedEquip in currentIgnoredBlockedEquip)
                                {
                                    if (Configuration.Instance.BlockedEquip.ElementAt(Configuration.Instance.BlockedEquipListNames.IndexOf(ignoredBlockedEquip)).Contains(equipment.asset.id)) return;
                                }
                            }

                            if (currentBlockedEquip.Contains("ALL"))
                            {
                                equipment.dequip();
                                return;
                            }

                            foreach (var blockedEquip in currentBlockedEquip)
                            {
                                if (Configuration.Instance.BlockedEquip.ElementAt(Configuration.Instance.BlockedEquipListNames.IndexOf(blockedEquip)).Contains(equipment.asset.id))
                                {
                                    equipment.dequip();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onPlayerDamage(Player player, ref EDeathCause cause, ref ELimb limb, ref CSteamID killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage)
        {
            if (cause == EDeathCause.BLEEDING || cause == EDeathCause.BONES || cause == EDeathCause.BREATH || cause == EDeathCause.BURNING || cause == EDeathCause.FOOD || cause == EDeathCause.FREEZING 
                || cause == EDeathCause.INFECTION || cause == EDeathCause.ARENA || cause == EDeathCause.KILL || cause == EDeathCause.SUICIDE || cause == EDeathCause.WATER)
            {
                return;
            }
            if (UnturnedPlayer.FromCSteamID(killer).Player == null && playerInZoneType(UnturnedPlayer.FromPlayer(player), Zone.noPlayerDamage))
            {
                if (cause == EDeathCause.ZOMBIE)
                {
                    UnturnedPlayer.FromPlayer(player).Infection = 0;
                }
                damage = 0;
                canDamage = false;
                return;
            }
            else if (UnturnedPlayer.FromCSteamID(killer).Player == null)
            {
                return;
            }
            if ((playerInZoneType(UnturnedPlayer.FromPlayer(player), Zone.noPlayerDamage) || playerInZoneType(UnturnedPlayer.FromCSteamID(killer), Zone.noPlayerDamage)) && !UnturnedPlayer.FromCSteamID(killer).HasPermission("advancedzones.override.playerdamage"))
            {
                List<Zone> currentZones = getPositionZones(player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.noPlayerDamage) && !UnturnedPlayer.FromCSteamID(killer).HasPermission(("advancedzones.override.playerdamage." + zone.getName()).ToLower()))
                    {
                        damage = 0;
                        canDamage = false;
                    }
                }
            }
        }

        private void onStructureDepoly(Structure structure, ItemStructureAsset asset, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission("advancedzones.override.build"))
            {
                List<Zone> currentZones = getPositionZones(point);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.noBuild))
                    {
                        if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission(("advancedzones.override.build." + zone.getName()).ToLower()))
                        {
                            List<string> currentBlockedBuildables = zone.getBlockedBuildables();
                            List<string> currentIgnoredBlockedBuildables = new List<string>();
                            for (int i = 0; i < currentBlockedBuildables.Count; i++)
                            {
                                if (currentBlockedBuildables.ElementAt(i).ToLower().Contains("ignore"))
                                {
                                    currentIgnoredBlockedBuildables.Add(currentBlockedBuildables.ElementAt(i));
                                    currentBlockedBuildables.Remove(currentBlockedBuildables.ElementAt(i));
                                }
                            }

                            if (currentIgnoredBlockedBuildables.Count > 0)
                            {
                                foreach (var ignoredBlockedBuildable in currentIgnoredBlockedBuildables)
                                {
                                    if (Configuration.Instance.BlockedBuildables.ElementAt(Configuration.Instance.BlockedBuildablesListNames.IndexOf(ignoredBlockedBuildable)).Contains(asset.id)) return;
                                }
                            }
                            if (currentBlockedBuildables.Contains("ALL"))
                            {
                                shouldAllow = false;
                                return;
                            }

                            foreach (var blockedBuildable in currentBlockedBuildables)
                            {
                                if (Configuration.Instance.BlockedBuildables.ElementAt(Configuration.Instance.BlockedBuildablesListNames.IndexOf(blockedBuildable)).Contains(asset.id))
                                {
                                    shouldAllow = false;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission("advancedzones.override.build"))
            {
                List<Zone> currentZones = getPositionZones(point);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.noBuild))
                    {
                        if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission(("advancedzones.override.build." + zone.getName()).ToLower()))
                        {
                            List<string> currentBlockedBuildables = zone.getBlockedBuildables();
                            List<string> currentIgnoredBlockedBuildables = new List<string>();
                            for(int i = 0; i < currentBlockedBuildables.Count; i++)
                            {
                                if (currentBlockedBuildables.ElementAt(i).ToLower().Contains("ignore"))
                                {
                                    currentIgnoredBlockedBuildables.Add(currentBlockedBuildables.ElementAt(i));
                                    currentBlockedBuildables.Remove(currentBlockedBuildables.ElementAt(i));
                                }
                            }

                            if (currentIgnoredBlockedBuildables.Count > 0)
                            {
                                foreach (var ignoredBlockedBuildable in currentIgnoredBlockedBuildables)
                                {
                                    if (Configuration.Instance.BlockedBuildables.ElementAt(Configuration.Instance.BlockedBuildablesListNames.IndexOf(ignoredBlockedBuildable)).Contains(asset.id)) return;
                                }
                            }

                            if (currentBlockedBuildables.Contains("ALL"))
                            {
                                shouldAllow = false;
                                return;
                            }

                            foreach (var blockedBuildable in currentBlockedBuildables)
                            {
                                if (Configuration.Instance.BlockedBuildables.ElementAt(Configuration.Instance.BlockedBuildablesListNames.IndexOf(blockedBuildable)).Contains(asset.id))
                                {
                                    shouldAllow = false;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onVehicleLockpick(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow)
        {
            if (transformInZoneType(vehicle.transform, Zone.noLockpick) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission("advancedzones.override.lockpick"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.noVehicleDamage) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission(("advancedzones.override.lockpick." + zone.getName()).ToLower()))
                    {
                        allow = false; ;
                    }
                }
            }
            else
            {
                allow = true;
            }
        }

        private void onVehicleDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if ((transformInZoneType(vehicle.transform, Zone.noVehicleDamage) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.vehicledamage")) && pendingTotalDamage > 0)
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.noVehicleDamage) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.vehicledamage." + zone.getName()).ToLower()))
                    {
                        shouldAllow = false;
                    }
                }
            }
            else
            {
                shouldAllow = true;
            }
        }

        private void onStructureDamage(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (transformInZoneType(structureTransform, Zone.noDamage))
            {
                if (UnturnedPlayer.FromCSteamID(instigatorSteamID) != null)
                {
                    if (!UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.damage") && pendingTotalDamage > 0)
                    {
                        List<Zone> currentZones = getPositionZones(structureTransform.transform.position);
                        foreach (var zone in currentZones)
                        {
                            if (zone.hasFlag(Zone.noDamage) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.damage." + zone.getName()).ToLower()))
                            {
                                shouldAllow = false;
                            }
                        }
                    }
                }
                else if (damageOrigin.ToString() == "Bullet_Explosion"
                || damageOrigin.ToString() == "Charge_Explosion"
                || damageOrigin.ToString() == "Food_Explosion"
                || damageOrigin.ToString() == "Rocket_Explosion"
                || damageOrigin.ToString() == "Sentry"
                || damageOrigin.ToString() == "Trap_Explosion"
                || damageOrigin.ToString() == "Vehicle_Explosion"
                || damageOrigin.ToString() == "Zombie_Swipe")
                {
                    shouldAllow = false;
                }
            }
        }

        private void onBarricadeDamage(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (transformInZoneType(barricadeTransform, Zone.noDamage))
            {
                if (UnturnedPlayer.FromCSteamID(instigatorSteamID) != null)
                {
                    if (!UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.damage") && pendingTotalDamage > 0)
                    {
                        if (barricadeTransform.name.ToString() != "1102"
                                && barricadeTransform.name.ToString() != "1101"
                                && barricadeTransform.name.ToString() != "1393"
                                && barricadeTransform.name.ToString() != "1241")
                        {
                            List<Zone> currentZones = getPositionZones(barricadeTransform.transform.position);
                            foreach (var zone in currentZones)
                            {
                                if (zone.hasFlag(Zone.noDamage) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.damage." + zone.getName()).ToLower()))
                                {
                                    shouldAllow = false;
                                }
                            }
                        }
                    }
                }
                else if ((damageOrigin.ToString() == "Bullet_Explosion"
                    || (damageOrigin.ToString() == "Charge_Explosion")
                    || damageOrigin.ToString() == "Food_Explosion"
                    || damageOrigin.ToString() == "Rocket_Explosion"
                    || damageOrigin.ToString() == "Sentry"
                    || damageOrigin.ToString() == "Trap_Explosion"
                    || damageOrigin.ToString() == "Vehicle_Explosion"
                    || damageOrigin.ToString() == "Zombie_Swipe") &&
                    (barricadeTransform.name.ToString() != "1102"
                    && barricadeTransform.name.ToString() != "1101"
                    && barricadeTransform.name.ToString() != "1393"
                    && barricadeTransform.name.ToString() != "1241"))
                {
                    shouldAllow = false;
                }
            }
        }        

        public List<Zone> convertConfigToZone()
        {
            int x = 0;
            List<Zone> zoneList = new List<Zone>();
            foreach (var z in Configuration.Instance.ZoneNames)
            {
                Zone temp = new Zone(z);
                foreach (var n in Configuration.Instance.ZoneNodes.ElementAt(x))
                {
                    temp.addNode(new Node(n[0], n[1], n[2]));
                }
                foreach (var f in Configuration.Instance.ZoneFlags.ElementAt(x))
                {
                    temp.addFlag(f);
                }
                foreach (var bE in Configuration.Instance.ZoneBlockedEquip.ElementAt(x))
                {
                    temp.addBlockedEquip(bE);
                }
                foreach (var bB in Configuration.Instance.ZoneBlockedBuildables.ElementAt(x))
                {
                    temp.addBlockedBuildable(bB);
                }
                foreach (var eAG in Configuration.Instance.ZoneEnterAddGroups.ElementAt(x))
                {
                    temp.addEnterAddGroup(eAG);
                }
                foreach (var eRG in Configuration.Instance.ZoneEnterRemoveGroups.ElementAt(x))
                {
                    temp.addEnterRemoveGroup(eRG);
                }
                foreach (var lAG in Configuration.Instance.ZoneLeaveAddGroups.ElementAt(x))
                {
                    temp.addLeaveAddGroup(lAG);
                }
                foreach (var lRG in Configuration.Instance.ZoneLeaveRemoveGroups.ElementAt(x))
                {
                    temp.addLeaveRemoveGroup(lRG);
                }
                zoneList.Add(temp);
                x++;
            }
            return zoneList;
        }

        public bool playerInAZone(UnturnedPlayer player)
        {
            return getPositionZones(player.Position).Count > 0;
        }

        public bool playerInZoneType(UnturnedPlayer player, int type)
        {
            List<Zone> zones = getPositionZones(player.Position);
            if (zones.Count == 0)
            {
                return false;
            } else
            {
                foreach (var z in zones)
                {
                    if (z.hasFlag(type))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public List<Zone> getPositionZones(Vector3 position)
        {
            List<Zone> zones = new List<Zone>();
            foreach (var z in convertConfigToZone())
            {
                if (z.isReady())
                {
                    float playerX = position.x;
                    float playerZ = position.z;

                    Node[] zoneNodes = z.getNodes();

                    int j = zoneNodes.Length - 1;
                    bool oddNodes = false;

                    for (int i = 0; i < zoneNodes.Length; i++)
                    {
                        if ((zoneNodes[i].getZ() < playerZ && zoneNodes[j].getZ() >= playerZ
                             || zoneNodes[j].getZ() < playerZ && zoneNodes[i].getZ() >= playerZ)
                             && (zoneNodes[i].getX() <= playerX || zoneNodes[j].getX() <= playerX))
                        {
                            if (zoneNodes[i].getX() + (playerZ - zoneNodes[i].getZ()) / (zoneNodes[j].getZ() - zoneNodes[i].getZ()) * (zoneNodes[j].getX() - zoneNodes[i].getX()) < playerX)
                            {
                                oddNodes = !oddNodes;
                            }
                        }
                        j = i;
                    }
                    if (oddNodes)
                    {
                        zones.Add(z);
                    }
                }
            }
            return zones;
        }

        public bool transformInZoneType(Transform transform, int type)
        {

            if (getPositionZones(transform.position).Count == 0)
            {
                return false;
            }
            else
            {
                foreach (var z in getPositionZones(transform.position))
                {
                    if (z.hasFlag(type))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public Zone getZoneByName(string zoneName)
        {
            foreach (var z in convertConfigToZone())
            {
                if (z.getName() == zoneName)
                {
                    return z;
                }
            }
            return null;
        }
    }
}