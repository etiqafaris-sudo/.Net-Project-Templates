// Full-page navigation that does not go through Blazor's NavigationManager JS interop.
// Use this instead of NavigationManager.NavigateTo(url, forceLoad: true) on interactive circuits
// to avoid TaskCanceledException when the circuit disconnects during unload.
window.forceNavigate = (url) => {
    window.location.assign(url);
};
