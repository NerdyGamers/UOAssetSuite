using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace UOAssetSuite.Models
{
    public static class PixelDataHelper
    {
        public static Bitmap DecodeArtData(byte[] pixelData, int width, int height)
        {
            if (pixelData == null || pixelData.Length == 0 || width <= 0 || height <= 0)
                return null;
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, width, height);
            var data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                int pixelDataIndex = 0;
                for (int y = 0; y < height; y++)
                {
                    int x = 0;
                    while (x < width && pixelDataIndex < pixelData.Length)
                    {
                        int runLength = pixelData[pixelDataIndex++];
                        if (runLength == 0)
                        {
                            int transparentCount = pixelData[pixelDataIndex++];
                            for (int i = 0; i < transparentCount && x < width; i++, x++)
                                SetPixel(data, x, y, width, 0);
                        }
                        else if (runLength < 0x80)
                        {
                            for (int i = 0; i < runLength && x < width; i++, x++)
                            {
                                if (pixelDataIndex + 1 >= pixelData.Length) break;
                                ushort color = (ushort)((pixelData[pixelDataIndex] << 8) | pixelData[pixelDataIndex + 1]);
                                pixelDataIndex += 2;
                                SetPixel(data, x, y, width, color);
                            }
                        }
                        else
                        {
                            int transparentCount = runLength - 0x7F;
                            for (int i = 0; i < transparentCount && x < width; i++, x++)
                                SetPixel(data, x, y, width, 0);
                        }
                    }
                }
            }
            finally { bitmap.UnlockBits(data); }
            return bitmap;
        }

        public static byte[] EncodeArtData(Bitmap bitmap)
        {
            if (bitmap == null) return Array.Empty<byte>();
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            for (int y = 0; y < bitmap.Height; y++)
            {
                int x = 0;
                while (x < bitmap.Width)
                {
                    int transparentStart = x;
                    while (x < bitmap.Width && bitmap.GetPixel(x, y).A == 0) x++;
                    if (x > transparentStart)
                    {
                        int transparentCount = x - transparentStart;
                        if (transparentCount > 0x7F) { writer.Write((byte)(0x80 + (transparentCount - 0x7F))); transparentCount = 0; }
                        else if (transparentCount > 0) { writer.Write((byte)0); writer.Write((byte)transparentCount); }
                    }
                    if (x >= bitmap.Width) break;
                    int colorStart = x;
                    while (x < bitmap.Width && bitmap.GetPixel(x, y).A != 0) x++;
                    int colorCount = x - colorStart;
                    if (colorCount > 0)
                    {
                        if (colorCount > 0x7F) colorCount = 0x7F;
                        writer.Write((byte)colorCount);
                        for (int i = colorStart; i < colorStart + colorCount; i++)
                        {
                            var pixel = bitmap.GetPixel(i, y);
                            ushort color = (ushort)((pixel.B >> 3) | ((pixel.G >> 3) << 5) | ((pixel.R >> 3) << 10) | (pixel.A > 0 ? (ushort)0x8000 : (ushort)0));
                            writer.Write((byte)(color >> 8));
                            writer.Write((byte)(color & 0xFF));
                        }
                    }
                }
            }
            return ms.ToArray();
        }

        private static unsafe void SetPixel(BitmapData data, int x, int y, int width, ushort color16)
        {
            byte* ptr = (byte*)data.Scan0.ToPointer();
            int index = y * data.Stride + x * 4;
            if (color16 == 0) { ptr[index] = 0; ptr[index + 1] = 0; ptr[index + 2] = 0; ptr[index + 3] = 0; }
            else
            {
                byte r = (byte)((color16 >> 10) & 0x1F);
                byte g = (byte)((color16 >> 5) & 0x1F);
                byte b = (byte)(color16 & 0x1F);
                ptr[index] = (byte)(b << 3);
                ptr[index + 1] = (byte)(g << 3);
                ptr[index + 2] = (byte)(r << 3);
                ptr[index + 3] = 255;
            }
        }

        public static Color Color16ToColor(ushort color16)
        {
            if (color16 == 0) return Color.Transparent;
            byte r = (byte)((color16 >> 10) & 0x1F);
            byte g = (byte)((color16 >> 5) & 0x1F);
            byte b = (byte)(color16 & 0x1F);
            return Color.FromArgb(255, r << 3, g << 3, b << 3);
        }

        public static ushort ColorToColor16(Color color)
        {
            if (color.A == 0) return 0;
            byte r = (byte)(color.R >> 3);
            byte g = (byte)(color.G >> 3);
            byte b = (byte)(color.B >> 3);
            return (ushort)((r << 10) | (g << 5) | b | 0x8000);
        }
    }
}