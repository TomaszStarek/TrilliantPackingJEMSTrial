using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using ZXing.Aztec.Internal;
using WindowsFormsApp5.ExtensionMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WindowsFormsApp5
{
    public class ApiJems
    {
        public struct Answer
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public string ApiInfo { get; set; }
        }
        private static Answer ReturnAnswer(bool status, string message, string apiInfo = default)
        {
            return new Answer() { Status = status, Message = message, ApiInfo = apiInfo };
        }

        public static string Token { get; private set;}





        public static Answer JEMS_WipStart(string token, string wipId, string resourceName, string connectionString)
        {
            string server = "", username = "", password = "";
            server = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;

            var secretSection = ConfigurationManager.GetSection("localSecrets") as NameValueCollection;

            if (secretSection != null)
            {
                password = secretSection["Password"]?.ToString();
                username = secretSection["Username"]?.ToString();
            }

         //   (RestResponse connectResponse, string apiTime) = ExecuteApi(server, "/api/user/signin", username, password, Method.Post, token, resourceName);



            Method reqMethod = Method.Post;
            JObject body = new JObject { { "ResourceName", resourceName } };

            //Send data
            (RestResponse connectResponse, string apiTime) = ExecuteApi(server, "/api/Wips/{wipId}/oktostart", username, password, reqMethod, token, jsonBody: body);

            //Read data
            string answer = connectResponse.Content;
            dynamic response = JObject.Parse(answer);
            bool status = (connectResponse.StatusCode == HttpStatusCode.OK);
            string msg = status ? response.routeStepName.ToString() : response.errorMessage.ToString();
            msg = msg.ClearJsonArray();
            string apiInfo = string.Concat("WipStart ; ", apiTime, " ; ", msg);
            return ReturnAnswer(status, answer, apiInfo);
        }
        public static (RestResponse, string) ExecuteApiTestBody(string token, string resorce, Method method, dynamic jsonBody = null)
        {
            try
            {
                //   Form1.aTimer.Enabled = false;
                //   Form1.aTimer.Stop();
                //    var authenticator = new JwtAuthenticator(token);
                var options = new RestClientOptions("https://kwi-stg.jemsms.corp.jabil.org/api-external-api")
                {
                    //  Authenticator = authenticator
                };


                var client = new RestClient(options);


                //     var request = new RestRequest("/api/Wips/4097148/oktostart?resourceName=PLKWIM0T19CMD01", Method.Get);
                var request = new RestRequest(resorce, method);
                if (token != null) request.AddCookie("UserToken", token, "/", "kwi-stg.jemsms.corp.jabil.org");
                //  request.AddStringBody("token", token);
                //   request.AddCookie("UserToken", token, "/", "kwi-stg.jemsms.corp.jabil.org");
                // The cancellation token comes from the caller. You can still make a call without it.
                //JObject body = new JObject {
                //                                { "mode", "Assigned"},
                //                                { "customerId", 14 },
                //                                { "containerType", "TRILLIANT BOX" },
                //                                { "creationDate", "2024-04-18T09:00:00" },
                //                                { "routeName", "TRILLIANT BB GEN4" },
                //                                { "routeVersion", "1" },
                //                                { "routeStepName", "PACKOUT" },
                //                                { "resourceName", "TRILLIANT PACKOUT" }
                //                          };
                //jsonBody = body;

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
                    //  jsonMess = JsonConvert.SerializeObject(jsonBody);
                    //         request.AddStringBody(jsonMess, DataFormat.Json);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", jsonMess, ParameterType.RequestBody);
                }

                var timer = StartTimer();
                RestResponse answer = client.Execute(request);
                string elapsedTime = StopTimer(timer);

                if (!answer.IsSuccessful)
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, answer.Content, answer.StatusCode.ToString());
                return (answer, elapsedTime);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            return (null, null);

        }

        public static (RestResponse, string) ExecuteApi(string server, string resource, string username, string password, Method reqMethod, string token = null, dynamic jsonBody = null)
        {
            try
            {
                RestClientOptions options;

                if (token is null)
                {
                    options = new RestClientOptions(server)
                    {
                        Authenticator = new HttpBasicAuthenticator(username, password)
                    };
                }
                else
                {
                    options = new RestClientOptions("https://kwi-stg.jemsms.corp.jabil.org/api-external-api");
                }


                var client = new RestClient(options);
                var request = new RestRequest(resource, reqMethod);
                if (token != null) request.AddCookie("UserToken", token, "/", "kwi-stg.jemsms.corp.jabil.org");
                // The cancellation token comes from the caller. You can still make a call without it.

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
                    //request.AddParameter("application/json", jsonMess, ParameterType.RequestBody);
                }


                var timer = StartTimer();
                RestResponse answer = client.Execute(request);
                string elapsedTime = StopTimer(timer);

                if (!answer.IsSuccessful)
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, answer.Content, answer.StatusCode.ToString());

                return (answer, elapsedTime);
            }            
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            return (null, null);

        }


        public static (RestResponse, string) ExecuteApiTest(string token, string resorce, Method method)
        {
          //  Form1.aTimer.Stop();
            //    var authenticator = new JwtAuthenticator(token);
            var options = new RestClientOptions("https://kwi-stg.jemsms.corp.jabil.org/api-external-api")
                {
                  //  Authenticator = authenticator
                };
            

            var client = new RestClient(options);


            //     var request = new RestRequest("/api/Wips/4097148/oktostart?resourceName=PLKWIM0T19CMD01", Method.Get);
            var request = new RestRequest(resorce, method);

            //  request.AddStringBody("token", token);
            request.AddCookie("UserToken", token, "/", "kwi-stg.jemsms.corp.jabil.org");
            // The cancellation token comes from the caller. You can still make a call without it.

            //if (jsonBody != null)
            //{
            //    string jsonMess;
            //    switch (jsonBody)
            //    {
            //        case object a when a.GetType() == typeof(JObject):
            //            jsonMess = ((JObject)jsonBody).ToString(); break;
            //        case object a when a.GetType() == typeof(string):
            //            jsonMess = jsonBody; break;
            //        default: jsonMess = JsonConvert.SerializeObject(jsonBody); break;
            //    }
            //    request.AddStringBody(jsonMess, DataFormat.Json);
            //    //request.AddParameter("application/json", jsonMess, ParameterType.RequestBody);
            //}
      //      client.AddCookie("UserToken", token, "/", "https://kwi-stg.jemsms.corp.jabil.org/api-external-api");

            var timer = StartTimer();
            RestResponse answer = client.Execute(request);
            string elapsedTime = StopTimer(timer);
            //   Form1.aTimer.Start();

            if (!answer.IsSuccessful)
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, answer.Content, answer.StatusCode.ToString());

            return (answer, elapsedTime);
        }



        public Answer GetTokenSync(string connectionString)
        {
            string server="", username ="", password = "";
            server = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;

            var secretSection = ConfigurationManager.GetSection("localSecrets") as NameValueCollection;

            if (secretSection != null)
            {
                password = secretSection["Password"]?.ToString();
                username = secretSection["Username"]?.ToString();
            }

            (RestResponse response, string apiTime) = ExecuteApi(server , "/api/user/signin", username, password, Method.Get);

            string answer = response.Content;
            bool status = false;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.Content.Contains('=') && response.Content.Contains(';'))
                {
                    string token = response.Content.Split(separator: new[] { '=' }, count: 2).Last();
                    token = token.Substring(0, token.LastIndexOf(';'));
                    answer = token;
                    status = true;
                }
            }
            string apiInfo = string.Concat("GetToken ; ", apiTime, $" ; TokenRecived? = {status}, StatusCode = {response.StatusCode}");
            Token = answer;
            return ReturnAnswer(status, answer, apiInfo);
        }

        private static Stopwatch StartTimer()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        private static string StopTimer(Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalSeconds.ToString();
        }


    }
}
