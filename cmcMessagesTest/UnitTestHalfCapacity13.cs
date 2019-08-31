using cmcMessages.Classes;
using cmcMessages.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cmcMessagesTest
{
    [TestClass]
    public class UnitTestHalfCapacity13
    {
        [TestMethod]
        public void Thread13()
        {
            const int _theradNum = 13;
            const int _runTime = 1000 * 30;
            const string _logsRoot = @"..\logs\";
            const string _gensRoot = @"..\gens\";
            
            ICmcMessageApplication app = new CmcMessageApplication(_theradNum);

            List<char> list = new List<char>();

            foreach (var index in Enumerable.Range(1, _theradNum))
            {
                list.Add((char)(64 + index));
            }

            app.Start();
            Task.Delay(_runTime).Wait();
            app.Stop();

            while (app.IsComplete() == false)
            {
                Task.Delay(1000).Wait();
            }

            foreach (var c in list)
            {
                int lineCountLogs = File.ReadLines($@"{_logsRoot}{c}.txt").Count();
                int lineCountGens = File.ReadLines($@"{_gensRoot}{c}.txt").Count();
                Assert.AreEqual(lineCountLogs, lineCountGens - 1);
            }
        }
    }
}
