using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp5.Data;
using WindowsFormsApp5.Properties;
using System.Runtime;

namespace WindowsFormsApp5
{
    public static class BoxToPackaut
    {
        public static List<string> ListOfScannedBarcodesVerified {get; private set; } = new List<string>();

        public static List<string> ListOfScannedBarcodesPacked{ get; private set; } = new List<string>();

        public static List<string> ListNcsOfVerifiedProducts { get; private set; } = new List<string>();

        public static string BoxId = "";

        public static string ContainerJems { get; set; } = "0";
        public static string Nc { get; set; } = "0";


        public static bool PackUnpackSnMultipleJEMS(List<string> list, string cointainer, string packUnpack, bool closeContainer)
        {
            //if (cointainer.Length < 5)
            //    cointainer = "JR00000032";
            try
            {
                JObject body = new JObject {
                { "closeContainer", closeContainer },
                { "containerName", $"{cointainer}" },
                { "resourceName", "TRILLIANT PACKOUT" },
                { "routeName", "TRILLIANT BB GEN4" },
                { "routeStepName", "PACKOUT" },
                { "routeVersion", "1" },
                 { $"wipsTo{packUnpack}", new JArray(list) },
                { "executePrintTrigger", true }
            };
                var pack = ApiJems.ExecuteApiTestBody(ApiJems.Token, "/api/Packout/WipDualPackUnpack", Method.Post, body);
                var packResults = JsonConvert.DeserializeObject<Data.PackUnpack.PackResponse>(pack.Item1.Content);
                var packed = packResults.Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Błąd przy pakowaniu numeru JEMS!!!", ex.Message);
                return false;
            }
            return true;
        }
        public static bool PackUnpackSnJEMS(string sn,string cointainer, string packUnpack, bool closeCointainer, bool executePrintTrigger)
        {
            Form1._myWindow.StopScannerCheckBoard();
            Form1._myWindow.StopScannerForPackout();
            // cointainer = "JR00000032";

            try
            {

                JObject body = new JObject {
                { "closeContainer", closeCointainer },
                { "containerName",cointainer },
                { "resourceName", "TRILLIANT PACKOUT" },
                { "routeName", "TRILLIANT BB GEN4" },
                { "routeStepName", "PACKOUT" },
                { "routeVersion", "1" },
                 { $"wipsTo{packUnpack}", new JArray($"{sn}") },
                { "executePrintTrigger", executePrintTrigger }
            };
                var pack = ApiJems.ExecuteApiTestBody(ApiJems.Token, "/api/Packout/WipDualPackUnpack", Method.Post, body);

                Form1._myWindow.RunScannerCheckBoard();
                Form1._myWindow.RunScannerForPackout();

                if (pack.Item1.ResponseStatus != RestSharp.ResponseStatus.Completed)
                    return false;

                var packResults = JsonConvert.DeserializeObject<Data.PackUnpack.PackResponse>(pack.Item1.Content);
                var packed = packResults.Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Błąd przy pakowaniu numeru JEMS!!!", ex.Message);
                return false;
            }
            return true;
        }
        public static bool CreateCointainerJEMS()
        {
            try
            {
                var date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                JObject body = new JObject {
                                            { "mode", "Assigned"},
                                            { "customerId", 14 },
                                            { "containerType", "TRILLIANT BOX" },
                                            { "creationDate", $"{date}" },   //2024-04-18T11:00:00
                                            { "routeName", "TRILLIANT BB GEN4" },
                                            { "routeVersion", "1" },
                                            { "routeStepName", "PACKOUT" },
                                            { "resourceName", "TRILLIANT PACKOUT" }
                };

                var createCointaner = ApiJems.ExecuteApiTestBody(ApiJems.Token, "/api/containers/CreateContainerByNextNumber", Method.Post, body);// "/api/Packout/WipDualPackUnpack";
                var createCointanerResponse = JsonConvert.DeserializeObject<Data.CreateCointainer>(createCointaner.Item1.Content);
                ContainerJems = createCointanerResponse.Result.ContainerName;
              //  ContainerJems = "JR00000032";

            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Błąd przy tworzeniu konteneru JEMS!!!", ex.Message);
                return false;
            }
            return true;
        }
        public static bool GetContainerNumber(string sn)
        {
            try
            {
                var cointainers = ApiJems.ExecuteApiTestBody(ApiJems.Token, $"/api/containers/containerhierarchy/contentsByWip/external?SiteCode=kwi&WipSerialNumber={sn}&CustomerId=14", Method.Get);
                //ContainerJems = "JR00000058";
                //return true;
                if (cointainers.Item1.IsSuccessful)
                {
                    var cointainersResponse = JsonConvert.DeserializeObject<Data.ContainerBySn>(cointainers.Item1.Content);
                    ContainerJems = cointainersResponse.ContainerNumber;
                    return true;
                }
                else
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Nie udało się pobrać numeru kontenera!!! Nie ma do czego pakować numerów, zrestartuj aplikację", "Błąd Jems Api");
                    ContainerJems = "";
                    return false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Błąd przy tworzeniu konteneru JEMS!!!", ex.Message);
                return false;
            }

        }

