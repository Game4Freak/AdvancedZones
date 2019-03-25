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
        public const string VERSION = "0.1.0.0";
        //TODO
        //public Dictionary<int, Vector3> lastpositon;

        /**
         * TODO:
         * Message on Enter / Leave
         * No Equip
         * 
         **/

        protected override void Load()
        {
            Instance = this;
            Logger.Log($"AdvancedZones v{VERSION}");
            
            // Block Damage
            BarricadeManager.onDamageBarricadeRequested += onBarricadeDamage;
            StructureManager.onDamageStructureRequested += onStructureDamage;
            VehicleManager.onVehicleLockpicked += onVehicleLockpick;
            VehicleManager.onDamageVehicleRequested += onVehicleDamage;
            UnturnedEvents.OnPlayerDamaged += onPlayerDamage;
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
            UnturnedEvents.OnPlayerDamaged -= onPlayerDamage;
            // Block Buildable
            BarricadeManager.onDeployBarricadeRequested -= onBarricadeDeploy;
            StructureManager.onDeployStructureRequested -= onStructureDepoly;
        }

        private void onStructureDepoly(Structure structure, ItemStructureAsset asset, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission("advancedzones.buildoverride"))
            {
                foreach (var i in Configuration.Instance.BlockedBuildables)
                {
                    if (asset.id == i)
                    {
                        List<Zone> currentZones = getPositionZones(point);
                        foreach (var zone in currentZones)
                        {
                            if (zone.hasFlag(Zone.noBuild))
                            {
                                shouldAllow = false;
                            }
                        }
                    }
                }
            }
        }

        private void onBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission("advancedzones.buildoverride"))
            {
                foreach (var i in Configuration.Instance.BlockedBuildables)
                {
                    if (asset.id == i)
                    {
                        List<Zone> currentZones = getPositionZones(point);
                        foreach (var zone in currentZones)
                        {
                            if (zone.hasFlag(Zone.noBuild))
                            {
                                shouldAllow = false;
                            }
                        }
                    }
                }
            }
        }
        
        private void onPlayerDamage(UnturnedPlayer player, ref EDeathCause cause, ref ELimb limb, ref UnturnedPlayer killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage)
        {
            if ((playerInZoneType(player, Zone.noPlayerDamage) || playerInZoneType(killer, Zone.noPlayerDamage)) && !killer.HasPermission("advancedzones.playerdamageoverride"))
            {
                // not working
                canDamage = false;
                damage = 0;
                byte healAmount = (byte)damage;
                new Thread((ThreadStart)(() =>
                {
                    Thread.Sleep(100);
                    player.Heal(healAmount);
                    player.Bleeding = false;
                })).Start();
            } else
            {
                canDamage = true;
            }
        }

        private void onVehicleLockpick(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow)
        {
            if (transformInZoneType(vehicle.transform, Zone.noLockpick) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission("advancedzones.lockpickoverride"))
            {
                allow = false;
            } else
            {
                allow = true;
            }
        }

        private void onVehicleDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if ((transformInZoneType(vehicle.transform, Zone.noVehicleDamage) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.vehicledamageoverride")) && pendingTotalDamage > 0)
            {
                shouldAllow = false;
            } else
            {
                shouldAllow = true;
            }
            //canRepair = true;
        }

        private void onStructureDamage(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (transformInZoneType(structureTransform, Zone.noDamage))
            {
                if (UnturnedPlayer.FromCSteamID(instigatorSteamID) != null)
                {
                    if (!UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.damageoverride") && pendingTotalDamage > 0)
                    {
                        shouldAllow = false;
                    }
                    else
                    {
                        shouldAllow = true;
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
                    if (!UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.damageoverride") && pendingTotalDamage > 0)
                    {
                        if (barricadeTransform.name.ToString() != "1102"
                                && barricadeTransform.name.ToString() != "1101"
                                && barricadeTransform.name.ToString() != "1393"
                                && barricadeTransform.name.ToString() != "1241")
                        {
                            shouldAllow = false;
                        }
                    } else
                    {
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

        //outdated
        /*private void onItemEquip(PlayerEquipment equipment, ItemJar jar, ItemAsset asset, ref bool shouldAllow)
        {
            foreach (var i in Configuration.Instance.BlockedEquiptables)
            {
                if (asset.id == i)
                {
                    if (playerInZoneType(UnturnedPlayer.FromPlayer(equipment.player), Zone.canNotEquip) && !UnturnedPlayer.FromPlayer(equipment.player).IsAdmin)
                    {
                        new Thread((ThreadStart)(() =>
                        {
                            Thread.Sleep(500);
                            UnturnedChat.Say(UnturnedPlayer.FromPlayer(equipment.player), "You're not allowed to use " + asset.name + " here.");
                            equipment.dequip();
                        })).Start();
                    }
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
                zoneList.Add(temp);
                x++;
            }
            return zoneList;
        }

        public bool playerInAZone(UnturnedPlayer player)
        {
            if (getPositionZones(player.Position).Count > 0)
            {
                return true;
            } else
            {
                return false;
            }
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