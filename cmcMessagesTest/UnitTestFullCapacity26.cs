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
    public class C_UnitTestFullCapacity26
    {
        [TestMethod]
        public void Thread26()
        {
            const int _theradNum = 26;
            const int _runTime = 1000 * 30;
            const string _logsRoot = @"..\logs\";
            const string _gensRoot = @"..\gens\";
            const int _maxPendingMins = 5;

            ICmcMessageApplication app = new CmcMessageApplication(_theradNum);

            List<char> list = new List<char>();

            foreach (var index in Enumerable.Range(1, _theradNum))
            {
                list.Add((char)(64 + index));
            }

            app.Start();
            Task.Delay(_runTime).Wait();
            app.Stop();

            int max = 0;
            while (app.IsComplete() == false && max <= _maxPendingMins * 60 * 1000)
            {
                max += 1000;
                Task.Delay(1000).Wait();
            }

            Assert.IsTrue(max <= _maxPendingMins * 60 * 1000);

            foreach (var c in list)
            {
                int lineCountLogs = File.ReadLines($@"{_logsRoot}{c}.txt").Count();
                int lineCountGens = File.ReadLines($@"{_gensRoot}{c}.txt").Count();
                Assert.AreEqual(lineCountLogs, lineCountGens - 1);
            }
        }
    }
}
