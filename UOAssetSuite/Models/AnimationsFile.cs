using System;
using System.Collections.Generic;
using System.IO;

namespace UOAssetSuite.Models
{
    public class AnimationFrame
    {
        public int Index { get; set; }
        public byte[] Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class AnimationGroup
    {
        public int GroupID { get; set; }
        public List<AnimationFrame> Frames { get; } = new();
    }

    public class AnimationsFile : IDisposable
    {
        private bool _disposed;
        public List<AnimationGroup> Groups { get; } = new();
        public int GroupCount => Groups.Count;

        public AnimationsFile(string filePath)
        {
            if (filePath.EndsWith(".uop", StringComparison.OrdinalIgnoreCase))
                LoadUOP(filePath);
            else
                LoadMUL(filePath);
        }

        private void LoadMUL(string filePath)
        {
            string idxPath = filePath.Replace(".mul", ".idx");
            var index = new FileIndex(idxPath);
            using var reader = new BinaryReader(File.OpenRead(filePath));
            for (int i = 0; i < index.Length; i++)
            {
                var entry = index[i];
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                int header = reader.ReadInt32();
                int frameCount = reader.ReadInt32();
                int[] frameOffsets = new int[frameCount];
                for (int j = 0; j < frameCount; j++) frameOffsets[j] = reader.ReadInt32();
                var group = new AnimationGroup { GroupID = i };
                for (int j = 0; j < frameCount; j++)
                {
                    reader.BaseStream.Seek(entry.Offset + frameOffsets[j], SeekOrigin.Begin);
                    int frameHeader = reader.ReadInt32();
                    int width = (frameHeader >> 10) & 0x3FF;
                    int height = frameHeader & 0x3FF;
                    int pixelDataSize = reader.ReadInt32();
                    var pixels = new byte[pixelDataSize];
                    reader.Read(pixels, 0, pixelDataSize);
                    group.Frames.Add(new AnimationFrame { Index = j, Data = pixels, Width = width, Height = height });
                }
                Groups.Add(group);
            }
        }

        private void LoadUOP(string filePath) { }

        public AnimationGroup GetGroup(int groupID) => groupID >= 0 && groupID < Groups.Count ? Groups[groupID] : null;

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
                foreach (var group in Groups)
                {
                    idxWriter.Write((int)offset);
                    idxWriter.Write(0);
                    long groupStart = writer.BaseStream.Position;
                    writer.Write(0);
                    writer.Write(group.Frames.Count);
                    long frameOffsetsPos = writer.BaseStream.Position;
                    for (int j = 0; j < group.Frames.Count; j++) writer.Write(0);
                    for (int j = 0; j < group.Frames.Count; j++)
                    {
                        var frame = group.Frames[j];
                        long framePos = writer.BaseStream.Position;
                        int header = ((frame.Width & 0x3FF) << 10) | (frame.Height & 0x3FF);
                        writer.Write(header);
                        writer.Write(frame.Data.Length);
                        writer.Write(frame.Data, 0, frame.Data.Length);
                        writer.BaseStream.Seek(frameOffsetsPos + j * 4, SeekOrigin.Begin);
                        writer.Write((int)(framePos - groupStart));
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                    }
                    long groupEnd = writer.BaseStream.Position;
                    writer.BaseStream.Seek(groupStart, SeekOrigin.Begin);
                    writer.Write((int)(groupEnd - groupStart - 4));
                    writer.BaseStream.Seek(0, SeekOrigin.End);
                    offset = groupEnd;
                }
            }
        }

        private void SaveUOP(string filePath) { }

        public void Dispose()
        {
            if (!_disposed) { Groups.Clear(); _disposed = true; }
        }
    }
}