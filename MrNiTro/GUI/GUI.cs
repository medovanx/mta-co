using Microsoft.VisualBasic;
using MTA.ServerBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace MTA.Franko.GUI
{
    public partial class GUI : Form
    {
        public static string AllText = "";
        public static bool Changed, First = true;
        public GUI()
        {
            this.InitializeComponent();
            timer1.Start();
            // System.Threading.Thread startserver = new System.Threading.Thread(Program.StartEngine);
            // startserver.Start();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new MTA.Database.MySqlCommand(MTA.Database.MySqlCommandType.UPDATE).Update("configuration").Set("ItemUID", MTA.Network.GamePackets.ConquerItem.ItemUID.Now).Where("Server", MTA.Constants.ServerName).Execute();
            Console.WriteLine("Save All Characters Successfully Done !");
            Program.CommandsAI("@restart");
            // Program.SetAllOffline();
            // Thread.Sleep(500);
            Program.CommandsAI("@save");
            Application.Restart();
            Environment.Exit(0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Save All Characters Successfully Done !");
            //    Program.OnclosingEvent();
            Program.CommandsAI("@save");
            //Thread.Sleep(500);
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new FrankoNpcControl().Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            System.Console.Clear();
            AllText = "";
            textBox_write_console.Text = ("Done Console Clear !");
            textBox_write_console.ForeColor = Color.Yellow;
        }

        private void textBox_write_console_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_write_console_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void GUI_Load(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (button7.Text == "Start GW")
            {
                if (!Game.GuildWar.IsWar)
                {
                    Game.GuildWar.Start();
                    Console.WriteLine("Guild War Starts At [" + DateTime.Now.ToString("hh:mm:ss]"));
                    textBox_write_console.Text = ("Guild War Starts At [" + DateTime.Now.ToString("hh:mm:ss]"));
                    textBox_write_console.ForeColor = Color.Yellow;
                    button7.Text = "Stop GW";
                }
            }
            else if (button7.Text == "Stop GW")
            {
                if (Game.GuildWar.IsWar)
                {
                    Game.GuildWar.End();
                    Console.WriteLine("Guild War Ends At [" + DateTime.Now.ToString("hh:mm:ss]"));
                    textBox_write_console.Text = ("Guild War Ends At [" + DateTime.Now.ToString("hh:mm:ss]"));
                    textBox_write_console.ForeColor = Color.Teal;
                    button7.Text = "Start GW";
                }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Kernel.GamePool.Count > Program.MaxOn)
            {
                Program.MaxOn = Kernel.GamePool.Count;
            }

            label11.Text = Kernel.GamePool.Count.ToString();
            label10.Text = Program.MaxOn.ToString();
            if (Changed)
            {
                if (AllText != "")
                    lock (AllText)
                    {
                        textBox_write_console.AppendText(AllText);
                        AllText = "";
                    }
                Changed = false;
            }
        }
        public static void WriteLine(object text)
        {
            lock (AllText)
            {
                string output = AllText + "\r\n";
                output += /*Sword.Console.TimeStamp() +*/ text.ToString() + " ";
                AllText = output;
                Changed = true;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
