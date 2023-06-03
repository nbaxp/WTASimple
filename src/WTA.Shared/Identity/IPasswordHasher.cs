namespace WTA.Shared.Identity;

public interface IPasswordHasher
{
    string CreateSalt();

    string HashPassword(string password, string salt);
}