using App.Tests.Managers;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ControllerExtensions
    {
        public static T SetupTestControllerContext<T>(this T controller)
               where T : ControllerBase
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = TestUser.ClaimsPrincipal
                },
                ActionDescriptor = new Controllers.ControllerActionDescriptor()
            };

            controller.Url = new Mock<IUrlHelper>().Object;

            return controller;
        }
    }
}
