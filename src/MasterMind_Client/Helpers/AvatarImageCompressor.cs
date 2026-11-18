using log4net;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MasterMind_Client.Helpers
{
    public static class AvatarImageCompressor
    {
        private const int MaximumAvatarBytes = 102400;
        private const int MaximumAvatarDimension = 512;
        private const int MinimumJpegQuality = 40;
        private const int InitialJpegQuality = 90;
        private const int JpegQualityStep = 10;
        private const double ResizeRatio = 0.8;

        private readonly static ILog logger = LogManager.GetLogger(typeof(AvatarImageCompressor));

        public static byte[] Compress(byte[] sourceContent)
        {
            if (sourceContent == null || sourceContent.Length == 0)
            {
                throw new AvatarCompressionException("The selected image is empty.");
            }

            try
            {
                BitmapFrame frame = DecodeFirstFrame(sourceContent);
                Size targetSize = CalculateTargetSize(frame);

                return EncodeWithinLimit(frame, targetSize);
            }
            catch (AvatarCompressionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Warn("The selected file is not a readable image.", ex);
                throw new AvatarCompressionException("The selected file is not a valid image.");
            }
        }

        private static BitmapFrame DecodeFirstFrame(byte[] sourceContent)
        {
            using (var stream = new MemoryStream(sourceContent))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.None,
                    BitmapCacheOption.OnLoad);

                if (decoder.Frames.Count == 0)
                {
                    throw new AvatarCompressionException("The selected file has no readable frames.");
                }

                return decoder.Frames[0];
            }
        }

        private static byte[] EncodeWithinLimit(BitmapFrame frame, Size targetSize)
        {
            Size currentSize = targetSize;

            while (true)
            {
                byte[] encoded = EncodeWithFallingQuality(frame, currentSize);

                if (encoded.Length <= MaximumAvatarBytes)
                {
                    return encoded;
                }

                Size reducedSize = ReduceSize(currentSize);

                if (reducedSize == currentSize)
                {
                    break;
                }

                currentSize = reducedSize;
            }

            logger.Error("The avatar could not be compressed below the maximum allowed size.");
            throw new AvatarCompressionException("The image could not be compressed below 100 KB.");
        }

        private static byte[] EncodeWithFallingQuality(BitmapFrame frame, Size targetSize)
        {
            byte[] smallest = null;

            for (int quality = InitialJpegQuality; quality >= MinimumJpegQuality; quality -= JpegQualityStep)
            {
                byte[] encoded = EncodeJpeg(frame, targetSize, quality);

                if (encoded.Length <= MaximumAvatarBytes)
                {
                    return encoded;
                }

                smallest = encoded;
            }

            return smallest;
        }

        private static byte[] EncodeJpeg(BitmapFrame frame, Size targetSize, int quality)
        {
            var encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = quality;

            using (var stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(RenderFlattened(frame, targetSize)));
                encoder.Save(stream);

                return stream.ToArray();
            }
        }

        private static BitmapSource RenderFlattened(BitmapFrame frame, Size targetSize)
        {
            var visual = new DrawingVisual();

            int targetWidth = (int)targetSize.Width;
            int targetHeight = (int)targetSize.Height;

            using (DrawingContext context = visual.RenderOpen())
            {
                var bounds = new Rect(0, 0, targetWidth, targetHeight);

                context.DrawRectangle(Brushes.Black, null, bounds);
                context.DrawImage(frame, bounds);
            }

            var target = new RenderTargetBitmap(
                targetWidth,
                targetHeight,
                96,
                96,
                PixelFormats.Pbgra32);

            target.Render(visual);

            return target;
        }

        private static Size CalculateTargetSize(BitmapFrame frame)
        {
            int longestSide = Math.Max(frame.PixelWidth, frame.PixelHeight);

            if (longestSide <= MaximumAvatarDimension)
            {
                return new Size(frame.PixelWidth, frame.PixelHeight);
            }

            double ratio = (double)MaximumAvatarDimension / longestSide;

            return new Size(
                Math.Max(1, (int)(frame.PixelWidth * ratio)),
                Math.Max(1, (int)(frame.PixelHeight * ratio)));
        }

        private static Size ReduceSize(Size currentSize)
        {
            int width = Math.Max(1, (int)(currentSize.Width * ResizeRatio));
            int height = Math.Max(1, (int)(currentSize.Height * ResizeRatio));

            if (width == currentSize.Width && height == currentSize.Height)
            {
                return currentSize;
            }

            return new Size(width, height);
        }
    }
}
