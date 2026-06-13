namespace Menu.DTOs;

public class LoginInputDto
{
    public string NombreUsuario { get; set; } = string.Empty;

    public string Clave { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = "/";
}
