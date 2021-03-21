function VideoEx(referrer) {

    //var videoContainer = element; //document.getElementById('video-container');
    //var video = element.getElementsByClassName('video')[0];
    //var videoControls = element.getElementsByClassName('video-controls')[0];
    //var playButton = element.getElementsByClassName('video-ex-play-btn')[0];

    //var playbackIcons = element.getElementsByClassName('playback-icons use');//!!!!!!!
    //var timeElapsed = element.getElementsByClassName('video-ex-time-elapsed')[0];

    // Select elements here
    var videoContainer = document.getElementById(referrer.id);
    var video = videoContainer.querySelector('video');
    var videoControls = videoContainer.querySelector('#video-controls');
    var playButton = videoContainer.querySelector('#play');
    var playbackIcons = videoContainer.querySelectorAll('.playback-icons use');
    var timeElapsed = videoContainer.querySelector('#time-elapsed');
    var duration = videoContainer.querySelector('#duration');
    var progressBar = videoContainer.querySelector('#progress-bar');
    var seek = videoContainer.querySelector('#seek');
    var seekTooltip = videoContainer.querySelector('#seek-tooltip');
    var volumeButton = videoContainer.querySelector('#volume-button');
    var volumeIcons = videoContainer.querySelectorAll('.volume-button use');
    var volumeMute = videoContainer.querySelector('use[href="#volume-mute"]');
    var volumeLow = videoContainer.querySelector('use[href="#volume-low"]');
    var volumeHigh = videoContainer.querySelector('use[href="#volume-high"]');
    var volume = videoContainer.querySelector('#volume');
    var playbackAnimation = videoContainer.querySelector('#playback-animation');
    var fullscreenButton = videoContainer.querySelector('#fullscreen-button');
    var fullscreenIcons = fullscreenButton.querySelectorAll('use');
    var pipButton = videoContainer.querySelector('#pip-button');
    var videoWorks = !!document.createElement('video').canPlayType;
    if (videoWorks) {
        video.controls = false;
        videoControls.classList.remove('hidden');
    }

    // Add functions here

    // togglePlay toggles the playback state of the video.
    // If the video playback is paused or ended, the video is played
    // otherwise, the video is paused
    function togglePlay() {
        if (video.paused || video.ended) {
            video.play();
            startTimeoutForHideControls();
        } else {
            video.pause();
        }
    }

    // updatePlayButton updates the playback icon and tooltip
    // depending on the playback state
    function updatePlayButton() {
        playbackIcons.forEach(icon => icon.classList.toggle('hidden'));

        if (video.paused) {
            playButton.setAttribute('data-title', 'Play (k)')
        } else {
            playButton.setAttribute('data-title', 'Pause (k)')
        }
    }

    // formatTime takes a time length in seconds and returns the time in
    // minutes and seconds
    function formatTime(timeInSeconds) {
        var time = new Date(timeInSeconds * 1000);
        //var result = .toISOString().substr(11, 8);
        var hours = time.getHours() - 1;
        var minutes = time.getMinutes();
        var seconds = time.getSeconds();
        var data = {
            hours: hours,
            minutes: minutes,
            seconds: seconds,
        };
        return data;
    };

    function formatTimeHumanize(data) {
        var format = "";
        var minutes = data.minutes < 10 ? `0${data.minutes}` : data.minutes;
        var seconds = data.seconds < 10 ? `0${data.seconds}` : data.seconds;
        if (data.hours > 0) {
            format = `${data.hours}:${minutes}:${seconds}`;
        }
        else if (data.minutes > 0) {
            format = `${minutes}:${seconds}`;
        }
        else {
            format = `0:${seconds}`;
        }

        return format;
    }

    // initializeVideo sets the video duration, and maximum value of the
    // progressBar
    function initializeVideo() {
        var videoDuration = Math.round(video.duration);
        seek.setAttribute('max', videoDuration);
        progressBar.setAttribute('max', videoDuration);
        var time = formatTime(videoDuration);
        duration.innerText = formatTimeHumanize(time);
        duration.setAttribute('datetime', `${time.hours}h ${time.minutes}m ${time.seconds}s`);
    }

    // updateTimeElapsed indicates how far through the video
    // the current playback is by updating the timeElapsed element
    function updateTimeElapsed() {
        var time = formatTime(Math.round(video.currentTime));
        timeElapsed.innerText = formatTimeHumanize(time);
        timeElapsed.setAttribute('datetime', `${time.hours}h ${time.minutes}m ${time.seconds}s`);
    }

    // updateProgress indicates how far through the video
    // the current playback is by updating the progress bar
    function updateProgress() {
        seek.value = Math.floor(video.currentTime);
        progressBar.value = Math.floor(video.currentTime);
    }

    // updateSeekTooltip uses the position of the mouse on the progress bar to
    // roughly work out what point in the video the user will skip to if
    // the progress bar is clicked at that point
    function updateSeekTooltip(event) {
        var skipTo = Math.round((event.offsetX / event.target.clientWidth) * parseInt(event.target.getAttribute('max'), 10));
        seek.setAttribute('data-seek', skipTo)
        var t = formatTime(skipTo);
        seekTooltip.textContent = formatTimeHumanize(t);
        var rect = video.getBoundingClientRect();
        seekTooltip.style.left = `${event.pageX - rect.left}px`;
    }

    // skipAhead jumps to a different point in the video when the progress bar
    // is clicked
    function skipAhead(event) {
        var skipTo = event.target.dataset.seek
            ? event.target.dataset.seek
            : event.target.value;
        moveTo(skipTo);
    }

    function moveTo(skipTo) {
        video.currentTime = skipTo;
        progressBar.value = skipTo;
        seek.value = skipTo;
    }

    // updateVolume updates the video's volume
    // and disables the muted state if active
    function updateVolume() {
        if (video.muted) {
            video.muted = false;
        }

        video.volume = volume.value;
    }

    // updateVolumeIcon updates the volume icon so that it correctly reflects
    // the volume of the video
    function updateVolumeIcon() {
        volumeIcons.forEach(icon => {
            icon.classList.add('hidden');
        });

        volumeButton.setAttribute('data-title', 'Mute (m)')

        if (video.muted || video.volume === 0) {
            volumeMute.classList.remove('hidden');
            volumeButton.setAttribute('data-title', 'Unmute (m)')
        } else if (video.volume > 0 && video.volume <= 0.5) {
            volumeLow.classList.remove('hidden');
        } else {
            volumeHigh.classList.remove('hidden');
        }
    }

    // toggleMute mutes or unmutes the video when executed
    // When the video is unmuted, the volume is returned to the value
    // it was set to before the video was muted
    function toggleMute() {
        video.muted = !video.muted;

        if (video.muted) {
            volume.setAttribute('data-volume', volume.value);
            volume.value = 0;
        } else {
            volume.value = volume.dataset.volume;
        }
    }

    // animatePlayback displays an animation when
    // the video is played or paused
    function animatePlayback() {
        playbackAnimation.animate([
            {
                opacity: 1,
                transform: "scale(1)",
            },
            {
                opacity: 0,
                transform: "scale(1.3)",
            }
        ], {
            duration: 500,
        });
    }

    // toggleFullScreen toggles the full screen state of the video
    // If the browser is currently in fullscreen mode,
    // then it must be exited and vice versa.
    function toggleFullScreen() {
        if (document.fullscreenElement) {
            document.exitFullscreen();
        } else {
            referrer.that.invokeMethodAsync("onFullscreenRequestButton");
            //videoContainer.requestFullscreen();
        }
        updateFullscreenButton();
    }

    // updateFullscreenButton changes the icon of the full screen button
    // and tooltip to reflect the current full screen state of the video
    function updateFullscreenButton() {
        fullscreenIcons.forEach(icon => icon.classList.toggle('hidden'));

        if (document.fullscreenElement) {
            fullscreenButton.setAttribute('data-title', 'Exit full screen (f)')
        } else {
            fullscreenButton.setAttribute('data-title', 'Full screen (f)')
        }
    }

    // togglePip toggles Picture-in-Picture mode on the video
    async function togglePip() {
        try {
            if (video !== document.pictureInPictureElement) {
                pipButton.disabled = true;
                await video.requestPictureInPicture();
            } else {
                await document.exitPictureInPicture();
            }
        } catch (error) {
            console.error(error)
        } finally {
            pipButton.disabled = false;
        }
    }

    // hideControls hides the video controls when not in use
    // if the video is paused, the controls must remain visible
    function hideControls() {
        if (video.paused) {
            return;
        }
        if (!videoControls.classList.contains("hidden")) {
            videoControls.classList.add('hidden');
            videoControls.classList.add('hide');
            videoContainer.classList.add('hidden-controls');
            referrer.that.invokeMethodAsync("onControlsStatusChange", false);
        }
    }

    // showControls displays the video controls
    function showControls() {
        if (videoControls.classList.contains("hidden")) {
            videoControls.classList.remove('hidden');
            videoControls.classList.remove('hide');
            videoContainer.classList.remove('hidden-controls');
            referrer.that.invokeMethodAsync("onControlsStatusChange", true);
        }
    }

    var timeout = null;
    var lastMousePosition = null;
    var lastMouseWhenStarted = null;

    function onMouseMovement(event) {
        lastMousePosition = {
            x: event.originalEvent.x,
            y: event.originalEvent.y,
        }

        showControls();

        if (timeout == null) {
            lastMouseWhenStarted = lastMousePosition;
            startTimeoutForHideControls();
        }
    }

    function startTimeoutForHideControls() {
        if (timeout) {
            clearTimeout(timeout);
        }

        timeout = setTimeout(() => {
            if (lastMouseWhenStarted.x == lastMousePosition.x && lastMouseWhenStarted.y == lastMousePosition.y) {
                hideControls();
                timeout = null;
            } else {
                lastMouseWhenStarted = lastMousePosition;
                startTimeoutForHideControls();
            }

        }, 2000);
    }

    // keyboardShortcuts executes the relevant functions for
    // each supported shortcut key

    var keyHoldInterval = null;
    var keyDelayTimeout = null;
    function keyboardShortcutCancel(event) {
        clearInterval(keyHoldInterval);
        clearTimeout(keyDelayTimeout);

        switch (event.key) {
            case "ArrowRight":
            case "ArrowLeft":
                break;
            default:
        }
    }

    function invokeKeyboardShortcuts(event) {
        clearInterval(keyHoldInterval);
        clearTimeout(keyDelayTimeout);
        keyDelayTimeout = setTimeout(() => {
            keyHoldInterval = setInterval(() => {
                keyboardShortcuts(event);
            }, 200);
        }, 500);
        keyboardShortcuts(event);
    }

    function onExternalPlayback() {
        togglePlay();
        animatePlayback();
        if (video.paused) {
            showControls();
        } else {
            startTimeoutForHideControls();
        }
    }

    function keyboardShortcuts(event) {
        switch (event.key) {
            case ' ':
                onExternalPlayback();
                break;
            case 'm':
                toggleMute();
                break;
            case 'ArrowUp':
                var val = parseFloat(volume.value);
                if (val < 1) {
                    volume.value = val + 0.05 > 1 ? 1 : val + 0.05;
                    updateVolume();
                }
                break;
            case 'ArrowDown':
                var val = parseFloat(volume.value);
                if (val > 0) {
                    volume.value = Math.max(0, val - 0.05);
                    updateVolume();
                }
                break;
            case 'ArrowRight':
                var val = video.currentTime + 10;
                if (val < video.duration) {
                    moveTo(val);
                }
                break;
            case 'ArrowLeft':
                var val = video.currentTime - 10;
                if (val > 0) {
                    moveTo(val);
                }
                break;
            case 'f':
                toggleFullScreen();
                break;
            case 'p':
                togglePip();
                break;
        }
    }

    // Add eventlisteners here
    $(playButton).on('click', togglePlay);
    $(video).on('play', updatePlayButton);
    $(video).on('pause', updatePlayButton);
    $(video).on('loadedmetadata', initializeVideo);
    $(video).on('timeupdate', updateTimeElapsed);
    $(video).on('timeupdate', updateProgress);
    $(video).on('volumechange', updateVolumeIcon);
    $(video).on('click', function () {
        togglePlay();
        videoContainer.focus();
    });
    $(video).on('click', animatePlayback);
    $(video).on('mouseenter', showControls);
    $(video).on('mousemove', onMouseMovement);
    $(video).on('mouseleave', startTimeoutForHideControls);
    $(videoControls).on('mouseenter', showControls);
    $(videoControls).on('mouseleave', startTimeoutForHideControls);
    $(seek).on('mousemove', updateSeekTooltip);
    $(seek).on('input', skipAhead);
    $(volume).on('input', updateVolume);
    $(volumeButton).on('click', toggleMute);
    $(fullscreenButton).on('click', toggleFullScreen);
    $(videoContainer).on('fullscreenchange', updateFullscreenButton);
    $(videoContainer).dblclick(toggleFullScreen);
    $(pipButton).on('click', togglePip);

    $(document).on('DOMContentLoaded', () => {
        if (!('pictureInPictureEnabled' in document)) {
            pipButton.classList.add('hidden');
        }
    });
    $(document).on('keydown', invokeKeyboardShortcuts);
    $(document).on('keyup', keyboardShortcutCancel);
    $(videoContainer).on("destroyed",
        () => {
            $(document).off('keydown', invokeKeyboardShortcuts);
            $(document).off('keyup', keyboardShortcutCancel);
        });

    if ('mediaSession' in navigator) {
        navigator.mediaSession.setActionHandler('play', function () {
            onExternalPlayback();
        });
        navigator.mediaSession.setActionHandler('pause', function () {
            onExternalPlayback();
        });
        navigator.mediaSession.setActionHandler('stop', function () { /* Code excerpted. */ });
    }
    videoContainer.focus();

    window.vEx = video;
}

