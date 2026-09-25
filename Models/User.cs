namespace ClinicBooking.Api.Models
{
    public enum UserRole
    {
        admin,   // gestisce prenotazioni, crea/modifica Doctor e Patient
        doctor    // può vedere/gestire solo i propri appuntamenti (funzionalità futura, per ora basta il ruolo)
    }

    public class User
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }   // MAI la password in chiaro — solo il suo hash
        public UserRole Role { get; set; } = UserRole.doctor;   // valore di default se non specificato
    }
}