# NeuroSky_GetSignal — Unity EEG Acquisition Project

## 📡 Overview
This project provides a **Unity-based real-time EEG acquisition and visualization system** using the **NeuroSky MindWave Mobile** device.

this tool is ideal for:
- BCI prototyping  
- Real-time EEG monitoring  
- Educational demos  
- Research experiments  

---

## 🧠 Features

### ✔ Real-time EEG Signal Acquisition
- Raw EEG  
- Blink detection  
- Attention / Meditation (eSense)  
- EEG Power Bands  

Uses ThinkGear JSON stream:

### ✔ Unity Integration
- Real-time waveform plot (`LineRenderer`)
- Prefabs for raw data and UI components
- Stable, low-latency connection

### ✔ Experiment Logging
- Auto-saves logs into `Assets/Logs/`
- CSV format (timestamped)
- Easy to analyze with Python/MATLAB

---

## 📂 Directory Structure

'''
NeuroSky_GetSignal/
│
├── Assets/
│ ├── Scripts/
│ │ ├── MindwaveManager.cs
│ │ ├── MindwaveRawLineRenderer.cs
│ │ ├── MindwaveSessionLogger.cs
│ │ └── MindwaveBlinkGate.cs
│ ├── Scenes/
│ ├── Plugins/
│ └── Logs/
│
├── ProjectSettings/
└── Packages/
'''

---

## 🔧 How to Run

### 1. Start ThinkGear Connector
Install TGC from NeuroSky and run it.

### 2. Pair the MindWave Mobile
Connect via Bluetooth.

### 3. Run Unity
Open `SampleScene` → Press **Play**

You will see:
- Raw EEG waveform  
- eSense values  
- Blink detection  
- Automatic logging  

---

## 💾 Log Format

Example CSV columns:
timestamp, rawEEG, blinkStrength, attention, meditation, delta, theta, alpha, beta, gamma


Logs are stored as:

---

## 🛠 Requirements
- Unity 2022.3.58f1
- Windows 10/11
- NeuroSky MindWave Mobile
- ThinkGear Connector

---

## 🙋‍♂️ Author
**YK (Yusuke Kutsukake)**  
Master’s student, SIT  
Research: EEG-based BCI, Covert Speech Decoding, Source Imaging

---
