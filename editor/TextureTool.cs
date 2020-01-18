using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class TextureToolWindow : EditorWindow
{
    private UnityEngine.Object fbx;

    Texture2D texture;
    Texture2D _postProcessTexture;
    bool showInverted = false;

    [MenuItem("Tools/TextureTool")]
    static void OpenWindow()
    {
        var window = GetWindow<TextureToolWindow>("Texture Previewer");
        window.position = new Rect(400, 400, 680, 600);
        window.Show();
    }

    private string[] options = new[] { "PolygonClip", "Clip", "Resize"};
    private int index;
    int _left = 0;
    int _right = 0;
    int _top = 0;
    int _bottom = 0;
    
    // resize
    int _width = 0;
    int _height = 0;
    
    // clip
    private Vector2 _scale = Vector2.one;
    private Vector2 _offset = Vector2.zero;

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        var oldTex = texture;
        texture = (Texture2D)EditorGUILayout.ObjectField("Add a Texture:",
            texture,
            typeof(Texture2D));
        var assetPath = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = TextureImporter.GetAtPath(assetPath) as TextureImporter;
        index = EditorGUILayout.Popup(index, options);
        
        PolygonClipLayout(assetPath);

        if (GUILayout.Button("Save"))
        {
            string dir = Path.GetDirectoryName(assetPath);
            string path = EditorUtility.OpenFilePanel("选择文件", dir, "png");
            if (!string.IsNullOrEmpty(path))
                SavePng(_postProcessTexture, path);
        }

        if (GUILayout.Button("Replace"))
        {
            if (!string.IsNullOrEmpty(assetPath))
                SavePng(_postProcessTexture, assetPath);
        }

        EditorGUILayout.PrefixLabel(new GUIContent("Path:"));
        EditorGUILayout.BeginVertical();

        if (texture && index == 1)
        {
            EditorGUILayout.PrefixLabel(new GUIContent("Clip:"));
            EditorGUILayout.BeginHorizontal();

            _scale.x = EditorGUILayout.FloatField("ScaleX", _scale.x);
            _scale.y = EditorGUILayout.FloatField("ScaleY", _scale.y);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _offset.x = EditorGUILayout.FloatField("OffsetX", _offset.x);
            _offset.y = EditorGUILayout.FloatField("OffsetY", _offset.y);
            EditorGUILayout.EndHorizontal();
            

            ClipTexture(texture, ref _postProcessTexture, _scale, _offset);
            
        }

        if (texture && index == 2)
        {
            Resize(assetPath);
        }

        if (texture)
        {
            EditorGUILayout.PrefixLabel(new GUIContent("Preview"));
            if (_postProcessTexture != null)
            {
                EditorGUI.DrawPreviewTexture(
                    new Rect(25, 230, texture.width, texture.height),
                    _postProcessTexture, null, ScaleMode.ScaleToFit);
                
                EditorGUI.HelpBox(new Rect(25, 280 + texture.height, 280, 25), 
                    string.Format("Size: {0} x {1}   Source Size: {2} x {3}",
                        _postProcessTexture.width, _postProcessTexture.height, texture.width, texture.height), MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.PrefixLabel(new GUIContent("No texture found"));
        }

        EditorGUILayout.EndHorizontal();
    }

    private void Resize(string assetPath)
    {
        var bytes = File.ReadAllBytes(assetPath);
        Texture2D temp = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount != 0);
        temp.LoadImage(bytes);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PrefixLabel(new GUIContent("Resize:"));

        _width = EditorGUILayout.IntField("Width", _width);
        _height = EditorGUILayout.IntField("Height", _height);
        _width = Mathf.Max(8, _width);
        _height = Mathf.Max(8, _height);

        if (_postProcessTexture != null)
            DestroyImmediate(_postProcessTexture);
        _postProcessTexture = new Texture2D(_width, _height, temp.format, temp.mipmapCount != 0);

        
        EditorGUILayout.EndVertical();
        var postProcessTexture = _postProcessTexture;
        RenderTexture rt = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGB32);
        try
        {
            Graphics.Blit(temp, rt, Vector2.one, Vector2.zero);
            RenderTexture.active = rt;
            postProcessTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        }
        catch (Exception e)
        {
            throw;
        }
        finally
        {
            DestroyImmediate(temp);
            RenderTexture.active = null;
            rt.Release();
        }

        postProcessTexture.Apply();
    }

    private void PolygonClipLayout(string assetPath)
    {
        if (index == 0 && texture)
        {
            var bytes = File.ReadAllBytes(assetPath);
            Texture2D temp = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount != 0);
            temp.LoadImage(bytes);
            //EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Left", GUILayout.Width(150f));

            _left = EditorGUILayout.IntSlider(_left, 0, temp.width - 1,
                GUILayout.Width(200f), GUILayout.Height(20f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Right", GUILayout.Width(150f));

            _right = EditorGUILayout.IntSlider(_right, 0, temp.width - 1,
                GUILayout.Width(200f), GUILayout.Height(20f));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Top", GUILayout.Width(150f));

            _top = EditorGUILayout.IntSlider(_top, 0, temp.height - 1,
                GUILayout.Width(200f), GUILayout.Height(20f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bottom", GUILayout.Width(150f));

            _bottom = EditorGUILayout.IntSlider(_bottom, 0, temp.height - 1,
                GUILayout.Width(200f), GUILayout.Height(20f));
            EditorGUILayout.EndHorizontal();


            PolygonClip(temp, ref _postProcessTexture, (int) _left, (int) _right,
                (int) _top, (int) _bottom);
            DestroyImmediate(temp);
        }
    }

    void SavePng(Texture2D texture, string path)
    {
        var bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
    }

    private void PolygonClip(Texture2D source, ref Texture2D postProcessTexture, int left, int right, int top,
        int bottom)
    {
        if (postProcessTexture)
            DestroyImmediate(postProcessTexture);
        left = Mathf.Clamp(left, 0, source.width - 2);
        right = Mathf.Clamp(right, left + 1, source.width - 1);
        bottom = Mathf.Clamp(bottom, 0, source.height - 2);
        top = Mathf.Clamp(top, bottom + 1, source.height - 1);

        //Copy the new texture
        int skipWidth = right - left;
        int skipHeight = top - bottom;
        postProcessTexture = new Texture2D(left + (source.width - right),
             source.height - top + bottom,
            source.format, false);


        for (int i = 0; i < postProcessTexture.height; i++)
        {
            for (int j = 0; j < postProcessTexture.width; j++)
            {
                int sourceX = j <= left ? j : j + skipWidth;
                int sourceY = i <= bottom ? i : i + skipHeight;
                var color = source.GetPixel(sourceX, sourceY);
                postProcessTexture.SetPixel(j, i, color);
            }
        }

        postProcessTexture.Apply();
    }

    void ClipTexture(Texture2D sourceTex, ref Texture2D postProcessTexture, Vector2 scale, Vector2 offset)
    {
        if (postProcessTexture)
            DestroyImmediate(postProcessTexture);
        RenderTexture rt = new RenderTexture(sourceTex.width, sourceTex.height, 0, RenderTextureFormat.ARGB32);
        try
        {
            Graphics.Blit(sourceTex, rt, scale, offset);
            RenderTexture.active = rt;
            postProcessTexture = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBA32, false);
            postProcessTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        }
        catch (Exception e)
        {
            throw;
        }
        finally
        {
            RenderTexture.active = null;
            rt.Release();
        }

        postProcessTexture.Apply();

    }

    //        Func<Task> func = async () =>
    //        {
    //            await Task.Delay(TimeSpan.FromSeconds(2));
    //            Debug.Log(DateTime.UtcNow);
    //        };
    //        Debug.Log(DateTime.UtcNow);
    //        func();


}