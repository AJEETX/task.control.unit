//using System;
//using System.Configuration;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;

//namespace risk.control.system.Helpers
//{
//    public static class ImageMask
//    {
//        private static Image maskImage = null;

//        public static bool CacheMask { get; private set; } = false;

//        static ImageMask()
//        {
//            var config = "ImageMask:CacheMask";

//            if (null != config)
//            {
//                bool.TryParse(config, out bool cache);
//                CacheMask = cache;

//                maskImage = Bitmap.FromFile("ImageMask:MaskPath");
//            }
//        }

//        public static void SetMask(Image mask)
//        {
//            maskImage = mask;
//        }

//        /// <summary>
//        /// Apply cached mask to an image.
//        /// </summary>
//        /// <param name="image"></param>
//        /// <param name="width"></param>
//        /// <param name="height"></param>
//        /// <returns></returns>
//        public static Image MaskImage(Image image, int width, int height)
//        {
//            Image resizedImage = (Bitmap)ResizeImageKeepAspectRatio(image, width, height);

//            if (CacheMask && null != maskImage)
//            {
//                using (var resizedMask = (Bitmap)ResizeImageKeepAspectRatio(maskImage, width, height))
//                {
//                    using (var g = Graphics.FromImage(resizedImage)) g.DrawImage(resizedMask, 0, 0);
//                }
//            }

//            return resizedImage;
//        }

//        /// <summary>
//        /// Apply a mask to an image.
//        /// </summary>
//        /// <param name="image"></param>
//        /// <param name="mask"></param>
//        /// <param name="width"></param>
//        /// <param name="height"></param>
//        /// <returns></returns>
//        public static Image MaskImage(Image image, Image mask, int width, int height)
//        {
//            Image resizedImage = (Bitmap)ResizeImageKeepAspectRatio(image, width, height);

//            using (var resizedMask = (Bitmap)ResizeImageKeepAspectRatio(mask, width, height))
//            {
//                using (var g = Graphics.FromImage(resizedImage)) g.DrawImage(resizedMask, 0, 0);
//            }

//            return resizedImage;
//        }

//        /// <summary>
//        /// Resize an image keeping its aspect ratio (cropping may occur).
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="width"></param>
//        /// <param name="height"></param>
//        /// <returns></returns>
//        public static Image ResizeImageKeepAspectRatio(Image source, int width, int height)
//        {
//            Image result = null;

//            try
//            {
//                if (source.Width != width || source.Height != height)
//                {
//                    // Resize image
//                    float sourceRatio = (float)source.Width / source.Height;

//                    using (var target = new Bitmap(width, height))
//                    {
//                        using (var g = System.Drawing.Graphics.FromImage(target))
//                        {
//                            g.CompositingQuality = CompositingQuality.HighQuality;
//                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
//                            g.SmoothingMode = SmoothingMode.HighQuality;

//                            // Scaling
//                            float scaling;
//                            float scalingY = (float)source.Height / height;
//                            float scalingX = (float)source.Width / width;
//                            if (scalingX < scalingY) scaling = scalingX; else scaling = scalingY;

//                            int newWidth = (int)(source.Width / scaling);
//                            int newHeight = (int)(source.Height / scaling);

//                            // Correct float to int rounding
//                            if (newWidth < width) newWidth = width;
//                            if (newHeight < height) newHeight = height;

//                            // See if image needs to be cropped
//                            int shiftX = 0;
//                            int shiftY = 0;

//                            if (newWidth > width)
//                            {
//                                shiftX = (newWidth - width) / 2;
//                            }

//                            if (newHeight > height)
//                            {
//                                shiftY = (newHeight - height) / 2;
//                            }

//                            // Draw image
//                            g.DrawImage(source, -shiftX, -shiftY, newWidth, newHeight);
//                        }

//                        result = new Bitmap(target);
//                    }
//                }
//                else
//                {
//                    // Image size matched the given size
//                    result = new Bitmap(target);
//                }
//            }
//            catch (Exception)
//            {
//                result = null;
//            }

//            return result;
//        }
//    }
//}