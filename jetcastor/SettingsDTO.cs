namespace jetcastor;

public record SettingsDTO
{ 
    public bool AbortOnLowSpace { get; set; }
    public Double MinFreeSpace { get; set; }
    public SettingsDTO (bool abortOnLowSpace, Double minFreeSpace)
    {
        AbortOnLowSpace = abortOnLowSpace;
        MinFreeSpace = minFreeSpace;
    }
  
}

