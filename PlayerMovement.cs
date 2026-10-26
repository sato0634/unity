using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rigidbodyを使用した3Dプレイヤー移動。
/// Unity 6 + Input System対応。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]

    [Tooltip("プレイヤーの移動速度")]
    [Min(0f)]
    [SerializeField] private float moveSpeed = 5.0f;

    [Tooltip("移動開始時の加速")]
    [Min(0f)]
    [SerializeField] private float acceleration = 20.0f;

    [Tooltip("移動停止時の減速")]
    [Min(0f)]
    [SerializeField] private float deceleration = 25.0f;


    [Header("ダッシュ（加速）設定")]

    [Tooltip("ダッシュのキーバインド。デフォルトはLeft Shift")]
    [SerializeField] private Key dashKey = Key.LeftShift;

    [Tooltip("ダッシュ中の移動速度")]
    [Min(0f)]
    [SerializeField] private float dashSpeed = 10.0f;

    [Tooltip("ダッシュ開始時の加速（この値で一気に加速する）")]
    [Min(0f)]
    [SerializeField] private float dashAcceleration = 40.0f;

    [Tooltip("按住（長押し）方式ではなく、1回押すごとにON/OFFを切り替える")]
    [SerializeField] private bool toggleDashMode = false;


    [Header("カメラ設定")]

    [Tooltip("移動方向の基準にするカメラ")]
    [SerializeField] private Transform cameraTransform;


    private Rigidbody rb;

    private Vector2 moveInput;

    private bool isDashing = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 物理演算によってプレイヤーが倒れないようにする
        rb.constraints |= RigidbodyConstraints.FreezeRotation;
    }


    private void Update()
    {
        ReadMovementInput();
        ReadDashInput();
    }


    private void FixedUpdate()
    {
        Move();
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

        if (toggleDashMode)
        {
            // トグル方式：1回押すごとにON/OFF切り替え
            if (keyControl.wasPressedThisFrame)
            {
                isDashing = !isDashing;
            }
        }
        else
        {
            // 長押し方式：押している間だけダッシュ
            isDashing = keyControl.isPressed;
        }
    }


    /// <summary>
    /// WASDの入力を取得する。
    /// </summary>
    private void ReadMovementInput()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            horizontal += 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            vertical -= 1f;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            vertical += 1f;
        }

        moveInput = new Vector2(horizontal, vertical);

        // 斜め移動が通常移動より速くならないようにする
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }


    /// <summary>
    /// Rigidbodyを使用してプレイヤーを移動させる。
    /// </summary>
    private void Move()
    {
        Vector3 moveDirection;

        if (cameraTransform != null)
        {
            // カメラの向きを基準にする
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            // 上下方向の影響を取り除く
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection =
                forward * moveInput.y +
                right * moveInput.x;
        }
        else
        {
            // カメラが設定されていない場合は
            // プレイヤー自身の向きを基準にする
            moveDirection =
                transform.forward * moveInput.y +
                transform.right * moveInput.x;
        }

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        Vector3 currentVelocity = rb.linearVelocity;

        // ダッシュ中は速度と加速率を切り替える
        float speed = isDashing ? dashSpeed : moveSpeed;
        float rate;

        if (isDashing)
        {
            rate = dashAcceleration;
        }
        else
        {
            rate = acceleration;
        }

        Vector3 targetVelocity =
            moveDirection * speed;

        // Y方向の速度はジャンプスクリプトに任せる
        targetVelocity.y = currentVelocity.y;

        float currentSpeed =
            new Vector3(
                currentVelocity.x,
                0f,
                currentVelocity.z
            ).magnitude;

        float targetSpeed =
            new Vector3(
                targetVelocity.x,
                0f,
                targetVelocity.z
            ).magnitude;

        if (targetSpeed <= currentSpeed)
        {
            // 減速時はダッシュでも通常の減速率を使う
            rate = deceleration;
        }

        Vector3 newVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            rate * Time.fixedDeltaTime
        );

        // Y方向は変更しない
        newVelocity.y = currentVelocity.y;

        rb.linearVelocity = newVelocity;
    }


    /// <summary>
    /// 現在の移動速度（Inspectorの値）を外部から取得する。
    /// </summary>
    public float MoveSpeed => moveSpeed;

    /// <summary>
    /// ダッシュ中かどうかを外部から取得する。
    /// アニメーションやエフェクトの判定に使える。
    /// </summary>
    public bool IsDashing => isDashing;


    /// <summary>
    /// 他のスクリプト（UIボタン等）からダッシュ状態を
    /// 強制的に切り替えるための公開メソッド。
    /// </summary>
    public void SetDashing(bool dashing)
    {
        isDashing = dashing;
    }


    /// <summary>
    /// Sceneビューに移動方向の参考情報を表示する。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // ダッシュ中のプレイヤーを黄色で表示
        Gizmos.color = isDashing ? Color.yellow : Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}