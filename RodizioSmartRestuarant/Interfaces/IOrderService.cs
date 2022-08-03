using RodizioSmartRestuarant.Entities.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Interfaces
{
    public interface IOrderService: IBaseService
    {
        /// <summary>
        /// This simply marks the order items individuals for deletion. If the device is a TCPServer then it give the full path, else it just does it locally
        /// </summary>
        /// <param name="orderItems"></param>
        /// <returns></returns>
        Task MarkOrderForDeletion(Order orderItems);

        /// <summary>
        /// This addes the order to the cancelled order list and then deletes it 
        /// </summary>
        /// <param name="orderInvoice"></param>
        /// <returns></returns>
        Task CancelOrder(string orderInvoice);

        Task<object> GetOfflineOrdersCompletedInclusive();
        /// <summary>
        /// This compares the <paramref name="querystring"/> to the orderNumber or phoneNumber of the order and returns a list of orders that have the same as <paramref name="querystring"/>
        /// </summary>
        /// <param name="querystring"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        List<Order> SearchForQueryString(string querystring, List<Order> orders);

        /// <summary>
        /// This moves the order in the fullpath to the completed directory
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        Task CompleteOrder(string fullPath);
    }
}
