using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Events;
using System.IO;

namespace fCraftCustom.NKMods.Helpers
{
    class Misc
    {
        public static void OnGetVersionString(ref string version) {
            version = "fCraft (Custom Team9000 Edition)";
        }

        public static void OnHeartbeat(object sender, HeartbeatSendingEventArgs e) {
            e.Cancel = true;
        }
        public static void DoHeartbeat(SchedulerTask task) {
            Player[] players = Server.Players;

            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (Player p in players) {
                //if (p.isHidden) continue;
                if (!first) sb.Append(",");
                sb.Append(p.ClassyName);
                first = false;
            }

            string query = String.Format("players={0}&max={1}&public={2}&ip={3}&port={4}&salt={5}&womsalt={6}",
                Uri.EscapeDataString(sb.ToString()),
                ConfigKey.MaxPlayers.GetInt(),
                ConfigKey.IsPublic.Enabled(),
                Uri.EscapeDataString(ConfigKey.IP.GetString()),
                Server.Port,
                Uri.EscapeDataString(NKMods.mainSalt),
                Uri.EscapeDataString(NKMods.womSalt));

            File.WriteAllText("status.txt", query, ASCIIEncoding.ASCII);
        }
        public static void OnBanChanged(object sender, PlayerInfoBanChangedEventArgs e) {
            if (e.IsBeingUnbanned) return;

            World[] worlds = WorldManager.Worlds;
            foreach (World world in worlds) {
                History.RevertHistory(world, e.PlayerInfo, e.Banner);
            }
        }

        public static void OnCheckingPlayerLogin(object sender, CheckingPlayerLoginEventArgs e) {
            PlayerInfo Info = PlayerDB.FindPlayerInfoExact(e.Name);

            bool ipMatch = false;
            bool lowLevel = true;
            if(Info != null) {
                if(Info.TimesVisited > 1 && Info.LastIP.Equals(e.Player.IP)) ipMatch = true;
                if(Info.Rank.Can(Permission.Kick)) lowLevel = false;
            }
            bool verifyMain = Server.VerifyName(e.Name, e.VerificationCode, NKMods.mainSalt);
            bool verifyWom = Server.VerifyName(e.Name, e.VerificationCode, NKMods.womSalt);

            if(verifyMain || (ipMatch && lowLevel) || (verifyWom && lowLevel) || (verifyWom && ipMatch)) {
                e.Verify = true;
                return;
            }

            if (verifyWom && !lowLevel) {
                e.Player.KickNow("You cannot connect via WoM Direct at your rank", LeaveReason.UnverifiedName);
                e.Abort = true;
                return;
            }

            e.Player.KickNow("It seems that minecraft.net authentication is down, sorry.", LeaveReason.UnverifiedName);
            e.Abort = true;
            return;
        }
    }
}
