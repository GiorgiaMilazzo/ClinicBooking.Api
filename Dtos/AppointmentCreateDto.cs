namespace ClinicBooking.Api.Dtos
{
    public class AppointmentCreateDto
    {
        public required DateTime DateTimeAppointment { get; set; }
        public required int PatientId { get; set; }
        public required int DoctorId { get; set; }
    }
}