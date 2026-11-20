namespace CompanyName.Web.Services;

public sealed class LoadingIndicatorService
{
    private int _count;
    private bool _interactionSuppressed;

    public bool IsActive => _count > 0;

    public bool IsVisible => IsActive && !_interactionSuppressed;

    public string? Message { get; private set; }

    public string? MessageKey { get; private set; }

    public event Action? OnChange;

    public void Show(string? message = null, string? messageKey = null)
    {
        Message = message;
        MessageKey = messageKey;
        _count++;
        NotifyStateChanged();
    }

    public void Hide()
    {
        if (_count <= 0)
        {
            return;
        }

        _count--;
        if (_count == 0)
        {
            Message = null;
            MessageKey = null;
            _interactionSuppressed = false;
        }

        NotifyStateChanged();
    }

    public void HideAll()
    {
        if (_count == 0)
        {
            return;
        }

        _count = 0;
        Message = null;
        MessageKey = null;
        _interactionSuppressed = false;
        NotifyStateChanged();
    }

    /// <summary>
    /// Hides the overlay while dialogs or alerts need focus. Cleared when loading count reaches zero.
    /// </summary>
    public void SuppressForInteraction()
    {
        if (!_interactionSuppressed)
        {
            _interactionSuppressed = true;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
