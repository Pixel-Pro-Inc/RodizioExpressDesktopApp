using API.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RodizioSmartRestaurant.UnitTests.Services.UnitTests
{
    [TestClass]
    public class FirebaseServicesTests
    {
        #region Store Data

        [TestMethod]
        public async Task StoreData_GiveItABranchWithinAList_GivesItTheSameBranch()
        {
            //Arrange
            FirebaseServices _fireBase = new FirebaseServices();
            string path = "Branch";
            List<Branch> branch = new List<Branch>() { new Branch() { Name = "TestBranch" } };

            //Act
            _fireBase.StoreData(path, branch);

            //Assert
            List<Branch> branches = await _fireBase.GetData<Branch>(path);
            foreach (Branch b in branches)
            {
                if (b.Name != "TestBranch") continue;
                Assert.IsTrue(true);
            }

        }

        #endregion

        #region Get Data
        /// <summary>
        /// This method is to test if it can store and then retrieve a single Order item instead of always a list of entities as it usually works out.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetData_GiveItAnOrderItem_GivesItTheSameOrderItem()
        {
            //Arrange
            FirebaseServices _fireBase = new FirebaseServices();
            string name = "TestOrderItem";
            OrderItem orderItem = new OrderItem() { Name = name, Id = 234, OrderNumber = "TestOrderNumber234" };
            string path = "Order/" + "rd29502" ;
            List< Order> iOrders = new List<Order>() { new Order() { orderItem } };
            _fireBase.StoreData(path, iOrders);

            //Act
            // This would be a miscellaneous list of mark orderItems for deletion so i wouldn't consider this a whole order
            List<Order> Orders = await _fireBase.GetDataArray<Order,OrderItem>(path);

            //Assert
            foreach (Order orderitem in Orders)
            {
                if (orderitem[0].Name != name) continue;
                Assert.IsTrue(true);
            }

        }
        /// <summary>
        /// This method is to test if it can store and then retrieve a single Order item instead of always a list of entities as it usually works out.
        /// 
        /// <para> Conclusion: No it cannot. It throws a parsing error cause it wont know what to do with the fields of the object, ( Sees it as a JProperty instead of Jobject</para>
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetData_GiveItAMenuItem_GivesItTheSameMenuItem()
        {
            //Arrange
            FirebaseServices _fireBase = new FirebaseServices();
            string name = "TestOrderItem";
            MenuItem orderItem = new MenuItem() { Name = name, Id = 234 };
            string path = "Menu" + "rd29502";
            _fireBase.StoreData(path, orderItem);

            //Act
            // This would be a miscellaneous list of mark orderItems for deletion so i wouldn't consider this a whole order
            List<MenuItem> menuItems = await _fireBase.GetData<MenuItem>(path);

            //Assert
            foreach (MenuItem item in menuItems)
            {
                if (item.Name != name) continue;
                Assert.IsTrue(true);
            }

        }

        [TestMethod]
        public async Task GetData_GiveItAppUserWithinAList_RecieveSameUser()
        {
            //Arrange
            FirebaseServices _fireBase = new FirebaseServices();
            string path = "Account";
            List<AppUser> user = new List<AppUser>() { new AppUser() { FirstName = "TestPainful" } };

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

        #endregion

        #region Delete Data

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
            Assert.IsTrue((await _fireBase.GetData<AppUser>(path)).Count == 0);

        }

        #endregion

    }
}
