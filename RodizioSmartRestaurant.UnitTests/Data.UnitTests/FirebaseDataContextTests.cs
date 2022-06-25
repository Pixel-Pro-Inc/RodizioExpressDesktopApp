using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Data;
using System;
using System.Collections.Generic;

namespace RodizioSmartRestaurant.UnitTests.Data.UnitTests
{
    [TestClass]
    public class FirebaseDataContextTests
    {
        #region GetData tests
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


    }
}
