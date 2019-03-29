using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.Areas.API.Controllers
{
    // I've tried the below too with no luck based on teh given link in the question
    //[Authorize(AuthenticationSchemes = AuthSchemes)]
    //[Route("api/[controller]")]
    //[ApiController]
    //public class BranchController : ControllerBase
    //{
    //    private const string AuthSchemes =
    //   CookieAuthenticationDefaults.AuthenticationScheme + "," +
    //   JwtBearerDefaults.AuthenticationScheme;

    [Authorize]
    [ApiController]
    public class BranchApiController : Controller
    {
        private readonly coDbContext _coDbContext;
        private readonly SelectListHelpers _selectListHelpers;

        public BranchApiController(coDbContext coDbContext, SelectListHelpers selectListHelpers)
        {
            _coDbContext = coDbContext;
            _selectListHelpers = selectListHelpers;
        }

        [HttpGet]
        [Route("/api/branches/user")]
        public ActionResult UserBranches()
        {
            // TODO: If initial is provided, get the user by inital and get their list of permitted active channels
            // ApplicationUser login = _userManager.FindByNameAsync(initials).Result;
            IEnumerable<dynamic> branches = _selectListHelpers.BranchList.Select(b => new
            {
                id = int.Parse(b.Value),
                name = b.Text
            });

            return Ok(branches);
        }

        // The below is in a separate helper file but have included it here for simplicity.  When called from Outlook Addin the _httpContext.User is null and causes error
        /// <summary>
        ///     Options for dropdown list of all active branches where the user has access
        /// </summary>
        public IEnumerable<SelectListItem> BranchList
        {
            get
            {
                return _coDbContext.Branches
                    .Where(b => (RolesHelper.IsInHeadOfficeRole(_httpContext.User) ||
                                 _sessionManager.BranchAccess.Contains(b.Id)) && !b.Disabled)
                    .OrderBy(b => b.Name)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = $"{b.Name.Trim()} ({b.Id})"
                    });
            }
        }
    }
}