using UnityEngine;

[CreateAssetMenu(
  fileName = "PlayerConfigData",
  menuName = "ScriptableObject/PlayerConfigData",
  order = 0)
]
public class PlayerConfig : ScriptableObject
{
    /// <summary>
    /// カメラ移動の感度
    /// </summary>
    public float mouseLookSensitivity = 2.0f;
    /// <summary>
    /// カメラとプレイヤー間の距離
    /// </summary>
    public float cameraDistance = 5.0f;
    /// <summary>
    /// 下向きの最大角度
    /// </summary>
    public float cameraDownMax = 60.0f;
    /// <summary>
    /// 上向きの最大角度
    /// </summary>
    public float cameraUpMax = 60.0f;
    /// <summary>
    /// 移動の加速度
    /// </summary>
    public float moveAcc = 1.0f;
    /// <summary>
    /// 移動しない時の減速力
    /// </summary>
    public float moveDec = 4.0f;
    /// <summary>
    /// 移動の最大速度
    /// </summary>
    public float moveSpeed = 2.0f;
    /// <summary>
    /// 回転の速さ
    /// </summary>
    public float rotateSpeed = 30.0f;
    /// <summary>
    /// ジャンプ力
    /// </summary>
    public float jumpForce = 5.0f;
    /// <summary>
    /// 空中での軌道修正できる割合
    /// </summary>
    public float airMoveMultiply = 0.1f;
    /// <summary>
    /// 着地モーションの時、減速する割合
    /// </summary>
    public float landingDeceMultiply = 0.75f;
    /// <summary>
    /// 掴みモーションの時の減速
    /// </summary>
    public float grabingDeceMultiply = 0.8f;
    /// <summary>
    /// 連続で掴んでいられる時間
    /// </summary>
    public float maxGrabingTime = 1.5f;
    /// <summary>
    /// 一度誰かをつかめたら、一定時間のインターバル
    /// </summary>
    public float grabInterval = 1.0f;
    /// <summary>
    /// ゴリラの攻撃モーション時の減速
    /// </summary>
    public float gorillaAttackingDeceMultiply = 0.1f;

    public float gorillaAttackPower = 10.0f;

    public float gorillaPunchKnockBack = 0.5f;
}
