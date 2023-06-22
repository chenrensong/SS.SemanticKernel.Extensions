using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Note that in order to use this class you will first need to run the script that downloads headless  webkit, chromium, etc to your machine
/// You will get an exception with details on how to do this on first run
/// </summary>
public class HtmlDataSource : IDataSource {
    public string Uri { get; private set; }

    public HtmlDataSource(string uri) {
        this.Uri = uri;
    }

    public async Task<IEnumerable<ContentResource>> Load() {
        var toReturn = new List<TextResource>();

        using var playwright = await Playwright.CreateAsync();
        {
            try {
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

                var page = await browser.NewPageAsync(new BrowserNewPageOptions { UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.39" });
                await page.GotoAsync(this.Uri);

                var content = await page.TextContentAsync("body");
                // 使用正则表达式替换多个换行为一个换行
                content = Regex.Replace(content, @"(\r?\n){2,}", "\n");
                // 使用正则表达式替换多个空格为一个空格
                content = Regex.Replace(content, @"\s{2,}", " ");
                // 使用正则表达式匹配 JSON
                string pattern = @"\{(?:[^{}]|(?<open>\{)|(?<close-open>\}))+(?(open)(?!))\}";
                MatchCollection matches = Regex.Matches(content, pattern);

                // 遍历匹配项并从 cleanedContent 中删除 JSON
                foreach (Match match in matches) {
                    content = content.Replace(match.Value, "");
                }
                toReturn.Add(new TextResource {
                    Id = this.Uri,
                    Value = content.Trim(),
                    ContentType = "text/html"
                });
            }
            catch (Exception) {

            }
        }

        return toReturn;
    }
}