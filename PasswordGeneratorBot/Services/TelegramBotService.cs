using PasswordGeneratorBot.Config;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using PasswordGeneratorBot.Interfaces;
using System.Net.Http;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordGeneratorBot.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly TelegramBotConfig _config;
        private readonly IPasswordGenerator _gen;
        private readonly Dictionary<long, GenState> _state;

        public TelegramBotService
            (TelegramBotConfig config,
            IPasswordGenerator gen)
        {
            _config = config;
            _botClient = new TelegramBotClient(_config.Token);
            _gen = gen;
            _state = new Dictionary<long, GenState>();
        }


        //--------------------------------------START------------------------------------------

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

        //--------------------------------------UPDATE------------------------------------------

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            if (update.Message is not { Text: { } text } message)
                return;

            long chatId = message.Chat.Id;


            if (_state.TryGetValue(chatId, out var state))
            {
                await HandleGenerateInputCommand(chatId, text, ct);
                return;
            }

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

        //--------------------------------------START COMMAND------------------------------------------
        
        public async Task HandleStartCommand(long chatId, CancellationToken ct)
        {
            var chat = await _botClient.GetChat(chatId, ct);

            await _botClient.SendMessage(
                chatId: chatId,
                text: $"Привет, {chat.FirstName ?? "друг"}! Это бот для генерации надежных паролей!\n" +
                      "Для генерации пароля пропиши комманду /gen",
                cancellationToken: ct);
        }

        //--------------------------------------GENERATOR------------------------------------------

        public async Task HandleGenerateCommand(long chatId, CancellationToken ct)
        {
            _state[chatId] = new GenState
            {
                Step = 1
            };

            await _botClient.SendMessage(
                chatId: chatId,
                text: "Введите длину пароля, " +
                "который вы хотите получить " +
                "(не менее 8 символов, иначе ненадежно, но и не более 30):",
                cancellationToken: ct);
        }
        public async Task HandleGenerateInputCommand(long chatId, string text, CancellationToken ct)
        {
            if (!_state.TryGetValue(chatId, out var state))
                return;

            switch (state.Step)
            {
                case 1 when decimal.TryParse(text, out var count):

                    if(count < 8)
                    {
                        await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Сколько раз повторять, не менее 8🤬",
                        cancellationToken: ct);
                    }
                    else if(count > 30)
                    {
                        await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Я же сказал, не более 30😡",
                        cancellationToken: ct);
                    }
                    else
                    {
                        var password = await _gen.GeneratePassword(count);
                        await _botClient.SendMessage(
                        chatId: chatId,
                        text: password,
                        cancellationToken: ct);
                        _state.Remove(chatId);
                    }
                    
                    break;

                default:
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Некорректный ввод, попробуйте снова",
                        cancellationToken: ct);
                    break;
            }
        }

        //--------------------------------------DEFAULT------------------------------------------

        public async Task HandleDefaultCommand(long chatId, CancellationToken ct)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "Не понимаю, о чем ты😅",
                cancellationToken: ct);
        }


        //--------------------------------------ERROR------------------------------------------

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return Task.CompletedTask;
        }

    }
}
