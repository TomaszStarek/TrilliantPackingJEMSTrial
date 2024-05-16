using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp5.Data
{
    public class Result
    {
        public string ContainerName { get; set; }
        public DateTime CreationDate { get; set; }
        public string ContainerStatus { get; set; }
        public string RouteName { get; set; }
        public string RouteVersion { get; set; }
        public string RouteStepName { get; set; }
        public string ResourceName { get; set; }
    }

    public class CreateCointainer
    {
        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object ResponseException { get; set; }
        public Result Result { get; set; }
    }
}
