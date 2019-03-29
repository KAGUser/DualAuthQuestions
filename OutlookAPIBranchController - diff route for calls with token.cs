using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CO.MVC.Areas.API.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class OutlookBranchApiController : BranchApiController
    {
        public OutlookBranchApiController(CoDbContext coDbContext, SelectListHelpers selectListHelpers) : base(coDbContext, selectListHelpers)
        {
        }

        [HttpGet("/api/outlook/branches/user/")]
        public new ActionResult UserBranches(string initials = null)
        {
            return base.UserBranches(initials);
        }

    }
}