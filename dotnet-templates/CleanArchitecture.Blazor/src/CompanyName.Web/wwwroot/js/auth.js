window.authLogin = async (email, password) => {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenInput?.value;
    if (!token) {
        return { success: false, error: 'Missing antiforgery token. Please refresh the page and try again.' };
    }

    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ email, password }),
        credentials: 'same-origin'
    });

    if (!response.ok) {
        const data = await response.json().catch(() => ({}));
        return { success: false, error: data.error || 'Login failed.' };
    }

    const data = await response.json();
    return { success: true, fullname: data.fullname };
};
