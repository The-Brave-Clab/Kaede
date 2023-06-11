using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime.CredentialManagement;
using Unity.Build;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Y3ADV.Editor.Build
{
    public static partial class ProjectBuild
    {
        private static readonly RegionEndpoint BucketRegion = RegionEndpoint.APNortheast1;
        private static readonly string BucketName = "y3adv-publish";

        [MenuItem("Y3ADV/Upload/WebGL")]
        public static void UploadWebGLArtifacts()
        {
            var buildConfig = GetBuildConfigurationFromAssetPath("WebGL/WebGLRelease.buildconfiguration");
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadBuildArtifacts(buildConfig));
        }

        [MenuItem("Y3ADV/Upload/WebGL (Development)")]
        public static void UploadWebGLDevArtifacts()
        {
            var buildConfig = GetBuildConfigurationFromAssetPath("WebGL/WebGLDevelop.buildconfiguration");
            EditorCoroutineUtility.StartCoroutineOwnerless(UploadBuildArtifacts(buildConfig));
        }

        [MenuItem("Y3ADV/Upload/WebGL", true)]
        [MenuItem("Y3ADV/Upload/WebGL (Development)", true)]
        public static bool CanUploadArtifacts()
        {
            var chain = new CredentialProfileStoreChain();
            return chain.TryGetAWSCredentials("github_actions", out _);
        }

        private static IEnumerator UploadBuildArtifacts(BuildConfiguration buildConfig)
        {
            var buildPath = buildConfig.GetBuildPipeline().GetOutputBuildDirectory(buildConfig);

            if (!buildPath.Exists)
            {
                Debug.LogError($"Build path {buildPath} does not exist.");
                yield break;
            }

            var files = buildPath.GetFiles("*", SearchOption.AllDirectories);

            var defaultProgressBarTitle = "Uploading";
            var defaultProgressBarMessage = "Uploading build artifacts...";

            try
            {
                EditorUtility.DisplayProgressBar(defaultProgressBarTitle, defaultProgressBarMessage, 0f);
                var chain = new CredentialProfileStoreChain();
                if (!chain.TryGetAWSCredentials("github_actions", out var credentials)) yield break;
                using var client = new AmazonS3Client(credentials, BucketRegion);
                var transferUtility = new TransferUtility(client);
                for (var i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    var key = file.FullName.Substring(buildPath.Parent!.FullName.Length + 1)
                        .Replace(Path.DirectorySeparatorChar, '/');

                    key = $"{buildConfig.GetPlatform().DisplayName}/{key}";

                    var contentType = GetContentType(file);
                    var contentEncoding = GetContentEncoding(file);

                    var progressBarTitle = $"Uploading ({i + 1}/{files.Length})";
                    var progressBarMessage = $"Uploading {key}...";

                    EditorUtility.DisplayProgressBar(progressBarTitle, progressBarMessage, 0f);

                    var request = new TransferUtilityUploadRequest
                    {
                        BucketName = BucketName,
                        Key = key,
                        FilePath = file.FullName,
                    };

                    if (contentType != null)
                        request.ContentType = contentType;

                    if (contentEncoding != null)
                        request.Headers.ContentEncoding = GetContentEncoding(file);

                    request.UploadProgressEvent += (sender, args) =>
                    {
                        EditorUtility.DisplayProgressBar(progressBarTitle, progressBarMessage,
                            args.PercentDone / 100f);
                    };

                    var uploadTask = transferUtility.UploadAsync(request);
                    while (!uploadTask.IsCompleted)
                        yield return null;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"Upload {buildConfig.name} completed successfully.");

            yield return null;
        }

        private static Dictionary<string, string> contentTypes = new()
        {
            { ".html", "text/html" },
            { ".css", "text/css" },
            { ".xml", "text/xml" },
            { ".js", "application/javascript" },
            { ".json", "application/json" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".ico", "image/x-icon" },
            { ".wasm", "application/wasm" },
            { ".data", "binary/octet-stream" },
            { ".bundle", "binary/octet-stream" },
        };
        
        private static Dictionary<string, string> contentEncodings = new()
        {
            { ".br", "br" }
        };

        private static string GetContentType(FileInfo file)
        {
            var extension = file.Extension.ToLower();
            if (contentEncodings.ContainsKey(extension))
                extension = Path.GetExtension(Path.GetFileNameWithoutExtension(file.FullName)).ToLower();

            return contentTypes.TryGetValue(extension, out var contentType) ? contentType : null;
        }

        private static string GetContentEncoding(FileInfo file)
        {
            var extension = file.Extension.ToLower();
            return contentEncodings.TryGetValue(extension, out var contentEncoding) ? contentEncoding : null;
        }
    }
}