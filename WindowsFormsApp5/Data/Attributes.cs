using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp5.Data
{
    internal class Attributes
    {
        public class InQueueRouteStep
        {
            public int InQueueRouteStepId { get; set; }
            public string InQueueRouteStepName { get; set; }
            public string InQueueRouteStepRouteName { get; set; }
        }

        public class WipAttribute
        {
            public int AttributeId { get; set; }
            public string AttributeName { get; set; }
            public string AttributeType { get; set; }
            public string AttributeValue { get; set; }
        }

        public class Wip
        {
            public int WipId { get; set; }
            public string SerialNumber { get; set; }
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
            public int DivisionId { get; set; }
            public string DivisionName { get; set; }
            public int MaterialId { get; set; }
            public string MaterialName { get; set; }
            public int AssemblyId { get; set; }
            public string AssemblyNumber { get; set; }
            public string AssemblyDescription { get; set; }
            public string AssemblyRevision { get; set; }
            public string AssemblyVersion { get; set; }
            public int PlannedOrderId { get; set; }
            public string PlannedOrderNumber { get; set; }
            public bool IsOnHold { get; set; }
            public bool IsScrapped { get; set; }
            public bool IsPacked { get; set; }
            public bool IsReferenceUnit { get; set; }
            public bool IsAssembled { get; set; }
            public string WipStatus { get; set; }
            public DateTime WipCreationDate { get; set; }
            public object ParentWip { get; set; }
            public IList<InQueueRouteStep> InQueueRouteSteps { get; set; }
            public object Panel { get; set; }
            public IList<WipAttribute> WipAttributes { get; set; }
        }

        public class attributes
        {
            public IList<Wip> Wips { get; set; }
        }

    }
}
