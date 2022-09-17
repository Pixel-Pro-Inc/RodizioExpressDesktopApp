using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestaurant.UnitTests.Extensions.UnitTests
{
    [TestClass]
    public class SerializedConvertExtensionsTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // This is so when offlineContext.GetData() is fired and it asks if its the server, it will fire yes
            LocalStorage.Instance = new LocalStorage();
            LocalStorage.Instance.networkIdentity = new NetworkIdentity("desktop", true);
        }


        [TestMethod]
        public void FromSerializedToObjectArray_CorrectOrderListgoesIn_CorrectResult()
        {
            //Arrange
            var initPatch = new List<Order> { new Order { new OrderItem { PaymentMethod = "test1" } } };
            // @Yewo: I still don't see why we need to make a list of list of I dictionary, when with other types we don't have to fret so much
            List<List<IDictionary<string, object>>> holder = new List<List<IDictionary<string, object>>>();
            foreach (var item in initPatch)
            {
                holder.Add(new List<IDictionary<string, object>>());

                foreach (var keyValuePair in item)
                {
                    holder[holder.Count - 1].Add(keyValuePair.AsDictionary());
                }
            }
            List<Order> result = new List<Order>();
            // NOTE:  I removed references to the serializedManager cause it doesn't perfectly do its job.
            // We use input here cause what we testing uses and takes in objects only
            object _object = holder;

            //Act
            result = _object.FromSerializedToObjectArray<Order, OrderItem>();

            //Assert
            Assert.IsNotNull(result[0]);
        }

        [TestMethod]
        public void FromSerializedToObject_CorrectObjectgoesIn_CorrectResult()
        {
            //Arrange
            SerializedObjectManager manager = new SerializedObjectManager();
            List<AppUser> initPatch = new List<AppUser> { new AppUser { FirstName = "test1" }, new AppUser { FirstName = "test2" } };
            Directories path = Directories.Account;

            manager.SaveData(initPatch, path);
            var input = manager.RetrieveData(path);
            List<AppUser> result = new List<AppUser>();

            //Act
            result = input.FromSerializedToObject<AppUser>();

            //Assert
            Assert.IsNotNull(result[0].FirstName);
        }

        [ClassCleanup]
        public static void TestFixtureTearDown()
        {
            // Runs once after all tests in this class are executed. (Optional)
            // Not guaranteed that it executes instantly after all tests from the class.
        }

    }
}
