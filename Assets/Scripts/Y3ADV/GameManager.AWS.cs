using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Amazon;

#if !WEBGL_BUILD
using Amazon.Runtime.Endpoints;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Internal;
#endif

namespace Y3ADV
{
    public partial class GameManager
    {
        private AWSSettingsAsset awsSettings;
        public static AWSSettingsAsset AWSSettings => Instance.awsSettings;

        private RegionEndpoint region = RegionEndpoint.APNortheast1;

        private void InitializeAWS()
        {
            awsSettings = Resources.Load<AWSSettingsAsset>("AWSSettings");
            InitializeS3Bucket(ref awsSettings.extractedBucket);
            InitializeS3Bucket(ref awsSettings.patchBucket);
            InitializeS3Bucket(ref awsSettings.translationBucket);
            InitializeS3Bucket(ref awsSettings.assetBundleBucket);
        }

        public static string GetObjectURL(in AWSSettingsAsset.Bucket bucket, string objectKey, bool ignoreLocalOverride = false)
        {
            if (StartupSettings.OverrideLoadPath && ignoreLocalOverride && bucket.name == "yuyuyui-datamine-extracted")
                return $"{StartupSettings.OverrideLoadPathUri}/{objectKey.Trim('/')}";

            var endpoint = bucket.useAccelerateEndpoint ?
                (bucket.useDualStackEndpoint ? "s3-accelerate.dualstack" : "s3-accelerate") :
                $"s3.{Instance.region.SystemName}";

            return $"https://{bucket.name}.{endpoint}.amazonaws.com/{objectKey.Trim('/')}";
        }
        
#if !WEBGL_BUILD
        public static async Task<GetObjectMetadataResponse> GetObjectMetadata(AWSSettingsAsset.Bucket bucket, string objectKey)
        {
            GetObjectMetadataResponse result;
            try
            {
                result = await bucket.s3Client.GetObjectMetadataAsync(bucket.name, objectKey);
            }
            catch (AmazonS3Exception e)
            {
                result = new GetObjectMetadataResponse
                {
                    HttpStatusCode = e.StatusCode
                };
            }

            return result;
        }
#endif
        
#if WEBGL_BUILD
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void InitializeS3Bucket(string region, string bucket, int useAccelerateEndpoint, int useDualStackEndpoint);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GetSignedURL(string bucket, string key, int timeoutHour);
#endif

        private static void InitializeS3Bucket(ref AWSSettingsAsset.Bucket bucket)
        {
#if WEBGL_BUILD
            InitializeS3Bucket(
                Instance.region.SystemName, bucket.name,
                bucket.useAccelerateEndpoint ? 1 : 0, bucket.useDualStackEndpoint ? 1 : 0);
#else
            AmazonS3Config config = new()
            {
                RegionEndpoint = Instance.region,
                UseAccelerateEndpoint = bucket.useAccelerateEndpoint,
                UseDualstackEndpoint = bucket.useDualStackEndpoint
            };

            bucket.s3Client = new AmazonS3Client(config);
#endif
        }
    }
}