namespace Testrepository.Server.Models.DTO;

public class CreateProjectDTO
{
    public string sub { get; set; }
    public string title { get; set; }
    
    
    public CreateProjectDTO() { }
    
    public CreateProjectDTO(string sub, string title)
    {
        this.sub = sub;
        this.title = title;
    }
}