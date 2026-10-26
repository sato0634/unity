using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤー移動（PlayerMovement）とは独立したダッシュ（加速）用スクリプト。
/// PlayerMovementと同じGameObjectにアタッチするだけで、
/// 指定ボタン（デフォルト：Left Shift）の長押しまたはトグルで加速できる。
/// Unity 6 + Input System対応。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("入力設定")]

    [Tooltip("ダッシュのキーバインド。デフォルトはLeft Shift")]
    [SerializeField] private Key dashKey = Key.LeftShift;

    [Tooltip("trueにすると1回押すごとにON/OFFを切り替える。falseなら長押し方式")]
    [SerializeField] private bool toggleDashMode = false;


    [Header("ダッシュ設定")]

    [Tooltip("ダッシュ中の移動速度倍率（PlayerMovementのmoveSpeedに乗算される）")]
    [Min(0f)]
    [SerializeField] private float speedMultiplier = 2.0f;

    [Tooltip("ダッシュ開始時の加速（この値で一気に加速する）")]
    [Min(0f)]
    [SerializeField] private float dashAcceleration = 40.0f;

    [Tooltip("ダッシュ可能時間（秒）。0以下で無制限")]
    [SerializeField] private float dashDuration = 2.0f;

    [Tooltip("ダッシュ後のクールダウン時間（秒）")]
    [Min(0f)]
    [SerializeField] private float cooldownTime = 3.0f;


    [Header("参照設定")]

    [Tooltip("空欄なら同じGameObjectのPlayerMovementを自動取得")]
    [SerializeField] private PlayerMovement playerMovement;


    private Rigidbody rb;

    private bool isDashing = false;

    // ダッシュゲージ（0〜1）。UI表示などに使える
    private float dashGauge = 1.0f;

    private bool isCoolingDown = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
    }


    private void Update()
    {
        ReadDashInput();
        UpdateDashGauge();
    }


    private void FixedUpdate()
    {
        ApplyDash();
    }


    /// <summary>
    /// ダッシュ（加速）の入力を取得する。
    /// </summary>
    private void ReadDashInput()
    {
        if (Keyboard.current == null)
        {
            isDashing = false;
            return;
        }

        KeyControl keyControl = Keyboard.current[dashKey];

        if (keyControl == null)
        {
            isDashing = false;
            return;
        }

        bool canDash = !isCoolingDown && HasDashRemaining();

        if (toggleDashMode)
        {
            // トグル方式：1回押すごとにON/OFF切り替え
            if (keyControl.wasPressedThisFrame)
            {
                if (isDashing)
                {
                    isDashing = false;
                }
                else if (canDash)
                {
                    isDashing = true;
                }
            }
        }
        else
        {
            // 長押し方式：押している間だけダッシュ
            isDashing = canDash && keyControl.isPressed;
        }
    }


    /// <summary>
    /// ダッシュゲージの消費・回復を行う。
    /// </summary>
    private void UpdateDashGauge()
    {
        if (dashDuration <= 0f)
        {
            // 無制限モード
            dashGauge = 1f;
            return;
        }

        if (isDashing)
        {
            dashGauge -= Time.deltaTime / dashDuration;

            if (dashGauge <= 0f)
            {
                // ゲージを使い切ったらダッシュ強制終了＆クールダウン開始
                dashGauge = 0f;
                isDashing = false;
                isCoolingDown = true;
            }
        }
        else
        {
            dashGauge += Time.deltaTime / cooldownTime;

            if (dashGauge >= 1f)
            {
                dashGauge = 1f;
                isCoolingDown = false;
            }
        }
    }


    /// <summary>
    /// まだダッシュゲージが残っているかどうか。
    /// </summary>
    private bool HasDashRemaining()
    {
        return dashDuration <= 0f || dashGauge > 0f;
    }


    /// <summary>
    /// ダッシュ中は目標速度を引き上げて一気に加速する。
    /// </summary>
    private void ApplyDash()
    {
        if (!isDashing)
        {
            return;
        }

        // 移動入力が無い場合はダッシュしない（その場で加速しない）
        if (playerMovement != null && !playerMovement.IsMoving)
        {
            return;
        }

        Vector3 currentVelocity = rb.linearVelocity;

        // XZ平面での進行方向（現在の速度ベース）
        Vector3 flatVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        Vector3 dashDirection;

        if (flatVelocity.sqrMagnitude > 0.001f)
        {
            dashDirection = flatVelocity.normalized;
        }
        else
        {
            // 停止中なら前方へ
            dashDirection = transform.forward;
            dashDirection.y = 0f;

            if (dashDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            dashDirection.Normalize();
        }

        float baseSpeed = playerMovement != null ? playerMovement.MoveSpeed : 5f;
        float dashTargetSpeed = baseSpeed * speedMultiplier;

        Vector3 targetVelocity = dashDirection * dashTargetSpeed;

        // Y方向の速度はジャンプスクリプトに任せる
        targetVelocity.y = currentVelocity.y;

        Vector3 newVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            dashAcceleration * Time.fixedDeltaTime
        );

        // Y方向は変更しない
        newVelocity.y = currentVelocity.y;

        rb.linearVelocity = newVelocity;
    }


    /// <summary>
    /// ダッシュ中かどうかを外部から取得する。
    /// アニメーションやエフェクトの判定に使える。
    /// </summary>
    public bool IsDashing => isDashing;

    /// <summary>
    /// ダッシュゲージ（0〜1）を外部から取得する。UI表示用。
    /// </summary>
    public float DashGauge => dashGauge;

    /// <summary>
    /// クールダウン中かどうかを外部から取得する。
    /// </summary>
    public bool IsCoolingDown => isCoolingDown;


    /// <summary>
    /// 他のスクリプト（UIボタン等）からダッシュ状態を
    /// 強制的に切り替えるための公開メソッド。
    /// </summary>
    public void SetDashing(bool dashing)
    {
        if (dashing && !HasDashRemaining())
        {
            isDashing = false;
            return;
        }

        isDashing = dashing;
    }


    /// <summary>
    /// ダッシュ設定を外部から書き換えるための公開メソッド。
    /// </summary>
    public void Configure(float speedMultiplier, float dashAcceleration, float dashDuration, float cooldownTime)
    {
        this.speedMultiplier = Mathf.Max(0f, speedMultiplier);
        this.dashAcceleration = Mathf.Max(0f, dashAcceleration);
        this.dashDuration = dashDuration;
        this.cooldownTime = Mathf.Max(0.01f, cooldownTime);
    }


    /// <summary>
    /// Sceneビューにダッシュ状態を表示する。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // ダッシュ中は黄色、クールダウン中は赤、通常は白で表示
        Gizmos.color = isDashing ? Color.yellow : (isCoolingDown ? Color.red : Color.white);
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
