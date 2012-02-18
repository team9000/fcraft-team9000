using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraftCustom.NKMods.Helpers;

namespace fCraftCustom.NKMods.Commands {
    class UnbanAllAll {
        public static CommandDescriptor cdUnbanAllAll = new CommandDescriptor {
            Name = "unbanallall",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new Permission[] { Permission.EditPlayerDB },
            Usage = "/unbanallall",
            Help = "Unban everyone. Ever.",
            Handler = DoUnbanAllAll
        };
        static void DoUnbanAllAll(Player player, Command cmd) {
            int unbanned = 0;
            int deranked = 0;
            foreach (PlayerInfo Info in PlayerDB.PlayerInfoList) {
                if (Info.IsBanned) {
                    Info.Unban(player, "UnbanAllAll", true, true);
                    unbanned++;
                    if (Info.Rank != RankManager.LowestRank) {
                        Info.ChangeRank(player, RankManager.LowestRank, "UnbanAllAll", true, true, false);
                        deranked++;
                    }
                }
            }

            Server.Message("&cUnbanned {0} players", unbanned);
            Server.Message("&cDe-ranked {0} players", deranked);
        }
    }
}
