using System;
using System.Collections.Generic;
using System.IO;

namespace UOAssetSuite.Models
{
    public class FileIndexEntry
    {
        public long Offset { get; set; }
        public int Size { get; set; }
        public int Extra { get; set; }
    }

    public class FileIndex : IDisposable
    {
        private readonly List<FileIndexEntry> _entries = new();
        private bool _disposed;

        public int Length => _entries.Count;
        public FileIndexEntry this[int index] => _entries[index];

        public FileIndex(string filePath)
        {
            if (File.Exists(filePath)) Load(filePath);
        }

        private void Load(string filePath)
        {
            using var reader = new BinaryReader(File.OpenRead(filePath));
            int count = (int)(reader.BaseStream.Length / 12);
            for (int i = 0; i < count; i++)
            {
                long offset = reader.ReadInt32() & 0x7FFFFFFF;
                int size = reader.ReadInt32();
                int extra = reader.ReadInt32();
                _entries.Add(new FileIndexEntry { Offset = offset * 0x8000, Size = size, Extra = extra });
            }
        }

        public void Dispose()
        {
            if (!_disposed) { _entries.Clear(); _disposed = true; }
        }
    }
}