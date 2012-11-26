using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moo.Core.Utility;
namespace Moo.API
{
    public static class Binary
    {
        static readonly TimeSpan EXPIRES = new TimeSpan(0, 10, 0);

        class BinaryEntity
        {
            public int ID;
            public string Name;
            public byte[] Bytes;
            public DateTime Expires;
            public bool Renewed;
        }
        static Dictionary<int, BinaryEntity> byID = new Dictionary<int, BinaryEntity>();
        static Queue<BinaryEntity> byExpires = new Queue<BinaryEntity>();
        static Dictionary<string, BinaryEntity> byName = new Dictionary<string, BinaryEntity>();

        public static int Add(byte[] arr, string name = null)
        {
            lock (typeof(Binary))
            {
                if (name != null)
                {
                    if (byName.ContainsKey(name))
                    {
                        byName[name].Expires = DateTime.Now.Add(EXPIRES);
                        byName[name].Renewed = true;
                        return byName[name].ID;
                    }
                }

                //Auto Pop
                while (byExpires.Count > 0)
                {
                    if (byExpires.Peek().Renewed)
                    {
                        byExpires.Peek().Renewed = false;
                        byExpires.Enqueue(byExpires.Dequeue());
                    }
                    else if (byExpires.Peek().Expires < DateTime.Now)
                    {
                        BinaryEntity entity = byExpires.Dequeue();
                        if (entity.Name != null)
                        {
                            byName.Remove(entity.Name);
                        }
                        byID.Remove(entity.ID);
                    }
                    else
                    {
                        break;
                    }
                }

                int id = Rand.RAND.Next();
                while (byID.ContainsKey(id))
                {
                    id++;
                }

                BinaryEntity theNew = new BinaryEntity
                {
                    ID = id,
                    Name = name,
                    Bytes = arr,
                    Expires = DateTime.Now.Add(EXPIRES),
                    Renewed = false
                };
                byID.Add(id, theNew);
                if (name != null)
                {
                    byName.Add(name, theNew);
                }
                byExpires.Enqueue(theNew);
                return id;
            }
        }

        public static bool Has(int key)
        {
            return byID.ContainsKey(key);
        }

        public static byte[] Get(int key)
        {
            return byID[key].Bytes;
        }

        public static string GetName(int key)
        {
            return byID[key].Name;
        }
    }
}