using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ServenteDaYasmim.Services
{
    public class TelegramBotService
    {
        private readonly ITelegramBotClient _client;
        private readonly string[] _commands;
        private readonly UnimedService _unimedService;

        public TelegramBotService()
        {
            _unimedService = new UnimedService();
            _commands = new string[] { "/faturarguias" };
            _client = new TelegramBotClient("5492016075:AAEjWLLYyp1MIwWBijRN3I2bnkIf7MkcRAY");
        }

        public async Task InitBotAsync()
        {
            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await _client.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Message is not { } message)
                    return;

                // Only process text messages
                if (message.Text is not { } messageText)
                    return;

                if (!messageText.StartsWith("/"))
                {
                    await _client.SendTextMessageAsync(chatId: message.Chat.Id, text: "Comando não encontrado.", cancellationToken: cancellationToken);
                    return;
                }

                if (messageText.StartsWith("/faturarguias"))
                {
                    await FaturarGuiasCommand(message, cancellationToken);
                    return;
                }

                await _client.SendTextMessageAsync(chatId: message.Chat.Id, text: "Comando não encontrado.", cancellationToken: cancellationToken);

                //// Echo received message text
                //Message sentMessage = await botClient.SendTextMessageAsync(
                //    chatId: chatId,
                //    text: "You said:\n" + messageText,
                //    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                   chatId: update.Message.Chat.Id,
                    text: $"Erro no processamento. {ex?.Message}",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task FaturarGuiasCommand(Message message, CancellationToken cancellationToken)
        {
            string guias = string.Empty;
            try
            {
                guias = message.Text.Split(" ")[1];

            }
            catch (Exception)
            {
                await _client.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Erro no comando informado. Os números das guias devem vir após o comando /faturarguias e separadas por vírgula. Ex: \n /faturarguias 1234,1234,1234 ",
                    cancellationToken: cancellationToken);

                return;
            }

            await _client.SendChatActionAsync(message.Chat.Id, ChatAction.Typing, cancellationToken: cancellationToken);

            await _client.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Guias informadas: {guias}...",
                cancellationToken: cancellationToken);

            var numeroGuias = guias.Split(",");

            foreach (var numeroGuia in numeroGuias)
            {
                try
                {
                    await _client.SendChatActionAsync(message.Chat.Id, ChatAction.Typing, cancellationToken: cancellationToken);

                    await _client.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"Iniciando faturamento da guia {numeroGuia}...",
                                cancellationToken: cancellationToken);

                    await _client.SendChatActionAsync(message.Chat.Id, ChatAction.Typing, cancellationToken: cancellationToken);

                    _unimedService.FaturarGuia(numeroGuia);

                    await _client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Guia {numeroGuia} faturada com sucesso...",
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    await _client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Erro ao faturar a guia {numeroGuia}. {ex?.Message}",
                        cancellationToken: cancellationToken);
                }
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}
