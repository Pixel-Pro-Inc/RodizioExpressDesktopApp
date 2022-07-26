﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using System;
using System.Collections.Generic;

namespace RodizioSmartRestaurant.UnitTests.Data.UnitTests
{
    [TestClass]
    public class FirebaseDataContextTests
    {
        public FirebaseDataContext fbDataContext = new FirebaseDataContext();
        #region GetData tests

        // All these are testing for is if it can manage to get values from the database, not the validity of the data
        [TestMethod]
        public async void GetData_GivenPathString_ReturnsListObjects()
        {
            //Arrange
            string path = "Branch";
            FirebaseDataContext fbContext = new FirebaseDataContext();

            //Act
            List<object> objects = await fbContext.GetData(path);

            //Assert
            Assert.IsFalse(objects == null);

        }
        [TestMethod]
        public async void GetData_GivenWrongString_ThrowsError()
        {
            //Arrange
            string path = "WrongString";

            //Act
            List<object> objects = await fbDataContext.GetData(path);

            //Assert
            Assert.IsTrue(objects == null);

        }
        #endregion

        #region DeleteData tests

        // All these are testing for is if it can successfully delete data in specified path
        [TestMethod]
        public async void DeleteData_GivenPathString_ReturnsWithNothing()
        {
            //Arrange
            string orderNumber = "TestNumber123";
            //REFACTOR: Use the aggregateProp next time after the tests run
            Order TestOrder = new Order()
            {
                new OrderItem() { OrderNumber=orderNumber }
            };
            string path = "Order/" + BranchSettings.Instance.branchId + "/" + TestOrder.OrderNumber;
            await fbDataContext.StoreData(path, TestOrder);

            //Act
            await fbDataContext.DeleteData(path);

            //Assert
            Assert.IsTrue(await fbDataContext.GetData(path) == null);

        }
        [TestMethod]
        public async void DeleteData_GivenWrongString_ReportWithNothing()
        {
            //Arrange
            string path = "WrongString";

            //Act
            await fbDataContext.DeleteData(path);

            //Assert
            Assert.IsTrue(await fbDataContext.GetData(path) == null);

        }
        #endregion


    }
}