﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using USSC.Services;
using USSC.Services.LogServices;
using USSC.Services.PermissionServices;
using USSC.Services.UserServices.Interfaces;
using USSC.Web.ViewModels.Admin;

namespace USSC.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAccessManager _accessManager;
        private readonly IUserDataService _userDataService;
        private readonly IEventLogService _eventLogService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly string _adminSubsystemName;

        public AdminController(IAccessManager accessManager, IUserDataService userDataService, 
            IEventLogService eventLogService, IHostEnvironment hostEnvironment)
        {
            _accessManager = accessManager;
            _userDataService = userDataService;
            _eventLogService = eventLogService;
            _hostEnvironment = hostEnvironment;
            _adminSubsystemName = Constants.AdminSubsystem;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var hasPermission = await _accessManager.HasPermission(User.Identity.Name, _adminSubsystemName);

            if (hasPermission)
            {
                var users = _userDataService.GetAllUsers();
                var roles = _userDataService.GetAllRoles();

                var userViewModel = users.Select(u => new UserViewModel()
                {
                    Id = u.Id,
                    Email = u.Email,
                    Name = $"{u.LastName} {u.Name} {u.Patronymic}",
                    Roles = string.Join(", ", _userDataService.GetUserRoles(u.Id).Result),
                    Accesses = string.Join(", ", _accessManager.GetAccessibleSubsystems(u.Id).Result)
                });

                var roleViewModel = roles.Select(r => new RoleViewModel()
                {
                    Name = r,
                    AccessibleSubsystems = string.Join(", ", _accessManager.GetAccessibleSubsystemsByRole(r).Result)
                });

                var adminViewModel = new AdminViewModel()
                {
                    RoleViewModels = roleViewModel,
                    UserViewModels = userViewModel
                };
                
                return View(adminViewModel);
            }

            return Forbid(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EventLog(int current = 0)
        {
            var hasPermission = await _accessManager.HasPermission(User.Identity.Name, _adminSubsystemName);

            if (hasPermission)
            {
                var pathToLogs = Path.Combine(_hostEnvironment.ContentRootPath, "Logs");

                var dates = _eventLogService.GetAllLogDates(pathToLogs);
                dates.Sort((time1, time2) => -time1.CompareTo(time2));

                var logs = _eventLogService.GetLogs(pathToLogs, dates[current]);

                var model = new EventLogViewModel()
                {
                    CurrentDate = current,
                    Dates = dates,
                    Logs = logs
                };

                return View(model);
            }

            return Forbid(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}