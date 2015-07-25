using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin;
using System.Collections.Generic;
using frontoffice.Models;

namespace frontoffice.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public UsersController()
        {
        }

        public UsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // bns
        // returns login view, accessible to anyone
        [AllowAnonymous]
        [HttpGet]    
        public ActionResult Login()
        {
            return View();
        }

        // bns
        // passes login parameters in model function parameter to the SignInManager object.
        // if passed, returns to the URL in returnurl function parameter
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        Helpers.UsersHelper.UpdateLoginStats(model.Email);
                        return RedirectToLocal(returnUrl);
                    }
                //case SignInStatus.LockedOut:
                //    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // bns
        // returns Manage view only if admin is logged in
        [Authorize(Roles="Admin")]
        [HttpGet]
        public ActionResult Manage()
        {
            return View();
        }

        // bns
        // Url /Users/Create returns view on HttpGet if admin is logged in
        [HttpGet]
        [Authorize(Roles="Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // bns
        // Creates new user in the database, only allowed to admins
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(frontoffice.Models.CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    AgentFullName = model.FullName
                };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    UserManager.AddToRole(user.Id, "User");
                    return RedirectToAction("Manage");
                }
                ModelState.AddModelError("", result.Errors.FirstOrDefault());
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            
            return RedirectToAction("Index", "Home");
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }
}
