namespace InternPortal.Application.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();  
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; }  = string.Empty;
    }
}
