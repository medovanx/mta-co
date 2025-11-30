using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Network.GamePackets;
using HoldemHand;
using MTA.Game;
using MTA.Network;

namespace MTA.Game.ConquerStructures
{
    #region PokerPlayer
    public class PokerPlayer
    {
        public List<Game.Enums.SeatAttributeEnum> SeatAttributes = new List<Game.Enums.SeatAttributeEnum>();
        public Entity Entity
        {
            get
            {
                if (Kernel.GamePool.ContainsKey(UID))
                    return Kernel.GamePool[UID].Entity;
                else { Online = false; return null; }
            }
            set
            {
                UID = value.UID;
            }
        }
        public PokerTable MyTable
        {
            get
            {
                if (Database.PokerTables.Tables.ContainsKey(TableUID)) return Database.PokerTables.Tables[TableUID];
                else return null;
            }
            set
            {
                TableUID = value.UID;
            }
        }
        public byte Seat;
        public bool Online = true;
        public uint TableUID;
        public uint UID;
        public List<Game.ConquerStructures.PokerCard> Cards = new List<Game.ConquerStructures.PokerCard>();
        public ulong TotalBet;
        public ulong Bet;
        public byte CurrentState = 1;
        public Game.Enums.PokerCallTypes RoundState = 0;
        public bool IsPlaying;
        public ulong Wins;
        public ulong MyBet;
        public ulong UnWins;
        public ulong MoneySafeAmnt
        {
            get
            {
                if (MyTable == null) return 0;
                if (!Online) return 0;
                if (MyTable.BetType == Game.Enums.PokerBetType.Money)
                    return Entity.Money;
                else if (MyTable.BetType == Game.Enums.PokerBetType.ConquerPoints)
                    return (ulong)Entity.ConquerPoints;
                else
                    return 0;
            }
        }
        public bool IsAllIn
        {
            get
            {
                return RoundState == Game.Enums.PokerCallTypes.AllIn;
            }
        }
        public void Send(Interfaces.IPacket p)
        {
            Send(p.ToArray());
        }
        public void Send(byte[] p)
        {
            if (!Online)
            {
                if (Database.PokerTables.Tables.ContainsKey(TableUID))
                {
                    if (Database.PokerTables.Tables[TableUID].Players.ContainsKey(UID))
                        Database.PokerTables.Tables[TableUID].Players[UID].RoundState = Game.Enums.PokerCallTypes.Fold;
                    return;
                }
            }
            if (Kernel.GamePool.ContainsKey(UID))
                Kernel.GamePool[UID].Send(p);
            else
            {
                Online = false;
                if (Database.PokerTables.Tables.ContainsKey(TableUID))
                {
                    if (Database.PokerTables.Tables[TableUID].Players.ContainsKey(UID))
                        Database.PokerTables.Tables[TableUID].Players[UID].RoundState = Game.Enums.PokerCallTypes.Fold;
                }
            }
        }
        public ushort GetFullPower(List<Game.ConquerStructures.PokerCard> TCards)
        {
            ushort P = 0;
            foreach (var C in TCards)
            {

                P += (ushort)(C.ID);
            }
            return P;
        }
        public void PlayMoney(ulong Money, ulong RoundMaxBet = 0)
        {
            if (MyTable.BetType == Game.Enums.PokerBetType.Money)
            {
                if (Entity.Money >= Money)
                    Entity.Money -= (ulong)Money;
                else
                    Entity.Money = 0;
            }
            else if (MyTable.BetType == Game.Enums.PokerBetType.ConquerPoints)
            {
                if (Entity.ConquerPoints >= Money)
                    Entity.ConquerPoints -= (uint)Money;
                else
                    Entity.ConquerPoints = 0;
            }

            Bet += Money;
            MyTable.Pot += Money;
            MyTable.MinimumRaiseAmount = Math.Max(MyTable.MinimumRaiseAmount, Bet);
            if (Bet > MyTable.HigherBet) MyTable.HigherBet = Bet;

            var D = new Network.GamePackets.Data(true);
            D.ID = 234;
            D.UID = MyTable.UID;
            D.dwParam = (uint)MyTable.Pot;
            D.Data24_Uint = (uint)RoundMaxBet;
            MyTable.SendToAll(D.ToArray());
        }
    }
    #endregion
    #region PokerTable
    public class PokerTable
    {
        #region Fields
        public Dictionary<uint, PokerPlayer> Players = new Dictionary<uint, PokerPlayer>(10);
        public Dictionary<uint, PokerPlayer> Watchers = new Dictionary<uint, PokerPlayer>(10);
        public uint UID;
        public byte Number;
        public bool TableType = false;
        public Game.Enums.PokerBetType BetType;
        public ushort X;
        public ushort Y;
        public uint Mesh;
        public bool Unlimited;
        public uint MinLimit;
        public ushort Map;
        public ulong Pot = 0;
        public ulong QuitPot = 0;
        private byte State = 1;
        public ulong HigherBet;
        public bool Crazy = false;
        public bool ShowHand = false;
        public ushort CountDown = 15;
        public uint MoveCountDown;
        public uint RoundCountDown;
        public void StopMoveCountDown()
        {
            Program.World.DelayedTask.Remove(MoveCountDown);
            MoveCountDown = 0;
        }
        public void StopRoundCountDown()
        {
            if (Players.Count < 1) return;
            Program.World.DelayedTask.Remove(RoundCountDown);
            RoundCountDown = 0;
        }
        private uint BigBlind;
        private uint SmallBlind;
        private Stack<Game.ConquerStructures.PokerCard> _ShuffleDeckCards;
        public List<Game.ConquerStructures.PokerCard> TableCards = new List<Game.ConquerStructures.PokerCard>();
        protected readonly List<MoneyPot> m_Pots = new List<MoneyPot>();
        private int m_CurrPotId;
        public List<MoneyPot> Pots
        {
            get { return m_Pots; }
        }
        private Game.Enums.RoundTypeEnum Round;
        private Game.Enums.RoundStateEnum m_RoundState;
        public Game.Enums.GameClientEnum m_State; // L'etat global de la game       


        /// <summary>
        /// Minimum amount to Raise
        /// </summary>
        public ulong MinimumRaiseAmount { get; set; }
        /// <summary>
        /// How many players are still in the Game (All-In not included)
        /// </summary>
        public int NbPlaying { get { return Players.Values.Count(x => x.IsPlaying && !x.IsAllIn); } }

        /// <summary>
        /// How many players are still in the Game (All-In included)
        /// </summary>
        public int NbPlayingAndAllIn { get { return NbPlaying + NbAllIn; } }
        /// <summary>
        /// How many player have played this round and are ready to play the next one
        /// </summary>
        public int NbPlayed { get; set; }

