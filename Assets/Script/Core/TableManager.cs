using SimpleJSON;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Newtonsoft.Json.Converters;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine.Tilemaps;
public class JsonNameAttribute : Attribute
{
    readonly string Name;

    public JsonNameAttribute(string name)
    {
        Name = name;
    }

    public string GetJsonName() => Name;
}

public class TableManager : BaseManager<TableManager>
{
    public int LOADING_DATA { get; private set; }

    public Dictionary<string, SkillTemplateData> SkillTable = new();
    public Dictionary<string, CharacterTemplateData> CharacterTable = new();
    public Dictionary<string, LevelTemplateData> LevelTable = new();
    public Dictionary<string, StatSheetData> StatSheetTable = new();
    public Dictionary<string, StringTableData> StringTable = new();

    protected override void _InitManager()
    {
        _LoadDataFile(SkillTable).Forget();
        _LoadDataFile(CharacterTable).Forget();
        _LoadDataFile(LevelTable).Forget();
        _LoadDataFile<StatSheetData, StatSheetInfo>(StatSheetTable).Forget();
        _LoadDataFile(StringTable).Forget();
    }

    string _GetJsonName<T>()
    {
        var dataType = typeof(T);
        object[] attrs = dataType.GetCustomAttributes(true);
        foreach (var attr in attrs)
        {
            var jsonAttr = attr as JsonNameAttribute;
            if (jsonAttr != null)
                return jsonAttr.GetJsonName();
        }

        return string.Empty;
    }

    //async UniTaskVoid LoadDataFile<TemplateData>(Dictionary<string, List<TemplateData>> tableDic)
    //    where TemplateData : BaseData
    //{
    //    var tableName = GetJsonName<List<TemplateData>>();
    //    var resultAsset = await LoadAssetAsync(tableName);
    //    if (resultAsset == null) return;

    //    var dataList = JsonConvert.DeserializeObject<DataCollection<TemplateData>>(resultAsset.text, GetJsonSettings(tableName));
    //    if (!tableDic.ContainsKey(tableName))
    //    {
    //        tableDic[tableName] = new List<TemplateData>();
    //    }

    //    tableDic[tableName].AddRange(dataList.Items);
    //}

    async UniTaskVoid _LoadDataFile<TData>(Dictionary<string, TData> tableDic)
        where TData : IKeyModel
    {
        var tableName = _GetJsonName<TData>();
        var resultAsset = await _LoadAssetAsync(tableName);
        if (resultAsset == null) return;

        var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(resultAsset.text);

        foreach (var kvp in jsonObject)
        {
            JToken jTok = kvp.Value;

            if (jTok.Type == JTokenType.Array)
            {
                var dataList = JsonConvert.DeserializeObject<List<TData>>(jTok.ToString());
                foreach (var data in dataList)
                {
                    data.KEY = kvp.Key;

                    _DataSetting(tableDic, tableName, data);
                }
            }
            else if (jTok.Type == JTokenType.Object)
            {
                var objTable = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(jTok.ToString());
                foreach (var objData in objTable)
                {
                    var data = JsonConvert.DeserializeObject<TData>(objData.Value.ToString());
                    data.KEY = objData.Key;

                    _DataSetting(tableDic, tableName, data);
                }
            }
        }
    }

    async UniTaskVoid _LoadDataFile<TData, TItem>(Dictionary<string, TData> tableDic)
        where TData : IKeyModel, IListModel<TItem>, new()
        where TItem : new()
    {
        var tableName = _GetJsonName<TData>();
        var resultAsset = await _LoadAssetAsync(tableName);
        if (resultAsset == null) return;

        var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(resultAsset.text);

        foreach (var kvp in jsonObject)
        {
            JToken jTok = kvp.Value;

            if (jTok.Type == JTokenType.Array)
            {
                var dataList = JsonConvert.DeserializeObject<List<TItem>>(jTok.ToString());
                if (!tableDic.ContainsKey(kvp.Key))
                {
                    TData data = new();
                    data.KEY = kvp.Key;
                    data._UNUSED_LIST_ = dataList;

                    _DataSetting(tableDic, tableName, data);
                }
            }
        }

        
    }

    void _DataSetting<TData>(Dictionary<string, TData> tableDic, string tableName, TData data)
        where TData : IKeyModel
    {
        Debug.Assert(!tableDic.ContainsKey(data.KEY), $"[{data.KEY}] is Already includes in [{tableName}]!");

        (data as IPostProcessAfterLoadModel)?.PostProcessAfterLoad();
        (data as IListToDicConverterModel)?.ConvertListToDictionary();

        tableDic[data.KEY] = data;
    }

    async UniTask<TextAsset> _LoadAssetAsync(string tableName)
    {
        var resultAsset = await Addressables.LoadAssetAsync<TextAsset>(tableName).Task.AsUniTask();
        if (resultAsset?.text == null)
        {
            Debug.LogError($"Failed to load data for {tableName}");
            return null;
        }
        return resultAsset;
    }
}
