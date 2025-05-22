using System.Text.Json.Serialization;


namespace Testrepository.Server.Models.DTO;
   
public class CreateSessionDTO
{
    [JsonPropertyName("projectId")]
    public int projectId { get; set; }
}