using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Moo.Core.Utility
{
    public class AutoPopDictionary<K, V>
    {
        Dictionary<K, V> dictionary=new Dictionary<K,V>();
        Queue<KeyValuePair<DateTime, K>> queue=new Queue<KeyValuePair<DateTime,K>>();

        public V this[K key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                dictionary[key] = value;
            }
        }

        public void Add(K key, V value)
        {
            dictionary.Add(key, value);
            queue.Enqueue(new KeyValuePair<DateTime, K>(DateTime.Now, key));
        }

        public bool ContainsKey(K key)
        {
            return dictionary.ContainsKey(key);
        }

        public void AutoPop(DateTime removeBefore)
        {
            while (queue.Count > 0 && queue.Peek().Key < removeBefore)
            {
                dictionary.Remove(queue.Dequeue().Value);
            }
        }
    }
}
