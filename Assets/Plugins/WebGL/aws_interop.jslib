var AWSInterop = {

    InitializeS3Bucket: function (_region, _bucket, _useAccelerateEndpoint, _useDualStackEndpoint) {
        if (this.buckets === undefined) {
            this.buckets = {}
        }

        this.buckets[UTF8ToString(_bucket)] = new AWS.S3({
            signatureVersion: "v4",
            region: UTF8ToString(_region),
            params: {
                Bucket: UTF8ToString(_bucket),
            },
            useAccelerateEndpoint: _useAccelerateEndpoint > 0,
            useDualstackEndpoint: _useDualStackEndpoint > 0,
        });
    },

    GetSignedURL: function (_bucket, _key, _timeoutHour) {
        const bucket = this.buckets[UTF8ToString(_bucket)];
        const params = {
            Expires: _timeoutHour * 3600,
            Key: UTF8ToString(_key)
        };

        var signedUrl = bucket.getSignedUrl("getObject", params);

        var bufferSize = lengthBytesUTF8(signedUrl) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(signedUrl, buffer, bufferSize);

        return buffer;
    },

};

mergeInto(LibraryManager.library, AWSInterop);