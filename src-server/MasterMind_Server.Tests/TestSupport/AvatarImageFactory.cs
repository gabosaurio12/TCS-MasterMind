using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MasterMind_Server.Tests.TestSupport
{
    internal static class AvatarImageFactory
    {
        public static byte[] CreatePng(int width, int height)
        {
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                FillWithNoise(bitmap);

                using (var buffer = new MemoryStream())
                {
                    bitmap.Save(buffer, ImageFormat.Png);

                    return buffer.ToArray();
                }
            }
        }

        public static byte[] CreateInvalidImage()
        {
            return new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        }

        public static byte[] CreateOversizedContent(int totalBytes)
        {
            var content = new byte[totalBytes];

            for (int index = 0; index < content.Length; index++)
            {
                content[index] = (byte)(index % 251);
            }

            return content;
        }

        private static void FillWithNoise(Bitmap bitmap)
        {
            var random = new Random(12345);

            for (int row = 0; row < bitmap.Height; row++)
            {
                for (int column = 0; column < bitmap.Width; column++)
                {
                    bitmap.SetPixel(column, row, Color.FromArgb(
                        random.Next(256),
                        random.Next(256),
                        random.Next(256)));
                }
            }
        }
    }
}
