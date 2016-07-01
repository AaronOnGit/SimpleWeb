using System.Web.Mvc;
using SimpleWeb.Models;
using System.Web.Security;
using SimpleWeb.Services;

namespace SimpleWeb.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            bool result = AccountServices.PasswordSignIn(model.Email, model.Password, model.RememberMe);
            if (result)
            {
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Incorrect email, or password!");
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool emailCheck = AccountServices.FindUserByEmail(model.Email);
                if (!emailCheck)
                {
                    bool result = AccountServices.AddUser(model.Email, model.Password);
                    if (result)
                    {
                        string token = AccountServices.GenerateToken(model.Email);
                        string subject = "Confirm your account";
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { token = token }, protocol: Request.Url.Scheme);
                        string body = string.Format("Please confirm your account by clicking <a href='{0}'>here</a>.<br> Or copy this link to your browser. {0}", callbackUrl);
                        AccountServices.SendEmail(model.Email, subject, body);
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("", "Something went wrong! Please try again.");
                return View(model);
            }
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult ConfirmEmail(string token)
        {
            if (token == null)
            {
                return View("Error");
            }
            var result = AccountServices.ActivateUser(token);
            return View(result ? "ConfirmEmail" : "Error");
        }


        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = AccountServices.IsEmailConfirmed(model.Email);
                if (!result)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }
                string token = AccountServices.GenerateToken(model.Email);
                string subject = "Reset your password";
                var callbackUrl = Url.Action("ResetPassword", "Account", new { token = token }, protocol: Request.Url.Scheme);
                string body = string.Format("Please reset your password by clicking <a href='{0}'>here</a>.<br> Or copy this link to your browser. {0}", callbackUrl);
                AccountServices.SendEmail(model.Email, subject, body);
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [AllowAnonymous]
        public ActionResult ResetPassword(string token)
        {
            bool result = AccountServices.VerifyToken(token);
            return result ? View() : View("Error");
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            bool result = AccountServices.UpdatePassword(model.Email, model.Password, model.Token);
            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }


        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

    }
}