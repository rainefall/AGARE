using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGAREditor
{
    // AGAR stands for "Abstraction Games ARchive"???? Maybe???? I dont know????
    public class AGAR
    {
        UInt32 VersionMajor;
        UInt32 VersionMinor;
        UInt32 FileCount;
        string Path;

        public List<ArchiveEntry> Files;

        public AGAR(string p)
        {
            Path = p;
            Files = new List<ArchiveEntry>();
        }

        // open a WAD file for editing
        public static AGAR Open(string wad)
        {
            // create archive
            AGAR archive = new AGAR(wad);

            using (BinaryReader file = new BinaryReader(File.Open(wad, FileMode.Open)))
            {
                // check if file contains AGAR in the header, if so it's most likely a valid WAD
                if (Encoding.ASCII.GetString(file.ReadBytes(4)) != "AGAR")
                {
                    MessageBox.Show("This is either a WAD file from a non supported game, or it is not a valid Abstraction Games WAD file.", "Error", MessageBoxButtons.OK);
                    return null;
                }

                // read version numbers
                archive.VersionMajor = file.ReadUInt32();
                archive.VersionMinor = file.ReadUInt32();

                // i don't actually know what these next 4 bytes do so i'm leaving them for now
                file.ReadBytes(4);

                // read number of files
                archive.FileCount = file.ReadUInt32();

                for (int i = 0; i < (int)archive.FileCount; i++ )
                {
                    // read length of path string
                    var stringLength = file.ReadInt32();

                    // read file entry
                    archive.Files.Add(new ArchiveEntry());
                    archive.Files[i].Path = Encoding.ASCII.GetString(file.ReadBytes(stringLength));
                    archive.Files[i].Size = file.ReadUInt64();
                    archive.Files[i].Position = file.ReadUInt64();
                }

                // read directory structure
                UInt32 directoryCount = file.ReadUInt32();
                for (int i = 0; i < (int)directoryCount; i++)
                {
                    // read length of directory name
                    UInt32 directoryNameLength = file.ReadUInt32();

                    // check if directory name's length isn't zero
                    // this is because the root directory does not have a name
                    if (directoryNameLength != 0)
                    {
                        string directoryName = Encoding.ASCII.GetString(file.ReadBytes((int)directoryNameLength));
                    }

                    // parse directory
                    UInt32 directoryEntries = file.ReadUInt32();
                    for (int j = 0; j < (int)directoryEntries; j++)
                    {
                        // read entry name
                        UInt32 entryNameLength = file.ReadUInt32();
                        string entryName = Encoding.ASCII.GetString(file.ReadBytes((int)entryNameLength));

                        // read entry type (dont know what these are yet)
                        byte type = file.ReadByte();
                    }
                }
            }

            return archive;
        }

        // save this archive to disk as a WAD file
        public void Save()
        {

        }

        public byte[] GetFileData(int fileIndex)
        {
            using (BinaryReader file = new BinaryReader(File.Open(Path, FileMode.Open)))
            {
                file.BaseStream.Seek((long)Files[fileIndex].Position, SeekOrigin.Begin);
                return file.ReadBytes((int)Files[fileIndex].Size);
            }
        }
    }
}
