using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp5.Data;
using WindowsFormsApp5.Properties;

namespace WindowsFormsApp5
{
    public static class CheckHistory
    {      
        public static bool GetToken()
        {
            try
            {
                var apiJems = new ApiJems();
                var apiRespone = apiJems.GetTokenSync("stg");

            }
            catch (Exception)
            {

                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Wystąpił błąd podczas pobierania tokena Jems!!!", "Błąd JEMS");
                return false;
            }


            return true;
        }

        public static bool checkSnHistoryJEMS(string sn)
        {
            Form1._myWindow.StopScannerCheckBoard();
            Form1._myWindow.StopScannerForPackout();
            var timer = API_JEMS.StartTimer();


            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var WipId = ApiJems.ExecuteApiTestBody(ApiJems.Token, $"api/Wips/GetWipIdBySerialNumber?SiteName=Kwidzyn&CustomerName=TRILLIANT&SerialNumber={sn}", Method.Get);
            var WipIdResults = JsonConvert.DeserializeObject<List<Data.WipIdResults>>(WipId.Item1.Content, settings);
            var Id = WipIdResults.FirstOrDefault().WipId;

     //       var okToStart = ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/Wips/{Id}/oktostart?resourceName=TRILLIANT PACKOUT", Method.Get);
     //       var operationHistories = ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/Wips/{Id}/OperationHistories", Method.Get);
     //       var attribiutes = ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/Wips/attributes?SiteName=Kwidzyn&SerialNumber={sn}", Method.Get);

            var task1 = Task.Run(() => ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/Wips/{Id}/oktostart?resourceName=TRILLIANT PACKOUT", Method.Get));
            var task2 = Task.Run(() => ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/Wips/{Id}/OperationHistories", Method.Get));
            var task3 = Task.Run(() => ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/Wips/attributes?SiteName=Kwidzyn&SerialNumber={sn}", Method.Get));

            // Poczekaj na zakończenie wszystkich zadań
            Task.WaitAll(task1, task2, task3);
            string elapsedTime = API_JEMS.StopTimer(timer);

            // Pobierz wyniki zadań
            var okToStart = task1.Result;
            var operationHistories = task2.Result;
            var attribiutes = task3.Result;


            Form1._myWindow.RunScannerCheckBoard();
            Form1._myWindow.RunScannerForPackout();
            return true;
            BoxToPackaut.Nc = WipIdResults.FirstOrDefault().MaterialName;


            var okToStartResults = JsonConvert.DeserializeObject<Data.OkToStart.wips>(okToStart.Item1.Content);

            if(okToStartResults.okToStart == false)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Wystąpił błąd podczas pobierania tokena Jems!!!", "Błąd okToStart false");
                return false;
            }



            var history = JsonConvert.DeserializeObject<Example>(operationHistories.Item1.Content);
            var historyResult = history.Wips.FirstOrDefault().OperationHistories;

            DateTime burinDate = DateTime.Now;
            DateTime fvtDate = DateTime.Now.AddDays(15); 

            foreach (var item in historyResult)
            {
                if (item.RouteStepName.ToUpper().Equals("BURN_IN"))
                    burinDate = item.EndDateTime;
                if (item.RouteStepName.ToUpper().Equals("FVT"))
                    fvtDate = item.EndDateTime;
            }
            var burnResult = CheckTimeEndStep(burinDate , ">", TimeSpan.FromMinutes(23 * 60 + 30));//22 * 60
            var fvtResult = CheckTimeEndStep(fvtDate , "<", TimeSpan.FromMinutes(24 * 60 * 14));

            if (burnResult == 0)
                ;   //step OK
            else if (burnResult == 1)
            {
               // MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Zły czas buforu dla BURN_IN", sn);
                return true;
            }

            if (fvtResult == 0)
                ;   //step OK
            else if (fvtResult == 1)
            {
             //   MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Zły czas buforu dla FVT", sn);
              //  return false;
            }


            var attributesResponse = JsonConvert.DeserializeObject<Data.Attributes.attributes>(attribiutes.Item1.Content);


            bool MOVEBOARD_CHECKDATA = false, MOVEBOARD_CHECKDATA1 = false, MOVEBOARD_CHECKDATA2 = false, MOVEBOARD_CHECKDATA3 = false, MOVEBOARD_CHECKDATA4 = false;
            bool MOVEBOARD_CHECKDATA5_1 = false, MOVEBOARD_CHECKDATA5_2 = false, MOVEBOARD_CHECKDATA6 = false, MOVEBOARD_CHECKDATA7 = false;


            string HW_Version = "";
            string FW_Version = "";
            string Class = "";

            if (!BoxToPackaut.CheckIsNcIsSameInBox(sn, BoxToPackaut.Nc))
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Niezgodność wersji NC: {BoxToPackaut.Nc}! Sprawdź numery!!!", sn);
                return true;
            }
            switch (BoxToPackaut.Nc)
            {
                case "BL-R0171A-1.1":
                    HW_Version = "1.1";
                    FW_Version = "3.5.0";
                    Class = "N/A";
                    break;
                case "BL-R0171A-1.3":
                    HW_Version = "1.3";
                    FW_Version = "3.5.0";
                    Class = "N/A";
                    break;
                case "BL-R0172A-1.2":
                    HW_Version = "1.1";
                    FW_Version = "3.5.0";
                    Class = "N/A";
                    break;
                case "BL-R0172B-1.1":
                    HW_Version = "1.1";
                    FW_Version = "80.237";
                    Class = "NEMA.78.68.49.56";
                    break;
                case "BL-R0172B-1.2":
                    HW_Version = "1.1";
                    FW_Version = "80.48";
                    Class = "NEMA.78.68.49.56";
                    break;
                case "BL-R0172C-1.2":
                    HW_Version = "1.1";
                    FW_Version = "80.45";
                    Class = "NEMA.78.68.49.52";
                    break;
                case "BL-R0172C-1.3":
                    HW_Version = "1.2";  //ma być 1.2  było 1.1 jak panel 20sztuk
                    FW_Version = "80.47"; //80.47
                    Class = "NEMA.78.68.49.52";
                    break;
                case "BL-R0172C-1.4":
                    HW_Version = "1.2";
                    FW_Version = "80.47";
                    Class = "NEMA.78.68.49.52";
                    break;
                case "BL-R0172D-1.0":
                    HW_Version = "1.1";
                    FW_Version = "80.49";
                    Class = "NEMA.78.68.49.48";
                    break;
                default:
                    HW_Version = "";
                    FW_Version = "";
                    Class = "";
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Niepoprawny numer NC!", BoxToPackaut.Nc);
                    break;
            }



            foreach (var item in attributesResponse.Wips.FirstOrDefault().WipAttributes)
            {
                if (item.AttributeName.Contains("Part_Number") && item.AttributeValue.Length > 13)
                    MOVEBOARD_CHECKDATA = true;
                else if (item.AttributeName.Contains("Ap Title") && item.AttributeValue.Length > 10)
                    MOVEBOARD_CHECKDATA1 = true;
                else if (item.AttributeName.Contains("RegFingerPrint") && item.AttributeValue.Length > 16)
                    MOVEBOARD_CHECKDATA2 = true;
                else if (item.AttributeName.Contains("HW_Version") && item.AttributeValue.Contains(HW_Version))
                    MOVEBOARD_CHECKDATA3 = true;
                else if (item.AttributeName.Contains("FW_Version") && item.AttributeValue.Contains(FW_Version))
                    MOVEBOARD_CHECKDATA4 = true;
                else if (item.AttributeName.Contains("Class") && item.AttributeValue.Contains(Class))
                    MOVEBOARD_CHECKDATA5_1 = true;
                else if (item.AttributeName.Contains("4_1 MAC") && item.AttributeValue.Length > 10)
                    MOVEBOARD_CHECKDATA5_2 = true;
                else if (item.AttributeName.Contains("AuthFingerPrint") && item.AttributeValue.Length > 18)
                    MOVEBOARD_CHECKDATA6 = true;
                else if (item.AttributeName.Contains("AuthCerName") && item.AttributeValue.Length > 14)
                    MOVEBOARD_CHECKDATA7 = true;

            }
            MOVEBOARD_CHECKDATA = true;
            MOVEBOARD_CHECKDATA7 = true;

            if (MOVEBOARD_CHECKDATA && MOVEBOARD_CHECKDATA1 && MOVEBOARD_CHECKDATA2 && MOVEBOARD_CHECKDATA3 && MOVEBOARD_CHECKDATA4 &&
                    MOVEBOARD_CHECKDATA5_1 && MOVEBOARD_CHECKDATA5_2 && MOVEBOARD_CHECKDATA6 && MOVEBOARD_CHECKDATA7)
                {
                    ;
                }
                else
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Niepoprawne parametry odpowiedzi JEMS, sprawdź historię płytki!", sn);
                    return true;
                }


                return true;
        }


