using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace UOAssetSuite.Models
{
    public class LandtileItem
    {
        public int Index { get; set; }
        public Bitmap Image { get; set; }
        public short TextureID { get; set; }
        public byte[] Unknown { get; set; } = Array.Empty<byte>();
    }

    public class LandtilesFile : IDisposable
    {
        private readonly FileIndex _index;
        private bool _disposed;
        public List<LandtileItem> Items { get; } = new();
        public int Count => Items.Count;

        public LandtilesFile(string filePath)
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
                if (header == 0x7FFF7FFF)
                {
                    short textureID = reader.ReadInt16();
                    var unknown = new byte[80];
                    reader.Read(unknown, 0, 80);
                    int pixelDataSize = reader.ReadInt32();
                    reader.ReadBytes(pixelDataSize);
                    Items.Add(new LandtileItem { Index = i, Image = null, TextureID = textureID, Unknown = unknown });
                }
                else
                {
                    int width = (header >> 10) & 0x3FF;
                    int height = header & 0x3FF;
                    short textureID = reader.ReadInt16();
                    var unknown = new byte[80];
                    reader.Read(unknown, 0, 80);
                    int pixelDataSize = reader.ReadInt32();
                    var pixels = new byte[pixelDataSize];
                    reader.Read(pixels, 0, pixelDataSize);
                    var bitmap = PixelDataHelper.DecodeArtData(pixels, width, height);
                    Items.Add(new LandtileItem { Index = i, Image = bitmap, TextureID = textureID, Unknown = unknown });
                }
            }
        }

        public LandtileItem GetLandtile(int index) => index >= 0 && index < Items.Count ? Items[index] : null;

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
                    int header = ((item.Image.Width & 0x3FF) << 10) | (item.Image.Height & 0x3FF);
                    writer.Write(header);
                    writer.Write(item.TextureID);
                    writer.Write(item.Unknown, 0, item.Unknown.Length);
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