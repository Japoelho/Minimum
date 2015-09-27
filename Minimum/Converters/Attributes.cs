using System;

namespace Minimum.XML.Mapping
{
    public class Node : Attribute
    {
        internal string Name { get; set; }
                
        public Node(string name)
        { Name = name; }
    }

    public class XPath : Attribute
    {
        internal string Value { get; set; }

        public XPath(string attribute)
        { Value = attribute; }
    }

    public class IgnoreNode : Attribute { }

    public class IsEmpty : Attribute
    {
        public bool Ignore { get; set; }
        public object UseValue { get; set; }
    }

    public class Format : Attribute
    {
        internal string Value { get; set; }

        public Format(string value)
        { Value = value; }
    }
}
