namespace SistemaRepuestosMaquinas.Business.DTOs;

public record RegisterRequest(string Nombres, string Apellidos, string Correo, string Password);
public record LoginRequest(string Correo, string Password);
public record AuthResponse(string Token, DateTime Expiration, string Rol);
