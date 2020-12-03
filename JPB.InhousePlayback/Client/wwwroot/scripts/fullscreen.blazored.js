var fullscreenBlazorApi = window["fullscreenBlazorApi"] || (window["fullscreenBlazorApi"] = {});

fullscreenBlazorApi.requestFullscreen = function (element) {
    if (element.requestFullscreen) {
        element.requestFullscreen();
    } else if (element.webkitRequestFullscreen) {
        element.webkitRequestFullscreen();
    } else if (element.mozRequestFullScreen) {
        element.mozRequestFullScreen();
    } else if (element.msRequestFullscreen) {
        element.msRequestFullscreen();
    }
}

fullscreenBlazorApi.getFullscreenElement = function () {
    var el = document.fullscreenElement ||
        document.webkitFullscreenElement ||
        document.mozFullScreenElement ||
        document.msFullscreenElement;
    if (el != null) {
        return el.id;
    }
    return null;
}

fullscreenBlazorApi.hasFullscreen = function (element) {
    return element == fullscreenBlazorApi.getFullscreenElement();
}

fullscreenBlazorApi.exitFullscreen = function () {
    if (!fullscreenBlazorApi.getFullscreenElement()) {
        return;
    }

    // exit full-screen
    if (document.exitFullscreen) {
        document.exitFullscreen();
    } else if (document.webkitExitFullscreen) {
        document.webkitExitFullscreen();
    } else if (document.mozCancelFullScreen) {
        document.mozCancelFullScreen();
    } else if (document.msExitFullscreen) {
        document.msExitFullscreen();
    }
}
fullscreenBlazorApi.fullscreenChangedEvent = [];

fullscreenBlazorApi.invokeFullscreenChangedEvent = function () {
    var toRemove = [];
    fullscreenBlazorApi.fullscreenChangedEvent.forEach(f => {
        try {
            f.invoker.invokeMethodAsync("onFullscreenChangedCallback");
        } catch (e) {
            toRemove.push(f);
        }
    });
    toRemove.forEach(e => {
        fullscreenBlazorApi.fullscreenChangedEvent.splice(fullscreenBlazorApi.fullscreenChangedEvent.indexOf(e), 1);
    });
}

fullscreenBlazorApi.subscribeToFullscreenChangedEvent = function (that) {
    var key = Date.now().toString(36) + Math.random().toString(36).substring(2);
    fullscreenBlazorApi.fullscreenChangedEvent.push({
        id: key,
        invoker: that
    });
}

fullscreenBlazorApi.unsubscribeToFullscreenChangedEvent = function (key) {
    var find = fullscreenBlazorApi.fullscreenChangedEvent.find(f => f.id == key);
    fullscreenBlazorApi.fullscreenChangedEvent.splice(fullscreenBlazorApi.fullscreenChangedEvent.indexOf(find), 1);
}

document.addEventListener("fullscreenchange", fullscreenBlazorApi.invokeFullscreenChangedEvent);
document.addEventListener("webkitfullscreenchange", fullscreenBlazorApi.invokeFullscreenChangedEvent);
document.addEventListener("mozfullscreenchange", fullscreenBlazorApi.invokeFullscreenChangedEvent);
document.addEventListener("MSFullscreenChange", fullscreenBlazorApi.invokeFullscreenChangedEvent);

