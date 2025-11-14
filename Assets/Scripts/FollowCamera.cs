using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target; // ボール
    public Vector3 offset = new Vector3(0, 5, -7);
    public float rotateSpeed = 3f;   // マウス回転速度
    public float returnSpeed = 2f;   // 元の位置に戻る速度

    private Vector3 currentOffset;   // 現在のカメラオフセット
    private bool isFreeLook = false; // 自由視点中かどうか
    private Vector3 targetOffset;    // 元に戻す時の目標オフセット

    void Start()
    {
        currentOffset = offset;
        targetOffset = offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ===== マウスホイール押し中（自由視点） =====
        if (Input.GetMouseButton(2)) // 中クリック
        {
            isFreeLook = true;

            float rotX = Input.GetAxis("Mouse X") * rotateSpeed;
            float rotY = -Input.GetAxis("Mouse Y") * rotateSpeed;

            // offsetを回転させてカメラをボールの周囲に回す
            Quaternion rotationX = Quaternion.AngleAxis(rotX, Vector3.up);
            Quaternion rotationY = Quaternion.AngleAxis(rotY, transform.right);

            currentOffset = rotationY * rotationX * currentOffset;
        }
        else
        {
            // 離した瞬間に「元のoffset」にスムーズに戻す
            if (isFreeLook)
            {
                isFreeLook = false;
                targetOffset = offset;
            }

            // スライドして元に戻る（滑らか補間）
            currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * returnSpeed);
        }

        // ===== カメラ位置更新 =====
        transform.position = target.position + currentOffset;
        transform.LookAt(target);
    }
}
