using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moo.Core.DB;
namespace Moo.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            MooAPI.APIClient client = new MooAPI.APIClient();
            Console.Write(client.Login("onetwogoo", "123456"));
        }
    }
}
