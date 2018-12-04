using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener();

            //TODO: Add some actual data handling, without ReadLine program just closes since nothing runs on main thread whatsoever.
            while (true)
            {
                Thread.Sleep(10000);
            }
        }

    }
}
