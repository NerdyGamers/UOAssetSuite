using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UOAssetSuite.Models
{
    [Flags]
    public enum TileFlag : ulong
    {
        None = 0x00000000, Background = 0x00000001, Weapon = 0x00000002, Transparent = 0x00000004,
        Translucent = 0x00000008, Wall = 0x00000010, Damaging = 0x00000020, Impassable = 0x00000040,
        Wet = 0x00000080, Unknown1 = 0x00000100, Surface = 0x00000200, Bridge = 0x00000400,
        Generic = 0x00000800, Window = 0x00001000, NoShoot = 0x00002000, PrefixA = 0x00004000,
        PrefixAn = 0x00008000, Internal = 0x00010000, Foliage = 0x00020000, PartialHue = 0x00040000,
        Unknown2 = 0x00080000, Map = 0x00100000, Container = 0x00200000, Wearable = 0x00400000,
        LightSource = 0x00800000, Animation = 0x01000000, NoDiagonal = 0x02000000, Unknown3 = 0x04000000,
        Armor = 0x08000000, Roof = 0x10000000, Door = 0x20000000, StairBack = 0x40000000,
        StairRight = 0x80000000
    }

    public class TileDataItem
    {
        public int Index { get; set; }
        public TileFlag Flags { get; set; }
        public int Weight { get; set; }
        public int Quality { get; set; }
        public int Quantity { get; set; }
        public int AnimID { get; set; }
        public int Hue { get; set; }
        public int StackingOffsetX { get; set; }
        public int StackingOffsetY { get; set; }
        public int StackingOffsetZ { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public int Value { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }
    }

    public class TiledataFile : IDisposable
    {
        private bool _disposed;
        public List<TileDataItem> Items { get; } = new();
        public int Count => Items.Count;

        public TiledataFile(string filePath) => Load(filePath);

        private void Load(string filePath)
        {
            using var reader = new BinaryReader(File.OpenRead(filePath));
            int version = reader.ReadInt32();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var item = new TileDataItem { Index = i };
                item.Flags = (TileFlag)reader.ReadUInt64();
                item.Weight = reader.ReadByte();
                item.Quality = reader.ReadByte();
                item.Quantity = reader.ReadByte();
                item.AnimID = reader.ReadUInt16();
                item.Hue = reader.ReadUInt16();
                item.StackingOffsetX = reader.ReadInt16();
                item.StackingOffsetY = reader.ReadInt16();
                item.StackingOffsetZ = reader.ReadInt16();
                item.Unknown1 = reader.ReadInt32();
                item.Unknown2 = reader.ReadInt32();
                item.Value = reader.ReadInt32();
                item.Height = reader.ReadByte();
                var nameBytes = new List<byte>();
                byte b;
                while ((b = reader.ReadByte()) != 0) nameBytes.Add(b);
                item.Name = Encoding.ASCII.GetString(nameBytes.ToArray());
                Items.Add(item);
            }
        }

        public TileDataItem GetTileData(int index) => index >= 0 && index < Items.Count ? Items[index] : null;

        public void SetTileData(int index, TileDataItem item)
        {
            if (index >= 0 && index < Items.Count) Items[index] = item;
            else if (index >= Items.Count)
            {
                while (Items.Count <= index) Items.Add(new TileDataItem { Index = Items.Count });
                Items[index] = item;
            }
        }

        public void Save(string filePath)
        {
            using var writer = new BinaryWriter(File.Create(filePath));
            writer.Write(0);
            writer.Write(Items.Count);
            foreach (var item in Items)
            {
                writer.Write((ulong)item.Flags);
                writer.Write((byte)item.Weight);
                writer.Write((byte)item.Quality);
                writer.Write((byte)item.Quantity);
                writer.Write((ushort)item.AnimID);
                writer.Write((ushort)item.Hue);
                writer.Write((short)item.StackingOffsetX);
                writer.Write((short)item.StackingOffsetY);
                writer.Write((short)item.StackingOffsetZ);
                writer.Write(item.Unknown1);
                writer.Write(item.Unknown2);
                writer.Write(item.Value);
                writer.Write((byte)item.Height);
                if (!string.IsNullOrEmpty(item.Name))
                {
                    var nameBytes = Encoding.ASCII.GetBytes(item.Name);
                    writer.Write(nameBytes, 0, nameBytes.Length);
                }
                writer.Write((byte)0);
            }
        }

        public void Dispose()
        {
            if (!_disposed) { Items.Clear(); _disposed = true; }
        }
    }
}