using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace WindowsFormsApp5
{
    using ExtensionMethods;
    /// <summary><list type="table">
    /// <item><see langword="API_JEMS V0.1"/></item>
    /// <item>Author: Paweł Latoszek</item>
    /// <item>Date: 2022-06-12</item></list>
    /// <list type="bullet">
    /// <item>
    /// <list type="table">
    /// <item>V0.1 - Base funcion of <see langword="ExecuteApiRequest"/>.</item>
    /// <item>Add <see langword="JEMS_GetToken"/>,  <see langword="JEMS_GetWipId"/>,  <see langword="JEMS_OkToStart"/>,  <see langword="JEMS_WipStart"/>, 
    /// <see langword="JEMS_WipComplete"/>, <see langword="JEMS_AddMeasurements"/>, <see langword="JEMS_AddFailure"/>.</item></list></item>
    /// <item>V0.2 </item>
    /// </list>
    /// </summary>
    static class API_JEMS
    {
        public const string PRD_SERVER = "kwi-stg.jemsms.corp.jabil.org";
        public const string STG_SERVER = "stg.jemsmm.kwi.corp.jabil.org";
        public const string LOGIN = @"";
        public const string PASSWORD = "";

        public struct Answer
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public string ApiInfo { get; set; }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //******************************************************************* Public fcn ****************************************************************************//
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get token to use another API
        /// </summary>
        /// <param name="server">Server address</param>
        /// <param name="login">Login to JEMS system</param>
        /// <param name="password">Password to JEMS system</param>
        /// <returns></returns>
        public static Answer JEMS_GetToken(string server = PRD_SERVER, string login = LOGIN, string password = PASSWORD)
        {
            //Data to send
            int port = 443;
            string resource = "/api/user/signin";
            Method reqMethod = Method.Get;
            var parameters = new Dictionary<string, string>
            {
                { "Name", login },
                { "Password", password }
            };

            //Send data

            (RestResponse connectResponse, string apiTime) = ExecuteApiRequest("https://kwi-stg.jemsms.corp.jabil.org/api-external-api", port, resource, reqMethod, parameters: parameters);

            //Read data
            string answer = connectResponse.Content;
            bool status = false;
            if (connectResponse.StatusCode == HttpStatusCode.OK)
            {
                if (connectResponse.Content.Contains('=') && connectResponse.Content.Contains(';'))
                {
                    string token = connectResponse.Content.Split(separator: new[] { '=' }, count: 2).Last();
                    token = token.Substring(0, token.LastIndexOf(';'));
                    answer = token;
                    status = true;
                }
            }
            string apiInfo = string.Concat("GetToken ; ", apiTime, $" ; TokenRecived? = {status}, StatusCode = {connectResponse.StatusCode}");
            return ReturnAnswer(status, answer, apiInfo);
        }

        /// <summary>
        /// Recive Id for serial number of board. Id is requiered for almost all Api.
        /// </summary>
        /// <param name="token">User Token for autorization</param>
        /// <param name="serialNumber">Serial number of product for what we want to recived Id</param>
        /// <param name="server">Server address</param>
        /// <returns></returns>
        public static Answer JEMS_GetWipId(string token, string serialNumber, string server = PRD_SERVER)
        {
            //Data to send
            int port = 2010;
            string resource = $"odata/wips?$filter=SerialNumber eq '{serialNumber}'&$select=id";
            Method reqMethod = Method.Get;

            //Send data
            (RestResponse connectResponse, string apiTime) = ExecuteApiRequest(server, port, resource, reqMethod, token);

            //Read data
            string answer = connectResponse.Content;
            bool status = false;
            if (connectResponse.StatusCode == HttpStatusCode.OK)
            {
                dynamic response = JObject.Parse(answer);
                string wipId = response.value.First.Id;
                status = double.TryParse(wipId, out double _);
                if (status) answer = wipId;
            }

            string apiInfo = string.Concat("GetWipID ; ", apiTime, " ; ", answer);
            return ReturnAnswer(status, answer, apiInfo);
        }

        /// <summary>
        /// Check if we can log data for specyfic sn and specyfic resource name.
        /// </summary>
        /// <param name="token">User Token for autorization</param>
        /// <param name="serialNumber">Serial number of product what we want to check</param>
        /// <param name="resourceName">Name of step what we want to check</param>
        /// <param name="server">Server address</param>
        /// <returns></returns>
        public static Answer JEMS_OkToStart(string token, string serialNumber, string resourceName, string server = PRD_SERVER, bool parseData = false)
        {
            //Data to send
            int port = 2001;
            string resource = $"api/wips/oktostart?serialNumber={serialNumber}&resourceName={resourceName}";
            Method reqMethod = Method.Get;

            //Send data
            (RestResponse connectResponse, string apiTime) = ExecuteApiRequest(server, port, resource, reqMethod, token);

            //Read data
            Answer returnedAnswer;
            dynamic response = JObject.Parse(connectResponse.Content);
            if (connectResponse.StatusCode == HttpStatusCode.OK)
            {
                bool result = response.result.okToStart;
                returnedAnswer = ReturnAnswer(result, result.ToString());
            }
            else
            {
                string msg = parseData ? OkToStartReadInfo(response) : response.responseException.validationErrors.First.Reason;
                returnedAnswer = ReturnAnswer(false, msg);
            }

            returnedAnswer.ApiInfo = string.Concat("OkToStart ; ", apiTime, " ; ", returnedAnswer.Message);
            return returnedAnswer;
        }

        /// <summary>
        /// Start WIP at ResourceName
        /// </summary>
        /// <param name="token">User Token for autorization</param>
        /// <param name="wipId">Id of product</param>
        /// <param name="resourceName">The name of the Resource to start the WIP</param>
        /// <param name="server">Server address</param>
        /// <returns></returns>
        public static Answer JEMS_WipStart(string token, string wipId, string resourceName, string server = PRD_SERVER)
        {
            //Data to send
            int port = 2001;
            string resource = $"api/wips/{wipId}/start";
            Method reqMethod = Method.Post;
            JObject body = new JObject{{ "ResourceName", resourceName }};

            //Send data
            (RestResponse connectResponse, string apiTime) = ExecuteApiRequest(server, port, resource, reqMethod, token, jsonBody: body);

            //Read data
            string answer = connectResponse.Content;
            dynamic response = JObject.Parse(answer);
            bool status = (connectResponse.StatusCode == HttpStatusCode.OK);
            string msg = status ? response.routeStepName.ToString() : response.errorMessage.ToString();
            msg = msg.ClearJsonArray();
            string apiInfo = string.Concat("WipStart ; ", apiTime, " ; ", msg);
            return ReturnAnswer(status, answer, apiInfo);
        }

        /// <summary>
        /// Complete WIP at ResourceName
        /// </summary>
        /// <param name="token">User Token for autorization</param>
        /// <param name="wipId">Id of product</param>
        /// <param name="resourceName">The name of the Resource to complete the WIP</param>
        /// <param name="server">Server address</param>
        /// <returns></returns>
        public static Answer JEMS_WipComplete(string token, string wipId, string resourceName, string server = PRD_SERVER)
        {
            //Data to send
            int port = 2001;
            string resource = $"api/wips/{wipId}/complete";
            Method reqMethod = Method.Post;
            JObject body = new JObject { { "ResourceName", resourceName } };

            //Send data
            (RestResponse connectResponse, string apiTime) = ExecuteApiRequest(server, port, resource, reqMethod, token, jsonBody: body);

            //Read data
            string answer = connectResponse.Content;
            dynamic response = JObject.Parse(answer);
            bool status = (connectResponse.StatusCode == HttpStatusCode.OK);
            string msg = status ? response.inQueueRouteSteps.ToString() : response.errorMessage.ToString();
            msg = msg.ClearJsonArray();
            string apiInfo = string.Concat("WipComplete ; ", apiTime, " ; ", msg);
            return ReturnAnswer(status, answer, apiInfo);
        }

        /// <summary>
        /// Abort WIP at ResourceName
        /// </summary>
        /// <param name="token">User Token for autorization</param>
        /// <param name="wipId">Id of product</param>
        /// <param name="resourceName">The name of the Resource to abort the WIP</param>
        /// <param name="server">Server address</param>
        /// <returns></returns>
        public static Answer JEMS_WipAbort(string token, string wipId, string resourceName, string server = PRD_SERVER)
        {
            //Data to send
            int port = 2001;
            string resource = $"api/wips/{wipId}/abort";
            Method reqMethod = Method.Post;
            JObject body = new JObject { { "ResourceName", resourceName } };

            //Send data
            (RestResponse connectResponse, string apiTime) = ExecuteApiRequest(server, port, resource, reqMethod, token, jsonBody: body);

            //Read data
            string answer = connectResponse.Content;
            dynamic response = JObject.Parse(answer);
            bool status = (connectResponse.StatusCode == HttpStatusCode.OK);
            string msg = status ? response.inQueueRouteSteps.ToString() : response.errorMessage.ToString();
            msg = msg.ClearJsonArray();
            string apiInfo = string.Concat("WipAbort ; ", apiTime, " ; ", msg);
            return ReturnAnswer(status, answer, apiInfo);
        }

        /// <summary>
        /// Add measurements to JEMS. WIP need to be started before send data.
        /// </summary>
        /// <param name="token">User Token for autorization</param>
        /// <param name="wipId">Id of product</param>
        /// <param name="mData">Measurements to add</param>
        /// <param name="server">Server address</param>
        /// <returns></returns>
        public static Answer JEMS_AddMeasurements(string token, string wipId, List<LogStructur.measurament> mData, string server = PRD_SERVER)
        {
            //Data to send
            int port = 2001;
            string resource = $"api/wips/{wipId}/AddMeasurements";
            Method reqMethod = Method.Post;
            List<object> measurements = new List<object>();
            foreach (var item in mData) //pars measurment data to json
            {
                var data = item;
                int counter = measurements.Where(x => ((string)((dynamic)x).MeasurementLabel).Contains(item.MeasureLabel)).Count();
                if (counter > 0)
                {
                    data.MeasureLabel += $"_{counter}";
                }
                measurements.Add(new Measurement(data).ToJObject());
            }
            object jsonBody = new { measurements };

            //Send data
            (RestResponse connectResponse, string apiTime) = ExecuteApiRequest(server, port, resource, reqMethod, token , jsonBody: jsonBody);

            //Read data
            string answer = connectResponse.Content;
            dynamic response = JObject.Parse(answer);
            bool status = (connectResponse.StatusCode == HttpStatusCode.OK);
            string msg = status ? connectResponse.Content.ToString() : response.errorMessage.ToString();
            msg = msg.ClearJsonArray();
            string apiInfo = string.Concat("AddMeasurements ; ", apiTime, " ; ", msg);
            return ReturnAnswer(status, answer, apiInfo);
        }

        /// <summary>
        /// Add failure to JEMS. WIP need to be started before send data. FailureDetails/FailureMessage max is 300 characters. More then 300 will be cut.
        /// </summary>
        /// <param name="token">User Token for autorization</param>
        /// <param name="wipId">Id of product</param>
        /// <param name="failures">Failure to add.</param>
        /// <param name="server">Server address</param>
        /// <returns></returns>
        public static Answer JEMS_AddFailure(string token, string wipId, List<LogStructur.failure> failures, string server = PRD_SERVER)
        {
            //Data to send
            int port = 2001;
            string resource = $"api/wips/{wipId}/operations/current/multipleFailures";
            Method reqMethod = Method.Post;
            JArray measureArray = new JArray();
            foreach (var item in failures) //pars failures data to json
            {
                int length = item.FailureDetails.Length > 300 ? 300 : item.FailureDetails.Length; //limit of msg is 300 characters
                JObject oneFailJson = new JObject { { "symptomLabel" , item.FailurName }, { "failureMessage" , item.FailureDetails.Substring(0, length)} };
                measureArray.Add(oneFailJson); //add to array
            }
            object jsonBody = new { failureLabelList = measureArray };

            //Send data
            (RestResponse connectResponse, string apiTime) = ExecuteApiRequest(server, port, resource, reqMethod, token, jsonBody: jsonBody);

            //Read data
            string answer = connectResponse.Content;
            dynamic response = JObject.Parse(answer);
            bool status = (connectResponse.StatusCode == HttpStatusCode.OK);
            string msg = status ? response.addedFailureLabelList.ToString() : response.errorMessage.ToString();
            msg = msg.ClearJsonArray();
            string apiInfo = string.Concat("AddFailure ; ", apiTime, " ; ", msg);
            return ReturnAnswer(status, answer, apiInfo);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //****************************************************************** Private fcn ****************************************************************************//
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Answer ReturnAnswer(bool status, string message, string apiInfo = default)
        {
            return new Answer() { Status = status, Message = message, ApiInfo = apiInfo };
        }

        /// <summary>
        /// Send request 
        /// </summary>
        /// <param name="server">Server address</param>
        /// <param name="port">Api port number</param>
        /// <param name="resource">Api directory</param>
        /// <param name="reqMethod">HTTP method (GET, POST etc.)</param>
        /// <param name="token">User Token for autorization</param>
        /// <param name="jsonBody">Body in json format. Can be object, JObject or string</param>
        /// <param name="parameters">Parameter what we want to send</param>
        /// <returns></returns>
        private static (RestResponse, string) ExecuteApiRequest(string server, int port, string resource, Method reqMethod, string token = null, dynamic jsonBody = null, Dictionary<string, string> parameters = null)
        {
            RestClient client = new RestClient($"http://{server}:{port}/");
            RestRequest request = new RestRequest(resource, reqMethod);
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    request.AddParameter(item.Key, item.Value);
                }
            }
            if (jsonBody != null)
            {
                string jsonMess;
                switch (jsonBody)
                {
                    case object a when a.GetType() == typeof(JObject): 
                        jsonMess = ((JObject)jsonBody).ToString(); break;
                    case object a when a.GetType() == typeof(string):
                        jsonMess = jsonBody; break;
                    default: jsonMess = JsonConvert.SerializeObject(jsonBody); break;
                }
                request.AddStringBody(jsonMess, DataFormat.Json);
               // request.AddCookie("UserToken", token, "/", server);
                //request.AddParameter("application/json", jsonMess, ParameterType.RequestBody);
            }
          //>>>>  if (token != null) client.AddCookie("UserToken", token, "/", server);

            var timer = StartTimer();
            RestResponse answer = client.Execute(request);
            string elapsedTime = StopTimer(timer);

            return (answer, elapsedTime);
        }

        public static Stopwatch StartTimer()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        public static string StopTimer(Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalSeconds.ToString();
        }

        private static string OkToStartReadInfo(dynamic response)
        {
            string result = response.responseException.validationErrors.First.Reason;
            string queueStr = "In Queue For: ";
            if (result.Contains(queueStr))
            {
                result = result.Split(new string[] { queueStr }, 2, System.StringSplitOptions.None).Last();
                if (result.Contains('/')) result = result.Substring(result.IndexOf('/'));
                if (result.Contains("PACKOUT"))
                {
                    result = "Płyta jest gotowa do spakowania!";
                }
                else
                {
                    result = string.Concat("Płyta oczekuje do kroku o nazwie: ", result, "!");
                }
            }
            else if (result.Contains("cannot start at any route step at this resource."))
            {
                result = "Płyta najprawdopodobniej jest już spakowana!";
            }
            result = string.Concat("Przekaż płytę do lidera aby sprawdził historię w JEMS. \nInformacja dodatkowe:\n", result);
            return result;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //**************************************************************** Measurement Class ************************************************************************//
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Class to creat structure of API AddMeasurements
    /// </summary>
    internal class Measurement
    {
        public string MeasurementLabel { get; private set; }
        public string MeasurementData { get; private set; }
        public bool ParametricFlag { get; private set; }
        public string MeasurementMessage { get; private set; }
        public double LowerLimit { get; private set; }
        public double UpperLimit { get; private set; }
        public bool FailOnLimits { get; private set; }

        public Measurement(string mLabel, string mData, double lLimit = default, double uLimit = default, string mMessage = null)
        {
            bool numericValue = (uLimit != lLimit);
            AddMeasure(mLabel, mData, numericValue, lLimit, uLimit, numericValue, mMessage);
        }

        public Measurement(LogStructur.measurament m)
        {
            bool numericValue = (m.LowerLimit != m.UpperLimit);
            AddMeasure(m.MeasureLabel, m.MeasureData, numericValue, m.LowerLimit, m.UpperLimit, numericValue);
        }

        private void AddMeasure(string mLabel, string mData, bool parametricFlag, double lLimit, double uLimit, bool failOnLimit, string mMessage = null)
        {
            this.MeasurementLabel = mLabel;
            this.MeasurementData = mData.Length > 100 ? mData.Substring(0,100) : mData;
            this.ParametricFlag = parametricFlag;
            this.LowerLimit = lLimit;
            this.UpperLimit = uLimit;
            this.FailOnLimits = failOnLimit;
            this.MeasurementMessage = mMessage;
        }

        /// <summary>
        /// Creat JObject with limits or without if we don't have defined limits in object. JSON data.
        /// </summary>
        /// <returns></returns>
        public JObject ToJObject()
        {
            if (double.TryParse(this.MeasurementData, out double value))
            {
                this.MeasurementData = value.ToString();
                if (this.ParametricFlag)
                {
                    this.MeasurementData = this.MeasurementData.Replace(',', '.');
                    return JObject.FromObject(this);
                }
            }
            return JObject.FromObject(new { this.MeasurementLabel, this.MeasurementData, this.ParametricFlag, this.FailOnLimits, this.MeasurementMessage });
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //********************************************************************* Extensions **************************************************************************//
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    namespace ExtensionMethods
    {
        public static class StringExtensions
        {
            public static string ClearJsonArray(this string str)
            {
                if (str.Contains("\r\n"))
                {
                    return str.Replace("\r\n", " ").Replace('[', ' ').Replace(']', ' ');
                }
                return str;
            }
        }
    }
}