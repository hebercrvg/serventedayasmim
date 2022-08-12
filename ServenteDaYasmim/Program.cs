// See https://aka.ms/new-console-template for more information

using ServenteDaYasmim.Services;

var botService = new TelegramBotService();

await botService.InitBotAsync();