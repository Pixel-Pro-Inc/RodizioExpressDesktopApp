using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using System;

namespace RodizioSmartRestaurant.UnitTests.Entities.UnitTests.Aggregates.UnitTests
{
    [TestClass]
    public class OrderTests
    {
        [TestMethod]
        public void SetPriceofOrder_GiveitOrderItemsWithPrice_GivesTheSumofAllTheOrderItemsPrice()
        {
            //Arrange
            float? TotalPrice=0f;
            Order TestOrder = new Order()
            {
                new OrderItem() { Price="5"},
                new OrderItem() { Price="5"},
                new OrderItem() { Price="5"}
            };

            //Act
            // FIXME: I think its having problems evaluating what the price is
            // Aggregate works perfectly, but for some reason it goes to the getter first, instead of setting it to the default value
            // UPDATE:
            // You need to use net. 6.0 . That way you can use the keyword init. You can only use it when you upgrade to vs 22
            TotalPrice = TestOrder.Price;

            //Assert
            //Checks if the prices add to 15
            Assert.AreEqual(15f, TotalPrice);
        }


    }
}
