// Save messages while dotnet is being instantiated.
let savedMessages = []
function saveMessagesWhileInstantiatingDotnet(e) {
    savedMessages.push(e);
}
self.addEventListener("message", saveMessagesWhileInstantiatingDotnet);

import { dotnet } from "../../_framework/dotnet.js"

let instance = await dotnet.create()

let objects = [];

instance.setModuleImports("boot.js", {
    createObject: () => {
        let newObject = {};
        objects.push(newObject);
        return newObject;
    },
    disposeObject: (obj) => {
        objects = objects.filter(item => item != obj);
    },
    postMessage: (message) => self.postMessage(message),
    registerOnMessage: (handler) => self.addEventListener("message", handler)
});

async function runWorker(e) {
    await instance.runMainAndExit(`${e.data.namespace}.wasm`, [JSON.stringify(e.data)]);
}

// No longer save messages as we switch to handle them as they come in.
self.removeEventListener("message", saveMessagesWhileInstantiatingDotnet);
// Go through events that were saved and run the 
for (const savedMessage of savedMessages) {
    await runWorker(savedMessage);
};

// Now listen for all future messages and process them.
self.addEventListener("message", runWorker);
