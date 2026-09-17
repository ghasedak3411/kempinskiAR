
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Google.XR.ARCoreExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// یک نقطه‌ی جغرافیایی + پریفبی که باید روی اون نمایش داده بشه.
/// </summary>
[Serializable]
public class GeoLocationPoint
{
    public string label = "Location";
    public double latitude;
    public double longitude;

    [Tooltip("پریفبی که وقتی کاربر وارد این نقطه شد نمایش داده میشه")]
    public GameObject prefab;

    [Tooltip("زاویه ثابت (Yaw) که همه پریفب‌ها با همین زاویه نمایش داده میشن")]
    public float fixedYRotation = 0f;

    public float triggerDistanceMeters = 7f;

    [Tooltip("مختصات محل spawn (ساختمون) مخصوص همین لوکیشن")]
    public double buildingLatitude = 25.812597;
    public double buildingLongitude = -80.189047;

    // این‌ها موقع اجرا پر میشن، دستی چیزی توشون نذار
    [HideInInspector] public GameObject spawnedInstance;
    [HideInInspector] public ARGeospatialAnchor spawnedAnchor;
}

/// <summary>
/// مدیریت اصلی: فاصله تا هر لوکیشن رو حساب می‌کنه، متن UI رو آپدیت می‌کنه،
/// و وقتی کاربر داخل شعاع یکی از لوکیشن‌ها شد، پریفب مربوطه رو با یک
/// Terrain Anchor (چسبیده به زمین) در همون مختصات نمایش می‌ده.
/// فقط یک پریفب در آنِ واحد فعاله؛ با ورود به لوکیشن جدید، قبلی Destroy میشه.
/// </summary>
public class GeospatialLocationManager : MonoBehaviour
{
    [Header("AR References (از سمپل ARCore Extensions)")]
    public ARAnchorManager anchorManager;
    public AREarthManager earthManager;

    [Header("Locations (فقط برای محاسبه فاصله و تشخیص ورود؛ محل spawn نیستن)")]
    public List<GeoLocationPoint> locations = new List<GeoLocationPoint>();

    [Header("UI")]
    public TMP_Text distanceText;

    [Tooltip("این تکست فقط 'online loc' یا 'offline loc' رو نشون میده، بسته به اینکه مختصات از سرور خونده شده یا نه")]
    public TMP_Text locationSourceText;

    [Header("Settings")]
    [Tooltip("حداکثر فاصله (متر) برای شمرده شدن به عنوان ورود به لوکیشن")]
    public float triggerDistanceMeters = 3f;

    [Tooltip("افست ارتفاع نسبت به زمین. برای چسبیدن کامل به زمین معمولا 0")]
    public double altitudeOffset = 0.0;

    [Header("Remote Locations (اختیاری)")]
    [Tooltip("آدرس فایل txt که مختصات ۴ لوکیشن ازش خونده میشه. اگه در دسترس نبود، مقادیر پیش‌فرض Inspector استفاده میشه.")]
    public string remoteLocationsUrl = "https://ar.arforall.com/loc.txt";

    [Header("Accuracy Requirement")]
    [Tooltip("حداقل دقتی که هم Horizontal Accuracy و هم Vertical Accuracy باید بهش برسن (متر) تا تریگر شروع بشه")]
    public double requiredAccuracyMeters = 3.0;

    private int currentActiveIndex = -1;
    private bool anchorResolveInProgress = false;

    // آخرین وضعیت/خطای Resolve؛ مستقیم روی صفحه نمایش داده میشه
    // تا بدون وصل بودن به Xcode Console هم بشه دید چی شده.
    private string lastStatusMessage = null;

    // تا وقتی مختصات ریموت (یا تصمیم به استفاده از پیش‌فرض) نهایی نشده،
    // هیچ پردازش تریگری شروع نمیشه.
    private bool initializationComplete = false;

    private IEnumerator Start()
    {
        yield return StartCoroutine(LoadLocationsFromRemote());
        initializationComplete = true;
    }

