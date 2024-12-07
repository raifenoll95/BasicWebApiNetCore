namespace progressApp.Models // O usa el namespace que hayas definido para tu proyecto
{
    public class LoginDto
    {   
        required
        public string Email { get; set; }

        required
        public string Password { get; set; }
    }
}