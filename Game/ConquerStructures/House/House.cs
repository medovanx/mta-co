using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.ConquerStructures.House
{
	public class House
	{
		public class HouseInfo
		{
			public uint UID;
			public string? Name;
			public ushort ID;
			public ushort maptype;
			public ushort level;
			public Dictionary<uint, SobNpcSpawn>? Furnitures;
			public Warehouse? Warehouse;
		}
		public static SafeDictionary<uint, HouseInfo> Houses = [];
		public static void LoadHouses()
		{
			try
			{
				MySqlCommand command = new(MySqlCommandType.SELECT);
				command.Select("house");
				MySqlReader reader = new(command);
				while (reader.Read())
				{
					HouseInfo info = new()
					{
						UID = reader.ReadUInt32("UID"),
						Name = reader.ReadString("Name"),
						ID = reader.ReadUInt16("ID"),
						maptype = reader.ReadUInt16("maptype"),
						level = reader.ReadUInt16("level"),
						Furnitures = []
					};
					byte[] data = reader.ReadBlob("Furnitures");
					if (data.Length > 0)
					{
						using var stream = new MemoryStream(data);
						using var r = new BinaryReader(stream);
						int count = r.ReadByte();
						for (uint x = 0; x < count; x++)
						{
							SobNpcSpawn Base = new();
							Base = ReadItem(r);
							if ((Base.Mesh / 10) == 820)
							{
								Base.Type = (Enums.NpcType)2;
								info.Warehouse = new Warehouse(null, (Warehouse.WarehouseID)Base.UID);
								var items = LoadItems(Base.UID);
								foreach (var item in items.Values)
								{
									if (!info.Warehouse.ContainsUID(item.UID))
									{
										info.Warehouse.Add2(item, null);
									}
								}
							}
							else
								Base.Type = (Enums.NpcType)26;
							Base.MapID = info.ID;
							info.Furnitures.TryAdd(Base.UID, Base);
						}
					}
					if (!Houses.ContainsKey(info.UID))
						Houses.Add(info.UID, info);
					_ = new Map(info.ID, info.maptype, Kernel.Maps[info.maptype].Path);

				}
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
				Program.SaveException(exception);
			}
		}
		///////////////////////////////////////////////////
		public static void WriteItem(BinaryWriter writer, SobNpcSpawn Base)
		{
			writer.Write(Base.UID);
			writer.Write(Base.Mesh);
			writer.Write(Base.X);
			writer.Write(Base.Y);
		}
		public static SobNpcSpawn ReadItem(BinaryReader reader)
		{
			SobNpcSpawn Base = new()
			{
				UID = reader.ReadUInt32(),//8
				Mesh = reader.ReadUInt16(),//8
				X = reader.ReadUInt16(),//10
				Y = reader.ReadUInt16()//12
			};
			return Base;
		}
		///////////////////////////////////////////////////  
		public static void SaveFurnitures(Client.GameState client)
		{
			if (!Houses.TryGetValue(client.Entity.UID, out HouseInfo? info))
				return;
			MemoryStream stream = new();
			BinaryWriter writer = new(stream);
			writer.Write(value: (byte)(info.Furnitures?.Count ?? 0));
			if (info.Furnitures != null)
			{
				foreach (var fur in info.Furnitures.Values)
					WriteItem(writer, fur);
			}
			string SQL = "UPDATE `house` SET Furnitures=@Furnitures where UID = " + client.Entity.UID + " ;";
			byte[] rawData = stream.ToArray();
			using (var conn = DataHolder.MySqlConnection)
			{
				conn.Open();
				using var cmd = new MySql.Data.MySqlClient.MySqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = SQL;
				cmd.Parameters.AddWithValue("@Furnitures", rawData);
				cmd.ExecuteNonQuery();
			}
		}
		///////////////////////////////////////////////////
		public static void createhouse(GameState client)
		{
			HouseInfo info = new()
			{
				UID = client.Entity.UID,
				Name = client.Entity.Name,
				ID = (ushort)client.Entity.UID,
				maptype = 1098,
				level = 1,
				Furnitures = []
			};
			if (!Houses.ContainsKey(info.UID))
				Houses.Add(info.UID, info);
			_ = new Map(info.ID, info.maptype, Kernel.Maps[info.maptype].Path);

			MySqlCommand command = new(MySqlCommandType.INSERT);
			command.Insert("house").Insert("UID", client.Entity.UID)
				 .Insert("maptype", info.maptype).Insert("level", info.level)
				.Insert("Name", client.Entity.Name).Insert("ID", (ushort)client.Entity.UID);
			command.Execute();

		}
		public static void UpgradeHouse(GameState client, byte level)
		{
			ushort _base = 1098;
			if (level == 1)
				_base = 1099;
			if (level == 2)
				_base = 2080;
			if (level == 3)
				_base = 1765;
			if (level == 4)
				_base = 3024;

			level++;
			if (level > 5)
				return;

			new MySqlCommand(MySqlCommandType.UPDATE).Update("house")
				.Set("Name", client.Entity.Name).Set("ID", (ushort)client.Entity.UID)
				.Set("maptype", _base).Set("level", level).Where("UID", client.Entity.UID).Execute();
			if (Kernel.Maps.ContainsKey((ushort)client.Entity.UID))
			{
				Kernel.Maps.Remove((ushort)client.Entity.UID);
				_ = new Map((ushort)client.Entity.UID, _base, Kernel.Maps[_base].Path);
			}
			if (Houses.ContainsKey(client.Entity.UID))
			{
				Houses[client.Entity.UID].maptype = _base;
				Houses[client.Entity.UID].level = level;
				//     Houses[client.Entity.UID].Furnitures = new Dictionary<uint, SobNpcSpawn>();
				SaveFurnitures(client);
			}
		}

		public static void DowngradeHouse(GameState client, byte currentLevel)
		{
			if (currentLevel <= 1)
				return; // Cannot downgrade below level 1

			byte newLevel = (byte)(currentLevel - 1);
			ushort _base = 1098; // Default for level 1

			// Determine maptype based on the new level
			if (newLevel == 1)
				_base = 1098;
			else if (newLevel == 2)
				_base = 1099;
			else if (newLevel == 3)
				_base = 2080;
			else if (newLevel == 4)
				_base = 1765;

			new MySqlCommand(MySqlCommandType.UPDATE).Update("house")
				.Set("Name", client.Entity.Name).Set("ID", (ushort)client.Entity.UID)
				.Set("maptype", _base).Set("level", newLevel).Where("UID", client.Entity.UID).Execute();

			if (Kernel.Maps.ContainsKey((ushort)client.Entity.UID))
			{
				Kernel.Maps.Remove((ushort)client.Entity.UID);
				_ = new Map((ushort)client.Entity.UID, _base, Kernel.Maps[_base].Path);
			}

			if (Houses.TryGetValue(client.Entity.UID, out HouseInfo? value))
			{
				value.maptype = _base;
				value.level = newLevel;
				SaveFurnitures(client);
			}
		}

		public static void Teleport(GameState client, HouseInfo info)
		{
			client.Entity.AdvancedTeleport(true);
			ushort X, Y;
			var cord = Kernel.Maps[info.maptype].RandomCoordinates();
			X = cord.Item1;
			Y = cord.Item2;
			if (client.Entity.EntityFlag == EntityFlag.Player)
			{
				if (client.InQualifier())
				{
					if (client.InQualifier())
					{
						if (client.Entity.MapID != 700 && client.Entity.MapID < 11000)
						{
							client.EndQualifier();
						}
					}
				}
			}

			client.Entity.X = X;
			client.Entity.Y = Y;
			client.Entity.PX = 0;
			client.Entity.PY = 0;
			client.Entity.PreviousMapID = client.Entity.MapID;
			client.Entity.MapID = info.ID;

			Data Data = new(true)
			{
				UID = client.Entity.UID,
				ID = Network.GamePackets.Data.Teleport,
				dwParam = info.maptype,
				wParam1 = X,
				wParam2 = Y
			};
			client.Send(Data);
			client.Send(new MapStatus() { BaseID = info.maptype, ID = info.ID });
			client.Entity.AdvancedTeleport(true);
		}

		public static void HouseWarehouse(GameState client, Network.GamePackets.Warehouse? warehousepacket = null)
		{
			if (client != null)
			{
				if (Houses.TryGetValue(client.Entity.UID, out HouseInfo? info))
				{
					var itembox = info.Furnitures?.Values.Where(xx => (xx.Mesh / 10) == 820).FirstOrDefault();
					if (itembox != null)
					{
						if (!client.Warehouses.ContainsKey((Warehouse.WarehouseID)itembox.UID))
						{
							info.Warehouse ??= new Warehouse(null, (Warehouse.WarehouseID)itembox.UID);
							client.Warehouses.Add((Warehouse.WarehouseID)itembox.UID, info.Warehouse);
						}
					}
				}
			}
		}
		public static SafeDictionary<uint, ConquerItem> LoadItems(uint Warehouse)
		{
			SafeDictionary<uint, ConquerItem> Items = [];
			using (var cmdx = new MySqlCommand(MySqlCommandType.SELECT).Select("items").Where("Warehouse", Warehouse))
			using (var readerx = new MySqlReader(cmdx))
			{
				while (readerx.Read())
				{
					var item = ConquerItemTable.deserialzeItem(readerx);
					if (!Items.ContainsKey(item.UID))
						Items.Add(item.UID, item);
				}
			}
			return Items;
		}

		private static ConquerItem deserialzeItem(MySqlReader reader)
		{
			ConquerItem item = new(true)
			{
				ID = reader.ReadUInt32("Id"),
				UID = reader.ReadUInt32("Uid"),
				//item.Durability = reader.ReadUInt16("Durability");
				MaximDurability = reader.ReadUInt16("MaximDurability")
			};
			item.Durability = item.MaximDurability;
			item.Position = reader.ReadUInt16("Position");
			item.Agate = reader.ReadString("Agate");
			item.SocketProgress = reader.ReadUInt32("SocketProgress");
			item.PlusProgress = reader.ReadUInt32("PlusProgress");
			item.SocketOne = (Enums.Gem)reader.ReadUInt16("SocketOne");
			item.SocketTwo = (Enums.Gem)reader.ReadUInt16("SocketTwo");
			item.Effect = (Enums.ItemEffect)reader.ReadUInt16("Effect");
			item.Mode = Enums.ItemMode.Default;
			item.Plus = reader.ReadByte("Plus");
			item.Bless = reader.ReadByte("Bless");
			item.Bound = reader.ReadBoolean("Bound");
			item.Enchant = reader.ReadByte("Enchant");
			item.Lock = reader.ReadByte("Locked");
			item.UnlockEnd = DateTime.FromBinary(reader.ReadInt64("UnlockEnd"));
			item.Suspicious = reader.ReadBoolean("Suspicious");
			item.SuspiciousStart = DateTime.FromBinary(reader.ReadInt64("SuspiciousStart"));
			item.Color = (Enums.Color)reader.ReadUInt32("Color");
			item.Warehouse = reader.ReadUInt16("Warehouse");
			item.StackSize = reader.ReadUInt16("StackSize");
			item.RefineItem = reader.ReadUInt32("RefineryItem");
			Int64 rTime = reader.ReadInt64("RefineryTime");

			if (item.ID == 300000)
			{
				uint NextSteedColor = reader.ReadUInt32("NextSteedColor");
				item.NextGreen = (byte)(NextSteedColor & 0xFF);
				item.NextBlue = (byte)((NextSteedColor >> 8) & 0xFF);
				item.NextRed = (byte)((NextSteedColor >> 16) & 0xFF);
			}
			if (item.RefineItem > 0 && rTime != 0)
			{
				item.RefineryTime = DateTime.FromBinary(rTime);
				if (DateTime.Now > item.RefineryTime)
				{
					item.RefineryTime = new DateTime(0);
					item.RefineItem = 0;
				}
			}
			if (item.Lock == 2)
				if (DateTime.Now >= item.UnlockEnd)
					item.Lock = 0;

			item.DayStamp = DateTime.FromBinary(reader.ReadInt64("DayStamp"));
			item.Days = reader.ReadByte("Days");
			return item;
		}

		public static HouseInfo? SpouseHouse(string Spousename)
		{
			foreach (var house in Houses.Values)
				if (house.Name == Spousename)
					return house;
			return null;
		}

		public static bool SpouseWarehouse(GameState client, Network.GamePackets.Warehouse warehousepacket)
		{
			HouseWarehouse(client, warehousepacket);
			var info = SpouseHouse(client.Entity.Spouse);
			if (info == null || client.Entity.MapID == client.Entity.UID)
				info = Houses[client.Entity.UID];
			if (info != null)
			{
				if (client.Entity.MapID == info.ID)
				{
					switch (warehousepacket.Type)
					{
						case Network.GamePackets.Warehouse.Entire:
							{
								Warehouse? wh = info.Warehouse;
								if (wh == null) return true;
								byte count = 0;
								warehousepacket.Count = 1;
								warehousepacket.Type = Network.GamePackets.Warehouse.AddItem;
								for (; count < wh.Count; count++)
								{
									warehousepacket.Append(wh.Objects[count]);
									client.Send(warehousepacket);
									ItemAdding add = new ItemAdding(true);
									if (wh.Objects[count].Purification.Available)
										add.Append(wh.Objects[count].Purification);
									if (wh.Objects[count].ExtraEffect.Available)
										add.Append(wh.Objects[count].ExtraEffect);
									if (wh.Objects[count].Purification.Available || wh.Objects[count].ExtraEffect.Available)
										client.Send(add);

								}
								return true;
							}
						case Network.GamePackets.Warehouse.AddItem:
							{
								Warehouse? wh = info.Warehouse;
								if (wh == null) return true;
								if (client.Inventory.TryGetItem(warehousepacket.UID, out ConquerItem item))
								{
									if (item.ID >= 729960 && item.ID <= 729970)
										return true;
									if (item.ID == 729611 || item.ID == 729612 || item.ID == 729613 || item.ID == 729614 || item.ID == 729703)
										return true;
									if (!ConquerItem.isRune(item.UID))
									{
										if (wh.Add2(item, client))
										{
											warehousepacket.UID = 0;
											warehousepacket.Count = 1;
											warehousepacket.Append(item);
											client.Send(warehousepacket);

											ItemAdding add = new ItemAdding(true);
											if (item.Purification.Available)
												add.Append(item.Purification);
											if (item.ExtraEffect.Available)
												add.Append(item.ExtraEffect);
											if (item.Purification.Available || item.ExtraEffect.Available)
												client.Send(add);

											info.Warehouse = wh;
											return true;
										}
									}
									else client.Send(new Message("You can not store Flame Stone Rune's in Warehouse", System.Drawing.Color.Red, Message.TopLeft));
								}
								break;
							}
						case Network.GamePackets.Warehouse.RemoveItem:
							{
								if (!client.Partners.ContainsKey(info.UID) && client.Entity.UID != info.UID)
								{
									client.Send(new Message("Sorry you cant, You Should be a Trade Partner.", Message.TopLeft));
									return true;
								}
								Warehouse? wh = info.Warehouse;
								if (wh == null) return true;
								if (wh.ContainsUID(warehousepacket.UID))
								{
									if (wh.Remove2(warehousepacket.UID, client))
									{
										info.Warehouse = wh;
										client.Send(warehousepacket);
										return true;
									}
								}
								break;
							}

					}


				}
			}
			return false;
		}

		public static SobNpcSpawn? CheckItemBox(GameState client, HouseInfo info)
		{
			return info.Furnitures?.Values.FirstOrDefault(xx => (xx.Mesh / 10) == 820);

		}

		public static void Move(GameState client, SobNpcSpawn sobnpc, HouseInfo info)
		{
			client.MessageBox("Do u Want To change its place?", (p) =>
			{
				info.Furnitures?.Remove(sobnpc.UID);
				p.Screen.FullWipe();
				p.Screen.Reload();
				NpcRequest req2 = new(5)
				{
					Mesh = sobnpc.Mesh,
					NpcTyp = sobnpc.Type
				};
				p.Send(req2);
			}, null);
		}
	}
}