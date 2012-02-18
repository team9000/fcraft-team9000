using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using System.IO;

namespace fCraftCustom.NKMods.Helpers {
    class AutoPromo {
        public static void DoAutoPromo(SchedulerTask task) {
            string warpfile = "warp.txt";

            try {
                if (!File.Exists(warpfile)) return;

                using (StreamReader reader = File.OpenText(warpfile)) {
                    while (!reader.EndOfStream) {
                        string[] fields = reader.ReadLine().Split(' ');
                        if (fields.Length != 3)
                            continue;

                        try {
                            PlayerInfo info = PlayerDB.FindPlayerInfoExact(fields[0]);

                            if (info == null) continue;
                            Rank newRank = RankManager.FindRank(fields[1]);
                            if (newRank == null) continue;
                            if (info.Rank == newRank) continue;

                            info.ChangeRank(Player.Console, newRank, "PROMOTION FROM FORUM (" + fields[2] + ")", true, true, false);
                        } catch (Exception) { }
                    }
                }
            }
            catch (Exception) {
            }

            try {
                File.Delete(warpfile);
            } catch(Exception) { }
        }
    }
}