        public static bool checkSnHistory(string sn)
        {
            try
            {
                //"NEQA4898711"
                var webservices = new WebServices();


                var burnResult = webservices.CheckTimeEndStep("QC BURN_IN", "TRILLIANT", sn, ">", TimeSpan.FromMinutes(23 * 60 + 30));//22 * 60
                var fvtResult = webservices.CheckTimeEndStep("FVT FVT", "TRILLIANT", sn, "<", TimeSpan.FromMinutes(24 * 60 * 14));

                if (burnResult == 0)
                    ;   //step OK
                else if (burnResult == 1)
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Numer produktu nie widnieje w systemie MES", sn);
                    return false;
                }
                else if (burnResult == 2)
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Brak kroku QC BURN_IN", sn);
                    return false;
                }
                else if (burnResult == 3)
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nieprawidłowy czas buforu! \n QC BURN_IN", sn);
                    return false;
                }


                if (fvtResult == 0)
                    ;   //step OK
                else if (fvtResult == 1)
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Numer produktu nie widnieje w systemie MES", sn);
                    return false;
                }
                else if (fvtResult == 2)
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Brak kroku FVT", sn);
                    return false;
                }
                else if (fvtResult == 3)
                {
                  //  MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nieprawidłowy czas buforu! \n FVT", sn);
                    return false;
                }

                var ncNumber = webservices.GetNcNumber("TRILLIANT", sn);
                var resultHistoryCheckPoint = webservices.CheckSerialNumberByCheckpoint("TRILLIANT", "PACKOUT", sn);
                var resultBoxNumber = webservices.GetBoxNumber("TRILLIANT", sn);

                
                var resultGetMeasuredData = webservices.GetMeasuredData("TRILLIANT", sn);
                if (resultGetMeasuredData is null)
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Brak odpowiedzi od MES, wysyłam zapytanie jeszcze raz!", sn);
                    resultGetMeasuredData = webservices.GetMeasuredData("TRILLIANT", sn);
                    if (resultGetMeasuredData is null)
                    {
                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Brak odpowiedzi od MES!", sn);
                        return false;
                    }
                }

                bool MOVEBOARD_CHECKDATA = false, MOVEBOARD_CHECKDATA1 = false, MOVEBOARD_CHECKDATA2 = false, MOVEBOARD_CHECKDATA3 = false, MOVEBOARD_CHECKDATA4 = false;
                bool MOVEBOARD_CHECKDATA5_1 = false, MOVEBOARD_CHECKDATA5_2 = false, MOVEBOARD_CHECKDATA6 = false, MOVEBOARD_CHECKDATA7 = false;


                string HW_Version = "";
                string FW_Version = "";
                string Class = "";

                if(!BoxToPackaut.CheckIsNcIsSameInBox(sn, ncNumber))
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Niezgodność wersji NC: {ncNumber}! Sprawdź numery!!!", sn);
                    return false;
                }


                switch (ncNumber)
                {
                    case "BL-R0171A-1.1":
                        HW_Version = "1.1";
                        FW_Version = "3.5.0";
                        Class = "N/A";
                        break;
                    case "BL-R0171A-1.3":
                        HW_Version = "1.3";
                        FW_Version = "3.5.0";
                        Class = "N/A";
                        break;
                    case "BL-R0172A-1.2":
                        HW_Version = "1.1";
                        FW_Version = "3.5.0";
                        Class = "N/A";
                        break;
                    case "BL-R0172B-1.1":
                        HW_Version = "1.1";
                        FW_Version = "80.237";
                        Class = "NEMA.78.68.49.56";
                        break;
                    case "BL-R0172B-1.2":
                        HW_Version = "1.1";
                        FW_Version = "80.48";
                        Class = "NEMA.78.68.49.56";
                        break;
                    case "BL-R0172C-1.2":
                        HW_Version = "1.1";
                        FW_Version = "80.45";
                        Class = "NEMA.78.68.49.52";
                        break;
                    case "BL-R0172C-1.3":
                        HW_Version = "1.2";  //ma być 1.2  było 1.1 jak panel 20sztuk
                        FW_Version = "80.47"; //80.47
                        Class = "NEMA.78.68.49.52";
                        break;
                    case "BL-R0172C-1.4":
                        HW_Version = "1.2";
                        FW_Version = "80.47";
                        Class = "NEMA.78.68.49.52";
                        break;
                    case "BL-R0172D-1.0":
                        HW_Version = "1.1";
                        FW_Version = "80.49";
                        Class = "NEMA.78.68.49.48";
                        break;
                    default:
                        HW_Version = "";
                        FW_Version = "";
                        Class = "";
                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Błąd numeru NC!", ncNumber);
                        break;
                }



                foreach (var item in resultGetMeasuredData)
                {
                    if (item.Contains("Part_Number;") && item.Length > 13)
                        MOVEBOARD_CHECKDATA = true;
                    else if (item.Contains("ApTitle;") && item.Length > 10)
                        MOVEBOARD_CHECKDATA1 = true;
                    else if (item.Contains("RegFingerPrint;") && item.Length > 16)
                        MOVEBOARD_CHECKDATA2 = true;
                    else if (item.Contains("HW_Version;" + HW_Version))
                        MOVEBOARD_CHECKDATA3 = true;
                    else if (item.Contains("FW_Version;" + FW_Version))
                        MOVEBOARD_CHECKDATA4 = true;
                    else if (item.Contains("Class;" + Class))
                        MOVEBOARD_CHECKDATA5_1 = true;
                    else if (item.Contains("4.1 MAC;") && item.Length > 10)
                        MOVEBOARD_CHECKDATA5_2 = true;
                    else if (item.Contains("AuthFingerPrint;") && item.Length > 18)
                        MOVEBOARD_CHECKDATA6 = true;
                    else if (item.Contains("AuthCerName;") && item.Length > 14)
                        MOVEBOARD_CHECKDATA7 = true;

                }


                // resultBoxNumber = "0";
                if (resultBoxNumber.Equals("0"))
                {

                    if (MOVEBOARD_CHECKDATA && MOVEBOARD_CHECKDATA1 && MOVEBOARD_CHECKDATA2 && MOVEBOARD_CHECKDATA3 && MOVEBOARD_CHECKDATA4 &&
                        MOVEBOARD_CHECKDATA5_1 && MOVEBOARD_CHECKDATA5_2 && MOVEBOARD_CHECKDATA6 && MOVEBOARD_CHECKDATA7)
                    {
                        ;
                    }
                    else
                    {
                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Niepoprawne parametry odpowiedzi MES, sprawdź historię płytki!", sn);
                        return false;
                    }


                    if (resultHistoryCheckPoint.Equals("True"))
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Niepoprawny proces weryfikacyjny, sprawdź historię płytki!", sn);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Numer seryjny ma już przypisany numer boxu!", sn);
                    return false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Błąd przy sprawdzaniu historii z MES!", sn);
                return false;
            }


        }

        public static int CheckTimeEndStep(DateTime StopDateTime, string sign, TimeSpan timespan)
        {
            try
            {

                    if (sign.Equals(">"))
                    {
                        if (DateTime.Now - StopDateTime > timespan)
                            return 0;
                    }
                    else if (sign.Equals("<"))
                    {
                        if (DateTime.Now - StopDateTime < timespan)
                            return 0;
                    }

                    return 1;//zły bufor
            }
            catch
            {
                MessageBox.Show("Nie udało się obliczyć czasu buforu");

                return 1;
            }

        }

        public static bool CheckAllNumbers(Label labelStatusInfo, Label labelUTR, Button buttonWygenerujBarcode,
            Label labelBarcode1Accepted, Label labelBarcode2Accepted, ref int _counterPiecesInBox, Label labelCountPiecesToBox, Label labelScanInfoPAK, Label labelCountPackedPieces,
            Button buttonIfMesDoneThenPlcDone, Button btnErrorOccursAccept)
        {
            if (!BoxToPackaut.CheckNumbers("TRILLIANT")) //if not all elements of scanned sn List are equal to string received from MES
            {
                ChangeControl.UpdateControl(labelStatusInfo, Color.Red, "Nie wszystkie numery w rolce mają prawidłowy numer boxu w MES! \n Sprawdź ponownie!", true);
                return false;
            }
            else // if everything is OK all numbers are equal/well packed
            {
                //reset all labels for the new box
                ChangeControl.UpdateControl(labelStatusInfo, Color.Green, "Numery sprawdzone, wszystkie OK! \n Dane boxu wyczyszczone, można zaczynać nowy box", true);
                ChangeControl.UpdateControl(labelUTR, "", true);
                ChangeControl.UpdateControl(buttonWygenerujBarcode, "", false);
                ChangeControl.UpdateControl(labelBarcode1Accepted, "", false);
                ChangeControl.UpdateControl(labelBarcode2Accepted, "", false);

                
                ChangeControl.UpdateControl(labelScanInfoPAK, Color.LavenderBlush, "", true);
                ChangeControl.UpdateControl(labelCountPackedPieces, Color.LavenderBlush, "0", true);

                _counterPiecesInBox = 0;  //numbers are OK, so reset the counter for the new box
                ChangeControl.UpdateControl(labelCountPiecesToBox, _counterPiecesInBox.ToString(), true);

                ChangeControl.UpdateControl(buttonIfMesDoneThenPlcDone, Color.Plum, "Sprawdź numery w MES", false);
                ChangeControl.UpdateControl(btnErrorOccursAccept, Color.Fuchsia, "Potwierdż wystąpienie błędów!", false);
                return true;

            }
        }
    }
}
