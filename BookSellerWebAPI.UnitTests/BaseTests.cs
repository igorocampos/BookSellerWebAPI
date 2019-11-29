using BookSellerWebAPI.Controllers;
using BookSellerWebAPI.Controllers.Filters;
using BookSellerWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static BookSellerWebAPI.UnitTests.MockerHelper;

namespace BookSellerWebAPI.UnitTests
{
    public abstract class BaseTests<T> where T : BaseModel
    {
        protected const int TOTAL_RECORDS = 5;
        protected const string LOWEST_NAME = "'''";
        protected const string HIGHEST_NAME = "ZZZ";

        protected async Task Get(int id, BaseController<T> controller)
        {
            // Act
            var response = await controller.Get(id);

            // Assert
            Assert.True(id <= 0 ? response.Result is NotFoundObjectResult : response.Value.Id == id, "Either the returned model ID was different from the requested, or it found a model when it should not.");
        }

        protected async Task Delete(int id, string expectedResultName, BaseController<T> controller)
        {
            // Act
            var response = await controller.Delete(id);

            // Assert
            Assert.True(response.Result.GetType().Name == expectedResultName, $"It was expected that this test case would have a '{expectedResultName}' return, however it did not. Actual result type: {response.Result.GetType()}.");
        }

        protected async Task Update(int findingId, string expectedResultName, BaseController<T> controller, T model)
        {
            // Act
            var response = await controller.Put(findingId, model);

            // Assert
            Assert.True(response.GetType().Name == expectedResultName, $"It was expected that this test case would have a '{expectedResultName}' return, however it did not. Actual result type: {response.GetType()}.");
        }

        protected async Task Create(string expectedResultName, Controller<T> controller, T model)
        {
            // Act
            ActionResult<T> response = null;

            // Mimic the behaviour of the model binder which is responsible for Validating the Model
            if (controller.Validate(model))
                response = await controller.Post(model);

            // Assert
            Assert.True(response?.Result.GetType().Name == expectedResultName, expectedResultName is null
                                                                               ? "It was expected that this test case would not pass the model biding validation, however it did."
                                                                               : $"It was expected that this test case would have a '{expectedResultName}' return, however it did not.");
        }

    }
}
