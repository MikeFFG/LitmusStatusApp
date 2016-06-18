using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LitmusStatus.Models
{
    public class Client
    {
        public string ClientName { get; set; }
        public string AppCode { get; set; }
        public string TimeInS { get; set; }
        public string CurrentStatus { get; set; }
        public string PlatformName { get; set; }
    }
}