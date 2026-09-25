namespace ClinicBooking.Api.Dtos
{
    public class PatientCreateDto
    {
        // required obbliga chi crea l'oggetto a valorizzare quel campo esplicitamente, altrimenti è errore
        // Regola pratica: mettilo su ogni campo che deve avere per forza un valore sensato per esistere davvero
        public required string Name { get; set; }
        public required string Lastname { get; set; }
        public required DateTime DateOfBirth { get; set; }
    }
}