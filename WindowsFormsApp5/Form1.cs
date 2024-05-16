using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using EasyModbus;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using System.Text.RegularExpressions;
using WindowsFormsApp5.mes;
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static WindowsFormsApp5.mes.WebApi;
using WindowsFormsApp5.Data;
using System.Security.Policy;

namespace WindowsFormsApp5
{
    

    public partial class Form1 : Form
    {
        #region fields
        private QrForm frm2, frm;
        private ConfirmProblemOccurrence frm3;
        private SerialPort port;
        private string _lineReadIn;
        private string _poprzedniBarcode;
        private int _counterPiecesInBox = 0;
        private bool _boxDone = false;

        public ScannerForCheckBoards scannerForCheckBoards;
        public ScannerForPackout scannerForPackout;

        // this will prevent cross-threading between the serial port
        // received data thread & the display of that data on the central thread
        private delegate void preventCrossThreading(string x);
        private preventCrossThreading accessControlFromCentralThread;
        public static System.Timers.Timer aTimer;
        private static int _TokenRefreshTime = 8*60*60*2;
        private static int _tickForToken = 0;
        private JEMS jems;
        public static Form1 _myWindow;

        #endregion


        public static bool MoreThan300piecesScanned { get; set; } = false;
        public static bool ScanOkScannerPackout { get; set; } = false;

        public static bool ScanPackout_FisrtTime { get; set; } = false;
        public static bool ScanOkScannerPackout_FisrtTime { get; set; } = false;
        public static bool ScanerBlocked { get; set; } = false;
        public static string ManualBarcodeAfterBlock { get; set; } = "";
        public static bool WipScanDataStatus { get; set; } = false;
        #region timer 
        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(500);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        // bool flaga; 

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            
            //if (_tickForToken <= 0)
            //{
            //    //jems = new JEMS();
            //    //var hukojs = jems.Token;
            //    _tickForToken = _TokenRefreshTime;
            //}
            //_tickForToken -= 1;
            if (scannerForCheckBoards.Port.IsOpen)
            {
                try
                {
                    scannerForCheckBoards.Port.Write("LON\r");
                }
                catch (Exception)
                {
                    ;
                }           
            }
            if (scannerForPackout.Port.IsOpen)
            {
                try
                {
                    scannerForPackout.Port.Write("LON\r");
                }
                catch (Exception)
                {
                    ;
                }
            }
        }
        #endregion

        public Form1()
        {
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            _myWindow = this;

            scannerForPackout = new ScannerForPackout("COM10", textBox3, labelScanOut2,labelCountPackedPieces,labelScanInfoPAK, labelStatusInfo, buttonWygenerujBarcode, BtnErrorOccursAccept);
//4 i 7
           scannerForCheckBoards = new ScannerForCheckBoards("COM11", textBox1, labelScanOut,
                        labelCountVerifiedPiecesToBox, labelStatusInfo, buttonWygenerujBarcode, BtnErrorOccursAccept);


            if (File.Exists(@"parametry.txt"))
            {
                _counterPiecesInBox = BoxToPackaut.Read_param();
                ChangeControl.UpdateControl(labelCountVerifiedPiecesToBox, _counterPiecesInBox.ToString(), true);
            }
            if (File.Exists(@"packedbarcodes.txt"))
            {
                _counterPiecesInBox = BoxToPackaut.ReadPackoutSn();
                ChangeControl.UpdateControl(labelCountPackedPieces, _counterPiecesInBox.ToString(), true);
            }
            if (File.Exists(@"errors.txt"))
            {
                ProblemsToReport.ReadErrors();

                if (ProblemsToReport.ListOfOccurredProblems.Any(s => s.Contains("Partia już zawiera 300 sztuk")))
                    MoreThan300piecesScanned = true;


            }
            if (File.Exists(@"warnings.txt"))
            {
                ProblemsToReport.ReadWarnings();

            }
            if (_counterPiecesInBox == 0)
            {
                try
                {
                //PLC.RunBool("PROGRAM:MainProgram.Mes_App_scan");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, ex.ToString(), "Błąd połączenia z PLC!");
                }
            }
        //    SetTimer();
            var k = new ApiJems();
            var a = k.GetTokenSync("stg");
   //             aTimer.Start();
            //  PLC.InitEvent();
            if (scannerForCheckBoards.Port.IsOpen)
            {
                //aTimer.Enabled = true;
                scannerForCheckBoards.Port.Write("LON\r");
            }
            else
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Port skanera zamknięty!");

            if (scannerForPackout.Port.IsOpen)
            {
                //aTimer.Enabled = true;
                scannerForPackout.Port.Write("LON\r");
            }
            else
                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Port skanera zamknięty!");
        }

        // this is called when the serial port has receive-data for us.

