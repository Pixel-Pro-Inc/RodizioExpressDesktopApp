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
            private Graphics graphics;
            private List<OrderItem> order { set; get; }
            private Branch shop { set; get; }

            private float amtReceived { get; set; }
            private float changeAmt { get; set; }

            private int InitialHeight = 360;
            public PrintJob(List<OrderItem> order, Branch shop, float amountReceived, float Change)
            {
                this.order = order;
                this.shop = shop;
                amtReceived = amountReceived;
                changeAmt = Change;

                AdjustHeight();
            }
            private void AdjustHeight()
            {
                var capacity = 5 * order.Count;
                InitialHeight += capacity;

                /*
                capacity = 5 * order.DealTransactions.Capacity;
                InitialHeight += capacity;
                */
            }
            public void Print(string printername)
            {
                PrintDocument = new PrintDocument();
                PrintDocument.PrinterSettings.PrinterName = printername == "printername"? null: printername;

                PrintDocument.PrintPage += new PrintPageEventHandler(FormatPage);
                PrintDocument.Print();
            } //this is whats called when we print the receipt
            void DrawAtStart(string text, int Offset)
            {
                int startX = 10;
                int startY = 5;
                Font minifont = new Font("Arial", 5);

                graphics.DrawString(text, minifont,
                         new SolidBrush(Color.Black), startX + 5, startY + Offset);
            }
            void InsertItem(string key, string value, int Offset)
            {
                Font minifont = new Font("Arial", 5);
                int startX = 10;
                int startY = 5;

                graphics.DrawString(key, minifont,
                             new SolidBrush(Color.Black), startX + 5, startY + Offset);

                graphics.DrawString(value, minifont,
                         new SolidBrush(Color.Black), startX + 130, startY + Offset);
            }
            void InsertHeaderStyleItem(string key, string value, int Offset)
            {
                int startX = 10;
                int startY = 5;
                Font itemfont = new Font("Arial", 6, FontStyle.Bold);

                graphics.DrawString(key, itemfont,
                             new SolidBrush(Color.Black), startX + 5, startY + Offset);

                graphics.DrawString(value, itemfont,
                         new SolidBrush(Color.Black), startX + 130, startY + Offset);
            }
            void DrawLine(string text, Font font, int Offset, int xOffset)
            {
                int startX = 10;
                int startY = 5;
                graphics.DrawString(text, font,
                         new SolidBrush(Color.Black), startX + xOffset, startY + Offset);
            }
            void DrawSimpleString(string text, Font font, int Offset, int xOffset)
            {
                int startX = 10;
                int startY = 5;
                graphics.DrawString(text, font,
                         new SolidBrush(Color.Black), startX + xOffset, startY + Offset);
            }
            private void FormatPage(object sender, PrintPageEventArgs e)
            {
                graphics = e.Graphics;
                Font minifont = new Font("Arial", 5);
                Font itemfont = new Font("Arial", 6);
                Font smallfont = new Font("Arial", 8);
                Font mediumfont = new Font("Arial", 10);
                Font largefont = new Font("Arial", 12);
                int Offset = 10;
                int smallinc = 10, mediuminc = 12, largeinc = 15;

                var bitmapImage = new BitmapImage(new Uri(@"pack://application:,,,/Images/rodizio_express_logo.png", UriKind.Absolute));

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)bitmapImage));
                var stream = new MemoryStream();
                encoder.Save(stream);
                stream.Flush();
                var image = new System.Drawing.Bitmap(stream);

                //Image image = new Bitmap("pack://application:,,,/Images/rodizio_express_logo.png");

                e.Graphics.DrawImage(image, 50, Offset, 100, 30);

                Offset = Offset + largeinc + 10;

                String underLine = "-------------------------------------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset = Offset + mediuminc;
                DrawAtStart("Order Number: " + order[0].OrderNumber.Substring(11, 4), Offset);

                if (!String.Equals(order[0].PhoneNumber, "N/A"))
                {
                    Offset = Offset + mediuminc;
                    DrawAtStart("Phone # : " + order[0].PhoneNumber, Offset);
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
                DrawSimpleString("Address: " + address, minifont, Offset, 15);

                Offset = Offset + smallinc;
                String number = "Tel: " + shop.PhoneNumber;
                DrawSimpleString(number, minifont, Offset, 35);

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
                DrawSimpleString(Cashier, minifont, Offset, 15);

                Offset = Offset + largeinc;
                string DrawnBy = "Powered by Pixel Pro";
                DrawSimpleString(DrawnBy, minifont, Offset, 15);
            }
        }
    }
}
