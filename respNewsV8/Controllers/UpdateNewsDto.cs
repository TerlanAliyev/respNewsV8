namespace respNewsV8.Controllers
{
    public class UpdateNewsDto
    {
        public string NewsTitle { get; set; }
        public string NewsContetText { get; set; }
        public int NewsCategoryId { get; set; }
        public int NewsLangId { get; set; }
        public int NewsRating { get; set; }
        public string NewsYoutubeLink { get; set; }
        public string NewsPhotoBase64 { get; set; } // Base64 string
    }
}
