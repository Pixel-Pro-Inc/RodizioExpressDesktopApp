using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;

namespace RodizioSmartRestaurant.UnitTests.View.UnitTests
{
    [TestClass]
    public class OfflineDataServiceTests
    {
        // This is the service the whole test method will use for reference to the public methods
        IOfflineDataService _offlineDataService;

        /// <summary>
        /// This checks if you try to store a user, you will get the user back. The problem is that it hingdes on the fact the revial of users must also work properly
        /// </summary>
       [TestMethod]
        public async void OfflineStoreData_GiveAccountDirectory_CorrectAccountValuesFromOnlineComparedtoOffline()
        {
            //Arrange
            string fullpath = "Account/";
            string firstname = "Proffessor Xavier2";
            AppUser TestUser = new AppUser() { FirstName = firstname };
            
            //Act
            await _offlineDataService.OfflineStoreData(fullpath, TestUser);

            //Asert
            List<AppUser> users= await _offlineDataService.GetOfflineData<AppUser>(fullpath);
            foreach (var user in users)
            {
                if (user.FirstName != firstname) continue;
                Assert.AreEqual(firstname, user.FirstName);
            }
        }


    }
}
