using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("--- 寿命設定 ---")]
    [SerializeField] private float lifeTime = 20.0f; // 20秒後に自動消滅

    [Header("--- エイム補正（ホーミング） ---")]
    [SerializeField] private float homingForce = 5.0f; // 追尾する力の強さ（弱めに設定）
    [SerializeField] private float homingRange = 50.0f; // 索敵範囲

    private Rigidbody rb;
    private Transform targetPin;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 1. 発射されて20秒後に消える
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // 3. ほんの少しだけPinに向かってカーブする（エイム補正）
        ApplyHoming();
    }

    // ピンに当たった時の処理
    void OnCollisionEnter(Collision collision)
    {
        // 2. Pinタグを持つ物体に触れた時、自分（Ball）が消える
        if (collision.gameObject.CompareTag("Pin"))
        {
            // 当たった瞬間すぐ消すと衝撃が伝わらないことがあるため、
            // 衝撃を与え終わった直後（0.05秒後など）に消すのがコツですが、
            // ここでは即座に消滅させます
            Destroy(gameObject);
        }
    }

    // 一番近いピンを探して少し力を加える処理
    void ApplyHoming()
    {
        // ステージ上のすべてのPinを探す
        GameObject[] pins = GameObject.FindGameObjectsWithTag("Pin");

        float closestDistance = Mathf.Infinity;
        Transform closestPin = null;

        // 一番近いピンを特定する
        foreach (GameObject pin in pins)
        {
            // ピンが既に気絶中や倒れている場合を除外したい場合はここに条件を追加
            // 今回は単純に距離だけで判定します

            float distance = Vector3.Distance(transform.position, pin.transform.position);

            // 範囲内かつ、これまでで一番近ければターゲットにする
            if (distance < closestDistance && distance < homingRange)
            {
                closestDistance = distance;
                closestPin = pin.transform;
            }
        }

        // ターゲットが見つかったら、そこに向かって少し力を加える
        if (closestPin != null)
        {
            Vector3 direction = (closestPin.position - transform.position).normalized;

            // 加速モード(Acceleration)で力を加えることで、質量に関係なく一定の補正をかける
            rb.AddForce(direction * homingForce, ForceMode.Acceleration);
        }
    }
}