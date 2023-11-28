using System.Xml.Linq;
using System;
using System.Text;
using System.Threading.Channels;
using Newtonsoft.Json;
using SuperSimpleTcp;

namespace Chatik.TestClient
{
    internal class Chat
    {
        public static SimpleTcpClient tcp;
        static void Main(string[] args)
        {
            Chat.tcp = new("127.0.0.1:286");
            Chat.tcp.Events.Connected += (ss, ee) => Console.WriteLine("Connected");
            Chat.tcp.Events.Disconnected += (ss, ee) => Console.WriteLine("Disconnected");
            Chat.tcp.Connect();
            Chat.tcp.Send($"+TestClient");
            tcp.Events.DataReceived += (s, e) =>
            {
                var ssss = Encoding.UTF8.GetString(e.Data);
                dynamic json = JsonConvert.DeserializeObject(ssss);
                string message = $"[{DateTime.Now.ToString("HH:mm:ss")}] {json.user} > {json.text}";
                Console.WriteLine(message);
            };
            while (true)
            {
                var msg = Console.ReadLine();
                tcp.Send($"M{msg}");
            }
        }
    }
}
