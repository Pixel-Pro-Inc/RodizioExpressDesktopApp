using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                PrintDocument.PrinterSettings.PrinterName = printername;

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

                //Image image = Resources.logo;
                //e.Graphics.DrawImage(image, startX + 50, startY + Offset, 100, 30);

                //graphics.DrawString("Welcome to HOT AND CRISPY", smallfont,
                //      new SolidBrush(Color.Black), startX + 22, startY + Offset);

                Offset = Offset + mediuminc;
                String Name = "Rodizio Express";
                DrawSimpleString(Name, largefont, Offset, 28);

                Offset = Offset + largeinc + 10;

                String underLine = "-------------------------------------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset = Offset + mediuminc;
                DrawAtStart("Invoice Number: " + order[0].OrderNumber, Offset);

                if (!String.Equals(order[0].PhoneNumber, "N/A"))
                {
                    Offset = Offset + mediuminc;
                    DrawAtStart("Phone # : " + order[0].PhoneNumber, Offset);
                }



                Offset = Offset + mediuminc;
                DrawAtStart("Date: " + DateTime.Now, Offset);

                Offset = Offset + smallinc;
                underLine = "-------------------------------------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset = Offset + largeinc;

                InsertHeaderStyleItem("Name. ", "Price. ", Offset);

                Offset = Offset + largeinc;
                foreach (var itran in order)
                {
                    InsertItem(itran.Name, itran.Price, Offset);
                    Offset = Offset + smallinc;
                }

                float total = 0;
                /*
                 if (!order.Cash.Discount.IsZero())
                foreach (var item in order)
                {
                    total += float.Parse(item.Price);
                }
                */


                Offset = Offset + smallinc;
                InsertHeaderStyleItem(" Amount Received: ", Math.Round(amtReceived, 2).ToString("f2"), Offset);

                Offset = Offset + smallinc;
                InsertHeaderStyleItem(" Change: ", Math.Round(changeAmt,2).ToString("f2"), Offset);

                Offset = Offset + largeinc;
                String address = shop.Location;
                DrawSimpleString("Address: " + address, minifont, Offset, 15);

                Offset = Offset + smallinc;
                String number = "Tel: " + shop.PhoneNumber;
                DrawSimpleString(number, minifont, Offset, 35);

                Offset = Offset + smallinc;
                String served = "Served by: " + order[0].employee.UserName;
                DrawSimpleString(served, minifont, Offset, 35);

                Offset = Offset + 7;
                underLine = "-------------------------------------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset = Offset + mediuminc;
                String greetings = "Thanks for visiting us.";
                DrawSimpleString(greetings, mediumfont, Offset, 28);

                Offset = Offset + mediuminc;
                underLine = "-------------------------------------";
                DrawLine(underLine, largefont, Offset, 0);

                Offset += (2 * mediuminc);
                string tip = "TIP: -----------------------------";
                InsertItem(tip, "", Offset);

                Offset = Offset + largeinc;
                string DrawnBy = "Powered by Pixel Pro";
                DrawSimpleString(DrawnBy, minifont, Offset, 15);
            }
            private void SetShopandOrder(OrderItem order, Branch shop)
            {
                //this is supposed to retrieve data from either local or firebase storage and then set order and shop to be the right values
            }
        }
    }
}
