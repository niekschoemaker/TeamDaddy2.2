using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using File = System.IO.File;

namespace unwdmi.Web
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 30; i++)
            {
                using (var file = File.OpenRead("Daddy-2019-1-28-14-23.pb"))
                {
                    var buffer = new BufferedStream(file, 2048);
                    while (true)
                    {
                        try
                        {
                            List<Task> taskList = new List<Task>();
                            for (int j = 0; j < 16; j++)
                                taskList.Add(Task.Run(() => Protobuf.Measurement.Parser.ParseDelimitedFrom(buffer)));
                            Task.WaitAll(taskList.ToArray());
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
            Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }
    }
}
