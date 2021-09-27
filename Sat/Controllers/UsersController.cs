using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Task4Core.Models;
using System.Collections.Generic;
using System;

namespace Task4Core.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> BlockOrDelete(IEnumerable<string> employeeIdsToDelete,string ButtonType)
        {
            var isUser = false;
            foreach (var id in employeeIdsToDelete)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) continue;
                if(User.Identity != null && User.Identity.Name == user.UserName)
                {
                    isUser = true;
                }
                switch (ButtonType)
                {
                    case "Block selected":
                    {
                        await _userManager.SetLockoutEnabledAsync(user, true);

                        await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddYears(100));
                        break;
                    }
                    case "Delete selected":
                    {
                        await _userManager.DeleteAsync(user);
                        break;
                    }
                    case "Unblock selected":
                        await _userManager.SetLockoutEnabledAsync(user, false);
                        break;
                }
            }
            
            if (!isUser|| ButtonType == "Unblock selected")
            {
           
                return RedirectToAction("UserList","Roles");
            }

            return RedirectToAction("Login", "Account");

        }

        [HttpPost]
        public void On()
        {

        }


    }
}