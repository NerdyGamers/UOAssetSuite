using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace UOAssetSuite.Models
{
    public class ArtItem
    {
        public int Index { get; set; }
        public Bitmap Image { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ArtFile : IDisposable
    {
        private readonly FileIndex _index;
        private readonly string _filePath;
        private bool _disposed;
        public List<ArtItem> Items { get; } = new();
        public int Count => Items.Count;

        public ArtFile(string filePath)
        {
            _filePath = filePath;
            if (filePath.EndsWith(".uop", StringComparison.OrdinalIgnoreCase))
                LoadUOP(filePath);
            else
            {
                string idxPath = filePath.Replace(".mul", ".idx");
                _index = new FileIndex(idxPath);
                LoadMUL(filePath);
            }
        }

        private void LoadMUL(string filePath)
        {
            using var reader = new BinaryReader(File.OpenRead(filePath));
            for (int i = 0; i < _index.Length; i++)
            {
                var entry = _index[i];
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                int header = reader.ReadInt32();
                int width = (header >> 10) & 0x3FF;
                int height = header & 0x3FF;
                int pixelDataSize = reader.ReadInt32();
                var pixels = new byte[pixelDataSize];
                reader.Read(pixels, 0, pixelDataSize);
                var bitmap = PixelDataHelper.DecodeArtData(pixels, width, height);
                Items.Add(new ArtItem { Index = i, Image = bitmap, Width = width, Height = height });
            }
        }

        private void LoadUOP(string filePath)
        {
            var uopFile = new UOPFile();
            uopFile.Load(filePath);
            for (uint blockID = 0; ; blockID++)
            {
                var blockData = uopFile.GetBlock(blockID);
                if (blockData == null || blockData.Length == 0) break;
                using var reader = new BinaryReader(new MemoryStream(blockData));
                int header = reader.ReadInt32();
                int width = (header >> 10) & 0x3FF;
                int height = header & 0x3FF;
                int pixelDataSize = reader.ReadInt32();
                var pixels = new byte[pixelDataSize];
                reader.Read(pixels, 0, pixelDataSize);
                var bitmap = PixelDataHelper.DecodeArtData(pixels, width, height);
                Items.Add(new ArtItem { Index = (int)blockID, Image = bitmap, Width = width, Height = height });
            }
        }

        public ArtItem GetArt(int index) => index >= 0 && index < Items.Count ? Items[index] : null;

        public void AddArt(int index, Bitmap image)
        {
            if (index >= 0 && index < Items.Count) Items[index].Image = image;
            else if (index >= Items.Count)
            {
                while (Items.Count <= index) Items.Add(new ArtItem { Index = Items.Count });
                Items[index].Image = image;
            }
        }

        public bool RemoveArt(int index)
        {
            if (index >= 0 && index < Items.Count) { Items[index].Image = null; return true; }
            return false;
        }

        public void Save(string filePath)
        {
            if (filePath.EndsWith(".uop", StringComparison.OrdinalIgnoreCase)) SaveUOP(filePath);
            else SaveMUL(filePath);
        }

        private void SaveMUL(string filePath)
        {
            string idxPath = filePath.Replace(".mul", ".idx");
            using (var writer = new BinaryWriter(File.Create(filePath)))
            using (var idxWriter = new BinaryWriter(File.Create(idxPath)))
            {
                long offset = 0;
                foreach (var item in Items)
                {
                    idxWriter.Write((int)offset);
                    idxWriter.Write(0);
                    if (item.Image == null) { writer.Write(0); writer.Write(0); offset = writer.BaseStream.Position; continue; }
                    int header = ((item.Width & 0x3FF) << 10) | (item.Height & 0x3FF);
                    writer.Write(header);
                    var pixelData = PixelDataHelper.EncodeArtData(item.Image);
                    writer.Write(pixelData.Length);
                    writer.Write(pixelData, 0, pixelData.Length);
                    offset = writer.BaseStream.Position;
                }
            }
        }

        private void SaveUOP(string filePath)
        {
            var uopFile = new UOPFile();
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item.Image == null) continue;
                using var ms = new MemoryStream();
                using var writer = new BinaryWriter(ms);
                int header = ((item.Width & 0x3FF) << 10) | (item.Height & 0x3FF);
                writer.Write(header);
                var pixelData = PixelDataHelper.EncodeArtData(item.Image);
                writer.Write(pixelData.Length);
                writer.Write(pixelData, 0, pixelData.Length);
                uopFile.GetBlock((uint)i);
            }
            uopFile.Save(filePath);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var item in Items) item.Image?.Dispose();
                Items.Clear();
                _disposed = true;
            }
        }
    }
}