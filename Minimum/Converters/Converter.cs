using Minimum.Loaders.WSQDecoder;
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
            public static byte[] ToJPG(byte[] image)
            {
                return ConvertImage(image, ImageFormat.Jpeg);
            }

            public static byte[] ToBMP(byte[] image)
            {
                return ConvertImage(image, ImageFormat.Bmp);
            }

            public static byte[] ToPNG(byte[] image)
            {
                return ConvertImage(image, ImageFormat.Png);
            }

            public static byte[] ToGIF(byte[] image)
            {
                return ConvertImage(image, ImageFormat.Gif);
            }

            private static byte[] ConvertImage(byte[] image, ImageFormat imageFormat)
            {
                using (MemoryStream stream = new MemoryStream(image))
                {
                    MemoryStream converted = new MemoryStream();
                    Image.FromStream(stream).Save(converted, imageFormat);

                    return converted.ToArray();
                }
            }
        }

        public class WSQ
        {
            public static byte[] ToJPG(byte[] wsq)
            {
                return ConvertImage(wsq, ImageFormat.Jpeg);
            }

            public static byte[] ToBMP(byte[] wsq)
            {
                return ConvertImage(wsq, ImageFormat.Bmp);
            }

            public static byte[] ToPNG(byte[] wsq)
            {
                return ConvertImage(wsq, ImageFormat.Png);
            }

            public static byte[] ToGIF(byte[] wsq)
            {
                return ConvertImage(wsq, ImageFormat.Gif);
            }

            private static byte[] ConvertImage(byte[] image, ImageFormat imageFormat)
            {
                WSQDecoder decoder = new WSQDecoder();
                Bitmap bitmap = decoder.Decode(image);

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
