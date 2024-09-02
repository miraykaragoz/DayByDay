namespace DayByDay.Models
{
    public class Diary
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImgPath { get; set; }
    }
}