window.JSAlert = (message) => {
    return Swal.fire({
        title: 'Alert',
        text: message,
        icon: 'info',
        confirmButtonText: 'OK'
    });
}

window.JSWarning = (message) => {
    return Swal.fire({
        title: 'Warning',
        text: message,
        icon: 'warning',
        confirmButtonText: 'OK'
    });
}

window.JSSuccess = (message) => {
    return Swal.fire({
        title: 'Success',
        text: message,
        icon: 'success',
        confirmButtonText: 'OK'
    });
}

window.JSError = (message) => {
    return Swal.fire({
        title: 'Error',
        text: message,
        icon: 'error',
        confirmButtonText: 'OK'
    });
}
