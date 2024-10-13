using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTranslate.Translation
{
    public interface ITranslateEngine
    {
        Task<string[]> TranslateAsync(string[] input, string targetLanguageCode, string sourceLanguageCode = "en");

    }
}
