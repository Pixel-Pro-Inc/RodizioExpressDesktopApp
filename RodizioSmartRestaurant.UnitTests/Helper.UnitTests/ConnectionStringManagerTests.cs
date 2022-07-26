using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Exceptions;
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
        #region Correct Input

        [TestMethod]
        public void GetConnectionString_BasepathProvided_CorrectValueReturned()
        {
            // Arrange
            string variableName = "RodizioSmartRestuarant.Properties.Settings.FireBaseBasePath";

            // Act
            string result = ConnectionStringManager.GetConnectionString(variableName);

            //Assert
            Assert.AreEqual("https://rodizoapp-default-rtdb.firebaseio.com/", result);

        }

        [TestMethod]
        public void GetConnectionStringSection_CorrectUserDefinedSectionComesFromTheDefaultSelectedConfigFile()
        {
            //Arrange
            ConnectionStringsSection result;
            string UserTag = "Rodizio";
            bool containsdeveloperTag = false;

            //Act
            result = ConnectionStringManager.GetConnectionStringSection();

            //Assert
            // TODO: Put this in a guard Clause
            foreach (ConnectionStringSettings conString in result.ConnectionStrings)
            {
                containsdeveloperTag = conString.Name.Contains(UserTag);
                if (containsdeveloperTag) break;
            }
            //NOTE: We use the RodizioTag cause we know the system won't make it unless we do, and if it find the tag that means it is getting it from the correct config file
            Assert.IsTrue(containsdeveloperTag);
        }

        [TestMethod]
        public void GetConnectionStringSection_ConfigProvided_CorrectValueReturned()
        {
            // Arrange
            ConnectionStringsSection result;
            string UserTag = "Rodizio";
            bool containsdeveloperTag = false;

            string path = "C:/Users/cash/source/repos/Pixel-Pro-Inc/RodizioExpressDesktopApp/RodizioSmartRestuarant/bin/Debug/RodizioSmartRestuarant.exe";
            Configuration config = ConfigurationManager.OpenExeConfiguration(path);

            // Act
            result = ConnectionStringManager.GetConnectionStringSection(config);

            //Assert
            // TODO: Put this in a guard Clause
            foreach (ConnectionStringSettings conString in result.ConnectionStrings)
            {
                containsdeveloperTag = conString.Name.Contains(UserTag);
                if (containsdeveloperTag) break;
            }
            Assert.IsTrue(containsdeveloperTag);

        }

        #endregion

        #region InCorrect Inputs

        [TestMethod]
        public void GetConnectionString_WrongBasepathProvided_ErrorThrown()
        {
            // Arrange
            string variableName = "Wrong Variable name";

            // Act
            try
            {
                string result = ConnectionStringManager.GetConnectionString(variableName);
            }
            //Assert
            catch (Exception exception)
            {
                Assert.IsTrue(exception is NoConnectionStringSectionFound);
            }

        }

        [TestMethod]
        public void GetConnectionStringSection_WrongConfigProvided_WrongValueReturned()
        {
            // Arrange
            ConnectionStringsSection result;
            string UserTag = "Rodizio";
            bool containsdeveloperTag = false;
            
            // Wrong path given
            string path = "C:/Users/cash/AppData/Local/Postman/Postman.exe";
            Configuration config = ConfigurationManager.OpenExeConfiguration(path);

            // Act
            result = ConnectionStringManager.GetConnectionStringSection(config);

            //Assert
            // TODO: Put this in a guard Clause
            foreach (ConnectionStringSettings conString in result.ConnectionStrings)
            {
                containsdeveloperTag = conString.Name.Contains(UserTag);
                if (containsdeveloperTag) break;
            }
            Assert.IsFalse(containsdeveloperTag);

        }

        #endregion

    }
}
