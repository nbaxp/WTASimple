namespace WTA.Shared.Authentication;

public interface IAuthenticationService
{
    AuthenticateResult Authenticate(string name, string operation);
}
