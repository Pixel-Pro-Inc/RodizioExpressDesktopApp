using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        #region GetData tests

        // All these are testing for is if it can manage to get values from the database, not the validity of the data
        [TestMethod]
        public async void GetData_GivenPathString_ReturnsListObjects()
        {
            //Arrange
            string path = "Branch";

            //Act
            List<object> objects = await FirebaseDataContext.Instance.GetData(path);

            //Assert
            Assert.IsFalse(objects == null);

        }
        [TestMethod]
        public async void GetData_GivenWrongString_ThrowsError()
        {
            //Arrange
            string path = "WrongString";

            //Act
            List<object> objects = await FirebaseDataContext.Instance.GetData(path);

            //Assert
            Assert.IsTrue(objects == null);

        }
        #endregion

        #region DeleteData tests

        // All these are testing for is if it can successfully delete data in specified path
        [TestMethod]
        public async void DeleteData_GivenPathString_ReturnsListObjects()
        {
            //Arrange
            string path = "Order/" + BranchSettings.Instance.branchId + "/" + item[0].OrderNumber;
            Order data = new Order() { };
            await FirebaseDataContext.Instance.StoreData(path, data);

            //Act
            await FirebaseDataContext.Instance.DeleteData(path);

            //Assert
            Assert.IsFalse(await FirebaseDataContext.Instance.GetData(path) == null);

        }
        [TestMethod]
        public async void DeleteData_GivenWrongString_ThrowsError()
        {
            //Arrange
            string path = "WrongString";

            //Act
            await FirebaseDataContext.Instance.DeleteData(path);

            //Assert
            Assert.IsTrue(await FirebaseDataContext.Instance.GetData(path) == null);

        }
        #endregion


    }
}
