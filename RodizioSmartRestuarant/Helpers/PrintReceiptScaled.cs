using Spire.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class PrintReceiptScaled
    {
        public void Print(string printerName, string receiptPath)
        {
            PdfDocument doc = new PdfDocument();
            doc.LoadFromFile(receiptPath);
            doc.PrintSettings.PrinterName = printerName;

            // Print all pages of a document using the default printer
            doc.Print();
        }
    }
}
