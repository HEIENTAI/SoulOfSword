using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CameraManager
{
    List<Camera> _allCamera = new List<Camera>();
    Camera _curCamera = null;
    public Camera CurrentCamera
    {
        get { return _curCamera; }
    }
    /// <summary>
    /// 刷新這個場景的攝影機
    /// </summary>
    public void RefreshCurrentSceneCameras()
    {
        GameObject[] allCamera = GameObject.FindGameObjectsWithTag("MainCamera");

        foreach (GameObject go in allCamera)
        {
            if (go.camera != null)
            {
                _allCamera.Add(go.camera);
            }
        }
    }
    /// <summary>
    /// 更新現在的主攝影機
    /// </summary>
    public void UpdateMainCamera()
    {
        if (_allCamera.Count == 0)
            return; // 沒有任何攝影機，不作事
        float angle = 360.0f;
        float dis = Mathf.Infinity;
        Camera nearCamera = null;

        foreach (Camera ca in _allCamera)
        {
            if (ca.WorldPositionInCameraViewPort(GameMain.Instance.MyRole.transform.position))
            {
                Vector3 diff = GameMain.Instance.MyRole.transform.position - ca.transform.position;
                float curAngle = Vector3.Angle(ca.transform.forward, diff);
                float curDis = diff.sqrMagnitude;
                if (curAngle < angle || (curAngle == angle && curDis < dis))
                {
                    nearCamera = ca;
                    angle = curAngle;
                    dis = curDis;
                }
            }
        }
        if (nearCamera == null)
        {
            nearCamera = Camera.main;
        }
        if (nearCamera != null)
        {
            _curCamera = nearCamera; // 設定現在的camera
            foreach (Camera ca in _allCamera)
            {
                ca.enabled = (ca == nearCamera);
                ca.GetComponent<AudioListener>().enabled = ca.enabled;
            }
        }
    }

}
