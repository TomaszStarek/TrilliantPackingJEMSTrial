using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using Api = WindowsFormsApp5.API_JEMS;

namespace WindowsFormsApp5
{
    internal class JEMS
    {
        
        private string _server;
        private string _token;

        public string Server
        {
            get { return _server; }
            private set { _server = value; }
        }

        public string Token
        {
            get { return _token; }
            private set { _token = value; }
        }

        /// <summary>
        /// Set up server information and log in to JEMS. In debug mode using always stg serwer.
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="withMesureValue"></param>
        /// <param name="server">Production server: prd.jemsmm.kwi.corp.jabil.org | Stg server: stg.jemsmm.kwi.corp.jabil.org</param>
        public JEMS(string server = Api.PRD_SERVER)
        {
            this.Server = server;
            var getToken = Api.JEMS_GetToken(Server);
            if (getToken.Status)
            {
                Token = getToken.Message;
            }
        }

        /// <summary>
        /// Send data directly to JEMS.
        /// </summary>
        /// <param name="logData"></param>
        /// <returns></returns>
        public string SendLogToSystem(LogStructur logData)
        {
            string answer;
            try
            {
                //Data preparation
                string serialNumber = logData.SerialNumber;
                string resourceName = logData.ResourceName;
                answer = string.Concat(Environment.NewLine, "Api send for sn: " , serialNumber, Environment.NewLine);
                List<LogStructur.measurament> measures = logData.Measuraments;
                List<LogStructur.failure> failures = logData.failures;

                //Api send
                var okToStart = Api.JEMS_OkToStart(Token, serialNumber, resourceName, Server);
                answer = AddAnswer(answer, okToStart.Status, okToStart.ApiInfo);
                if (okToStart.Status) //check if we can log data
                {
                    var getWipId = Api.JEMS_GetWipId(Token, serialNumber, Server); //take id for product
                    answer = AddAnswer(answer, getWipId.Status, getWipId.ApiInfo);
                    //Check if recived Id and only then send next api
                    if (getWipId.Status)
                    {
                        string wipId = getWipId.Message;
                        //Start wip
                        var wipStart = Api.JEMS_WipStart(Token, wipId, resourceName, Server);
                        answer = AddAnswer(answer, wipStart.Status, wipStart.ApiInfo);
                        //Only if wip is correctly started then send next api
                        if (wipStart.Status)
                        {
                            bool cmpleteWip = true;
                            if (measures.Count > 0) //if we have data then send
                            {
                                var addMeasurements = Api.JEMS_AddMeasurements(Token, wipId, measures, Server);
                                answer = AddAnswer(answer, addMeasurements.Status, addMeasurements.ApiInfo);
                                cmpleteWip &= addMeasurements.Status;
                            }
                            if (failures.Count > 0) //if we have data then send
                            {
                                var addFailure = Api.JEMS_AddFailure(Token, wipId, failures, Server);
                                answer = AddAnswer(answer, addFailure.Status, addFailure.ApiInfo);
                                cmpleteWip &= addFailure.Status;
                            }
                            if (cmpleteWip)
                            {
                                //Complete wip
                                var wipComplete = Api.JEMS_WipComplete(Token, wipId, resourceName, Server);
                                answer = AddAnswer(answer, wipComplete.Status, wipComplete.ApiInfo);
                            }
                            else
                            {
                                //Abort wip
                                var wipAbort = Api.JEMS_WipAbort(Token, wipId, resourceName, Server);
                                answer = AddAnswer(answer, wipAbort.Status, wipAbort.ApiInfo);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                return e.Message;
            }
            return answer;
        }

        private string AddAnswer(string answer, bool status, string msg)
        {
            string strStatus = status ? "OK: " : "NOK: ";
            return string.Concat(answer, strStatus, DateTime.Now.ToString(), " - ", msg, Environment.NewLine);
        }
    }
}
