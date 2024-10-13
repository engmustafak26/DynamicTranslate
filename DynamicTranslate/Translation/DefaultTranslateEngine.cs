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
                const string delimiter = @"/\";

                var translator = new GoogleTranslator();

                var result = await translator.TranslateAsync(string.Join(delimiter, input), toLanguageCode, fromLanguageCode);
                return result.Translation.Split(delimiter);
            }
            catch (Exception ex)
            {
                return input;
            }
        }
    }
}
