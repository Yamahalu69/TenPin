using UnityEngine;
using System.Collections;

// Rigidbody必須
[RequireComponent(typeof(Rigidbody))]
public class PinController : MonoBehaviour
{
    // 外部（GameManager）から「今気絶してる？」と聞けるようにする
    public bool IsStunned => isStunned;

    // 「何秒後に復活するか」をGameManagerに伝えるためのプロパティ
    public float RevivalTime { get; private set; } = 0f;

    [Header("--- 逃走設定 ---")]
    [SerializeField] private float runPower = 20.0f; // 逃げる足の速さ
    [SerializeField] private float safeDistance = 20.0f; // 逃げ始める距離

    [Header("--- 衝突・気絶設定 ---")]
    [SerializeField] private float impactForce = 15.0f; // ぶっ飛ぶ強さ
    [SerializeField] private float stunTime = 120.0f; // 気絶時間（秒）
    [SerializeField] private Color stunnedColor = Color.gray; // ★【追加】倒れた時の色

    // 内部変数
    private Transform player;
    private Rigidbody rb;
    private Renderer myRenderer; // ★【追加】色を変えるために必要
    private Color originalColor; // ★【追加】元の色を覚えておく
    private bool isStunned = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ★【追加】レンダラーを取得し、元の色を保存しておく
        myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            originalColor = myRenderer.material.color;
        }

        // 重心を底面に下げる
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        // 摩擦設定
        rb.linearDamping = 5.0f;
        rb.angularDamping = 5.0f;

        // 復活場所を記憶
        initialPosition = rb.position;
        initialRotation = rb.rotation;

        // プレイヤーを見つける
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        // 気絶中、またはプレイヤー不在なら何もしない
        if (isStunned || player == null) return;

        RunAwayLogic();
    }

    // 逃げるAI
    void RunAwayLogic()
    {
        Transform targetThreat = GetClosestThreat();
        if (targetThreat == null) return;

        Vector3 runDirection = rb.position - targetThreat.position;
        runDirection.y = 0;
        runDirection.Normalize();

        rb.AddForce(runDirection * runPower, ForceMode.Acceleration);

        if (runDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(runDirection);
            Quaternion nextRotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            rb.MoveRotation(nextRotation);
        }
    }

    Transform GetClosestThreat()
    {
        Transform closestTransform = null;
        float closestDist = safeDistance;

        // A. プレイヤーとの距離
        if (player != null)
        {
            float distToPlayer = Vector3.Distance(rb.position, player.position);
            if (distToPlayer < closestDist)
            {
                closestDist = distToPlayer;
                closestTransform = player;
            }
        }

        // B. 近くのボール
        Collider[] nearbyObjects = Physics.OverlapSphere(rb.position, safeDistance);
        foreach (var col in nearbyObjects)
        {
            if (col.CompareTag("Ball"))
            {
                float distToBall = Vector3.Distance(rb.position, col.transform.position);
                if (distToBall < closestDist)
                {
                    closestDist = distToBall;
                    closestTransform = col.transform;
                }
            }
        }
        return closestTransform;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isStunned) return;

        if (collision.gameObject.CompareTag("Ball"))
        {
            StartCoroutine(StunRoutine(collision));
        }
    }

    IEnumerator StunRoutine(Collision collision)
    {
        isStunned = true;

        // 復活予定時刻をセット
        RevivalTime = Time.time + stunTime;

        // ★【追加】色を暗くする
        if (myRenderer != null)
        {
            myRenderer.material.color = stunnedColor;
        }

        Vector3 incomingDir = (rb.position - collision.transform.position).normalized;
        Vector3 blowAwayDir = incomingDir + Vector3.up * 0.5f;

        float originalDrag = rb.linearDamping;
        rb.linearDamping = 0.5f;

        rb.AddForce(blowAwayDir * impactForce, ForceMode.Impulse);

        yield return new WaitForSeconds(stunTime);

        // --- 復活処理 ---
        rb.isKinematic = true;
        rb.linearDamping = originalDrag;

        yield return new WaitForFixedUpdate();

        rb.MovePosition(initialPosition + Vector3.up * 0.5f);
        rb.MoveRotation(initialRotation);

        // ★【追加】色を元に戻す
        if (myRenderer != null)
        {
            myRenderer.material.color = originalColor;
        }

        rb.isKinematic = false;
        isStunned = false;
        RevivalTime = 0f;
    }
}