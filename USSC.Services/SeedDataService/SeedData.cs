﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using USSC.Infrastructure;
using USSC.Infrastructure.Models;
using USSC.Infrastructure.Services;
using USSC.Services.UserServices.Interfaces;

namespace USSC.Services
{
    public class SeedData : ISeedData
    {
        private readonly ILogger<SeedData> _logger;
        private readonly AppDbContext _context;
        private readonly IRegistrationService _registrationService;
        private readonly IApplicationDataService _applicationData;

        private readonly List<Role> _defaultRoles = new List<Role>
        {
            new Role() { Name = "Администратор"},
            new Role() { Name = "Пользователь" }
        };

        private readonly List<Subsystem> _subsystems = new List<Subsystem>()
        {
            new Subsystem() { Name = Constants.AdminSubsystem },
            new Subsystem() { Name = Constants.PersonalSubsystem },
            new Subsystem() { Name = Constants.TransportSubsystem },
            new Subsystem() { Name = Constants.DocumentSubsystem },
            new Subsystem() { Name = Constants.RegistrySubsystem }
        };

        public SeedData(ILogger<SeedData> logger, AppDbContext context, IRegistrationService registrationService, IApplicationDataService applicationData)
        {
            _logger = logger;
            _context = context;
            _registrationService = registrationService;
            _applicationData = applicationData;
        }

        public void Initialise()
        {
            _logger.LogInformation("Seeding database");

            if (_context.Database.GetPendingMigrations().Any())
            {
                _logger.LogInformation("Migrating database");
                _context.Database.Migrate();
                _logger.LogInformation("Migrated successfully");
            }

            AddDefaultRoles();
            AddSubsystems();
            AddDefaultPermissions();
            AddSuperUser().Wait();

            _logger.LogInformation("Database seeded successfully");
        }

        private void AddDefaultRoles()
        {
            if (!_context.Roles.Any())
            {
                _logger.LogInformation("Adding default roles");

                _context.Roles.AddRange(_defaultRoles);
                _context.SaveChanges();

                _logger.LogInformation("Default roles added successfully");
            }
        }

        private void AddSubsystems()
        {
            if (!_context.Subsystems.Any())
            {
                _logger.LogInformation("Adding subsystems");

                _context.Subsystems.AddRange(_subsystems);
                _context.SaveChanges();

                _logger.LogInformation("Subsystems Added successfully");
            }
        }

        private void AddDefaultPermissions()
        {
            if (!_context.RoleSubsystems.Any())
            {
                _logger.LogInformation("Adding default permissions");

                foreach (var subsystem in _subsystems)
                {
                    if (subsystem.Name == "Администрирование") 
                        continue;
                    
                    var subsystemEntity = _applicationData.Data.Subsystems.GetSubsystemByName(subsystem.Name);
                    var userRole = _applicationData.Data.Users.FindRole(_defaultRoles[1].Name).Result;
                    _applicationData.Data.Subsystems.IssuePermission(userRole, subsystemEntity);
                }

                foreach (var subsystem in _subsystems)
                {
                    var adminRole = _applicationData.Data.Users.FindRole(_defaultRoles[0].Name).Result;
                    _applicationData.Data.Subsystems.IssuePermission(adminRole, subsystem);
                }

                _logger.LogInformation("Default permissions added successfully");
            }
        }

        private async Task AddSuperUser()
        {
            if (!_context.Users.Any())
            {
                _logger.LogInformation("Registration superuser");

                await _registrationService.RegisterUser("admin", "Главный", "администратор", "123",
                    new List<string> {_defaultRoles[0].Name});

                _logger.LogInformation("Superuser added successfully");
            }
        }
    }
}
