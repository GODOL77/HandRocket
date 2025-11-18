import math

# 각도 계산
def angle_between(p1, p2, p3):
    a = [p1[0]-p2[0], p1[1]-p2[1], p1[2]-p2[2]]
    b = [p3[0]-p2[0], p3[1]-p2[1], p3[2]-p2[2]]
    dot = a[0]*b[0] + a[1]*b[1] + a[2]*b[2]
    mag_a = math.sqrt(sum(i**2 for i in a))
    mag_b = math.sqrt(sum(i**2 for i in b))
    cos_theta = dot / (mag_a * mag_b + 1e-6)
    angle = math.degrees(math.acos(max(min(cos_theta, 1), -1)))
    return angle

# 손가락 상태 계산
def get_finger_states(lmlist):
    finger_states = []
    finger_joints = [[1,2,3,4],[5,6,7,8],[9,10,11,12],[13,14,15,16],[17,18,19,20]]
    for joints in finger_joints:
        a1 = angle_between(lmlist[joints[0]], lmlist[joints[1]], lmlist[joints[2]])
        a2 = angle_between(lmlist[joints[1]], lmlist[joints[2]], lmlist[joints[3]])
        avg_angle = (a1+a2)/2
        finger_states.append(1 if avg_angle>160 else 0)
    return finger_states

# 손 상태 분류
def get_hand_state(fingers):
    if fingers == [0,0,0,0,0]:
        return "FIST"
    elif fingers == [1,1,1,1,1]:
        return "OPEN"
    elif fingers[1] == 1 and fingers[2] == 1:
        return "V"
    elif fingers == [0,0,1,0,0]:
        return "MIDDLE"
    else:
        return "UNKNOWN"

# exe 테스트용 main
if __name__ == "__main__":
    print("functionLib exe 테스트")
