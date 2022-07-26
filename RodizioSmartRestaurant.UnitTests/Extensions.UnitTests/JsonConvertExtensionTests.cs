using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Extensions;
using System;
using System.Collections.Generic;

namespace RodizioSmartRestaurant.UnitTests.Extensions.UnitTests
{
    [TestClass]
    public class JsonConvertExtensionTests
    {
        [TestMethod]
        public async void FromJsonToObjectArray_MenuResponseFromFirebase_ListofMenusReturned()
        {
            //Arrange
            FirebaseDataContext _firebaseDataContext = new FirebaseDataContext();
            string path = "Menu/rd29502";
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
    }
}
