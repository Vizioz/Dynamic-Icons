// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpHandler.cs" company="Vizioz Limited">
//   This code is Open Source cover by the attached MIT License (MIT)
//   Originally developed by Vizioz Limited
// </copyright>
// <summary>
//   The http handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DynamicIcons
{
    using System;
    using System.Configuration;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Web;

    using ImageProcessor;

    /// <summary>
    /// The http handler.
    /// </summary>
    public class HttpHandler : IHttpHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandler"/> class.
        /// </summary>
        /// <param name="isReusable">
        /// The is reusable.
        /// </param>
        public HttpHandler(bool isReusable)
        {
            this.IsReusable = isReusable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHandler"/> class.
        /// </summary>
        public HttpHandler()
        {
                // TEST
        }

        /// <summary>
        /// Gets or sets a value indicating whether is reusable.
        /// </summary>
        public bool IsReusable { get; set; }

        /// <summary>
        /// The process request.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            int quality = 80;
            byte[] photoBytes;
            var iconType = SupportedIconTypes(context);

            if (iconType == DynamicIconType.Unsupported)
            {
                return;
            }

            var appSettingsReader = new AppSettingsReader();
            
            string dynamicIcon;
            try
            {
                dynamicIcon = (string)appSettingsReader.GetValue("dynamicIcon", typeof(string));
            }
            catch (Exception)
            {
                return;
            }

            quality = GetQuality(context, appSettingsReader, dynamicIcon, out photoBytes, quality);

            // This is in preparation for future Icon formats to be supported.
            switch (iconType)
            {
                case DynamicIconType.AppleTouchIcon:
                    GetAppleTouchIcon(context, photoBytes, quality);
                break;
            }
        }

        /// <summary>
        /// The get quality.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="appSettingsReader">
        /// The app settings reader.
        /// </param>
        /// <param name="dynamicIcon">
        /// The dynamic icon.
        /// </param>
        /// <param name="photoBytes">
        /// The photo bytes.
        /// </param>
        /// <param name="quality">
        /// The quality.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private static int GetQuality(
            HttpContext context,
            AppSettingsReader appSettingsReader,
            string dynamicIcon,
            out byte[] photoBytes,
            int quality)
        {
            string dynamicIconQuality;
            try
            {
                dynamicIconQuality = (string)appSettingsReader.GetValue("dynamicIconQuality", typeof(string));
            }
            catch (Exception)
            {
                dynamicIconQuality = "80";
            }

            var dynamicIconPath = context.Server.MapPath(dynamicIcon);

            photoBytes = File.ReadAllBytes(dynamicIconPath);

            if (dynamicIconQuality.Trim() == string.Empty)
            {
                return quality;
            }

            int result;
            int.TryParse(dynamicIconQuality, out result);
            if (result > 0)
            {
                quality = result;
            }

            return quality;
        }

        /// <summary>
        /// The get apple touch icon.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="photoBytes">
        /// The photo bytes.
        /// </param>
        /// <param name="quality">
        /// The quality.
        /// </param>
        private static void GetAppleTouchIcon(HttpContext context, byte[] photoBytes, int quality)
        {
            var format = ImageFormat.Png;

            var imageName = context.Request.Url.Segments.Last().ToLower().Substring(17).Replace("-precomposed", string.Empty);

            string width = string.Empty;
            string height = string.Empty;

            if (imageName.Contains("x"))
            {
                width = imageName.Substring(0, imageName.IndexOf("x", StringComparison.Ordinal));
                height = imageName.Substring(
                    imageName.IndexOf("x", StringComparison.Ordinal) + 1,
                    imageName.IndexOf(".", StringComparison.Ordinal) - (imageName.IndexOf("x", StringComparison.Ordinal) + 1));
            }

            int widthInt, heightInt;

            var size = new Size();
            if (int.TryParse(width, out widthInt) && int.TryParse(height, out heightInt))
            {
                size.Width = widthInt;
                size.Height = heightInt;
            }
            else
            {
                size.Width = 156;
                size.Height = 156;
            }

            using (var inStream = new MemoryStream(photoBytes))
            {
                using (var outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (var imageFactory = new ImageFactory(true))
                    {
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream).Resize(size).Format(format).Quality(quality).Save(outStream);
                    }

                    context.Response.Clear();
                    context.Response.ContentType = "image/png";

                    string headerValue = string.Format("attachment; filename={0}", context.Request.Url.Segments.Last());

                    context.Response.AppendHeader("Content-Disposition", headerValue);

                    outStream.WriteTo(context.Response.OutputStream);

                    context.Response.End();
                }
            }
        }

        /// <summary>
        /// Method to check if the current request is a supported icon type.
        /// </summary>
        /// <param name="context">
        /// The current request context.
        /// </param>
        /// <returns>
        /// The icon type
        /// </returns>
        private static DynamicIconType SupportedIconTypes(HttpContext context)
        {
            // As additional formats are added they will need to be added to this statement.
            if (context.Request.Url.Segments.Last().ToLower().Contains("apple-touch-icon"))
            {
                return DynamicIconType.AppleTouchIcon;
            }

            return DynamicIconType.Unsupported;                
        }
    }
}
