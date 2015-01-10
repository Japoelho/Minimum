using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Minimum.XML.Mapping;
using Minimum.Loaders.WSQDecoder;

namespace Minimum
{
    public class Loader
    {
        public class XML
        {
            public static T Load<T>(XElement xmlElement) where T : class, new()
            {
                T element = (T)LoadNodeIntoObject(xmlElement, typeof(T));

                return element;
            }

            private static object LoadNodeIntoObject(XElement xElement, Type valueType, PropertyInfo property = null)
            {
                object value = null;

                if (valueType.IsArray || typeof(Array).IsAssignableFrom(valueType))
                {
                    IEnumerable<XElement> xElements = xElement.Parent.Elements(xElement.Name);
                    value = Array.CreateInstance(valueType.GetElementType(), xElements.Count());

                    int count = 0;
                    foreach (XElement element in xElements)
                    {
                        (value as Array).SetValue(LoadNodeIntoObject(element, valueType.GetElementType(), null), count++);
                    }
                }
                else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    Type listType = valueType.GetGenericArguments()[0];

                    IEnumerable<XElement> xElements = xElement.Parent.Elements(xElement.Name);
                    value = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));

                    foreach (XElement element in xElements)
                    {
                        (value as IList).Add(LoadNodeIntoObject(element, listType, null));
                    }
                }
                else if (valueType.IsClass == true && valueType.Equals(typeof(System.String)) == false)
                {
                    value = Activator.CreateInstance(valueType);

                    PropertyInfo[] properties = valueType.GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        XElement element = null;

                        Node nodeName = Attribute.GetCustomAttribute(properties[i], typeof(Node)) as Node;
                        element = nodeName != null ? xElement.Element(nodeName.Name) : xElement.Element(properties[i].Name);

                        if (element != null)
                        {
                            properties[i].SetValue(value, LoadNodeIntoObject(element, properties[i].PropertyType, properties[i]), null);
                        }
                    }
                }
                else
                {
                    value = FormatReadValue(xElement.Value, valueType, property);
                }

                return value;
            }

            private static object FormatReadValue(object value, Type valueType, PropertyInfo property)
            {
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    if (valueType.GetGenericArguments()[0].IsEnum)
                    { return Enum.Parse(valueType.GetGenericArguments()[0], value.ToString()); }

                    if (valueType.GetGenericArguments()[0].IsValueType)
                    { valueType = valueType.GetGenericArguments()[0]; }
                }

                if (valueType.IsEnum)
                {
                    return Enum.Parse(valueType, value.ToString());
                }

                switch (valueType.Name)
                {
                    case "Boolean":
                    case "bool":
                    case "Single":
                    case "Decimal":
                    case "Int64":
                    case "Int32":
                    case "Int16":
                    case "int": { return Convert.ChangeType(value, valueType); }
                    case "Guid": { return Guid.Parse(value.ToString()); }
                    case "DateTime":
                        {
                            if (property != null)
                            {
                                Format dateFormat = Attribute.GetCustomAttribute(property, typeof(Format)) as Format;
                                if (dateFormat != null && String.IsNullOrEmpty(dateFormat.Value) == false)
                                {
                                    return DateTime.ParseExact(value.ToString(), dateFormat.Value, System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }

                            return Convert.ChangeType(value, valueType);
                        }
                    default: { return Convert.ChangeType(value, valueType); }
                }
            }

            public static XDocument Load(object element)
            {
                XDocument xDocument = new XDocument();

                XElement xElement = null;
                if (typeof(IList).IsAssignableFrom(element.GetType())) { xElement = new XElement("List" + element.GetType().GetGenericArguments()[0].Name); }

                xDocument.Add(LoadObjectIntoNode(xElement, element));
                
                return xDocument;
            }

            private static XElement LoadObjectIntoNode(XElement parent, object value, PropertyInfo property = null)
            {
                Type valueType = value != null ? value.GetType() : property.PropertyType;

                Node nodeName = property != null ? Attribute.GetCustomAttribute(property, typeof(Node)) as Node : Attribute.GetCustomAttribute(valueType, typeof(Node)) as Node;
                string elementName = nodeName != null ? nodeName.Name : property != null ? property.Name : valueType.Name;

                IsEmpty isEmpty = property != null ? Attribute.GetCustomAttribute(property, typeof(IsEmpty)) as IsEmpty : Attribute.GetCustomAttribute(valueType, typeof(IsEmpty)) as IsEmpty;
                if (value == null)
                {
                    if (isEmpty != null && isEmpty.Ignore == true) { return null; }
                    if (isEmpty != null && isEmpty.UseValue != null) { return LoadObjectIntoNode(parent, isEmpty.UseValue, property); }
                    return new XElement(elementName);
                }

                if (valueType.IsArray || typeof(Array).IsAssignableFrom(valueType))
                {
                    Array array = (Array)value;

                    for (int i = 0; i < array.Length; i++)
                    {
                        XElement element = LoadObjectIntoNode(parent, array.GetValue(i), property);
                        if (element != null) { parent.Add(element); }
                    }

                    return parent;
                }
                else if (typeof(IList).IsAssignableFrom(valueType))
                {
                    IList list = (value as IList);

                    for (int i = 0; i < list.Count; i++)
                    {
                        XElement element = LoadObjectIntoNode(parent, list[i], property);
                        if (element != null) { parent.Add(element); }
                    }

                    return parent;
                }
                else if (valueType.IsClass == true && valueType.Equals(typeof(System.String)) == false)
                {
                    XElement xElement = new XElement(elementName);

                    PropertyInfo[] properties = valueType.GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {   
                        if (properties[i].PropertyType.IsArray || typeof(Array).IsAssignableFrom(properties[i].PropertyType) || (properties[i].PropertyType.IsGenericType && properties[i].PropertyType.GetGenericTypeDefinition() == typeof(IList<>)))
                        {
                            LoadObjectIntoNode(xElement, properties[i].GetValue(value), properties[i]);
                        }
                        else
                        {
                            XElement element = LoadObjectIntoNode(xElement, properties[i].GetValue(value), properties[i]);
                            if (element != null) { xElement.Add(element); }
                        }
                    }

                    return xElement;
                }
                else
                {
                    XElement xElement = new XElement(elementName);
                    xElement.Value = FormatWriteValue(value, valueType, property).ToString();
                    return xElement;
                }
            }

            private static object FormatWriteValue(object value, Type valueType, PropertyInfo property)
            {
                Format valueFormat = property != null ? Attribute.GetCustomAttribute(property, typeof(Format)) as Format : null;

                if (valueType.IsEnum)
                {
                    return Enum.Parse(valueType, value.ToString()).ToString();
                }

                switch (valueType.Name)
                {
                    case "Boolean":
                    case "bool":
                        { return value.ToString(); }
                    case "Single":
                        {
                            if (valueFormat != null) { return Convert.ToSingle(value).ToString(valueFormat.Value); }
                            return value.ToString(); 
                        }
                    case "Decimal": 
                        {
                            if (valueFormat != null) { return Convert.ToDecimal(value).ToString(valueFormat.Value); }
                            return value.ToString(); 
                        }
                    case "Double":
                        {
                            if (valueFormat != null) { return Convert.ToDouble(value).ToString(valueFormat.Value); }
                            return value.ToString(); 
                        }
                    case "Int64":
                    case "Int32":
                    case "Int16": 
                        { 
                            return value.ToString(); 
                        }
                    case "DateTime":
                        {
                            if (valueFormat != null) { return Convert.ToDateTime(value).ToString(valueFormat.Value); }
                            return value.ToString();
                        }
                    default: { return value.ToString(); }
                }
            }
        }

        public class JSON
        {
            public static T Load<T>(string jsonString) where T : class, new()
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }

            public static string Load(object element)
            {
                return JsonConvert.SerializeObject(element);
            }
        }

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
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
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
