using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using MTA.Game;
using MTA.Interfaces;

namespace MTA.Franko
{
    public partial class FrankoNpcControl : Form
    {
        public FrankoNpcControl()
        {
            InitializeComponent();
        }
        public static ushort num;
        public static Map Frankomap;
        public static uint getid(string name)
        {
            Map map = Kernel.Maps[num];
            foreach (var Franko in map.Npcs)
            {
                if (Franko.Value.Name == name)
                {
                    return Franko.Key;
                }
                

            }
            return 0;

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            uint npcid = getid(comboBox1.Text);
            textBox2.Text =Convert.ToString(npcid);
            textBox3.Text =Convert.ToString(Frankomap.Npcs[npcid].Name);
            textBox4.Text = Convert.ToString(Frankomap.Npcs[npcid].Mesh );
            textBox5.Text = Convert.ToString(Frankomap.Npcs[npcid].MapID);
            textBox6.Text = Convert.ToString(Frankomap.Npcs[npcid].X);
            textBox7.Text = Convert.ToString(Frankomap.Npcs[npcid].Y);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            try
            {
                if (textBox1.Text != "")
                {
                    Map Franko = Kernel.Maps[Convert.ToUInt16(textBox1.Text)];
                    Frankomap = Franko;
                    num = Convert.ToUInt16(textBox1.Text);
                    foreach (var Frankon in Franko.Npcs)
                    {
                        comboBox1.Items.Add(Frankon.Value.Name);
                    }
                }
                else
                {
                    MessageBox.Show("You Should Write Map Num");
                }
            }
            catch (Exception exp)
                {
                    Console.WriteLine(exp);
                }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void FrankoNpcControl_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (INpc npc in Frankomap.Npcs.Values)
                if (Frankomap != null)
                {
                    if (textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "" && textBox5.Text != "" && textBox6.Text != "" && textBox7.Text != "")
                    {
                        Frankomap.Npcs[Convert.ToUInt32(textBox2.Text)].Name = textBox3.Text;
                        Frankomap.Npcs[Convert.ToUInt32(textBox2.Text)].Mesh = Convert.ToUInt16(textBox4.Text);
                        Frankomap.Npcs[Convert.ToUInt32(textBox2.Text)].MapID = Convert.ToUInt16(textBox5.Text);
                        Frankomap.Npcs[Convert.ToUInt32(textBox2.Text)].X = Convert.ToUInt16(textBox6.Text);
                        Frankomap.Npcs[Convert.ToUInt32(textBox2.Text)].Y = Convert.ToUInt16(textBox7.Text);
                        Program.CommandsAI("@save");
                        Console.WriteLine("A Npc Which Have ID " + textBox2.Text+" Changed By Franko");
                        using (var cmd = new Database.MySqlCommand(Database.MySqlCommandType.UPDATE))
                            cmd.Update("npcs").Set("name", npc.Name).Set("type", (ushort)npc.Type).Set("lookface", (int)npc.Mesh).Set("cellx", npc.X).Set("celly", npc.Y).Set("mapid", npc.MapID).Where("id", npc.UID).Execute();

                        foreach (Client.GameState Frankoc in Kernel.GamePool.Values)
                        {
                            Frankoc.Screen.Reload(null);
                        }
                    }
                    else
                    {
                        MessageBox.Show("You Should Write All informations");
                    }
                }
                else
                {
                    MessageBox.Show("You Should CHOOSE A Map And Npc");
                }
            }
            catch (Exception exe)
            {
                Console.WriteLine(exe);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("WwW.Franko.com");
        }
    }
}
