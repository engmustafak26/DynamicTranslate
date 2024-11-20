using GTranslate.Translators;
using System;
using System.Threading.Tasks;

namespace DynamicTranslate.Translation
{
    internal class DefaultTranslateEngine : ITranslateEngine
    {

        public async Task<string[]> TranslateAsync(string[] input, string toLanguageCode, string fromLanguageCode)
        {
            try
            {
                if (input is null || input.Length == 0)
                    return new string[0];
                const string delimiter = " \r\n|||\r\n ";

                var translator = new AggregateTranslator();

                var result = await translator.TranslateAsync(string.Join(delimiter, input), toLanguageCode, fromLanguageCode);
                var translateString = result.Translation;//.Replace("| 0|", delimiter).Replace("| 0 |", delimiter).Replace("|0 |", delimiter);
                var returnTranslation = translateString.Split(new string[] { delimiter }, StringSplitOptions.None);

                return returnTranslation;
            }
            catch (Exception ex)
            {
                return input;
            }
        }
    }
}
