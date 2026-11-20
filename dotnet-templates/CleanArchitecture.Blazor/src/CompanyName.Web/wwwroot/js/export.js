window.downloadFileFromBase64 = (fileName, base64Str, contentType) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64Str}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.printDashboard = () => {
    window.print();
};
