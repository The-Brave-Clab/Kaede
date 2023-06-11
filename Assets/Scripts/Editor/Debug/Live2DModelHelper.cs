using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using live2d;
using live2d.framework;

namespace Y3ADV.Editor
{
    public static class Live2DModelHelper
    {
        public static ModelContext ModelContext(this Live2DModelUnity model)
        {
            var modelContextField = typeof(Live2DModelUnity)
                .GetField("modelContext", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return modelContextField.GetValue(model) as ModelContext;
        }

        public static List<string> GetAllParameters(this Live2DModelUnity model)
        {
            var modelContext = model.ModelContext();

            var floatParamIDList = typeof(ModelContext)
                .GetField("floatParamIDList", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(modelContext) as ParamID[];

            var paramIDHashMap = typeof(ParamID)
                .GetField("id_hashmap", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null) as Dictionary<string, ParamID>;

            return floatParamIDList!
                .Where(p => paramIDHashMap!.ContainsValue(p))
                .Select(p => paramIDHashMap!
                    .Where(x => x.Value == p)
                    .Select(x => x.Key)
                    .First())
                .ToList();
        }

        public static int GetParameterIndex(this Live2DModelUnity model, string paramId)
        {
            var floatParamIDList = typeof(ModelContext)
                .GetField("floatParamIDList", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(model.ModelContext()) as ParamID[];
            
            var paramIDHashMap = typeof(ParamID)
                .GetField("id_hashmap", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null) as Dictionary<string, ParamID>;

            var param = paramIDHashMap![paramId];
            return Array.IndexOf(floatParamIDList!, param);
        }

        public static float GetParameterMin(this Live2DModelUnity model, string paramId)
        {
            var floatParamMinList = typeof(ModelContext)
                .GetField("floatParamMinList", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(model.ModelContext()) as float[];

            return floatParamMinList![model.GetParameterIndex(paramId)];
        }

        public static float GetParameterMax(this Live2DModelUnity model, string paramId)
        {
            var floatParamMaxList = typeof(ModelContext)
                .GetField("floatParamMaxList", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(model.ModelContext()) as float[];

            return floatParamMaxList![model.GetParameterIndex(paramId)];
        }

        public static List<string> GetAllPartsData(this Live2DModelUnity model)
        {
            var modelContext = model.ModelContext();

            var partsDataList = typeof(ModelContext)
                .GetField("partsDataList", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(modelContext) as List<PartsData>;

            var partsDataIDHashMap = typeof(PartsDataID)
                .GetField("id_hashmap", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null) as Dictionary<string, PartsDataID>;

            return partsDataList!
                .Where(p => partsDataIDHashMap!.ContainsValue(p.getPartsDataID()))
                .Select(p => partsDataIDHashMap!
                    .Where(x => x.Value == p.getPartsDataID())
                    .Select(x => x.Key)
                    .First())
                .ToList();
        }

        public static int GetPartsDataIndex(this Live2DModelUnity model, string partsId)
        {
            var partsDataList = typeof(ModelContext)
                .GetField("partsDataList", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(model.ModelContext()) as List<PartsData>;

            var partsDataIDHashMap = typeof(PartsDataID)
                .GetField("id_hashmap", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null) as Dictionary<string, PartsDataID>;

            var partsData = partsDataIDHashMap![partsId];
            return partsDataList!.FindIndex(p => p.getPartsDataID() == partsData);
        }
    }
}