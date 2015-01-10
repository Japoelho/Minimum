using System;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;
using System.Collections;
using Minimum.XML.Mapping;

namespace PS.Util.XML
{
    public class XMLoader
    {
        public static XmlDocument Load(object element)
        {
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", null));

            Node node = Attribute.GetCustomAttribute(element.GetType(), typeof(Node)) as Node;
            XmlNode body = document.CreateElement(node != null ? node.Name : element.GetType().Name);

            document.AppendChild(body);

            LoadValues(element, body);            

            return document;
        }

        public static void Load(object element, string document)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(document);
            LoadObject(element, xml);
        }

        public static void Load(object element, XmlElement document)
        {
            LoadObject(element, document);
        }

        public static void Load(object element, XmlDocument document)
        {
            LoadObject(element, document.DocumentElement);
        }

        private static void LoadObject(object element, XmlNode parent)
        {
            Type type = element.GetType();
            PropertyInfo[] properties = type.GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                bool ignore = Attribute.GetCustomAttribute(properties[i], typeof(IgnoreNode)) as IgnoreNode != null ? true : false;
                if (ignore == true) { continue; }

                Node name = Attribute.GetCustomAttribute(properties[i], typeof(Node)) as Node;
                string nodeName = name != null ? name.Name : properties[i].Name;

                if (properties[i].PropertyType.IsGenericType && (properties[i].PropertyType.GetGenericTypeDefinition().Equals(typeof(IList<>)) || typeof(IList).IsAssignableFrom(properties[i].PropertyType)))
                {
                    //Listas
                    XmlNodeList nodeList = parent.SelectNodes(".//" + nodeName);
                    if (nodeList == null) { continue; }

                    Type subType = properties[i].PropertyType.GetGenericArguments()[0];
                    object property = properties[i].GetValue(element, null);

                    //if (property == null) { continue; }
                    //if (properties[i].PropertyType.IsInterface == false)
                    if(property == null && properties[i].PropertyType.IsInterface == false)
                    { 
                        property = Activator.CreateInstance(properties[i].PropertyType);
                        properties[i].SetValue(element, property, null);
                    }

                    if (subType.IsValueType || subType.Equals(typeof(System.String)) || subType.Equals(typeof(System.Object)))
                    {
                        for (int j = 0; j < nodeList.Count; j++)
                        {
                            (property as IList).Add(Convert.ChangeType(nodeList.Item(j).InnerText, subType));
                        }
                    }
                    else
                    {
                        for (int j = 0; j < nodeList.Count; j++)
                        {
                            object listElement = Activator.CreateInstance(subType);
                            LoadObject(listElement, nodeList.Item(j));
                            (property as IList).Add(listElement);
                        }
                    }
                }
                else if (properties[i].PropertyType.IsArray)
                {
                    //Arrays
                    XmlNode node = parent.SelectSingleNode(".//" + nodeName);
                    if (node == null) { continue; }

                    Type subType = properties[i].PropertyType.GetElementType();
                    object property = null;

                    if (node.ChildNodes.Count > 0)
                    { property = Array.CreateInstance(subType, node.ChildNodes.Count); }
                    else
                    { continue; }

                    if (subType.IsValueType || subType.Equals(typeof(System.String)))
                    {
                        for (int j = 0; j < node.ChildNodes.Count; j++)
                        {
                            (property as Array).SetValue(Convert.ChangeType(node.ChildNodes[j].InnerText, subType), j);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < node.ChildNodes.Count; j++)
                        {
                            object arrayElement = Activator.CreateInstance(subType);
                            LoadObject(arrayElement, node.ChildNodes[j]);
                            (property as Array).SetValue(arrayElement, j);
                        }
                    }
                }
                else if (properties[i].PropertyType.IsClass && !properties[i].PropertyType.Equals(typeof(System.String)))
                {
                    //Classes
                    XmlNode node = parent.SelectSingleNode(".//" + nodeName);
                    if (node == null) { continue; }

                    object property = properties[i].GetValue(element, null);
                    if (property == null)
                    { property = Activator.CreateInstance(properties[i].PropertyType); }

                    LoadObject(property, node);

                    properties[i].SetValue(element, property, null);
                }
                else if (properties[i].PropertyType.IsEnum)
                {
                    //Enums
                    XmlNode node = parent.SelectSingleNode(".//" + nodeName);
                    if (node == null) { continue; }
                    if (String.IsNullOrEmpty(node.InnerText)) { continue; }

                    properties[i].SetValue(element, Enum.Parse(properties[i].PropertyType, node.InnerText), null);
                }
                else
                {
                    //Propriedades
                    XmlNode node = parent.SelectSingleNode(".//" + nodeName);
                    if (node == null) { continue; }
                    if (String.IsNullOrEmpty(node.InnerText)) { continue; }

                    properties[i].SetValue(element, Convert.ChangeType(node.InnerText, properties[i].PropertyType), null);
                }
            }
        }

        private static void LoadValues(object element, XmlNode parent)
        {
            Type type = element.GetType();
            PropertyInfo[] properties = type.GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                bool ignore = Attribute.GetCustomAttribute(properties[i], typeof(IgnoreNode)) as IgnoreNode != null ? true : false;
                if (ignore == true) { continue; }

                Node nodeAttribute = Attribute.GetCustomAttribute(properties[i], typeof(Node)) as Node;
                string nodeName = nodeAttribute != null ? nodeAttribute.Name : properties[i].Name;

                if (properties[i].PropertyType.IsGenericType && (properties[i].PropertyType.GetGenericTypeDefinition().Equals(typeof(IList<>)) || typeof(IList).IsAssignableFrom(properties[i].PropertyType)))
                {
                    //Listas
                    //XmlNode nodeList = LoadList(properties[i].GetValue(element, null), properties[i].PropertyType.GetGenericArguments()[0], nodeName, parent);
                    //if (nodeList != null) { parent.AppendChild(nodeList); }
                    LoadList(properties[i].GetValue(element, null), properties[i].PropertyType.GetGenericArguments()[0], nodeName, parent);
                }
                else if (properties[i].PropertyType.IsArray)
                {
                    //Arrays
                    //XmlNode nodeList = LoadList(properties[i].GetValue(element, null), properties[i].PropertyType.GetElementType(), nodeName, parent);
                    //if (nodeList != null) { parent.AppendChild(nodeList); }
                    LoadList(properties[i].GetValue(element, null), properties[i].PropertyType.GetElementType(), nodeName, parent);
                }
                else if (properties[i].PropertyType.IsClass && !properties[i].PropertyType.Equals(typeof(System.String)))
                {
                    //Classes
                    object value = properties[i].GetValue(element, null);
                    if (value == null) { continue; }

                    XmlNode child = parent.OwnerDocument.CreateElement(nodeName);
                    LoadValues(value, child);
                    parent.AppendChild(child);
                }
                else
                {
                    //Propriedades Comuns
                    object value = properties[i].GetValue(element, null);

                    XmlNode node = parent.OwnerDocument.CreateElement(nodeName);
                    node.InnerText = value != null ? value.ToString() : "";
                    parent.AppendChild(node);
                }
            }
        }

        private static void LoadList(object list, Type listType, string nodeName, XmlNode parent)
        {
            if (list == null) { return; }

            //XmlNode nodeList = parent.OwnerDocument.CreateElement(parentName);
            if (listType.IsValueType || listType.Equals(typeof(System.String)))
            {
                for (int j = 0; j < (list as IList).Count; j++)
                {
                    //Node name = Attribute.GetCustomAttribute(listType, typeof(Node)) as Node;
                    //string nodeName = name != null ? name.Name : listType.Name;

                    XmlNode node = parent.OwnerDocument.CreateElement(nodeName);
                    node.InnerText = (list as IList)[j] != null ? (list as IList)[j].ToString() : "";
                    parent.AppendChild(node);
                }
            }
            else
            {
                for (int j = 0; j < (list as IList).Count; j++)
                {
                    //Node name = Attribute.GetCustomAttribute(listType, typeof(Node)) as Node;
                    //string nodeName = name != null ? name.Name : listType.Name;

                    XmlNode node = parent.OwnerDocument.CreateElement(nodeName);
                    LoadValues((list as IList)[j], node);
                    parent.AppendChild(node);
                }
            }

            //return nodeList;
        }
    }
}