        private readonly List<ulong> m_AllInCaps = new List<ulong>(); // All the distincts ALL_IN CAPS of the ROUND
        private bool NewGame;
        /// <summary>
        /// How many players are All In
        /// </summary>
        public int NbAllIn { get; set; }
        #endregion Fields
        #region Seats

        /// <summary>
        /// Who is the current player
        /// </summary>       
        public PokerPlayer CurrentPlayer
        {
            get
            {
                return Players.Values.FirstOrDefault(x => x.SeatAttributes.Contains(Game.Enums.SeatAttributeEnum.CurrentPlayer));
            }
        }
        public PokerPlayer DealerPlayer
        {
            get
            {
                return Players.Values.FirstOrDefault(x => x.SeatAttributes.Contains(Game.Enums.SeatAttributeEnum.Dealer));
            }
        }
        public PokerPlayer BigBlindPlayer
        {
            get
            {
                return Players.Values.FirstOrDefault(x => x.SeatAttributes.Contains(Game.Enums.SeatAttributeEnum.BigBlind));
            }
        }
        public PokerPlayer SmallBlindPlayer
        {
            get
            {
                return Players.Values.FirstOrDefault(x => x.SeatAttributes.Contains(Game.Enums.SeatAttributeEnum.SmallBlind));
            }
        }
        public PokerPlayer SeatOfTheFirstPlayer
        {
            get
            {
                var seat = GetSeatOfPlayingPlayerNextTo(DealerPlayer);

                if (Round == Game.Enums.RoundTypeEnum.Preflop)
                {
                    //Ad B : A      A
                    //Ad B C: A     A->B->C->A
                    //Ad B C D: D   A->B->C->D
                    seat = NbPlayingAndAllIn < 3 ? DealerPlayer : GetSeatOfPlayingPlayerNextTo(GetSeatOfPlayingPlayerNextTo(GetSeatOfPlayingPlayerNextTo(DealerPlayer)));
                }

                return seat;
            }
        }
        public bool CheckSeat(byte Seat)
        {
            bool Free = true;
            foreach (var P in Players.Values)
            {
                if (P.Seat == Seat) Free = false;
            }
            foreach (var P in Watchers.Values)
            {
                if (P.CurrentState == 3)
                    if (P.Seat == Seat) Free = false;
            }
            return Free;
        }
        public PokerPlayer ChangeCurrentPlayerTo(PokerPlayer player)
        {
            var oldPlayer = CurrentPlayer;
            if (oldPlayer != null)
                oldPlayer.SeatAttributes.Remove(Game.Enums.SeatAttributeEnum.CurrentPlayer);
            if (player != null)
                player.SeatAttributes.Add(Game.Enums.SeatAttributeEnum.CurrentPlayer);

            return player;
        }
        public PokerPlayer GetSeatOfPlayingPlayerNextTo(PokerPlayer player)
        {
            var noSeat = player == null ? -1 : player.Seat;
            for (var i = 0; i < 10; ++i)
            {
                var si = (byte)((noSeat + 1 + i) % 10);
                var PlayerID = PlayerBySeat(si);
                if (Players.ContainsKey(PlayerID))
                {
                    var Player = Players[PlayerID];
                    if (Player.RoundState != Game.Enums.PokerCallTypes.Fold && Player.Online)
                        return Player;
                }
            }
            return new PokerPlayer();
        }
        public PokerPlayer GetSeatOfPlayingPlayerJustBefore(PokerPlayer player)
        {
            var noSeat = player == null ? -1 : player.Seat;
            for (var i = 0; i < 10; ++i)
            {
                var id = (noSeat - 1 - i) % 10;
                if (id < 0)
                    id = 10 + id;
                var si = (byte)((noSeat + 1 + i) % 10);
                var PlayerID = PlayerBySeat(si);
                if (Players.ContainsKey(PlayerID))
                {
                    var Player = Players[PlayerID];
                    if (Player.RoundState != Game.Enums.PokerCallTypes.Fold && Player.Online)
                        return Player;
                }
            }
            return new PokerPlayer();
        }
        private uint PlayerBySeat(byte S)
        {
            uint I = 0;
            foreach (var P in Players.Values)
                if (P.Seat == S) I = P.UID;
            return I;
        }
        public PokerPlayer GetPlayerFromName(string Name)
        {
            foreach (var pl in Players.Values)
            {
                if (pl.Entity.Name == Name)
                    return pl;
            }
            return new PokerPlayer();
        }

