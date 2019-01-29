using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class ConsoleLog : ILogger
    {
        public void Error(object o)
        {
            Console.WriteLine($"Error::{o}");
        }

        public void Log(object o)
        {
            Console.WriteLine($"Log::{o}");
        }

        public void Warnning(object o)
        {
            Console.WriteLine($"Warnning::{o}");
        }
    }
}
