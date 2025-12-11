using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RocketManager RM;
    public Image launchBG;
    public Text fuelText;
    public Text leaveTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // launchBG.gameObject.SetActive(false);
        // fuelText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        fuelText.text = "Fuel: " + RM.fuel.ToString("F1");
        float remainingTime = 30f - RM.curTime;
        // 남은 시간이 음수가 되는 것을 방지 (선택적)
        if (remainingTime < 0) remainingTime = 0f;
        leaveTime.text = "LeaveTime " + remainingTime.ToString("F1");
        // if (RM.rocketLaunched)
        // {
        //     launchBG.gameObject.SetActive(true);
        //     fuelText.gameObject.SetActive(true);
        // }
    }
}
