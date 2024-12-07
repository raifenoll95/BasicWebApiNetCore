using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using progressApp.Models;

[ApiController]
[Route("api/[controller]")]
// apiUsers/users
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private string? _key;
    private readonly ILogger<UsersController> _logger;

    public UsersController(AppDbContext context, ILogger<UsersController> logger, IConfiguration configuration)
    {
        this._context = context;
        this._logger = logger;
        this._key = configuration["JwtSettings:Key"];
    }

    // Devolver todos los usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // POST: api/Users (registrar nuevo usuario)
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        // Hashear la contraseña antes de guardarla en la base de datos
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

        // Guardar el usuario con la contraseña hasheada
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUsers", new { id = user.Id }, user);
    }

    // POST: api/users/login
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginDto login)
    {
        // Verificar si el email existe
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
        _logger.LogInformation($"Password entered: {login.Email}");
        if (user == null)
        {
            return Unauthorized("Invalid email");
        }

        // Verificar la contraseña utilizando BCrypt para comparar el hash
        if (!BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash)) // Usa BCrypt para comparar las contraseñas
        {
            return Unauthorized("Invalid password.");
        }

        // Si el login es exitoso, generar un JWT
        var token = GenerateJwtToken(user);

        return Ok(token); // Devuelve el token generado
    }

    //Devolver un token (generalmente un JWT) en lugar de los datos del usuario directamente al realizar el login tiene varias ventajas importantes 
    // relacionadas con la seguridad y la escalabilidad de las aplicaciones modernas.

    // Cómo funciona: El token contiene toda la información necesaria para identificar al usuario (por ejemplo, su ID y roles) y no 
    // requiere que el servidor mantenga un estado o sesión activa para cada usuario.
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email)
        };

        // Comprobar antes que el key no sea null
        if (string.IsNullOrEmpty(this._key))
        {
            throw new InvalidOperationException("JWT key is not initialized or is empty.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "progressApp", // Cambia esto por el nombre de tu aplicación
            audience: "progressApp", // Cambia esto por el nombre de tu aplicación
            claims: claims,
            expires: DateTime.Now.AddHours(1), // El token expira en 1 hora
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // http://localhost:5000/api/users/me. En Authorization va el bareerToken
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        // Obtén el ID del usuario desde el token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Busca al usuario en la base de datos
        var user = await _context.Users.FindAsync(int.Parse(userId!));
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Devuelve los datos del usuario
        return Ok(new {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name
        });
    }

    //Put usuario
    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<User>> UpdateCurrentUser([FromBody] UpdateUserDto updateUserDto)
    {
        // Obtén el ID del usuario desde el token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized("Invalid token: user ID claim not found.");
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid token: unable to parse user ID.");
        }

        // Busca al usuario en la base de datos
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Actualiza los datos del usuario con la información proporcionada en el body
        user.Name = updateUserDto.Name ?? user.Name;  // Si no se pasa el nuevo nombre, se conserva el actual
        user.Email = updateUserDto.Email ?? user.Email;  // Lo mismo con el email
        if (!string.IsNullOrEmpty(updateUserDto.Password))
        {
            // Si se proporciona una nueva contraseña, la hasheamos y la actualizamos
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
        }

        // Guardar los cambios en la base de datos
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Devolver el usuario actualizado
        return Ok(new
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name
        });
    }

    // Delete usuario
    [HttpDelete("me")]
    [Authorize]
    public async Task<ActionResult> DeleteCurrentUser()
    {
        // Obtén el ID del usuario desde el token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized("User not found in token.");
        }

        // Busca al usuario en la base de datos
        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Eliminar al usuario de la base de datos
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        // Retorna una respuesta exitosa
        return NoContent(); // 204 No Content: indica que la operación fue exitosa, pero no se devuelve contenido.
    }
}
