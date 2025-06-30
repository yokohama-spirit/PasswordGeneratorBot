using PasswordGeneratorBot.Config;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace PasswordGeneratorBot.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly TelegramBotConfig _config;
        private readonly IPasswordGenerator _gen;

        public TelegramBotService
            (TelegramBotConfig config,
            IPasswordGenerator gen)
        {
            _config = config;
            _botClient = new TelegramBotClient(_config.Token);
            _gen = gen;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );

            Console.WriteLine("Бот запущен. Нажмите Ctrl+C для остановки...");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            if (update.Message is not { Text: { } text } message)
                return;

            long chatId = message.Chat.Id;


            switch (text)
            {
                case "/start":
                    await HandleStartCommand(chatId, ct);
                    break;

                case "/gen":
                    await HandleGenerateCommand(chatId, ct);
                    break;

                default:
                    await HandleDefaultCommand(chatId, ct);
                    break;
            }

        }

        public async Task HandleStartCommand(long chatId, CancellationToken ct)
        {
            var chat = await _botClient.GetChat(chatId, ct);

            await _botClient.SendMessage(
                chatId: chatId,
                text: $"Привет, {chat.FirstName ?? "друг"}! Это бот для генерации надежных паролей!\n" +
                      "Для генерации пароля пропиши комманду /gen",
                cancellationToken: ct);
        }

        public async Task HandleGenerateCommand(long chatId, CancellationToken ct)
        {
            var password = await _gen.GeneratePasswordAsync();

            await _botClient.SendMessage(
                chatId: chatId,
                text: password,
                cancellationToken: ct);
        }

        public async Task HandleDefaultCommand(long chatId, CancellationToken ct)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "Не понимаю, о чем ты😅",
                cancellationToken: ct);
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return Task.CompletedTask;
        }

    }
}
