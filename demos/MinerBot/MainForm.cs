using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Craft.Net;
using Craft.Net.Client;
using Craft.Net.Client.Events;
using Craft.Net.Common;
using MineBot.Bot;

namespace MineBot
{
    public partial class MainForm : Form
    {
        delegate void SetTextCallback(object sender, string message);

        //private MinecraftClient _client;
        private Vector3 _seek;
        private bool _seekDirty;
        private bool _doSeek;
        private readonly MinerBotManager _botManager;

        public MainForm()
        {
            InitializeComponent();
            
            Text = Application.ProductName + " " + Application.ProductVersion;

            _botManager = new MinerBotManager();
            _botManager.LogMessage += LogIt;

            UpdateUI();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = (_client != null);
            e.Cancel = _botManager.IsConnected;
        }

        private void buttonPing_Click(object sender, EventArgs e)
        {
            var endPoint = GetEndPoint();
            if (endPoint == null) return;
            var ping = ServerPing.DoPing(endPoint);

            if (ping == null)
            {
                textBoxLog.Text = null;
                MessageBox.Show(string.Format("Unable to ping {0}", endPoint));
                return;
            }
            var sb = new StringBuilder();
            sb.AppendFormat("Players: {0}/{1}", ping.Players.OnlinePlayers, ping.Players.MaxPlayers);
            sb.AppendLine();
            sb.AppendFormat("Version: {0}", ping.Version.Name);
            sb.AppendLine();
            sb.AppendFormat("ProtocolVersion: {0}", ping.Version.ProtocolVersion);
            sb.AppendLine();
            sb.AppendFormat("Description: {0}", ping.Description);
            sb.AppendLine();
            sb.AppendFormat("Latency: {0}", ping.Latency.TotalMilliseconds);
            LogIt(this, sb.ToString());
        }

        private IPEndPoint GetEndPoint()
        {
            var endPoint = Globals.ParseEndPoint(comboBox1.Text);
            if (endPoint == null) MessageBox.Show(string.Format("Unable to parse connect string {0}", comboBox1.Text));
            return endPoint;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_botManager.IsConnected) return;

            var endPoint = GetEndPoint();
            if (endPoint == null) return;
            var name = string.Format(textBoxUserName.Text, _botManager.Count + 1);
            _botManager.AddMiner(endPoint, name);
            UpdateUI();
            _seekDirty = true;
            timer1.Enabled = true;
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (!_botManager.IsConnected) return;
            timer1.Enabled = false;
            _botManager.DisconnectAll("Done for today");
            UpdateUI();
        }

        private void UpdateUI()
        {
            var isConnected = _botManager.IsConnected;
            buttonConnect.Enabled = !isConnected;
            buttonDisconnect.Enabled = isConnected;
        }
        
        private void LogIt(object sender, string message)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (textBoxLog.InvokeRequired)
            {
                Invoke(new SetTextCallback(LogIt), new object[] { sender, message });
            }
            else
            {
                textBoxLog.Text = message;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _botManager.Update();
            label3.Text = "Count: " + _botManager.Count;
        }
    }
}
