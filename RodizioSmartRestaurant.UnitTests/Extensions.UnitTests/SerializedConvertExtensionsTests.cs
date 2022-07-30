using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestaurant.UnitTests.Extensions.UnitTests
{
    [TestClass]
    public class SerializedConvertExtensionsTests
    {
        [TestMethod]
        public async Task FromSerializedToObjectArray_CorrectObjectgoesIn_CorrectResult()
        {
            //Arrange
            var input = await OfflineDataContext.GetData(Directories.Order);
            List<Order> result = new List<Order>();

            //Act
            result = input.FromSerializedToObjectArray<Order, OrderItem>();

            //Assert
            Assert.IsNotNull(result[0].Id);
        }

        [TestMethod]
        public async Task FromSerializedToObject_CorrectObjectgoesIn_CorrectResult()
        {
            //Arrange
            var input = await OfflineDataContext.GetData(Directories.Account);
            List<AppUser> result = new List<AppUser>();

            //Act
            result = input.FromSerializedToObject<AppUser>();

            //Assert
            Assert.IsNotNull(result[0].Id);
        }

    }
}
