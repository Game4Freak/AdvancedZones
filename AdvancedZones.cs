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
        public const string VERSION = "0.4.0.0";
        private int frame;

        /**
         * TODO:
         * Message on Enter / Leave
         * Group on Enter / Leave
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

        private void Update()
        {
            // TODO: set with command
            frame++;
            if (frame % 10 != 0) return;

            // Player Equip
            foreach (var player in Provider.clients)
            {
                if (player.player.equipment.isSelected && playerInZoneType(UnturnedPlayer.FromSteamPlayer(player), Zone.noItemEquip))
                {
                    onPlayerEquiped(player.player, player.player.equipment);
                }
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

        /*private void onPlayerUpdatePosition(UnturnedPlayer player, Vector3 position)
        {
            if (Configuration.Instance.UsePlayerMovementUpdate)
            {
                if (lastpositon.TryGetValue(int.Parse(player.CSteamID.ToString()), out Vector3 lastP))
                {
                    List<Zone> lastPositionZones = getPositionZones(lastP);
                    List<Zone> PositonZones = getPositionZones(player.Position);
                    if (lastPositionZones != PositonZones)
                    {
                        foreach (var lz in lastPositionZones)
                        {
                            foreach (var z in PositonZones)
                            {
                                if (lz.getName() == z.getName())
                                {
                                    lastPositionZones.Remove(lz);
                                    PositonZones.Remove(z);
                                    break;
                                }
                                
                            }
                        }
                        foreach (var z in lastPositionZones)
                        {
                            if (z.hasFlag(Zone.canMessageEnterNLeft))
                            {
                                UnturnedChat.Say(player, "Now leaving zone: " + z.getName(), UnturnedChat.GetColorFromRGB(255, 87, 51));
                            }
                        }
                        foreach (var z in PositonZones)
                        {
                            if (z.hasFlag(Zone.canMessageEnterNLeft))
                            {
                                UnturnedChat.Say(player, "Now entering zone: " + z.getName(), UnturnedChat.GetColorFromRGB(46, 204, 113));
                            }
                        }
                    }
                    lastpositon.Remove(int.Parse(player.CSteamID.ToString()));
                    lastpositon.Add(int.Parse(player.CSteamID.ToString()), player.Position);
                } else
                {
                    foreach (var z in getPositionZones(player.Position))
                    {
                        if (z.hasFlag(Zone.canMessageEnterNLeft))
                        {
                            UnturnedChat.Say(player, "Now entering zone: " + z.getName(), UnturnedChat.GetColorFromRGB(46, 204, 113));
                        }
                    }
                    lastpositon.Add(int.Parse(player.CSteamID.ToString()), player.Position);
                }

            }
        }*/
        

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
            } else
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