        public void RunScannerCheckBoard()
        {
            scannerForCheckBoards.Port.Write("LON\r");
        }
        public void RunScannerForPackout()
        {
            scannerForPackout.Port.Write("LON\r");
        }
        public void StopScannerCheckBoard()
        {
            scannerForCheckBoards.Port.Write("LOFF\r");
        }
        public void StopScannerForPackout()
        {
            scannerForPackout.Port.Write("LOFF\r");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;

            if(clickedButton.Name.Equals("button1"))
            {
                if (scannerForCheckBoards.Port.IsOpen)
                {
                    //   aTimer.Enabled = true;
                    scannerForCheckBoards.Port.Write("LON\r");
                }
                else
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Port skanera zamknięty!");
            }
            else
            {
                if (scannerForPackout.Port.IsOpen)
                {
                 //   aTimer.Enabled = true;
                    scannerForPackout.Port.Write("LON\r");
                }
                else
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Port skanera zamknięty!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;

            if (clickedButton.Name.Equals("button2"))
            {
                if (scannerForCheckBoards.Port.IsOpen)
                {
                    //  aTimer.Enabled = false;
                    scannerForCheckBoards.Port.Write("LOFF\r");
                }
                else
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Port skanera zamknięty!");
            }
            else
            {
                if (scannerForPackout.Port.IsOpen)
                {
                  //  aTimer.Enabled = false;
                    scannerForPackout.Port.Write("LOFF\r");
                }
                else
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Port skanera zamknięty!");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PLC.WriteBool("PROGRAM:MainProgram.Mes_App_scan");
            ProblemsToReport.WriteToListProblem("Wciśnięto przycisk SKAN_OK");
            //    PLC.ReadDint("PROGRAM:MainProgram.HMI_MainPlacedParts");           
            //    PLC.RunDint("PROGRAM:MainProgram.Mes_App_counter");            
        }

        private void ClearAllData()
        {
            BoxToPackaut.PackUnpackSnMultipleJEMS(BoxToPackaut.ListOfScannedBarcodesPacked, BoxToPackaut.ContainerJems, "UnPack", false);
            BoxToPackaut.ContainerJems = "";
            var result = BoxToPackaut.ClearEverything();
            BoxToPackaut.ContainerJems = "";
            if (result)
            {
                _counterPiecesInBox = 0;
                scannerForPackout._counterPiecesPacked = 0;
                Form1.ScanOkScannerPackout = false;
                _boxDone = false;

                ChangeControl.UpdateControl(labelScanInfoPAK, Color.HotPink, "Wyzerowano wszystkie dane!", true);
                ChangeControl.UpdateControl(labelCountPackedPieces, Color.DeepPink, "0", true);

                ProblemsToReport.ClearAllErr();
                ProblemsToReport.ClearAllWarn();

                ChangeControl.UpdateControl(labelStatusInfo, Color.HotPink, "Wyzerowano wszystkie dane!", true);
                ChangeControl.UpdateControl(buttonWygenerujBarcode, Color.DeepPink, "", false);

                ChangeControl.UpdateControl(labelBarcode1Accepted, Color.LawnGreen, "", false);
                ChangeControl.UpdateControl(labelBarcode2Accepted, Color.LawnGreen, "", false);

                ChangeControl.UpdateControl(buttonIfMesDoneThenPlcDone, Color.Plum, "", false);
                ChangeControl.UpdateControl(BtnErrorOccursAccept, Color.Fuchsia, "", false);

                ChangeControl.UpdateControl(labelCountVerifiedPiecesToBox, SystemColors.Control, _counterPiecesInBox.ToString(), true);

                var res = PLC.DintToZero("PROGRAM:MainProgram.App_Mes_Error_Occurred_int");

                if (!res)
                {
                    res = PLC.DintToZero("PROGRAM:MainProgram.App_Mes_Error_Occurred_int");
                    if (!res)
                    {
                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Błąd przy zerowaniu pamięci błędów maszyny"
                            , "Błąd komunikacji ze sterownikiem");
                        ProblemsToReport.WriteToListProblem("Błąd przy zerowaniu pamięci błędów maszyny");
                    }
                }
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            ClearAllData();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ChangeControl.UpdateControl(labelStatusInfo, Color.HotPink, "Zeskanowano 300 sztuk wygeneruj Barkody! \n Pamiętaj żeby je potwierdzić po zeskanowaniu!", true);
            ChangeControl.UpdateControl(buttonWygenerujBarcode, Color.DeepPink, "Wygeneruj Barkody!", true);

            ChangeControl.UpdateControl(labelBarcode1Accepted, Color.Red, "Zaakceptuj 1. część barkodu!", true);
            ChangeControl.UpdateControl(labelBarcode2Accepted, Color.Red, "Zaakceptuj 2. część barkodu!", true);

            ChangeControl.UpdateControl(labelBarcode1Accepted, Color.LawnGreen, "Zaakceptowano 1. część barkodu!", true);
            ChangeControl.UpdateControl(labelBarcode2Accepted, Color.LawnGreen, "Zaakceptowano 2. część barkodu!", true);
            ChangeControl.UpdateControl(BtnErrorOccursAccept, Color.Fuchsia, "Potwierdź wystąpienie błędów!", true);

            ProblemsToReport.WriteToListProblem("Wciśnięto przycisk odkryj okienka");
            //    ChangeControl.UpdateControl(buttonIfMesDoneThenPlcDone, Color.Plum, "Sprawdź barkody zeskanowanych płytek w MES i wyslij sygnał końca boxu do PLC!", true);

        }
        private void BtnErrorOccursAccept_Click(object sender, EventArgs e)
        {
            frm3 = new ConfirmProblemOccurrence( labelStatusInfo, buttonWygenerujBarcode, BtnErrorOccursAccept, buttonDoneBoxNoBarcode, _counterPiecesInBox);
            frm3.Show();
        }

        private void BarcodeGenerateOnClick()
        {

                var xd = BoxToPackaut.CreateStringFromList();

                //MessageBox.Show(xd.Item1);
                //MessageBox.Show(xd.Item2);
                frm2 = new QrForm(xd.Item2, false, labelBarcode2Accepted, buttonIfMesDoneThenPlcDone);
                frm2.Show();
                frm = new QrForm(xd.Item1, true, labelBarcode1Accepted, buttonIfMesDoneThenPlcDone);
                frm.Show();

                ChangeControl.UpdateControl(labelBarcode1Accepted, Color.Red, "Zaakceptuj 1. część barkodu!", true);
                ChangeControl.UpdateControl(labelBarcode2Accepted, Color.Red, "Zaakceptuj 2. część barkodu!", true);

                ChangeControl.UpdateControl(labelStatusInfo, "Wysyłam do plc sygnał końca boxu! \nPo zeskanowaniu barkodów do MES, sprawdź je za pomocą przycisku: Sprawdź numery w MES! ", true);

                ChangeControl.UpdateControl(buttonIfMesDoneThenPlcDone, Color.Plum, "Sprawdź numery w MES", true);
                Thread.Sleep(250);
                ////////if (!_boxDone)
                ////////{
                ////////    var res = PLC.WriteBool("PROGRAM:MainProgram.Mes_App_Box_Done"); //if everything OK send to the PLC signal to done the box

                ////////    if (!res) //if not display info for technicians what they should check
                ////////    {
                ////////        res = PLC.WriteBool("PROGRAM:MainProgram.Mes_App_Box_Done");
                ////////        if (!res)
                ////////        {
                ////////            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Numery zostały poprawnie spakowane! \n Nastąpił jednak błąd komunikacji ze sterownikiem...", " Wezwij UTR");
                ////////            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Wpiszcie hasło i spróbujcie wyzwolić sygnał przyciskiem końca partii,\n\n jeśli nie pomoże to zróbcie restart aplikacji,\n\n" +
                ////////                "jeśli i to nie pomoże sprawdźcie połączenie pomiędzy maszyną a komputerem: \n przycisk windows -> odpalenie wiersza poleceń (cmd) ->\n" +
                ////////                "wpiszcie polecenie: ping 192.168.1.214");
                ////////        }

                ////////    }
                ////////    else
                ////////        _boxDone = true;
                ////////}

        }

        private void FinishBoxWithoutGenerateBarcodes()
        {
            if (!_boxDone)
            {
                //var res = PLC.WriteBool("PROGRAM:MainProgram.Mes_App_Box_Done"); //if everything OK send to the PLC signal to done the box

                //if (!res) //if not display info for technicians what they should check
                //{
                //    res = PLC.WriteBool("PROGRAM:MainProgram.Mes_App_Box_Done");
                //    if (!res)
                //    {
                //        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Numery zostały poprawnie spakowane! \n Nastąpił jednak błąd komunikacji ze sterownikiem...", " Wezwij UTR");
                //        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Wpiszcie hasło i spróbujcie wyzwolić sygnał przyciskiem końca partii,\n\n jeśli nie pomoże to zróbcie restart aplikacji,\n\n" +
                //            "jeśli i to nie pomoże sprawdźcie połączenie pomiędzy maszyną a komputerem: \n przycisk windows -> odpalenie wiersza poleceń (cmd) ->\n" +
                //            "wpiszcie polecenie: ping 192.168.1.214");
                //    }
                    

                //}
                //if(res)
                //{
                    //if(SQL.ReadCountOfSerialNumbersOfGivenIdBox(SQL.BoxIdFromDb) == 300)
                    //{
                        if (BoxToPackaut.WriteCompletedListToTxtLog(BoxToPackaut.BoxId) == 1)
                        {
                            BoxToPackaut.ListOfScannedBarcodesVerified.Clear();
                            BoxToPackaut.ListOfScannedBarcodesPacked.Clear();
                            BoxToPackaut.ListNcsOfVerifiedProducts.Clear();
                            BoxToPackaut.ClearActualListTxtLog("parametry");
                            BoxToPackaut.ClearActualListTxtLog("packedbarcodes");
                        }
                        else
                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nie udało się stworzyć pliku z numerami boxu na C:/logi!");


                        if (ProblemsToReport.WriteCompletedErrorListToTxtLog(BoxToPackaut.BoxId) == 1)
                        {
                            ;
                        }
                        else
                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nie udało się stworzyć pliku z błędami boxu na C:/errorlogi!");


                        if (ProblemsToReport.WriteCompletedWarningsListToTxtLog(BoxToPackaut.BoxId) == 1)
                        {
                            ;
                        }
                        else
                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nie udało się stworzyć pliku z błędami boxu na C:/warningslogi!");

                    //}
                    //else
                    //{
                    //    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Błąd przy zapisie / odczycie rekordów bazy danych"
                    //        , "Błąd komunikacji z bazą danych");
                    //}                   

                    _counterPiecesInBox = 0;
                    _boxDone = false;

                    ProblemsToReport.ClearAllErr();
                    ProblemsToReport.ClearAllWarn();

                    var res2 = PLC.DintToZero("PROGRAM:MainProgram.App_Mes_Error_Occurred_int");

                    if (!res2)
                    {
                        res2 = PLC.DintToZero("PROGRAM:MainProgram.App_Mes_Error_Occurred_int");
                        if (!res2)
                        {
                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Błąd przy zerowaniu pamięci błędów maszyny"
                                , "Błąd komunikacji ze sterownikiem");
                            ProblemsToReport.WriteToListProblem("Błąd przy zerowaniu pamięci błędów maszyny");
                        }
                    }

                    //reset all labels for the new box
                    ChangeControl.UpdateControl(buttonDoneBoxNoBarcode, Color.DarkMagenta, "Zakończ pakowanie rolki", false);
                    ChangeControl.UpdateControl(labelStatusInfo, Color.Green, "Zweryfikuj rolkę za pomocą nowej aplikacji, żeby wygenerować barkody \n Dane boxu wyczyszczone, można zaczynać nowy box", true);
                    ChangeControl.UpdateControl(labelUTR, "", true);
                    ChangeControl.UpdateControl(buttonWygenerujBarcode, "", false);
                    ChangeControl.UpdateControl(labelBarcode1Accepted, "", false);
                    ChangeControl.UpdateControl(labelBarcode2Accepted, "", false);

                    _counterPiecesInBox = 0;  //numbers are OK, so reset the counter for the new box
                    ChangeControl.UpdateControl(labelCountVerifiedPiecesToBox, _counterPiecesInBox.ToString(), true);
                    ChangeControl.UpdateControl(labelCountPackedPieces, _counterPiecesInBox.ToString(), true);

                    ChangeControl.UpdateControl(buttonIfMesDoneThenPlcDone, Color.Plum, "Sprawdź numery w MES", false);
                    ChangeControl.UpdateControl(BtnErrorOccursAccept, Color.Fuchsia, "Potwierdż wystąpienie błędów!", false);

                //}
                    
            }



        }

        private void button6_Click(object sender, EventArgs e)
        {
            //ChangeControl.UpdateControl(labelStatusInfo, "Wystąpiły błędy podczas pakowania!\nZaakceptuj wyskakujący komunikat!", true);
            //return;

            if (frm3 is null)
            {

                    BarcodeGenerateOnClick();

            }
            else
            {
                if (frm3.Confirmed)
                {
                    BarcodeGenerateOnClick();
                }
                else
                {
                    ChangeControl.UpdateControl(labelStatusInfo, "Wystąpiły błędy podczas pakowania!\nZaakceptuj wyskakujący komunikat!", true);
                }
            }

        }



        private void button6_Click_2(object sender, EventArgs e)
        {
            if (_counterPiecesInBox == 300 || _counterPiecesInBox == 0)
            {
                PLC.WriteBool("PROGRAM:MainProgram.Mes_App_Box_Done");
                ProblemsToReport.WriteToListProblem("Wciśnięto przycisk końca partii");
            }
            else
                MessageBox.Show("Brak 300 płytek, nie udało się wysłać sygnału końca partii");

        }

        private void buttonIfMesDoneThenPlcDone_Click(object sender, EventArgs e)
        {
            try
            {
                if (frm2 != null && frm != null)
                    if (!frm2.Confirmed)
                    {
                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Zaakceptuj drugą część barkodu!");
                    }
                    else
                    {
                        if (!frm.Confirmed)
                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Zaakceptuj pierwszą część barkodu!");
                        else
                        {
                            ChangeControl.UpdateControl(labelStatusInfo, Color.Plum, "Barkody poprawnie zaakceptowane, sprawdzam numery w MES!", true);
                            Thread.Sleep(100);
                            var res = CheckHistory.CheckAllNumbers(labelStatusInfo, labelUTR, buttonWygenerujBarcode, labelBarcode1Accepted,
                                        labelBarcode2Accepted, ref _counterPiecesInBox, labelCountVerifiedPiecesToBox, labelScanInfoPAK, labelCountPackedPieces, buttonIfMesDoneThenPlcDone, BtnErrorOccursAccept);
                            if (res)
                            {
                                var result = PLC.DintToZero("PROGRAM:MainProgram.App_Mes_Error_Occurred_int");

                                if (!result)
                                {
                                    result = PLC.DintToZero("PROGRAM:MainProgram.App_Mes_Error_Occurred_int");
                                    if (!result)
                                    {
                                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Błąd przy zerowaniu pamięci błędów maszyny"
                                            , "Błąd komunikacji ze sterownikiem");
                                        ProblemsToReport.WriteToListProblem("Błąd przy zerowaniu pamięci błędów maszyny");
                                    }
                                }
                                _boxDone = false;

                                textBox2.Text = string.Empty;
                                textBox2.Visible = true;
                            }
                                
                        }

                    }
                else
                {
                    ChangeControl.UpdateControl(labelStatusInfo, Color.Red, "Wpierw wygeneruj i zaakceptuj barkody!", true);
                    MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Wpierw wygeneruj i zaakceptuj barkody!");
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Błąd programu:" + ex);
            }


        }

        private void button7_Click(object sender, EventArgs e)
        {
            var frm3 = new ListOfScannedSn();
            frm3.Show();
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            //var K = BoxToPackaut.CheckPeelTest();
            
            var kuk = new ApiJems();
            
            var a = kuk.GetTokenSync("stg");

            var tt = ApiJems.Token;


            BoxToPackaut.PackUnpackSnJEMS("NEQA6482063", "JR00000032", "UnPack", false);

            //   var task1 = Task.Run(() => WebApi.CallApiWithCookie());

            //  task1.Wait();
            //    var huiasd = CheckHistory.checkSnHistoryJEMS("NEQA8660327");


            //   var kupaosa = BoxToPackaut.GetOpenCointainer();

            //            BoxToPackaut.PackUnpackSnJEMS("NEQA6481051", "JR00000032", "Pack", false);

            //            //   BoxToPackaut.CreateCointainerJEMS();

            //            //     BoxToPackaut.PackUnpackSnJEMS("NEQA8660327", "JR00000017", "UnPack", false);

            //            //var toTest = new List<string>();
            //            //toTest.Add("NEQA8660327");
            //            //toTest.Add("NEQA8660326");

            //            List<string> toTest = new List<string>
            //{
            //    "NEQA8660011", "NEQA8660012", "NEQA8660013", "NEQA8660014", "NEQA8660015", "NEQA8660016", "NEQA8660017", "NEQA8660018", "NEQA8660019", "NEQA8660020",
            //    "NEQA8660021", "NEQA8660022", "NEQA8660023", "NEQA8660024", "NEQA8660025", "NEQA8660026", "NEQA8660027", "NEQA8660028", "NEQA8660029", "NEQA8660030",
            //    "NEQA8660031", "NEQA8660032", "NEQA8660033", "NEQA8660034", "NEQA8660035", "NEQA8660036", "NEQA8660037", "NEQA8660038", "NEQA8660039", "NEQA8660040",
            //    "NEQA8660041", "NEQA8660042", "NEQA8660043", "NEQA8660044", "NEQA8660045", "NEQA8660046", "NEQA8660047", "NEQA8660048", "NEQA8660049", "NEQA8660050",
            //    "NEQA8660051", "NEQA8660052", "NEQA8660053", "NEQA8660054", "NEQA8660055", "NEQA8660056", "NEQA8660057", "NEQA8660058", "NEQA8660059", "NEQA8660060",
            //    "NEQA8660061", "NEQA8660062", "NEQA8660063", "NEQA8660064", "NEQA8660065", "NEQA8660066", "NEQA8660067", "NEQA8660068", "NEQA8660069", "NEQA8660070",
            //    "NEQA8660071", "NEQA8660072", "NEQA8660073", "NEQA8660074", "NEQA8660075", "NEQA8660076", "NEQA8660077", "NEQA8660078", "NEQA8660079", "NEQA8660080",
            //    "NEQA8660081", "NEQA8660082", "NEQA8660083", "NEQA8660084", "NEQA8660085", "NEQA8660086", "NEQA8660087", "NEQA8660088", "NEQA8660089", "NEQA8660090",
            //    "NEQA8660091", "NEQA8660092", "NEQA8660093", "NEQA8660094", "NEQA8660095", "NEQA8660096", "NEQA8660097", "NEQA8660098", "NEQA8660099", "NEQA8660100",
            //    "NEQA8660101", "NEQA8660102", "NEQA8660103", "NEQA8660104", "NEQA8660105", "NEQA8660106", "NEQA8660107", "NEQA8660108", "NEQA8660109", "NEQA8660110",
            //    "NEQA8660111", "NEQA8660112", "NEQA8660113", "NEQA8660114", "NEQA8660115", "NEQA8660116", "NEQA8660117", "NEQA8660118", "NEQA8660119", "NEQA8660120",
            //    "NEQA8660121", "NEQA8660122", "NEQA8660123", "NEQA8660124", "NEQA8660125", "NEQA8660126", "NEQA8660127", "NEQA8660128", "NEQA8660129", "NEQA8660130",
            //    "NEQA8660131", "NEQA8660132", "NEQA8660133", "NEQA8660134", "NEQA8660135", "NEQA8660136", "NEQA8660137", "NEQA8660138", "NEQA8660139", "NEQA8660140",
            //    "NEQA8660141", "NEQA8660142", "NEQA8660143", "NEQA8660144", "NEQA8660145", "NEQA8660146", "NEQA8660147", "NEQA8660148", "NEQA8660149", "NEQA8660150",
            //    "NEQA8660151", "NEQA8660152", "NEQA8660153", "NEQA8660154", "NEQA8660155", "NEQA8660156", "NEQA8660157", "NEQA8660158", "NEQA8660159", "NEQA8660160",
            //    "NEQA8660161", "NEQA8660162", "NEQA8660163", "NEQA8660164", "NEQA8660165", "NEQA8660166", "NEQA8660167", "NEQA8660168", "NEQA8660169", "NEQA8660170",
            //    "NEQA8660171", "NEQA8660172", "NEQA8660173", "NEQA8660174", "NEQA8660175", "NEQA8660176", "NEQA8660177", "NEQA8660178", "NEQA8660179", "NEQA8660180",
            //    "NEQA8660181", "NEQA8660182", "NEQA8660183", "NEQA8660184", "NEQA8660185", "NEQA8660186", "NEQA8660187", "NEQA8660188", "NEQA8660189", "NEQA8660190",
            //    "NEQA8660191", "NEQA8660192", "NEQA8660193", "NEQA8660194", "NEQA8660195", "NEQA8660196", "NEQA8660197", "NEQA8660198", "NEQA8660199", "NEQA8660200",
            //    "NEQA8660201", "NEQA8660202", "NEQA8660203", "NEQA8660204", "NEQA8660205", "NEQA8660206", "NEQA8660207", "NEQA8660208", "NEQA8660209", "NEQA8660210",
            //    "NEQA8660211", "NEQA8660212", "NEQA8660213", "NEQA8660214", "NEQA8660215", "NEQA8660216", "NEQA8660217", "NEQA8660218", "NEQA8660219", "NEQA8660220",
            //    "NEQA8660221", "NEQA8660222", "NEQA8660223", "NEQA8660224", "NEQA8660225", "NEQA8660226", "NEQA8660227", "NEQA8660228", "NEQA8660229", "NEQA8660230",
            //    "NEQA8660231", "NEQA8660232", "NEQA8660233", "NEQA8660234", "NEQA8660235", "NEQA8660236", "NEQA8660237", "NEQA8660238", "NEQA8660239", "NEQA8660240",
            //    "NEQA8660241", "NEQA8660242", "NEQA8660243", "NEQA8660244", "NEQA8660245", "NEQA8660246", "NEQA8660247", "NEQA8660248", "NEQA8660249", "NEQA8660250",
            //    "NEQA8660251", "NEQA8660252", "NEQA8660253", "NEQA8660254", "NEQA8660255", "NEQA8660256", "NEQA8660257", "NEQA8660258", "NEQA8660259", "NEQA8660260",
            //    "NEQA8660261", "NEQA8660262", "NEQA8660263", "NEQA8660264", "NEQA8660265", "NEQA8660266", "NEQA8660267", "NEQA8660268", "NEQA8660269", "NEQA8660270",
            //    "NEQA8660271", "NEQA8660272", "NEQA8660273", "NEQA8660274", "NEQA8660275", "NEQA8660276", "NEQA8660277", "NEQA8660278", "NEQA8660279", "NEQA8660280",
            //    "NEQA8660281", "NEQA8660282", "NEQA8660283", "NEQA8660284", "NEQA8660285", "NEQA8660286", "NEQA8660287", "NEQA8660288", "NEQA8660289", "NEQA8660290",
            //    "NEQA8660291", "NEQA8660292", "NEQA8660293", "NEQA8660294", "NEQA8660295", "NEQA8660296", "NEQA8660297", "NEQA8660298", "NEQA8660299", "NEQA8660300",
            //    "NEQA8660301", "NEQA8660302", "NEQA8660303", "NEQA8660304", "NEQA8660305", "NEQA8660306", "NEQA8660307", "NEQA8660308", "NEQA8660309", "NEQA8660310"
            //};


            //            List<string> list1 = new List<string>
            //{
            //    "NEQA8660011", "NEQA8660012", "NEQA8660013", "NEQA8660014", "NEQA8660015", "NEQA8660016", "NEQA8660017", "NEQA8660018", "NEQA8660019", "NEQA8660020",
            //    "NEQA8660021", "NEQA8660022", "NEQA8660023", "NEQA8660024", "NEQA8660025", "NEQA8660026", "NEQA8660027", "NEQA8660028", "NEQA8660029", "NEQA8660030",
            //    "NEQA8660031", "NEQA8660032", "NEQA8660033", "NEQA8660034", "NEQA8660035", "NEQA8660036", "NEQA8660037", "NEQA8660038", "NEQA8660039", "NEQA8660040",
            //    "NEQA8660041", "NEQA8660042", "NEQA8660043", "NEQA8660044", "NEQA8660045", "NEQA8660046", "NEQA8660047", "NEQA8660048", "NEQA8660049", "NEQA8660050",
            //    "NEQA8660051", "NEQA8660052", "NEQA8660053", "NEQA8660054", "NEQA8660055", "NEQA8660056", "NEQA8660057", "NEQA8660058", "NEQA8660059", "NEQA8660060",
            //    "NEQA8660061", "NEQA8660062", "NEQA8660063", "NEQA8660064", "NEQA8660065", "NEQA8660066", "NEQA8660067", "NEQA8660068", "NEQA8660069", "NEQA8660070",
            //    "NEQA8660071", "NEQA8660072", "NEQA8660073", "NEQA8660074", "NEQA8660075", "NEQA8660076", "NEQA8660077", "NEQA8660078", "NEQA8660079", "NEQA8660080",
            //    "NEQA8660081", "NEQA8660082", "NEQA8660083", "NEQA8660084", "NEQA8660085", "NEQA8660086", "NEQA8660087", "NEQA8660088", "NEQA8660089", "NEQA8660090",
            //    "NEQA8660091", "NEQA8660092", "NEQA8660093", "NEQA8660094", "NEQA8660095", "NEQA8660096", "NEQA8660097", "NEQA8660098", "NEQA8660099", "NEQA8660100",
            //    "NEQA8660101", "NEQA8660102", "NEQA8660103", "NEQA8660104", "NEQA8660105", "NEQA8660106", "NEQA8660107", "NEQA8660108", "NEQA8660109", "NEQA8660110"
            //};

            //            List<string> list2 = new List<string>
            //{
            //    "NEQA8660111", "NEQA8660112", "NEQA8660113", "NEQA8660114", "NEQA8660115", "NEQA8660116", "NEQA8660117", "NEQA8660118", "NEQA8660119", "NEQA8660120",
            //    "NEQA8660121", "NEQA8660122", "NEQA8660123", "NEQA8660124", "NEQA8660125", "NEQA8660126", "NEQA8660127", "NEQA8660128", "NEQA8660129", "NEQA8660130",
            //    "NEQA8660131", "NEQA8660132", "NEQA8660133", "NEQA8660134", "NEQA8660135", "NEQA8660136", "NEQA8660137", "NEQA8660138", "NEQA8660139", "NEQA8660140",
            //    "NEQA8660141", "NEQA8660142", "NEQA8660143", "NEQA8660144", "NEQA8660145", "NEQA8660146", "NEQA8660147", "NEQA8660148", "NEQA8660149", "NEQA8660150",
            //    "NEQA8660151", "NEQA8660152", "NEQA8660153", "NEQA8660154", "NEQA8660155", "NEQA8660156", "NEQA8660157", "NEQA8660158", "NEQA8660159", "NEQA8660160",
            //    "NEQA8660161", "NEQA8660162", "NEQA8660163", "NEQA8660164", "NEQA8660165", "NEQA8660166", "NEQA8660167", "NEQA8660168", "NEQA8660169", "NEQA8660170",
            //    "NEQA8660171", "NEQA8660172", "NEQA8660173", "NEQA8660174", "NEQA8660175", "NEQA8660176", "NEQA8660177", "NEQA8660178", "NEQA8660179", "NEQA8660180",
            //    "NEQA8660181", "NEQA8660182", "NEQA8660183", "NEQA8660184", "NEQA8660185", "NEQA8660186", "NEQA8660187", "NEQA8660188", "NEQA8660189", "NEQA8660190",
            //    "NEQA8660191", "NEQA8660192", "NEQA8660193", "NEQA8660194", "NEQA8660195", "NEQA8660196", "NEQA8660197", "NEQA8660198", "NEQA8660199", "NEQA8660200"
            //};

            //            List<string> list3 = new List<string>
            //{
            //    "NEQA8660011", "NEQA8660212", "NEQA8660213", "NEQA8660214", "NEQA8660215", "NEQA8660216", "NEQA8660217", "NEQA8660218", "NEQA8660219", "NEQA8660220",
            //    "NEQA8660221", "NEQA8660222", "NEQA8660223", "NEQA8660224", "NEQA8660225", "NEQA8660226", "NEQA8660227", "NEQA8660228", "NEQA8660229", "NEQA8660230",
            //    "NEQA8660231", "NEQA8660232", "NEQA8660233", "NEQA8660234", "NEQA8660235", "NEQA8660236", "NEQA8660237", "NEQA8660238", "NEQA8660239", "NEQA8660240",
            //    "NEQA8660241", "NEQA8660242", "NEQA8660243", "NEQA8660244", "NEQA8660245", "NEQA8660246", "NEQA8660247", "NEQA8660248", "NEQA8660249", "NEQA8660250",
            //    "NEQA8660251", "NEQA8660252", "NEQA8660253", "NEQA8660254", "NEQA8660255", "NEQA8660256", "NEQA8660257", "NEQA8660258", "NEQA8660259", "NEQA8660260",
            //    "NEQA8660261", "NEQA8660262", "NEQA8660263", "NEQA8660264", "NEQA8660265", "NEQA8660266", "NEQA8660267", "NEQA8660268", "NEQA8660269", "NEQA8660270",
            //    "NEQA8660271", "NEQA8660272", "NEQA8660273", "NEQA8660274", "NEQA8660275", "NEQA8660276", "NEQA8660277", "NEQA8660278", "NEQA8660279", "NEQA8660280",
            //    "NEQA8660281", "NEQA8660282", "NEQA8660283", "NEQA8660284", "NEQA8660285", "NEQA8660286", "NEQA8660287", "NEQA8660288", "NEQA8660289", "NEQA8660290",
            //    "NEQA8660291", "NEQA8660292", "NEQA8660293", "NEQA8660294", "NEQA8660295", "NEQA8660296", "NEQA8660297", "NEQA8660298", "NEQA8660299", "NEQA8660300",
            //    "NEQA8660301", "NEQA8660302", "NEQA8660303", "NEQA8660304", "NEQA8660305", "NEQA8660306", "NEQA8660307", "NEQA8660308", "NEQA8660309", "NEQA8660310"
            //};
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(toTest.GetRange(0, 100), "JR00000017", "Pack", false);
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(toTest.GetRange(100, 100), "JR00000017", "Pack", false);
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(toTest.GetRange(200, 100), "JR00000017", "Pack", false);
            //            BoxToPackaut.PackUnpackSnJEMS("NEQA8660327", "JR00000017", "UnPack", false);

            //            BoxToPackaut.PackUnpackSnMultipleJEMS(toTest.GetRange(0, 100), "JR00000017", "UnPack", false);
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(toTest.GetRange(100, 100), "JR00000017", "UnPack", false);
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(toTest.GetRange(200, 100), "JR00000017", "UnPack", false);

            //            BoxToPackaut.PackUnpackSnMultipleJEMS(list1, "JR00000017", "Pack", false);
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(list2, "JR00000017", "Pack", false);
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(list3, "JR00000017", "Pack", false);


            //            BoxToPackaut.PackUnpackSnMultipleJEMS(list1, "JR00000017", "UnPack", false);
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(list2, "JR00000017", "UnPack", false);
            //            BoxToPackaut.PackUnpackSnMultipleJEMS(list3, "JR00000017", "UnPack", false);
            //            //  BoxToPackaut.PackUnpackSnJEMS("NEQA8660327", "JR00000017", "Pack", false);



            //            CheckHistory.checkSnHistoryJEMS("NEQA8660326");
            //            //string a = "PLKWIM0T19CMD01";
            //            //string b = "4097148";
            //            //string c = "stg";

            //            //JObject body = new JObject {
            //            //                                { "mode", "Assigned"},
            //            //                                { "customerId", 14 },
            //            //                                { "containerType", "TRILLIANT BOX" },
            //            //                                { "creationDate", "2024-04-18T11:00:00" },
            //            //                                { "routeName", "TRILLIANT BB GEN4" },
            //            //                                { "routeVersion", "1" },
            //            //                                { "routeStepName", "PACKOUT" },
            //            //                                { "resourceName", "TRILLIANT PACKOUT" }
            //            //                          };

            //            //      var createCointaner = ApiJems.ExecuteApiTestBody(hsh, "/api/containers/CreateContainerByNextNumber", Method.Post, body);   "/api/Packout/WipDualPackUnpack"



            //        var WipId = ApiJems.ExecuteApiTestBody(ApiJems.Token, "api/Wips/GetWipIdBySerialNumber?SiteName=Kwidzyn&CustomerName=TRILLIANT&SerialNumber=NEQA8660326", Method.Get);

            //            var settings = new JsonSerializerSettings
            //            {
            //                NullValueHandling = NullValueHandling.Ignore,
            //                MissingMemberHandling = MissingMemberHandling.Ignore
            //            };

            //            var myobjList = JsonConvert.DeserializeObject<List<Data.WipIdResults>>(WipId.Item1.Content, settings);



            //            var operationHistories = ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/Wips/{myobjList[0].WipId}/OperationHistories", Method.Get);

            //             var example = JsonConvert.DeserializeObject<Example>(operationHistories.Item1.Content);
            //            var koko = example.Wips.FirstOrDefault().OperationHistories;


            //            string sn = "NEQA8660326";
            //            var attribiutes = ApiJems.ExecuteApiTest(ApiJems.Token, $"/api/Wips/attributes?SiteName=Kwidzyn&SerialNumber={sn}", Method.Get);
            //            var attributesResponse = JsonConvert.DeserializeObject<Data.Attributes.attributes>(attribiutes.Item1.Content);
            //            //            var myobjList2 = JsonConvert.DeserializeObject<Wip>(operationHistories.Item1.Content, settings);

            //            var okToStart = ApiJems.ExecuteApiTest(ApiJems.Token, "/api/Wips/4097148/oktostart?resourceName=TRILLIANT PACKOUT", Method.Get);
            //            var okToStartResponse = JsonConvert.DeserializeObject<Data.OkToStart.wips>(okToStart.Item1.Content);


            //            JObject body = new JObject {
            //                { "closeContainer", true },
            //                { "containerName", "JR00000004" },
            //                { "resourceName", "TRILLIANT PACKOUT" },
            //                { "routeName", "TRILLIANT BB GEN4" },
            //                { "routeStepName", "PACKOUT" },
            //                { "routeVersion", "1" },
            //                 { "wipsToPack", new JArray("NEQA8660013","NEQA8660011") },
            //                { "executePrintTrigger", true }
            //            };
            //            var pack = ApiJems.ExecuteApiTestBody(ApiJems.Token, "/api/Packout/WipDualPackUnpack", Method.Post, body);

            //            //   var hasidhasi = ApiJems.ExecuteApiTest(hsh, "/api/Wips/4097148/oktostart?resourceName=PLKWIM0T19CMD01", Method.Get);  ///api/Wips/GetWipIdBySerialNumber?SiteName=Kwidzyn&CustomerName=TRILLIANT&SerialNumber=NEQA8660012
            //            var JEDEN = ApiJems.ExecuteApiTest(ApiJems.Token, "api/Wips/GetWipIdBySerialNumber?SiteName=Kwidzyn&CustomerName=TRILLIANT&SerialNumber=NEQA8660327", Method.Get);
            //            var dwa = ApiJems.ExecuteApiTest(ApiJems.Token, "/api/Wips/4317747/OperationHistories?resourceName=TRILLIANT PACKOUT", Method.Get);
            //     //       var hasidhasi2 = ApiJems.ExecuteApiTest(hsh, "api/wips/oktostart?serialNumber=NEQA8660012&resourceName=TRILLIANT PACKOUT", Method.Get);   //24214-C521240001
            //      //      var atribiutess = ApiJems.ExecuteApiTest(ApiJems.Token, "/api/Wips/attributes?SiteName=Kwidzyn&SerialNumber=NEQA8660327", Method.Get);   //24214-C521240001
            //   //         var operationHistories = ApiJems.ExecuteApiTest(ApiJems.Token, "api/Wips/OperationHistories?SiteName=Kwidzyn&SerialNumber=NEQA8660012", Method.Get);

            //      //  https://kwi-stg.jemsms.corp.jabil.org/api-external-api/api/containers/CreateContainerByNextNumber

            //            //   var hasidhasi8 = ApiJems.ExecuteApiTest(hsh, "api/wips/oktostart?serialNumber=24214-C521240001&resourceName=HVT01", Method.Get);   //24214-C521240001
            //            // var hasidhasi3 = ApiJems.ExecuteApiTest(hsh, "/api/Wips/attributes?SiteName=Kwidzyn&SerialNumber=24214-C521240001", Method.Get);   //24214-C521240001
            //            //  var hasidhasi4 = ApiJems.ExecuteApiTest(hsh, "api/Wips/OperationHistories?SiteName=Kwidzyn&SerialNumber=24214-C521240001", Method.Get);


            //            //$"api/wips/oktostart?serialNumber={serialNumber}&resourceName={resourceName}"
            //            //   http://[servername]/api-external-api/api/Wips/OperationHistories?SiteName=S&CustomerName=C&SerialNumber=W&MaterialName=M

            //            //        var huoaj = ApiJems.ExecuteApiTest(hsh, "/api/Wips/4097148/OperationHistories?resourceName=PLKWIM0T19CMD01", Method.Get);

            //            //--->    var huasdhu = ApiJems.JEMS_WipStart(hsh, b, a, c);

            //            //    Task.Run(() => WebApi.LinkByLinkStation()).Wait();

            //            //CheckHistory.checkSnHistory("NEQA2859111");
            //            //           MessageBox.Show(new Form { TopLevel = true, TopMost = true }, $"Niezgodność wersji NC: BL-R0172C-1.2! Sprawdź numery!!!", "NEQA213714102");

            //            //var hui = new WebServices();

            //            //hui.GetNcNumber("TRILLIANT", "NEQA5353882");
            //            //  var kak = CheckHistory.checkSnHistory("NEQA5159002");



            //            // var version = $"{libplctag.LibPlcTag.VersionMajor}.{libplctag.LibPlcTag.VersionMinor}.{libplctag.LibPlcTag.VersionPatch}";
            //            ////    Form1.ScanOkScannerPackout = true;
            //            //  SQL.ReadCountOfSerialNumbersOfGivenIdBox("19");
            //            ////SQL.CreateBoxInDb();
            //            ////SQL.ReadCreatedBoxInDb();
            //            ////SQL.SendDataOfCompletedBoxToDb();
            //            ////SQL.SendDataOfAllErrorsFromBoxToDb();
            //            ////SQL.SendDataOfAllWarningsFromBoxToDb();
            //            ///
            //            //ChangeControl.UpdateControl(BtnErrorOccursAccept, Color.Red, "Wpierw wygeneruj i zaakceptuj barkody!", true);
            //            //          SQL.UpdateCountOfErrorsInDbBox("695684","19");
            //            //      ChangeControl.UpdateControl(labelStatusInfo, Color.Red, $"Wystąpił błąd historii produktu! \n1.Sprawdź wyskakujące komunikaty!\n2.Zeskanuj skanerem ręcznym nową płytkę!\n3.Odłóż ją na miejsce odczytu skanera automatycznego!", true);
            //            //int uj = 0;
            //            //PLC.ReadDint("HMI_MainPlacedParts", out uj);
            //            //PROGRAM:MainProgram.App_Mes_Error_Occurred_int  TM50ppWG
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if(textBox2.Text.ToUpper().Equals("RTU"))
            {
                ChangeControl.UpdateControl(labelUTR, "", false);
                textBox2.Visible = false;
            }        
            else
                ChangeControl.UpdateControl(labelUTR, "", true);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var fail = true;
                var barcodeManual = Regex.Replace(textBox1.Text, @"\s+", string.Empty);

                if(barcodeManual.Length == 11)
                {
                    if(CheckHistory.checkSnHistory(barcodeManual))
                    {
                        if(BoxToPackaut.CheckIsBarcodeDuplicated(barcodeManual) == 0)
                        {
                            ManualBarcodeAfterBlock = barcodeManual;
                            fail = false;
                        }
                    }

                }
                if(fail)
                {
                    textBox1.Text = String.Empty;
                    textBox1.Select();                   
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            FinishBoxWithoutGenerateBarcodes();
        }

        public void OnApplicationExit(object sender, EventArgs e)
        {
    //        aTimer.Stop();
            Thread.Sleep(100);

            if (scannerForCheckBoards.Port.IsOpen)
            {
                scannerForCheckBoards.Port.Write("LOFF\r");
            }
            if (scannerForPackout.Port.IsOpen)
            {
                scannerForPackout.Port.Write("LOFF\r");
            }

            if (_counterPiecesInBox > 0)
                ProblemsToReport.WriteToListProblem("Zamknięto aplikację podczas pakowania");
            //    Thread.Sleep(10000);
            //port.Close();
            System.Windows.Forms.Application.Exit();
            //Application.
        }

 
    }
}
