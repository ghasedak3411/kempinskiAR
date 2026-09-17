using UnityEngine;

/// <summary>
/// این ابجکت رو همیشه به سمت یک ترنسفرم هدف (مثلا دوربین کاربر) می‌چرخونه،
/// فقط روی محور Y (Yaw) - یعنی کج یا واژگون نمیشه، همیشه صاف می‌مونه.
/// </summary>
public class LookAtYAxisOnly : MonoBehaviour
{
    [Tooltip("ابجکتی که باید به سمتش نگاه کنه؛ اگه خالی بمونه خودش Main Camera رو پیدا می‌کنه")]
    public Transform target;

    void Start()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            if (Camera.main != null)
                target = Camera.main.transform;
            else
                return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // ارتفاع رو نادیده بگیر تا فقط دوران Y اعمال بشه

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }
}