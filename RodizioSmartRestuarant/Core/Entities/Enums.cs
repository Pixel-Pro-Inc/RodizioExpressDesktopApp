namespace RodizioSmartRestuarant.Core.Entities
{
    public class Enums
    {
        public enum UIChangeSource
        {
            Deletion,
            Edit,
            Addition,
            Search,
            StartUp
        }
        public enum Directories
        {
            Order, 
            Menu, 
            Account, 
            Branch,
            BranchId,
            PrinterName,
            Settings,
            NetworkInterface,
            Print,
            TCPServer,
            TCPServerIP,
            Error,
            CalledOutOrders
        }
    }
}
