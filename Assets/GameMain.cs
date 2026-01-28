using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("--- ゲーム設定 ---")]
    [SerializeField] private float timeLimit = 300.0f; // 制限時間
    [SerializeField] private string nextSceneName = "Title";

    [Header("--- UI設定 ---")]
    [SerializeField] private Text timerText; // 全体の制限時間
    [SerializeField] private Text revivalText; // ピン復活までのカウントダウン
    [SerializeField] private Text remainingPinsText; // ★【追加】残りピン数表示用
    [SerializeField] private GameObject clearPanel;

    // 内部変数
    private float currentTime;
    private bool isGameActive = true;
    private PinController[] allPins;

    void Start()
    {
        currentTime = timeLimit;

        // ステージ上の全ピンを取得
        GameObject[] pinObjects = GameObject.FindGameObjectsWithTag("Pin");
        allPins = new PinController[pinObjects.Length];
        for (int i = 0; i < pinObjects.Length; i++)
        {
            allPins[i] = pinObjects[i].GetComponent<PinController>();
        }

        if (revivalText != null) revivalText.text = "";

        // 初回の残り本数表示更新
        UpdateRemainingPinsText(allPins.Length);

        Debug.Log("ゲーム開始！ ピンの数: " + allPins.Length);
    }

    void Update()
    {
        if (!isGameActive) return;

        // 1. 全体時間のカウントダウン
        HandleGlobalTimer();

        // 2. ピン復活までのドキドキタイマー表示
        HandleRevivalTimer();

        // 3. クリア判定 ＆ 残り本数更新
        CheckGameStatus();
    }

    void HandleGlobalTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            GameOver();
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60F);
            int seconds = Mathf.FloorToInt(currentTime % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (currentTime <= 30f) timerText.color = Color.red;
            else timerText.color = Color.white;
        }
    }

    void HandleRevivalTimer()
    {
        if (revivalText == null) return;

        float minRevivalTime = Mathf.Infinity;
        bool isAnyPinDown = false;

        foreach (var pin in allPins)
        {
            if (pin != null && pin.IsStunned)
            {
                isAnyPinDown = true;
                if (pin.RevivalTime < minRevivalTime)
                {
                    minRevivalTime = pin.RevivalTime;
                }
            }
        }

        if (isAnyPinDown)
        {
            float remaining = minRevivalTime - Time.time;
            if (remaining < 0) remaining = 0;

            revivalText.text = $"復活まで: {remaining:0.0}秒";
            revivalText.color = Color.yellow;

            if (remaining < 3.0f) revivalText.color = Color.red;
        }
        else
        {
            revivalText.text = "";
        }
    }

    // ★【修正】クリア判定と残り本数表示をまとめて行う
    void CheckGameStatus()
    {
        int downedCount = 0;

        foreach (var pin in allPins)
        {
            if (pin != null && pin.IsStunned)
            {
                downedCount++;
            }
        }

        // 残りの本数を計算 (全体 - 倒れた数)
        int remainingCount = allPins.Length - downedCount;

        // ★【追加】UI更新
        UpdateRemainingPinsText(remainingCount);

        // 全員倒れていたらクリア
        if (downedCount >= allPins.Length && allPins.Length > 0)
        {
            GameClear();
        }
    }

    // ★【追加】残り本数のテキスト更新用メソッド
    void UpdateRemainingPinsText(int count)
    {
        if (remainingPinsText != null)
        {
            remainingPinsText.text = "残りピン数: " + count + "本";
        }
    }

    void GameClear()
    {
        isGameActive = false;
        Debug.Log("<color=yellow>ゲームクリア！すべてのピンを倒しました！</color>");

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        if (revivalText != null) revivalText.text = "";
    }

    void GameOver()
    {
        isGameActive = false;
        Debug.Log("<color=red>タイムアップ！ゲームオーバー...</color>");
    }
}