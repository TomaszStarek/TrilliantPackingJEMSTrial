using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp5
{
    public class ScannerForPackout : Scanner
    {
        private Label _labelScanOut2;
        private Label _labelCountVerifiedPieces;
        private Label _labelScanInfoPAK;
        private Label _labelStatusInfo;
        private Button _buttonWygenerujBarcode;
        private Button _btnErrorOccursAccept;

        private string _lineReadIn = "";
        private string _poprzedniBarcode;
        public int _counterPiecesPacked = 0;
      //  public bool ScanOK { get; set; } = false;
        public ScannerForPackout(string com, TextBox textBox, Label labelScanOut2, Label labelCountVerifiedPieces, Label labelScanInfoPAK,
            Label labelStatusInfo, Button buttonWygenerujBarcode, Button btnErrorOccursAccept) : base(com, textBox)
        {
            _labelScanOut2 = labelScanOut2;
            _labelCountVerifiedPieces = labelCountVerifiedPieces;
            _labelScanInfoPAK = labelScanInfoPAK;
            _labelStatusInfo = labelStatusInfo;
            _buttonWygenerujBarcode = buttonWygenerujBarcode;
            _btnErrorOccursAccept = btnErrorOccursAccept;

        }
        public override void port_DataReceived(object sender, SerialDataReceivedEventArgs rcvdData)
        {
            while (Port.BytesToRead > 0)
            {
                _lineReadIn += Port.ReadExisting();
                Thread.Sleep(25);
            }

            _lineReadIn = Regex.Replace(_lineReadIn, @"\s+", string.Empty);
            ChangeControl.UpdateControl(_labelScanOut2, _lineReadIn, true);

            if (_lineReadIn.Length > 11)
                _lineReadIn = _lineReadIn.Remove(11);
            
            var test = Form1.ScanOkScannerPackout;

            if(Form1.ScanPackout_FisrtTime) //ScanPackout_FisrtTime
            {
                if(BoxToPackaut.ListOfScannedBarcodesPacked.Count > 0)
                {
                    if (BoxToPackaut.ListOfScannedBarcodesPacked.Last().Equals(_lineReadIn) &&
                         Form1.ScanOkScannerPackout_FisrtTime)
                    {
                        if(BoxToPackaut.CheckIsNumberIsPacked(_lineReadIn))
                        {

                            if(BoxToPackaut.ListOfScannedBarcodesPacked.Count > 294 && BoxToPackaut.ListOfScannedBarcodesPacked.Count <= 300
                        && BoxToPackaut.ListOfScannedBarcodesVerified.Count == 300)
                            {
                                ChangeControl.UpdateControl(_labelScanInfoPAK, Color.LawnGreen, $"Zeskanowano poprawnie barkod: {_lineReadIn}", true);
                                try
                                {
                                    var res = PLC.WriteBool("PROGRAM:MainProgram.Mes_App_scan");
                                    if (!res)
                                    {
                                        res = PLC.WriteBool("PROGRAM:MainProgram.Mes_App_scan");

                                        if (!res)
                                        {
                                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nastąpił błąd komunikacji ze sterownikiem...", " Wezwij UTR");
                                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Wpiszcie hasło i spróbujcie wyzwolić sygnał przyciskiem SKAN_OK,\n\n jeśli nie pomoże to zróbcie restart aplikacji, spróbujcie wyzwolić sygnał przyciskiem SKAN_OK,\n\n" +
                                                "jeśli i to nie pomoże sprawdźcie połączenie pomiędzy maszyną a komputerem: \n przycisk windows -> odpalenie wiersza poleceń (cmd) ->\n" +
                                                "wpiszcie polecenie: ping 192.168.1.214");
                                        }

                                    }

                                }
                                catch (Exception)
                                {

                                    MessageBox.Show("Pomoże?");
                                }
                                Form1.ScanOkScannerPackout_FisrtTime = false;
                                return;
                            }

                            Form1.ScanOkScannerPackout = true;
                            Form1.ScanOkScannerPackout_FisrtTime = false;



                        }
                      //  Form1._myWindow.RunScannerCheckBoard();
                     //   _poprzedniBarcode = _lineReadIn;
                        return;
                    }
                    else
                        ChangeControl.UpdateControl(_labelScanInfoPAK, Color.Red, $"{_lineReadIn} NOK / nie spełnia wymagań", true);
                }
                Form1.ScanPackout_FisrtTime = false;
                return;

            }



            if (_lineReadIn.Length == 11 && !_lineReadIn.Contains("ER") && (!_lineReadIn.Equals(_poprzedniBarcode) )) //|| !Form1.ScanOkScannerPackout 
            {
                var checkSnCanBeAddedToPackedList = PackoutMethodsForCheckSn.CheckSnCanBeAddedToPackedList(_lineReadIn);
                if (checkSnCanBeAddedToPackedList == 0)
                {
                    var isItLastElement = false;
                    if (BoxToPackaut.ListOfScannedBarcodesPacked.Count > 0)
                        isItLastElement = BoxToPackaut.ListOfScannedBarcodesPacked.Last().Equals(_lineReadIn);

                    if (!_lineReadIn.Equals(_poprzedniBarcode) && !isItLastElement && Form1.ScanOkScannerPackout == false)
                    {

                        BoxToPackaut.AddSnToPackoutListAndWriteToFile(_lineReadIn);
                        // Form1.ScanOkScannerPackout = true;

                        if (BoxToPackaut.ListOfScannedBarcodesPacked.Count > 294 &&
                         BoxToPackaut.ListOfScannedBarcodesVerified.IndexOf(_lineReadIn) == BoxToPackaut.ListOfScannedBarcodesPacked.IndexOf(_lineReadIn))
                        {
                            

                            //if (BoxToPackaut.ListOfScannedBarcodesPacked.Count == 0)
                            //    BoxToPackaut.GetOpenCointainer();
                            if(BoxToPackaut.PackUnpackSnJEMS(_lineReadIn, BoxToPackaut.ContainerJems, "Pack", false, false))
                                Form1.ScanOkScannerPackout = true;
                            else
                            {
                                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Pakowanie fail");
                                _poprzedniBarcode = "";
                                return;
                            }

                        }

                        //   Form1._myWindow.RunScannerCheckBoard();
                        _poprzedniBarcode = _lineReadIn;

                    }
                    if (BoxToPackaut.ListOfScannedBarcodesVerified.IndexOf(_lineReadIn) == BoxToPackaut.ListOfScannedBarcodesPacked.IndexOf(_lineReadIn) && Form1.ScanOkScannerPackout == false)
                    {
                        if(BoxToPackaut.ListOfScannedBarcodesPacked.Count <= 294)
                        {
                            bool executeTrigger = false;
                            bool closeContainer = false;
                            if(BoxToPackaut.ListOfScannedBarcodesPacked.Count >= 299)
                            {
                                executeTrigger = true;
                                closeContainer = true;
                            }
                            if (BoxToPackaut.PackUnpackSnJEMS(_lineReadIn, BoxToPackaut.ContainerJems, "Pack", closeContainer, executeTrigger))
                                Form1.ScanOkScannerPackout = true;
                            else
                            {
                                MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Pakowanie fail");
                                _poprzedniBarcode = "";
                                return;
                            }

                            _counterPiecesPacked++;
                            ChangeControl.UpdateControl(_labelCountVerifiedPieces, Color.LawnGreen, BoxToPackaut.ListOfScannedBarcodesPacked.Count.ToString(), true);
                            ChangeControl.UpdateControl(_labelScanInfoPAK, Color.LawnGreen, $"Zeskanowano poprawnie barkod: {_lineReadIn}", true);
                         //   Form1._myWindow.RunScannerCheckBoard();
                        }

                    }                           
                    else
                    {
                       // Form1.ScanOkScannerPackout = false;
                            ChangeControl.UpdateControl(_labelScanInfoPAK, Color.Red, $"{_lineReadIn} NOK / (nie zgadza się kolejność)", true);
                    } 
                            

                    if (Form1.ScanOkScannerPackout && BoxToPackaut.ListOfScannedBarcodesPacked.Count > 294 && BoxToPackaut.ListOfScannedBarcodesPacked.Count <= 300
                        && BoxToPackaut.ListOfScannedBarcodesVerified.Count ==300)
                    {
                        //if (BoxToPackaut.ListOfScannedBarcodesPacked.Count == 1)
                        //    BoxToPackaut.GetOpenCointainer();
                        bool executeTrigger = false;
                        bool closeContainer = false;
                        if (BoxToPackaut.ListOfScannedBarcodesPacked.Count >= 299)
                        {
                            executeTrigger = true;
                            closeContainer = true;
                        }

                        if (BoxToPackaut.PackUnpackSnJEMS(_lineReadIn, BoxToPackaut.ContainerJems, "Pack", closeContainer, executeTrigger))
                        {
                            ChangeControl.UpdateControl(_labelCountVerifiedPieces, Color.LawnGreen, BoxToPackaut.ListOfScannedBarcodesPacked.Count.ToString(), true);
                            ChangeControl.UpdateControl(_labelScanInfoPAK, Color.LawnGreen, $"Zeskanowano poprawnie barkod: {_lineReadIn}", true);
                            try
                            {
                                var res = PLC.WriteBool("PROGRAM:MainProgram.Mes_App_scan");
                                if (!res)
                                {
                                    res = PLC.WriteBool("PROGRAM:MainProgram.Mes_App_scan");

                                    if (!res)
                                    {
                                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Nastąpił błąd komunikacji ze sterownikiem...", " Wezwij UTR");
                                        MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Wpiszcie hasło i spróbujcie wyzwolić sygnał przyciskiem SKAN_OK,\n\n jeśli nie pomoże to zróbcie restart aplikacji, spróbujcie wyzwolić sygnał przyciskiem SKAN_OK,\n\n" +
                                            "jeśli i to nie pomoże sprawdźcie połączenie pomiędzy maszyną a komputerem: \n przycisk windows -> odpalenie wiersza poleceń (cmd) ->\n" +
                                            "wpiszcie polecenie: ping 192.168.1.214");
                                    }

                                }

                            }
                            catch (Exception)
                            {

                                MessageBox.Show("Pomoże?");
                            }

                            Form1.ScanOkScannerPackout = false;


      //     Form1._myWindow.RunScannerForPackout();
                          //  Form1.aTimer.Enabled = false;

                         //   Form1.aTimer.Enabled = true;
                        }
                        else
                        {
                            MessageBox.Show(new Form { TopLevel = true, TopMost = true }, "Pakowanie fail");
                            _poprzedniBarcode = "";
                            return;
                        }

                    }
                    if (BoxToPackaut.ListOfScannedBarcodesPacked.Count == 300)
                    {
                        ChangeControl.UpdateControl(_labelStatusInfo, Color.HotPink, "Zweryfikowano i spakowano 300 sztuk!", true);

                        if(Form1.MoreThan300piecesScanned == true)
                            ChangeControl.UpdateControl(_btnErrorOccursAccept, Color.DeepPink, "Wystąpiły błędy podczas pakowania!\nZaakceptuj wyskakujący komunikat!", true);
                    }
                    
                }
                else
                {
                    if(checkSnCanBeAddedToPackedList == 2)
                        ChangeControl.UpdateControl(_labelScanInfoPAK, Color.Red, $"{_lineReadIn} NOK / nie spełnia wymagań", true);
                    else if(_labelScanInfoPAK.BackColor != Color.Green && _labelScanInfoPAK.BackColor != Color.Red)
                    {
                        ChangeControl.UpdateControl(_labelScanInfoPAK, Color.Green, $"{_lineReadIn} został już zweryfikowany", true);
                      //  Form1.ScanOkScannerPackout = true;
                    }

                }

            }
          //  Form1._myWindow.RunScannerForPackout();
            _poprzedniBarcode = _lineReadIn;
            _lineReadIn = string.Empty;
        }
    }
}

