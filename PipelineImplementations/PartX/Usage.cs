using System;
using System.Linq;
using System.Threading.Tasks;

namespace PipelineImplementations.PartX
{
    public static class Usage
    {
        public static async Task Use()
        {
            const string exampleStr = "The pipeline pattern is the best pattern";

            var mgPipeline = new TplAsyncPipelineBuilder<string>()
                .AddStep(str => FindMostCommonAsync(str))
                .AddStep(str => CountChars(str))
                .AddStep(len => IsOdd(len))
                .Build();

            var result = await mgPipeline.Execute(exampleStr);
            Console.WriteLine($"MG pipeline: {result}");
        }

        private static async Task<string> FindMostCommonAsync(string input)
        {
            await Task.Delay(10);
            return input.Split(' ')
                .GroupBy(word => word)
                .OrderBy(group => group.Count())
                .Last()
                .Key;
        }

        private static int CountChars(string input) => input.Length;

        private static bool IsOdd(int number) => number % 2 == 1;
    }
}