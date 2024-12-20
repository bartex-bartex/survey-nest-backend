﻿using System.ComponentModel.DataAnnotations;

namespace SurveyNest.Api.DTO;

public class RegisterDTO
{
    [Required]
    public string? UserName { get; set; }

    [Required]
    public string? Password { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}
