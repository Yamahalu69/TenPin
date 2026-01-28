using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerShooter : MonoBehaviour
{
    [Header("--- 移動設定 ---")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float mouseSensitivity = 1.5f;

    [Header("--- 視点・ズーム設定 ---")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomFOV = 30f;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("--- 発射設定 ---")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootForce = 30f;
    [SerializeField] private float fireCooldown = 5.0f;

    [Header("--- UI設定 ---")]
    [SerializeField] private Slider cooldownSlider;

    // 内部変数
    private float xRotation = 0f;
    private CharacterController characterController;
    private float currentCooldown = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // カーソルロック
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // UI初期化
        if (cooldownSlider != null)
        {
            cooldownSlider.maxValue = fireCooldown;
            cooldownSlider.value = 0;
            cooldownSlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleZoom();
        HandleShooting();
        HandleResetView();
        UpdateUI();
    }

    // 1. WASD移動
    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move.y = -9.81f; // 重力

        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    // 2. マウス視点操作
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // Spaceキーで視点リセット
    void HandleResetView()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            xRotation = 0f;
            playerCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    // 3. 右クリックでズーム
    void HandleZoom()
    {
        if (playerCamera == null) return;
        float targetFOV = Input.GetMouseButton(1) ? zoomFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    // 4. 左クリックで発射
    void HandleShooting()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            return;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
            currentCooldown = fireCooldown;
            if (cooldownSlider != null) cooldownSlider.gameObject.SetActive(true);
        }
    }

    // 実際にボールを出す処理（整理済み）
    void Shoot()
    {
        if (ballPrefab == null || firePoint == null) return;

        // ボールを生成
        GameObject ball = Instantiate(ballPrefab, firePoint.position, firePoint.rotation);

        // プレイヤーとボールの衝突を無視（自分に当たって止まるのを防ぐ）
        Collider ballCol = ball.GetComponent<Collider>();
        if (ballCol != null)
        {
            Physics.IgnoreCollision(ballCol, characterController);
        }

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // カメラの正面方向に飛ばす
            Vector3 forceDirection = playerCamera.transform.forward * shootForce;
            rb.AddForce(forceDirection, ForceMode.VelocityChange);

            Debug.Log("発射！ 力の大きさ: " + forceDirection.magnitude);
        }
        else
        {
            Debug.LogError("ボールに Rigidbody がついていません！Prefabを確認してください！");
        }
    }

    // 5. UI更新
    void UpdateUI()
    {
        if (cooldownSlider != null)
        {
            cooldownSlider.value = currentCooldown;
            if (currentCooldown <= 0 && cooldownSlider.gameObject.activeSelf)
            {
                cooldownSlider.gameObject.SetActive(false);
            }
        }
    }
}