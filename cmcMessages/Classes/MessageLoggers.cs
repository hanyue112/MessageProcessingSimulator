using cmcMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.IO;
using System.Diagnostics;

namespace cmcMessages.Classes
{
    class MessageLoggers : IMessageLoggers
    {
        private readonly BufferBlock<CmcMessagePayload>[] _logsbuffer;
        private readonly Action _notifyExisting, _notifyTerminate;
        private readonly List<IMessageFactory> _glist;
        private readonly string root = @"..\logs\";
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _ct;
        private const int _rndLow = 10;
        private const int _rndHigh = 20;
        private const int _sleepDuration = 100;

        public MessageLoggers(List<IMessageFactory> glist, Action notifyExisting, Action notifyTerminate)
        {
            _logsbuffer = new BufferBlock<CmcMessagePayload>[glist.Count];
            _notifyExisting = notifyExisting;
            _notifyTerminate = notifyTerminate;

            for (int i = 0; i < glist.Count; i++)
            {
                Trace.WriteLine($"Initializing and allocating memory for logger {Number2String(i + 1, true)}");
                _logsbuffer[i] = new BufferBlock<CmcMessagePayload>();
            }

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            else
            {
                foreach (FileInfo f in new DirectoryInfo(root).EnumerateFiles())
                {
                    f.Delete();
                }
            }
            _glist = glist;

            _cts = new CancellationTokenSource();
            _ct = _cts.Token;
        }

        public void OnMessageDelivered(CmcMessagePayload p)
        {
            _logsbuffer[char.ToUpper(p.MessageType) - 65].Post(p);
        }

        public void Start()
        {
            foreach (var g in _glist)
            {
                Task.Run(() =>
                     {
                         try
                         {
                             Trace.WriteLine($"Messages logger thread of {g.GetName()} started.");

                             string path = $@"{root}{g.GetName()}.txt";
                             Random random = new Random();

                             while (!_ct.IsCancellationRequested)
                             {
                                 if (_logsbuffer[_glist.IndexOf(g)].TryReceive(out CmcMessagePayload p))
                                 {
                                     if (p.SeqNum == 0)
                                     {
                                         break;
                                     }
                                     using (StreamWriter sw = File.AppendText(path))
                                     {
                                         sw.WriteLine($"{p.SeqNum}: {p.DatetimeCreated.ToString()}");
                                     }
                                     Task.Delay(random.Next(_rndLow, _rndHigh)).Wait();
                                 }
                                 else
                                 {
                                     Task.Delay(_sleepDuration).Wait();
                                 }
                             }
                             _logsbuffer[_glist.IndexOf(g)].Complete();

                             if (_notifyExisting != null)
                             {
                                 _notifyExisting.Invoke();
                             }
                         }
                         catch (Exception ex)
                         {
                             Trace.WriteLine($"Exception occurred {ex.Message}, Logger(s) terminating.");
                             _cts.Cancel();

                             if (_notifyTerminate != null)
                             {
                                 _notifyTerminate.Invoke();
                             }
                         }
                         finally
                         {
                             Trace.WriteLine($"Logger of {g.GetName()} stopped.");
                         }
                     }, _ct);
            }
        }

        public void Terminate()
        {
            _cts.Cancel();
        }

        private char Number2String(int number, bool isCaps)
        {
            return (char)((isCaps ? 65 : 97) + (number - 1));
        }
    }
}