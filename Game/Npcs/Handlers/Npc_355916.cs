using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;
using MTA.Database;

namespace MTA.Game.Npcs.Handlers
{
    /// <summary>
    /// Wrangler NPC
    /// </summary>
    public static class Npc_355916
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    dialog.Text(
                        "Good day! Have you ever wondered what a Wrangler is? Well, please allow "
                        + "me to introduce this sub-class to you. Wrangler are masters of the "
                        + "MaxHp energies and can increase! "
                        + "Isn`t that wonderful? Would you like to join us? After you join "
                        + "this sub-class, you can level it up and get promoted to increase your power!");
                    if (!client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Wrangler))
                        dialog.Option("Yes, I`d like to join it.", 1);
                    dialog.Option("I want to get promoted.", 2);
                    dialog.Option("How can I get promoted?", 3);
                    dialog.Option("Not my concern.", 255);
                    dialog.Avatar(100);
                    dialog.Send();
                    break;
                case 1:
                    dialog.Text(
                        "I`m glad that you are willing to be one of us. I`m sure you will make a great "
                        + "Wrangler. Still, there are a few requiriments you need to meet. Though players "
                        + "of all classes can join us, we only accept players who are over level 70, and it "
                        + "takes 40 Saddle`s to pay the tuition fee. After joining the Wrangler sub-class, "
                        + "click open your character sheet and you`ll find the sub-class button on the "
                        + "left-hand corner. You may level up your sub-class on the sub-class button sheet. You "
                        + "need to meet a certain sub-class level to get promoted to higher sub-class "
                        + "phases.");
                    dialog.Option("I see. Count me in.", 100);
                    dialog.Option("Oh, it does not suit me.", 255);
                    dialog.Avatar(100);
                    dialog.Send();
                    break;
                case 2:
                    if (!client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Wrangler))
                    {
                        dialog.Text(
                            "You are not a Wrangler yet and can`t get promoted. Do you want to join the "
                            + "Warlock sub-class now?");
                        dialog.Option("Yes, I`d like to join.", 1);
                        dialog.Option("Oh. Not now.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    else
                    {
                        dialog.Text(
                           "To promote to Phase " + (client.Entity.SubClasses.Classes[(byte)ClassID.Wrangler].Phase + 1).ToString() + " you must meet the requirements. have you meet "
                           + "them into the sub-class sheet?");
                        dialog.Option("Positive.", 200);
                        dialog.Option("Oh. not yet.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 3:
                    dialog.Text(
                        "Good day! Please spare me a few minutes so that I can tell you about the "
                        + "sub-classes in Twin City. Each Sub-Class has 9 phases, and the higher the "
                        + "phase is, the more bonus effects the sub-class will offer. But keep in mind "
                        + "that these sub-classes also have a level system and you need to level them "
                        + "up before you can get promoted. You can level up your sub-class by gaining "
                        + "study points.");
                    dialog.Option("How can I get study points?", 30);
                    dialog.Option("Oh.", 255);
                    dialog.Avatar(100);
                    dialog.Send();
                    break;
                case 30:
                    dialog.Text(
                        "Well, the only way to get those points is to read the sub-class books, such as "
                        + "the Diligence Book. When you`ve gained enough study points, open the "
                        + "sub-class sheet from the character panel and level up the sub-class there. "
                        + "Monsters in the wild are always bothering young adventurers, so you might "
                        + "find some books from them.");
                    dialog.Option("Okay.", 255);
                    dialog.Avatar(100);
                    dialog.Send();
                    break;
                case 100:
                    if (Network.PacketHandler.PassLearn((byte)ClassID.Wrangler, client.Entity))
                    {
                        client.Entity.SubClasses.Classes.Add((byte)ClassID.Wrangler, new Game.SubClass() { ID = (byte)ClassID.Wrangler, Level = 1, Phase = 1 });
                        SubClassTable.Insert(client.Entity, (byte)ClassID.Wrangler);
                        client.Entity.SubClasses.SendLearn(ClassID.Wrangler, 1, client);
                        client.Entity.SubClasses.SendPromoted(ClassID.Wrangler, 1, client);
                        dialog.Text(
                            "Congratulations! You`ve learned the Wrangler way. Hope you use this power "
                            + "for the good of us all.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    else
                    {
                        dialog.Text(
                            "I`m sorry, you need to give me 40 Saddle's and "
                            + "reach level 70 to join us.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 200:
                    if (client.Entity.SubClasses.Classes[(byte)ClassID.Wrangler].Phase == 9)
                    {
                        dialog.Text("Your sub-class phase is already 9, you can`t promote it anymore!");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                        return;
                    }
                    if (client.Entity.SubClasses.Classes[(byte)ClassID.Wrangler].Phase < client.Entity.SubClasses.Classes[(byte)ClassID.Wrangler].Level)
                    {
                        client.Entity.SubClasses.Classes[(byte)ClassID.Wrangler].Phase++;
                        dialog.Text("You have promoted your sub-class successfully.");
                        dialog.Option("Oh, Thanks.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                        SubClassTable.Update(client.Entity, client.Entity.SubClasses.Classes[(byte)ClassID.Wrangler]);
                    }
                    else
                    {
                        dialog.Text("I`m sorry, you don`t meet the requirements yet.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
            }
        }
    }
}

