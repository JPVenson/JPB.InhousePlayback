using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace JPB.InhousePlayback.Shared.Jwt
{
	public static class JwtCoder
	{
		public static string EncodeToken(JwtSecurityToken jwtToken)
		{
			return new JwtSecurityTokenHandler().WriteToken(jwtToken);
		}

		public static JwtSecurityToken DecodeToken(string token)
		{
			return new JwtSecurityTokenHandler().ReadJwtToken(token);
		}
	}
}
