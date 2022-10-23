using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGAREditor
{
    public class ArchiveEntry
    {
        public string Path;
        public string Name;
        public string Folder;

        public UInt64 Size;
        public UInt64 Position;

        public string FilePath = "";

        override public string ToString()
        {
            return Path;
        }
    }
}
