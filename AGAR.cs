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

        long HeaderOffset;

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

                archive.HeaderOffset = file.BaseStream.Position;
            }

            return archive;
        }

        // save this archive to disk as a WAD file
        public void Save(string wad)
        {
            // prevent overwrite of existing wad file
            if (wad == Path)
                return;

            using (BinaryWriter file = new BinaryWriter(File.Open(wad, FileMode.Create)))
            {
                // write header : AGAR
                file.Write(Encoding.ASCII.GetBytes("AGAR"));
                // write header : version numbers
                file.Write(VersionMajor);
                file.Write(VersionMinor);
                // write header : the thing that i dont know what it does
                file.Write(UInt32.MinValue); // is this a bad way to do it? maybe. do i care? not really

                // write file structure
                UInt64 currentOffset = 0;

                file.Write(Files.Count);
                for (int i = 0; i < Files.Count; i++)
                {
                    // write path length and the path itself
                    file.Write(Files[i].Path.Length);
                    file.Write(Encoding.ASCII.GetBytes(Files[i].Path));
                    file.Write(Files[i].Size);
                    file.Write(currentOffset);
                    currentOffset += Files[i].Size;
                }

                // write directory structure
                // (hardcoded for now, i just want to edit hotline miami 2 music files)
                file.Write((UInt32)2); // number of directories
                file.Write((UInt32)0); // length of first directory name (that would be the root directory)
                file.Write((UInt32)1); // entries in this directory
                file.Write((UInt32)5); // length of entry name (5, length of "Music")
                file.Write(Encoding.ASCII.GetBytes("Music")); // name of that entry
                file.Write((byte)1); // type of entry (1 specifies directory)

                file.Write((UInt32)5); // length of second directory name (5, length of "Music")
                file.Write(Encoding.ASCII.GetBytes("Music")); // name of this directory
                file.Write(Files.Count); // entries in this directory

                for (int i = 0; i < Files.Count; i++)
                {
                    string name = Files[i].Path.Substring(6); // this removes "Music/" from the path, good for this test code but NOT a permanent solution
                    file.Write(name.Length); // write file name length
                    file.Write(Encoding.ASCII.GetBytes(name)); // write file name
                    file.Write((byte)0); // type of entry (0 specifies file I THINK)
                }

                // write file data
                using (BinaryReader basewad = new BinaryReader(File.Open(Path, FileMode.Open)))
                {
                    basewad.BaseStream.Seek(HeaderOffset, SeekOrigin.Begin);
                    for (int i = 0; i < Files.Count; i++)
                    {
                        if (Files[i].FilePath == "")
                        {
                            file.Write(basewad.ReadBytes((int)Files[i].Size));
                        } else
                        {
                            using (BinaryReader customFile = new BinaryReader(File.Open(Files[i].FilePath, FileMode.Open)))
                            {
                                file.Write(customFile.ReadBytes((int)customFile.BaseStream.Length));
                            }
                        }
                    }
                }
            }
        }

        public byte[] GetFileData(int fileIndex)
        {
            using (BinaryReader file = new BinaryReader(File.Open(Path, FileMode.Open)))
            {
                file.BaseStream.Seek((long)Files[fileIndex].Position + HeaderOffset, SeekOrigin.Begin);
                return file.ReadBytes((int)Files[fileIndex].Size);
            }
        }
    }
}
