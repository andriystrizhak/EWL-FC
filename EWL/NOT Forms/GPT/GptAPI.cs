﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using EWL;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using OpenAI;
using Eng_Flash_Cards_Learner.NOT_Forms.GPT;
using OpenAI.ObjectModels.ResponseModels;
using DevExpress.XtraSplashScreen;
using EWL.EF_SQLite;

namespace Eng_Flash_Cards_Learner.NOT_Forms
{
    public static class GptApi
    {
        public static event Action<string, IOverlaySplashScreenHandle?>? GPTResponseHandler;
        public static event Action<string, IOverlaySplashScreenHandle?>? GPTErrorHandler;

        static Label CurrentLabel { get; set; } = null!;

        static string MyApiKey 
        { 
            get { return SQLService.Get_GPTApiKey(); }
        }

        const string PromptForTests = "Create four options: first of them is a special word or phrase " +
            "and other three options may be slightly similar in meaning to the special word or phrase, " +
            "but not the same. Do not change special word or phrase. I'll send you several special words or phrases. " +
            "You have to do it with each of them and numerate answers. There's example with 'one' and 'apple' words:\r\n" +
            "1.\r\na) one\r\nb) seven\r\nc) zero\r\nd) one million\r\n\r\n2.\r\na) apple\r\nb) pear\r\nc) banana\r\nd) pumpkin";

        const string PromptForFC = "Create a one short, clear and independent sentence for every special word " +
            "or phrase where special word or phrase is missing. The missing in the sentence is marked with the underline \"_____\" " +
            "And add Ukrainian translation with missing word. Number each line. There's example for 'one' and 'to be' words-phrases:\r\n" +
            "1. I have only ___ orange left. / У мене залишилося тільки один апельсин.\r\n" +
            "2. __ __ honest, I'm not an astronaut. / Чесно кажучи, я не астронавт.";


        public static async Task GetResponseAsStream(Label label, string[] words, GptPurpose purpose)
        {
            CurrentLabel = label;

            var api = GetOpenAIService();
            string prompt = GetPrompt(purpose);
            int maxTokens = words.Length * 20;

            IAsyncEnumerable<ChatCompletionCreateResponse>? completionResult = null;

            try
            {
                completionResult = api.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromUser(PromptForTests + $"\r\nThere's the words:\r\n{string.Join("\r\n", words)}")
                    },
                    MaxTokens = maxTokens
                });
            }
            catch { }

            await foreach (var completion in completionResult)
            {
                if (completion.Successful)
                {
                    UpdateLabelText(completion.Choices[0].Message.Content);
                }
                else
                {
                    if (completion.Error == null)
                        throw new Exception("Unknown Error");

                    UpdateLabelText($"{completion.Error.Code}: {completion.Error.Message}");
                }
            }
        }

        public static async Task GetResponse(string[] words, GptPurpose purpose, IOverlaySplashScreenHandle overlayPHandler)
        {
            var api = GetOpenAIService(overlayPHandler);

            if (api == null) return;

            string prompt = GetPrompt(purpose);
            ChatCompletionCreateResponse? completionResult = null;

            int maxTokens = words.Length * 50;

            try
            {
                completionResult = await api.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem(prompt),
                        ChatMessage.FromUser($"\r\nThere's the words:\r\n{string.Join("\r\n", words)}")
                    },
                    MaxTokens = maxTokens
                });
            }
            catch { }

            if (completionResult == null)
                GPTErrorHandler?.Invoke("No connection", overlayPHandler); //TEMP
            else if (completionResult.Successful)
                GPTResponseHandler?.Invoke(completionResult.Choices[0].Message.Content, overlayPHandler);
            else
            {
                if (completionResult.Error == null)
                    GPTErrorHandler?.Invoke("Якась незрозуміла помилка Х(", overlayPHandler);

                GPTErrorHandler?.Invoke($"{completionResult.Error?.Code}:\n {completionResult.Error?.Message}", overlayPHandler);
            }   
        }

        static OpenAIService? GetOpenAIService(IOverlaySplashScreenHandle? overlayPHandler = null)
        {
            if (MyApiKey == "" || MyApiKey == null) //Якщо значення ключа не вдалося отримати
            {
                GPTErrorHandler?.Invoke("invalid_api_key:\n Бла-бла-бла, все погано", overlayPHandler);
                return null;
            }
            else
            {
                var api = new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = MyApiKey,
                });
                api.SetDefaultModelId(Models.Gpt_3_5_Turbo_1106);
                return api;
            }
        }

        static string GetPrompt(GptPurpose purpose)
            => purpose switch
            {
                GptPurpose.Test => PromptForTests,
                GptPurpose.FlashCards => PromptForFC,
                _ => ""
            };

        private static void UpdateLabelText(string text)
        {
            if (CurrentLabel.InvokeRequired)
                CurrentLabel.Invoke(new Action(() => CurrentLabel.Text += text));
            else
                CurrentLabel.Text = text;
        }
    }
}
