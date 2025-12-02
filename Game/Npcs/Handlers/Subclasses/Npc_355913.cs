using MTA.Network.GamePackets;
using MTA.Database;

namespace MTA.Game.Npcs.Handlers.Subclasses
{
    /// <summary>
    /// Lead Performer NPC
    /// </summary>
    public static class Npc_355913
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    dialog.Text(
                        "Hello there! Please allow me to introduce to you the sub-class of Performers. "
                        + "Performers are gifted in the harmonizing of song and swordplay into "
                        + "beautiful, unique dances. These dances will surely make you the center of "
                        + "people`s attention! Would you like to join us? After you join this sub-class, you "
                        + "can level it up and get promoted to learn more stylish dances.");
                    if (!client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                        dialog.Option("Yes, I`d like to join it.", 1);
                    dialog.Option("I want to get promoted.", 2);
                    dialog.Option("How can I get promoted?", 3);
                    dialog.Option("Learn UniqueDance?", 11);
                    dialog.Option("Not my concern.", 255);
                    dialog.Avatar(100);
                    dialog.Send();
                    break;
                case 1:
                    dialog.Text(
                        "I`m glad that you are willing to be one of us. I`m sure your dances will be very "
                        + "charming. Still, there are a few requirements you need to meet. Though "
                        + "players of all classes can join us, we only accept players who are over level "
                        + "70, and it takes 15 Orchids to pay the tuition fee. After joining the Performer "
                        + "sub-class, click open your character sheet and you`ll find the "
                        + "sub-class button on the left-hand corner. You may level up your sub-class on "
                        + "the sub-class button sheet. You need to meet a certain sub-class level to get "
                        + "promoted to higher sub-class phases.");
                    dialog.Option("I see. Count me in.", 100);
                    dialog.Option("Oh, it does not suit me.", 255);
                    dialog.Avatar(100);
                    dialog.Send();
                    break;
                case 2:
                    if (!client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                    {
                        dialog.Text(
                            "You are not a Performer yet and can`t get promoted. Do you want to join the "
                            + "Performer sub-class now?");
                        dialog.Option("Yes, I`d like to join.", 1);
                        dialog.Option("Oh. Not now.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    else
                    {
                        dialog.Text(
                           "To promote to Phase " + (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase + 1).ToString() + " you must meet the requirements. Have you met "
                           + "them in the sub-class sheet?");
                        dialog.Option("Positive.", 200);
                        dialog.Option("Oh. Not yet.", 255);
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
                    if (Network.PacketHandler.PassLearn((byte)ClassID.Performer, client.Entity))
                    {
                        client.Entity.SubClasses.Classes.Add((byte)ClassID.Performer, new MTA.Game.SubClass() { ID = (byte)ClassID.Performer, Level = 1, Phase = 1 });
                        SubClassTable.Insert(client.Entity, (byte)ClassID.Performer);
                        client.Entity.SubClasses.SendLearn(ClassID.Performer, 1, client);
                        client.Entity.SubClasses.SendPromoted(ClassID.Performer, 1, client);
                        dialog.Text(
                            "Congratulations! You`ve learned the Performer way. Hope you use this power "
                            + "for the good of us all.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    else
                    {
                        dialog.Text(
                            "I`m sorry, you need to give me 15 Orchids and "
                            + "reach level 70 to join us.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 11:
                    {
                        dialog.Text("I can help you learn all Unique Dances. You should have the required Sub-Class and "
                            + "5,000 CPs to learn the dance. What would you like to choose to learn? ");
                        dialog.Option("Battle Dance [P1].", 12);
                        dialog.Option("Triumph [P3].", 13);
                        dialog.Option("Step Stomp [P5].", 14);
                        dialog.Option("Moon Light [P7].", 15);
                        dialog.Option("Snow Wind [P9].", 16);
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 12:
                    if (client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                    {
                        if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase >= 1 && client.Entity.ConquerPoints >= 5000)
                        {
                            client.AddSpell(new Spell(true) { ID = 1415 });
                            client.Entity.ConquerPoints -= 5000;
                            dialog.Text(
                                "Congratulations! You've learned the Battle Dance. Hope you use this power "
                                + "for the good of us all.");
                            dialog.Option("Thanks.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required Phase.");
                            dialog.Option("Oh.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 13:
                    if (client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                    {
                        if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase >= 3 && client.Entity.ConquerPoints >= 5000)
                        {
                            client.AddSpell(new Spell(true) { ID = 1416 });
                            client.Entity.ConquerPoints -= 5000;
                            dialog.Text(
                                "Congratulations! You've learned the Triumph. Hope you use this power "
                                + "for the good of us all.");
                            dialog.Option("Thanks.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required Phase.");
                            dialog.Option("Oh.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 14:
                    if (client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                    {
                        if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase >= 5 && client.Entity.ConquerPoints >= 5000)
                        {
                            client.AddSpell(new Spell(true) { ID = 1417 });
                            client.Entity.ConquerPoints -= 5000;
                            dialog.Text(
                                "Congratulations! You've learned the Step Stomp. Hope you use this power "
                                + "for the good of us all.");
                            dialog.Option("Thanks.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required Phase.");
                            dialog.Option("Oh.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 15:
                    if (client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                    {
                        if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase >= 7 && client.Entity.ConquerPoints >= 5000)
                        {
                            client.AddSpell(new Spell(true) { ID = 1418 });
                            client.Entity.ConquerPoints -= 5000;
                            dialog.Text(
                                "Congratulations! You've learned the Moon Light. Hope you use this power "
                                + "for the good of us all.");
                            dialog.Option("Thanks.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required Phase.");
                            dialog.Option("Oh.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 16:
                    if (client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                    {
                        if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase >= 9 && client.Entity.ConquerPoints >= 5000)
                        {
                            client.AddSpell(new Spell(true) { ID = 1419 });
                            client.Entity.ConquerPoints -= 5000;
                            dialog.Text(
                                "Congratulations! You've learned the Snow Wind. Hope you use this power "
                                + "for the good of us all.");
                            dialog.Option("Thanks.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required Phase.");
                            dialog.Option("Oh.", 255);
                            dialog.Avatar(100);
                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                    }
                    break;
                case 200:
                    if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase == 9)
                    {
                        dialog.Text("Your sub-class phase is already 9, you can`t promote it anymore!");
                        dialog.Option("Oh.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
                        return;
                    }
                    if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase < client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Level)
                    {
                        client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase++;
                        SubClassTable.Update(client.Entity, client.Entity.SubClasses.Classes[(byte)ClassID.Performer]);
                        dialog.Text("You have promoted your sub-class successfully.");
                        dialog.Option("Oh, Thanks.", 255);
                        dialog.Avatar(100);
                        dialog.Send();
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

