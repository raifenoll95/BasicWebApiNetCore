namespace progressApp.Models // O usa el namespace que hayas definido para tu proyecto
{
    public class User
    {
        // La propiedad 'Id' representará la columna 'Id' de la tabla Users.
        public int Id { get; set; }
        
        // Propiedades que representan otras columnas de la tabla Users
        required
        public string Name { get; set; }

        required
        public string Email { get; set; }

        required
        public string PasswordHash { get; set; }
    }
}