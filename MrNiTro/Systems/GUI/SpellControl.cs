using MTA.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MTA.MaTrix.GUI
{
    public partial class SpellControl : Form
    {
        public SpellControl()
        {
            InitializeComponent();
            textBox2.Text = powerperc.Increment.ToString();
        }

        private void SpellControl_Load(object sender, EventArgs e)
        {
            if (MTA.Database.SpellTable.SpellInformations != null)
            {
                foreach (var items in MTA.Database.SpellTable.SpellInformations.Values)
                {
                    var str = items[0].ID + "==" + items[0].Name;
                    comboBox1.Items.Add(str);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!groupBox1.Visible)
                groupBox1.Visible = true;
            if (comboBox1.SelectedItem != null)
            {
                var data = comboBox1.SelectedItem.ToString().Split(new string[] { "==" }, StringSplitOptions.RemoveEmptyEntries);
                uint id = 0;
                if (uint.TryParse(data[0], out id))
                    textBox1.Text = id.ToString();
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ushort id = 0;
            if (ushort.TryParse(textBox1.Text, out id))
            {
                if (MTA.Database.SpellTable.SpellInformations.ContainsKey(id))
                {
                    var spell = MTA.Database.SpellTable.SpellInformations[id];
                    treeView1.Nodes.Clear();
                    var nodes = treeView1.Nodes.Add(id.ToString());
                    foreach (var item in spell.Values)
                    {
                        var mininodes = nodes.Nodes.Add(item.Level.ToString()).Nodes;
                        mininodes.Add(item.Power.ToString());
                        mininodes.Add(item.PowerPercent.ToString());
                    }
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                var data = treeView1.SelectedNode.FullPath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                ushort id = 0;
                byte level = 0;
                if (data.Length > 0)
                {
                    ushort.TryParse(data[0], out id);
                    if (data.Length > 1)
                        byte.TryParse(data[1], out level);

                    if (MTA.Database.SpellTable.SpellInformations.ContainsKey(id))
                    {
                        if (MTA.Database.SpellTable.SpellInformations[id].ContainsKey(level))
                        {
                            var spell = MTA.Database.SpellTable.SpellInformations[id][level];

                            power.Text = spell.Power.ToString();
                            powerperc.Value = (decimal)spell.PowerPercent;

                            powerperc.ValueChanged += (Sender, ee) =>
                                {
                                    if (powerperc.Value == (decimal)spell.PowerPercent)
                                        return;
                                    spell.Power = (ushort)(30000 + (powerperc.Value * 100));
                                    spell.PowerPercent = ((float)spell.Power % 1000) / 100;
                                    power.Text = spell.Power.ToString();

                                    if (checkBox1.Checked)
                                    {
                                        var command = new MTA.Database.MySqlCommand(MySqlCommandType.UPDATE);
                                        command.Update("spells").Set("power", spell.Power).Where("type", spell.ID).And("level", spell.Level).Execute();

                                    }
                                };
                            power.TextChanged += (Sender, ee) =>
                            {
                                ushort Power = 0;
                                if (ushort.TryParse(power.Text.ToString(), out Power))
                                {
                                    if (Power == spell.Power)
                                        return;
                                    spell.Power = Power;
                                    spell.PowerPercent = ((float)Power % 1000) / 100;
                                    powerperc.Value = (decimal)spell.PowerPercent;

                                    if (checkBox1.Checked)
                                    {
                                        var command = new MTA.Database.MySqlCommand(MySqlCommandType.UPDATE);
                                        command.Update("spells").Set("power", spell.Power).Where("type", spell.ID).And("level", spell.Level).Execute();

                                    }
                                }
                            };
                        }

                    }
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            decimal id = 0;
            if (decimal.TryParse(textBox2.Text, out id))
                powerperc.Increment = id;
        }
    }
}
