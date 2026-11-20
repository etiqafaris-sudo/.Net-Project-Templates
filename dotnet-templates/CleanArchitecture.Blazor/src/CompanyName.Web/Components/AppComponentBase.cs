using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Common.Models;
using CompanyName.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CompanyName.Web.Components;

public abstract class AppComponentBase<TModel> : OwningComponentBase where TModel : class, new()
{
    private int _localLoadingCount;

    //protected IDal _dal => ScopedServices.GetRequiredService<IDal>();

    [Inject] protected AlertService AlertService { get; set; } = default!;

    [Inject] protected LoadingIndicatorService LoadingIndicator { get; set; } = default!;

    protected IDropdownParameterService DropdownService => ScopedServices.GetRequiredService<IDropdownParameterService>();

    public TModel Model { get; set; } = new();

    protected bool IsLoading => _localLoadingCount > 0;

    protected async Task JSSuccessAsync(string message) => await AlertService.JSSuccessAsync(message);
    protected async Task JSErrorAsync(string message) => await AlertService.JSErrorAsync(message);
    protected async Task JSAlertAsync(string message) => await AlertService.JSAlertAsync(message);
    protected async Task JSWarningAsync(string message) => await AlertService.JSWarningAsync(message);

    /// <summary>
    /// Shows the global overlay spinner over the current screen. Nested calls are counted.
    /// </summary>
    protected void ShowLoading(string? message = null, string? messageKey = null)
    {
        _localLoadingCount++;
        LoadingIndicator.Show(message, messageKey);
    }

    /// <summary>
    /// Hides one ShowLoading call. The overlay stays visible until all nested calls are hidden.
    /// </summary>
    protected void HideLoading()
    {
        if (_localLoadingCount <= 0)
        {
            return;
        }

        _localLoadingCount--;
        LoadingIndicator.Hide();
    }

    /// <summary>
    /// Runs work while the global overlay is visible over the current page.
    /// </summary>
    protected async Task RunWithLoadingAsync(Func<Task> action, string? message = null, string? messageKey = null)
    {
        ShowLoading(message, messageKey);
        try
        {
            await Task.Yield();
            await action();
        }
        finally
        {
            HideLoading();
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Runs work while the global overlay is visible over the current page and returns a result.
    /// </summary>
    protected async Task<TResult> RunWithLoadingAsync<TResult>(Func<Task<TResult>> action, string? message = null, string? messageKey = null)
    {
        ShowLoading(message, messageKey);
        try
        {
            await Task.Yield();
            return await action();
        }
        finally
        {
            HideLoading();
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Global utility to handle file uploads from InputFile component.
    /// Reads the file into a byte array and returns a FileUploadResult.
    /// Default max file size is 5MB (1024 * 1024 * 5).
    /// </summary>
    protected async Task<FileUploadResult?> HandleFileUploadAsync(InputFileChangeEventArgs e, long maxFileSize = 5242880)
    {
        var file = e.File;
        if (file == null)
        {
            return null;
        }

        try
        {
            using var stream = file.OpenReadStream(maxFileSize);
            using var memoryStream = new System.IO.MemoryStream();
            await stream.CopyToAsync(memoryStream);

            return new FileUploadResult
            {
                FileName = file.Name,
                ContentType = file.ContentType,
                Size = file.Size,
                FileContent = memoryStream.ToArray()
            };
        }
        catch (Exception ex)
        {
            await JSErrorAsync($"Failed to upload file: {ex.Message}");
            return null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            while (_localLoadingCount > 0)
            {
                HideLoading();
            }
        }

        base.Dispose(disposing);
    }
}
