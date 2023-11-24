using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Azure.AI.OpenAI
{
    public class CoreCompletionsLogProbabilityModel
    {
        public IReadOnlyList<string> Tokens { get; }

        public IReadOnlyList<float?> TokenLogProbabilities { get; }

        public IReadOnlyList<IDictionary<string, float?>> TopLogProbabilities { get; }

        public IReadOnlyList<int> TextOffsets { get; }

        internal CoreCompletionsLogProbabilityModel(IEnumerable<string> tokens, IEnumerable<float?> tokenLogProbabilities, IEnumerable<IDictionary<string, float?>> topLogProbabilities, IEnumerable<int> textOffsets)
        {
            Azure.Core.Argument.AssertNotNull(tokens, "tokens");
            Azure.Core.Argument.AssertNotNull(tokenLogProbabilities, "tokenLogProbabilities");
            Azure.Core.Argument.AssertNotNull(topLogProbabilities, "topLogProbabilities");
            Azure.Core.Argument.AssertNotNull(textOffsets, "textOffsets");
            Tokens = tokens.ToList();
            TokenLogProbabilities = tokenLogProbabilities.ToList();
            TopLogProbabilities = topLogProbabilities.ToList();
            TextOffsets = textOffsets.ToList();
        }

        internal CoreCompletionsLogProbabilityModel(IReadOnlyList<string> tokens, IReadOnlyList<float?> tokenLogProbabilities, IReadOnlyList<IDictionary<string, float?>> topLogProbabilities, IReadOnlyList<int> textOffsets)
        {
            Tokens = tokens;
            TokenLogProbabilities = tokenLogProbabilities;
            TopLogProbabilities = topLogProbabilities;
            TextOffsets = textOffsets;
        }

        internal static CoreCompletionsLogProbabilityModel DeserializeCompletionsLogProbabilityModel(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            IReadOnlyList<string> tokens = null;
            IReadOnlyList<float?> tokenLogProbabilities = null;
            IReadOnlyList<IDictionary<string, float?>> topLogProbabilities = null;
            IReadOnlyList<int> textOffsets = null;
            foreach (JsonProperty item in element.EnumerateObject())
            {
                if (item.NameEquals(new byte[6] { 116, 111, 107, 101, 110, 115 }))
                {
                    List<string> list = new List<string>();
                    foreach (JsonElement item2 in item.Value.EnumerateArray())
                    {
                        list.Add(item2.GetString());
                    }
                    tokens = list;
                }
                else if (item.NameEquals(new byte[14]
                {
                116, 111, 107, 101, 110, 95, 108, 111, 103, 112,
                114, 111, 98, 115
                }))
                {
                    List<float?> list2 = new List<float?>();
                    foreach (JsonElement item3 in item.Value.EnumerateArray())
                    {
                        if (item3.ValueKind == JsonValueKind.Null)
                        {
                            list2.Add(null);
                        }
                        else
                        {
                            list2.Add(item3.GetSingle());
                        }
                    }
                    tokenLogProbabilities = list2;
                }
                else if (item.NameEquals(new byte[12]
                {
                116, 111, 112, 95, 108, 111, 103, 112, 114, 111,
                98, 115
                }))
                {
                    List<IDictionary<string, float?>> list3 = new List<IDictionary<string, float?>>();
                    foreach (JsonElement item4 in item.Value.EnumerateArray())
                    {
                        if (item4.ValueKind == JsonValueKind.Null)
                        {
                            list3.Add(null);
                            continue;
                        }
                        Dictionary<string, float?> dictionary = new Dictionary<string, float?>();
                        foreach (JsonProperty item5 in item4.EnumerateObject())
                        {
                            if (item5.Value.ValueKind == JsonValueKind.Null)
                            {
                                dictionary.Add(item5.Name, null);
                            }
                            else
                            {
                                dictionary.Add(item5.Name, item5.Value.GetSingle());
                            }
                        }
                        list3.Add(dictionary);
                    }
                    topLogProbabilities = list3;
                }
                else
                {
                    if (!item.NameEquals(new byte[11]
                    {
                    116, 101, 120, 116, 95, 111, 102, 102, 115, 101,
                    116
                    }))
                    {
                        continue;
                    }
                    List<int> list4 = new List<int>();
                    foreach (JsonElement item6 in item.Value.EnumerateArray())
                    {
                        list4.Add(item6.GetInt32());
                    }
                    textOffsets = list4;
                }
            }
            return new CoreCompletionsLogProbabilityModel(tokens, tokenLogProbabilities, topLogProbabilities, textOffsets);
        }

        internal static CoreCompletionsLogProbabilityModel FromResponse(Response response)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(response.Content))
            {
                return DeserializeCompletionsLogProbabilityModel(jsonDocument.RootElement);
            }
        }
    }





}