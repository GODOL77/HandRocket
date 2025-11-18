using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonStarter : MonoBehaviour
{
    private Process webcamProcess;
    private Process functionProcess;

    void Start()
    {
        RunExe("webcam.exe", ref webcamProcess);
        RunExe("functionLib.exe", ref functionProcess);
    }

    void RunExe(string exeName, ref Process process)
    {
        // exe가 들어있는 경로
        string exePath = Path.Combine(Application.streamingAssetsPath, "python", exeName);

        if (!File.Exists(exePath))
        {
            UnityEngine.Debug.LogError("Python exe 파일을 찾을 수 없습니다: " + exePath);
            return;
        }

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = exePath;
        psi.WorkingDirectory = Path.GetDirectoryName(exePath);
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.CreateNoWindow = true;

        process = new Process();
        process.StartInfo = psi;

        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.Log("[Python OUT] " + args.Data);
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.LogError("[Python ERR] " + args.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        UnityEngine.Debug.Log("실행됨: " + exeName);
    }

    void OnApplicationQuit()
    {
        TryKill(webcamProcess);
        TryKill(functionProcess);
    }

    void TryKill(Process p)
    {
        try
        {
            if (p != null && !p.HasExited)
            {
                p.Kill();
                UnityEngine.Debug.Log("Python exe 종료됨");
            }
        }
        catch { }
    }
}
