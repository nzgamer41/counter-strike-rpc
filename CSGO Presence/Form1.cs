﻿using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using DiscordRPC;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace CSGO_Presence
{
    public partial class Form1 : Form
    {
        static double v = 2.2;
        static DateTime? Start = null;
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
            try
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
                    MessageBox.Show("I couldn't find CSGO in the default steam directory, can you please tell me where your Steam library that has CSGO installed is?\nNote: I want the folder that has Steam.dll and steamapps in it, NOT CSGO's folder.");
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
                        MessageBox.Show($"Error: res returned {res} on line 25\nYou probably clicked cancel or the X in the corner of the file browser window.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is DirectoryNotFoundException)
                {
                    MessageBox.Show("You've picked the wrong directory. Please make sure you're using the folder that has Steam.dll and steamapps in it, and not the CSGO folder.\nLet's have another go at that.");
                    InstallButton_Click(sender, e);
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
            Hide();

            RunListener();

        }

        private void RunListener()
        {
            if (Start == null)
                Start = DateTime.UtcNow;
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:2348/");
            Task.Run(() => {
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
                RunListener();
            });
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            HttpClient http = new HttpClient();
            if (!HttpListener.IsSupported)
            {
                MessageBox.Show("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            client = new DiscordRpcClient("494943194165805082", true, 0);
            client.Initialize();
            
            var hi = await http.GetAsync("https://raw.githubusercontent.com/Lilwiggy/counter-strike-rpc/master/version.json");
            string s = await hi.Content.ReadAsStringAsync();
            dynamic JSON = JObject.Parse(s);

            if (JSON.v != v)
            {
                MessageBox.Show("You have an outdated version! I'll open up your favorite browser with a link to the latest version for you to download :)");
                Process.Start($"https://github.com/Lilwiggy/counter-strike-rpc/releases/tag/V{JSON.v}");
            }
            RunListener();

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
