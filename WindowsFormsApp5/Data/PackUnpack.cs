using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp5.Data
{
    internal class PackUnpack
    {
        public class Documents
        {
            public IList<object> Model { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class Result
        {
            public string ContainerName { get; set; }
            public int ContainerStatus { get; set; }
            public IList<object> WipsUnpacked { get; set; }
            public IList<string> WipsPacked { get; set; }
            public Documents Documents { get; set; }
        }

        public class PackResponse
        {
            public string Version { get; set; }
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public object ResponseException { get; set; }
            public Result Result { get; set; }
        }
    }
}
