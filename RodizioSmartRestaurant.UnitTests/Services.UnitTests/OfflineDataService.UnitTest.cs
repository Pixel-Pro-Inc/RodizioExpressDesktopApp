﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RodizioSmartRestaurant.UnitTests.View.UnitTests
{
    [TestClass]
    public class OfflineDataServiceTests
    {

        /// <summary>
        /// This was ignored cause it can't work without actually running the app and the network server being instanciated
        /// <para>
        /// This checks if you try to store a user, you will get the user back. The problem is that it hingdes on the fact the revial of users must also work properly
        /// </para>
        /// </summary>
        [TestMethod]
       [Ignore]
        public async Task OfflineStoreData_GiveAccountDirectory_CorrectAccountValuesFromOnlineComparedtoOffline()
        {
            //Arrange
            string fullpath = "Account/";
            string firstname = "Proffessor Xavier2";
            AppUser TestUser = new AppUser() { FirstName = firstname };
            OfflineDataService offlineService = new OfflineDataService();
            
            //Act
            await offlineService.OfflineStoreData(fullpath, TestUser);

            //Asert
            // NOTE: Here is where the problem is thrown cause it can't work without making a networkServer which is done when fully loaded
            List<AppUser> users= await offlineService.GetOfflineData<AppUser>(fullpath);
            foreach (var user in users)
            {
                if (user.FirstName != firstname) continue;
                Assert.AreEqual(firstname, user.FirstName);
            }
        }
        /// <summary>
        /// This was ignored cause it can't work without actually running the app and the network server being instanciated
        /// <para>
        /// This checks if you give the right path it should give you AppUsers no problem
        /// </para>
        /// </summary>
        [TestMethod]
        [Ignore]
        public async Task GetOfflineData_GiveCorrectparameterValues_CorrectResponseReturned()
        {
            //Arrange
            string fullpath = "Account/";
            OfflineDataService offlineService = new OfflineDataService();

            //Act
            // NOTE: Here is where the problem is thrown cause it can't work without making a networkServer which is done when fully loaded
            List<AppUser> users = await offlineService.GetOfflineData<AppUser>(fullpath);

            //Assert
            Assert.IsNotNull(users);

        }


    }
}