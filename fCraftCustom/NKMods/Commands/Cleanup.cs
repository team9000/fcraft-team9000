using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraftCustom.NKMods.Helpers;

namespace fCraftCustom.NKMods.Commands {
    class Cleanup {
        static int CleanupOldHistory() {
            try {
                int count = 0;
                while (true) {
                    string removename = "";
                    foreach (string name in History.historyinfos.Keys) {
                        try {
                            World world = WorldManager.FindWorldExact(name);
                            if (world == null || !History.ShouldKeepHistory(world)) {
                                removename = name;
                                break;
                            }
                        }
                        catch (Exception) {
                        }
                    }

                    if (removename.Equals("")) {
                        break;
                    }
                    else {
                        History.historyinfos.Remove(removename);
                        count++;
                    }
                }
                return count;
            }
            catch (Exception) {
            }

            return 0;
        }

        public static CommandDescriptor cdCleanup = new CommandDescriptor {
            Name = "cleanup",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new Permission[] { Permission.Promote },
            Usage = "/cleanup",
            Help = "Cleanup old history entries.",
            Handler = CleanupCmd
        };
        static void CleanupCmd(Player player, Command cmd) {
            int count = CleanupOldHistory();
            player.Message("&cCleaned up history for {0} old worlds", count);

            count = Helpers.Traps.trapinfo.Count;
            Helpers.Traps.trapinfo.Clear();
            player.Message("&cCleaned up trap info for {0} players", count);
        }

        public static CommandDescriptor cdCleanupAll = new CommandDescriptor {
            Name = "cleanupall",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new Permission[] { Permission.Promote },
            Usage = "/cleanupall",
            Help = "Erase all history entries.",
            Handler = CleanupAll
        };
        static void CleanupAll(Player player, Command cmd) {
            int count = Helpers.History.historyinfos.Count;
            Helpers.History.historyinfos.Clear();
            player.Message("&cRemoved history for {0} worlds", count);

            count = Helpers.Traps.trapinfo.Count;
            Helpers.Traps.trapinfo.Clear();
            player.Message("&cCleaned up trap info for {0} players", count);
        }
    }
}
