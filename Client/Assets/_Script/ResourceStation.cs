using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 資源管理 class, 使用者無需得知資源路徑, 交由此 class 管理取得的資源
/// 使用和亞特類似的管理法（只管到assetbundle）
/// </summary>
public class ResourceStation : MonoBehaviour
{
    #region Singleton
    private static ResourceStation _instance;

    public static ResourceStation Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go;
                GameMain gameMain = GameMain.Instance;

                go = (gameMain == null) ? new GameObject("ResourceStation") : gameMain.gameObject;

                _instance = go.AddComponent<ResourceStation>();
            }
            return _instance;
        }
    }
    #endregion


    public Object GetResource(string name)
    {
        return Resources.Load(name);
    }

    /// <summary>
    /// 依據modelName取得資源
    /// </summary>
    /// <param name="modelName">模型名稱</param>
    /// <returns>相對應資源</returns>
    public Object GetModelResource(string modelName)
    {
        return Resources.Load(GlobalConst.DIR_MODEL + Path.AltDirectorySeparatorChar + modelName + System.IO.Path.AltDirectorySeparatorChar + modelName); 
    }
}
