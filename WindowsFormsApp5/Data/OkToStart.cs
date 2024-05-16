using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp5.Data;

namespace WindowsFormsApp5.Data
{
    public class OkToStart
    {
        public class Failure
        {
            public string failureLabel { get; set; }
            public string failureMessage { get; set; }
            public string operatorOk { get; set; }
        public DateTime failureDateTime { get; set; }
        }

        public class Defect
        {
            public string defectType { get; set; }
            public string defectCategory { get; set; }
            public string defectLocation { get; set; }
            public string customerDefect { get; set; }
            public int defectQuantity { get; set; }
            public string defectDetail { get; set; }
            public bool isFalseCall { get; set; }
            public string operatorOk { get; set; }
        public DateTime defectDateTime { get; set; }
    }

    public class Rework
    {
        public string reworkCategory { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime completeDateTime { get; set; }
        public string startOperator { get; set; }
        public string completeOperator { get; set; }
        public string comment { get; set; }
        public string detail { get; set; }
    }

    public class WipOperationsDetail
    {
        public int wipProcessStepHistoryId { get; set; }
        public string manufacturingAreaName { get; set; }
        public string routeName { get; set; }
        public string routeStepName { get; set; }
        public string resourceName { get; set; }
        public string processStatus { get; set; }
        public DateTime inQueueDateTime { get; set; }
        public DateTime arrivalDateTime { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public int wipReturnCount { get; set; }
        public int cellNumber { get; set; }
        public string userName { get; set; }
        public DateTime lastUpdated { get; set; }
        public IList<Failure> failures { get; set; }
        public IList<Defect> defects { get; set; }
        public IList<Rework> reworks { get; set; }
    }

    public class WipDataCollection
    {
        public int wipProcessStepHistoryId { get; set; }
        public string dataCollectionGroupName { get; set; }
        public string dataCollectionLabel { get; set; }
        public string value { get; set; }
        public string routeName { get; set; }
        public string routeStepName { get; set; }
        public DateTime routeStepStartDatetime { get; set; }
        public string dataTypeName { get; set; }
        public int lowerLimit { get; set; }
        public int upperLimit { get; set; }
        public string unitOfMeasure { get; set; }
    }

    public class WipsData
    {
        public int wipId { get; set; }
        public string serialNumber { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public int divisionId { get; set; }
        public string divisionName { get; set; }
        public string productFamilyName { get; set; }
        public int materialId { get; set; }
        public string materialName { get; set; }
        public int assemblyRevisionId { get; set; }
        public string assemblyNumber { get; set; }
        public string assemblyRevision { get; set; }
        public string assemblyVersion { get; set; }
        public int panelId { get; set; }
        public string panelSerialNumber { get; set; }
        public int panelPosition { get; set; }
        public string wipStatus { get; set; }
        public bool isAssembled { get; set; }
        public bool isOnHold { get; set; }
        public bool isPacked { get; set; }
        public string bomName { get; set; }
        public IList<WipOperationsDetail> wipOperationsDetails { get; set; }
        public IList<WipDataCollection> wipDataCollections { get; set; }
    }

    public class wips
    {
        public bool okToStart { get; set; }
        public int routeStepId { get; set; }
        public string routeStepName { get; set; }
        public int routeId { get; set; }
        public string routeName { get; set; }
        public int resourceId { get; set; }
        public string resourceName { get; set; }
        public int panelId { get; set; }
        public IList<WipsData> wipsData { get; set; }
    }
}

}