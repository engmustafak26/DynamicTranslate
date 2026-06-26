using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DynamicTranslate.Demo.Filters
{
    public class TranslationResultFilter : ResultFilterAttribute, IResultFilter
    {
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Get language from header
            var language = GetLanguageFromHeader(context.HttpContext.Request);
            if (context.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                await objectResult.Value.Translate(language);
            }
            await base.OnResultExecutionAsync(context, next);
        }




        private string GetLanguageFromHeader(HttpRequest request)
        {
            // Check Accept-Language header
            var acceptLanguage = request.Headers["Accept-Language"].FirstOrDefault();

            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                // Parse the language code and get first two letters
                var languages = acceptLanguage.Split(',')
                    .Select(l => l.Split(';')[0].Trim())
                    .Where(l => !string.IsNullOrEmpty(l))
                    .ToList();

                var preferredLanguage = languages.FirstOrDefault();

                if (!string.IsNullOrEmpty(preferredLanguage))
                {
                    // Return only first two letters (e.g., "en-US" -> "en", "fr-FR" -> "fr")
                    var twoLetterCode = preferredLanguage.Length >= 2
                        ? preferredLanguage.Substring(0, 2).ToLowerInvariant()
                        : preferredLanguage.ToLowerInvariant();


                    return twoLetterCode;

                }
            }

            // Default language (English)
            return "en";
        }
    }
}
