using System.Text.Json;
using SuperSimpleTcp;

namespace Chatik.Server
{
    public record Message(string date, string user, string text);
    internal class Program
    {
        public static List<Message> history;
        public const int MAX_HISTORY_SIZE = 50;
        static void Main(string[] args)
        {
            Dictionary<string, string> ipName = new();
            history = new();
            SimpleTcpServer s = new("0.0.0.0:286");
            s.Start();
            Console.WriteLine("started");
            void AddHistory(Message msg)
            {
                history.Insert(0, msg);
                if(history.Count >= MAX_HISTORY_SIZE)
                history.RemoveRange(MAX_HISTORY_SIZE-1, 1);
            }
            void SendAll(string name, string content)
            {
                try
                {
                    AddHistory(new(DateTime.Now.ToString("HH:mm:ss"), name, content));
                    s.GetClients().ToList()
                        .ForEach(ip => s.Send(ip, $"{{\"user\":\"{name}\",\"text\":\"{content}\"}}"));
                }
                catch { }
            }
            s.Events.ClientDisconnected += (ss, e) =>
            {
                if (s.Connections != 0)
                    SendAll("System", $"User {ipName[e.IpPort]} disconnected");
                Console.WriteLine($"-{ipName[e.IpPort]}");
                ipName.Remove(e.IpPort);
            };
            s.Events.DataReceived += (ss, e) =>
            {
                string content = System.Text.Encoding.Default.GetString(e.Data!);
                if (content.Length == 0) return;
                if (content[0] == '+')
                {
                    ipName.Add(e.IpPort, content.Remove(0, 1));
                    SendAll("System", $"User {content.Remove(0, 1)} joined the chat.");
                }
                else if (content[0] == 'H')
                {
                    history.Reverse();
                    s.Send(e.IpPort, "H" + Newtonsoft.Json.JsonConvert.SerializeObject(history));
                    history.Reverse();
                }
                else SendAll(ipName[e.IpPort], content.Remove(0, 1));
                Console.WriteLine(content);
            };
            new Thread(() =>
            {
                while (true)
                {
                    if (s.Connections == 0) continue;
                    var sysMsg = Console.ReadLine();
                    SendAll("System", sysMsg);
                }
            }).Start();
        }
    }
}
