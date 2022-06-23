using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Helpers;
using System;

namespace RodizioSmartRestaurant.UnitTests
{
    /// <summary>
    /// We expect the FakeClass to be able to be called by services only for security purposes. But that can be revised.
    /// It will provide both dev and prod environments variables for the application to use
    /// </summary>
    [TestClass]
    public class ConnectionStringManagerTests
    {
        /// <summary>
        /// Checks if the production basePath value can be obtained
        /// </summary>
        [TestMethod]
        public void GetConnectionString_BasepathProvidedCorrectValueReturned_databaseBasePath()
        {
            // Arrange
            string providerName= "FirebaseDataBaseSettings";
            string variableName = "BasePath";

            // Act
            string result =ConnectionStringManager.GetConnectionString(providerName, variableName);

            //Assert
            Assert.AreEqual(result, "https://rodizoapp-default-rtdb.firebaseio.com/");

        }

        [TestMethod]
        public void GetConnectionStrings_DisplaysAllTheConnectionStringsItCanFind()
        {
            //Arrange

            //Act
            string result=ConnectionStringManager.GetConnection();

            //Assert
            Assert.AreEqual("https://rodizoapp-default-rtdb.firebaseio.com/", result);
        }
    }
}
