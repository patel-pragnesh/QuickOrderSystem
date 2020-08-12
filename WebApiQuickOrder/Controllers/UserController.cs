﻿using Library.DTO;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApiQuickOrder.Context;

namespace WebApiQuickOrder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly QOContext _context;
        private readonly IConfiguration _configuration;

        public UserController(QOContext context, IConfiguration configuration)
        {
            _context = context;
            this._configuration = configuration;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/User/5
        [HttpGet("[action]/{name}")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUserWithName(string name)
        {
            var _users = await _context.Users.Where(user=>user.Name == name).ToListAsync();

            if (_users == null)
            {
                return NotFound();
            }

            List<UserDTO> userDTOs = new List<UserDTO>();

            foreach (var item in _users)
            {
                var userDTO = new UserDTO()
                {
                    UserId = item.UserId,
                    Name = item.Name,
                    Address = item.Address,
                    Email = item.Email,
                    Gender = item.Gender,
                    Phone = item.Phone
                };

                userDTOs.Add(userDTO);

            }



            return userDTOs;
        }




        // PUT: api/User/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut]
        public async Task<User> PutUser(User user)
        {
            var oldUser = _context.Users.Where(u => u.UserId == user.UserId).FirstOrDefault();

            if (oldUser != null)
            {
                _context.Users.Remove(oldUser);

                _context.Users.Add(user);
                _context.Attach(user.UserLogin);

                if (user.Stores.Count > 0)
                {
                    foreach (var item in user.Stores)
                    {
                        _context.Attach(item);
                    }
                }


                try
                {

                    _context.SaveChanges();
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                }

                return user;
            }
            else
            {
                return null;
            }
        }

        // POST: api/User
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        //[HttpPost]
        //public async Task<ActionResult<User>> PostUser([FromBody]User user)
        //{
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        //}


        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody]User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

             return BuildToken(user);

            //return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        Login AuthenticateUser(Login loginCredentials)
        {
            Login user = _context.Logins.SingleOrDefault(x => x.Username == loginCredentials.Username && x.Password == loginCredentials.Password);
            return user;
        }

        //Genera token de seguridad para los distintos action a los que se tendra acceso.
        //===========================================================================================
        string GenerateJWTToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserId.ToString()),
                new Claim("fullName", userInfo.Name.ToString()),
                new Claim("role","User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("[action]/{username}/{password}")]
        public User CheckUserCredential(string username, string password)
        {
            var loginOfUser = _context.Logins.Where(c => c.Username == username && c.Password == password).FirstOrDefault();


            if (loginOfUser != null)
            {
                if (_context.Users.Count() > 0)
                {
                    var user = _context.Users.Where(u => u.LoginId == loginOfUser.LoginId).Include(s => s.Stores).FirstOrDefault();

                    return user;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
           

        }

        [HttpGet("[action]/{username}/{password}")]
        public TokenDTO LoginCredential(string username, string password)
        {

            IActionResult response = Unauthorized();

            var loginOfUser = _context.Logins.Where(c => c.Username == username && c.Password == password).FirstOrDefault();
            if (loginOfUser != null)
            {

            if (_context.Users.Count() > 0)
            {

               
                var user = _context.Users.Where(u => u.LoginId == loginOfUser.LoginId).Include(s => s.Stores).FirstOrDefault();


                //Login authloging = AuthenticateUser(loginOfUser);


                var tokenString = GenerateJWTToken(user);

                   //SecurityToken securityToken = tokenString;
                    TokenDTO tokenDTO = new TokenDTO()
                    {
                        Token = tokenString,
                        UserDetail = user,
                        
                    };


                    return tokenDTO;
              
                //user.Employees = _context.Employees.Where(e => e.UserId == user.UserId).Include(s => s.EmployeeStore).ToList();
              
            }
            else
            {
                return null;
            }
            }
            else
            {
                return null;
            }

        }

        [HttpGet("[action]/{email}")]
        public bool ForgotCodeSend(string email)
        {
            var result = _context.Users.Where(u => u.Email == email).FirstOrDefault();

            if (result != null)
            {
                var forgotpassword = new ForgotPassword()
                {
                    Code = Guid.NewGuid().ToString(),
                    Email = result.Email,

                };

                _context.ForgotPasswords.Add(forgotpassword);

                _context.SaveChangesAsync();

                var senderEmail = new MailAddress("est.juanpablotorres@gmail.com", "Quick Order");
                var receiverEmail = new MailAddress(result.Email, result.Name);

                var sub = "Quick Order Forgot Password Code";
                var body = "<b>Forgot Secret Code:</b>" + forgotpassword.Code;
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("est.juanpablotorres@gmail.com", "jp84704tt")
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    IsBodyHtml = true,
                    Subject = sub,
                    Body = body
                })
                {
                    smtp.Send(mess);
                }

                return true;

            }
            else
            {
                return false;
            }
        }

        [HttpGet("[action]/{code}")]
        public bool ConfirmCode(string code)
        {
            var result = _context.ForgotPasswords.Where(u => u.Code == code).FirstOrDefault();
            var userInfo = _context.Users.Where(u => u.Email == result.Email).Include(l => l.UserLogin).FirstOrDefault();

            if (result != null)
            {

                var senderEmail = new MailAddress("est.juanpablotorres@gmail.com", "Quick Order");
                var receiverEmail = new MailAddress(userInfo.Email, userInfo.Name);

                var sub = "Loging Credials";
                var body = "<div>Username" + userInfo.UserLogin.Username + "</div>" + "<div>Password" + userInfo.UserLogin.Password + "</div>";
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("est.juanpablotorres@gmail.com", "jp84704tt")
                };
                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    IsBodyHtml = true,
                    Subject = sub,
                    Body = body
                })
                {
                    smtp.Send(mess);
                }

                _context.ForgotPasswords.Remove(result);
                _context.SaveChangesAsync();

                return true;

            }
            else
            {
                return false;
            }
        }

        [HttpGet("[action]/{username}/{password}")]
        public async Task<bool> CheckIfUsernameAndPasswordExist(string username, string password)
        {
            return await _context.Logins.AnyAsync(l => l.Username == username && l.Password == password);
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        private IActionResult BuildToken(User userInfo)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.UserId.ToString()),
                new Claim("miValor", "Lo que yo quiera"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Llave_super_secreta"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(1);

            JwtSecurityToken token = new JwtSecurityToken(
               issuer: "yourdomain.com",
               audience: "yourdomain.com",
               claims: claims,
               expires: expiration,
               signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = expiration
            });

        }
    }
}