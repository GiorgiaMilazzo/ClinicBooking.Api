// Dtos/DoctorDto.cs — cosa restituisci in uscita (GET/POST/PUT response)
namespace ClinicBooking.Api.Dtos
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public DateTime RegistrationDate { get; set; }
        // niente lista Appointments qui: è proprio quello che causava il ciclo infinito
    }
}