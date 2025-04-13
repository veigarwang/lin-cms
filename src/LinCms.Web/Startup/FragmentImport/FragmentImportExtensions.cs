using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WText = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace LinCms.Startup
{
    public class TextFragment
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }
        [Column(StringLength = -1)]
        public string OriginalText { get; set; } = string.Empty;

        [Column(StringLength = -1)]
        public string? Commentary { get; set; } = null;

        [Column(StringLength = -1)]
        public string Section { get; set; } = string.Empty; // 所属章节
    }

    public class EntryFragment
    {
        public long Id { get; set; }
        public long EntryId { get; set; }
        public long FragmentId { get; set; }
        public int SortOrder { get; set; }
    }

    public class Entry
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<EntryFragment> Fragments { get; set; } = new();
    }

    public class FragmentImporter : IHostedService
    {
        private readonly IFreeSql _freeSql;
        private readonly string _wordPath = @"C:\Users\Veigar\Desktop\《山海经注》 - 副本.docx";
        private readonly List<string> titles = ["南山經第一", "南次一經", "南次二經", "南次三經", "西山經第二", "西次一經", "西次二經", "西次三經", "西次四經", "北山經第三", "北次一經", "北次二經", "北次三經", "東山經第四", "東次一經", "東次二經", "東次三經", "東次四經", "中山經第五", "中次一經", "中次二經", "中次三經", "中次四經", "中次五經", "中次六經", "中次七經", "中次八經", "中次九經", "中次十經", "中次一十一山經", "中次十二經", "海外南經第六", "海外西經第七", "海外北經第八", "海外東經第九", "海内南經第十", "海内西經第十一", "海内北經第十二", "海内東經第十三", "大荒東經第十四", "大荒南經第十五", "大荒西經第十六", "大荒北經第十七", "海内經第十八"];

        public FragmentImporter(IFreeSql freeSql)
        {
            _freeSql = freeSql;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_wordPath))
            {
                Console.WriteLine($"Word 文件未找到: {_wordPath}");
                return;
            }

            var fragments = ExtractFragments(_wordPath);

            // 清空旧数据，重新插入
            //await _freeSql.Delete<EntryFragment>().Where("1=1").ExecuteAffrowsAsync();
            await _freeSql.Delete<TextFragment>().Where("1=1").ExecuteAffrowsAsync();
            await _freeSql.Ado.ExecuteNonQueryAsync("DBCC CHECKIDENT ('text_fragment', RESEED, 0)");
            await _freeSql.Insert(fragments).ExecuteAffrowsAsync();

            Console.WriteLine($"已导入 {fragments.Count} 条片段。\n");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private List<TextFragment> ExtractFragments(string filePath)
        {
            var fragments = new List<TextFragment>();
            using var wordDoc = WordprocessingDocument.Open(filePath, false);
            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body == null) return fragments;

            var regex = new Regex("郭璞注：『((?:[^『』]|「.*?」|『.*?』)*?)』");

            int index = 0;
            string currentSection = string.Empty;
            foreach (var para in body.Elements<Paragraph>())
            {
                if (index++ <= 10) continue;
                var fullText = string.Join("", para.Descendants<WText>().Select(t => t.Text)).Trim();
                if (string.IsNullOrWhiteSpace(fullText)) continue;
                if (titles.Contains(fullText))
                {
                    currentSection = fullText;
                    int chapterIndex = currentSection.IndexOf("第");
                    if (chapterIndex > 0)
                        currentSection = currentSection.Remove(chapterIndex);
                    continue;
                }
                if (fullText == "山海經終") break;

                //fullText = fullText + "<br/>";

                // 拆分为“正文 + 注释”对
                var matches = regex.Matches(fullText);
                if (matches.Count == 0)
                {
                    fragments.Add(new TextFragment
                    {
                        OriginalText = fullText,
                        Section = currentSection
                    });
                    continue;
                }

                int lastIndex = 0;
                var sentenceRegex = new Regex("[^。]*[。]?");
                foreach (Match match in matches)
                {
                    int matchIndex = match.Index;
                    string beforeText = fullText.Substring(lastIndex, matchIndex - lastIndex);
                    string commentary = match.Groups[1].Value;

                    // 提取前文中最后一个句子（以“。”结尾或末尾）作为 originalText                    
                    var sentMatches = sentenceRegex.Matches(beforeText).Cast<Match>().Select(m => m.Value.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

                    if (sentMatches.Count == 1)
                    {
                        string lastSentence = sentMatches.Last();
                        fragments.Add(new TextFragment
                        {
                            OriginalText = lastSentence,
                            Commentary = commentary,
                            Section = currentSection
                        });
                    }
                    else if (sentMatches.Count > 1)
                    {
                        for (int i = 0; i < sentMatches.Count - 1; i++)
                        {
                            fragments.Add(new TextFragment
                            {
                                OriginalText = sentMatches[i],
                                Section = currentSection
                            });
                        }
                        fragments.Add(new TextFragment
                        {
                            OriginalText = sentMatches[sentMatches.Count - 1],
                            Commentary = commentary,
                            Section = currentSection
                        });
                    }

                    lastIndex = match.Index + match.Length;
                }

                // 若末尾还有无注释的文字段落，也补充为片段
                if (lastIndex >= fullText.Length)
                {
                    continue;
                }
                var remaining = fullText.Substring(lastIndex).Trim();
                fragments.Add(new TextFragment
                {
                    OriginalText = remaining,
                    Section = currentSection
                });
            }

            return fragments;
        }
    }

    public static class FragmentImporterExtensions
    {
        public static IServiceCollection AddFragmentImporter(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, FragmentImporter>();
            return services;
        }
    }
}