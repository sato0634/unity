using UnityEngine;

/// <summary>
/// プレイヤーが指定したY軸座標より下に落下した場合、
/// 設定した座標に戻すスクリプト。
/// EmptyなGameObjectにそのまま貼り付けて使えるように、
/// Rigidbodyの有無を問わず動作する。
/// Unity 6対応。
/// </summary>
public class EmptyScript : MonoBehaviour
{
    [Header("落下検知設定")]

    [Tooltip("このY軸座標より下に落下したらリスポーンする")]
    [SerializeField] private float fallThresholdY = -20.0f;


    [Header("リスポーン設定")]

    [Tooltip("落下後に戻す座標（Inspectorから書き換え可能）")]
    [SerializeField] private Vector3 respawnPosition = Vector3.zero;

    [Tooltip("シーン内の別のオブジェクト（チェックポイント等）をリスポーン地点として追従する。設定した場合は上の座標より優先される")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("リスポーン時に速度をリセットする（Rigidbodyがある場合のみ）")]
    [SerializeField] private bool resetVelocityOnRespawn = true;


    private Rigidbody rb;


    private void Awake()
    {
        // Rigidbodyがあってもなくても動く
        rb = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        // 指定Y軸より下に落下したか判定
        if (transform.position.y < fallThresholdY)
        {
            Respawn();
        }
    }


    /// <summary>
    /// プレイヤーをリスポーン座標に戻す。
    /// </summary>
    private void Respawn()
    {
        Vector3 targetPosition = GetRespawnPosition();

        if (rb != null)
        {
            // Rigidbodyがある場合は物理エンジン経由で移動
            rb.position = targetPosition;

            if (resetVelocityOnRespawn)
            {
                // 落下による速度をクリア
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            // Rigidbodyがない場合は直接Transformを書き換える
            transform.position = targetPosition;
        }
    }


    /// <summary>
    /// リスポーン座標を取得する。
    /// respawnPointが設定されていればそちらを優先する。
    /// </summary>
    private Vector3 GetRespawnPosition()
    {
        if (respawnPoint != null)
        {
            return respawnPoint.position;
        }

        return respawnPosition;
    }


    /// <summary>
    /// スクリプトや他の処理（チェックポイント取得など）から
    /// リスポーン座標を書き換えるための公開メソッド。
    /// </summary>
    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }


    /// <summary>
    /// 現在のリスポーン座標を取得する。
    /// </summary>
    public Vector3 GetRespawnPositionValue()
    {
        return GetRespawnPosition();
    }


    /// <summary>
    /// リスポーン閾値のY座標を書き換えるための公開メソッド。
    /// </summary>
    public void SetFallThresholdY(float newY)
    {
        fallThresholdY = newY;
    }


    /// <summary>
    /// Sceneビューにリスポーン地点と落下閾値を表示する。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 targetPosition =
            respawnPoint != null
                ? respawnPoint.position
                : respawnPosition;

        // リスポーン地点を表示
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);

        // 落下閾値の高さに表示
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-100f, fallThresholdY, 0f),
            new Vector3(100f, fallThresholdY, 0f)
        );
    }
}
