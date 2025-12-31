using System;
using MTA.Client;
using MTA.Game.ConquerStructures;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.SteedRace;

/// <summary>
///     Horse Race Manager - Allows players to join the Horse Race
/// </summary>
[NpcHandler(20)]
public static class NpcHorseRaceManager {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                var raceEvent = (BaseEvent)EventScheduler.GetEvent("STEED_RACE")!;
                var scheduleDesc = raceEvent.GetScheduleDescription();
                dialog.Text("Greetings, " + client.Entity.Name +
                            $"! The Steed Race begins {scheduleDesc}. Test your speed and skill against other riders to claim magnificent rewards!");
                dialog.Option("Yes, I wish to join!", 1);
                dialog.Option("Show me your wares.", 2);
                dialog.Option("Perhaps another time.", 255);
                dialog.Send();
                break;
            }
            case 1: {
                var raceEvent = (SteedRaceEvent)EventScheduler.GetEvent("STEED_RACE")!;
                if (!raceEvent.CanJoin) {
                    var scheduleDesc = raceEvent.GetScheduleDescription();
                    dialog.Text(
                        $"The Steed Race is not currently accepting participants. Races commence {scheduleDesc}. Return when the next race begins!");
                    dialog.Option("I shall return later.", 255);
                    dialog.Send();
                    break;
                }

                if (client.Equipment.Free(ConquerItem.Steed)) {
                    dialog.Text(
                        "You must first equip a steed, brave rider! A true racer cannot compete without their trusted mount.");
                    dialog.Option("I understand.", 255);
                    dialog.Send();
                    return;
                }

                if (!client.Spells.ContainsKey(7001)) {
                    dialog.Text(
                        "You must master the Riding skill before you can participate in the race. Seek out a skill trainer to learn this essential ability.");
                    dialog.Option("I understand.", 255);
                    dialog.Send();
                    return;
                }

                raceEvent.AddPlayer(client);
                break;
            }
            case 2: {
                if (client.Map.Npcs.TryGetValue(client.ActiveNpc, out var npc)) {
                    var data = new Data(true) {
                        ID = Data.OpenWindow,
                        UID = client.Entity.UID,
                        TimeStamp = Time32.Now,
                        dwParam = 464,
                        wParam1 = npc.X,
                        wParam2 = npc.Y
                    };
                    client.Send(data);
                }

                break;
            }
        }
    }
}

/// <summary>
///     Horse Race Prize Claimer - Allows winners to claim their prize
/// </summary>
[NpcHandler(4488522)]
public static class NpcHorseRacePrizeClaimer {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        switch (npcRequest.OptionID) {
            case 0: {
                dialog.Text("Greetings, " + client.Entity.Name +
                            "! Congratulations on completing the race, valiant rider! Would you like to claim your well-earned prize?");
                dialog.Option("Yes, I claim my reward!", 1);
                dialog.Option("Not at this moment.", 255);
                dialog.Send();
                break;
            }
            case 1: {
                var raceEvent = (SteedRaceEvent)EventScheduler.GetEvent("STEED_RACE")!;
                if (!raceEvent.IsActive) {
                    dialog.Text(
                        "I apologize, but the Steed Race tournament is not currently active. Prizes can only be claimed while the tournament is in progress.");
                    dialog.Option("I understand.", 255);
                    dialog.Send();
                    break;
                }

                if (DateTime.Now.Minute >= 45 && DateTime.Now.Minute <= 59) {
                    raceEvent.FinishRace(client);
                    client.Entity.ConquerPoints += 200;
                    Daily.CheckAlive();
                    client.Entity.Teleport(1002, 302, 278);
                    client.Send("Congratulations! You have claimed your prize of 200 Conquer Points!");
                }
                else {
                    dialog.Text("Greetings, " + client.Entity.Name +
                                ". Prizes can only be claimed between 45 and 59 minutes past the hour, while the tournament remains active.");
                    dialog.Option("I understand.", 255);
                    dialog.Send();
                }

                break;
            }
        }
    }
}