        public static bool CheckIsNumberIsPacked(string sn)
        {
            //if (ContainerJems == "JR00000058")
            //    return true;

            var cointainers = ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/containers/containerhierarchy/contentsByWip/external?SiteCode=kwi&WipSerialNumber={sn}&CustomerId=14", Method.Get);

            if (cointainers.Item1.IsSuccessful)
            {
                var cointainersResponse = JsonConvert.DeserializeObject<Data.ContainerBySn>(cointainers.Item1.Content);
                if(ContainerJems == cointainersResponse.ContainerNumber)
                    return true;
                else
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Sprawdź czy ten numer: {sn} jest spakowany do poprawnego kontenera!", "Błąd numeru kontenera");
                    ContainerJems = "";
                    return false;
                }

            }
            else
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Nie udało się pobrać numeru kontenera!!! Numer prawdopodobnie nie jest spakowany", "Błąd Jems Api");
                ContainerJems = "";
                return false;
            }
        }


        public static bool GetOpenCointainer()
        {
            try
            {
                var cointainers = ApiJems.ExecuteApiTest(ApiJems.Token, $"api/containers/GetOpenContainerByType?SiteId=10&CustomerId=14", Method.Get);
                var cointainersResponse = JsonConvert.DeserializeObject<List<Data.GetOpenCointainer>>(cointainers.Item1.Content);
                
                var firstEmptyCointainer = cointainersResponse.Where(x => x.QuantityPacked == 0 && x.ContainerType == Nc).FirstOrDefault();

                if (firstEmptyCointainer != null)
                {
                    ContainerJems = firstEmptyCointainer.ContainerNumber;
                }
                else
                {
                   // ContainerJems = "JR00000032";
                    CreateCointainerJEMS();
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Błąd przy tworzeniu konteneru JEMS!!!", ex.Message);
                return false;
            }
           
            if(ContainerJems.Length > 2) { return true; }
            else { return false; }
        }

        public static void AddSnToPackoutListAndWriteToFile(string snToAdd)
        {
            ListOfScannedBarcodesPacked.Add(snToAdd);
            WriteActualSnToTxt(snToAdd, "packedbarcodes");
        }


        private static int WriteActualSnToTxt(string sn, string fileName)
        {


            // textBox1.Text = sn;

            ////////////////////////string sciezka = ("C:/logi/");      //definiowanieścieżki do której zapisywane logi
            ////////////////////////var date = DateTime.Now;
            ////////////////////////if (Directory.Exists(sciezka))       //sprawdzanie czy sciezka istnieje
            ////////////////////////{
            ////////////////////////    ;
            ////////////////////////}
            ////////////////////////else
            ////////////////////////    System.IO.Directory.CreateDirectory(sciezka); //jeśli nie to ją tworzy

            try
            {
                sn = System.Text.RegularExpressions.Regex.Replace(sn, @"\s+", string.Empty);

                using (StreamWriter sw = new StreamWriter(fileName + @".txt", true))
                {

                    sw.WriteLine(sn);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true}, ex.Message);              
                return 0;
            }

            return 1;
        }

        public static bool CheckIsNcIsSameInBox(string barcode, string ncToVerify)
        {
            if (ListNcsOfVerifiedProducts.Contains(ncToVerify) || ListNcsOfVerifiedProducts.Count == 0)
            {
                ListNcsOfVerifiedProducts.Add(ncToVerify);
                return true;
            }
            else
            {
                ProblemsToReport.WriteWarnigToList($"Sn: {barcode},  Niezgodna wersja NC:{ncToVerify}");
                return false;
            }

        }


        public static int CheckIsBarcodeDuplicatedAndAddToListIfNot(string barcodeToCheck)
        {
            if (ListOfScannedBarcodesVerified.Count >= 300)
                return 2;


            if (!ListOfScannedBarcodesVerified.Contains(barcodeToCheck) )
            {
                ListOfScannedBarcodesVerified.Add(barcodeToCheck);
                WriteActualSnToTxt(barcodeToCheck, "parametry");
                return 0;
            }
            return 1;
                
        }
        public static int CheckIsBarcodeDuplicated(string barcodeToCheck)
        {
            if (ListOfScannedBarcodesVerified.Count >= 300)
                return 2;

            if (!ListOfScannedBarcodesVerified.Contains(barcodeToCheck))
            {
                return 0;
            }
            return 1;

        }
        public static bool IsListCountIsLessEqThan300()
        {

            if (ListOfScannedBarcodesVerified.Count <= 300)
            {
                return true;
            }

            return false;
        }

        public static bool IsListCountIsEqual300()
        {

            if(ListOfScannedBarcodesVerified.Count == 300)
            {
                return true;
            }

            return false;
        }

        public static bool ClearEverything()
        {
            DialogResult d = MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Czy jesteś pewny/a. Utracisz wszystkie dane", "Usuwanie danych", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (d == DialogResult.Yes)
            {
                ListOfScannedBarcodesVerified.Clear();
                ListOfScannedBarcodesPacked.Clear();
                BoxToPackaut.ListNcsOfVerifiedProducts.Clear();
                ClearActualListTxtLog("parametry");
                ClearActualListTxtLog("packedbarcodes");
                return true;

            }
            return false;
            
        }

        public static Tuple<string, string> CreateStringFromList()
        {
            var _barcode1 = "";
            var _barcode2 = "";
            var i = 0;

            foreach (var item in ListOfScannedBarcodesVerified)
            {
                if(i < 150)
                    _barcode1 += $"{item};";
                else
                    _barcode2 += $"{item};";
                i++;
            }

            return Tuple.Create(_barcode1, _barcode2);
        }

        public static int WriteCompletedListToTxtLog(string sn)
        {
            sn = System.Text.RegularExpressions.Regex.Replace(sn, @"\s+", string.Empty);

            // textBox1.Text = sn;

            string sciezka = ("C:/logi/");      //definiowanieścieżki do której zapisywane logi
            var date = DateTime.Now;
            if (Directory.Exists(sciezka))       //sprawdzanie czy sciezka istnieje
            {
                ;
            }
            else
                System.IO.Directory.CreateDirectory(sciezka); //jeśli nie to ją tworzy

            try
            {
                using (StreamWriter sw = new StreamWriter("C:/logi/" + sn + "(" + date.ToString("yyyy-MM-dd HH-mm-ss") + ")" + ".txt"))
                {

                    foreach (var item in ListOfScannedBarcodesVerified)
                    {
                        sw.WriteLine(item);
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, ex.Message);
                return 0;
            }

            return 1;

        }

        public static bool ClearActualListTxtLog(string name)
        {
            try
            {
                using (FileStream fs = File.Open($@"{name}.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    lock (fs)
                    {
                        fs.SetLength(0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, ex.Message);
                return false;
            }

            return true;
        }

        public static int Read_param()
        {
            string sciezka = (@"parametry.txt");
            int i = 0;
            try
            {
                using (StreamReader sr = new StreamReader(sciezka))
                {
                    ListOfScannedBarcodesVerified.Clear();
                    
                    while (sr.Peek() >= 0)
                    {
                        ListOfScannedBarcodesVerified.Add(sr.ReadLine());
                        i++;
                    }
                    sr.Close();
                }

                if(ListOfScannedBarcodesVerified.Count > 0)
                {
                    ListNcsOfVerifiedProducts.Clear();

                    //var webservices = new WebServices();
                    //var nc = webservices.GetNcNumber("TRILLIANT", ListOfScannedBarcodesVerified.FirstOrDefault());

                    //if (!nc.ToUpper().Equals("ERROR"))
                    //    ListNcsOfVerifiedProducts.Add(nc);
                }


            }
            catch (Exception ex)
            {

                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "blad odczytu parametrow:" + ex);
                ListOfScannedBarcodesVerified.Clear();
                return 0;
            }
            return i;
        }
        public static int ReadPackoutSn()
        {
            string sciezka = (@"packedbarcodes.txt");
            int i = 0;
            try
            {
                using (StreamReader sr = new StreamReader(sciezka))
                {
                    ListOfScannedBarcodesPacked.Clear();

                    while (sr.Peek() >= 0)
                    {
                        ListOfScannedBarcodesPacked.Add(sr.ReadLine());
                        i++;
                    }
                    sr.Close();
                }
                Form1.ScanPackout_FisrtTime = true;
                if (ListOfScannedBarcodesPacked.Count >= i && ListOfScannedBarcodesVerified.Count >= i && i > 0)
                    if (ListOfScannedBarcodesPacked[i - 1] == ListOfScannedBarcodesVerified[i - 1])
                    {
                        Form1.ScanOkScannerPackout_FisrtTime = true;
                    }
                return i;

            }
            catch (Exception ex)
            {

                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "blad odczytu parametrow:\n\n" + ex);
                ListOfScannedBarcodesVerified.Clear();
                return 0;
            }


        }

        public static bool CheckNumbers(string client)
        {
            List<string> listToCompare = new List<string>();
            var webservices = new WebServices();
            try
            {
                string first = webservices.GetBoxNumber("TRILLIANT", ListOfScannedBarcodesVerified[0]);
                string last = webservices.GetBoxNumber("TRILLIANT", ListOfScannedBarcodesVerified[ListOfScannedBarcodesVerified.Count - 1]);

                BoxToPackaut.BoxId = last;

                if (first.Equals("0") || last.Equals("0"))
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nie wszystkie numery seryjne mają przypisany numer boxu!");
                    return false;
                }

                if (first == last)
                {
                    var result = webservices.GetSerialNoByBox("TRILLIANT", first);
                    listToCompare = result.Split(';').ToList();

                    var firstNotSecond = ListOfScannedBarcodesVerified.Except(listToCompare).ToList();

                    if (firstNotSecond.Count == 0)
                    {
                        //zrobic zapis do pliku i wyczyscic liste
                        if (WriteCompletedListToTxtLog(BoxToPackaut.BoxId) == 1)
                        {
                            ListOfScannedBarcodesVerified.Clear();
                            ListOfScannedBarcodesPacked.Clear();
                            ListNcsOfVerifiedProducts.Clear();
                            ClearActualListTxtLog("parametry");
                            ClearActualListTxtLog("packedbarcodes");
                            
                        }
                        else
                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nie udało się stworzyć pliku z numerami boxu na C:/logi!");

                        var errorCount = ProblemsToReport.ListOfOccurredProblems.Count;
                        if (errorCount > 0)
                        {
                            //SQL.SendDataOfAllErrorsFromBoxToDb();
                            //try
                            //{
                            //    SQL.UpdateCountOfErrorsInDbBox(errorCount.ToString(), SQL.BoxIdFromDb);
                            //}
                            //catch (Exception)
                            //{
                            //    //SQL.UpdateCountOfErrorsInDbBox("99999", SQL.BoxIdFromDb);
                            //}
                            

                            if (ProblemsToReport.WriteCompletedErrorListToTxtLog(BoxToPackaut.BoxId) == 1)
                            {
                                ;
                            }
                            else
                                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nie udało się stworzyć pliku z błędami boxu na C:/errorlogi!");
                            ProblemsToReport.ClearAllErr();
                        }
                        if (ProblemsToReport.ListOfWarnings.Count > 0)
                        {
                            //SQL.SendDataOfAllWarningsFromBoxToDb();

                            if (ProblemsToReport.WriteCompletedWarningsListToTxtLog(BoxToPackaut.BoxId) == 1)
                            {
                                ;
                            }
                            else
                                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nie udało się stworzyć pliku z błędami boxu na C:/warningslogi!");
                            ProblemsToReport.ClearAllWarn();
                        }
                        return true;
                    }
                    
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Błąd programu:" + ex);
                return false;
            }

        }

        public static int CheckPeelTest()
        {
            var hour = DateTime.Now.Hour;
            var day = DateTime.Now.Day;
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;

            string StartDateToCheck = "";
            string EndDateToCheck = "";

            if (hour >= 6)
            {
                if (hour >= 14) //druga zmina
                {
                    StartDateToCheck = $"{day.ToString("D2")}-{month.ToString("D2")}-{year} 14:00:00";
                    EndDateToCheck = $"{day.ToString("D2")}-{month.ToString("D2")}-{year} 22:00:00";
                }
                else // pierwsza zmiana
                {
                    StartDateToCheck = $"{day.ToString("D2")}-{month.ToString("D2")}-{year} 06:00:00";
                    EndDateToCheck = $"{day.ToString("D2")}-{month.ToString("D2")}-{year} 14:00:00";
                }
            }
            else // trzecia zmiana
            {
                if (hour <= 22)
                    StartDateToCheck = $"{(day - 1).ToString("D2")}-{month.ToString("D2")}-{year} 22:00:00";
                else
                    StartDateToCheck = $"{day.ToString("D2")}-{month.ToString("D2")}-{year} 22:00:00";


                EndDateToCheck = $"{day.ToString("D2")}-{month.ToString("D2")}-{year} 06:00:00";
            }


           return WebServices.GetLineStepReport(StartDateToCheck, EndDateToCheck);
        }

        //public static bool CheckIsBarcodePackedThenClearItFromList()
        //{
        //    //List<string> copyOfListOfScannedBarcodes = new List<string>();
        //    ListOfScannedBarcodes.Clear();
        //    for (int i = 0; i < 301; i++)
        //    {
        //        ListOfScannedBarcodes.Add("NEQA4909856");

        //    }

        //    List<string> copyOfListOfScannedBarcodes = new List<string>(ListOfScannedBarcodes);
        //      var lenghtOfList = ListOfScannedBarcodes.Count;
        //    string boxnumber = "", boxnumberToCompare = "";

        //    WebServices webServices = new WebServices();

        //    for (int i = 0; i < lenghtOfList; i++)
        //    {
        //        boxnumber = webServices.GetBoxNumber("TRILLIANT", copyOfListOfScannedBarcodes[0]);

        //        if (i == 0)
        //            boxnumberToCompare = boxnumber;


        //        if (boxnumber.Length > 2 && boxnumberToCompare.Equals(boxnumber))
        //            copyOfListOfScannedBarcodes.RemoveAt(0);
        //        else
        //        {
        //            if (boxnumber.Equals("0"))
        //                MessageBox.Show($"Numer {copyOfListOfScannedBarcodes[0]} nie ma przypisanego boxu!");
        //            else if (!boxnumberToCompare.Equals(boxnumber))
        //                MessageBox.Show("Nie wszystkie numery z listy mają ten sam numer boxu!");
        //            else
        //                MessageBox.Show("Brak połączenia z MES!");

        //            return false;
        //        }

        //    }
        //    //zrobic zapis do pliku i wyczyscic liste
        //    if (WriteCompletedListToTxtLog(boxnumber) == 1)
        //    {
        //        ListOfScannedBarcodes.Clear();
        //        ClearActualListTxtLog();
        //    }
        //    else
        //        MessageBox.Show("Nie udało się stworzyć pliku z numerami boxu na C:/logi!");



        //    if (ListOfScannedBarcodes.Count > 0)
        //        return false;
        //    else
        //        return true;

        //}

    }
}
