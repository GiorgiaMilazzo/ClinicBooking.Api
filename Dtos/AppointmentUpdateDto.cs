using ClinicBooking.Api.Models;
// credo un Dto a parte per Update perché i campi che ha senso creare e quelli che ha senso modificare dopo non sono sempre gli stessi
// Dtos/AppointmentUpdateDto.cs
namespace ClinicBooking.Api.Dtos
{
    public class AppointmentUpdateDto
    {
        public required DateTime DateTimeAppointment { get; set; }
        public required AppointmentStatus Status { get; set; }
    }
}