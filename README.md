# StudyEnglish_for_Using_NeuroSky
This project is a preliminary experiment to explore the possibility of using Neurosky for effective English learning.
NeuroSky_GetSignal — Unity EEG Acquisition Project
📡 Overview

This project provides a Unity-based real-time EEG acquisition and visualization system using the NeuroSky MindWave Mobile device.
It is designed for educational use, hackathons, and Brain–Computer Interface (BCI) prototyping.

The repository includes:

Real-time EEG signal acquisition (raw EEG, eSense, EEG power bands)

JSON parsing via ThinkGear protocol

Unity scripts for signal logging and visualization

Experimental session logger

Sample GUI for monitoring attention, meditation, blink, and raw waveforms

This project was developed during Tokyo Hackathon 2025.

🧠 Features
✔ 1. Real-time Signal Acquisition

Connects to MindWave mobile via ThinkGear Connector

Supports raw, blink, attention, meditation, and power spectrums

Stable JSON-based stream reader (enableRawOutput: true)

✔ 2. Unity Integration

Real-time visualization using LineRenderer

Prefabs and scripts under Assets/Scripts

Logging system under Assets/Logs/

✔ 3. Experiment Logging

Automatically records timestamped CSV logs

Useful for later analysis (Python, MATLAB, EEG pipelines)

✔ 4. Simple UI for monitoring EEG states

Attention/meditation bar

Raw signal plot

Blink detection gate

📂 Directory Structure
NeuroSky_GetSignal/
│
├── Assets/
│   ├── Scripts/
│   │   ├── MindwaveManager.cs
│   │   ├── MindwaveRawLineRenderer.cs
│   │   ├── MindwaveSessionLogger.cs
│   │   └── MindwaveBlinkGate.cs
│   │
│   ├── Scenes/
│   ├── Plugins/
│   └── Logs/
│
├── ProjectSettings/
└── Packages/


Unity 必須フォルダ（Assets / ProjectSettings / Packages）はすべて含まれています。

🔧 How to Run
1. Install ThinkGear Connector

Download from NeuroSky official website.

2. Start ThinkGear Connector

Wait until it shows:

"connected": true

3. Run the Unity scene

Open SampleScene and press ▶（Play）

Real-time EEG stream will appear in Unity.

💾 Logging

Logs are automatically saved at:

Assets/Logs/session_TIMESTAMP.csv


Contains:

raw wave

blink strength

attention

meditation

EEG band powers

🛠 Requirements

Unity 2021.3+

NeuroSky MindWave Mobile

ThinkGear Connector (TGC)

Windows 10/11
