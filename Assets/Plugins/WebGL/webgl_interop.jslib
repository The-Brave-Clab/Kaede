var WebGLInterop = {

    $InteropGameObjects: {
        WebGLInteropGameObject: "",
        SoundManagerGameObject: "",
        UIManagerGameObject: "",
        FullscreenManagerGameObject: "",
    },

    $InteropCallbacks: {
        OnScriptLoaded: function(script) { },
        OnMessageCommand: function(speaker, voiceId, message) { },
        OnScenarioStarted: function() { },
        OnScenarioFinished: function() { },
        OnExitFullscreen: function() { },
        OnToggleAutoMode: function(on) { },
        OnToggleDramaMode: function(on) { }
    },

    $InteropFunctions: {
        ResetPlayer: function (scriptName, languageCode) {
            let unifiedName = [scriptName, languageCode].join(":");
            SendMessage(InteropGameObjects.WebGLInteropGameObject, "ResetPlayer", unifiedName);
        },

        SetVolume: function (master, bgm, voice, sfx) {
            SendMessage(InteropGameObjects.SoundManagerGameObject, "SetMasterVolume", master);
            SendMessage(InteropGameObjects.SoundManagerGameObject, "SetBGMVolume", bgm);
            SendMessage(InteropGameObjects.SoundManagerGameObject, "SetVoiceVolume", voice);
            SendMessage(InteropGameObjects.SoundManagerGameObject, "SetSEVolume", sfx);
        },

        EnterFullscreen: function() {
            SendMessage(InteropGameObjects.FullscreenManagerGameObject, "ChangeFullscreen", 1);
        },

        ExitFullscreen: function() {
            SendMessage(InteropGameObjects.FullscreenManagerGameObject, "ChangeFullscreen", 0);
            if (InteropGameObjects.UIManagerGameObject !== "")
              SendMessage(InteropGameObjects.UIManagerGameObject, "HideMenu");
        },

        ToggleAutoMode: function(on) {
            if (InteropGameObjects.UIManagerGameObject === "")
                return;
            if (on)
                SendMessage(InteropGameObjects.UIManagerGameObject, "ToggleAutoMode", 1);
            else
                SendMessage(InteropGameObjects.UIManagerGameObject, "ToggleAutoMode", 0);
        },

        ToggleDramaMode: function(on) {
            if (InteropGameObjects.UIManagerGameObject === "")
                return;
            if (on)
                SendMessage(InteropGameObjects.UIManagerGameObject, "ToggleDramaMode", 1);
            else
                SendMessage(InteropGameObjects.UIManagerGameObject, "ToggleDramaMode", 0);
        },

        ToggleInputInterception: function(on) {
            if (InteropGameObjects.WebGLInteropGameObject === "")
                return;
            if (on)
                SendMessage(InteropGameObjects.WebGLInteropGameObject, 'EnableInput');
            else
                SendMessage(InteropGameObjects.WebGLInteropGameObject, 'DisableInput');
        },
    },

    GetWebGLScriptName: function () {
        var scriptName;
        if (typeof _y3advScriptName === 'undefined') {
            console.error("Please set the value of _y3advScriptName before initializing the Unity Player!");
            scriptName = "";
        }
        
        scriptName = _y3advScriptName;

        var bufferSize = lengthBytesUTF8(scriptName) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(scriptName, buffer, bufferSize);

        return buffer;
    },

    RegisterWebGLInteropGameObject: function (gameObjectName) {
        InteropGameObjects.WebGLInteropGameObject = UTF8ToString(gameObjectName);
    },

    RegisterSoundManagerGameObject: function (gameObjectName) {
        InteropGameObjects.SoundManagerGameObject = UTF8ToString(gameObjectName);
    },

    RegisterUIManagerGameObject: function (gameObjectName) {
        InteropGameObjects.UIManagerGameObject = UTF8ToString(gameObjectName);
    },

    RegisterFullscreenManagerGameObject: function (gameObjectName) {
        InteropGameObjects.FullscreenManagerGameObject = UTF8ToString(gameObjectName);
    },

    RegisterInterops: function () {
        b352a51964f6fc4813af8f08d403ec0d(
            InteropCallbacks, 
            InteropFunctions.ResetPlayer, 
            InteropFunctions.SetVolume,
            InteropFunctions.EnterFullscreen,
            InteropFunctions.ExitFullscreen,
            InteropFunctions.ToggleAutoMode,
            InteropFunctions.ToggleDramaMode,
            InteropFunctions.ToggleInputInterception);
    },

    OnScriptLoaded: function (script) {
        InteropCallbacks.OnScriptLoaded(UTF8ToString(script));
    },

    OnMessageCommand: function (speaker, voiceId, message) {
        InteropCallbacks.OnMessageCommand(UTF8ToString(speaker), UTF8ToString(voiceId), UTF8ToString(message));
    },

    OnScenarioStarted: function () {
        InteropCallbacks.OnScenarioStarted();
    },

    OnScenarioFinished: function () {
        InteropCallbacks.OnScenarioFinished();
    },

    OnExitFullscreen: function () {
        InteropCallbacks.OnExitFullscreen();
    },

    OnToggleAutoMode: function (on) {
        InteropCallbacks.OnToggleAutoMode(on > 0);
    },

    OnToggleDramaMode: function (on) {
        InteropCallbacks.OnToggleDramaMode(on > 0);
    }

};

autoAddDeps(WebGLInterop, '$InteropGameObjects');
autoAddDeps(WebGLInterop, '$InteropCallbacks');
autoAddDeps(WebGLInterop, '$InteropFunctions');
mergeInto(LibraryManager.library, WebGLInterop);