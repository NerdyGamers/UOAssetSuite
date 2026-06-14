using System;
using System.Collections.Generic;
using System.IO;

namespace UOAssetSuite.Models
{
    public class MapCell
    {
        public short TileID { get; set; }
        public byte Z { get; set; }
    }

    public class MapBlock
    {
        public int X { get; set; }
        public int Y { get; set; }
        public MapCell[] Cells { get; set; } = Array.Empty<MapCell>();
    }

    public class MapsFile : IDisposable
    {
        private bool _disposed;
        public int MapID { get; set; }
        public int Width { get; set; } = 6144;
        public int Height { get; set; } = 4096;
        public List<MapBlock> Blocks { get; } = new();

        public MapsFile(string filePath)
        {
            MapID = ExtractMapID(filePath);
            Load(filePath);
        }

        private int ExtractMapID(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName.StartsWith("map") && int.TryParse(fileName.Substring(3), out int id)) return id;
            return 0;
        }

        private void Load(string filePath)
        {
            if (filePath.EndsWith(".uop", StringComparison.OrdinalIgnoreCase)) LoadUOP(filePath);
            else LoadMUL(filePath);
        }

        private void LoadMUL(string filePath)
        {
            using var reader = new BinaryReader(File.OpenRead(filePath));
            int blockWidth = Width / 8;
            int blockHeight = Height / 8;
            for (int y = 0; y < blockHeight; y++)
            {
                for (int x = 0; x < blockWidth; x++)
                {
                    var block = new MapBlock { X = x * 8, Y = y * 8, Cells = new MapCell[64] };
                    for (int i = 0; i < 64; i++)
                    {
                        block.Cells[i] = new MapCell { TileID = reader.ReadInt16(), Z = reader.ReadByte() };
                    }
                    Blocks.Add(block);
                }
            }
        }

        private void LoadUOP(string filePath) { }

        public MapCell GetCell(int x, int y)
        {
            int blockX = x / 8;
            int blockY = y / 8;
            int cellIndex = ((y % 8) * 8) + (x % 8);
            int blockIndex = blockY * (Width / 8) + blockX;
            if (blockIndex >= 0 && blockIndex < Blocks.Count)
            {
                var block = Blocks[blockIndex];
                if (cellIndex >= 0 && cellIndex < block.Cells.Length) return block.Cells[cellIndex];
            }
            return null;
        }

        public void SetCell(int x, int y, short tileID, byte z)
        {
            int blockX = x / 8;
            int blockY = y / 8;
            int cellIndex = ((y % 8) * 8) + (x % 8);
            int blockIndex = blockY * (Width / 8) + blockX;
            if (blockIndex >= 0 && blockIndex < Blocks.Count)
            {
                var block = Blocks[blockIndex];
                if (cellIndex >= 0 && cellIndex < block.Cells.Length)
                    block.Cells[cellIndex] = new MapCell { TileID = tileID, Z = z };
            }
        }

        public void Save(string filePath)
        {
            if (filePath.EndsWith(".uop", StringComparison.OrdinalIgnoreCase)) SaveUOP(filePath);
            else SaveMUL(filePath);
        }

        private void SaveMUL(string filePath)
        {
            using var writer = new BinaryWriter(File.Create(filePath));
            foreach (var block in Blocks)
            {
                foreach (var cell in block.Cells)
                {
                    writer.Write(cell.TileID);
                    writer.Write(cell.Z);
                }
            }
        }

        private void SaveUOP(string filePath) { }

        public void Dispose()
        {
            if (!_disposed) { Blocks.Clear(); _disposed = true; }
        }
    }
}