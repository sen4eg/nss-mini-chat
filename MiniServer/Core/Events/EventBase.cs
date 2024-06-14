namespace MiniServer.Events; 
using System.Threading.Tasks;

public abstract class EventBase<T>
{
    private readonly Action _logic;

    protected EventBase(Action logic)
    {
        _logic = logic;
    }

    protected abstract Task<T> ExecuteAsync();

    public Task<T> Execute()
    {
        _logic?.Invoke();
        return ExecuteAsync();
    }
}