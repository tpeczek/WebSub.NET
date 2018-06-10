var WebSub = WebSub || {};

WebSub.Subscriber = (function () {
    let consoleOutput;
    let subscribeForm;

    function initializeSubscribeForm() {
        subscribeForm = document.getElementById('subscribe-form');

        document.getElementById('subscribe-submit').addEventListener('click', function(event) {
            event.preventDefault();
            event.stopPropagation();

            fetch(subscribeForm.getAttribute('action'), {
                body: new FormData(subscribeForm),
                method: 'post',
            })
            .then(function(response) {
                response.json().then(function (data) {
                    let message;

                    if (data.subscribed) {
                        message = 'Subscribe request has succeed:';
                        message += '\n- Id: ' + data.subscription.id;
                        message += '\n- Topic: ' + data.subscription.topicUrl;
                        message += '\n- Hub: ' + data.subscription.hubUrl;
                        message += '\n- Callback: ' + data.subscription.callbackUrl;
                    } else if (data.errors) {
                        message = 'Subscribe request has failed:';
                        for (let errorIndex = 0; errorIndex < data.errors.length; errorIndex++) {
                            message += '\n- ' + data.errors[errorIndex];
                        }
                    }

                    writeToConsole(message);
                });
            });
        }, false);
    };

    function initializeConsole() {
        consoleOutput = document.getElementById('output');
        document.getElementById('clear').addEventListener('click', clearConsole);
    };

    function initializeServerSentEvents() {
        if (!!window.EventSource) {
            var webSubWebHookIncomingSource = new EventSource('/sse/webhooks/incoming/websub');

            webSubWebHookIncomingSource.onmessage = function(event) {
                writeToConsole(event.data);
            };
        }
    };

    function clearConsole() {
        while (consoleOutput.childNodes.length > 0) {
            consoleOutput.removeChild(consoleOutput.lastChild);
        }
    };

    function writeToConsole(text) {
        var paragraph = document.createElement('p');
        paragraph.style.wordWrap = 'break-word';
        paragraph.style.whiteSpace = 'pre';
        paragraph.appendChild(document.createTextNode(text));

        consoleOutput.appendChild(paragraph);
    };

    return {
        initialize: function () {
            initializeConsole();
            initializeSubscribeForm();
            initializeServerSentEvents();
        }
    };
})();

WebSub.Subscriber.initialize();
