﻿using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using DiscordRPC;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CSGO_Presence
{
    public partial class Form1 : Form
    {
        static DateTime? Start = null;
        static bool Running = false;
        static bool WorkShop;
        static string Steam_ID;
        static string Map;
        static string TeamName;
        static string Mode;
        static dynamic Now;
        public static DiscordRpcClient client;
        static HttpListener listener = new HttpListener();
        public Form1()
        {
            InitializeComponent();
        }
        string cfgText = @"""Discord Presence v.1""
{
    ""uri"" ""http://127.0.0.1:2348""
    ""timeout"" ""5.0""
    ""buffer"" ""0.1""
    ""heartbeat"" ""15.0""
    ""data""
   {
        ""map"" ""1""
        ""round"" ""1""
        ""player_match_stats"" ""1""
        ""player_id"" ""1""
   }
}";

        // Oh yeah btw APR gay and NBK worst player
        private void InstallButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo.exe"))
            {
                if (File.Exists(@"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\cfg\gamestate_integration_discordpresence.cfg"))
                {
                    MessageBox.Show("Nice! You have TWO versions installed now! But that'd be stupid so I did nothing.");
                }
                else
                {
                    File.WriteAllText(@"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\cfg\gamestate_integration_discordpresence.cfg", cfgText);
                    MessageBox.Show("Installed! I hope you and enjoy! glhf <3");
                }
            }
            else
            {
                MessageBox.Show("So uh, I tried locating the normal steam directory and it doesn't exist. Major oof. So I'm gonna let you direct me to where it exists :D\nNote: I am looking for the directory STEAM is in not csgo.");
                var Browse = new FolderBrowserDialog();
                DialogResult res = Browse.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (File.Exists($@"{Browse.SelectedPath}\steamapps\common\Counter-Strike Global Offensive\csgo\cfg\gamestate_integration_discordpresence.cfg"))
                    {
                        MessageBox.Show("Nice! You have TWO versions installed now! But that'd be stupid so I did nothing.");
                    }
                    else
                    {
                        File.WriteAllText($@"{Browse.SelectedPath}\steamapps\common\Counter-Strike Global Offensive\csgo\cfg\gamestate_integration_discordpresence.cfg", cfgText);
                        MessageBox.Show("Installed! I hope you and enjoy! glhf <3");
                    }
                }
                else
                {
                    MessageBox.Show($"Error: res returned {res} on line 25");
                }
            }
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
            Process csgo = new Process();
            csgo.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            csgo.StartInfo.FileName = "CMD.exe";
            csgo.StartInfo.Arguments = "/C start steam://rungameid/730";
            csgo.Start();
            if (Start == null)
                Start = DateTime.UtcNow;

            Running = true;
            Hide();
            while (Running)
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://127.0.0.1:2348/");
                listener.Start();
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                dynamic JSON = JObject.Parse(GetRequestData(request));
                UpdatePresence(JSON);
                string responseString = "";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
                listener.Stop();

                Process[] proc = Process.GetProcessesByName("csgo");
                if (proc.Length > 0)
                {
                    Running = true;
                    MessageBox.Show("Running set to true");
                }
                else
                {
                    Running = false;
                    MessageBox.Show("Running set to false");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!HttpListener.IsSupported)
            {
                MessageBox.Show("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            client = new DiscordRpcClient("494943194165805082", true, 0);
            client.Initialize();

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
                Hide();

            else if (FormWindowState.Normal == this.WindowState)
                NotifyIcon.Visible = false;
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                NotifyIcon.ContextMenuStrip = new ContextMenuStrip();
                ToolStripButton quitButton = new ToolStripButton();
                quitButton.Text = "Quit";
                quitButton.Click += HandleQuit;
                NotifyIcon.ContextMenuStrip.Items.Add(quitButton);
            }
            else if (e.Button == MouseButtons.Left)
            {
                Show();
            }

        }

        private void HandleQuit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public static string GetRequestData(HttpListenerRequest request)
        {
            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);

            string s = reader.ReadToEnd();
            body.Close();
            reader.Close();
            return s;
        }

        public static void UpdatePresence(dynamic json)
        {
            MessageBox.Show(json.ToString());

            RichPresence presence = new RichPresence();

            if (json.player.activity == "menu")
                Mode = "In menus";

            if (Steam_ID == null)
                Steam_ID = json.player.steamid;


            if (json.map != null)
            {
                if (Mode == "In menus" && json.map.phase.ToString() != "live")
                {
                    Now = null;
                }
                else
                {
                    if (Now == null)
                        Now = DateTime.UtcNow;
                }
                switch (json.map.mode.ToString())
                {
                    case "gungameprogressive":
                        Mode = "Arms Race";
                        break;
                    case "gungametrbomb":
                        Mode = "Demolition";
                        break;
                    case "scrimcomp2v2":
                        Mode = "Wingman";
                        break;
                    default:
                        Mode = char.ToUpper(json.map.mode.ToString().ToCharArray()[0]) + json.map.mode.ToString().Substring(1);
                        break;
                }
                switch (json.map.name.ToString())
                {
                    case "de_cbble":
                        Map = "Cobblestone";
                        break;
                    case "de_stmarc":
                        Map = "St. Marc";
                        break;
                    case "de_dust2":
                        Map = "Dust II";
                        break;
                    case "de_shortnuke":
                        Map = "Nuke";
                        break;
                    default:
                        if (json.map.name.ToString().StartsWith("workshop"))
                        {
                            WorkShop = true;
                            Map = json.map.name.ToString().Substring(json.map.name.ToString().Split('/')[1].Length + json.map.name.ToString().Split('/')[2].Length + 1);
                        }
                        else
                        {
                            WorkShop = false;
                            Map = char.ToUpper(json.map.name.ToString().Substring(3).ToCharArray()[0]) + json.map.name.ToString().Substring(4);
                        }
                        break;
                }
            }

            if (json.player.team != null)
            {
                if (json.player.team.ToString() == "CT")
                    TeamName = "Counter-Terrorists";
                else
                    TeamName = "Terrorists";
                if (json.player.match_stats != null)
                {
                    if (json.player.steamid == Steam_ID)
                    {
                        string s = json.player.team.ToString() == "T"
                            ? $"Score: {json.map.team_t.score}:{json.map.team_ct.score}"
                            : $"Score: {json.map.team_ct.score}:{json.map.team_t.score}";
                        presence.State = $"K: {json.player.match_stats.kills} / A: {json.player.match_stats.assists} / D: {json.player.match_stats.deaths}. {s}";
                    }
                    else
                    {
                        presence.State = $"Spectating. Score: T: {json.map.team_t.score} / CT: {json.map.team_ct.score}";
                    }
                }
                presence.Details = $"Playing {Mode}";
                if (Now != null)
                {
                    presence.Timestamps = new Timestamps()
                    {
                        Start = Now
                    };

                }
                if (!WorkShop)
                {
                    presence.Assets = new Assets()
                    {
                        LargeImageKey = Map.ToLower().Replace(' ', '_'),
                        LargeImageText = Map,
                        SmallImageKey = json.player.team.ToString().ToLower(),
                        SmallImageText = TeamName
                    };
                }
                else
                {
                    presence.Assets = new Assets()
                    {
                        LargeImageKey = "workshop",
                        LargeImageText = Map,
                        SmallImageKey = json.player.team.ToString().ToLower(),
                        SmallImageText = TeamName
                    };
                }
                client.SetPresence(presence);
            }
            else if (Mode == "In menus")
            {
                presence.Details = Mode;
                presence.Assets = new Assets()
                {
                    LargeImageKey = "idle",
                    LargeImageText = "In menus"
                };
                presence.Timestamps = new Timestamps()
                {
                    Start = Start
                };
                client.SetPresence(presence);
            }
        }
        }
}