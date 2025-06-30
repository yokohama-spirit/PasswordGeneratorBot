namespace PasswordGeneratorBot.Services
{
    public interface IPasswordGenerator
    {
        Task<string> GeneratePasswordAsync();
    }
}
