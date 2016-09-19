using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HelpBot
{
    public static class LuisExtensions
    {
        public static bool HasIntent(this LuisResult luisResult)
        {
            bool hasIntent = (luisResult != null && luisResult.TopScoringIntent != null && !string.IsNullOrEmpty(luisResult.TopScoringIntent.Name));
            return hasIntent;
        }

        public static string GetTopIntentName(this LuisResult luisResult)
        {
            if (luisResult.HasIntent())
            {
                return luisResult.TopScoringIntent.Name;
            }
            return null;
        }

        public static string GetEntityValue(this LuisResult luisResult, string entityName)
        {
            if (luisResult.Entities != null && luisResult.Entities.ContainsKey(entityName))
            {
                return luisResult.Entities[entityName].FirstOrDefault().Value;
            }
            return null;
        }
    }
}