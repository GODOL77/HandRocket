using UnityEngine;
using System.Collections;
using System;

public class RocketManager : MonoBehaviour
{
    [Header("Game Settings")]
    public Rigidbody rocketRb; // Inspector에 Rocket 오브젝트의 Rigidbody 연결
    public GameObject Rocket;  // Inspector에 Rocket 오브젝트 연결

    [Tooltip("로켓 파워(연료량)에 곱해지는 추진력 계수")]
    public float thrustMultiplier = 2000f; // ★ 로켓이 날아가도록 값을 크게 조정 (질량에 따라 조절 필요)

    [Header("Thruster State")]
    public bool isThrusting = false;

    [Header("Rocket State")]
    public float fuel = 0f;                   // 충전된 연료의 양
    public int rocketParts = 0;               // 조립된 로켓부품의 양
    public bool rocketLaunched = false;        // 로켓발사확인함수
    private bool countdownFinished = false;   // 30초 쿨다운 끝나면 발사

    [Header("Rocket HighHeight")]
    public float maxAltitude = 0.0f;

    public int rocketPower = 0;
    public float curTime = 0f;
    public bool hoseAttached = false;


    void Start()
    {
        // Inspector 연결 안전 체크 및 Rigidbody 자동 가져오기 시도
        if (Rocket != null && rocketRb == null)
        {
            rocketRb = Rocket.GetComponent<Rigidbody>();
        }
        
        // Rigidbody 연결 최종 확인
        if (rocketRb == null)
        {
            Debug.LogError("Rocket Rigidbody가 RocketManager에 연결되지 않았거나 Rocket 오브젝트에 Rigidbody 컴포넌트가 없습니다!");
        }
        else
        {
            // 초기에는 물리적으로 움직이지 않도록 설정
            rocketRb.isKinematic = true; 
        }

        // 초기 최고 고도 설정 (로켓이 지면에 있을 때의 Y값)
        if (Rocket != null)
        {
            maxAltitude = Rocket.transform.position.y - 3.2f; // 시작 기준 높이
        }
    }

    void Update()
    {
        // 1. 카운트다운 및 부품/연료 체크
        if (!countdownFinished)
        {
            CheckAssembly();
            CheckFuel();
            LaunchTimer();
        }
        
        // 2. 카운트다운이 끝났지만 아직 발사되지 않은 경우, 최종 파워 체크
        if (countdownFinished && !rocketLaunched)
        {
            CheckRocketPower(); // 이 함수는 LaunchRocket()을 호출하고 rocketLaunched = true로 설정함
        }

        // 3. 최고 고도 기록은 매 프레임 업데이트
        RecordMaxAltitude();
    }
    
    // 🌟 물리 연산은 FixedUpdate에서 처리해야 가장 정확하고 안정적입니다.
    void FixedUpdate()
    {
        // 로켓이 발사되었고, 추진 중일 때만 힘을 적용합니다.
        if (rocketLaunched && isThrusting)
        {
            ApplyThrust();
        }
    }

    private void LaunchTimer()
    {
        curTime += Time.deltaTime;
        if (curTime >= 30.0f)
        {
            countdownFinished = true;
            // 카운트다운이 끝났으니 LaunchTimer는 더 이상 할 일이 없습니다.
        }
    }

    void CheckAssembly() 
    {
        // 부품 조립 판정 하는 함수 적어야함.
        if (rocketParts >= 3)
        {
            // Debug.Log("부품 조립 완료"); // 잦은 로그 방지
        }
    }

    void CheckFuel() // 연료 확인 함수
    {
        if (rocketParts >= 3 && hoseAttached && fuel < 100f) // 최대 연료량 100f 가정
        {
            fuel += Time.deltaTime * 5f; // 연료 충전 속도 조절
        }
    }

    void CheckRocketPower()
    { 
        if (rocketParts >= 3 && fuel > 0f) 
        {
            rocketPower = (int)fuel;
            Debug.Log("✅ 제한시간 내 발사 준비 완료! 최종 파워: " + rocketPower);
            LaunchRocket();
        }
        else
        {
            // 발사 실패 로그는 한 번만 출력하도록 조건을 추가할 수 있습니다.
            Debug.LogWarning("❌ 제한시간 초과! 로켓 발사 실패 (Fuel: " + fuel.ToString("F1") + ", Parts: " + rocketParts + ")");
        }
    }

    void LaunchRocket()
    {
        if (rocketRb == null) return;
        
        rocketLaunched = true;
        rocketRb.isKinematic = false;   // 로켓에 물리 힘이 적용되도록 설정
        isThrusting = true;             // FixedUpdate에서 추진력이 적용되기 시작
    }

    private void ApplyThrust()
    {
        if (fuel > 0 && rocketRb != null)
        {
            // 현재 연료량과 추진 계수를 곱하여 힘 계산
            // fuel이 줄어들면 추진력도 같이 줄어듭니다.
            float currentThrustForce = thrustMultiplier * fuel; 
            
            // FixedUpdate에서 ForceMode.Force 사용
            rocketRb.AddForce(Vector3.up * currentThrustForce, ForceMode.Force); 
            
            // 연료 소모율은 FixedUpdate의 시간 간격(Time.fixedDeltaTime)에 맞춰 소모
            fuel -= Time.fixedDeltaTime * 5.0f; 
        }
        else
        {
            fuel = 0;
            isThrusting = false;
            Debug.Log("🔥 연료 소진! 추진 중단, 자유 낙하 시작.");
        }
    }

    void RecordMaxAltitude()
    {
        if (Rocket == null) return;
        
        float currentY = Rocket.transform.position.y;
        
        if (currentY > maxAltitude)
        {
            maxAltitude = currentY;
            // Debug.Log($"새로운 최고 고도 기록: {maxAltitude:F2}m");
        }
    }
}