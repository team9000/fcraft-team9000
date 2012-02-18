using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Events;

namespace fCraftCustom.NKMods.Helpers {
    class TrapInfo {
        public int count = 0;
        public DateTime timer = DateTime.UtcNow;
    }
    class Traps {
        public static Dictionary<Player, TrapInfo> trapinfo = new Dictionary<Player, TrapInfo>();

        public static void OnPlayerConnected(object sender, PlayerConnectedEventArgs e) {
            trapinfo.Remove(e.Player);
        }
        public static void OnPlayerDisconnected(object sender, PlayerDisconnectedEventArgs e) {
            trapinfo.Remove(e.Player);
        }
        public static void OnPlayerPlacingBlock(object sender, PlayerPlacingBlockEventArgs e) {
            if (e.Context != BlockChangeContext.Manual && e.Context != BlockChangeContext.Replaced) return;
            if (e.Result != CanPlaceResult.ZoneDenied) return;
            if (e.Player.Info.Rank != RankManager.LowestRank) return;
            if (e.NewBlock != Block.Air) return;

            Player player = e.Player;
            TrapInfo ptrapinfo;
            if (trapinfo.ContainsKey(player)) {
                ptrapinfo = trapinfo[player];
            }
            else {
                ptrapinfo = trapinfo[player] = new TrapInfo();
            }

            Zone deniedZone = player.World.Map.Zones.FindDenied(e.Coords, player);
            if (deniedZone == null)
                return;

            PlayerExceptions playerList = deniedZone.ExceptionList;
            bool trapzone = false;
            for (int i = 0; i < playerList.Included.Length; i++) {
                if (playerList.Included[i].Name == "trap") {
                    trapzone = true;
                    break;
                }
            }
            if (!trapzone)
                return;

            if (DateTime.UtcNow.Subtract(ptrapinfo.timer).TotalMinutes >= 5) {
                ptrapinfo.timer = DateTime.UtcNow;
                ptrapinfo.count = 0;
            }
            ptrapinfo.count++;
            if (ptrapinfo.count > 20) {
                player.Info.BanIP(Player.Console, "Griefing Detection (" + deniedZone.Name + ")", true, true);
            }

            e.Result = CanPlaceResult.PluginDeniedNoUpdate;
        }
    }
}
