    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Testrepository.Server.Models.Entities;

    [Table("background_info", Schema = "testschema")]
    public class BackgroundInfoEntity
    {
        [Key]
        public int Id { get; set; }  

        [Required]
        public string Text { get; set; }  

        public string StoryId { get; set; }  
        [ForeignKey("StoryId")]
        public Story Story { get; set; }  
    }