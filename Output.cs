using System.ComponentModel.DataAnnotations;

public class Output
{
    [Key]
    public int Id {get; set;}
    public string URL {get; set;}
    public string ScreenshotPath {get; set;}
    public string SiteDescription {get; set;}
    public string ShortText {get; set;}
}