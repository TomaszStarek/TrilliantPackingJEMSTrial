using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsFormsApp5.mes
{
    public static class WebApi
    {
        public static string Token { get; set; }

        public class WipData
        {
            public IList<string> routeStep { get; set; }
            public IList<string> stepInstance { get; set; }
            public string StartDateTime { get; set; }
            public string EndDateTime { get; set; }
            public int LangId { get; set; }
        }

        public class WipResult
        {
            public string Site { get; set; }
            public string Building { get; set; }
            public string ManufacturingArea { get; set; }
            public string ManufacturingAreaId { get; set; }
            public string Route { get; set; }
            public string RouteId { get; set; }
            public string Step { get; set; }
            public string StepId { get; set; }
            public string StepInstance { get; set; }
            public string StepInstanceId { get; set; }
            public string Equipment { get; set; }
            public string EquipmentId { get; set; }
            public string Customer { get; set; }
            public string Division { get; set; }
            public string Assembly { get; set; }
            public string Revision { get; set; }
            public string ProcessLoop { get; set; }
            public string TestLoop { get; set; }
            public string TestStatus { get; set; }
            public DateTime CompletionTime { get; set; }
            public string WipId { get; set; }
        }


        public static async Task CallApiWithCookie()
        {
            try
            {
                // Create HttpClientHandler and set the cookie
                var handler = new HttpClientHandler();
                handler.CookieContainer = new CookieContainer();
                handler.CookieContainer.Add(new Uri("kwi-stg.jemsms.corp.jabil.org"), new Cookie("UserToken", ApiJems.Token));

                // Create HttpClient with the handler
                using (var httpClient = new HttpClient(handler))
                {
                    // Make the API call
                    var response = await httpClient.GetAsync("https://kwi-stg.jemsms.corp.jabil.org/api-external-api/api/Wips/4439310/oktostart");
                    // Check the response
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(responseBody);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to call API. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }


        public static async Task LinkByLinkStation()
        {
            try
            {
                var UtcTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var UtcTime2 = DateTime.UtcNow.AddHours(-1).AddMinutes(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");


                WipData wip = new WipData()
                {
                    routeStep = new List<string>() { "FVT" },
                    stepInstance = new List<string>() { "PLKWIM0T28B1TPT" },
                    StartDateTime = UtcTime2,
                    EndDateTime = UtcTime,
                    LangId = 0

                };


                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("apiKey", "7520e3c1-b17e-41aa-b32d-3b1820dc8e15");
                    var content = new StringContent(JsonConvert.SerializeObject(wip), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://plkwim0app01/meswebapi/Wip/WipScanData", content);

                    var responseString = await response.Content.ReadAsStringAsync();

                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var myobjList = JsonConvert.DeserializeObject<List<WipResult>>(responseString, settings);

                        if (myobjList[0].TestStatus != null && myobjList[0].TestStatus.ToUpper().Contains("P"))
                        {
                            Form1.WipScanDataStatus = true;
                        }
                        else
                            Form1.WipScanDataStatus = false;





                    //if (responseString.ToString().ToLower().Contains("success"))
                    //{
                    //    Task.Run(() => MainWindow.MyWindow.PartCompleted(true));
                    //}
                    //else
                    //    Task.Run(() => MainWindow.MyWindow.PartCompleted(false));


                    //       ListAssemblyRouteByAssemblyData = JsonConvert.DeserializeObject<Data.ListAssemblyRouteByAssemblyData>(responseString);
                    //  responseString.Result.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd WebAPI");
            }

        }

    }
}
