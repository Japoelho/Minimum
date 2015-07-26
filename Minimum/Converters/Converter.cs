using Minimum.Loaders.WSQDecoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace Minimum
{
    public class Converter
    {
        public class IMG
        {
            public static byte[] ToJPG(string base64string)
            {
                return ConvertImage(base64string, ImageFormat.Jpeg);
            }

            public static byte[] ToBMP(string base64string)
            {
                return ConvertImage(base64string, ImageFormat.Bmp);
            }

            public static byte[] ToPNG(string base64string)
            {
                return ConvertImage(base64string, ImageFormat.Png);
            }

            public static byte[] ToGIF(string base64string)
            {
                return ConvertImage(base64string, ImageFormat.Gif);
            }

            private static byte[] ConvertImage(string base64string, ImageFormat imageFormat)
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64string)))
                {
                    MemoryStream converted = new MemoryStream();
                    Image.FromStream(stream).Save(converted, imageFormat);

                    return converted.ToArray();
                }
            }
        }

        public class WSQ
        {
            public static byte[] ToJPG(string base64string)
            {
                return ConvertImage(base64string, ImageFormat.Jpeg);
            }

            public static byte[] ToBMP(string base64string)
            {
                return ConvertImage(base64string, ImageFormat.Bmp);
            }

            public static byte[] ToPNG(string base64string)
            {
                return ConvertImage(base64string, ImageFormat.Png);
            }

            public static byte[] ToGIF(string base64string)
            {
                return ConvertImage(base64string, ImageFormat.Gif);
            }

            private static byte[] ConvertImage(string base64string, ImageFormat imageFormat)
            {
                WSQDecoder decoder = new WSQDecoder();
                Bitmap bitmap = decoder.Decode(Convert.FromBase64String(base64string));

                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, imageFormat);

                    return stream.ToArray();
                }
            }
        }

        public class EmbeddedResource
        {
            public static string ToString(string resourceName)
            {
                Assembly assembly = Assembly.GetCallingAssembly();
                Stream stream = assembly.GetManifestResourceStream(resourceName);
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            public static byte[] ToByte(string resourceName)
            {
                Assembly assembly = Assembly.GetCallingAssembly();
                //string[] ress = assembly.GetManifestResourceNames();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}
