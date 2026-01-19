# NeuroBitMonitor 🧠

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Platform](https://img.shields.io/badge/Platform-Windows%20(WPF)-blue)](https://dotnet.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-6.0%2B-purple)](https://dotnet.microsoft.com/)

**NeuroBitMonitor** is a real-time Brain-Computer Interface (BCI) dashboard built with **C# WPF**. It connects to NeuroSky MindWave headsets via the ThinkGear Connector, allowing you to visualize raw EEG waves, monitor attention states, and record data for scientific analysis.

![Application Screenshot](assets/screenshot.png)
*(Note: Add a screenshot of your app here)*

## ✨ Features

*   **Real-Time Oscilloscope:** Visualizes Raw EEG signal (512Hz) with auto-scaling.
*   **eSense™ Meters:** Displays proprietary **Attention** (Focus) and **Meditation** (Relaxation) levels (0-100).
*   **Spectral Power Bands:** Break down of brainwave frequencies:
    *   Delta, Theta
    *   Alpha (Low/High)
    *   Beta (Low/High)
    *   Gamma (Low/High)
*   **Blink Detection:** Detects blink strength for potential trigger controls.
*   **Data Logging:** Records Raw EEG data and timestamps to CSV files for post-processing (Python/MATLAB).
*   **Multi-Threaded:** Handles high-speed data parsing on background threads to keep the UI responsive.

## 🛠️ Prerequisites

To run this application, you need:

1.  **Hardware:** NeuroSky MindWave Mobile (or Mobile 2) Headset.
2.  **Middleware:** [ThinkGear Connector (TGC)](http://developer.neurosky.com/docs/doku.php?id=thinkgear_connector_tgc) installed and running in the background.
3.  **OS:** Windows 10 or 11.
4.  **.NET Runtime:** .NET 6.0 or higher.

## 🚀 Getting Started

### Installation

1.  Clone the repository:
    ```bash
    git clone https://github.com/YOUR_USERNAME/NeuroBitMonitor.git
    ```
2.  Open the solution file `NeuroBitMonitor.sln` in **Visual Studio 2022**.
3.  Restore NuGet packages (if any) and build the solution (`Ctrl + Shift + B`).

### Running the App

1.  Ensure your Bluetooth is on and your **MindWave headset is paired**.
2.  Start the **ThinkGear Connector** app (you should see the tray icon).
3.  Run **NeuroBitMonitor**.
4.  Click the **Connect** button.
    *   *Status will change to: "Connected to Headset"*

## 📊 Data Logging

When you click **"Toggle Logging"**, the application begins writing data to a CSV file located in your **My Documents** folder.

**File Format:** `RawEEG_YYYYMMDD_HHMMSS.csv`

| RawValue | Timestamp |
| :--- | :--- |
| 243 | 2023-10-25 14:30:01.123 |
| 256 | 2023-10-25 14:30:01.125 |
| 198 | 2023-10-25 14:30:01.127 |

> **Note:** The log captures the raw signal (approx 512 samples per second).

## 🏗️ Architecture

The application follows the **MVVM (Model-View-ViewModel)** design pattern:

*   **Models (`MindWavePacket.cs`):** Defines the JSON structure for parsing data (Raw, eSense, Power bands).
*   **Services (`ThinkGearService.cs`):** Manages the TCP Client connection (`127.0.0.1:13854`) to the ThinkGear Connector. It reads the JSON stream and fires events.
*   **ViewModel (`MainViewModel.cs`):** Handles UI logic, data binding, CSV writing, and graph plotting.
*   **View (`MainWindow.xaml`):** The XAML user interface.

## 📦 Dependencies

*   **Newtonsoft.Json:** Used for deserializing the JSON data packets sent by the ThinkGear Connector.
*   **System.IO.Ports:** (Indirectly used via TCP/Stream logic).

## 🤝 Contributing

Contributions are welcome!
1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---

**Disclaimer:** This software is for educational and research purposes only. It is not a medical device.
