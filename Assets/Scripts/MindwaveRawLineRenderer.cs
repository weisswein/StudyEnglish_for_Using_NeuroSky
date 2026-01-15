using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MindwaveRawLineRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MindwaveController controller;
    [SerializeField] private LineRenderer line;

    [Header("Display")]
    [SerializeField] private float windowSeconds = 5f;     // 表示窓（秒）
    [SerializeField] private int assumedSfreq = 512;       // Rawのサンプルレート想定
    [SerializeField] private int decimate = 4;             // 4なら512→128点相当で描画
    [SerializeField] private float xScale = 1f;            // 横方向のスケール
    [SerializeField] private float yScale = 0.001f;        // 縦方向のスケール（Raw値に合わせて調整）
    [SerializeField] private bool autoYScale = false;      // 後で必要ならON
    [SerializeField] private float autoYTarget = 1.0f;     // auto時の目標振幅

    [Header("Quality Gate (optional)")]
    [SerializeField] private bool grayWhenNoSignal = true;
    [SerializeField] private MindwaveController qualitySource; // 同じcontrollerでもOK

    private readonly Queue<int> raw = new Queue<int>();
    private int capacitySamples;
    private int frameCounter = 0;
    private int latestPoorSignal = 200;

    private void Reset()
    {
        line = GetComponent<LineRenderer>();
    }

    private void Awake()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        capacitySamples = Mathf.Max(1, Mathf.RoundToInt(windowSeconds * assumedSfreq));
    }

    private void OnEnable()
    {
        if (controller != null) controller.OnUpdateRawEEG += OnRaw;

        // poorSignalを取れるなら取る（MindwaveDataModelの構造に依存）
        if (qualitySource != null)
            qualitySource.OnUpdateMindwaveData += OnMindwaveData;
    }

    private void OnDisable()
    {
        if (controller != null) controller.OnUpdateRawEEG -= OnRaw;
        if (qualitySource != null)
            qualitySource.OnUpdateMindwaveData -= OnMindwaveData;
    }

    private void OnRaw(int v)
    {
        raw.Enqueue(v);
        while (raw.Count > capacitySamples)
            raw.Dequeue();
    }

    private void OnMindwaveData(MindwaveDataModel m)
    {
        latestPoorSignal = m.poorSignalLevel;
    }

    private void Update()
    {
        // 60fpsで毎フレーム描画してもOKだが、軽くしたいなら2フレに1回など
        frameCounter++;
        if (frameCounter % 1 != 0) return;

        Draw();
    }

    private void Draw()
    {
        if (raw.Count < decimate + 1) return;

        // decimate間引きで点列を作る
        int n = raw.Count / decimate;
        if (n < 2) return;

        // autoYScale（任意）
        float yS = yScale;
        if (autoYScale)
        {
            // 簡易：直近のmax-minからスケールを調整
            int min = int.MaxValue, max = int.MinValue;
            foreach (var v in raw)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }
            float range = Mathf.Max(1f, max - min);
            yS = autoYTarget / range;
        }

        line.positionCount = n;

        // Queueを配列化（軽量化したければ自前リングバッファにするとさらに良い）
        int[] arr = raw.ToArray();

        for (int i = 0; i < n; i++)
        {
            int idx = i * decimate;
            float x = (i / (float)(n - 1)) * xScale;
            float y = arr[idx] * yS;
            line.SetPosition(i, new Vector3(x, y, 0f));
        }

        // 品質が悪いときは視覚的に落とす（マテリアル色はプロジェクト次第で調整）
        if (grayWhenNoSignal && latestPoorSignal >= 100)
        {
            line.enabled = true; // 消すなら false
            // 色を変えたい場合は LineRendererのstartColor/endColorをここで変更（任意）
        }
    }
}
