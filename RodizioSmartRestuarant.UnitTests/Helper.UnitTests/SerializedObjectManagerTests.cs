using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestaurant.UnitTests.Helper.UnitTests
{
    [TestClass]
    public class SerializedObjectManagerTests
    {
        // @Abel: Please tackle the testing environments. Its now getting worrysome
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // This is so when offlineContext.GetData() is fired and it asks if its the server, it will fire yes
            LocalStorage.Instance = new LocalStorage();
            LocalStorage.Instance.networkIdentity = new NetworkIdentity("desktop", true);
        }

        [TestMethod]
        public void SaveData_SingleUserGiven_SameUserReturned()
        {
            // Arrange
            SerializedObjectManager manager = new SerializedObjectManager();
            Directories path = Directories.Account;
            string expected = "TestTEst2s";
            List<AppUser> Users = new List<AppUser>() { new AppUser() { FirstName= expected} };

            // Act
            //manager.SaveData(Users, path);

            // Assert
            List<List<AppUser>> result = (List<List<AppUser>>)manager.RetrieveData(path);
            foreach (List<AppUser> user in result)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    if (expected != user[i].FirstName) continue;
                    Assert.AreEqual(expected, user[i].FirstName);
                }                
            }

        }
    }
}
