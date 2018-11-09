using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Excel;
using Google.Protobuf;
using Pk2Proto;

public class ModelBonesEditor
{
    public const string playerPath = "Assets/ArtResources/Models/Player/player.prefab";

    public const string sourcePath = "Assets/ArtResources/Models/Player";
    
    public const string sourceTexturesPath = "Assets/ArtResources/Models/Player/Textures";
    public const string targetModelsPath = "Assets/Resources/CharacterCustomization/Player/Models";
    public const string targetTexturePath = "Assets/Resources/CharacterCustomization/Player/Textures";
    public const string targetPath = "Assets/Resources/CharacterCustomization/Player";

    public const string newMaterialsPath = targetPath + "/Materials/";
    public const string facadeExcelPath = "/Excels/Server/Appearance.xlsx";
    public const string equipExcelPath = "/Excels/Server/Equipments_Parts.xlsx";
    const string outPath = "Assets/StreamingAssets/BonesConfiger.txt";

    private static Dictionary<string, string> sourceTexPathDic = new Dictionary<string, string>();
    static Dictionary<string, string> targetTexPathDic = new Dictionary<string, string>();
    static Dictionary<string, string> targetMatPathDic = new Dictionary<string, string>();
    static Dictionary<string, string> sourceMatPathDic = new Dictionary<string, string>();
    private static Dictionary<string, string> modelsDic = new Dictionary<string, string>();
    private static Dictionary<string, string> texturesDic = new Dictionary<string, string>();
    //private static Dictionary<string, string> materialsDic = new Dictionary<string, string>();
    private const int equipModelNum = 3;
    private const int equipTextureNum = 4;

    private const string ModelStr = "Models";

    private const string TextureStr = "Textures";
    // private const int 

