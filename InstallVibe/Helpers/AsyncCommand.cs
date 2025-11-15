using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InstallVibe.Helpers;

/// <summary>
/// Async implementation of ICommand that handles async/await operations properly
/// Prevents multiple concurrent executions and provides error handling
/// </summary>
public class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    public event EventHandler? CanExecuteChanged;

    public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute();
        }
        catch (Exception ex)
        {
            // Log error - in production, use proper logging framework
            System.Diagnostics.Debug.WriteLine($"AsyncCommand error: {ex.Message}");

            // Re-throw to allow view models to handle if needed
            throw;
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Generic async command with parameter support
/// </summary>
public class AsyncCommand<T> : ICommand
{
    private readonly Func<T?, Task> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private bool _isExecuting;

    public event EventHandler? CanExecuteChanged;

    public AsyncCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_isExecuting)
            return false;

        if (_canExecute == null)
            return true;

        T? typedParameter = parameter is T t ? t : default;
        return _canExecute(typedParameter);
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            T? typedParameter = parameter is T t ? t : default;
            await _execute(typedParameter);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AsyncCommand<T> error: {ex.Message}");
            throw;
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/*
 * USAGE EXAMPLES:
 *
 * 1. Simple async command in ViewModel:
 *
 * public ICommand SaveCommand { get; }
 *
 * public MyViewModel()
 * {
 *     SaveCommand = new AsyncCommand(SaveAsync);
 * }
 *
 * private async Task SaveAsync()
 * {
 *     await _repository.SaveGuideAsync(CurrentGuide);
 *     StatusMessage = "Guide saved successfully";
 * }
 *
 * 2. Async command with CanExecute:
 *
 * public ICommand DeleteCommand { get; }
 *
 * public MyViewModel()
 * {
 *     DeleteCommand = new AsyncCommand(DeleteAsync, CanDelete);
 * }
 *
 * private bool CanDelete() => SelectedGuide != null;
 *
 * private async Task DeleteAsync()
 * {
 *     await _repository.DeleteGuideAsync(SelectedGuide.Id);
 *     SelectedGuide = null;
 *     DeleteCommand.RaiseCanExecuteChanged();
 * }
 *
 * 3. Async command with parameter:
 *
 * public ICommand EditStepCommand { get; }
 *
 * public MyViewModel()
 * {
 *     EditStepCommand = new AsyncCommand<int>(EditStepAsync);
 * }
 *
 * private async Task EditStepAsync(int? stepId)
 * {
 *     if (!stepId.HasValue) return;
 *
 *     var step = await _repository.GetStepAsync(stepId.Value);
 *     _navigationService.NavigateToStepEditor(stepId.Value);
 * }
 *
 * 4. XAML Binding:
 *
 * <Button Command="{x:Bind ViewModel.SaveCommand}" Content="Save" />
 * <Button Command="{x:Bind ViewModel.EditStepCommand}"
 *         CommandParameter="{Binding StepId}"
 *         Content="Edit" />
 *
 * 5. Error handling in ViewModel:
 *
 * public ICommand LoadDataCommand { get; }
 *
 * public MyViewModel()
 * {
 *     LoadDataCommand = new AsyncCommand(LoadDataWithErrorHandling);
 * }
 *
 * private async Task LoadDataWithErrorHandling()
 * {
 *     try
 *     {
 *         IsLoading = true;
 *         ErrorMessage = string.Empty;
 *
 *         var guides = await _repository.GetAllGuidesAsync();
 *         Guides = new ObservableCollection<Guide>(guides);
 *     }
 *     catch (Exception ex)
 *     {
 *         ErrorMessage = $"Failed to load data: {ex.Message}";
 *         // Log to telemetry service if available
 *     }
 *     finally
 *     {
 *         IsLoading = false;
 *     }
 * }
 */