        #endregion
        public void ToLocal(byte[] P)
        {
            var Locals = Kernel.GamePool.Values.ToArray();
            foreach (Client.GameState client in Locals)
            {
                if (client != null)
                {
                    if (client.Map.ID == Map)
                    {
                        if (Kernel.GetDistance(client.Entity.X, client.Entity.Y, X, Y) > 25)
                        {
                            continue;
                        }
                        client.Send(P);
                    }

                }
            }
        }
        public void ToLocal(Interfaces.IPacket P)
        {
            ToLocal(P.ToArray());
        }
        public void SendToAll(Interfaces.IPacket P)
        {
            SendToAll(P.ToArray());
        }
        public void SendToAll(byte[] P)
        {
            foreach (var Player in Players.Values)
                Player.Send(P);
            foreach (var Player in Watchers.Values)
                Player.Send(P);
        }
        public void Spawn(Client.GameState client)
        {
            client.Send(Spawn());
        }
        public byte[] Spawn()
        {
            var Info = new MsgTexasNpcInfo(Players.Count)
            {
                UID = UID,

                Number = Number,
                TableType = TableType,
                BetType = (byte)BetType,
                X = X,
                Y = Y,
                Mesh = Mesh,
                Unlimited = Unlimited,
                MinLimit = MinLimit,
                Pot = Pot,
                State = State
            };
            if (Mesh == 7247567 || Mesh == 7255787)
                Crazy = true;
            if (Mesh == 7578087)
            {
                ShowHand = true;
                TableType = true;
            }
            foreach (var p in Players.Values)
                Info.Append(p);

            return Info.ToArray();
        }
        public ulong MinRaiseAmnt(PokerPlayer p)
        {
            return Math.Min(CallAmnt(p) + MinimumRaiseAmount, MaxRaiseAmnt(p));
        }
        private ulong MaxRaiseAmnt(PokerPlayer p)
        {
            return p.MoneySafeAmnt;
        }
        public ulong CallAmnt(PokerPlayer p)
        {
            return HigherBet - p.Bet;
        }
        private uint GetHighestPower()
        {
            uint WinnerId = 0;
            ushort HighestPower = 0;
            ulong HighestHandPower = 0;
            foreach (var Pla in Players.Values)
            {
                if (Pla.RoundState == Game.Enums.PokerCallTypes.Fold || !Pla.Online) continue;
                ulong HP = GetHandPower(Pla.Cards.ToArray(), Pla);
                if (HP > HighestHandPower)
                {
                    HighestHandPower = HP;
                    WinnerId = Pla.UID;
                    HighestPower = Pla.GetFullPower(Pla.Cards);
                }
                else if (HP == HighestHandPower)
                {
                    //if (Pla.GetFullPower(Pla.Cards) > HighestPower)
                    {
                        WinnerId = Pla.UID;
                        HighestPower = Pla.GetFullPower(Pla.Cards);
                    }
                }
            }

            return WinnerId;
        }
        public ulong GetHandPower(Game.ConquerStructures.PokerCard[] hand, PokerPlayer Pl)
        {
            ulong _hand = 0;
            ulong board = 0;
            foreach (var item in hand)
            {
                _hand |= HoldemHand.Hand.CardMasksTable[item.ID];
            }
            foreach (var item in this.TableCards)
            {
                board |= HoldemHand.Hand.CardMasksTable[item.ID];
            }
            return HoldemHand.Hand.Evaluate((ulong)(board | _hand));
        }
        public bool SitIn(GameState client, byte Seat, bool Player = true)
        {
            if (Players.Count >= 1)
            {
                Start();
            }
            if (m_State == Game.Enums.GameClientEnum.Init || m_State == Game.Enums.GameClientEnum.End)
            {
                client.MessageBox(string.Format("Can't join, bad timing: {0}", m_State));
                return false;
            }
            if (CheckSeat(Seat) || !Player)
            {
                client.Entity.PokerTableUID = UID;
                PokerPlayer Pl = new PokerPlayer();
                Pl.Entity = client.Entity;
                Pl.UID = client.Entity.UID;
                Pl.TableUID = UID;
                Pl.Seat = Seat;

                if (Player)
                {
                    if (Pot > 0)
                        Pl.RoundState = Game.Enums.PokerCallTypes.Fold;
                    else
                        Pl.IsPlaying = true;
                    if (!Players.ContainsKey(Pl.UID))
                        Players.Add(Pl.UID, Pl);
                    ToLocal(this.Spawn());
                }
                else
                {
                    if (!Watchers.ContainsKey(Pl.UID))
                        Watchers.Add(Pl.UID, Pl);
                }
                return true;
            }
            return false;
        }
        #region LeaveGame
        #endregion
        public void LeaveGame(PokerPlayer p)
        {
            foreach (var other in p.MyTable.Players.Values)
            {
                if (other == null) return;
                other.Send(new MsgShowHandEnter(true) { Type = 1, TableType2 = (byte)(ShowHand ? 2 : 1), Seat = 0, UID = p.UID, State = 0, TableNumber = p.MyTable.Number }.ToArray());
            }
            if (Players.Count <= 1)
            {
                StopMoveCountDown();
                RaiseEverythingEnded();
                Program.World.DelayedTask.StartDelayedTask(() =>
                {
                    StartANewGame();
                }, CountDown);
            }
            else
            {
                ContinueBettingRound();
            }
            #region Remove
            var PokerLeaveTable = new MsgShowHandExit(true) { Type = 1, Type2 = (uint)(Crazy ? 1 : 2), UID = p.UID };
            if (Players.ContainsKey(p.UID))
            {
                p.IsPlaying = false;
                if (p.Entity != null)
                    p.Entity.PokerTableUID = 0;
                Players.Remove(p.UID);
            }
            else if (Watchers.ContainsKey(p.UID))
            {
                p.IsPlaying = false;
                if (p.Entity != null)
                    p.Entity.PokerTableUID = 0;
                Watchers.Remove(p.UID);
            }
            #endregion

            ToLocal(PokerLeaveTable.ToArray());
            ToLocal(Spawn());
            TryToBegin();
        }
        public void CheckPokerOut(PokerPlayer p)
        {
            var T = p.Entity.PokerTable;
            if (T == null) return;
            if (T.Players.ContainsKey(p.Entity.UID) && T.Pot > 1)
            {
                T.StopMoveCountDown();
                T.RemovePlayer(p.Entity.UID);
            }
            else
                T.RemovePlayer(p.Entity.UID);
        }
        public void RemovePlayer(uint Id)
        {
            var PokerLeaveTable = new MsgShowHandExit(true) { Type = 1, Type2 = (uint)(Crazy ? 1 : 2), UID = Id };
            if (Players.ContainsKey(Id))
            {
                Players[Id].IsPlaying = false;
                if (Players[Id].Entity != null)
                    Players[Id].Entity.PokerTableUID = 0;
                this.folders += Players[Id].TotalBet;
                Players.Remove(Id);
            }
            else if (Watchers.ContainsKey(Id))
            {
                Watchers[Id].IsPlaying = false;
                if (Watchers[Id].Entity != null)
                    Watchers[Id].Entity.PokerTableUID = 0;
                Watchers.Remove(Id);
            }

            ToLocal(PokerLeaveTable.ToArray());

            ToLocal(this.Spawn());
            if (Players.ContainsKey(Id))
                Players.Remove(Id);
            if (NbPlayingAndAllIn == 1 || NbPlayed >= NbPlayingAndAllIn)
                EndBettingRound();
            TryToBegin();

        }
        public void UpdateSeats(GameState client, byte NoSeat)
        {
            byte CurrentState = 1;
            if (!Players.ContainsKey(client.Entity.UID))
                if (Watchers.ContainsKey(client.Entity.UID))
                    CurrentState = (byte)Watchers[client.Entity.UID].CurrentState;

            client.Send(new MsgShowHandEnter(true) { Type = 1, TableType2 = (byte)(ShowHand ? 2 : 1), Seat = NoSeat, UID = client.Entity.UID, State = CurrentState, TableNumber = Number }.ToArray());

            foreach (var P in Players.Values)
            {
                if (P.UID == client.Entity.UID) continue;
                client.Send(new MsgShowHandEnter(true) { Type = 1, TableType2 = (byte)(ShowHand ? 2 : 1), Seat = P.Seat, UID = P.UID, State = (byte)P.CurrentState, TableNumber = Number }.ToArray());
                P.Send(new MsgShowHandEnter(true) { Type = 1, TableType2 = (byte)(ShowHand ? 2 : 1), Seat = NoSeat, UID = client.Entity.UID, State = CurrentState, TableNumber = Number }.ToArray());
            }
            foreach (var P in Watchers.Values)
            {
                if (P.UID == client.Entity.UID) continue;
                client.Send(new MsgShowHandEnter(true) { Type = 1, TableType2 = (byte)(ShowHand ? 2 : 1), Seat = P.Seat, UID = P.UID, State = (byte)P.CurrentState, TableNumber = Number }.ToArray());
                P.Send(new MsgShowHandEnter(true) { Type = 1, TableType2 = (byte)(ShowHand ? 2 : 1), Seat = NoSeat, UID = client.Entity.UID, State = CurrentState, TableNumber = Number }.ToArray());
            }
        }
        public void StartMoveCountDown(byte CountDown, uint PlayerId)
        {
            StopMoveCountDown();
            MoveCountDown = Program.World.DelayedTask.StartDelayedTask(() =>
            {
                MoveCountDownEnded(PlayerId);

            }, CountDown * 1000);
        }
        public void MoveCountDownEnded(uint PlayerId)
        {
            StopMoveCountDown();
            var PokerPlayerMove = new MsgShowHandCallAction(true)
            {
                Type = Game.Enums.PokerCallTypes.Fold,
                LastBet = 0,
                Bet = Pot,
                UID = PlayerId,
            };
            SendToAll(PokerPlayerMove);
            if (Players.ContainsKey(PlayerId))
            {
                Players[PlayerId].Cards.Clear();
                Players[PlayerId].RoundState = Game.Enums.PokerCallTypes.Fold;
                Players[PlayerId].IsPlaying = false;
            }
            ContinueBettingRound();
        }
        public static byte[] PokerRoundResult(PokerTable T, uint WinnerId, ulong MoneyWins)
        {
            PacketBuilder P = new PacketBuilder(2095, 8 + T.Players.Count * 15);
            P.Short(20);//Timer
            P.Short(T.Players.Count);
            P.Int(0);
            P.Int(0);
            P.Int(0);
            P.Long(WinnerId);

            P.Long((MoneyWins - T.Players[WinnerId].Bet));
            P.Long(0);

            foreach (PokerPlayer Pl in T.Players.Values)
            {
                try
                {
                    byte ContinuePlaying = 0;
                    if (Pl.UID == WinnerId) continue;
                    if (T.BetType == 0)
                        if (Pl.Entity.Money >= T.MinLimit * 10)
                            ContinuePlaying = 0;
                        else ContinuePlaying = 1;
                    else if (T.BetType == Game.Enums.PokerBetType.ConquerPoints)
                        if (Pl.Entity.ConquerPoints >= T.MinLimit * 10)
                            ContinuePlaying = 0;
                        else ContinuePlaying = 1;
                    if (ContinuePlaying == 0)
                        P.Int(0);
                    else
                    {
                        P.Int(1);
                        Pl.CurrentState = 2;
                    }
                    P.Int(255);
                    P.Int(0);
                    P.Long(Pl.UID);
                    P.Long((int)(0xffffffff - Pl.Bet));
                    P.Short(0xffff);
                    P.Short(0xffff);
                }
                catch
                {
                    P.Int(0);
                    P.Int(255);
                    P.Int(0);
                    P.Long(Pl.UID);
                    P.Long((int)(0xffffffff - Pl.Bet));
                    P.Short(0xffff);
                    P.Short(0xffff);
                }
            }
            return P.getFinal();
        }
        public void EndRound(uint WinnerId)
        {
            SendToAll(new MsgShowHandActivePlayer(true)
            {
                TimeDown = CountDown
            });

            StopMoveCountDown();

            ulong WinsVal = Pot - (Pot / 10);//commision

            WinnerId = GetHighestPower();

            if (Players.ContainsKey(WinnerId))
            {
                if (BetType == Game.Enums.PokerBetType.Money)
                    Players[WinnerId].Entity.Money += (uint)WinsVal;
                else if (BetType == Game.Enums.PokerBetType.ConquerPoints)
                    Players[WinnerId].Entity.ConquerPoints += (uint)WinsVal;
            }
            var PokerRoundResult = new MsgShowHandGameResult(Players.Count)
            {
                Timer = CountDown,
            };
            PokerRoundResult.Append(this, WinnerId, WinsVal);
            SendToAll(PokerRoundResult);

            Pot = 0;
            QuitPot = 0;
            #region Start new round
            if (Players.Count < 2) return;
            else
            {
                Pot = 0;
                StartRound();
            }
            #endregion
        }
        public void PlayerMove(MsgShowHandCallAction PlayeMove, uint PlayerId)
        {
            if (Pot == 0) return;
            try
            {
                if (Players.ContainsKey(PlayerId))
                {
                    var IsCurrentPlayer = CurrentPlayer.UID == PlayerId;
                    if (IsCurrentPlayer)
                        StopMoveCountDown();

                    var Player = Players[PlayerId];
                    var CallType = PlayeMove.Type;
                    Player.RoundState = CallType;

                    ulong Betting = 0;
                    var _CallAmnt = CallAmnt(Player);
                    var _MinRaiseAmnt = MinRaiseAmnt(Player) + Player.Bet;
                    var _MaxRaiseAmnt = MaxRaiseAmnt(Player);

                    switch (CallType)
                    {
                        case Game.Enums.PokerCallTypes.Check:
                            {
                                break;
                            }
                        case Game.Enums.PokerCallTypes.Fold:
                            {
                                MoveCountDownEnded(PlayerId);
                                Pot -= Player.TotalBet;
                                QuitPot += Player.TotalBet + Player.Bet;
                                break;
                            }
                        case Game.Enums.PokerCallTypes.Bet:
                            {
                                if (PlayeMove.LastBet != 0)
                                {
                                    var min = _MinRaiseAmnt;
                                    min = Math.Max(min, PlayeMove.LastBet);

                                    Betting = min - Player.Bet;
                                }
                                else
                                    Betting = _MinRaiseAmnt;

                                //if (Round > Game.Enums.RoundTypeEnum.Flop)
                                //    Betting *= 2;
                                Player.PlayMoney(Betting);
                                break;
                            }
                        case Game.Enums.PokerCallTypes.Call:
                            {
                                Betting = _CallAmnt;
                                Player.PlayMoney(Betting);
                                break;
                            }
                        case Game.Enums.PokerCallTypes.Rise:
                            {
                                if (PlayeMove.LastBet != 0)
                                {
                                    var min = _MinRaiseAmnt;
                                    min = Math.Max(min, PlayeMove.LastBet);

                                    Betting = min - Player.Bet;
                                }
                                else
                                    Betting = _MinRaiseAmnt;

                                Betting = Math.Min(Betting, _MaxRaiseAmnt);
                                Player.PlayMoney(Betting, HigherBet);

                                NbPlayed = NbAllIn;
                                break;
                            }
                        case Game.Enums.PokerCallTypes.AllIn:
                            {
                                Betting = _MaxRaiseAmnt;
                                Player.PlayMoney(Betting, HigherBet);

                                NbAllIn++;
                                if (!m_AllInCaps.Contains(Betting))
                                    m_AllInCaps.Add(Betting);

                                NbPlayed = NbAllIn;
                                break;
                            }

                        default: SendToAll(PlayeMove); break;
                    }
                    if (!Player.IsAllIn)
                        NbPlayed++;
                    PlayeMove = new MsgShowHandCallAction(true)
                    {
                        Type = CallType,
                        LastBet = Betting,
                        Bet = Player.Bet,
                        UID = PlayerId,
                    };
                    SendToAll(PlayeMove);

                    // Ok this player received enough attention !
                    if (IsCurrentPlayer)
                        ContinueBettingRound();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public void Start()
        {
            if (m_State == Game.Enums.GameClientEnum.Init)
                AdvanceToNextGameState(); //Advancing to WaitForPlayers State
        }
        public void StartANewGame()
        {
            m_State = Game.Enums.GameClientEnum.WaitForPlayers;
            NewGame = true;
            TryToBegin();
        }
        public void AdvanceToNextGameState()
        {
            try
            {
                if (m_State == Game.Enums.GameClientEnum.End)
                    return;

                m_State = (Game.Enums.GameClientEnum)(((int)m_State) + 1);

                switch (m_State)
                {
                    case Game.Enums.GameClientEnum.Init:

                        break;
                    case Game.Enums.GameClientEnum.WaitForPlayers:
                        TryToBegin();

                        break;
                    case Game.Enums.GameClientEnum.WaitForBlinds:
                        HigherBet = 0;
                        AdvanceToNextGameState(); //Advancing to Playing State

                        break;
                    case Game.Enums.GameClientEnum.Playing:
                        if (Players.Count < 1) return;
                        Round = Game.Enums.RoundTypeEnum.Preflop;
                        m_RoundState = Game.Enums.RoundStateEnum.Cards;
                        var count = CountDown;
                        if (!NewGame)
                            count = 5;


                        RoundCountDown = Program.World.DelayedTask.StartDelayedTask(StartRound, count * 1000);
                        SendToAll(new MsgShowHandActivePlayer(true)
                        {
                            TimeDown = count
                        });
                        break;
                    case Game.Enums.GameClientEnum.Showdown:
                        ShowAllCards();
                        break;
                    case Game.Enums.GameClientEnum.DecideWinners:
                        DecideWinners();
                        break;
                    case Game.Enums.GameClientEnum.DistributeMoney:
                        DistributeMoney();
                        StartANewGame();
                        break;
                    case Game.Enums.GameClientEnum.End:
                        RaiseEverythingEnded();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void AdvanceToNextRound()
        {
            if (m_State != Game.Enums.GameClientEnum.Playing)
                return;

            if (Round == Game.Enums.RoundTypeEnum.River)
                AdvanceToNextGameState(); //Advancing to Showdown State
            else
            {
                m_RoundState = Game.Enums.RoundStateEnum.Cards;
                ChangeCurrentPlayerTo(DealerPlayer);
                Round = (Game.Enums.RoundTypeEnum)(((int)Round) + 1);
                StartRound();
            }
        }
        private void AdvanceToNextRoundState()
        {
            if (m_State != Game.Enums.GameClientEnum.Playing)
                return;
            if (m_RoundState == Game.Enums.RoundStateEnum.Cumul)
                return;
            m_RoundState = (Game.Enums.RoundStateEnum)(((int)m_RoundState) + 1);
            StartRound();
        }
        public void TryToBegin()
        {
            if (Players.Count < 1) return;

            foreach (var p in Players.Values)
            {
                if (!p.Online)
                    LeaveGame(p);
                if (p.MoneySafeAmnt > 0)
                    p.IsPlaying = true;
                if (p.CurrentState == 2)
                {
                    if (!Watchers.ContainsKey(p.UID))
                        Watchers.Add(p.UID, p);
                }
            }
            foreach (var P in Watchers.Values)
            {
                if (P.CurrentState == 3)
                {
                    if (!Players.ContainsKey(P.UID))
                        Players.Add(P.UID, P);
                }
                else if (P.CurrentState == 2)
                {
                    if (Players.ContainsKey(P.UID))
                        Players.Remove(P.UID);
                }
            }

            if (NbPlaying == 0)
                RaiseEverythingEnded();
            else if (NbPlaying >= 2)
            {

                InitTable();
                AdvanceToNextGameState(); //Advancing to WaitForBlinds State
            }
            else
            {
                if (DealerPlayer != null)
                    DealerPlayer.SeatAttributes.Remove(Game.Enums.SeatAttributeEnum.Dealer);
                RaiseEverythingEnded();
            }
        }
        public void RaiseEverythingEnded()
        {
            foreach (var p in Players.Values)
            {
                p.SeatAttributes.Clear();
                p.RoundState = Game.Enums.PokerCallTypes.Fold;
                if (p.MoneySafeAmnt > 0)
                    p.IsPlaying = true;
            }
            NewGame = false;
            Pot = 0;
            QuitPot = 0;
            SendToAll(new MsgShowHandActivePlayer(true)
            {
                TimeDown = 0
            });
            StopRoundCountDown();
            var D = new Network.GamePackets.Data(true);
            D.ID = 234;
            D.UID = UID;
            D.dwParam = (uint)Pot;
            SendToAll(D.ToArray());
            ToLocal(Spawn());

        }
        public static List<Game.ConquerStructures.PokerCard> GetSortedDeck(bool crazy, bool showhand)
        {
            List<Game.ConquerStructures.PokerCard> list = new List<Game.ConquerStructures.PokerCard>();
            for (byte i = (byte)Game.Enums.PokerCardsType.Hearts; i < 4; i++)
            {
                if (showhand == true)
                {
                    for (byte j = (byte)(showhand == true ? Game.Enums.PokerCardsValue.Ten : Game.Enums.PokerCardsValue.Two); j < 13; j++)
                    {
                        list.Add(new Game.ConquerStructures.PokerCard((Game.Enums.PokerCardsType)i, (Game.Enums.PokerCardsValue)j));
                    }
                }
                else if (crazy == true)
                {
                    for (byte j = (byte)(crazy == true ? Game.Enums.PokerCardsValue.Eight : Game.Enums.PokerCardsValue.Two); j < 13; j++)
                    {
                        list.Add(new Game.ConquerStructures.PokerCard((Game.Enums.PokerCardsType)i, (Game.Enums.PokerCardsValue)j));
                    }
                }
                else
                {
                    for (byte j = (byte)(Game.Enums.PokerCardsValue.Four); j < 13; j++)
                    {
                        list.Add(new Game.ConquerStructures.PokerCard((Game.Enums.PokerCardsType)i, (Game.Enums.PokerCardsValue)j));
                    }
                }

            }
            return list;
        }
        public static int RandomWithMax(int max)
        {
            return (max > 0) ? Kernel.Random.Next(max + 1) : max;
        }
        public static Stack<Game.ConquerStructures.PokerCard> GetShuffledDeck(bool crazy, bool showhand)
        {
            Stack<Game.ConquerStructures.PokerCard> stack = new Stack<Game.ConquerStructures.PokerCard>();
            List<Game.ConquerStructures.PokerCard> sortedDeck = GetSortedDeck(crazy, showhand);
            while (sortedDeck.Count > 0)
            {
                int index = RandomWithMax(sortedDeck.Count - 1);
                stack.Push(sortedDeck[index]);
                sortedDeck.RemoveAt(index);
            }
            return stack;
        }
        public void InitTable()
        {
            foreach (var P in Players.Values)
            {
                if (Watchers.ContainsKey(P.UID))
                    Watchers.Remove(P.UID);
                P.Cards.Clear();
                P.CurrentState = 1;
                P.RoundState = 0;
                P.Bet = 0;
                P.TotalBet = 0;
                P.MyBet = 0;
                P.SeatAttributes.Clear();
            }

            m_CurrPotId = 0;
            m_Pots.Clear();
            m_Pots.Add(new MoneyPot(0));

            TableCards.Clear();

            _ShuffleDeckCards = GetShuffledDeck(Crazy, ShowHand); ;

            if (Unlimited)
            {
                SmallBlind = MinLimit / 2;
                BigBlind = MinLimit;
            }
            else
            {
                SmallBlind = MinLimit;
                BigBlind = MinLimit * 2;
            }

            Pot = 0;
            QuitPot = 0;
            NbPlayed = 0;
            NbAllIn = 0;
            m_AllInCaps.Clear();

            //var _DealerPlayer = GetHighestPower();
            //Players[_DealerPlayer].SeatAttributes.Add(Game.Enums.SeatAttributeEnum.Dealer);

            var previousDealer = DealerPlayer;


            var nextDealerSeat = GetSeatOfPlayingPlayerNextTo(previousDealer);
            nextDealerSeat.SeatAttributes.Add(Game.Enums.SeatAttributeEnum.Dealer);

            var _bigblind = GetSeatOfPlayingPlayerNextTo(DealerPlayer);
            _bigblind.SeatAttributes.Add(Game.Enums.SeatAttributeEnum.BigBlind);

            var _smallblind = GetSeatOfPlayingPlayerNextTo(_bigblind);
            _smallblind.SeatAttributes.Add(Game.Enums.SeatAttributeEnum.SmallBlind);
        }
        public void StartRound()
        {
            if (Players.Count < 1) return;
            EndLast = 0;
            if (RoundCountDown != 0)
                StopRoundCountDown();
            switch (m_RoundState)
            {
                case Game.Enums.RoundStateEnum.Cards:
                    StartCardRound();
                    break;
                case Game.Enums.RoundStateEnum.Betting:
                    StartBettingRound();
                    break;
                case Game.Enums.RoundStateEnum.Cumul:
                    StartCumulRound();
                    break;
            }
        }
        #region CardRound
        private void StartCardRound()
        {
            if (_ShuffleDeckCards.Count == 0)
            {
                TryToBegin();
                return;
            }
            switch (Round)
            {
                case Game.Enums.RoundTypeEnum.Preflop:

                    DealHole();
                    break;
                case Game.Enums.RoundTypeEnum.Flop:
                    DealFlop();
                    break;
                case Game.Enums.RoundTypeEnum.Turn:
                    DealTurn();
                    break;
                case Game.Enums.RoundTypeEnum.River:
                    DealRiver();
                    break;
            }

            AdvanceToNextRoundState(); // Advance to Betting Round State
        }
        private void Deal2Hole()
        {
            try
            {
                foreach (var p in Players.Values)
                {
                    p.Cards.AddRange(DealCards(1));
                }

                foreach (var p in Players.Values)
                    p.PlayMoney(SmallBlind);
                if (BigBlindPlayer != null)
                    BigBlindPlayer.PlayMoney(BigBlind);
                if (SmallBlindPlayer != null)
                    SmallBlindPlayer.PlayMoney(SmallBlind);
                var PokerCards1Card = new MsgShowHandDealtCard(Players.Count)
                {
                    Type = 7,
                    RoundStage = 4,
                    Delear = DealerPlayer.UID
                };
                PokerCards1Card.Append(this, 1);
                SendToAll(PokerCards1Card);
            }
            catch
            {
                TryToBegin();
            }
        }
        private void DealHole()
        {
            try
            {
                foreach (var p in Players.Values)
                {
                    p.Cards.AddRange(DealCards(1));
                }

                foreach (var p in Players.Values)
                    p.PlayMoney(SmallBlind);
                if (BigBlindPlayer != null)
                    BigBlindPlayer.PlayMoney(BigBlind);
                if (SmallBlindPlayer != null)
                    SmallBlindPlayer.PlayMoney(SmallBlind);
                var PokerCards1Card = new MsgShowHandDealtCard(Players.Count)
                {
                    Type = 7,
                    RoundStage = 4,
                    Delear = DealerPlayer.UID
                };
                PokerCards1Card.Append(this, 1);
                SendToAll(PokerCards1Card);
                //2nd Card
                foreach (var p in Players.Values)
                {
                    p.Cards.AddRange(DealCards(1));
                }

                foreach (var Pl in Players.Values)
                {
                    var PokerCards2Cards = new MsgShowHandDealtCard(Players.Count)
                    {
                        CardsCount = 2,
                    };
                    PokerCards2Cards.Append(DealerPlayer.UID, SmallBlindPlayer.UID, BigBlindPlayer.UID, Pl.Cards.ToArray());
                    foreach (var Pl2 in Players.Values)
                        PokerCards2Cards.Append(Pl2);

                    Pl.Send(PokerCards2Cards);
                }
            }
            catch
            {
                TryToBegin();
            }
        }
        private void DealFlop()
        {
            if (Players.Count < 1) return;
            if (TableCards.Count < 3)
            {
                TableCards.AddRange(DealCards(3));
                var MsgShowHandDealtCards = new MsgShowHandDealtCard(0)
                {
                    RoundStage = (ushort)Round,
                    CardsCount = 3
                };
                MsgShowHandDealtCards.Append(DealerPlayer != null ? DealerPlayer.UID : 0, SmallBlindPlayer != null ? SmallBlindPlayer.UID : 0, BigBlindPlayer != null ? BigBlindPlayer.UID : 0, TableCards.ToArray());
                SendToAll(MsgShowHandDealtCards);
            }
        }
        private void DealTurn()
        {
            if (Players.Count < 1) return;
            if (TableCards.Count < 4)
            {
                var newcards = DealCards(1);
                TableCards.AddRange(newcards);
                var MsgShowHandDealtCards = new MsgShowHandDealtCard(0)
                {
                    RoundStage = (ushort)Round,
                    CardsCount = 1
                };
                MsgShowHandDealtCards.Append(DealerPlayer != null ? DealerPlayer.UID : 0, SmallBlindPlayer != null ? SmallBlindPlayer.UID : 0, BigBlindPlayer != null ? BigBlindPlayer.UID : 0, newcards);
                SendToAll(MsgShowHandDealtCards);
            }
        }
        private void DealRiver()
        {
            if (Players.Count < 1) return;
            if (TableCards.Count < 5)
            {
                var newcards = DealCards(1);
                TableCards.AddRange(newcards);
                var MsgShowHandDealtCards = new MsgShowHandDealtCard(0)
                {
                    RoundStage = (ushort)Round,
                    CardsCount = 1
                };
                MsgShowHandDealtCards.Append(DealerPlayer != null ? DealerPlayer.UID : 0, SmallBlindPlayer != null ? SmallBlindPlayer.UID : 0, BigBlindPlayer != null ? BigBlindPlayer.UID : 0, newcards);
                SendToAll(MsgShowHandDealtCards);
            }
        }
        private Game.ConquerStructures.PokerCard[] DealCards(int count)
        {

            var set = new Game.ConquerStructures.PokerCard[count];
            for (int i = 0; i < count; i++)
            {
                set[i] = _ShuffleDeckCards.Pop();
            }
            return set;
        }
        #endregion
        #region BettingRound
        private void StartBettingRound()
        {
            ChangeCurrentPlayerTo(GetSeatOfPlayingPlayerJustBefore(SeatOfTheFirstPlayer));
            NbPlayed = 0;
            MinimumRaiseAmount = MinLimit;
            if (Round > Game.Enums.RoundTypeEnum.Flop)
                MinimumRaiseAmount *= 2;
            if (NbPlaying <= 1)
                EndBettingRound();
            else
                ContinueBettingRound();
        }
        private void ContinueBettingRound()
        {
            if (NbPlayingAndAllIn == 1 || NbPlayed >= NbPlayingAndAllIn)
                EndBettingRound();
            else
                ChooseNextPlayer();
        }
        private void EndBettingRound()
        {
            AdvanceToNextRoundState(); // Advance to Cumul Round State
        }
        private void ChooseNextPlayer()
        {
            var next = GetSeatOfPlayingPlayerNextTo(CurrentPlayer);

            ChangeCurrentPlayerTo(next);
            MoveGame(next);
            //if (!next.Online)
            //{
            //   // PlayMoney(next.Player, 0);
            //}
        }
        public void MoveGame(PokerPlayer _CurrentPlayer, Game.Enums.PokerCallTypes CallType = Game.Enums.PokerCallTypes.Fold)
        {
            if (_CurrentPlayer.UID == 0)
            {
                StartANewGame();
                return;
            }
            bool JustAllin = false;
            bool CanRaise = false;
            var minx = CallAmnt(_CurrentPlayer);
            var min = MinRaiseAmnt(_CurrentPlayer) + _CurrentPlayer.Bet;
            var max = MaxRaiseAmnt(_CurrentPlayer);
            CallType = Game.Enums.PokerCallTypes.Call;

            if (HigherBet <= _CurrentPlayer.Bet)
                CallType = Game.Enums.PokerCallTypes.Check;
            if (_CurrentPlayer.Entity != null)
            {
                if (BetType == Game.Enums.PokerBetType.Money)
                {

                    if (_CurrentPlayer.Entity.Money < HigherBet)
                        JustAllin = true;
                    else if (_CurrentPlayer.Entity.Money < minx)
                        JustAllin = true;
                    if (_CurrentPlayer.Entity.Money >= min)
                        CanRaise = true;
                }
                else if (BetType == Game.Enums.PokerBetType.ConquerPoints)
                {
                    if (_CurrentPlayer.Entity.ConquerPoints < HigherBet)
                        JustAllin = true;
                    else if (_CurrentPlayer.Entity.ConquerPoints < minx)
                        JustAllin = true;
                    if (_CurrentPlayer.Entity.ConquerPoints >= min)
                        CanRaise = true;
                }
            }
            if (CanRaise)
            {
                var calltype = Game.Enums.PokerCallTypes.Rise;
                if (_CurrentPlayer.SeatAttributes.Contains(Game.Enums.SeatAttributeEnum.BigBlind))
                {
                    if (Round > Game.Enums.RoundTypeEnum.Preflop)
                        calltype = Game.Enums.PokerCallTypes.Bet;
                }
                CallType |= calltype;
            }
            CallType |= Game.Enums.PokerCallTypes.Fold | Game.Enums.PokerCallTypes.AllIn;

            if (JustAllin)
                CallType = Game.Enums.PokerCallTypes.AllIn | Game.Enums.PokerCallTypes.Fold;
            byte CountDown = 15;
            SendToAll(new MsgShowHandActivePlayer(true) { Type = CallType, UID = _CurrentPlayer.UID, TimeDown = CountDown, MinRaiseAmount = min, MaxRaiseAmount = max });
            StartMoveCountDown(CountDown, _CurrentPlayer.UID);
        }
        #endregion
        #region CumulRound
        private void StartCumulRound()
        {

            ManagePotsRoundEnd();
            if (NbPlayingAndAllIn <= 1)
                AdvanceToNextGameState(); //Advancing to Showdown State
            else
                AdvanceToNextRound(); //Advancing to Next Round
        }
        public void ManagePotsRoundEnd()
        {
            ulong currentTaken = 0;
            m_AllInCaps.Sort();

            while (m_AllInCaps.Count > 0)
            {
                var pot = m_Pots[m_CurrPotId];
                pot.DetachAllPlayers();

                var aicf = m_AllInCaps[0];
                m_AllInCaps.RemoveAt(0);

                ulong cap = aicf - currentTaken;
                foreach (var p in Players.Values)
                    AddBet(p, pot, Math.Min(p.Bet, cap));

                currentTaken += cap;
                m_CurrPotId++;
                m_Pots.Add(new MoneyPot(m_CurrPotId));
            }

            var curPot = m_Pots[m_CurrPotId];
            curPot.DetachAllPlayers();
            foreach (var p in Players.Values)
                AddBet(p, curPot, p.Bet);

            HigherBet = 0;
        }
        private void AddBet(PokerPlayer p, MoneyPot pot, ulong bet)
        {
            p.Bet -= bet;
            pot.AddAmount(bet);
            p.TotalBet += bet;
            p.MyBet += bet;
            if (bet >= 0 && (p.RoundState != Game.Enums.PokerCallTypes.Fold || p.IsAllIn))
                pot.AttachPlayer(p);
        }
        #endregion CumulRound
        private void ShowAllCards()
        {
            var PokerShowAllCards = new MsgShowHandLayCard(Players.Count);
            PokerShowAllCards.Append(this);
            SendToAll(PokerShowAllCards);
            AdvanceToNextGameState(); //Advancing to DecideWinners State
        }
        #region DecideWinners

        private void DecideWinners()
        {
            CleanPotsForWinning();
            AdvanceToNextGameState(); //Advancing to DistributeMoney State
        }
        /// <summary>
        /// Detach all the players that are not winning this pot
        /// </summary>
        public void CleanPotsForWinning()
        {
            if (Players.Count < 1) return;
            for (var i = 0; i < m_Pots.Count; ++i)
            {
                var pot = m_Pots[i];
                uint bestHand = 0;
                var infos = new List<PokerPlayer>(pot.AttachedPlayers);

                //If there is more than one player attach to the pot, we need to choose who will split it !
                if (infos.Count > 1)
                {
                    foreach (var p in infos)
                    {
                        var handValue = EvaluateCards(p.Cards);
                        if (handValue > bestHand)
                        {
                            pot.DetachAllPlayers();
                            pot.AttachPlayer(p);
                            bestHand = handValue;
                        }
                        else if (handValue == bestHand)
                            pot.AttachPlayer(p);
                    }
                }
            }
        }
        /// <summary>
        /// Put a number on the current "Hand" of a player. The we will use that number to compare who is winning !
        /// </summary>
        /// <param name="playerCards">Player cards</param>
        /// <returns>A unsigned int that we can use to compare with another hand</returns>
        private uint EvaluateCards(ICollection<PokerCard> playerCards)
        {
            if (TableCards == null || TableCards.Count != 5 || playerCards == null || playerCards.Count != 2)
                return 0;
            var hand = new Hand(String.Join(" ", playerCards), String.Join(" ", TableCards));
            return hand.HandValue;
        }

        #endregion
        public uint EndLast = 0;
        public void DistributeMoney()
        {
            if (Players.Count < 1) return;
            Dictionary<uint, ulong> Winners = new Dictionary<uint, ulong>();

            foreach (var pot in Pots)
            {
                var players = pot.AttachedPlayers;
                if (players.Length > 0)
                {
                    ulong wonAmount = pot.Amount / (ulong)players.Length;
                    if (wonAmount > 0)
                    {
                        foreach (var p in players)
                        {
                            if (players.Length > 1)
                                wonAmount = (ulong)(pot.Amount * p.TotalBet / Pot);
                            if (!Winners.ContainsKey(p.UID))
                                Winners.Add(p.UID, 0);
                            Winners[p.UID] += wonAmount;
                        }
                    }
                }
            }
            ulong WinsVal = 0;
            uint WinnerId = 0;
            foreach (var winner in Winners)
            {
                ulong winnvalue = 0;
                if (!Players.ContainsKey(winner.Key)) continue;
                var winnerplayer = Players[winner.Key];
                foreach (var p in Players.Values)
                {
                    if (p.UID != winnerplayer.UID)
                    {
                        if (p.TotalBet >= winnerplayer.TotalBet)
                        {
                            winnvalue += winnerplayer.TotalBet;
                            var unwonAmount = p.TotalBet - winnerplayer.TotalBet;
                            if (Winners.Count > 1)
                            {
                                if (Winners.ContainsKey(p.UID))
                                {
                                    continue;
                                }
                            }
                            if (unwonAmount != 0)
                            {
                                p.UnWins += unwonAmount;
                                if (BetType == Game.Enums.PokerBetType.Money)
                                    p.Entity.Money += (uint)unwonAmount;
                                else if (BetType == Game.Enums.PokerBetType.ConquerPoints)
                                    p.Entity.ConquerPoints += (uint)unwonAmount;
                                p.TotalBet -= unwonAmount;
                            }
                        }
                        else
                            winnvalue += p.TotalBet;
                    }
                }
                ulong wonAmount = QuitPot / (ulong)Winners.Count;
                winnvalue += wonAmount;
                if (Winners.Count > 1)
                    winnvalue = winnerplayer.Wins = winner.Value;
                else
                    winnerplayer.Wins = (ulong)(winnvalue + winnerplayer.TotalBet);
                if (BetType == Game.Enums.PokerBetType.Money)
                    winnerplayer.Entity.Money += (ulong)winnerplayer.Wins;
                else if (BetType == Game.Enums.PokerBetType.ConquerPoints)
                    winnerplayer.Entity.ConquerPoints += (uint)winnerplayer.Wins;

                winnerplayer.TotalBet -= winnvalue;

                if (winnvalue > WinsVal)
                {
                    WinsVal = winnvalue;
                    WinnerId = winner.Key;
                }
            }
            //Program.AddPokerLog(this);
            var PokerRoundResult = new MsgShowHandGameResult(Players.Count)
            {
                Timer = CountDown,
            };
            PokerRoundResult.Append(this, WinnerId, WinsVal);
            SendToAll(PokerRoundResult);

            Program.World.DelayedTask.StartDelayedTask(() =>
            {
                // DistributeMoney();
                StartANewGame();
            }, CountDown);
            EndLast = 1;
        }
        public ulong folders;
    }
    #endregion
    #region MoneyPot
    public class MoneyPot
    {
        #region Fields
        private readonly List<PokerPlayer> m_AttachedPlayers = new List<PokerPlayer>();
        #endregion Fields

        #region Properties

        /// <summary>
        /// Sequence given to the actual MenyPot
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Amount of money in the Pot
        /// </summary>
        public ulong Amount { get; private set; }

        /// <summary>
        /// Number of player playing for this Pot
        /// </summary>
        public PokerPlayer[] AttachedPlayers
        {
            get { return m_AttachedPlayers.ToArray(); }
        }
        #endregion Properties

        #region Ctors & Init

        public MoneyPot(int id, ulong amount = 0)
        {
            Id = id;
            Amount = amount;
        }
        #endregion Ctors & Init

        #region Public Methods

        /// <summary>
        /// Attach a player to the MoneyPot
        /// </summary>
        public void AttachPlayer(PokerPlayer p)
        {
            m_AttachedPlayers.Add(p);
        }

        /// <summary>
        /// Detach all players from the MoneyPot
        /// </summary>
        public void DetachAllPlayers()
        {
            m_AttachedPlayers.Clear();
        }

        /// <summary>
        /// Add money to the pot !
        /// </summary>
        public void AddAmount(ulong added)
        {
            Amount += added;
        }
        #endregion Public Methods
    }
    #endregion
}
