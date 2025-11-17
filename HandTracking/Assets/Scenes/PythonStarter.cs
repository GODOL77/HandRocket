using UnityEngine;
using System.Diagnostics;
using System;


// 현재 포트 점유 충돌 문제로 인해 python 스크립트 실행 순서를 정해야 함
// webcam.py에서 socket.bind 삭제 혹은 유니티 프로젝트 세팅에서 ScriptExecutionOrder 설정이 필요함


public class PythonStarter : MonoBehaviour
{
    private Process pythonProcess;

    void Start()
    {
        RunPythonScript();
    }


    void RunPythonScript()
    {

        try
        {


            string pythonScriptPath = @"D:\HandRocket\HandTracking\Assets\python\webcam.py";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "python";
            startInfo.Arguments = pythonScriptPath;

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;

            //프로세스 시작
            pythonProcess = new Process();
            pythonProcess.StartInfo = startInfo;

            //유니티가 멈추지않고 출력 받기
            pythonProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    UnityEngine.Debug.Log("Python Output: " + args.Data);
                }
            };
            pythonProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    UnityEngine.Debug.LogError("Python Error: " + args.Data);
                }
            };

            pythonProcess.Start();

            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            UnityEngine.Debug.Log("Python script started successfully.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Exception while starting Python script: " + e.Message);
            UnityEngine.Debug.LogWarning("PC에 python.exe가 설치되어 있고, 시스템 환경 변수(PATH)에 등록되어 있는지 확인하세요.");
        }


    }

    // Update is called once per frame
    void Update()
{

}
    
    void OnApplicationQuit()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            UnityEngine.Debug.Log("Killing python process...");
            pythonProcess.Kill(); // 프로세스 강제 종료
            pythonProcess = null;
        }
    }
}
