namespace respNewsV8.Controllers
{
    public class UploadNewsDto
    {
        public string NewsTitle { get; set; }
        public string NewsContetText { get; set; }
        public int? NewsCategoryId { get; set; }
        public int? NewsLangId { get; set; }
        public int? NewsRating { get; set; }

        public List<IFormFile> NewsPhotos { get; set; } // Base64 ya da URL olaraq
    }
}
