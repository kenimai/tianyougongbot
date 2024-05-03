using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI_API;

namespace TainYouGongBot
{
    class Program
    {
        public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient? _client;
        private OpenAIAPI? _api;

        string DiscordToken = "";
        string OpenAIAPIKey = "";

        public async Task MainAsync()
        {
            //建立discord client
            _client = new DiscordSocketClient();
            _client.MessageReceived += CommandHandler;
            _client.Log += Log;

            //取得TOKEN
            DiscordToken = File.ReadAllText("token.txt");
            OpenAIAPIKey = File.ReadAllText("openai.txt");

            await _client.LoginAsync(TokenType.Bot, DiscordToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task<Task> CommandHandler(SocketMessage message)
        {
            _api = new OpenAIAPI(OpenAIAPIKey);

            string command = "";
            int lengthOfCommand = -1;

            if (!message.Content.StartsWith('!'))
                return Task.CompletedTask;

            if (message.Author.IsBot)
                return Task.CompletedTask;

            if (message.Content.Contains(' '))
                lengthOfCommand = message.Content.IndexOf(' ');
            else
                lengthOfCommand = message.Content.Length;

            command = message.Content.Substring(1, lengthOfCommand - 1).ToLower();

            if (command.Equals("hello"))
            {
                _ = message.Channel.SendMessageAsync($@"Hello {message.Author.Mention}");         
            }
            else
            {
                var chat = _api.Chat.CreateConversation();

                chat.AppendUserInput(command);

                await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
                {
                    //Console.Write(res);
                    _ = message.Channel.SendMessageAsync(res);
                }
            }

            return Task.CompletedTask;
        }
    }
}

