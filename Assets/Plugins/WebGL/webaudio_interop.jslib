var AudioPlugin = {
    $AudioPluginAnalyzers: {},
    $AudioPluginFunctions: {

        FindAudioChannel: function (duration) {
            var acceptableDistance = 0.01;

            var channel = null;

            if (typeof WEBAudio === 'undefined') return false;
            var keys = Object.keys(WEBAudio.audioInstances);
            if (keys.length > 1) {
                for (var i = keys.length - 1; i >= 0; i--) {
                    var key = keys[i];
                    if (WEBAudio.audioInstances[key] != null) {
                        var pSource = WEBAudio.audioInstances[key].source;
                        if (pSource != null && pSource.buffer != null && Math.abs(pSource.buffer.duration - duration) < acceptableDistance) {
                            channel = WEBAudio.audioInstances[key];
                            break;
                        }
                    }
                }
            }

            if (channel == null) {
                console.warn("Did not find an audio channel that matches!");
            }

            return channel;
        },

        FixChannelResumeInfinite: function (channel) {
            if (channel == null) {
                return;
            }

            if (channel.source.isPausedMockNode) {
                if (typeof channel.source.playbackPausedAtPosition !== "undefined") {
                    if (!isFinite(channel.source.playbackPausedAtPosition)) {
                        channel.source.playbackPausedAtPosition = 0.0;
                    }
                }
            }
        }

    },

    StartSampling: function (namePtr, duration, bufferSize) {

        var name = UTF8ToString(namePtr);
        if (AudioPluginAnalyzers[name] != null) return;

        var analyzer = null;
        var channel = null;

        try {
            channel = AudioPluginFunctions.FindAudioChannel(duration);

            if (channel == null) {
                return false;
            }

            AudioPluginFunctions.FixChannelResumeInfinite(channel);

            if (channel.source.isPausedMockNode) {
                channel.resume();
                if (channel.source.isPausedMockNode) {
                    console.log("Audio source " + name + " is a mock node");
                    return false;
                }
            }

            analyzer = WEBAudio.audioContext.createAnalyser();
            analyzer.fftSize = bufferSize * 2;
            analyzer.smoothingTimeConstant = 0;
            channel.source.connect(analyzer);

            AudioPluginAnalyzers[name] = {
                analyzer: analyzer,
                channel: channel
            };

            return true;
        } catch (e) {
            console.error("Failed to connect analyser to source " + e);

            if (analyzer != null && channel.source != null) {
                if (typeof channel.source.isPausedMockNode === "undefined" || !channel.source.isPausedMockNode) {
                    source.disconnect(analyzer);
                }
            }
        }

        return false;
    },

    CloseSampling: function (namePtr) {
        var name = UTF8ToString(namePtr);
        var analyzerObj = AudioPluginAnalyzers[name];

        if (analyzerObj != null) {
            var success = false;
            try {
                if (typeof analyzerObj.channel.source !== "undefined" &&
                    (typeof analyzerObj.channel.source.isPausedMockNode === "undefined" || !analyzerObj.channel.source.isPausedMockNode))
                    analyzerObj.channel.source.disconnect(analyzerObj.analyzer);
                success = true;
            } catch (e) {
                console.warn("Failed to disconnect analyser " + name + " from source " + e);
            } finally {
                delete AudioPluginAnalyzers[name];
                return true;
            }
        }

        return false;
    },

    GetSamples: function (namePtr, bufferPtr, bufferSize) {
        var name = UTF8ToString(namePtr);
        if (AudioPluginAnalyzers[name] == null) {
            console.warn("analyzer with name " + name + " not found!");
            return false;
        }
        try {
            var buffer = new Float32Array(bufferSize);

            var analyzerObj = AudioPluginAnalyzers[name];

            if (analyzerObj == null) {
                console.warn("Could not find analyzer " + name + " to get lipsync data for");
                return false;
            }

            analyzerObj.analyzer.getFloatFrequencyData(buffer);
            for (var i = 0; i < buffer.length; i++) {
                HEAPF32[(bufferPtr >> 2) + i] = Math.pow(10, buffer[i] / 20) * 4;
            }
            return true;
        } catch (e) {
            console.error("Failed to get lipsync sample data " + e);
        }

        return false;
    },

    SetChannelLoop: function (duration, loop, loopStart, loopEnd) {

        var channel = null;
        try {
            channel = AudioPluginFunctions.FindAudioChannel(duration);

            if (channel == null) {
                return false;
            }

            if (channel.source == null) {
                return false;
            }

            channel.source.loop = loop;
            channel.source.loopStart = loopStart;
            channel.source.loopEnd = loopEnd;

        } catch (e) {
            console.log("Failed to set loop " + e);
        }

        return false;
    },

    FixChannelInfinite: function (duration) {
        
        channel = AudioPluginFunctions.FindAudioChannel(duration);

        if (channel == null) {
            return false;
        }

        AudioPluginFunctions.FixChannelResumeInfinite(channel);
    }
};

autoAddDeps(AudioPlugin, '$AudioPluginAnalyzers');
autoAddDeps(AudioPlugin, '$AudioPluginFunctions');
mergeInto(LibraryManager.library, AudioPlugin);