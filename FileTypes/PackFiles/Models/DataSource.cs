﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileTypes.PackFiles.Models
{
    public interface IDataSource
    {
        long Size{get;}
        byte[] ReadData();
    }

    public class FileSystemSource : IDataSource
    {
        public long Size { get; private set; }

        protected string filepath;
        public FileSystemSource(string filepath)
            : base()
        {
            Size = new FileInfo(filepath).Length;
            this.filepath = filepath;
        }
        public byte[] ReadData()
        {
            return File.ReadAllBytes(filepath);
        }
    }

    public class MemorySource : IDataSource
    {
        public long Size { get; private set; }

        private byte[] data;
        public MemorySource(byte[] data)
        {
            Size = data.Length;
            this.data = data;
        }
        public byte[] ReadData()
        {
            return data;
        }

        public static MemorySource FromFile(string path)
        {
            return new MemorySource(File.ReadAllBytes(path));
        }
    }

    public class PackedFileSource : IDataSource
    {
        public long Size { get; private set; }
        private string filepath;
        public long Offset
        {
            get;
            private set;
        }
        public PackedFileSource(string packfilePath, long offset, long length)
        {
            Offset = offset;
            filepath = packfilePath;
            Size = length;
        }
        public byte[] ReadData()
        {
            byte[] data = new byte[Size];
            using (Stream stream = File.OpenRead(filepath))
            {
                stream.Seek(Offset, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }
            return data;
        }
    }
}