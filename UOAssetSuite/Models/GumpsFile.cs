using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace UOAssetSuite.Models
{
    public class GumpItem
    {
        public int Index { get; set; }
        public Bitmap Image { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class GumpsFile : IDisposable
    {
        private readonly FileIndex _index;
        private bool _disposed;
        public List<GumpItem> Items { get; } = new();
        public int Count => Items.Count;

        public GumpsFile(string filePath)
        {
            string idxPath = filePath.Replace(".mul", ".idx");
            _index = new FileIndex(idxPath);
            Load(filePath);
        }

        private void Load(string filePath)
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
                Items.Add(new GumpItem { Index = i, Image = bitmap, Width = width, Height = height });
            }
        }

        public GumpItem GetGump(int index) => index >= 0 && index < Items.Count ? Items[index] : null;

        public void AddGump(int index, Bitmap image)
        {
            if (index >= 0 && index < Items.Count) Items[index].Image = image;
            else if (index >= Items.Count)
            {
                while (Items.Count <= index) Items.Add(new GumpItem { Index = Items.Count });
                Items[index].Image = image;
            }
        }

        public bool RemoveGump(int index)
        {
            if (index >= 0 && index < Items.Count) { Items[index].Image = null; return true; }
            return false;
        }

        public void Save(string filePath)
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
                    if (item.Image == null) { writer.Write(0); continue; }
                    int header = ((item.Width & 0x3FF) << 10) | (item.Height & 0x3FF);
                    writer.Write(header);
                    var pixelData = PixelDataHelper.EncodeArtData(item.Image);
                    writer.Write(pixelData.Length);
                    writer.Write(pixelData, 0, pixelData.Length);
                    offset = writer.BaseStream.Position;
                }
            }
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