using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GolfShot : MonoBehaviour
{
    public float maxPower = 30f;            // 最大ショット力
    public LineRenderer aimLine;            // 方向・パワーを表示
    public float powerMultiplier = 5f;      // パワー係数
    public float stopThreshold = 1f;      // 減速開始速度
    public float slowStopDuration = 3f;     // 減速して止まるまでの時間(秒)
    public float stopTime = 0.5f;           // 停止判定までの時間

    private Rigidbody rb;
    private Vector3 dragStartPoint;
    private bool isDragging;
    private bool canShoot = true;
    private bool slowingDown = false;
    private float slowTimer = 0f;
    private float stopTimer = 0f;
    private Camera mainCam;

    public Text shotCountText; //打数表示用のUI
    public GameObject clearPanel; //クリア時に表示するUI
    public int shotCount = 0; //打数カウント  

    private Vector3 lastShotPosition; //ショット前の位置を保存

    [Header("ステージ遷移")]
    public string nextSceneName;  // 次のステージ名
    public float nextStageDelay = 2f; // 何秒後に遷移するか


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        if (aimLine) aimLine.enabled = false;

        UpdateShotText();
        if (clearPanel) clearPanel.SetActive(false);
    }

    void Update()
    {
        // --- 減速・停止処理 ---
        if (!canShoot)
        {
            float speed = rb.linearVelocity.magnitude;

            // 一定以下になったら減速開始
            if (speed < stopThreshold && speed > 0.01f && !slowingDown)
            {
                slowingDown = true;
                slowTimer = 0f;
            }

            // 減速中
            if (slowingDown)
            {
                slowTimer += Time.deltaTime;
                float t = Mathf.Clamp01(slowTimer / slowStopDuration);

                // 線形補間で速度を減らす
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, t);

                // 1秒経過で完全停止
                if (slowTimer >= slowStopDuration)
                {
                    rb.linearVelocity = Vector3.zero;
                    slowingDown = false;
                }
            }

            // 完全停止判定
            if (rb.linearVelocity.magnitude < 0.05f)
            {
                stopTimer += Time.deltaTime;
                if (stopTimer >= stopTime)
                {
                    canShoot = true;  // 再度ショット可能
                    stopTimer = 0;
                }
            }
            else
            {
                stopTimer = 0;
            }
        }

        // --- 打てる状態のときのみショット操作 ---
        if (canShoot)
        {
            // ドラッグ開始
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                dragStartPoint = Input.mousePosition;

                if (aimLine)
                {
                    aimLine.enabled = true;
                    aimLine.SetPosition(0, transform.position);
                    aimLine.SetPosition(1, transform.position);
                }
            }

            // ドラッグ中
            if (isDragging)
            {
                Vector3 dragEndPoint = Input.mousePosition;
                Vector3 dir = dragStartPoint - dragEndPoint;
                dir.z = dir.y;
                dir.y = 0;
                Vector3 shotDir = new Vector3(dir.x, 0, dir.z).normalized;

                float distance = Vector3.Distance(dragStartPoint, dragEndPoint);
                float power = Mathf.Min(distance / 10f, maxPower);

                if (aimLine)
                {
                    aimLine.SetPosition(0, transform.position);
                    aimLine.SetPosition(1, transform.position + shotDir * power / 2f);
                }
            }

            // ショット発射
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                canShoot = false;
                slowingDown = false;

                Vector3 dir = dragStartPoint - Input.mousePosition;
                dir.z = dir.y;
                dir.y = 0;
                Vector3 shotDir = new Vector3(dir.x, 0, dir.z).normalized;

                float distance = Vector3.Distance(dragStartPoint, Input.mousePosition);
                float power = Mathf.Min(distance / 10f, maxPower);

                rb.AddForce(shotDir * power * powerMultiplier, ForceMode.Impulse);

                if (aimLine)
                    aimLine.enabled = false;

                lastShotPosition = transform.position;

                // ====== 打数を増やす ======
                shotCount++;
                UpdateShotText();
            }
        }

        // ====== ボールが落ちたらリセット ======
        if (!canShoot && transform.position.y < -5f)
        {
            // 落下停止（速度を完全リセット）
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 一瞬だけ物理を無効化してから戻すと完全停止できる
            rb.isKinematic = true;
            transform.position = lastShotPosition;
            rb.isKinematic = false;

            // 次のショット可能
            canShoot = true;
            slowingDown = false;
            stopTimer = 0f;
        }
    }
    // ====== 打数をUIに表示する関数 ======
    void UpdateShotText()
    {
        if (shotCountText)
            shotCountText.text = "打数: " + shotCount.ToString();
    }

    // ====== ゴール判定 ======
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            Debug.Log("クリア！");
            if (clearPanel)
                clearPanel.SetActive(true);

            // Inspector で指定したシーンに遷移
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Invoke(nameof(LoadNextStage), nextStageDelay);
            }
        }
    }

    void LoadNextStage()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
