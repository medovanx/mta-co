using System;
using MTA.Network;
using MTA.Database;
using MTA;
using MTA.Interfaces;
using MTA.Network.GamePackets;

namespace MTA.Game.Attacking
{
    public class DurabilityItems
    {
        public static void AttackDurabilityItems(Entity attacker, Entity attacked)
        {
            if (attacker.EntityFlag == EntityFlag.Player)
            {
                try
                {
                    uint position = (uint)Kernel.Random.Next(1, 629);
                    if ((((position == 4) || (position == 5)) || (((position == 6) || (position == 10)) || (position == 18))) && !attacker.Owner.Equipment.Free(position))
                    {
                        ConquerItem item = attacker.Owner.Equipment.TryGetItem(position);
                        if ((((((item != null) && (position != 0)) && ((item.ID != 50000) && (item.ID != 1050000))) && (((item.ID != 1050001) && (item.ID != 1050002)) && ((item.ID != 1050020) && (item.ID != 1050021)))) && ((((item.ID != 1050022) && (item.ID != 1050023)) && ((item.ID != 1050030) && (item.ID != 1050031))) && (((item.ID != 1050032) && (item.ID != 1050033)) && ((item.ID != 1050040) && (item.ID != 1050041))))) && ((((item.ID != 1050042) && (item.ID != 1050043)) && ((item.ID != 1050050) && (item.ID != 1050051))) && ((item.ID != 1050052) && Kernel.ChanceSuccess((double)(100 - (attacker.Dodge / 90))))))
                        {
                            if ((item.ID > 0) && (item.Durability > 0))
                            {
                                if (item.Durability >= 1)
                                {
                                    if (attacker.EntityFlag == EntityFlag.Player)
                                    {
                                        if ((item.Durability - 1) <= 0)
                                        {
                                            item.Durability = 0;
                                        }
                                        else
                                        {
                                            item.Durability = (ushort)(item.Durability - 1);
                                        }
                                    }
                                    item.Mode = Enums.ItemMode.Update;
                                    ConquerItemTable.UpdateDurabilityItem2(item, item.ID);
                                    ConquerItemTable.UpdateItemID(attacker.Owner.Equipment.TryGetItem(position), attacker.Owner);
                                    if (item.Durability <= 100)
                                    {
                                        attacker.Owner.Send(new Message("Warning: Your item dura is too low, Go and repair it now! ", System.Drawing.Color.Red, 2011));
                                    }
                                }
                            }
                            else if ((item.Durability == 0) && (item.ID > 0))
                            {
                                item.Durability = 0;
                                item.Mode = Enums.ItemMode.Update;
                                attacker.Owner.CalculateStatBonus();
                                attacker.Owner.CalculateHPBonus();
                                attacker.Owner.GemAlgorithm();
                                attacker.Owner.Send(PacketHandler.WindowStats(attacker.Owner));
                                ConquerItemTable.SetDurabilityItem0(item);
                                ConquerItemTable.UpdateItemID(attacker.Owner.Equipment.TryGetItem(position), attacker.Owner);
                                attacker.Owner.Send(new Message("Warning: Your item dura is bad, go and repair it now.", System.Drawing.Color.Red, 2011));
                                _String str = new _String(true)
                                {
                                    UID = attacker.UID,
                                    Type = 10,
                                    TextsCount = 1
                                };
                                str.Texts.Add("expression_error");
                                attacker.Owner.Send(str.ToArray());
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception); Program.SaveException(exception);
                }
            }
        }
        public static void DefenceDurabilityItems(Entity attacker, Entity attacked)
        {
            try
            {
                if (((attacked != null) && (attacked.Owner != null)) && (attacked.EntityFlag == EntityFlag.Player))
                {
                    uint position = (uint)Kernel.Random.Next(1, 700);
                    if (((((position == 1) || (position == 2)) || ((position == 3) || (position == 8))) || ((position == 11) || (position == 0x12))) && !attacked.Owner.Equipment.Free(position))
                    {
                        ConquerItem item = attacked.Owner.Equipment.TryGetItem(position);
                        if (((item != null) || (position != 0)) && ((((((item.ID != 50000) && (item.ID != 1050000)) && ((item.ID != 1050001) && (item.ID != 1050002))) && (((item.ID != 1050020) && (item.ID != 1050021)) && ((item.ID != 1050022) && (item.ID != 1050023)))) && ((((item.ID != 1050030) && (item.ID != 1050031)) && ((item.ID != 1050032) && (item.ID != 1050033))) && (((item.ID != 1050040) && (item.ID != 1050041)) && ((item.ID != 1050042) && (item.ID != 1050043))))) && (((item.ID != 1050050) && (item.ID != 1050051)) && ((item.ID != 1050052) && Kernel.ChanceSuccess((double)(100 - (attacked.Dodge / 100)))))))
                        {
                            if ((item.ID > 0) && (item.Durability > 0))
                            {
                                if (item.Durability >= 1)
                                {
                                    if (attacker.EntityFlag == EntityFlag.Monster)
                                    {
                                        if ((item.Durability - 50) <= 0)
                                        {
                                            item.Durability = 0;
                                        }
                                        else
                                        {
                                            item.Durability = (ushort)(item.Durability - 50);
                                        }
                                    }
                                    else if (attacker.EntityFlag == EntityFlag.Player)
                                    {
                                        if ((item.Durability - 50) <= 0)
                                        {
                                            item.Durability = 0;
                                        }
                                        else
                                        {
                                            item.Durability = (ushort)(item.Durability - 50);
                                        }
                                    }
                                    item.Mode = Enums.ItemMode.Update;
                                    ConquerItemTable.UpdateDurabilityItem2(item, item.ID);
                                    ConquerItemTable.UpdateItemID(attacked.Owner.Equipment.TryGetItem(position), attacked.Owner);
                                    if (item.Durability <= 100)
                                    {
                                        attacked.Owner.Send(new Message("Warning: Your item dura is too low, Go and repair it now! ", System.Drawing.Color.Red, 2011));
                                    }
                                }
                            }
                            else if ((item.Durability == 0) && (item.ID > 0))
                            {
                                item.Durability = 0;
                                item.Mode = Enums.ItemMode.Update;
                                attacked.Owner.CalculateStatBonus();
                                attacked.Owner.CalculateHPBonus();
                                attacked.Owner.GemAlgorithm();
                                attacked.Owner.Send(PacketHandler.WindowStats(attacked.Owner));
                                ConquerItemTable.SetDurabilityItem0(item);
                                ConquerItemTable.UpdateItemID(attacked.Owner.Equipment.TryGetItem(position), attacked.Owner);
                                attacked.Owner.Send(new Message("Warning: Your item dura is bad, go and repair it now.", System.Drawing.Color.Red, 2011));
                                _String str = new _String(true)
                                {
                                    UID = attacked.UID,
                                    Type = 10,
                                    TextsCount = 1
                                };
                                str.Texts.Add("expression_error");
                                attacked.Owner.Send(str.ToArray());
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception); Program.SaveException(exception);
            }
        }
    }

}