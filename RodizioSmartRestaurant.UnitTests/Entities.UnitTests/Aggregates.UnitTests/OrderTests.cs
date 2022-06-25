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
            float TotalPrice;
            Order TestOrder = new Order()
            {
                new OrderItem() { Price="5"},
                new OrderItem() { Price="5"},
                new OrderItem() { Price="5"}
            };

            //Act
            TotalPrice = TestOrder.TotalPrice;

            //Assert
            Assert.AreEqual(15f, TotalPrice);
        }
    }
}
