﻿// Copyright 2009-2012 Matvei Stefarov <me@matvei.org>
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using fCraft.Events;
using fCraft.GUI;
using System.Threading;

namespace fCraft.ServerGUI {

    public sealed partial class MainForm : Form {
        volatile bool shutdownPending, startupComplete, shutdownComplete;
        const int MaxLinesInLog = 2000;

        public MainForm() {
            InitializeComponent();
            Shown += StartUp;
            console.OnCommand += console_Enter;
        }


        #region Startup
        Thread startupThread;

        void StartUp( object sender, EventArgs a ) {
            Logger.Logged += OnLogged;
            Heartbeat.UriChanged += OnHeartbeatUriChanged;
            Server.PlayerListChanged += OnPlayerListChanged;
            Server.ShutdownEnded += OnServerShutdownEnded;
            Text = "fCraft " + Updater.CurrentRelease.VersionString + " - starting...";
            startupThread = new Thread( StartupThread );
            startupThread.Name = "fCraft ServerGUI Startup";
            startupThread.Start();
        }


        void StartupThread() {
#if !DEBUG
            try {
#endif
                Server.InitLibrary( Environment.GetCommandLineArgs() );
                if( shutdownPending ) return;

                Server.InitServer();
                if( shutdownPending ) return;

                BeginInvoke( (Action)OnInitSuccess );

                UpdaterResult update = Updater.CheckForUpdates();
                if( shutdownPending ) return;

                if( update.UpdateAvailable ) {
                    new UpdateWindow( update, false ).ShowDialog();
                }

                if( !ConfigKey.ProcessPriority.IsBlank() ) {
                    try {
                        Process.GetCurrentProcess().PriorityClass = ConfigKey.ProcessPriority.GetEnum<ProcessPriorityClass>();
                    } catch( Exception ) {
                        Logger.Log( LogType.Warning,
                                    "MainForm.StartServer: Could not set process priority, using defaults." );
                    }
                }

                if( shutdownPending ) return;
                if( Server.StartServer() ) {
                    startupComplete = true;
                    BeginInvoke( (Action)OnStartupSuccess );
                } else {
                    BeginInvoke( (Action)OnStartupFailure );
                }
#if !DEBUG
            } catch( Exception ex ) {
                Logger.LogAndReportCrash( "Unhandled exception in ServerGUI.StartUp", "ServerGUI", ex, true );
                Shutdown( ShutdownReason.Crashed, Server.HasArg( ArgKey.ExitOnCrash ) );
            }
#endif
        }


        void OnInitSuccess() {
            Text = "fCraft " + Updater.CurrentRelease.VersionString + " - " + ConfigKey.ServerName.GetString();
        }


        void OnStartupSuccess() {
            if( !ConfigKey.HeartbeatEnabled.Enabled() ) {
                uriDisplay.Text = "Heartbeat disabled. See externalurl.txt";
            }
            console.Enabled = true;
            console.Text = "";
        }


        void OnStartupFailure() {
            Shutdown( ShutdownReason.FailedToStart, Server.HasArg( ArgKey.ExitOnCrash ) );
        }

        #endregion


        #region Shutdown

        protected override void OnFormClosing( FormClosingEventArgs e ) {
            if( startupThread != null && !shutdownComplete ) {
                Shutdown( ShutdownReason.ProcessClosing, true );
                e.Cancel = true;
            } else {
                base.OnFormClosing( e );
            }
        }


        void Shutdown( ShutdownReason reason, bool quit ) {
            if( shutdownPending ) return;
            shutdownPending = true;
            console.Enabled = false;
            console.Text = "Shutting down...";
            Text = "fCraft " + Updater.CurrentRelease.VersionString + " - shutting down...";
            uriDisplay.Enabled = false;
            if( !startupComplete ) {
                startupThread.Join();
            }
            Server.Shutdown( new ShutdownParams( reason, TimeSpan.Zero, quit, false ), false );
        }


