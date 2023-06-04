namespace WTA.Shared.Authentication;

public class AuthenticateResult
{
    public bool Succeeded { get; set; }
    public bool Failed { get; set; }
    public bool EnableColumnLimit { get; set; }
    public bool EnableRowLimit { get; set; }
    public List<string> Columns { get; set; } = new List<string>();
    public List<string> Rows { get; set; } = new List<string>();
}
