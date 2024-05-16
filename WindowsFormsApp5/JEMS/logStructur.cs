using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WindowsFormsApp5
{
    class LogStructur : ILogInformation
    {
        private string _serialNumber; //S
        private string _customer; //C
        private string _division; //I
        private string _resourceName; //N
        private string _routeStepName; //P (optional)
        private string _operator; //O
        private bool _testStatus; //T
        private DateTime _startDate; //[
        private DateTime _stopDate; //]
        private List<measurament> _measuraments; //list of data
        private List<failure> _failures; //list of fails
        private string _outLog;
        private string _fileExtension;
        public string OutLog
        {
            get { return _outLog; }
            set { _outLog = value; }
        }
        public string FileExtension
        {
            get { return _fileExtension; }
            set { _fileExtension = value; }
        }
        public string SerialNumber
        {
            get { return _serialNumber; }
            private set { _serialNumber = value; }
        }
        public string Customer
        {
            get { return _customer; }
            private set { _customer = value; }
        }
        public string Division
        {
            get { return _division; }
            private set { _division = value; }
        }
        public string ResourceName
        {
            get { return _resourceName; }
            private set { _resourceName = value; }
        }
        public string RouteStepName
        {
            get { return _routeStepName; }
            private set { _routeStepName = value; }
        }
        public string Operator
        {
            get { return _operator; }
            private set { _operator = value; }
        }
        public bool TestStatus
        {
            get { return _testStatus; }
            set { _testStatus = value; }
        }
        public DateTime StartDate
        {
            get { return _startDate; }
            private set { _startDate = value; }
        }
        public DateTime StopDate
        {
            get { return _stopDate; }
            private set { _stopDate = value; }
        }

        public List<measurament> Measuraments
        {
            get { return _measuraments; }
            private set { _measuraments = value; }
        }

        public struct measurament
        {
            public string MeasureLabel; //M
            public string MeasureData; //d
            public double UpperLimit; //k
            public double LowerLimit; //l
        }

        public List<failure> failures
        {
            get { return _failures; }
            private set { _failures = value; }
        }

        public struct failure
        {
            public string FailurName; //F
            public string FailureDetails; //>
            public double UpperLimit; //k
            public double LowerLimit; //l
        }

        private string _comment;

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }


        /// <summary>
        /// Constructor for logs to our system.
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="customer"></param>
        /// <param name="resourceName"></param>
        /// <param name="operatorName"></param>
        /// <param name="startDate"></param>
        /// <param name="stopDate"></param>
        /// <param name="routeStepName"></param>
        /// <param name="devision"></param>
        public LogStructur(string sn, string customer, string resourceName, string operatorName, DateTime startDate, DateTime stopDate, string routeStepName, string devision = null)
        {
            this.SerialNumber = sn;
            this.Customer = customer;
            this.ResourceName = resourceName;
            this.Operator = operatorName;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.RouteStepName = routeStepName;
            this.Division = devision == null ? customer : devision; //rule that if we don't have devision put customer in log
            this.Measuraments = new List<measurament>();
            this.failures = new List<failure>();
        }
        /// <summary>
        /// Constructor only for report preparing. Only important data.
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="dateTime"></param>
        /// <param name="resourceName"></param>
        public LogStructur(string sn, DateTime dateTime, string resourceName)
        {
            this.SerialNumber = sn;
            this.Customer = default;
            this.ResourceName = resourceName;
            this.StartDate = dateTime;
            this.StopDate = dateTime;
            this.RouteStepName = default;
            this.Division = default;
            this.Measuraments = new List<measurament>();
            this.failures = new List<failure>();
        }

        public void AddMeasurament(measurament m)
        {
            if (m.MeasureLabel != default && m.MeasureData != default)
            {
                if (!m.MeasureLabel.Contains('\\'))
                {
                    try
                    {
                        string pattern = string.Concat(@"\b", m.MeasureLabel, @"(_[0-9]*)?\b"); //pattern to find item where test name start with exactly name or name + _index //https://regexr.com/
                        Regex theSameNameRule = new Regex(pattern);
                        int qtyOfTheSameTestName = this.Measuraments.Where(x => theSameNameRule.IsMatch(x.MeasureLabel)).Count();
                        if (qtyOfTheSameTestName > 0)
                        {
                            m.MeasureLabel += $"_{qtyOfTheSameTestName}";
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                this.Measuraments.Add(m);
            }
        }

        public void AddMeasurament(string measureLabel, double measureData, double upperLimit = 0, double lowerLimit = 0)
        {
            AddMeasurament(measureLabel, measureData.ToString(), upperLimit, lowerLimit);
        }

        public void AddMeasurament(string measureLabel, string measureData, string upperLimit, string lowerLimit)
        {
            Double.TryParse(upperLimit, out double uLimit);
            Double.TryParse(lowerLimit, out double lLimit);
            AddMeasurament(measureLabel, measureData, uLimit, lLimit);
        }

        public void AddMeasurament(string measureLabel, string measureData, double upperLimit = 0, double lowerLimit = 0)
        {
            var tempMeasurament = new measurament
            {
                MeasureLabel = measureLabel,
                MeasureData = measureData,
                UpperLimit = upperLimit,
                LowerLimit = lowerLimit,
            };
            AddMeasurament(tempMeasurament);
        }

        public void AddFailur(string failurName, string failureDetails, double upperLimit = 0, double lowerLimit = 0)
        {
            var tempFail = new failure
            {
                FailurName = failurName,
                FailureDetails = failureDetails,
                UpperLimit = upperLimit,
                LowerLimit = lowerLimit
            };
            this.failures.Add(tempFail);
        }

        public void AddFailur(measurament measurament)
        {
            AddFailur(measurament.MeasureLabel, measurament.MeasureData, measurament.UpperLimit, measurament.LowerLimit);
        }

        public void AddTestStatus(bool testStatus)
        {
            this.TestStatus = testStatus;
        }

        public void CopyMeasurments(List<measurament> measuraments)
        {
            foreach (var m in measuraments)
            {
                AddMeasurament(m.MeasureLabel, m.MeasureData, m.UpperLimit, m.LowerLimit);
            }
        }
        public void CopyFailure(List<failure> failures)
        {
            foreach (var f in failures)
            {
                AddFailur(f.FailurName, f.FailureDetails, f.UpperLimit, f.LowerLimit);
            }
        }
    }
}
