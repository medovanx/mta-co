namespace MTA.Game.ConquerStructures.Society
{
    public class Enemy : Interfaces.IKnownPerson
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
        public bool IsBoy => Kernel.GamePool.ContainsKey(ID) && Constants.BodyTypes.IsBoy(Kernel.GamePool[ID].Entity.Body);

        public Client.GameState Client
        {
            get
            {
                return Kernel.GamePool[ID];
            }
        }
    }
}