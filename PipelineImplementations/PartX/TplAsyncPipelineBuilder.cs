using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PipelineImplementations.PartX
{
    public class TplAsyncPipelineBuilder<TInput> : TplAsyncPipelineBuilder<TInput, TInput> { }

    public class TplAsyncPipelineBuilder<TFirstInput, TCurrentInput>
    {
        private IDataflowBlock _firstStep;
        private IDataflowBlock _lastStep;        

        public TplAsyncPipelineBuilder() : this(null, null)
        {

        }
        private TplAsyncPipelineBuilder(IDataflowBlock firstStep, IDataflowBlock lastStep)
        {
            _firstStep = firstStep;
            _lastStep = lastStep;
        }

        private void PushStep(IDataflowBlock step){
            _firstStep = _firstStep ?? step;
            _lastStep = step;
        }

        public TplAsyncPipelineBuilder<TFirstInput, TOutput> AddStep<TOutput>(Func<TCurrentInput, Task<TOutput>> transformAsync)
        {
            switch (_lastStep)
            {
                case null:
                    var initialBlock = new TransformBlock<TCurrentInput, Task<TOutput>>(async (input) => await transformAsync(input));
                    PushStep(initialBlock);
                    break;

                case ISourceBlock<Task<TCurrentInput>> asyncSourceBlock:
                    var newAsyncBlock = new TransformBlock<Task<TCurrentInput>, Task<TOutput>>(async (input) => await transformAsync(await input));
                    asyncSourceBlock.LinkTo(newAsyncBlock);
                    PushStep(newAsyncBlock);
                    break;

                case ISourceBlock<TCurrentInput> sourceBlock:
                    var newBlock = new TransformBlock<TCurrentInput, Task<TOutput>>(async (input) => await transformAsync(input));
                    sourceBlock.LinkTo(newBlock);
                    PushStep(newBlock);
                    break;
            }
            return new TplAsyncPipelineBuilder<TFirstInput, TOutput>(_firstStep, _lastStep);
        }

        public TplAsyncPipelineBuilder<TFirstInput, TOutput> AddStep<TOutput>(Func<TCurrentInput, TOutput> transform)
            => AddStep(input => Task.FromResult(transform(input)));

        public TplAsyncPipeline<TFirstInput, TCurrentInput> Build()
        {
            var completionTask = new TaskCompletionSource<TCurrentInput>();
            var firstStep = _firstStep as ITargetBlock<TFirstInput>;

            AddStep(input =>
            {
                completionTask.SetResult(input);
                return input;
            });

            return new TplAsyncPipeline<TFirstInput, TCurrentInput>(firstStep, completionTask.Task);
        }
    }
}
