﻿using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace FileTypes.PackFiles.Models
{

    public class PackFileContainer
    {
        public string Name { get; set; }

        public PFHeader Header { get; set; }
        public bool IsCaPackFile { get; set; } = false;

        public Dictionary<string, IPackFile> FileList { get; set; } = new Dictionary<string, IPackFile>();


        public PackFileContainer(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name} - {Header?.LoadOrder}";
        }

        public PackFileContainer(string packFileSystemPath, BinaryReader reader)
        {
            Name = Path.GetFileNameWithoutExtension(packFileSystemPath);
            Header = new PFHeader(reader);

            FileList = new Dictionary<string, IPackFile>(Header.FileCount);

            long offset = Header.DataStart;
            for (int i = 0; i < Header.FileCount; i++)
            {
                uint size = reader.ReadUInt32();

                if (Header.HasAdditionalInfo)
                    reader.ReadUInt32();

                byte isCompressed = 0;
                if (Header.Version == "PFH5")
                    isCompressed = reader.ReadByte();   // For warhammer 2, terrain files are compressed

                string packedFileName = IOFunctions.TheadUnsafeReadZeroTerminatedAscii(reader);

                var packFileName = Path.GetFileName(packedFileName);
                var fileContent = new PackFile(packFileName, new PackedFileSource(packFileSystemPath, offset, size));

                //var d = fileContent.DataSource.ReadData();
                //var k = reader.ReadByte();
                FileList.Add(packedFileName, fileContent);
                offset += size;
            }
        }


        public void MergePackFileContainer(PackFileContainer other)
        {
            foreach (var item in other.FileList)
                FileList[item.Key] = item.Value;
            return;
        }

        internal void SaveToByteArray(BinaryWriter writer)
        {
            long fileNamesOffset = 0;
            foreach (var file in FileList)
            {
                if (Header.Version == "PFH5")
                    fileNamesOffset += 1;
                if (Header.HasAdditionalInfo)
                    fileNamesOffset += 4;
                fileNamesOffset += 4 + ((file.Key.Length + 1));    // Size + filename with zero terminator
            }

            Header.Save(FileList.Count(), (int)fileNamesOffset, writer);

            // Save all the files
            var sortedFiles = FileList.OrderBy(x => x.Key);
            foreach (var file in sortedFiles)
            {
                writer.Write((int)(file.Value as PackFile).DataSource.Size);
                if (Header.HasAdditionalInfo)
                    writer.Write((int)0);   // timestamp

                if (Header.Version == "PFH5")
                    writer.Write((byte)0);  // Compression

                foreach (byte c in file.Key)
                    writer.Write(c);
                writer.Write((byte)0);
            }


            // Write the files
            foreach (var file in sortedFiles)
            {
                var data = (file.Value as PackFile).DataSource.ReadData();
                writer.Write(data);
            }
        }
    }
}