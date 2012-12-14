using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moo.Core.IndexAPI
{
    public class IndexItem
    {
        public int ID;
        public IEnumerable<string> Keywords;
        public string Title;
        public string Content;
    }
}
