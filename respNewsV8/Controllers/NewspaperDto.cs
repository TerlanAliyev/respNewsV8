namespace respNewsV8.Controllers
{
    public class newspaperDto
    {
        public string NewspaperTitle { get; set; }
    public string NewspaperLinkFlip { get; set; }
    public DateTime NewspaperDate { get; set; }
    public bool NewspaperStatus { get; set; }
    public string NewspaperPrice { get; set; }
    public IFormFile NewspaperCoverUrl { get; set; }
    public IFormFile NewspaperPdfFile { get; set; } // PDF dosyası için
    }
}
