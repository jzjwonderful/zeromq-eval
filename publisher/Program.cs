using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var pubSocket = new PublisherSocket())
            {
                Console.WriteLine("Publisher socket binding...");
                pubSocket.Options.SendHighWatermark = 1000;
                pubSocket.Bind("tcp://*:20045");
                for (var i = 0; i < 100; i++)
                {
                    // 
                    string msg = string.Format("{0}",Environment.TickCount);
                    pubSocket.SendMoreFrame("TopicA").SendMoreFrame(msg).SendFrame(i.ToString());
                    Thread.Sleep(10);
                }

                pubSocket.SendMoreFrame("TopicA").SendMoreFrame("End").SendFrame("End");
                Thread.Sleep(2000);
            }

            
        }
    }
}