using Serilog;
using System;
using System.Threading;

namespace AppInsightsDemo
{
    class Program
    {
        const string INSTRUMENTATION_KEY = "<Your AI instrumentation key";
        static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
            .WriteTo
            .ApplicationInsightsTraces(INSTRUMENTATION_KEY)
            .CreateLogger();

            try
            {
                Log.Debug("Getting started");
                Log.Information("Hello {Name} from thread {ThreadId}", Environment.GetEnvironmentVariable("USERNAME"), Thread.CurrentThread.ManagedThreadId);
                Log.Warning("No coins remain at position {@Position}", new { Lat = 25, Long = 134 });

                Fail();
            }
            catch (Exception e) 
            {
                Log.Error(e, "Something went wrong");
            }

            Log.CloseAndFlush();
            Console.ReadKey();
        }

        static void Fail()
        {
            throw new DivideByZeroException();
        }

    }
}
