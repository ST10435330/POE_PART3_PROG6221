
using System;
using System.Collections.Generic;

namespace CybersecurityBotGUI
{
    public class ChatBot
    {
        private Dictionary<string, string> userMemory = new Dictionary<string, string>();
        private Dictionary<string, List<string>> keywordResponses = new Dictionary<string, List<string>>();
        private Random random = new Random();
        private string currentTopic = "";

        public ChatBot()
        {
            InitializeResponses();
        }

        private void InitializeResponses()
        {
            keywordResponses["password"] = new List<string>
            {
                "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.",
                "A good password should be at least 12 characters long and include numbers, symbols, and mixed case letters.",
                "Consider using a password manager to help you create and store strong passwords securely."
            };

            keywordResponses["phish"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organizations.",
                "Never click on links in unexpected emails. Instead, go directly to the company's website.",
                "Phishing attempts often create a sense of urgency. Always verify before taking action."
            };

            keywordResponses["scam"] = new List<string>
            {
                "Scammers often pretend to be tech support or government officials. Never give control of your computer to someone who contacts you.",
                "If an offer seems too good to be true, it probably is. Be skeptical of unexpected prizes or opportunities.",
                "Scammers may pressure you to act quickly. Take your time to verify any suspicious requests."
            };

            keywordResponses["privacy"] = new List<string>
            {
                "Review your privacy settings on social media regularly to control what information is shared.",
                "Be careful about what personal information you share online. Even small details can be used against you.",
                "Consider using privacy-focused browsers and search engines to reduce tracking."
            };
        }

        public string Respond(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please enter a message.";

            input = input.ToLower();

            string sentiment = DetectSentiment(input);

            if (currentTopic == "followup" && (input.Contains("yes") || input.Contains("sure") || input.Contains("more")))
            {
                currentTopic = userMemory.ContainsKey("interest") ? userMemory["interest"] : "";
                return GetKeywordResponse(currentTopic, sentiment);
            }

            foreach (var keyword in keywordResponses.Keys)
            {
                if (input.Contains(keyword))
                {
                    currentTopic = keyword;
                    string response = GetKeywordResponse(keyword, sentiment);

                    if (input.Contains("interested") || input.Contains("like to know") || input.Contains("tell me more"))
                    {
                        userMemory["interest"] = keyword;
                        return $"{response}\n\nI'll remember you're interested in {keyword}. Would you like more information about this?";
                    }

                    return response;
                }
            }

            if (!string.IsNullOrEmpty(currentTopic) && (input.Contains("more") || input.Contains("another") || input.Contains("different")))
            {
                return GetKeywordResponse(currentTopic, sentiment);
            }

            if (userMemory.ContainsKey("interest") && input.Contains("remember"))
            {
                return $"Yes! You mentioned you were interested in {userMemory["interest"]}. Would you like more tips about that?";
            }

            return "I'm not sure I understand. Can you try rephrasing or ask about password safety, phishing, or online scams?";
        }

        private string GetKeywordResponse(string keyword, string sentiment)
        {
            if (!keywordResponses.ContainsKey(keyword))
                return "Sorry, I don't have any information on that topic.";

            var responses = keywordResponses[keyword];
            string response = responses[random.Next(responses.Count)];
            return AdjustForSentiment(response, sentiment);
        }

        private string DetectSentiment(string input)
        {
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("nervous"))
                return "worried";
            if (input.Contains("angry") || input.Contains("frustrated") || input.Contains("annoyed"))
                return "frustrated";
            if (input.Contains("happy") || input.Contains("excited") || input.Contains("great"))
                return "positive";
            if (input.Contains("confused") || input.Contains("unsure") || input.Contains("don't understand"))
                return "confused";

            return "neutral";
        }

        private string AdjustForSentiment(string response, string sentiment)
        {
            switch (sentiment)
            {
                case "worried":
                    return $"I understand this can be worrying. {response} Remember, being aware is the first step to staying safe.";
                case "frustrated":
                    return $"I hear your frustration. Cybersecurity can be challenging. {response} Let me know if you'd like more help.";
                case "confused":
                    return $"I'll try to explain this clearly. {response} Does this help clarify things?";
                case "positive":
                    return $"Great to hear you're engaged with cybersecurity! {response}";
                default:
                    return response;
            }
        }
    }
}
