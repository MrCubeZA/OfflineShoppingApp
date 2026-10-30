using System.Windows.Input;

namespace OfflineShoppingApp.Shopping.Commands;

public class AsyncCommand : ICommand
{
    private readonly Func<Task> execute;
    private bool isRunning;

    public event EventHandler? CanExecuteChanged;

    public AsyncCommand(Func<Task> execute)
    {
        this.execute = execute;
    }

    public bool CanExecute(object? parameter)
    {
        return !isRunning;
    }

    public async void Execute(object? parameter)
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try
        {
            await execute();
        }
        finally
        {
            isRunning = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
