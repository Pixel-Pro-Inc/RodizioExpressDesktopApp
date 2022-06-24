using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Configuration;

namespace RodizioSmartRestaurant.UnitTests
{
    /// <summary>
    /// We expect the FakeClass to be able to be called by services only for security purposes. But that can be revised.
    /// It will provide both dev and prod environments variables for the application to use
    /// </summary>
    [TestClass]
    public class ConnectionStringManagerTests
    {
        [TestMethod]
        public void GetConnectionString_BasepathProvided_CorrectValueReturned()
        {
            // Arrange
            string variableName = "RodizioSmartRestuarant.Properties.Settings.BasePath";

            // Act
            string result =ConnectionStringManager.GetConnectionString(variableName);

            //Assert
            Assert.AreEqual("https://rodizoapp-default-rtdb.firebaseio.com/", result);

        }

        [TestMethod]
        public void GetConnectionStringSection_CorrectUserDefinedSectionComesFromTheSelectedConfigFile()
        {
            //Arrange
            ConnectionStringsSection result;
            string UserTag = "Rodizio";
            bool containsdeveloperTag =false;

            //Act
            result =ConnectionStringManager.GetConnectionStringSection();

            //Assert
            // TODO: Put this in a guard Clause
            foreach (ConnectionStringSettings conString in result.ConnectionStrings)
            {
                containsdeveloperTag  = conString.Name.Contains(UserTag);
                if (containsdeveloperTag ) break;
            }
            //NOTE: We use the RodizioTag cause we know the system won't make it unless we do, and if it find the 
            Assert.IsTrue(containsdeveloperTag );
        }

        

    }
}
