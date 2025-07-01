namespace PasswordGeneratorBot.Interfaces
{
    public interface ITelegramBotService
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
