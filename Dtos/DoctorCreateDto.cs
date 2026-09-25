namespace ClinicBooking.Api.Dtos
{
    public class DoctorCreateDto
    {
        public required string Name { get; set; }
        public required string Lastname { get; set; }
        public string? Specialization { get; set; }
    }
}