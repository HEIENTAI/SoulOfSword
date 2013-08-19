using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CameraManager
{
    List<Camera> _allCamera = new List<Camera>();

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
            Vector3 myRolePosInCameraViewPort = ca.WorldToViewportPoint(GameMain.Instance.MyRole.transform.position);
            Vector3 diff = GameMain.Instance.MyRole.transform.position - ca.transform.position;
            
            if (myRolePosInCameraViewPort.x >= 0.0f && myRolePosInCameraViewPort.x <= 1.0f
                && myRolePosInCameraViewPort.y >= 0.0f && myRolePosInCameraViewPort.y <= 1.0f
                && myRolePosInCameraViewPort.z > 0.0f) // 看的到
            {
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
        if (nearCamera != null)
        {
            foreach (Camera ca in _allCamera)
            {
                ca.enabled = (ca == nearCamera);
                ca.GetComponent<AudioListener>().enabled = ca.enabled;
            }
        }
    }

}
