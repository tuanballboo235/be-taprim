using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TAPrim.Models;
using Telegram.Bot.Types;
using User = TAPrim.Models.User;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class JwtService : IJwtSService
	{
		private readonly IConfiguration _configuration;

		public JwtService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string GenerateToken(User user)
		{
			var claims = new List<Claim>
			{
				new Claim(
					ClaimTypes.NameIdentifier,
					user.UserId.ToString()
				),

				new Claim(
					ClaimTypes.Name,
					user.Username
				),

				new Claim(
					ClaimTypes.Role,
					user.Role
				)
			};

			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
			);

			var creds = new SigningCredentials(
				key,
				SecurityAlgorithms.HmacSha256
			);

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(
					double.Parse(_configuration["Jwt:ExpireMinutes"]!)
				),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler()
				.WriteToken(token);
		}


	}
}
