namespace CompanyName.Web.Components.Pages.SampleDemo;

public partial class Loading
{
    private bool _simulationComplete;

    private async Task SimulateLoadAsync()
    {
        _simulationComplete = false;
        await RunWithLoadingAsync(async () =>
        {
            await Task.Delay(2000);
            _simulationComplete = true;
        });
    }
}
