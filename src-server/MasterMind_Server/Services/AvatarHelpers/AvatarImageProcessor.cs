using log4net;
using MasterMind_Server.Exceptions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace MasterMind_Server.Services.ClassHelpers
{
    internal class AvatarImageProcessor
    {
        private const int MaximumAvatarBytes = 102400;
        private const int MaximumAvatarDimension = 512;
        private const int MaximumDecodedDimension = 8000;
        private const int InitialJpegQuality = 90;
        private const int MinimumJpegQuality = 40;
        private const int JpegQualityStep = 10;
        private const double ResizeRatio = 0.8;

        private readonly static ILog logger = LogManager.GetLogger(typeof(AvatarImageProcessor));

        public byte[] Compress(byte[] sourceContent)
        {
            try
            {
                using (var stream = new MemoryStream(sourceContent))
                {
                    using (var image = Image.FromStream(stream, true))
                    {
                        ValidateDimensions(image);

                        return EncodeWithinLimit(image);
                    }
                }
            }
            catch (AvatarOperationException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                logger.Warn("The uploaded avatar is not a valid image.", ex);
                throw new AvatarOperationException(ErrorMessages.InvalidImage);
            }
            catch (OutOfMemoryException ex)
            {
                logger.Error("The uploaded avatar could not be decoded.", ex);
                throw new AvatarOperationException(ErrorMessages.InvalidImage);
            }
        }

        private void ValidateDimensions(Image image)
        {
            if (image.Width > MaximumDecodedDimension || image.Height > MaximumDecodedDimension)
            {
                logger.Warn("The uploaded avatar exceeds the maximum allowed dimensions.");
                throw new AvatarOperationException(ErrorMessages.ImageTooLarge);
            }
        }

        private byte[] EncodeWithinLimit(Image image)
        {
            Size currentSize = CalculateTargetSize(image);

            while (true)
            {
                byte[] encoded = EncodeWithFallingQuality(image, currentSize);

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
            throw new AvatarOperationException(ErrorMessages.ImageProcessingError);
        }

        private byte[] EncodeWithFallingQuality(Image image, Size targetSize)
        {
            byte[] smallest = null;

            for (int quality = InitialJpegQuality; quality >= MinimumJpegQuality; quality -= JpegQualityStep)
            {
                using (var bitmap = RenderToBitmap(image, targetSize))
                {
                    byte[] encoded = EncodeJpeg(bitmap, quality);

                    if (encoded.Length <= MaximumAvatarBytes)
                    {
                        return encoded;
                    }

                    smallest = encoded;
                }
            }

            return smallest;
        }

        private Size CalculateTargetSize(Image image)
        {
            int longestSide = Math.Max(image.Width, image.Height);

            if (longestSide <= MaximumAvatarDimension)
            {
                return new Size(image.Width, image.Height);
            }

            double ratio = (double)MaximumAvatarDimension / longestSide;

            return new Size(
                Math.Max(1, (int)(image.Width * ratio)),
                Math.Max(1, (int)(image.Height * ratio)));
        }

        private Bitmap RenderToBitmap(Image image, Size targetSize)
        {
            var bitmap = new Bitmap(targetSize.Width, targetSize.Height, PixelFormat.Format24bppRgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Black);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.DrawImage(image, new Rectangle(0, 0, targetSize.Width, targetSize.Height));
            }

            return bitmap;
        }

        private byte[] EncodeJpeg(Bitmap bitmap, int quality)
        {
            ImageCodecInfo codec = GetJpegCodec();

            using (var parameters = new EncoderParameters(1))
            {
                parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                using (var buffer = new MemoryStream())
                {
                    bitmap.Save(buffer, codec, parameters);

                    return buffer.ToArray();
                }
            }
        }

        private ImageCodecInfo GetJpegCodec()
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    return codec;
                }
            }

            throw new AvatarOperationException(ErrorMessages.ImageProcessingError);
        }

        private Size ReduceSize(Size currentSize)
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
