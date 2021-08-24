using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sat.ViewModels;
using Sat.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Localization;

namespace Task4Core.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHtmlLocalizer<AccountController> _localizer;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IHtmlLocalizer<AccountController> localizer)
        {
            
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email};
               
                user.LockoutEnabled = false;
                // добавляем пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
              
                if (result.Succeeded)
                {
                    var userToId = await _userManager.FindByEmailAsync(model.Email);
                    //await _userManager.AddToRoleAsync(userToId, "User");
                    // установка куки
                    await _signInManager.SignInAsync(user, false);
                    var res = await _userManager.FindByNameAsync(model.Email);
                    await _userManager.SetLockoutEnabledAsync(res, false);
                    _ = _userManager.UpdateAsync(res);
                    return RedirectToAction("Index", "Collections");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
               
                if (result.Succeeded)
                {
                    // проверяем, принадлежит ли URL приложению
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        var res = await _userManager.FindByEmailAsync(model.Email);
                       
                        await _userManager.UpdateAsync(res);
                        var role = _userManager.IsInRoleAsync(res, "Admin");
                        if (!role.Result)
                        {
                            return RedirectToAction("Index", "Collections");
                        }
                        else
                        {
                            return RedirectToAction("UserList", "Roles");
                        }

                    }
                }
                else
                {
                    var res = await _userManager.FindByEmailAsync(model.Email);
                    var test = _localizer["You are blocked"];
                    if (res != null &&res.LockoutEnabled == true)
                    {
                        ModelState.AddModelError("", _localizer.GetString("You are blocked"));
                    }
                    else
                    {
                        ModelState.AddModelError("", _localizer.GetString("Incorrect username and (or) password"));
                    }
                }
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                                    new { ReturnUrl = returnUrl });

            var properties =
                _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }
        public async Task<IActionResult>
    ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                                        info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new User
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await _userManager.CreateAsync(user);
                    }

                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }



                return View("Error");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}