namespace FlvPlayer
{
  public class PlayerParameters
  {
    public string FlvUrl { get; set; }
    /// <summary>
    /// The Video Capture Image Url
    /// </summary>
    public string CaptureUrl { get; set; }
    /// <summary>
    /// Is Auto Play, Default is true
    /// </summary>
    public bool IsAutoPlay { get; set; }
    /// <summary>
    /// Is Allow Full Screen, Default is true
    /// </summary>
    public bool IsAllowFullScreen { get; set; }

    public PlayerParameters()
    {
      IsAutoPlay = true;
      IsAllowFullScreen = true;
    }

    public PlayerParameters(string flvUrl)
    {
      IsAutoPlay = true;
      IsAllowFullScreen = true;
      FlvUrl = flvUrl;
    }
  }
}
