using System;
using Unity.Build;
using Unity.Build.Classic;
using Unity.Build.Common;
using UnityEditor;
using UnityEngine;

namespace Y3ADV.Editor.Build
{
    public sealed class FillWebGLBuildConfigName : IBuildComponent
    { }

    public sealed class WebGLBuildConfigName : ClassicBuildPipelineCustomizer
    {
        public override Type[] UsedComponents => new[] { typeof(FillWebGLBuildConfigName) };

        public override void OnBeforeBuild()
        {
            base.OnBeforeBuild();

            if (Context.TryGetComponent<OutputBuildDirectory>(out _)) return;
            if (BuildTarget != BuildTarget.WebGL) return;
            if (!Context.TryGetComponent<FillWebGLBuildConfigName>(out _)) return;

            var buildConfigName = Context.BuildConfigurationName;
            PlayerSettings.SetTemplateCustomValue("BUILD_CONFIG_NAME", buildConfigName);
        }
    }
}