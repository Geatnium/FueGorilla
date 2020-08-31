using UnityEngine;

public class MouseLooker : MonoBehaviour
{
    private Transform camTr; // カメラのトランスフォーム
    private Transform camRotTr; // カメラの回転軸オブジェクトのトランスフォーム
    private Vector3 camRot; // カメラの回転用のベクトル

    /// <summary>
    /// カメラ移動の感度
    /// </summary>
    private float sensitivity = 1.0f;
    private float sensitivityM = 1.0f;

    /// <summary>
    /// 設定ファイル
    /// </summary>
    private PlayerConfig playerConfig;

    public void StartM()
    {
        // 設定ファイルを読み込み
        playerConfig = Resources.Load<PlayerConfig>("PlayerConfigData");
        sensitivity = playerConfig.mouseLookSensitivity;
        // カメラと回転軸のトランスフォームを取得し、カメラを入れ子にして位置を調整
        camTr = Camera.main.transform;
        camRotTr = transform.Find("CameraRot");
        camRot = camRotTr.eulerAngles;
        camTr.parent = camRotTr;
        camTr.localEulerAngles = Vector3.zero;
        camTr.localPosition = new Vector3(0.0f, 0.0f, -playerConfig.cameraDistance + 0.25f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateM()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            sensitivityM += 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            sensitivityM -= 0.1f;
        }
        if (sensitivityM <= 0.0f) sensitivityM = 0.1f;
        if (sensitivityM >= 2.0f) sensitivityM = 1.9f;
        // マウス移動の入力
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * sensitivityM;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * sensitivityM;
        // マウスの移動量によってカメラを範囲内で回転
        camRot = new Vector3(Mathf.Clamp(camRot.x - mouseY, -playerConfig.cameraDownMax, playerConfig.cameraUpMax), camRot.y + mouseX, 0f);
        camRotTr.eulerAngles = new Vector3(camRot.x, camRot.y, 0);
        // 回転軸からカメラに向けてレイを撃って、cameraDistance以内で当たっていたらカメラを寄せる
        Ray ray = new Ray(camRotTr.position, camTr.position - camRotTr.position);
        RaycastHit hit;
        float distance = playerConfig.cameraDistance;
        if (Physics.Raycast(ray, out hit, playerConfig.cameraDistance))
        {
            distance = hit.distance;
        }
        camTr.localPosition = new Vector3(0, 0, -distance + 0.25f);
    }
}
