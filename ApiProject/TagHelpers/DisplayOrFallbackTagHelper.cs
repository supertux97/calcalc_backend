using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace calcalc.TagHelpers
{
    public class DisplayOrFallbackTagHelper : TagHelper
    {
        public string? Display { get; set; }
        public string Fallback { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            if (String.IsNullOrEmpty(Display))
            {
                output.Content.SetHtmlContent($"<span>{ System.Security.SecurityElement.Escape(Fallback)}</span>");
            }
            else
            {
                output.Content.SetHtmlContent($"<span>{ System.Security.SecurityElement.Escape(Display)}</span>");
            }  
        }
    }
}