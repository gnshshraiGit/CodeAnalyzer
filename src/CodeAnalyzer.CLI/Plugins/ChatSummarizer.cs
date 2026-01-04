using System;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CodeAnalyzer.Plugins
{

    public static class ChatSummarizer
    {
        public static ChatHistorySummarizationReducer GetChatSummarizerPlugin(this Kernel kernel, IConfiguration config)
        {
            var targetCount = int.TryParse(config["ChatHistoryReducer:TargetCount"], out int _targetCount) ? _targetCount : 0;
            var threshholdCount = int.TryParse(config["ChatHistoryReducer:ThresholdCount"], out int _threshholdCount) ? _threshholdCount : 0;
            
            return new ChatHistorySummarizationReducer(kernel.GetRequiredService<IChatCompletionService>(), targetCount, threshholdCount);
        }
    }
}