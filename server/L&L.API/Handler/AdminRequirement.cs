using Microsoft.AspNetCore.Authorization;

namespace L_L.API.Handler
{
    public class AdminRequirement : IAuthorizationRequirement
    {
        public string RequiredRole { get; }

        public AdminRequirement(string requiredRole)
        {
            RequiredRole = requiredRole;
        }
    }

}
