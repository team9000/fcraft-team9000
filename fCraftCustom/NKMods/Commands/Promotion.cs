using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using System.IO;

namespace fCraftCustom.NKMods.Commands {
    class Promotion {
        public static readonly CommandDescriptor cdEngage = new CommandDescriptor {
            Name = "engage",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Promote },
            Usage = "/engage",
            Help = "Engages the T9k promotion warp drive",
            Handler = Engage
        };

        static void Engage(Player player, Command cmd) {
            try {
                string adminname = "CONSOLE";
                if (player != null && player.Name != null)
                    adminname = player.Name;

                string warpfile = "warp.txt";
                if (File.Exists(warpfile)) {
                    Server.Message("{0}{1} has engaged the Team9000 PROMOTION WARP DRIVE",
                                                                Color.Lime, adminname);
                    using (StreamReader reader = File.OpenText(warpfile)) {
                        while (!reader.EndOfStream) {
                            string[] fields = reader.ReadLine().Split(' ');
                            if (fields.Length != 3)
                                continue;

                            PlayerInfo info = PlayerDB.FindPlayerInfoExact(fields[0]);

                            if (info == null) continue;
                            Rank newRank = RankManager.FindRank(fields[1]);
                            if (newRank == null) continue;
                            if (info.Rank == newRank) continue;

                            info.ChangeRank(Player.Console, newRank, "PROMOTION WARP (" + fields[2] + ")", true, true, false);
                        }
                    }
                    File.Delete(warpfile);
                }
            }
            catch (Exception) {
            }
        }
    }
}
