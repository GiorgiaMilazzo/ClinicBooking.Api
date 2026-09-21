namespace ClinicBooking.Api.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string? Specialization { get; set; } // ? ovvero nullable, opzionale
        public DateTime RegistrationDate { get; set;} // quando è stato inserito

        // Navigaton property: lista delle vidite del medico
        public List<Appointment> Appointments { get; set; } = new();
    }
}