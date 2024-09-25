using UnityEngine;

public class StreamingAssetsDefine
{
    public const string RootFolderName = "BuildinFiles";

    public static string StreamAssetsDir =>
        $"{Application.streamingAssetsPath}/{RootFolderName}";
}