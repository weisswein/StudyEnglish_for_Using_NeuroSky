using System.Collections.Generic;
using UnityEngine;

public class MindwaveBlinkGate : MonoBehaviour
{
    [Header("Refs")]
    public MindwaveController controller;
    public MindwaveSessionLogger logger;

    [Header("Blink detection")]
    public int blinkThreshold = 20;
    public float postMaskSec = 0.30f;

    [Header("Theta window (2s window, 1s hop)")]
    public float windowSec = 2.0f;
    public float hopSec = 1.0f;

    // ===== Blink mask runtime =====
    public bool isMasked;
    public float maskUntilTime;
    public float lastBlinkRatio;

    // ===== Segment state (robust) =====
    private bool segmentOpen = false;
    private int segmentId = 0;
    private int currentTrial = 0;
    private string currentType = "";     // "REST" or "TASK"
    private float segmentStartT = 0f;
    private float nextComputeT = 0f;

    // ===== REST baseline (latest REST) =====
    private bool restReady = false;
    private float restThetaMean = 0f;

    // ===== Buffers =====
    private readonly List<Sample> samples = new();        // raw theta samples in segment (for windowing)
    private readonly List<float> restThetaWins = new();   // REST: collect theta_win to compute baseline

    private struct Sample
    {
        public float t;
        public float theta;
        public bool valid;
        public Sample(float t, float theta, bool valid) { this.t = t; this.theta = theta; this.valid = valid; }
    }

    void OnEnable()
    {
        if (controller != null)
        {
            controller.OnUpdateBlink += OnBlink;
            controller.OnUpdateMindwaveData += OnData;
        }
    }

    void OnDisable()
    {
        if (controller != null)
        {
            controller.OnUpdateBlink -= OnBlink;
            controller.OnUpdateMindwaveData -= OnData;
        }
    }

    void Update()
    {
        isMasked = (Time.time < maskUntilTime);
    }

    // ===== Blink event =====
    void OnBlink(int strength)
    {
        lastBlinkRatio = Mathf.Clamp01(strength / (float)MindwaveHelper.BLINK_MAX);

        if (strength >= blinkThreshold)
        {
            maskUntilTime = Mathf.Max(maskUntilTime, Time.time + postMaskSec);

            // ログ（セグメント外でも残したければ segmentId=-1 等でもOKだが、今は開いてる時だけ）
            if (segmentOpen)
            {
                logger?.Marker("BLINK", currentTrial, currentType, "",
                    $"seg={segmentId}, strength={strength}");
            }
        }
    }

    // ===== EEG data =====
    void OnData(MindwaveDataModel m)
    {
        if (!segmentOpen) return;

        float t = Time.realtimeSinceStartup;

        bool valid = !isMasked;
        float theta = m.eegPower.theta;

        samples.Add(new Sample(t, theta, valid));

        // keep small buffer
        float keepFrom = t - (windowSec + 1.0f);
        samples.RemoveAll(s => s.t < keepFrom);

        // 1秒ごとに計算
        if (t >= nextComputeT)
        {
            nextComputeT += hopSec;
            ComputeAndLog(nowT: t);
        }
    }

    void ComputeAndLog(float nowT)
    {
        float from = nowT - windowSec;

        double sum = 0;
        int n = 0;

        foreach (var s in samples)
        {
            if (!s.valid) continue;
            if (s.t < from) continue;
            sum += s.theta;
            n++;
        }

        if (n == 0)
        {
            logger?.Marker("THETA_SKIP", currentTrial, currentType, "",
                $"seg={segmentId}, no valid samples (blink masked)");
            return;
        }

        float thetaWin = (float)(sum / n);

        if (currentType == "REST")
        {
            // RESTは窓値を溜めて、あとでbaseline確定
            restThetaWins.Add(thetaWin);
            logger?.Marker("REST_THETA", currentTrial, "REST", "",
                $"seg={segmentId}, theta_win={thetaWin:F3}, n={n}");
            return;
        }

        if (currentType == "TASK")
        {
            if (!restReady)
            {
                logger?.Marker("DTHETA_WAIT_REST", currentTrial, "TASK", "",
                    $"seg={segmentId}, theta_win={thetaWin:F3}, n={n} (REST baseline not ready)");
                return;
            }

            float dTheta = thetaWin - restThetaMean; // Task − Rest
            logger?.Marker("DTHETA", currentTrial, "TASK", "",
                $"seg={segmentId}, dtheta={dTheta:F3}, theta_win={thetaWin:F3}, rest_mean={restThetaMean:F3}, n={n}");
        }
    }

    // ===== Public controls (called from hotkeys) =====
    public void CalibStart(int trial, string type) // type: "REST" or "TASK"
    {
        // もし前が開いてたら壊れないように強制終了
        if (segmentOpen)
        {
            AutoEnd(reason: "CALIB_START received while segment open");
        }

        segmentOpen = true;
        segmentId++;
        currentTrial = trial;
        currentType = type;
        segmentStartT = Time.realtimeSinceStartup;

        samples.Clear();
        restThetaWins.Clear();
        nextComputeT = Time.realtimeSinceStartup + hopSec;

        logger?.Marker("SEG_START", currentTrial, currentType, "",
            $"seg={segmentId}, type={currentType}");
    }

    public void CalibEnd(int trial)
    {
        if (!segmentOpen)
        {
            logger?.Marker("SEG_END_IGNORED", trial, "END", "", "no open segment");
            return;
        }

        // セグメント終了ログ
        logger?.Marker("SEG_END", currentTrial, currentType, "",
            $"seg={segmentId}, dur={(Time.realtimeSinceStartup - segmentStartT):F2}s");

        // RESTならbaseline更新（直近REST採用）
        if (currentType == "REST")
        {
            if (restThetaWins.Count == 0)
            {
                restReady = false;
                logger?.Marker("REST_BASELINE_FAIL", currentTrial, "REST", "",
                    $"seg={segmentId}, no valid REST windows (too much blink?)");
            }
            else
            {
                double s = 0;
                foreach (var v in restThetaWins) s += v;
                restThetaMean = (float)(s / restThetaWins.Count);
                restReady = true;

                logger?.Marker("REST_BASELINE", currentTrial, "REST", "",
                    $"seg={segmentId}, rest_mean={restThetaMean:F3}, windows={restThetaWins.Count}");
            }
        }

        // close
        segmentOpen = false;
        currentType = "";
        samples.Clear();
        restThetaWins.Clear();
    }

    private void AutoEnd(string reason)
    {
        logger?.Marker("AUTO_END", currentTrial, currentType, "",
            $"seg={segmentId}, reason={reason}");

        // RESTならbaseline更新はしてもいいが、今回は「手動Bで閉じたRESTのみ採用」にするならコメントアウト
        // 今は壊れない優先で「AUTO_ENDでもREST baseline更新」しておく
        if (currentType == "REST")
        {
            if (restThetaWins.Count > 0)
            {
                double s = 0;
                foreach (var v in restThetaWins) s += v;
                restThetaMean = (float)(s / restThetaWins.Count);
                restReady = true;

                logger?.Marker("REST_BASELINE", currentTrial, "REST", "",
                    $"seg={segmentId}, rest_mean={restThetaMean:F3}, windows={restThetaWins.Count} (AUTO_END)");
            }
            else
            {
                restReady = false;
                logger?.Marker("REST_BASELINE_FAIL", currentTrial, "REST", "",
                    $"seg={segmentId}, no valid REST windows (AUTO_END)");
            }
        }

        segmentOpen = false;
        currentType = "";
        samples.Clear();
        restThetaWins.Clear();
    }
}