    private const int facadeModelNum = 3;
    private const int facadeTextureNum = 4;


    
    public static bool DeleteFiles(string path)
    {
        if (Directory.Exists(path) == false)
        {
            return false;
        }
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] files = dir.GetFiles();
        try
        {
            foreach (var item in files)
            {
                File.Delete(item.FullName);
            }
            if (dir.GetDirectories().Length != 0)
            {
                foreach (var item in dir.GetDirectories())
                {
                    DeleteFiles(dir.ToString() + "\\" + item.ToString());
                }
            }
            Directory.Delete(path);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static ExcelDataSet ReadExcel(string excelPath)
    {
        ExcelDataSet set = new ExcelDataSet();
        if (!File.Exists(excelPath))
        {
            throw new Exception("path not exists");
        }
        try
        {
            FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            do
            {
                ExcelDataTable table = new ExcelDataTable();
                // sheet name   
                while (excelReader.Read())
                {
                    ExcelDataRow row = new ExcelDataRow();
                    for (int i = 0; i < excelReader.FieldCount; i++)
                    {
                        string value = excelReader.IsDBNull(i) ? "" : excelReader.GetString(i);
                        row.Cells.Add(value);
                    }
                    table.Rows.Add(row);
                }
                if (table.Rows.Count > 0)
                    set.Tables.Add(table);

            } while (excelReader.NextResult());
            excelReader.Dispose();
            stream.Dispose();
        }
        catch (Exception e)
        {
            DebugLogWrapper.LogError(e);
        }

        return set;
    }



    [MenuItem("Tools/Generate Player Data")]
    public static void GeneratePlayerData()
    {

        var excelPath = Application.dataPath + facadeExcelPath;
        var facadeSet = ReadExcel(excelPath);
        var facadeTables = facadeSet.Tables;
        foreach (var table in facadeTables)
        {

            var rows = table.Rows;
            if (rows == null) continue;
            if (rows.Count < 3) continue;
            if (rows[0].Cells.Count < 6) continue;

            if (!rows[0].Cells[facadeModelNum].Equals(ModelStr) || !rows[0].Cells[facadeTextureNum].Equals(TextureStr))
                continue;
            for (int i = 0; i < rows.Count; ++i)
            {
                if (i == 0) continue;

                var row = rows[i];
                var kv = row.Cells[facadeTextureNum].Trim();
                if (kv == string.Empty) continue;
                if (!texturesDic.ContainsKey(kv))
                    texturesDic.Add(kv, kv);

                continue;
                var modelsStr = row.Cells[facadeModelNum].Trim();
                if (modelsStr == string.Empty) continue;

                var __modelsStr = modelsStr.Split(';');
                List<string> modelStrList = new List<string>();
                foreach (var _modelsStr in __modelsStr)
                {
                    var es = _modelsStr.Split('|');
                    for (int j = 0; j < es.Length; j++)
                    {
                        if (es[j].Trim() != string.Empty)
                            modelStrList.Add(es[j].Trim());
                    }
                }
                foreach (var k in modelStrList)
                {
                    if (!modelsDic.ContainsKey(k))
                        modelsDic.Add(k, k);
                }
            }
        }

        excelPath = Application.dataPath + equipExcelPath;
        var equipSet = ReadExcel(excelPath);
        var equipTables = equipSet.Tables;
        foreach (var table in equipTables)
        {

            var rows = table.Rows;
            if (rows == null) continue;
            if (rows.Count < 3) continue;
            if (rows[0].Cells.Count < 6) continue;

            if (!rows[0].Cells[equipModelNum].Equals(ModelStr) || !rows[0].Cells[equipTextureNum].Equals(TextureStr))
                continue;
            for (int i = 0; i < rows.Count; ++i)
            {
                if (i == 0) continue;

                var row = rows[i];
                var kv = row.Cells[equipTextureNum].Trim();
                if (kv == string.Empty) continue;
                if (!texturesDic.ContainsKey(kv))
                    texturesDic.Add(kv, kv);

                continue;
                var modelsStr = row.Cells[equipModelNum].Trim();
                if (modelsStr == string.Empty) continue;

                var __modelsStr = modelsStr.Split(new []{';', '|'});
  
                foreach (var k in __modelsStr)
                {
                    if (!modelsDic.ContainsKey(k))
                        modelsDic.Add(k, k);
                }

            }
        }

        sourceTexPathDic.Clear();
        GetAllAssetPath(sourceTexPathDic, sourcePath, "t:Texture");
        targetTexPathDic.Clear();
        GetAllAssetPath(targetTexPathDic, targetPath, "t:Texture");
        
        foreach (var tex in texturesDic)
        {
            if (!targetTexPathDic.ContainsKey(tex.Key))
            {
                if (!sourceTexPathDic.ContainsKey(tex.Key))
                {
                    DebugLogWrapper.LogError("asset miss>>>" + tex.Key);
                    continue;
                }
                var sp = sourceTexPathDic[tex.Key];
                var tp = targetTexturePath + GetTextureSubPath(sp);
                AssetDatabase.CopyAsset(sp, tp);
                targetTexPathDic.Add(tex.Key, tp);
            }
        }

        var player = AssetDatabase.LoadAssetAtPath<GameObject>(playerPath);
        var dummyCount = player.transform.childCount;
        List<GameObject> gos = new List<GameObject>();
        for (int i = 0; i < dummyCount; ++i)
        {
            var child = player.transform.GetChild(i);
            if(child.name == "Bip01")continue;
            if(child.name == "Dummy_idle_ball")continue;
            if (child.name.Contains("Dummy_"))
            {
                var modelsCount = child.childCount;
                for (int j = 0; j < modelsCount; ++j)
                {
                    gos.Add(child.GetChild(j).gameObject);
                }
            }
        }


        // 导出models 并且生成bones info
        ExportBonesInfo(gos, outPath);

        
        
        targetMatPathDic.Clear();
        sourceMatPathDic.Clear();

        // 搜集所有assert
        
        
        GetAllAssetPath(targetMatPathDic, targetPath, "t:Material");
        GetAllAssetPath(sourceMatPathDic, sourcePath, "t:Material");

        Dictionary<string, string> goPathsDic = new Dictionary<string, string>();

        GetAllAssetPath(goPathsDic, targetModelsPath, "t:Prefab");

        foreach (var goPath in goPathsDic)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(goPath.Value);
            var render = go.GetComponent<Renderer>();
            var mat = render.sharedMaterial;
            if (mat == null)
            {
                DebugLogWrapper.LogError("材质丢失，请手动指定" + go.name);
                continue;
            }
            string matPath = string.Empty;
            if (targetMatPathDic.TryGetValue(mat.name, out matPath))
            {
                var newMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                var sourceMat = AssetDatabase.LoadAssetAtPath<Material>(sourceMatPathDic[mat.name]);

                // 重新指定引用的材质
                render.sharedMaterial = newMat;
                ReplaceMatTexs(sourceMat, newMat);
            }
            else
            {
                var sourceMat = AssetDatabase.LoadAssetAtPath<Material>(sourceMatPathDic[mat.name]);
                var newMat = new Material(sourceMat);
                newMat.name = sourceMat.name;
                AssetDatabase.CreateAsset(newMat, newMaterialsPath + newMat.name);
                render.sharedMaterial = newMat;
                ReplaceMatTexs(sourceMat, newMat);
            }

        }
        AssetDatabase.SaveAssets();
    }
    public static void ExportBonesInfo(List<GameObject> gos, string outPath)
    {
        if (gos == null || gos.Count <= 0) return;

        DeleteFiles(targetModelsPath);
        //AssetDatabase.DeleteAsset(targetModelsPath);
        AssetDatabase.CreateFolder(targetPath, "Models");
        StringBuilder str = new StringBuilder();
        str.Append("function GetGameConfigurations()\r\n\tlocal ret = {}\r\n");
        str.Append("\tbonesConfiger = {}\r\n");
        str.Append("\tret['bonesConfiger'] = bonesConfiger\r\n");
        for (int i = 0; i < gos.Count; i++)
        {
            var item = gos[i];
            if (!(item is GameObject)) continue;
            if (item.name.Contains("@")) continue;
            //if(!AssetDatabase.GetAssetPath(item))
            var smr = item.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr == null) continue;
            string parent = item.transform.parent.name;
            str.Append("\tbonesConfiger[\"").Append(smr.name).Append("\"] = {");
            //string rootbone
            for (int j = 0; j < smr.bones.Length; j++)
            {
                if (smr.bones[j] == null) continue;
                str.Append("'").Append(smr.bones[j].name);
                str.Append("', ");
            }
            str.Append("'").Append(smr.rootBone.name).Append("', '").Append(parent).Append("'}\r\n");

            var path = targetModelsPath + "/" + item.name + ".prefab";
            GameObject asset = null;
            while(true)
            {
                
                PrefabUtility.CreatePrefab(path, item);
                asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                asset.layer = LayerMask.NameToLayer("CastShadow");
                var len = asset.GetComponentsInChildren<SkinnedMeshRenderer>().Length;
                Debug.LogError(len + "  " + asset.name);
                if (len == 1)
                {
                    break;
                }
                else
                    AssetDatabase.DeleteAsset(path);
            }
        }

