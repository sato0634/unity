using UnityEngine;

/// <summary>
/// プレイヤーが特定のY軸座標まで落下すると、設定した座標に戻すスクリプト。
/// 落下検知Y座標（fallThreshold）とリスポーン位置（respawnPosition）は
/// Inspector からでも他スクリプトからでも書き換え可能。
/// PlayerMovement / PlayerJump / PlayerDash と同じ GameObject にアタッチして使う。
/// </summary>
public class PlayerRespawn : MonoBehaviour
{
    [Header("落下検知")]
    [Tooltip("このY座標より下まで落下したらリスポーンする")]
    [SerializeField] private float fallThreshold = -15f;

    [Header("リスポーン先")]
    [Tooltip("落下時に戻す座標")]
    [SerializeField] private Vector3 respawnPosition = Vector3.zero;

    [Header("その他設定")]
    [Tooltip("true の場合、アタッチ時点の位置を自動でリスポーン位置として記録する")]
    [SerializeField] private bool useStartPositionAsRespawn = false;

    [Tooltip("リスポーン時に速度をリセットする")]
    [SerializeField] private bool resetVelocityOnRespawn = true;

    [Tooltip("リスポーン後に一瞬無敵にする時間（秒）。0 で無効")]
    [SerializeField] private float invincibilityAfterRespawn = 0.5f;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private PlayerDash playerDash;
    private float invincibleTimer = 0f;

    public bool IsInvincible => invincibleTimer > 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerDash = GetComponent<PlayerDash>();

        if (useStartPositionAsRespawn)
        {
            respawnPosition = transform.position;
        }
    }

    private void Update()
    {
        // 無敵時間のカウントダウン
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
        }

        // Y軸の落下判定
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    /// <summary>
    /// リスポーン処理。他スクリプトからも呼び出せる。
    /// </summary>
    public void Respawn()
    {
        transform.position = respawnPosition;

        if (resetVelocityOnRespawn && rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // ダッシュ中なら解除してクールダウンもリセット
        if (playerDash != null)
        {
            playerDash.SetDashing(false);
        }

        if (invincibilityAfterRespawn > 0f)
        {
            invincibleTimer = invincibilityAfterRespawn;
        }
    }

    // ---------------- 外部から座標を書き換えるAPI ----------------

    /// <summary>
    /// リスポーン位置を他スクリプトから変更する（チェックポイント等用）。
    /// </summary>
    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
    }

    /// <summary>
    /// 現在のプレイヤー位置を新しいリスポーン位置として記録する。
    /// </summary>
    public void CaptureCurrentPositionAsRespawn()
    {
        respawnPosition = transform.position;
    }

    /// <summary>
    /// 落下検知Y座標を変更する。
    /// </summary>
    public void SetFallThreshold(float y)
    {
        fallThreshold = y;
    }

    /// <summary>
    /// 設定をまとめて変更する。
    /// </summary>
    public void Configure(float newFallThreshold, Vector3 newRespawnPosition)
    {
        fallThreshold = newFallThreshold;
        respawnPosition = newRespawnPosition;
    }

    public Vector3 RespawnPosition => respawnPosition;
    public float FallThreshold => fallThreshold;

    // ---------------- Sceneビューでの可視化 ----------------

    private void OnDrawGizmosSelected()
    {
        // 落下検知ライン（赤い横線）
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 10f, fallThreshold, 0f),
            new Vector3(transform.position.x + 10f, fallThreshold, 0f));

        // リスポーン位置（黄色のマーカー）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(respawnPosition, 0.5f);
    }
}
