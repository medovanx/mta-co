namespace MTA.Game.ConquerStructures.Society
{
    public class Friend : Interfaces.IKnownPerson
    {
        public uint ID
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public bool IsOnline
        {
            get
            {
                return Kernel.GamePool.ContainsKey(ID);
            }
        }
        public Client.GameState Client
        {
            get
            {
                return Kernel.GamePool[ID];
            }
        }
        public NobilityRank NobilityRank
        {
            get
            {
                if (Kernel.GamePool.ContainsKey(ID))
                    return Kernel.GamePool[ID].Entity.NobilityRank;
                else
                    return 0;
            }
        }
        public bool IsBoy => Kernel.GamePool.TryGetValue(ID, out var value) && Constants.BodyTypes.IsBoy(value.Entity.Body);

        public string Message
        {
            get;
            set;
        }
    }
}