using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationHotkeys : MonoBehaviour
{
    public MindwaveSessionLogger logger;
    public MindwaveBlinkGate gate;   // ★追加

    int trial = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // REST
        {
            trial++;
            logger.Marker("CALIB_START", trial, "REST", "REST", "start");
            gate?.CalibStart(trial, "REST");   // ★解析開始
        }

        if (Input.GetKeyDown(KeyCode.V)) // TASK
        {
            trial++;
            logger.Marker("CALIB_START", trial, "TASK", "TASK", "start");
            gate?.CalibStart(trial, "TASK");   // ★解析開始
        }

        if (Input.GetKeyDown(KeyCode.B)) // END
        {
            logger.Marker("CALIB_END", trial, "END", "END", "end");
            gate?.CalibEnd(trial);              // ★解析終了
        }
    }
}

