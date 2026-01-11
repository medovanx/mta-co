namespace MTA.Game.Constants {
    /// <summary>
    ///     Packet IDs for network communication, mapping packet numbers to their message names.
    /// </summary>
    public enum Packets : ushort {
        /// <summary>Guild member donation</summary>
        MsgSynpOffer = 1058,

        /// <summary>Guild minimum donation requirements</summary>
        MsgDutyMinContri = 1061,

        /// <summary>Guild and guild member details</summary>
        MsgSyndicateAttributeInfo = 1106,

        /// <summary>Guild action (command) request</summary>
        MsgSyndicate = 1107,

        /// <summary>Used to send flowers</summary>
        MsgFlower = 1150,

        /// <summary>Used to display flower rank</summary>
        MsgRank = 1151,

        /// <summary>Used to interact with a NPC and contains multiple Dialog Action types</summary>
        MsgNpc = 2031,

        /// <summary>Used to interact with a NPC and contains multiple Dialog Action types</summary>
        MsgTaskDialog = 2032,

        /// <summary>Guild donation rankings</summary>
        MsgFactionRankInfo = 2101,

        /// <summary>Guild member list</summary>
        MsgSynMemberList = 2102,

        /// <summary>Guild arsenal tab info</summary>
        MsgTotemPoleInfo = 2201,

        /// <summary>Guild arsenal weapons view</summary>
        MsgWeaponsInfo = 2202,

        /// <summary>Guild arsenal totem pole</summary>
        MsgTotemPole = 2203,

        /// <summary>Hmm</summary>
        MsgUnknown = 2204,

        /// <summary>Guild recruitment advertising</summary>
        MsgSynRecruitAdvertising = 2225,

        /// <summary>Guild recruitment advertising list</summary>
        MsgSynRecruitAdvertisingList = 2226,

        /// <summary>Guild recruitment advertising options</summary>
        MsgSynRecruitAdvertisingOpt = 2227,

        /// <summary> NPC spawn information </summary>
        MsgNpcInfo = 2030
    }
}