    /// <summary>
    /// تلاش می‌کنه مختصات ۴ لوکیشن رو از remoteLocationsUrl بخونه.
    /// فرمت فایل: ۸ خط، هر خط "lat,lon" بدون فاصله.
    /// ترتیب: تریگر۱، بیلدینگ۱، تریگر۲، بیلدینگ۲، تریگر۳، بیلدینگ۳، تریگر۴، بیلدینگ۴.
    /// اگه آدرس در دسترس نبود یا فرمت خراب بود، هیچ خطایی throw نمیشه و
    /// مقادیر پیش‌فرضی که توی Inspector برای هر ۴ لوکیشن ست شده دست‌نخورده می‌مونه.
    /// </summary>
    private IEnumerator LoadLocationsFromRemote()
    {
        if (string.IsNullOrEmpty(remoteLocationsUrl))
        {
            Debug.Log("[Geospatial] remoteLocationsUrl خالیه؛ از مقادیر پیش‌فرض Inspector استفاده میشه.");
            SetLocationSourceText(false);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(remoteLocationsUrl))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool failed = request.result != UnityWebRequest.Result.Success;
#else
            bool failed = request.isNetworkError || request.isHttpError;
#endif
            if (failed)
            {
                Debug.LogWarning($"[Geospatial] دانلود مختصات از {remoteLocationsUrl} ناموفق بود ({request.error})؛ از مقادیر پیش‌فرض Inspector استفاده میشه.");
                SetLocationSourceText(false);
                yield break;
            }

            ApplyRemoteLocations(request.downloadHandler.text);
        }
    }

    private void ApplyRemoteLocations(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            Debug.LogWarning("[Geospatial] فایل مختصات ریموت خالی بود؛ از مقادیر پیش‌فرض Inspector استفاده میشه.");
            SetLocationSourceText(false);
            return;
        }

        var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 8)
        {
            Debug.LogWarning($"[Geospatial] فایل مختصات ریموت {lines.Length} خط داشت (انتظار ۸ خط)؛ از مقادیر پیش‌فرض Inspector استفاده میشه.");
            SetLocationSourceText(false);
            return;
        }

        var pairs = new List<(double lat, double lon)>(8);

        for (int i = 0; i < 8; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length != 2 ||
                !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) ||
                !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
            {
                Debug.LogWarning($"[Geospatial] خط {i + 1} فایل مختصات ریموت ('{lines[i]}') قابل پارس نبود؛ از مقادیر پیش‌فرض Inspector استفاده میشه.");
                SetLocationSourceText(false);
                return; // یا همه‌ی ۴ لوکیشن درست پارس میشن یا هیچ‌کدوم تغییر نمی‌کنن
            }

            pairs.Add((lat, lon));
        }

        int locationCount = Mathf.Min(locations.Count, 4);
        for (int i = 0; i < locationCount; i++)
        {
            var trigger = pairs[i * 2];
            var building = pairs[i * 2 + 1];

            locations[i].latitude = trigger.lat;
            locations[i].longitude = trigger.lon;
            locations[i].buildingLatitude = building.lat;
            locations[i].buildingLongitude = building.lon;
        }

        Debug.Log($"[Geospatial] مختصات {locationCount} لوکیشن با موفقیت از {remoteLocationsUrl} اعمال شد.");
        SetLocationSourceText(true);
    }

    private void SetLocationSourceText(bool isOnline)
    {
        if (locationSourceText != null)
            locationSourceText.text = isOnline ? "online loc" : "offline loc";
    }

    void Update()
    {
        if (!initializationComplete)
            return;

        if (earthManager == null || anchorManager == null)
            return;

        // خیلی از پیاده‌سازی‌های ARCore Extensions تا وقتی Session واقعاً
        // ساخته و آماده نشده، خوندن EarthTrackingState رو با
        // NullReferenceException جواب میدن (مخصوصاً چند ثانیه‌ی اول اجرا).
        // برای همین این خوندن رو محافظت‌شده انجام میدیم تا اسپم ارور
        // و رفتار نامشخص پیش نیاد.
        UnityEngine.XR.ARSubsystems.TrackingState trackingState;
        try
        {
            trackingState = earthManager.EarthTrackingState;
        }
        catch (Exception)
        {
            // Session هنوز آماده نیست؛ همین فریم رو رد کن و فریم بعد دوباره تلاش کن
            return;
        }

        // فقط وقتی VPS واقعا موقعیت رو ترک کرده باشه ادامه بده
        if (trackingState != UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
        {
            if (distanceText != null)
                distanceText.text = "Finding (VPS), Please turn on Location Services from your phone settings.";
            return;
        }

        var pose = earthManager.CameraGeospatialPose;
        double currentLat = pose.Latitude;
        double currentLon = pose.Longitude;

        // اگه در حال Resolve کردن نیستیم، قبل از هر تریگر جدید مطمئن شو
        // دقت افقی و عمودی هردو به حد لازم رسیدن؛ وگرنه فقط پیام دقت رو نشون بده.
        if (currentActiveIndex < 0)
        {
            double horizontalAccuracy = pose.HorizontalAccuracy;
            double verticalAccuracy = pose.VerticalAccuracy;
            bool accuracyGood = horizontalAccuracy < requiredAccuracyMeters && verticalAccuracy < requiredAccuracyMeters;

            if (!accuracyGood)
            {
                if (distanceText != null)
                {
                    distanceText.text =
                        $"Please wait, improving accuracy...\n" +
                        $"Horizontal accuracy: {horizontalAccuracy:F1} m (needs to be under {requiredAccuracyMeters:F0} m)\n" +
                        $"Vertical accuracy: {verticalAccuracy:F1} m (needs to be under {requiredAccuracyMeters:F0} m)";
                }
                return;
            }
        }

        UpdateDistancesUI(currentLat, currentLon);
        CheckTriggers(currentLat, currentLon);
    }

    /// <summary>
    /// فاصله بین دو مختصات جغرافیایی به متر (فرمول Haversine - دقت بالا برای فاصله‌های کوتاه)
    /// </summary>
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000.0; // شعاع زمین به متر
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private void UpdateDistancesUI(double lat, double lon)
    {
        if (distanceText == null) return;

        if (currentActiveIndex >= 0)
        {
            distanceText.text = anchorResolveInProgress
                ? "Look at the tower's location\n(Resolving anchor...)"
                : "Look at the tower's location";
            return;
        }

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < locations.Count; i++)
        {
            double d = HaversineDistance(lat, lon, locations[i].latitude, locations[i].longitude);
            sb.AppendLine($"{locations[i].label}: {d:F1} m");
        }

        // فاصله‌ی تریگر فعلی رو هم نشون بده تا موقع تست دستی راحت‌تر چک کنی
        sb.AppendLine($"Trigger Distance: {triggerDistanceMeters:F0} m");

        // اگه آخرین تلاش برای Resolve با خطا مواجه شده، همینجا مستقیم نشونش بده
        if (!string.IsNullOrEmpty(lastStatusMessage))
        {
            sb.AppendLine();
            sb.AppendLine(lastStatusMessage);
        }

        distanceText.text = sb.ToString();
    }

    private void CheckTriggers(double lat, double lon)
    {
        if (anchorResolveInProgress) return;

        int matchIndex = -1;
        for (int i = 0; i < locations.Count; i++)
        {
            double d = HaversineDistance(lat, lon, locations[i].latitude, locations[i].longitude);
            if (d <= triggerDistanceMeters)
            {
                matchIndex = i;
                break;
            }
        }

        // اگه هنوز توی همون لوکیشن قبلی هستیم، کاری نکن
        if (matchIndex == currentActiveIndex) return;

        // یا از محدوده خارج شده یا وارد لوکیشن جدید شده -> قبلی رو نابود کن
        ClearActivePrefab();

        if (matchIndex >= 0)
        {
            StartCoroutine(SpawnPrefabAt(locations[matchIndex], matchIndex));
        }
    }

    private void ClearActivePrefab()
    {
        if (currentActiveIndex < 0 || currentActiveIndex >= locations.Count)
        {
            currentActiveIndex = -1;
            return;
        }

        var loc = locations[currentActiveIndex];

        if (loc.spawnedInstance != null)
        {
            Destroy(loc.spawnedInstance);
            loc.spawnedInstance = null;
        }

        if (loc.spawnedAnchor != null)
        {
            Destroy(loc.spawnedAnchor.gameObject);
            loc.spawnedAnchor = null;
        }

        currentActiveIndex = -1;
    }

    private IEnumerator SpawnPrefabAt(GeoLocationPoint loc, int index)
    {
        if (loc.prefab == null)
        {
            Debug.LogWarning($"[Geospatial] پریفب برای {loc.label} ست نشده.");
            yield break;
        }

        anchorResolveInProgress = true;
        currentActiveIndex = index;
        lastStatusMessage = null; // پیام تلاش قبلی رو پاک کن، تلاش تازه شروع شده

        // مهم: اگه هر خطایی حین Resolve (چه NullReferenceException خود پلاگین
        // به‌خاطر نبود VPS، چه هر مشکل دیگه‌ای) رخ بده، finally تضمین می‌کنه
        // anchorResolveInProgress دوباره false بشه تا برنامه برای همیشه گیر نکنه.
        bool resolvedSuccessfully = false;
        bool pollingFailed = false;
        ResolveAnchorOnTerrainPromise promise = null;

        try
        {
            promise = anchorManager.ResolveAnchorOnTerrainAsync(
                loc.buildingLatitude,
                loc.buildingLongitude,
                altitudeOffset,
                Quaternion.identity);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Geospatial] خطا هنگام شروع Resolve برای {loc.label}: {e.Message}");
            lastStatusMessage = $"[ERROR] Failed to start resolve for {loc.label}: {e.GetType().Name}";
            pollingFailed = true;
        }

        // به‌جای "yield return promise" (که باعث میشه خودِ موتور Unity هر فریم
        // مستقیماً promise.keepWaiting رو چک کنه، بیرون از try/catch ما)،
        // اینجا دستی و مرحله‌به‌مرحله وضعیتش رو چک می‌کنیم تا اگه پلاگین ARCore
        // به‌خاطر نبود VPS یا هر مشکل دیگه‌ای Exception بده، همینجا بگیریمش.
        if (!pollingFailed && promise != null)
        {
            while (true)
            {
                bool stillPending;
                try
                {
                    stillPending = promise.State == PromiseState.Pending;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Geospatial] خطا هنگام بررسی وضعیت Resolve برای {loc.label}: {e.Message}");
                    lastStatusMessage = $"[ERROR] Resolve failed for {loc.label}\n({e.GetType().Name}: likely VPS unavailable here)";
                    pollingFailed = true;
                    break;
                }

                if (!stillPending) break;
                yield return null;
            }
        }

        try
        {
            if (pollingFailed || promise == null)
            {
                Debug.LogWarning($"[Geospatial] Resolve برای {loc.label} به‌خاطر خطای داخلی پلاگین (احتمالاً VPS در دسترس نیست) ناموفق بود.");
                if (string.IsNullOrEmpty(lastStatusMessage))
                    lastStatusMessage = $"[ERROR] Resolve failed for {loc.label} (VPS likely unavailable at this exact spot)";
            }
            else if (promise.State != PromiseState.Done)
            {
                Debug.LogWarning($"[Geospatial] Resolve لغو شد یا ناتمام ماند برای {loc.label}");
                lastStatusMessage = $"[ERROR] Resolve cancelled/incomplete for {loc.label}";
            }
            else
            {
                var result = promise.Result;

                if (result.TerrainAnchorState == TerrainAnchorState.Success && result.Anchor != null)
                {
                    loc.spawnedAnchor = result.Anchor;

                    loc.spawnedInstance = Instantiate(loc.prefab, result.Anchor.transform);
                    loc.spawnedInstance.transform.localPosition = Vector3.zero;
                    loc.spawnedInstance.transform.localRotation = Quaternion.identity;

                    Debug.Log($"[Geospatial] {loc.label} با موفقیت روی زمین قرار گرفت.");
                    lastStatusMessage = null;
                    resolvedSuccessfully = true;
                }
                else
                {
                    Debug.LogWarning($"[Geospatial] عدم موفقیت Resolve برای {loc.label}: {result.TerrainAnchorState}");
                    lastStatusMessage = $"[ERROR] Resolve state for {loc.label}: {result.TerrainAnchorState}";
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Geospatial] خطا هنگام خوندن نتیجه‌ی Resolve برای {loc.label}: {e.Message}");
            lastStatusMessage = $"[ERROR] Reading resolve result for {loc.label}\n({e.GetType().Name})";
        }
        finally
        {
            anchorResolveInProgress = false;

            if (!resolvedSuccessfully)
            {
                // اگه موفق نشد، اجازه بده کاربر بتونه دوباره با ورود/خروج از شعاع تلاش کنه
                currentActiveIndex = -1;
            }
        }
    }

    /// <summary>
    /// این متد رو به OnClick یه دکمه‌ی UI وصل کن؛ هر بار که بزنیش،
    /// یک متر به شعاع تریگر (triggerDistanceMeters) اضافه می‌کنه.
    /// برای تست دستی فاصله‌ی مورد نیاز مفیده.
    /// </summary>
    public void IncreaseTriggerDistance()
    {
        triggerDistanceMeters += 1f;
        Debug.Log($"[Geospatial] Trigger distance increased to {triggerDistanceMeters} m");
    }
}
