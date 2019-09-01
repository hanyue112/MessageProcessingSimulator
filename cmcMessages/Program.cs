using cmcMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace cmcMessages
{
    class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;

        static void Main(string[] args)
        {
            Trace.Listeners.Clear();

            FileStream fs = new FileStream(@"..\AppTrace.log", FileMode.Append);
            TextWriterTraceListener fileTrace = new TextWriterTraceListener(fs)
            {
                Name = "FileLogger",
                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
            };

            TextWriterTraceListener consoleTrace = new TextWriterTraceListener(Console.Out)
            {
                Name = "ConsoleLogger",
                TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
            };

            Trace.Listeners.Add(fileTrace);
            Trace.Listeners.Add(consoleTrace);
            Trace.AutoFlush = true;
            Trace.WriteLine($"{Environment.NewLine}App starting...........{DateTime.Now}");

            if (args.Length != 1)
            {
                Trace.WriteLine("Please enter a numeric argument only.");
                Console.ReadKey(true);
                return;
            }

            if (int.TryParse(args[0], out int i))
            {
                if (i < 1 || i > 26)
                {
                    Trace.WriteLine("Please enter a numeric argument between 1-26 only.");
                    Console.ReadKey(true);
                    return;
                }
            }
            else
            {
                Trace.WriteLine("Please enter a numeric argument only.");
                Console.ReadKey(true);
                return;
            }

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

            IEnumerable<Type> clsIps = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICmcMessageApplication)));
            ICmcMessageApplication app = (ICmcMessageApplication)Activator.CreateInstance(clsIps.First(), i);

            try
            {
                app.Start();
                Console.Title = "Press any key to stop...";
                Console.ReadKey(true);
                app.Stop();
                Console.ReadKey(true);
                Environment.Exit(0);
            }
            catch
            {
                Trace.WriteLine("Application terminated unexcepted, Press any key to continue...");
                Console.ReadKey(true);
                Environment.Exit(-1);
            }
            finally
            {
                fs.Close();
            }
        }
    }
}