using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using StructureMap;
using Zazz.Core.Interfaces.Repositories;

namespace Zazz.Web
{
    public class ZazzRoleProvider : RoleProvider
    {
        private IUserRoleRepository _repository
        {
            get { return ObjectFactory.Container.GetInstance<IUserRoleRepository>(); }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return _repository.GetUserRoles(username)
                       .Any(r => r.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
        }

        public override string[] GetRolesForUser(string username)
        {
            return _repository.GetUserRoles(username).ToArray();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }
    }
}