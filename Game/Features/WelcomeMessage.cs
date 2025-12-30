using MTA.Network.GamePackets;

namespace MTA.Game.Features {
    internal class WelcomeMessage {
        public static string Header = "";
        public static string Body1 = "";
        public static string Body2 = "";
        public static string Body3 = "";
        public static string Body4 = "";
        public static string Body5 = "";
        public static string Footer = "";


        public static void Load() {
            var reader = new Database.MySqlReader(new Database.MySqlCommand(Database.MySqlCommandType.SELECT).Select("welcome_message"));
            if (!reader.Read()) return;
            Header = reader.ReadString("header");
            Body1 = reader.ReadString("body_1");
            Body2 = reader.ReadString("body_2");
            Body3 = reader.ReadString("body_3");
            Body4 = reader.ReadString("body_4");
            Body5 = reader.ReadString("body_5");
            Footer = reader.ReadString("footer");
        }

        public static void SendToClient(Client.GameState client) {
            if (!string.IsNullOrEmpty(Header))
                client.Send(new GameUpdates(GameUpdates.Header, Header));
            if (!string.IsNullOrEmpty(Body1))
                client.Send(new GameUpdates(GameUpdates.Body, Body1));
            if (!string.IsNullOrEmpty(Body2))
                client.Send(new GameUpdates(GameUpdates.Body, Body2));
            if (!string.IsNullOrEmpty(Body3))
                client.Send(new GameUpdates(GameUpdates.Body, Body3));
            if (!string.IsNullOrEmpty(Body4))
                client.Send(new GameUpdates(GameUpdates.Body, Body4));
            if (!string.IsNullOrEmpty(Body5))
                client.Send(new GameUpdates(GameUpdates.Body, Body5));
            if (!string.IsNullOrEmpty(Footer))
                client.Send(new GameUpdates(GameUpdates.Footer, Footer));
        }
    }
}

