using MTA.Network.GamePackets;
using MTA.Database;

namespace MTA.Game.Npcs.Handlers.Subclasses
{
    /// <summary>
    /// Lead Performer
    /// </summary>
    [NpcHandler(355913)]
    public static class Npc_LeadPerformer
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    dialog.Text(
                        "Greetings! Let me introduce you to the Performer sub-class. "
                        + "Performers excel at blending music and swordplay into mesmerizing dances. "
                        + "These unique dances will make you the center of attention! Would you like to join us? "
                        + "After joining, you can level up and get promoted to unlock even more stylish dances.");
                    if (!client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                        dialog.Option("Yes, I`d like to join it.", 1);
                    dialog.Option("I want to get promoted.", 2);
                    dialog.Option("Learn unique dances.", 11);
                    dialog.Option("Learn dances [D2 - D8]", 20);
                    dialog.Option("Not my concern.", 255);
                    dialog.Send();
                    break;
                case 1:
                    dialog.Text(
                        "I'm glad you're interested in joining us. I'm sure your dances will be enchanting! "
                        + "There are a few requirements: any class can join, but you must be over level 70, "
                        + "and pay 15 Orchids as a tuition fee. After joining, open your character sheet and "
                        + "look for the sub-class button in the lower left corner. You can level up your sub-class "
                        + "there. You must reach certain sub-class levels to be promoted to higher phases.");
                    dialog.Option("I see. Count me in.", 100);
                    dialog.Option("Oh, it does not suit me.", 255);
                    dialog.Send();
                    break;
                case 2:
                    if (!client.Entity.SubClasses.Classes.ContainsKey((byte)ClassID.Performer))
                    {
                        dialog.Text(
                            "You are not a Performer yet and cannot be promoted. Would you like to join the "
                            + "Performer sub-class now?");
                        dialog.Option("Yes, I`d like to join.", 1);
                        dialog.Option("Oh. Not now.", 255);
                        dialog.Send();
                    }
                    else
                    {
                        dialog.Text(
                            $"To promote to Phase {client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase + 1}, you must meet the requirements. Have you met them in the sub-class sheet?");
                        dialog.Option("Positive.", 200);
                        dialog.Option("Oh. Not yet.", 255);
                        dialog.Send();
                    }
                    break;
                case 100:
                    if (Network.PacketHandler.PassLearn((byte)ClassID.Performer, client.Entity))
                    {
                        client.Entity.SubClasses.Classes.Add((byte)ClassID.Performer, new MTA.Game.SubClass() { ID = (byte)ClassID.Performer, Level = 1, Phase = 1 });
                        SubClassTable.Insert(client.Entity, (byte)ClassID.Performer);
                        client.Entity.SubClasses.SendLearn(ClassID.Performer, 1, client);
                        client.Entity.SubClasses.SendPromoted(ClassID.Performer, 1, client);
                        dialog.Text(
                            "Congratulations! You have learned the Performer way. Use your new power wisely!");
                        dialog.Option("Oh.", 255);

                        dialog.Send();
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you need to give me 15 Orchids and reach level 70 to join us.");
                        dialog.Option("Oh.", 255);

                        dialog.Send();
                    }
                    break;
                case 11:
                    {
                        dialog.Text("I can help you learn unique dances. You must have the required sub-class and 5,000 CPs to learn each dance. Which dance would you like to learn?");
                        dialog.Option("Battle Dance [P1].", 12);
                        dialog.Option("Triumph [P3].", 13);
                        dialog.Option("Step Stomp [P5].", 14);
                        dialog.Option("Moon Light [P7].", 15);
                        dialog.Option("Snow Wind [P9].", 16);
                        dialog.Option("Oh.", 255);

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
                                "Congratulations! You have learned Battle Dance. Enjoy your new skill!");
                            dialog.Option("Thanks.", 255);

                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required phase.");
                            dialog.Option("Oh.", 255);

                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);

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
                                "Congratulations! You have learned Triumph. Enjoy your new skill!");
                            dialog.Option("Thanks.", 255);

                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required phase.");
                            dialog.Option("Oh.", 255);

                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);

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
                                "Congratulations! You have learned Step Stomp. Enjoy your new skill!");
                            dialog.Option("Thanks.", 255);

                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required phase.");
                            dialog.Option("Oh.", 255);

                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);

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
                                "Congratulations! You have learned Moon Light. Enjoy your new skill!");
                            dialog.Option("Thanks.", 255);

                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required phase.");
                            dialog.Option("Oh.", 255);

                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);

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
                                "Congratulations! You have learned Snow Wind. Enjoy your new skill!");
                            dialog.Option("Thanks.", 255);

                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text(
                                "I'm sorry, you don't have enough CPs or the required phase.");
                            dialog.Option("Oh.", 255);

                            dialog.Send();
                        }
                    }
                    else
                    {
                        dialog.Text(
                            "I'm sorry, you are not a Performer yet.");
                        dialog.Option("Oh.", 255);

                        dialog.Send();
                    }
                    break;
                case 200:
                    if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase == 9)
                    {
                        dialog.Text("Your sub-class phase is already 9. You cannot promote it any further!");
                        dialog.Option("Oh.", 255);

                        dialog.Send();
                        return;
                    }
                    if (client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase < client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Level)
                    {
                        client.Entity.SubClasses.Classes[(byte)ClassID.Performer].Phase++;
                        SubClassTable.Update(client.Entity, client.Entity.SubClasses.Classes[(byte)ClassID.Performer]);
                        dialog.Text("You have successfully promoted your sub-class.");
                        dialog.Option("Oh, Thanks.", 255);

                        dialog.Send();
                    }
                    else
                    {
                        dialog.Text("I'm sorry, you do not meet the requirements yet.");
                        dialog.Option("Oh.", 255);

                        dialog.Send();
                    }
                    break;
                case 20:
                    {
                        ushort[] danceSpells = { 1380, 1385, 1390, 1395, 1400, 1405, 1410 };
                        foreach (var spellId in danceSpells)
                        {
                            client.AddSpell(new Spell(true) { ID = spellId });
                        }
                        dialog.Text("Congratulations! You have learned all dances from Dance 2 to Dance 8.");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                        break;
                    }
            }
        }
    }
}

