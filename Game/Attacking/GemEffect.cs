using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTA.ServerBase;
using MTA.Network.GamePackets;

namespace MTA.Game.Attacking
{
    public class GemEffect
    {

        public static void Effect(Entity client)
        {
            List<int> setGem = new List<int>();
            int nGem = 0;
            for (uint i = 1; i < 12; i++)
            {
                if (i != ConquerItem.Bottle)
                {
                    ConquerItem item = client.Owner.Equipment.TryGetItem(i);
                    if (item != null && item.ID != 0)
                    {
                        nGem = (int)item.SocketOne;
                        if (nGem != 0 && nGem != 255)
                            setGem.Add(nGem);
                        nGem = (int)item.SocketTwo;
                        if (nGem != 0 && nGem != 255)
                            setGem.Add(nGem);
                    }
                }
            }
            int nGems = setGem.Count;
            if (nGems <= 0)
                return;
            string strEffect;
            switch (setGem[Kernel.Random.Next(nGems)])
            {
                case 3:
                    strEffect = "phoenix";
                    break;
                case 13:
                    strEffect = "goldendragon";
                    break;
                case 23:
                    strEffect = "fastflash";
                    break;
                case 33:
                    strEffect = "rainbow";
                    break;
                case 43:
                    strEffect = "goldenkylin";
                    break;
                case 53:
                    strEffect = "purpleray";
                    break;
                case 63:
                    strEffect = "moon";
                    break;
                case 73:
                    strEffect = "recovery";
                    break;
                default:
                    return;
            }
            _String str = new _String(true);
            str.UID = client.UID;
            str.TextsCount = 1;
            str.Type = _String.Effect;
            str.Texts.Add(strEffect);
            client.Owner.SendScreen(str, true);
        }
    }
}