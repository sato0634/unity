
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rigidbodyを使用したプレイヤーのジャンプ処理。
/// Unity 6 + Input System対応。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerJump : MonoBehaviour
{
    [Header("ジャンプ設定")]

    [Tooltip("ジャンプの到達高度（m）")]
    [Min(0f)]
    [SerializeField] private float jumpHeight = 3.0f;

    [Tooltip("上昇中の重力倍率")]
    [Min(0f)]
    [SerializeField] private float upwardGravityMultiplier = 1.0f;

    [Tooltip("下降中の重力倍率")]
    [Min(0f)]
    [SerializeField] private float fallGravityMultiplier = 2.5f;


    [Header("ジャンプ入力")]

    [Tooltip("ジャンプ入力を記憶する時間（秒）")]
    [Min(0f)]
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Tooltip("ジャンプに使用するキー")]
    [SerializeField] private Key jumpKey = Key.Space;


    [Header("接地判定")]

    [Tooltip("地面として扱うLayer")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("接地判定に使用するSphereCastの半径")]
    [Min(0.01f)]
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Tooltip("コライダー底面から地面を探す距離")]
    [Min(0f)]
    [SerializeField] private float groundCheckDistance = 0.15f;


    [Header("デバッグ")]

    [Tooltip("Sceneビューに接地判定を表示")]
    [SerializeField] private bool showGroundCheckGizmo = true;


    private Rigidbody rb;
    private Collider col;

    private bool isGrounded;
    private float jumpBufferCounter;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // プレイヤーが倒れないようにする
        rb.constraints |= RigidbodyConstraints.FreezeRotation;
    }


    private void Update()
    {
        // 接地判定
        CheckGround();

        // ジャンプ入力
        if (Keyboard.current != null &&
            Keyboard.current[jumpKey].wasPressedThisFrame)
        {
            // 入力を一定時間保存する
            jumpBufferCounter = jumpBufferTime;
        }

        // ジャンプ入力の残り時間を減らす
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }


    private void FixedUpdate()
    {
        // 接地していて、ジャンプ入力が保存されていればジャンプ
        if (isGrounded && jumpBufferCounter > 0f)
        {
            ExecuteJump();

            // 使用した入力を消費
            jumpBufferCounter = 0f;
        }

        // カスタム重力
        ApplyCustomGravity();
    }


    /// <summary>
    /// SphereCastを使用して接地状態を確認する。
    /// </summary>
    private void CheckGround()
    {
        Bounds bounds = col.bounds;

        Vector3 origin = new Vector3(
            bounds.center.x,
            bounds.min.y + groundCheckRadius,
            bounds.center.z
        );

        isGrounded = Physics.SphereCast(
            origin,
            groundCheckRadius,
            Vector3.down,
            out _,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }


    /// <summary>
    /// jumpHeightからジャンプ初速度を計算して適用する。
    /// </summary>
    private void ExecuteJump()
    {
        // v = √(2gh)
        float gravityMagnitude =
            Mathf.Abs(
                Physics.gravity.y * upwardGravityMultiplier
            );

        float jumpVelocity =
            Mathf.Sqrt(2.0f * gravityMagnitude * jumpHeight);

        Vector3 velocity = rb.linearVelocity;

        // 垂直方向の速度だけ変更
        velocity.y = jumpVelocity;

        rb.linearVelocity = velocity;
    }


    /// <summary>
    /// 上昇中と下降中で重力倍率を変更する。
    /// </summary>
    private void ApplyCustomGravity()
    {
        if (isGrounded)
        {
            return;
        }

        float gravityMultiplier;

        if (rb.linearVelocity.y < 0f)
        {
            // 下降中
            gravityMultiplier = fallGravityMultiplier;
        }
        else
        {
            // 上昇中
            gravityMultiplier = upwardGravityMultiplier;
        }

        float extraGravity =
            (gravityMultiplier - 1.0f) * Physics.gravity.y;

        rb.linearVelocity +=
            Vector3.up *
            extraGravity *
            Time.fixedDeltaTime;
    }


    /// <summary>
    /// Sceneビューに接地判定を表示する。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showGroundCheckGizmo)
        {
            return;
        }

        Collider targetCollider = GetComponent<Collider>();

        if (targetCollider == null)
        {
            return;
        }

        Bounds bounds = targetCollider.bounds;

        Vector3 origin = new Vector3(
            bounds.center.x,
            bounds.min.y + groundCheckRadius,
            bounds.center.z
        );

        Vector3 end =
            origin + Vector3.down * groundCheckDistance;

        Gizmos.DrawWireSphere(
            origin,
            groundCheckRadius
        );

        Gizmos.DrawWireSphere(
            end,
            groundCheckRadius
        );

        Gizmos.DrawLine(origin, end);
    }
}