namespace ClinicBooking.Api.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        public List<Appointment> Appointments { get; set; }= new();
    }
}