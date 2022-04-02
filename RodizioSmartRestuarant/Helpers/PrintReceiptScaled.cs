using ceTe.DynamicPDF.Printing;
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
            PrintJob printJob = new PrintJob(printerName, receiptPath);

            printJob.PrintOptions.Orientation.Type = OrientationType.Portrait;
            printJob.PrintOptions.Orientation.Rotated = false;

            //PercentagePageScaling percentagePageScaling = new PercentagePageScaling(0.25f);
            //printJob.PrintOptions.Scaling = percentagePageScaling;
            printJob.PrintOptions.Scaling = PageScaling.ActualSize;
            printJob.PrintOptions.SetPaperSizeByName("72mm Width * 210mm Height");
            var size = printJob.PrintOptions.PaperSize;
            var size_2 = printJob.PrintOptions.Resolution;

            printJob.Print();
        }
    }
}
