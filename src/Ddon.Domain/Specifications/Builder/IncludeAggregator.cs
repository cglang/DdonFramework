using System.Collections.Generic;

namespace Ddon.Domain.Specifications.Builder
{
    public class IncludeAggregator : IIncludeAggregator
    {
        private readonly List<string> navigationPropertyNames = new();

        public IncludeAggregator(string? navigationPropertyName)
        {
            AddNavigationPropertyName(navigationPropertyName);
        }

        public void AddNavigationPropertyName(string? navigationPropertyName)
        {
            if (!string.IsNullOrEmpty(navigationPropertyName))
            {
                navigationPropertyNames.Add(navigationPropertyName!);
            }
        }

        public string IncludeString
        {
            get
            {
                string output = string.Empty;

                for (int i = 0; i < navigationPropertyNames.Count; i++)
                {
                    output = i == 0 ? navigationPropertyNames[i] : $"{output}.{navigationPropertyNames[i]}";
                }

                return output;
            }
        }
    }
}
