using System.Threading.Tasks;

namespace DynamicTranslate.Translation
{
    public interface ITranslateEngine
    {
        Task<string[]> TranslateAsync(string[] input, string targetLanguageCode, string sourceLanguageCode = "en");

    }
}
