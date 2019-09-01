using cmcMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace cmcMessages.Classes
{
    public class CmcMessageApplication : ICmcMessageApplication
    {
        private readonly List<IMessageFactory> _generators;
        private readonly IMessageLoggers _messageLoggers;
        private readonly BufferBlock<CmcMessagePayload> _bufferFIFO;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _ct;
        private readonly Action _notifyExitingHwnd, _notifyTerminateHwnd;
        private volatile int _numOfGenerators = 0;
        private readonly object num_Lock = new object();
        private const int _rndLow = 5;
        private const int _rndHigh = 10;
        private const int _sizePerGenerator = 100;
        private const int _poolMultiplier = 3;
        private const int _sleepDuration = 100;
        private readonly string root = @"..\gens\";
        private volatile bool _isComplete;

        public CmcMessageApplication(int numOfGenerators)
        {
            if (numOfGenerators < 1 || numOfGenerators > 26)
            {
                throw new InvalidOperationException("numOfGenerators must between 1 to 26");
            }

            Trace.WriteLine($"Parameter of the numner of generator(s) = {numOfGenerators}");
            Trace.WriteLine($"Initializing threads pool, pool size is {numOfGenerators * _poolMultiplier}");

            ThreadPool.SetMinThreads(numOfGenerators * _poolMultiplier, numOfGenerators * _poolMultiplier);

            _numOfGenerators = numOfGenerators;
            _notifyExitingHwnd = NotifyExiting;
            _notifyTerminateHwnd = NotifyTerminate;

            _generators = new List<IMessageFactory>();
            IEnumerable<Type> clsIps = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IMessageFactory)));
            _generators.AddRange(Enumerable.Range(1, numOfGenerators).Select((g, index) => (IMessageFactory)Activator.CreateInstance(clsIps.First(), index + 1)).ToList());

            clsIps = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IMessageLoggers)));
            _messageLoggers = (IMessageLoggers)Activator.CreateInstance(clsIps.First(), _generators, _notifyExitingHwnd, _notifyTerminateHwnd);

            _bufferFIFO = new BufferBlock<CmcMessagePayload>(new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = _sizePerGenerator * numOfGenerators,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            });

            _cts = new CancellationTokenSource();
            _ct = _cts.Token;

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
        }

        public void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    _messageLoggers.Start();

                    Trace.WriteLine($"Single entry point consumer Thread started.");

                    while (_numOfGenerators > 0)
                    {
                        if (_bufferFIFO.TryReceive(out CmcMessagePayload p))
                        {
                            using (StreamWriter sw = File.AppendText($@"{root}{p.MessageType}.txt"))
                            {
                                sw.WriteLine($"{p.SeqNum}: {p.DatetimeCreated.ToString()}");
                            }
                            _messageLoggers.OnMessageDelivered(p);
                        }
                        else
                        {
                            Task.Delay(_sleepDuration).Wait();
                        }
                    }
                    _bufferFIFO.Complete();
                    _isComplete = true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception occurred {ex.Message}, generators terminating.");
                    _cts.Cancel();

                    _messageLoggers.Terminate();
                    Trace.WriteLine($"Exception occurred {ex.Message}, consumer terminated.");
                }
                finally
                {
                    Trace.WriteLine($"Message queue closed. Press any key to exit...");
                    Console.Title = "Press any key to exit...";
                }
            });

            foreach (var generator in _generators)
            {
                Task.Run(() =>
                {
                    try
                    {
                        Trace.WriteLine($"Message generator thread {generator.GetName()} started.");

                        Random random = new Random();
                        while (!_ct.IsCancellationRequested)
                        {
                            _bufferFIFO.Post(generator.Next());
                            Task.Delay(random.Next(_rndLow, _rndHigh)).Wait();
                        }

                        _bufferFIFO.Post(generator.FullStop());
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Exception occurred {ex.Message}, terminating.");
                        _cts.Cancel();
                        _messageLoggers.Terminate();
                    }
                    finally
                    {
                        Trace.WriteLine($"Generator {generator.GetName()} stopped.");
                    }
                }, _ct);
            }
        }

        public void Stop()
        {
            if (_cts.IsCancellationRequested)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Trace.WriteLine($"Exception(s) occurred, Press any key to exit...");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Trace.WriteLine("Application shutting down. Please wait for all data completing the processing, early close of this application will cause inconsistency.");
                Console.ResetColor();
                Console.Title = "Application shutting down, DO NOT CLOSE THIS CONSOLE!";
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        public void NotifyExiting()
        {
            lock (num_Lock)
            {
                _numOfGenerators--;
            }
        }

        public void NotifyTerminate()
        {
            _numOfGenerators = 0;
            _cts.Cancel();
        }

        public bool IsComplete()
        {
            return _isComplete;
        }
    }
}