        str.Append("\treturn ret\r\nend");
        byte[] bytes = Encoding.UTF8.GetBytes(str.ToString());

        if (string.IsNullOrEmpty(outPath))
            outPath = Application.streamingAssetsPath + "/bonesConfig.txt";
        File.WriteAllBytes(outPath, bytes);
    }


    public static string GetTextureSubPath(string path)
    {
        if (!path.Contains("Textures")) return string.Empty;
        var pos = path.IndexOf("Textures");
        return path.Substring(pos + 8);
    }
    public static void GetAllAssetPath(Dictionary<string, string> retDir, string path, string filter)
    {
        string[] ids = AssetDatabase.FindAssets(filter, new string[] { path });
        for (int i = 0; i < ids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var strs = assetPath.Split('/');
            var str = strs[strs.Length - 1].Trim().Split('.')[0];
            if (retDir.ContainsKey(str))
                DebugLogWrapper.LogError("检测到重复资源>>>>" + str + ": " + assetPath + "   retDir:" + retDir[str]);
            else
                retDir.Add(str, assetPath);
        }
    }

    public static void ReplaceMatTexs(Material sourceMaterial, Material targetMaterial)
    {
        ReplaceMatTexture(sourceMaterial, targetMaterial, "_MainTex");
        ReplaceMatTexture(sourceMaterial, targetMaterial, "_SpecularMap");
        ReplaceMatTexture(sourceMaterial, targetMaterial, "_NormalMap");
        ReplaceMatTexture(sourceMaterial, targetMaterial, "_EyesTex");
        ReplaceMatTexture(sourceMaterial, targetMaterial, "_MouthTex");
        ReplaceMatTexture(sourceMaterial, targetMaterial, "_TattooTex");
        ReplaceMatTexture(sourceMaterial, targetMaterial, "_ReflectionTex");
    }
    private static void ReplaceMatTexture(Material sourceMaterial, Material targetMaterial, string attr)
    {
        if (sourceTexPathDic == null || targetTexPathDic == null || sourceMaterial == null || targetMaterial == null) return;
        if (sourceMaterial.HasProperty(attr))
        {
            
            var tex = sourceMaterial.GetTexture(attr);
            if (tex == null) return;
            var texName = tex.name;
            string targetTexPath = string.Empty;
            if (targetTexPathDic.TryGetValue(texName, out targetTexPath))
            {
                var newtex = AssetDatabase.LoadAssetAtPath<Texture>(targetTexPath);
                if (newtex != null)
                {
                    targetMaterial.SetTexture(attr, newtex);
                }
                else
                {
                    throw new Exception("target texture path error: "+ targetTexPath);
                }
            }
            else
            {
                string sourcePath = String.Empty;

                if (sourceTexPathDic.TryGetValue(texName, out sourcePath))
                {
                    var end = sourcePath.Split('.')[1];

                    var tp = targetTexturePath + GetTextureSubPath(sourcePath);
                    AssetDatabase.CopyAsset(sourcePath, tp);
                    targetTexPathDic.Add(texName, tp);

                    var newtex = AssetDatabase.LoadAssetAtPath<Texture>(tp);
                    targetMaterial.SetTexture(attr, newtex);
                }
                else
                {
                    DebugLogWrapper.LogError("材质引用的纹理丢失:" + targetMaterial.name + "texture" + tex.name);
                    return;
                }
            }
        }
    }
}
