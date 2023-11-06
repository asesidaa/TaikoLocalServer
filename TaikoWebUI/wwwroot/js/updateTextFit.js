// Function to be called when the viewfinder size changes
var title;
var myDonName;
var myDonNameOutline;
var init = false
const observer = new ResizeObserver(handleResize);
function handleResize() {
    updateFit()
}

function waitForElm(selector) {
    return new Promise(resolve => {
        if (document.querySelector(selector)) {
            return resolve(document.querySelector(selector));
        }

        const observer = new MutationObserver(mutations => {
            if (document.querySelector(selector)) {
                observer.disconnect();
                resolve(document.querySelector(selector));
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    });
}

// This is a monkey hack to detect when the page has changed (to stop the observer events)
var pushState = history.pushState;
history.pushState = function () {
    pushState.apply(history, arguments);
    init = false;
    observer.disconnect();
};

// Fetches all the relevant objects on the page so updateFit can reference them later
function initNameplate() {
    if (window.location.href.indexOf("Profile") > -1) {
        waitForElm('#nameplate-title').then((elm) => {
            title = elm
                waitForElm('#nameplate-name').then((elm) => {
                    myDonName = elm
                    waitForElm('#nameplate-name-outline').then((elm) => {
                        myDonNameOutline = elm
                        observer.observe(document.getElementById('nameplate'));
                        init = true
                    });
                });
        });
    }
}

function updateFit() {
    textFit(title, { alignHoriz: true, alignVert: true });
    textFit(myDonName, { alignHoriz: true, alignVert: true });
    textFit(myDonNameOutline, { alignHoriz: true, alignVert: true });
}


// Used to individually update texts on the nameplate
function updateMyDonName(elm) {
    if (init) {
        myDonName.textContent = elm
        myDonNameOutline.textContent = elm
        updateFit()
    }
}
function updateTitle(elm) {
    if (init) {
        title.textContent = elm
        updateFit()
    }
}

// Called by html onload when the image of the nameplate is loaded (this ensures the first fit is properly done)
function nameplateLoaded() {
    initNameplate()
}