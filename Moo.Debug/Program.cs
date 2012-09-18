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
            MooAPI.JsonAPIClient client = new MooAPI.JsonAPIClient();
            Console.WriteLine(client.GetCurrentUser(client.Login("onetwogoo", "123456")).ID);
        }
    }
}
