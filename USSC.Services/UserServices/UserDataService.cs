﻿using System.Collections.Generic;
using System.Threading.Tasks;
using USSC.Infrastructure.Models;
using USSC.Infrastructure.Services;
using USSC.Services.UserServices.Interfaces;

namespace USSC.Services.UserServices
{
    public class UserDataService : IUserDataService
    {
        private readonly IApplicationDataService _applicationData;

        public UserDataService(IApplicationDataService applicationData)
        {
            _applicationData = applicationData;
        }

        public async Task<User> GetUserData(string email)
        {
            var user = await _applicationData.Data.Users.FindUserByEmail(email);

            return user;
        }

        public async Task<List<Role>> GetUserRoles(int userId)
        {
            var roles = await _applicationData.Data.Users.GetUserRoles(userId);

            return roles;
        }
    }
}
