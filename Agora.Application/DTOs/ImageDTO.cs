namespace Agora.Application.DTOs;

public class ImageDTO
{
    public int Id { get; set; }
    public byte[]? Data { get; set; } // Mapping cột [imageFile] binary
}

