using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using DNS.Ser;
using Makaretu.Dns;

namespace DNS
{
    internal class DnsServer
    {
        private readonly IPEndPoint google = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);

        private readonly HashSet<ResourceRecord> info =
            new HashSet<ResourceRecord>();

        private readonly UdpClient listener;
        private readonly UdpClient sender;
        private readonly HashSet<(string, DnsType)> container = new HashSet<(string, DnsType)>();
        private bool run;
        private readonly Serializer serializer = new Serializer();

        public DnsServer(int port)
        {
            listener = new UdpClient(53);
            sender = new UdpClient(port);
            var timer = new Timer(100);
            timer.Elapsed += (obj, eleArgs) => Del();
            timer.Start();
            serializer.Load(this);
            run = true;
            Listen(sender);
            Listen(listener);
        }

        private async void Listen(UdpClient client)
        {
            while (run)
            {
                var d = await client.ReceiveAsync();
                var s = new MemoryStream(d.Buffer);
                var m = new Message();
                m.Read(s);
                if (m.IsResponse)
                {
                    AddAnswers(m);
                }
                else if (!IsContained(m))
                {
                    var by = m.ToByteArray();
                    var stream = new MemoryStream();
                    stream.Write(by, 0, by.Length);
                    await sender.SendAsync(stream.ToArray(), (int) stream.Length, google);
                }
            }
        }

        public void Stop()
        {
            if (!run)
                return;
            run = false;
            serializer.Save(info);
        }

        public bool IsContained(Message msg)
        {
            var remove = new List<Question>();
            lock (info)
            {
                foreach (var question in msg.Questions)
                    if (!container.Contains((question.Name.ToString(), question.Type)))
                        return false;
            }

            return true;
        }


        public void AddAnswers(Message msg)
        {
            foreach (var answer in msg.Answers) Add(answer);
        }

        public void Add(ResourceRecord record)
        {
            lock (info)
            {
                if (!info.Contains(record))
                {
                    info.Add(record);
                    Console.WriteLine(record.ToString() + " в кеше");
                    container.Add((record.Name.ToString(), record.Type));
                }
            }
        }

        public async void Del()
        {
            await Task.Run(() =>
            {
                var expired = new List<ResourceRecord>();
                lock (info)
                {
                    foreach (var info in info)
                        if (info.IsExpired())
                            expired.Add(info);

                    foreach (var info in expired) this.info.Remove(info);
                }
            });
        }
    }
}