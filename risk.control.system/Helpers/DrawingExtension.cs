namespace risk.control.system.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;

    public static class DrawingExtensions
    {
        public static string ImageType(this Image? image)
        {
            if (image.RawFormat.Equals(ImageFormat.Bmp))
            {
                return "Bmp";
            }
            else if (image.RawFormat.Equals(ImageFormat.MemoryBmp))
            {
                return "BMP";
            }
            else if (image.RawFormat.Equals(ImageFormat.Wmf))
            {
                return "Emf";
            }
            else if (image.RawFormat.Equals(ImageFormat.Wmf))
            {
                return "Wmf";
            }
            else if (image.RawFormat.Equals(ImageFormat.Gif))
            {
                return "Gif";
            }
            else if (image.RawFormat.Equals(ImageFormat.Jpeg))
            {
                return "Jpeg";
            }
            else if (image.RawFormat.Equals(ImageFormat.Png))
            {
                return "Png";
            }
            else if (image.RawFormat.Equals(ImageFormat.Tiff))
            {
                return "Tiff";
            }
            else if (image.RawFormat.Equals(ImageFormat.Exif))
            {
                return "Exif";
            }
            else if (image.RawFormat.Equals(ImageFormat.Icon))
            {
                return "Ico";
            }

            return "";
        }
    }
}