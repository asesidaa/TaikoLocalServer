// Function to be called when the viewfinder size changes
var title;
var myDonName;
var myDonNameOutline;
var scoreboard;
var init = false

const nameplateObserver = new ResizeObserver(handleNameplateResize);
const scoreboardObserver = new ResizeObserver(handleScoreboardResize);

function handleNameplateResize() {
    updateNameplate();
}

function handleScoreboardResize() {
    updateScoreboard();
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
    nameplateObserver.disconnect();
    scoreboardObserver.disconnect();
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
                        nameplateObserver.observe(document.getElementById('nameplate'));
                        init = true
                    });
                });
        });
    }
}

function initScoreboard() {
    if (window.location.href.indexOf("Profile") > -1) {
        waitForElm('#scoreboard').then((elm) => {
            scoreboard = elm
            scoreboardObserver.observe(elm);
        });
    }
}

function updateNameplate() {
    if (title.offsetWidth > 0 && title.offsetHeight > 0) {
        textFit(title, { alignHoriz: true, alignVert: true });
        textFit(myDonName, { alignHoriz: true, alignVert: true });
        textFit(myDonNameOutline, { alignHoriz: true, alignVert: true });
    }
}

function updateScoreboard() {
    if (scoreboard.offsetWidth > 0 && scoreboard.offsetHeight > 0) {
        var row = scoreboard.children;
        for (var i = 0; i < row.length; i++) {
            var column = row[i].children;
            for (var j = 0; j < column.length; j++) {
                var texts = column[j].children;
                for (var k = 0; k < texts.length; k++) {
                    if (texts[k].offsetWidth > 0 && texts[k].offsetHeight > 0) {
                        textFit(texts[k], { alignHoriz: true, alignVert: true });
                    }
                }
            }
        }
    }
}

// Used to individually update texts on the nameplate
function updateMyDonName(elm) {
    if (init) {
        myDonName.textContent = elm
        myDonNameOutline.textContent = elm
        updateNameplate()
    }
}
function updateTitle(elm) {
    if (init) {
        title.textContent = elm
        updateNameplate()
    }
}

// Called by html onload when the image of the nameplate is loaded (this ensures the first fit is properly done)
function nameplateLoaded() {
    initNameplate();
    initScoreboard();
}