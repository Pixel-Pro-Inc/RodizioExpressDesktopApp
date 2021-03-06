using PdfSharp.Drawing;
using PdfSharp.Pdf;
using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RodizioSmartRestuarant.Helpers
{
    class ReceiptSlip
    {
        public class PrintJob
        {
            private PrintDocument PrintDocument;
            private XGraphics graphics;
            private List<OrderItem> order { set; get; }
            private Branch shop { set; get; }

            private float amtReceived { get; set; }
            private float changeAmt { get; set; }
            private float scaleFactor = 1.22f;
            public PrintJob(List<OrderItem> order, Branch shop, float amountReceived, float Change)
            {
                this.order = order;
                this.shop = shop;
                amtReceived = amountReceived;
                changeAmt = Change;
            }
            #region View

            void DrawAtStart(string text, int Offset)
            {
                int startX = 10;
                int startY = 5;
                Font minifont = new Font("Arial", 5 * scaleFactor * 1.3f, GraphicsUnit.World);

                XPoint xPoint = new XPoint()
                {
                    X = startX + 5,
                    Y = startY + Offset
                };

                graphics.DrawString(text, minifont, new XSolidBrush(XColors.Black), xPoint);
                //new SolidBrush(Color.Black), startX + 5, startY + Offset);
            }
            void InsertItem(string key, string value, int Offset)
            {
                Font minifont = new Font("Arial", 5 * scaleFactor * 1.3f, GraphicsUnit.World);
                int startX = 10;
                int startY = 5;

                XPoint xPoint = new XPoint()
                {
                    X = startX + 5,
                    Y = startY + Offset
                };

                XPoint xPoint_1 = new XPoint()
                {
                    X = startX + 170,
                    Y = (startY + Offset) - 2
                };

                graphics.DrawString(key, minifont, new XSolidBrush(XColors.Black), xPoint);
                //new SolidBrush(Color.Black), startX + 5, startY + Offset);

                graphics.DrawString(value, minifont, new XSolidBrush(XColors.Black), xPoint_1.X, xPoint_1.Y, XStringFormats.CenterRight);
                //graphics.DrawString(value, minifont, new XSolidBrush(XColors.Black), xPoint_1);
                //new SolidBrush(Color.Black), startX + 130, startY + Offset);
            }
            void InsertHeaderStyleItem(string key, string value, int Offset)
            {
                int startX = 10;
                int startY = 5;
                Font itemfont = new Font("Arial", 6 * scaleFactor * 1.3f, FontStyle.Bold, GraphicsUnit.World);

                XPoint xPoint = new XPoint()
                {
                    X = startX + 5,
                    Y = startY + Offset
                };

                XPoint xPoint_1 = new XPoint()
                {
                    X = startX + 170,
                    Y = (startY + Offset) - 2
                };

                graphics.DrawString(key, itemfont, new XSolidBrush(XColors.Black), xPoint);
                //new SolidBrush(Color.Black), startX + 5, startY + Offset);

                graphics.DrawString(value, itemfont, new XSolidBrush(XColors.Black), xPoint_1.X, xPoint_1.Y, XStringFormats.CenterRight);
                //graphics.DrawString(value, itemfont, new XSolidBrush(XColors.Black), xPoint_1);
                //new SolidBrush(Color.Black), startX + 130, startY + Offset);
            }
            void DrawLine(string text, Font font, int Offset, int xOffset)
            {
                int startX = 10;
                int startY = 5;
                XPoint xPoint = new XPoint()
                {
                    X = startX + xOffset,
                    Y = startY + Offset
                };
                graphics.DrawString(text, font, new XSolidBrush(XColors.Black), xPoint);
                //new SolidBrush(Color.Black), startX + xOffset, startY + Offset);
            }
            void DrawSimpleString(string text, Font font, int Offset, int xOffset)
            {
                int startX = 10;
                int startY = 5;
                XPoint xPoint = new XPoint()
                {
                    X = startX + xOffset,
                    Y = startY + Offset
                };
                graphics.DrawString(text, font, new XSolidBrush(XColors.Black), xPoint);
                //new SolidBrush(Color.Black), startX + xOffset, startY + Offset);
            }
            private void FormatPage()
            {
                Font _minifont = new Font("Arial", 5 * scaleFactor * 1.3f, FontStyle.Regular, GraphicsUnit.World);
                Font mediumfont = new Font("Arial", 10 * scaleFactor, FontStyle.Regular, GraphicsUnit.World);
                Font largefont = new Font("Arial", 12 * scaleFactor, FontStyle.Regular, GraphicsUnit.World);

                int Offset = 10;
                int smallinc = 10, mediuminc = 12, largeinc = 15;

                var bitmapImage = new BitmapImage(new Uri(@"pack://application:,,,/Images/rodizio_express_logo.png", UriKind.Absolute));

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)bitmapImage));
                var stream = new MemoryStream();
                encoder.Save(stream);
                stream.Flush();

                var xImage = XImage.FromStream(stream);
                //var image = new System.Drawing.Bitmap(stream);

                graphics.DrawImage(xImage, 30, 5, 101 * scaleFactor, 26 * scaleFactor);

                Offset = Offset + largeinc + 10;

                String underLine = "-------------------------------------";
                DrawLine(underLine, largefont, 40, 0);

                Offset = Offset + mediuminc + 10;
                String orderNumber = "Order Number: " + order[0].OrderNumber.Substring(11, 4);
                DrawSimpleString(orderNumber, mediumfont, Offset, 28);

                if (!String.Equals(order[0].PhoneNumber, "N/A"))
                {
                    if (!string.IsNullOrEmpty(order[0].PhoneNumber))
                    {
                        Offset = Offset + mediuminc + 5;
                        DrawAtStart("Phone # : " + order[0].PhoneNumber, Offset);
                    }                    
                }



                Offset = Offset + mediuminc;
                DrawAtStart("Date: " + DateTime.Now, Offset);

                Offset = Offset + mediuminc;
                int pTime = 0;
                foreach (var item in order)
                {
                    if (pTime < item.PrepTime)
                        pTime = item.PrepTime;
                }
                DrawAtStart("Order Preparation Time: " + pTime + " minutes", Offset);

                Offset = Offset + smallinc;
                underLine = "-------------------------------------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset = Offset + largeinc;

                InsertHeaderStyleItem("Name. ", "Price. ", Offset);

                Offset = Offset + largeinc;
                foreach (var itran in order)
                {
                    InsertItem(itran.Name + " x " + itran.Quantity, Formatting.FormatAmountString(float.Parse(itran.Price)), Offset);
                    Offset = Offset + smallinc;
                }


                float total = 0;

                foreach (var item in order)
                {
                    total += float.Parse(item.Price);
                }

                Offset = Offset + smallinc;
                InsertHeaderStyleItem(" Amount Payable: ", Formatting.FormatAmountString(total), Offset);

                Offset = Offset + smallinc;
                InsertHeaderStyleItem(" Amount Received: ", Formatting.FormatAmountString(amtReceived), Offset);

                Offset = Offset + smallinc;
                InsertHeaderStyleItem(" Change: ", Formatting.FormatAmountString(changeAmt), Offset);

                Offset = Offset + largeinc;
                String address = shop.Location;
                DrawSimpleString("Address: " + address, _minifont, Offset, 15);

                Offset = Offset + smallinc;

                String number = "Tel : ";

                foreach (var item in shop.PhoneNumbers)
                {
                    if (shop.PhoneNumbers.IndexOf(item) == 0)
                        number += item;

                    if (shop.PhoneNumbers.IndexOf(item) != 0)
                        number += " / " + item;
                }

                DrawSimpleString(number, _minifont, Offset, 35);

                /*Offset = Offset + 17;
                underLine = "-------- TAX INVOICE ---------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset = Offset + smallinc;
                String vatNumber = "Vat Vendor No: BW00003654253";
                DrawSimpleString(vatNumber, _minifont, Offset, 5);

                Offset = Offset + smallinc;
                String vatRate = "Vat Rate: 14.00%";
                DrawSimpleString(vatRate, _minifont, Offset, 5);

                Offset = Offset + smallinc;
                InsertHeaderStyleItem("Exclusive: ", Formatting.FormatAmountString(total / 1.14f), Offset);

                Offset = Offset + smallinc;
                InsertHeaderStyleItem("Vat: ", Formatting.FormatAmountString(total - (total / 1.14f)), Offset);

                Offset = Offset + smallinc;
                InsertHeaderStyleItem("Inclusive: ", Formatting.FormatAmountString(total), Offset);
                */

                Offset = Offset + 7;
                underLine = "-------------------------------------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset = Offset + mediuminc;
                String greetings = "Thanks for visiting us.";
                DrawSimpleString(greetings, mediumfont, Offset, 28);

                Offset = Offset + mediuminc;
                underLine = "-------------------------------------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset = Offset + largeinc;
                string Cashier = "Cashier: " + LocalStorage.Instance.user.FullName();
                DrawSimpleString(Cashier, _minifont, Offset, 15);

                Offset = Offset + largeinc;
                string DrawnBy = "Software Developed by Pixel Pro";
                DrawSimpleString(DrawnBy, _minifont, Offset, 15);

                Offset = Offset + 17;
                underLine = "----- SCAN TO ORDER -----";
                DrawLine(underLine, largefont, Offset, 0);

                var _bitmapImage = new BitmapImage(new Uri(@"pack://application:,,,/Images/receipt_qr.png", UriKind.Absolute));

                var _encoder = new PngBitmapEncoder();
                _encoder.Frames.Add(BitmapFrame.Create((BitmapImage)_bitmapImage));
                var _stream = new MemoryStream();
                _encoder.Save(_stream);
                _stream.Flush();

                var _xImage = XImage.FromStream(_stream);

                graphics.DrawImage(_xImage, 30, Offset + 10, 120 * scaleFactor, 120 * scaleFactor);
            }

            #endregion

            //this is whats called when we print the receipt
            public void Print(string printername)
            {
                string printerName = printername == "printername"? null: printername;

                PdfDocument pdfDoc = new PdfDocument();

                PdfPage pdfPage = new PdfPage();
                //
                //pdfPage.Size = PdfSharp.PageSize.Undefined;
                pdfPage.Width = 204;//2.83465 Xunit/Milimeter (2.83465 * 72)
                pdfDoc.Pages.Add(pdfPage);

                graphics = XGraphics.FromPdfPage(pdfPage);

                FormatPage();

                string printPath = new SerializedObjectManager().PrintReceiptPath(Enums.Directories.Print) + "/receipt.pdf";

                pdfDoc.Save(printPath);

                new PrintReceiptScaled().Print(printerName, printPath);
            } 
          
        }
    }
}
