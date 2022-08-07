using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RodizioSmartRestaurant.UnitTests.Data.UnitTests
{
    [TestClass]
    public class FirebaseDataContextTests
    {
        private static FirebaseDataContext fbDataContext = new FirebaseDataContext();
        // We don't need to delete data over and over cause since the data is unique it will just keep over writting

        #region GetData tests

        // All these are testing for is if it can manage to get values from the database, not the validity of the data
        [TestMethod]
        public async Task GetData_GiveBranch_ReturnListOfBranches()
        {
            //Arrange
            string path = "Branch";

            //Act
            List<object> objects = await fbDataContext.GetData(path);

            //Assert
            // I'm using count cause the objects are never null, they just contain nothing or something
            Assert.IsFalse(objects.Count == 0);

        }

        [TestMethod]
        public async Task GetData_GivenWrongString_ReturnsNothing()
        {
            //Arrange
            string path = "WrongString";
            //paths.Add("GetData_GivenWrongString_ReturnsNothing", path);

            //Act
            List<object> objects = await fbDataContext.GetData(path);

            //Assert
            // I'm using count cause the objects are never null, they just contain nothing or somethingwha
            Assert.IsTrue(objects.Count == 0);

        }

        #endregion
        #region StoreData Tests

        [TestMethod]
        public async Task StoreData_GiveOrder_ThepathisNotEmptyAnymore()
        {
            //Arrange
            string orderNumber = "TestStoreData123";
            //REFACTOR: Use the aggregateProp next time after the tests run
            Order TestOrder = new Order()
            {
                new OrderItem() { OrderNumber=orderNumber }
            };
            string path = "Order/" + "rd29502" + "/" + TestOrder.OrderNumber;
            //paths.Add("StoreData_GiveOrder_ThepathisNotEmptyAnymore", path);


            //Act
            await fbDataContext.StoreData(path, TestOrder);

            //Assert
            // I'm using count cause the objects are never null, they just contain nothing or something
            // I also don't want to check what data is in there cause I know that the path is unique
            Assert.IsTrue((await fbDataContext.GetData(path)).Count == 1);

        }

        #endregion
        #region DeleteData tests

        // All these are testing for is if it can successfully delete data in specified path
        [TestMethod]
        public async Task DeleteData_GivenPathString_ReturnsWithNothing()
        {
            //Arrange
            string orderNumber = "TestNumber123";
            //REFACTOR: Use the aggregateProp next time after the tests run
            Order TestOrder = new Order()
            {
                new OrderItem() { OrderNumber=orderNumber }
            };
            string path = "Order/" + "rd29502" + "/" + TestOrder.OrderNumber;
            //paths.Add("DeleteData_GivenPathString_ReturnsWithNothing", path);
            await fbDataContext.StoreData(path, TestOrder);

            //Act
            await fbDataContext.DeleteData(path);

            //Assert
            Assert.IsTrue((await fbDataContext.GetData(path)).Count == 0);

        }

        #endregion

    }
}
