using Spire.Pdf;

namespace RodizioSmartRestuarant.Infrastructure.Helpers
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
