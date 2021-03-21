if (!window['BlazoredMediaData']) {
    window['BlazoredMediaData'] = {
        activeListener: new Array(),

        isSupported: function () {
            return "mediaSession" in navigator;
        },

        setMediaData: function (data) {
            if (data) {
                navigator.mediaSession.metadata = new MediaMetadata(data);
            } else {
                navigator.mediaSession.metadata = null;
            }
        },

        getMediaData: function () {
            return navigator.mediaSession.metadata;
        },

        setMediaPlaybackState(data) {
            navigator.mediaSession.playbackState = data;
        },

        getMediaPlaybackState() {
            return navigator.mediaSession.playbackState;
        },

        setMediaPositionData(data) {
            navigator.mediaSession.setPositionState(data);
        },

        mediaSessionCallback(eventData) {
            window.BlazoredMediaData.activeListener.forEach(listener => {
                listener.mediaSessionService.invokeMethod("BlazoredMediaData_eventCallback", eventData);
            });
        },

        reAttachListeners(eventTypes) {
            eventTypes.forEach(eventType => {
                navigator.mediaSession.setActionHandler(eventType, window.BlazoredMediaData.mediaSessionCallback);
            });
        },

        setActionHandler(listenerService, serviceKey, eventTypes) {
            var instance = window.BlazoredMediaData.activeListener.find(e => e.serviceKey == serviceKey);
            if (instance == null) {
                window.BlazoredMediaData.activeListener.push({
                    mediaSessionService: listenerService,
                    eventTypes,
                    serviceKey
                });
            } else {
                eventTypes.forEach(e => {
                    if (instance.eventTypes.some(f => f == e)) {
                        return;
                    }
                    instance.eventTypes.push(e);
                });
            }
            window.BlazoredMediaData.reAttachListeners(eventTypes);
        },

        removeActionHandler(serviceKey, eventTypes) {
            var instance = window.BlazoredMediaData.activeListener.find(e => e.serviceKey == serviceKey);
            if (instance != null) {
                eventTypes.forEach(e => {
                    var idxOfEventType = instance.eventTypes.indexOf(e);
                    if (idxOfEventType != -1) {
                        instance.eventTypes.splice(idxOfEventType, 1);
                    }
                });

                if (instance.eventTypes.length == 0) {
                    var indexOfInstance = window.BlazoredMediaData.activeListener.indexOf(instance);
                    window.BlazoredMediaData.activeListener.splice(indexOfInstance, 1);
                }
            }
        }
    }
}