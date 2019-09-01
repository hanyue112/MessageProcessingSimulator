using cmcMessages.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace cmcMessages
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IEnumerable<Type> clsIps = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IConsoleView)));
                IConsoleView app = (IConsoleView)Activator.CreateInstance(clsIps.First());
                app.Lanuch(args);
                Environment.Exit(0);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Application Exception:");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                Environment.Exit(-1);
            }
        }
    }
}