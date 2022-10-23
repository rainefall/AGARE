using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGAREditor
{
    public partial class AGARE : Form
    {
        AGAR Archive;

        ListBox filesList;

        public AGARE()
        {
            InitializeComponent();

            filesList = new ListBox();
            filesList.Name = "FilesList";
            filesList.Location = new Point(2, 32);
            filesList.Size = new Size(256, Height - 128);
            Controls.Add(filesList);
        }

        private void openWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Abstraction Games WAD (*.wad)|*.wad|All files (*.*)|*.*";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = fileDialog.FileName;

                    Archive = AGAR.Open(filePath);

                    foreach (ArchiveEntry a in Archive.Files)
                    {
                        filesList.Items.Add(a);
                    }
                }
            }
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog fileDialog = new SaveFileDialog())
            {
                fileDialog.Filter = "All files (*.*)|*.*";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = fileDialog.FileName;

                    using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.OpenOrCreate)))
                    {
                        writer.Write(Archive.GetFileData(filesList.SelectedIndex));
                    }
                }
            }
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "All files (*.*)|*.*";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = fileDialog.FileName;

                    var newFile = new ArchiveEntry();
                    newFile.Path = Prompt.ShowDialog("Enter the name of the file archive.", "Name");
                    newFile.FilePath = filePath;
                    using (BinaryReader b = new BinaryReader(File.OpenRead(filePath)))
                        newFile.Size = (ulong)b.BaseStream.Length;

                    Archive.Files.Add(newFile);
                    Archive.Files.Sort((x,y) => x.Path.CompareTo(y.Path));

                    // reset displayed files
                    filesList.Items.Clear();
                    foreach (ArchiveEntry a in Archive.Files)
                    {
                        filesList.Items.Add(a);
                    }
                }
            }
        }

        private void saveWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog fileDialog = new SaveFileDialog())
            {
                fileDialog.Filter = "Abstraction Games WAD (*.wad)|*.wad|All files (*.*)|*.*";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = fileDialog.FileName;

                    Archive.Save(filePath);
                }
            }
        }

        private void addResourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "All files (*.*)|*.*";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = fileDialog.FileName;

                    Archive.Files[filesList.SelectedIndex].FilePath = filePath;
                }
            }
        }
    }
}
