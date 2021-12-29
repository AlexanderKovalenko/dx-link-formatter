public class LinkProcessingInfo {
    public string Host { get; set; }
    public string StaticTitle { get; set; }
    public bool UseFragment { get; set; }
    public int SegmentIndex { get; set; }
    public bool ParseTitleAttribute { get; set; }

    public string RemoveChars { get; set; }
    public string ReplaceChars { get; set; }
    public string TrimChars { get; set; }
    public bool SplitCamelCase { get; set; }
    public string SplitAndTake { get; set; }
    public bool CapitalizeWords { get; set; }
}