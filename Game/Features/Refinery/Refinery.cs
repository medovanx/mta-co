using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MTA.Database;

namespace MTA
{
    public class Refinery
    {
        public class RefineryBoxes
        {
            public UInt32 Identifier, Position;
            public Boolean Untradable;
            public RefineryItem.RefineryType Type;
        }
        public class RefineryItem
        {
            public UInt32 Identifier, Position, Percent;
            public Byte Level;
            public Boolean Untradable;
            public RefineryType Type;

            public enum RefineryType
            {
                MDefence = 1,
                Critical = 2,
                SCritical = 3,
                Immunity = 4,
                BreakThrough = 5,
                Counteraction = 6,
                Detoxication = 7,
                Block = 8,
                Penetration = 9,
                Intensification = 11
            }
        }
        public static void Load()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("databaserefinery"))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    RefineryItem ri = new RefineryItem();
                    ri.Identifier = reader.ReadUInt32("Identifier");
                    ri.Level = reader.ReadByte("Level");
                    ri.Percent = reader.ReadUInt32("Percent");
                    ri.Position = reader.ReadUInt32("Position");
                    ri.Type = (RefineryItem.RefineryType)reader.ReadUInt32("RefineType");
                    ri.Untradable = reader.ReadBoolean("Untradable");
                    Kernel.DatabaseRefinery.Add(ri.Identifier, ri);
                }
            }
            Console.WriteLine(String.Format("Loaded Database Refinery Items ({0}).", Kernel.DatabaseRefinery.Count));
            LoadBoxes();
        }
        public static void LoadBoxes()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("databaserefineryboxes"))
            using (var reader = cmd.CreateReader())
            {
                while (reader.Read())
                {
                    RefineryBoxes ri = new RefineryBoxes();
                    ri.Identifier = reader.ReadUInt32("Identifier");
                    ri.Position = reader.ReadUInt32("Position");
                    ri.Type = (RefineryItem.RefineryType)reader.ReadUInt32("RefineType");
                    ri.Untradable = reader.ReadBoolean("Untradable");

                    Kernel.DatabaseRefineryBoxes.Add(ri.Identifier, ri);
                }
            }

            Console.WriteLine(String.Format("Loaded Database Refinery Boxes ({0}).", Kernel.DatabaseRefinery.Count));
        }
    }
}