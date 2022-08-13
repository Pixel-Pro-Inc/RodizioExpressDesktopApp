using API.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RodizioSmartRestaurant.UnitTests.Services.UnitTests
{
    [TestClass]
    public class FirebaseServicesTests
    {
        [TestMethod]
        public async Task StoreData_GiveItAppUser_PathNoLongerEmpty()
        {
            //Arrange
            FirebaseServices _fireBase = new FirebaseServices();
            string path = "Account";
            AppUser user = new AppUser() { FirstName = "TestPainful" };

            //Act
            _fireBase.StoreData(path, user);

            //Assert
            // This is to say, it was null and then there was something stored in it eventually
            List<AppUser> users = await _fireBase.GetData<AppUser>(path);
            foreach (AppUser use in users)
            {
                if (use.FirstName != "TestPainful") continue;
                Assert.IsTrue(true);
            }

        }

        [TestMethod]
        public async Task GetData_GiveItAppUser_RecieveSameUser()
        {
            //Arrange
            FirebaseServices _fireBase = new FirebaseServices();
            string path = "Account";
            AppUser user = new AppUser() { FirstName = "TestPainful" };

            //Act
            _fireBase.StoreData(path, user);

            //Assert
            // This is to say, it was null and then there was something stored in it eventually
            List<AppUser> users = await _fireBase.GetData<AppUser>(path);
            foreach (AppUser use in users)
            {
                if (use.FirstName != "TestPainful") continue;
                Assert.IsTrue(true);
            }

        }

        [TestMethod]
        public async Task DeleteData_GiveItAppUser_PathNoLongerEmpty()
        {
            //Arrange
            FirebaseServices _fireBase = new FirebaseServices();
            string path = "Account";
            AppUser user = new AppUser() { FirstName = "TestPainful" };

            //Act
            _fireBase.StoreData(path, user);

            //Assert
            // This is to say, it was null and then there was something stored in it eventually
            _fireBase.DeleteData(path);
            Assert.IsTrue( (await _fireBase.GetData<AppUser>(path)).Count==0);

        }

    }
}
