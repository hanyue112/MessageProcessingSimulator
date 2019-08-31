using cmcMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace cmcMessages
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please enter a numeric argument only.");
                Console.ReadKey();
                return;
            }

            if (int.TryParse(args[0], out int i))
            {
                if (i < 1 || i > 26)
                {
                    Console.WriteLine("Please enter a numeric argument between 1-26 only.");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Please enter a numeric argument only.");
                Console.ReadKey();
                return;
            }

            IEnumerable<Type> clsIps = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICmcMessageApplication)));
            ICmcMessageApplication app = (ICmcMessageApplication)Activator.CreateInstance(clsIps.First(), i);

            try
            {
                app.Start();
                Console.Title = "Press any key to stop...";
                Console.ReadKey();
                app.Stop();
                Console.ReadKey();
                Environment.Exit(0);
            }
            catch
            {
                Console.WriteLine("Application terminated unexcepted, Press any key to continue...");
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }
    }
}