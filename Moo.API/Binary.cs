using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Moo.Core.Utility;
namespace Moo.API
{
    public static class Binary
    {
        class BinaryEntity : IDisposable
        {
            public Guid ID;
            public string Name;
            public FileInfo File;
            public DateTime Expires;
            public bool Renewed;
            public bool Deleted;

            ~BinaryEntity()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            void Dispose(bool disposing)
            {
                if (!Deleted)
                {
                    Deleted = true;
                    if (File.Exists)
                    {
                        File.Delete();
                    }
                }
            }
        }
        static Dictionary<Guid, BinaryEntity> byID = new Dictionary<Guid, BinaryEntity>();
        static Queue<BinaryEntity> byExpires = new Queue<BinaryEntity>();
        static Dictionary<string, BinaryEntity> byName = new Dictionary<string, BinaryEntity>();

        public static Guid Add(byte[] arr, string name = null)
        {
            lock (typeof(Binary))
            {
                if (name != null)
                {
                    if (byName.ContainsKey(name))
                    {
                        byName[name].Expires = DateTime.Now.Add(Config.BlobExpires);
                        byName[name].Renewed = true;
                        return byName[name].ID;
                    }
                }

                AutoPop();

                Guid id = Guid.NewGuid();

                string fileName = Config.TemporyFileDirectory + Path.GetRandomFileName();
                File.WriteAllBytes(fileName, new byte[0]);
                FileInfo file = new FileInfo(fileName);
                file.Attributes = FileAttributes.Temporary;
                File.WriteAllBytes(fileName, arr);

                BinaryEntity theNew = new BinaryEntity
                {
                    ID = id,
                    Name = name,
                    File = file,
                    Expires = DateTime.Now.Add(Config.BlobExpires),
                    Renewed = false,
                    Deleted = false
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

        public static void AutoPop()
        {
            while (byExpires.Count > 0)
            {
                if (byExpires.Peek().Renewed)
                {
                    byExpires.Peek().Renewed = false;
                    byExpires.Enqueue(byExpires.Dequeue());
                }
                else if (byExpires.Peek().Deleted || byExpires.Peek().Expires < DateTime.Now)
                {
                    BinaryEntity entity = byExpires.Dequeue();
                    entity.Dispose();
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
        }

        public static bool Has(Guid key)
        {
            return byID.ContainsKey(key) && !byID[key].Deleted;
        }

        public static byte[] Get(Guid key, bool delete = false)
        {
            byte[] result = File.ReadAllBytes(byID[key].File.FullName);
            if (delete)
            {
                Delete(key);
            }
            return result;
        }

        public static string GetName(Guid key)
        {
            return byID[key].Name;
        }

        public static string GetFileName(Guid key)
        {
            return byID[key].File.Name;
        }

        public static void Delete(Guid key)
        {
            byID[key].Dispose();
        }
    }
}