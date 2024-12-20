﻿using System.ComponentModel.DataAnnotations;

namespace SurveyNest.Api.DTO;

public class LoginDTO
{
    [Required]
    public string? UserName { get; set; }

    [Required]
    public string? Password { get; set; }
}