window["VideoEx"] = VideoEx;



//window['Blazored'].setProperty = function (el, name, value) {
//    if (!el) {
//        return;
//    }
//    try {
//        el[name] = value;
//    } catch (e) {
//        console.error(e);
//    }
//};

//window['Blazored'].getProperty = function (el, name) {
//    if (!el) {
//        return null;
//    }
//    try {
//        return el[name];
//    } catch (e) {
//        console.error(e);
//        return null;
//    }
//};

//window['Blazored'].invoke = function (el, name, ...arguments) {
//    if (!el) {
//        return null;
//    }
//    try {
//        return el[name](name, ...arguments);
//    } catch (e) {
//        console.error(e);
//        return null;
//    }
//};

//window['Blazored']['registerCustomEventHandler'] = function (el, eventName, payload) {
//    if (!(el && eventName)) {
//        return false
//    }
//    if (!el.hasOwnProperty('customEvent')) {
//        el['customEvent'] = function (eventName, payload) {

//            this['value'] = getJSON(this, eventName, payload)

//            var event
//            if (typeof (Event) === 'function') {
//                event = new Event('change')
//            } else {
//                event = document.createEvent('Event')
//                event.initEvent('change', true, true)
//            }

//            this.dispatchEvent(event)
//        }
//    }

//    if ($) {
//        $(el).on(eventName, function () { el.customEvent(eventName, payload) });
//    } else {
//        el.addEventListener(eventName, function () { el.customEvent(eventName, payload) });
//    }


//    // Craft a bespoke json string to serve as a payload for the event
//    function getJSON(el, eventName, payload) {
//        if (payload && payload.length > 0) {
//            // this syntax copies just the properties we request from the source element
//            // IE 11 compatible
//            let data = {};
//            for (var obj in payload) {
//                var item = payload[obj];
//                if (el[item]) {
//                    data[item] = el[item]
//                }
//            }

//            // this stringify overload eliminates undefined/null/empty values
//            return JSON.stringify(
//                { name: eventName, state: data }
//                , function (k, v) { return (v === undefined || v == null || v.length === 0) ? undefined : v }
//            )
//        } else {
//            return JSON.stringify(
//                { name: eventName }
//            )
//        }
//    }
//}