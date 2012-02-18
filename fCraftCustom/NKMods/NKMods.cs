using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using fCraft;
using System.Collections.Generic;
using fCraft.Events;
using fCraftCustom.NKMods;

namespace fCraftCustom.NKMods {
    class NKMods {
        public static String mainSalt = "";
        public static String womSalt = "";

        public static void Init() {
            // Helpers.Worlds
            Player.Connected += Helpers.Worlds.OnPlayerConnected;
            WorldManager.SearchingForWorld += Helpers.Worlds.OnSearchingForWorld;

            // Helpers.Traps
            Player.Connected += Helpers.Traps.OnPlayerConnected;
            Player.Disconnected += Helpers.Traps.OnPlayerDisconnected;
            Player.PlacingBlock += Helpers.Traps.OnPlayerPlacingBlock;

            // Helpers.Misc
            //Server.OnGetVersionString += Helpers.Misc.OnGetVersionString;
            Heartbeat.Sending += Helpers.Misc.OnHeartbeat;
            Scheduler.NewTask(Helpers.Misc.DoHeartbeat).RunForever(TimeSpan.FromMilliseconds(1000));
            PlayerInfo.BanChanged += Helpers.Misc.OnBanChanged;
            PlayerDB.SaveInterval = TimeSpan.FromSeconds(300);
            Player.CheckingPlayerLogin += Helpers.Misc.OnCheckingPlayerLogin;

            // Helpers.History
            Player.PlacedBlock += Helpers.History.OnPlayerPlacedBlock;
            Player.PlacingBlock += Commands.HistoryCmd.OnPlayerPlacingBlock;

            // Commands
            Server.Initialized += OnInit;

            // Listeners
            Server.Starting += OnStarting;

        }

        static void OnInit(Object sender, EventArgs e) {
            CommandManager.RegisterCustomCommand(Commands.Cleanup.cdCleanup);
            CommandManager.RegisterCustomCommand(Commands.Cleanup.cdCleanupAll);
            CommandManager.RegisterCustomCommand(Commands.HistoryCmd.cdHistory);
            CommandManager.RegisterCustomCommand(Commands.HistoryCmd.cdRevert);
            CommandManager.RegisterCustomCommand(Commands.Promotion.cdEngage);
            CommandManager.RegisterCustomCommand(Commands.UnbanAllAll.cdUnbanAllAll);
            mainSalt = Server.GetRandomString(32);
            womSalt = Server.GetRandomString(32);
        }

        static void OnStarting(Object sender, EventArgs e) {
            string portsfile = "ports.txt";
            if (!File.Exists(portsfile)) return;

            using (StreamReader reader = File.OpenText(portsfile)) {
                while (!reader.EndOfStream) {
                    string[] fields = reader.ReadLine().Split(' ');

                    IPAddress ip = IPAddress.Any;
                    int port = 0;

                    if (fields.Length == 2) {
                        ip = IPAddress.Parse(fields[0]);
                        port = Int32.Parse(fields[1]);
                    } else {
                        port = Int32.Parse(fields[0]);
                    }

                    try {
                        TcpListener listener = new TcpListener(ip, port);
                        listener.Start();
                        Server.listeners.Add(listener);

                    } catch (Exception ex) {
                        // if the port is unavailable, try next one
                        Logger.Log(LogType.Error, "Could not start listening on port {0}",
                                    port, ex.Message);
                    }
                }
            }
        }
    }
}