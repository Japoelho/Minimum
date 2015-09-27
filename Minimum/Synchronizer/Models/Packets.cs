using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimum.Synchronizer.Model
{
    public class Custom
    {
        public int EventID { get; set; }
        public string Data { get; set; }
    }

    internal class Request
    {
        public string Type { get; set; }
        public object[] Parameters { get; set; }
    }

    internal class Response
    {
        public bool HasMore { get; set; }
        public string Data { get; set; }
        public object[] Parameters { get; set; }
    }

    internal class Update
    {
        public string Type { get; set; }
        public string Data { get; set; }
        public object[] Parameters { get; set; }
    }
    
    internal class Packet
    {
        public PacketType Type { get; set; }        
        public string Data { get; set; }        
    }

    internal enum PacketType
    {
        Request = 0, 
        Update = 1, 
        Custom = 2, 
        Success = 200, 
        Error = 500
    }
}