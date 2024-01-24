using Schedulers;

namespace AbsEngine.Jobs
{
    public class ActionJob : IJob
    {
        Action _action;

        public ActionJob(Action action)
        {
            _action = action;
        }

        public void Execute()
        {
            _action.Invoke();
        }
    }
}
