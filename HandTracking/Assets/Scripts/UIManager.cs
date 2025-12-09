using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RocketManager RM;
    public Image launchBG;
    public Text  fuelText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        launchBG.gameObject.SetActive(false);
        fuelText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (RM.rocketLaunched)
        {
            fuelText.text = "Fuel: " + RM.fuel.ToString("F1");
            launchBG.gameObject.SetActive(true);
            fuelText.gameObject.SetActive(true);

        }
    }
}
