addEventListener("message", e => {
    let input = e.data;

    let result = work(input);

    postMessage(result)
});

function work(input) {
    let result = 0;
    for (let i = 0; i < input.length; i++) {
        result += input.charCodeAt(i);
    }
    return result;
}