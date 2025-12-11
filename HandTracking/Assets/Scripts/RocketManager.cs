using UnityEngine;
using System.Collections;

public class RocketManager : MonoBehaviour
{

    [Header("Game Settings")]
    //public float countdownTime = 20f;
    public Rigidbody rocketRb;
    public GameObject Rocket;
    public float powerMultiplier = 100f;

    [Header("Rocket State")]
    public float fuel = 0f;                 // 충전된 연료의 양
    public int rocketParts = 0;             // 조립된 로켓부품의 양
    public bool rocketLaunched = false;     // 로켓발사확인함수
    private bool countdownFinished = false; // 20초 쿨다운 끝나면 발사

    public int rocketPower = 0;

    public float curTime = 0f;

    public bool hoseAttached = false;


    void Start()
    {
        
    }

    void Update()
    {
        
        if (!countdownFinished) // 카운트다운이 끝날 때까지 작동
        {
            CheckAssembly();
            CheckFuel();
            LaunchTimer();
        }

        if (countdownFinished && !rocketLaunched)   // 카운트다운 끝 & 로켓 발사 안되었을 시(완성되었는지 확인하는 if문 필요함)
        {
            CheckRocketPower();
        }

        // if (rocketLaunched == true)
        // {
        //     LaunchRocket();
        // }
    }

    private void LaunchTimer()   // 코루틴으로 초세는 함수
    {
        curTime += Time.deltaTime;
        if (curTime >= 30.0f)
        {
            countdownFinished = true;

        }
        
    }

    void CheckAssembly()    // 부품 조립 되었는지 확인하는 함수
    {
        // 부품 조립 판정 하는 함수 적어야함.
        if (rocketParts >= 3)
        {
            Debug.Log("부품 조립 완료");
        }
    }

    void CheckFuel()        // 연료 확인 함수
    {
        if (rocketParts == 3 && hoseAttached)
        {
            fuel += Time.deltaTime;
        }
    }

    void CheckRocketPower()
    { 
        
        if (rocketParts == 3 && fuel > 0f) 
        {
            // if (fuel <= 10)
            // {
            //     rocketPower = 1;
            // }
            // else if (fuel <= 20)
            // {
            //     rocketPower = 2;
            // }
            // else
            // {
            //     rocketPower = 3;
            // }
            rocketPower = (int)fuel;
            Debug.Log("✅ 제한시간 내 발사 준비 완료! 최종 파워: " + rocketPower);
            LaunchRocket();
        }
        else
        {
            Debug.LogWarning("❌ 제한시간 초과! 로켓 발사 실패 (Fuel: " + fuel + ", Parts: " + rocketParts + ")");
        }
    }

    void LaunchRocket()
    {
        rocketLaunched = true;

        rocketRb.isKinematic = false;   // 물리 힘 받도록 설정
        rocketRb.AddForce(Vector3.up * rocketPower * powerMultiplier, ForceMode.Impulse);
        // Rocket.transform.position += new Vector3(0, rocketPower * 0.1f, 0);
    }
}
