using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
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
        private readonly Dictionary<(string, DnsType), List<ResourceRecord>> Cache = new Dictionary<(string, DnsType), List<ResourceRecord>>();
        private readonly UdpClient listener;
        private readonly UdpClient sender;
        private bool run;
        private readonly Serializer serializer = new Serializer();

        public DnsServer(int port)
        {
            listener = new UdpClient(53);
            sender = new UdpClient(port);
            var timer = new Timer(100);
            //  timer.Elapsed += (obj, eleArgs) => Del();
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
                    await sender.SendAsync(stream.ToArray(), (int)stream.Length, google);
                }
                else
                {
                    SendAnswer(m.Id, m.Questions, d.RemoteEndPoint);
                }
            }
        }

        private void SendAnswer(ushort id, List<Question> questions, IPEndPoint endpoint)
        {
            if (Cache != null)
            {
                var answers = new List<ResourceRecord>();
                foreach (var question in questions)
                {
                    foreach (var answ in Cache[(question.Name.ToString(), question.Type)])
                    {
                        answers.Add(answ);
                    }
                }
                var m = new Message()
                {
                    QR = true,
                    Id = id,
                    Answers = answers
                };
                var value = m.ToByteArray();
                Console.WriteLine(answers.Count + " answers are sanded to remote host " + endpoint.Address);
                sender.SendAsync(value, value.Length, endpoint);
            }

        }

        private IEnumerable<ResourceRecord> GetAllRecords()
        {
            foreach (var recs in Cache.Values)
            {
                foreach (var rec in recs)
                {
                    yield return rec;
                }

            }
        }
        public void Stop()
        {
            if (!run)
                return;
            run = false;
            serializer.Save(GetAllRecords().ToArray());
        }

        private bool IsContained(Message msg)
        {
            var remove = new List<Question>();
            lock (Cache)
            {
                foreach (var question in msg.Questions)
                    if (!Cache.ContainsKey((question.Name.ToString(), question.Type)))
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
            lock (Cache)
            {
                var name = (record.Name.ToString(), record.Type);
                if (!Cache.ContainsKey(name))
                    Cache[name] = new List<ResourceRecord>();

                if (!Cache[name].Contains(record))
                {
                    Console.WriteLine(record + " Added");
                    Cache[name].Add(record);
                }
            }
        }

        public async void Del()
        {
            await Task.Run(() =>
            {
                var expired = new List<ResourceRecord>();
                lock (Cache)
                {
                    foreach (var info in Cache.Values)
                    {
                        foreach (var rec in info)
                        {
                            if (rec.IsExpired())
                                expired.Add(rec);

                            foreach (var recExp in expired)
                            {
                                var recName = (recExp.Name.ToString(), recExp.Type);
                                Cache[recName].Remove(rec);
                                if (Cache[recName].Count == 0)
                                    Cache.Remove(recName);
                            }

                        }
                    }
                }
            });
        }
    }
}