using System.ComponentModel.DataAnnotations;

namespace SurveyNest.Api.DTO
{
    public class LoginOAuthDTO
    {
        [Required]
        public string? AccessToken { get; set; }

        [Required]
        public string? Provider { get; set; }
    }
}
