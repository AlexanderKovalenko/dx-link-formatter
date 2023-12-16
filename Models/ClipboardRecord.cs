public class ClipboardRecord {
    public int Id { get; set; }
    public string Content { get; set; }
    public ClipboardContentType ContentType { get; set; }
    //public DateTime Time { get; set; }
}

public enum ClipboardContentType { 
    Text,
    Image,
    FileLink
}