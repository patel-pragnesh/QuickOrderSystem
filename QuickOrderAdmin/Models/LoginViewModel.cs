﻿using System.ComponentModel.DataAnnotations;

namespace QuickOrderAdmin.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your username"), StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter your password"), StringLength(50)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
