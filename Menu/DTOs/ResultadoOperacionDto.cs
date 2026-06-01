namespace Menu.DTOs;

public class ResultadoOperacionDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public static ResultadoOperacionDto Ok(string message)
    {
        return new ResultadoOperacionDto
        {
            Success = true,
            Message = message
        };
    }

    public static ResultadoOperacionDto Fail(string message)
    {
        return new ResultadoOperacionDto
        {
            Success = false,
            Message = message
        };
    }
}