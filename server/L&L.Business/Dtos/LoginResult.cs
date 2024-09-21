using Microsoft.IdentityModel.Tokens;

namespace L_L.Business.Dtos
{
    public class LoginResult
    {
        public bool Authenticated { get; set; }
        public SecurityToken? Token { get; set; }
    }
}
