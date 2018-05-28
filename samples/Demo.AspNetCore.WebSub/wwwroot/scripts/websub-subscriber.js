var WebSub = WebSub || {};

WebSub.Subscriber = (function () {
    let consoleOutput;

    function initializeConsole() {
        consoleOutput = document.getElementById('output');
        document.getElementById('clear').addEventListener('click', clearConsole);
    };

    function clearConsole() {
        while (consoleOutput.childNodes.length > 0) {
            consoleOutput.removeChild(consoleOutput.lastChild);
        }
    };

    function writeToConsole(text) {
        var paragraph = document.createElement('p');
        paragraph.style.wordWrap = 'break-word';
        paragraph.appendChild(document.createTextNode(text));

        consoleOutput.appendChild(paragraph);
    };

    return {
        initialize: function () {
            initializeConsole();
        }
    };
})();

WebSub.Subscriber.initialize();
