﻿using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Five_a_side.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private IConfiguration _configuration;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult PostLogin([FromBody] LoginRequest loginRequest)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var secToken = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                null,
                expires: DateTime.Now.AddMinutes(40),
                signingCredentials:credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(secToken);

            return Ok(token);
        }
    }
}
