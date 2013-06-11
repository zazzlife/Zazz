﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IUserService
    {
        AccountType GetUserAccountType(int userId);

        int GetUserId(string username);

        User GetUser(string username, bool includeDetails = false, bool includeClubDetails = false,
                     bool includeWeeklies = false, bool includePreferences = false);

        User GetUser(int userId, bool includeDetails = false, bool includeClubDetails = false,
                     bool includeWeeklies = false, bool includePreferences = false);

        IEnumerable<UserSearchResult> Search(string name);

        byte[] GetUserPassword(int userId);

        string GetUserDisplayName(int userId);

        string GetUserDisplayName(string username);
    }
}