using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
namespace SubscriberA
{
    class Program
    {
        public static IList<string> allowableCommandLineArgs
            = new[] { "TopicA", "TopicB", "All" };
        static void Main(string[] args)
        {
            if (args.Length != 1 || !allowableCommandLineArgs.Contains(args[0]))
            {
                Console.WriteLine("Expected one argument, either " +
                                  "'TopicA', 'TopicB' or 'All'");
                Environment.Exit(-1);
            }
            string topic = args[0] == "All" ? "" : args[0];
            Console.WriteLine("Subscriber started for Topic : {0}", topic);
            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect("tcp://localhost:20045");
                subSocket.Subscribe(topic);
                Console.WriteLine("Subscriber socket connecting...");

                Dictionary<long, long> latency_counter = new Dictionary<long, long>();
                long expected_msg_index = 0;
                long msg_count = 0;
                while (true)
                {
                    msg_count++;
                    string messageTopicReceived = subSocket.ReceiveFrameString();
                    string messageReceived = subSocket.ReceiveFrameString();
                    if (messageReceived == "End")
                    {
                        break;
                    }
                    long index = long.Parse(subSocket.ReceiveFrameString());
                    var recv_time = Environment.TickCount;
                    var send_time = long.Parse(messageReceived);
                    var latency = recv_time - send_time;
                    latency_counter[latency] = latency_counter.ContainsKey(latency) ? latency_counter[latency] + 1 : 1;
                    if (index != expected_msg_index)
                    {
                        Console.WriteLine("Missed message{0}", expected_msg_index);
                    }
                    expected_msg_index = index + 1;

                    Console.WriteLine("Received message: {0} {1} {2}", messageTopicReceived, messageReceived, index);
                }
                Console.WriteLine("Latency distribution:");
                foreach (var latency in latency_counter)
                {
                    Console.WriteLine("Latency: {0}ms, Count: {1}", latency.Key, latency.Value);
                }
                Console.WriteLine("count of received messages: {0}", msg_count);
                Console.WriteLine("Subscriber finished");
            }
        }
    }
}