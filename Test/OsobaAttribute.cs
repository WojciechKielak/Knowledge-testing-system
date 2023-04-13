namespace Client;

public class DisplayOsobaAttribute : Attribute
{
    public string DisplayOsoba { get; }

    public DisplayOsobaAttribute(string displayName)
    {
        DisplayOsoba = displayName;
    }
}