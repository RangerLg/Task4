using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Sat.Models;
using System.Collections.Generic;
using Sat.ViewModels;
using System;

namespace CustomIdentityApp.Controllers
{
    public class UsersController : Controller
    {
        UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(_userManager.Users.ToList());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email  };
                user.DataReg = DateTime.Today;
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
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

        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            EditUserViewModel model = new EditUserViewModel { Id = user.Id, Email = user.Email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;
                   

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            
            return View(model);
        }

        //public ActionResult Delete(IEnumerable<string> employeeIdsToDelete, string Name1)
        //{
        //    foreach (var s in employeeIdsToDelete)
        //    {
        //        Deletes(s);
        //    }
        //    return RedirectToAction("Index");
        //}
        [HttpPost]
        public async Task<IActionResult> Delete(IEnumerable<string> employeeIdsToDelete)
        {
            foreach (var id in employeeIdsToDelete)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {

                        
                    }
                }
            }
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> BlockorDelete(IEnumerable<string> employeeIdsToDelete,string ButtonType)
        {
            foreach (var id in employeeIdsToDelete)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    if (ButtonType == "Block selected")
                    {
                        await _userManager.SetLockoutEnabledAsync(user, true);

                        var result = await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddYears(100));
                    }
                    else if(ButtonType == "Delete selected")
                    {
                        var result = await _userManager.DeleteAsync(user);
                    }

                    else if (ButtonType == "Unblock selected")
                    {
                        await _userManager.SetLockoutEnabledAsync(user, false);
                    }
                }
            }
            var rez = User.Identity.Name;
            return RedirectToAction("Index");

        }

        [HttpPost]
        public void On()
        {

        }


    }
}