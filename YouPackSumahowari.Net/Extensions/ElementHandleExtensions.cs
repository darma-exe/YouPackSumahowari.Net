using PuppeteerSharp;

namespace YouPackSumahowari.Net.Extensions;

public static class ElementHandleExtensions
{
    public static async Task<string> GetAttributeAsync(this IElementHandle element, string attributeName)
    {
        return await element.EvaluateFunctionAsync<string>($"e => e.getAttribute('{attributeName}')");
    }

    public static async Task<string> GetTextContentAsync(this IElementHandle element)
    {
        return await element.EvaluateFunctionAsync<string>("e => e.textContent");
    }
}