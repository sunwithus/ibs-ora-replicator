// показать / скрыть элемент по id
toggleElement = (id) => {
    let element = document.getElementById(id);
    console.log("Element is " + id + ": " + element.style.visibility);
    if (element.style.visibility = "hidden") {
        element.style.visibility = "visible";
    }
    else {
        element.style.visibility = "hidden";
    }
    console.log("Element is " + id + ": " + element.style.visibility);
}

showElement = (id) => {
    let element = document.getElementById(id);
    element.style.visibility = "visible";
    console.log("showElement " + id);
}

hideElement = (id) => {
    let element = document.getElementById(id);
    element.style.visibility = "hidden";
    console.log("hideElement " + id);
}

clearTextElement = (id) => {
    let element = document.getElementById(id);
    element.textContent = "";
    console.log("textContent of " + id + ": " + element.style.visibility);
}

window.saveAsFile = (data, filename) => {
    const blob = new Blob([data], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
}

