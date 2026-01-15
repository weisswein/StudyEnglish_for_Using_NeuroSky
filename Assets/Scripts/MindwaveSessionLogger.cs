using System;
using System.IO;
using UnityEngine;

public class MindwaveSessionLogger : MonoBehaviour
{
    [SerializeField] private MindwaveController controller;
    [SerializeField] private string fileName = "session.csv";

    private StreamWriter sw;

    private void Start()
    {
        var dir = Path.Combine(Application.dataPath, "Logs");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, fileName);
        sw = new StreamWriter(path, false) { AutoFlush = true };

        sw.WriteLine("t,utc,type,trial,condition,stim_id,poorSignal,status,attention,meditation,delta,theta,lowAlpha,highAlpha,lowBeta,highBeta,lowGamma,note");
        Debug.Log($"[SessionCSV] {path}");
    }

    private void OnEnable()
    {
        if (controller != null)
            controller.OnUpdateMindwaveData += OnData;
    }

    private void OnDisable()
    {
        if (controller != null)
            controller.OnUpdateMindwaveData -= OnData;
        sw?.Close();
        sw = null;
    }

    private void OnData(MindwaveDataModel m)
    {
        float t = Time.realtimeSinceStartup;
        string utc = DateTime.UtcNow.ToString("o");

        // eSense / eegPower のフィールド名は各モデル定義に依存（たぶん小文字）
        // まずは “存在する前提” で書く。コンパイルエラーが出たら、そのモデル定義に合わせて名前だけ直す。
        int att = m.eSense.attention;
        int med = m.eSense.meditation;

        var p = m.eegPower;
        sw.WriteLine($"{t},{utc},STATE,,,,{m.poorSignalLevel},{m.status},{att},{med},{p.delta},{p.theta},{p.lowAlpha},{p.highAlpha},{p.lowBeta},{p.highBeta},{p.lowGamma},");
    }

    public void Marker(string markerType, int trial, string condition, string stimId, string note = "")
    {
        float t = Time.realtimeSinceStartup;
        string utc = DateTime.UtcNow.ToString("o");
        sw.WriteLine($"{t},{utc},MARKER,{trial},{condition},{stimId},,,,{markerType},,,,,,,,\"{note}\"");
    }
}
