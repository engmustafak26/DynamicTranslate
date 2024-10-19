using GTranslate.Results;
using GTranslate.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTranslate.Translation
{
    internal class DefaultTranslateEngine : ITranslateEngine
    {

        public async Task<string[]> TranslateAsync(string[] input, string toLanguageCode, string fromLanguageCode = "en")
        {
            try
            {
                if (input is null || input.Length == 0)
                    return new string[0];
                const string delimiter = @" ____ ";

                var translator = new GoogleTranslator();

                List<Task<GoogleTranslationResult>> tasks = new List<Task<GoogleTranslationResult>>(input.Length);
                foreach (var token in input)
                {
                    tasks.Add(translator.TranslateAsync(token, toLanguageCode, fromLanguageCode));
                }
                await Task.WhenAll(tasks);
                var result = tasks.Select(x => x.Result.Translation).ToArray();
                return result;
            }
            catch (Exception ex)
            {
                return input;
            }
        }
    }
}
