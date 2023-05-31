namespace WTA.Infrastructure.Authentication;

public interface IAuthenticationService
{
    AuthenticateResult Authenticate(string name, string operation);
}