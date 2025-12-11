using UnityEngine;
using System.Collections;

public class RocketManager : MonoBehaviour
{

    [Header("Game Settings")]
    //public float countdownTime = 20f;
    public Rigidbody rocketRb;
    public GameObject Rocket;

    [Tooltip("로켓 파워(연료량)에 곱해지는 추진력 계수")]
    public float thrustMultiplier = 10f;

    [Header("Thruster State")]
    public bool isThrusting = false;

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

        if (rocketLaunched && isThrusting)
        {
            ApplyThrust();
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
        isThrusting = true;
        // rocketRb.AddForce(Vector3.up * rocketPower * powerMultiplier, ForceMode.Impulse);
    }

    private void ApplyThrust()
    {
        if (fuel > 0)
        {
            // [핵심 수정 1] 현재 연료량(fuel)과 추진 계수를 곱하여 실제 힘(Force) 계산
            // fuel이 줄어들면 추진력도 같이 줄어들어 현실적인 소모 표현이 가능합니다.
            float currentThrustForce = thrustMultiplier * fuel; 
            
            // [핵심 수정 2] ForceMode.Force 사용 (질량의 영향을 받음)
            // Time.deltaTime을 곱하지 않습니다. ForceMode.Force는 FixedUpdate에서 쓰는 것이 일반적이지만, 
            // Update에서 쓰면 Unity가 자동으로 deltaTime을 계산하여 적용해 줍니다.
            rocketRb.AddForce(Vector3.up * currentThrustForce, ForceMode.Force); 
            
            // 연료 소모율
            fuel -= Time.deltaTime * 1.5f; 
        }
        else
        {
            fuel = 0;
            isThrusting = false;
            Debug.Log("🔥 연료 소진! 추진 중단, 자유 낙하 시작.");
        }
    }
}
