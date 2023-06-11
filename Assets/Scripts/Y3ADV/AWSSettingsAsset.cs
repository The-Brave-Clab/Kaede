using System;
using Amazon.S3;
using UnityEngine;

namespace Y3ADV
{
    [CreateAssetMenu(fileName = "AWSSettings", menuName = "Y3ADV/Create AWS Settings Asset")]
    public class AWSSettingsAsset : ScriptableObject
    {
        [Serializable]
        public struct Bucket
        {
            public string name;
            public bool useAccelerateEndpoint;
            public bool useDualStackEndpoint;

#if !WEBGL_BUILD
            [NonSerialized]
            public IAmazonS3 s3Client;
#endif
        }

        public double timeoutDuration = 1;
        public Bucket extractedBucket;
        public Bucket patchBucket;
        public Bucket translationBucket;
        public Bucket assetBundleBucket;
    }
}