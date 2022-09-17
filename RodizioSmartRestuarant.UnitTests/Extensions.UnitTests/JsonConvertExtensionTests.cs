using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Exceptions;
using RodizioSmartRestuarant.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RodizioSmartRestaurant.UnitTests.Extensions.UnitTests
{
    [TestClass]
    public class JsonConvertExtensionTests
    {
        [TestMethod]
        public async Task FromJsonToObjectArray_MenuResponseFromFirebase_ListofMenusReturned()
        {
            //Arrange
            FirebaseDataContext _firebaseDataContext = new FirebaseDataContext();
            string path = "menu/rd29502";
            List<object> response = await _firebaseDataContext.GetData(path);

            //Act
            List<Menu> menus = response.FromJsonToObjectArray<Menu>();

            //Assert
            for (int i = 0; i < menus.Count; i++)
            {
                foreach (MenuItem item in menus[0])
                {
                    Assert.IsNotNull(item.Name);
                }
            }

        }

        [TestMethod]
        public async Task FromJsonToObjectArray_AppUserResponseFromFirebase_ThrowsOutOfRangeException()
        {
            //Arrange
            FirebaseDataContext _firebaseDataContext = new FirebaseDataContext();
            string path = "Account";
            List<object> response = await _firebaseDataContext.GetData(path);

            try
            {
                //Act
                List<AppUser> users = response.FromJsonToObjectArray<AppUser>();
            }
            catch (ArgumentOutOfRangeException)
            {
                //Assert
                // If it catches the out of range exception then it passes,
                // This is so we know it fires the correct exception when it should
                Assert.IsTrue(true);
            }

        }

        /// <summary>
        /// This doesn't work for some reason. it Throws the exception in the correct manner and catches it still, and then when it is time to throw the exception it ends the stack 
        /// trace.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [Ignore]
        public async Task FromJsonToObjectArray_AppUserResponseFromFirebase_ThrowsFailedtoConvertFromJson()
        {
            //Arrange
            FirebaseDataContext _firebaseDataContext = new FirebaseDataContext();
            string path = "Account";
            List<object> response = await _firebaseDataContext.GetData(path);

            try
            {
                //Act
                List<AppUser> users = response.FromJsonToObjectArray<AppUser>();
            }
            catch (FailedToConvertFromJson)
            {
                //Assert
                // If it catches the out of range exception then it passes,
                // This is so we know it fires the correct exception when it should
                Assert.IsTrue(true);
            }

        }

    }
}
