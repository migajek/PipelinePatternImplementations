using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PipelineImplementations.PartX
{
    public class TplAsyncPipeline<TInput, TOutput>
    {
        readonly ITargetBlock<TInput> _firstStep;
        readonly Task<TOutput> _completionTask;

        public TplAsyncPipeline(ITargetBlock<TInput> firstStep, Task<TOutput> completionTask)
        {
            _firstStep = firstStep;
             _completionTask = completionTask;
        }

        public async Task<TOutput> Execute(TInput input)
        {
            await _firstStep.SendAsync(input);
            
            var result =  await _completionTask;
            return result;
        }
    }
}
