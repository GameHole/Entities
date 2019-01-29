using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public interface ILogger
    {
        void Log(object o);
        void Warnning(object o);
        void Error(object o);
    }
   public static class DebugUtility
    {
        private static ILogger logger;
        public static void Provide(ILogger log)
        {
            logger = log;
        }
        public static void Log(object o)
        {
            logger?.Log(o);
        }
        public static void Warnning(object o)
        {
            logger?.Warnning(o);
        }
        public static void Error(object o)
        {
            logger?.Error(o);
        }
    }
}
