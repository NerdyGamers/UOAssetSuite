using System;
using System.Collections.Generic;
using System.IO;

namespace UOAssetSuite.Models
{
    public class UOPFile : IDisposable
    {
        private readonly Dictionary<uint, byte[]> _blocks = new();
        private bool _disposed;
        public uint Version { get; private set; }
        public uint NextBlockID { get; private set; }

        public void Load(string filePath)
        {
            using var reader = new BinaryReader(File.OpenRead(filePath));
            Version = reader.ReadUInt32();
            uint blockCapacity = reader.ReadUInt32();
            uint blockSize = reader.ReadUInt32();
            NextBlockID = reader.ReadUInt32();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint blockID = reader.ReadUInt32();
                uint headerLength = reader.ReadUInt32();
                uint compressedLength = reader.ReadUInt32();
                uint decompressedLength = reader.ReadUInt32();
                uint checksum = reader.ReadUInt32();
                uint flag = reader.ReadUInt32();
                var header = new byte[headerLength];
                reader.Read(header, 0, (int)headerLength);
                var compressedData = new byte[compressedLength];
                reader.Read(compressedData, 0, (int)compressedLength);
                byte[] data = compressedLength == decompressedLength ? compressedData : Decompress(compressedData);
                _blocks[blockID] = data;
            }
        }

        private byte[] Decompress(byte[] compressedData) => compressedData;

        public byte[] GetBlock(uint blockID) => _blocks.TryGetValue(blockID, out var data) ? data : null;

        public void Save(string filePath)
        {
            using var writer = new BinaryWriter(File.Create(filePath));
            writer.Write(Version);
            writer.Write((uint)_blocks.Count);
            writer.Write(0x4000);
            writer.Write(NextBlockID);
            foreach (var kvp in _blocks)
            {
                writer.Write(kvp.Key);
                writer.Write(0);
                writer.Write((uint)kvp.Value.Length);
                writer.Write((uint)kvp.Value.Length);
                writer.Write(0);
                writer.Write(0);
                writer.Write(kvp.Value, 0, kvp.Value.Length);
            }
        }

        public void Dispose()
        {
            if (!_disposed) { _blocks.Clear(); _disposed = true; }
        }
    }
}