        void OnServerShutdownEnded( object sender, ShutdownEventArgs e ) {
            try {
                BeginInvoke( (Action)delegate {
                    shutdownComplete = true;
                    switch( e.ShutdownParams.Reason ) {
                        case ShutdownReason.FailedToInitialize:
                        case ShutdownReason.FailedToStart:
                        case ShutdownReason.Crashed:
                            if( Server.HasArg( ArgKey.ExitOnCrash ) ) {
                                Application.Exit();
                            }
                            break;
                        default:
                            Application.Exit();
                            break;
                    }
                } );
            } catch( ObjectDisposedException ) {
            } catch( InvalidOperationException ) { }
        }

        #endregion


        public void OnLogged( object sender, LogEventArgs e ) {
            if( !e.WriteToConsole ) return;
            try {
                if( shutdownComplete ) return;
                if( logBox.InvokeRequired ) {
                    BeginInvoke( (EventHandler<LogEventArgs>)OnLogged, sender, e );
                } else {
                    logBox.AppendText( e.Message + Environment.NewLine );
                    if( logBox.Lines.Length > MaxLinesInLog ) {
                        logBox.Text = "----- cut off, see fCraft.log for complete log -----" +
                            Environment.NewLine +
                            logBox.Text.Substring( logBox.GetFirstCharIndexFromLine( 50 ) );
                    }
                    logBox.SelectionStart = logBox.Text.Length;
                    logBox.ScrollToCaret();
                    logBox.Refresh();
                }
            } catch( ObjectDisposedException ) {
            } catch( InvalidOperationException ) { }
        }


        public void OnHeartbeatUriChanged( object sender, UriChangedEventArgs e ) {
            try {
                if( shutdownPending ) return;
                if( uriDisplay.InvokeRequired ) {
                    BeginInvoke( (EventHandler<UriChangedEventArgs>)OnHeartbeatUriChanged,
                            sender, e );
                } else {
                    uriDisplay.Text = e.NewUri.ToString();
                    uriDisplay.Enabled = true;
                    bPlay.Enabled = true;
                }
            } catch( ObjectDisposedException ) {
            } catch( InvalidOperationException ) { }
        }


        public void OnPlayerListChanged( object sender, EventArgs e ) {
            try {
                if( shutdownPending ) return;
                if( playerList.InvokeRequired ) {
                    BeginInvoke( (EventHandler)OnPlayerListChanged, null, EventArgs.Empty );
                } else {
                    playerList.Items.Clear();
                    Player[] playerListCache = Server.Players.OrderBy( p => p.Info.Rank.Index ).ToArray();
                    foreach( Player player in playerListCache ) {
                        playerList.Items.Add( player.Info.Rank.Name + " - " + player.Name );
                    }
                }
            } catch( ObjectDisposedException ) {
            } catch( InvalidOperationException ) { }
        }


        private void console_Enter() {
            string[] separator = { Environment.NewLine };
            string[] lines = console.Text.Trim().Split( separator, StringSplitOptions.RemoveEmptyEntries );
            foreach( string line in lines ) {
#if !DEBUG
                try {
#endif
                    if( line.Equals( "/Clear", StringComparison.OrdinalIgnoreCase ) ) {
                        logBox.Clear();
                    } else if( line.Equals( "/credits", StringComparison.OrdinalIgnoreCase ) ) {
                        new AboutWindow().Show();
                    } else {
                        Player.Console.ParseMessage( line, true );
                    }
#if !DEBUG
                } catch( Exception ex ) {
                    Logger.LogToConsole( "Error occured while trying to execute last console command: " );
                    Logger.LogToConsole( ex.GetType().Name + ": " + ex.Message );
                    Logger.LogAndReportCrash( "Exception executing command from console", "ServerGUI", ex, false );
                }
#endif
            }
            console.Text = "";
        }


        private void bPlay_Click( object sender, EventArgs e ) {
            try {
                Process.Start( uriDisplay.Text );
            } catch( Exception ) {
                MessageBox.Show( "Could not open server URL. Please copy/paste it manually." );
            }
        }
    }
}