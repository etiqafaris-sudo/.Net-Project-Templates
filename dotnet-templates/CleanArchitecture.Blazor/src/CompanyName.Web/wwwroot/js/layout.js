window.triggerFileInput = function (elementId) {
    const input = document.getElementById(elementId);
    if (input) {
        input.click();
    }
};

window.initMainNavToggle = function () {
    const toggle = document.getElementById('main-nav-toggle');
    if (!toggle) {
        return;
    }

    toggle.checked = window.matchMedia('(min-width: 641px)').matches;
};
