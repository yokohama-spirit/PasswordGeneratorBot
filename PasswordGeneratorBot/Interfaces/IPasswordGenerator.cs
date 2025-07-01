namespace PasswordGeneratorBot.Interfaces
{
    public interface IPasswordGenerator
    {
        Task<string> GeneratePassword(decimal lenght);
    }
}
