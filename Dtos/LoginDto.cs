namespace ClinicBooking.Api.Dtos
{
    public class LoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }   // password in CHIARO — mai PasswordHash qui
    }
}