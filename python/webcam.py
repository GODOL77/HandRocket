import cv2
from cvzone.HandTrackingModule import HandDetector
import socket
import functionLib

# Parameters
width, height = 1280, 720
windowName = "Hand Detector"
serverAddress = "127.0.0.1"
serverPort = 5052
maxHands = 2
debug = False

# Load Webcam
cap = cv2.VideoCapture(0)
cap.set(3, width)
cap.set(4, height)

# Hand Detector
detector = HandDetector(maxHands = maxHands, detectionCon = 0.8)

# Communication
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = (serverAddress, serverPort)
def one_hand_detection():
    while True:
        # Get the frame from the webcam
        success, img = cap.read()
        if not success:
            break
    
        # Hands
        hands, img = detector.findHands(img)
    
        data = []
    
        if hands:
            # Get the first hand detected
            hand = hands[0]
    
            # Get the landmark list
            lmlist = hand['lmList']

            for lm in lmlist:
                data.extend([lm[0], height - lm[1], lm[2]])
    
            sock.sendto(str.encode(str(data)), serverAddressPort)
    
        img = cv2.resize(img, (0, 0), None, 0.5, 0.5)
        cv2.imshow(windowName, img)
        cv2.waitKey(1)
    
    cap.release()
    cv2.destroyAllWindows()
    
def two_hand_detection():
    while True:
        # Get the frame from the webcam
        success, img = cap.read()
        if not success:
            break
    
        # Hands
        hands, img = detector.findHands(img)
    
        if hands:
            for hand in hands:
                lmlist = hand['lmList']
    
                # 손가락 상태 계산
                fingers = functionLib.get_finger_states(lmlist)
                state = functionLib.get_hand_state(fingers)
    
                # 데이터 패킷 구성
                data = []
                for lm in lmlist:
                    data.extend([lm[0], height - lm[1], lm[2]])

                # str 전송
                data_str = f"{state}|" + str(data)
                sock.sendto(data_str.encode('utf-8'), serverAddressPort)
       
        # 미리보기
        img = cv2.resize(img, (0, 0), None, 0.5, 0.5)
        cv2.imshow(windowName, img)
        cv2.waitKey(1)
    
    cap.release()
    cv2.destroyAllWindows()
    
    # todo : '두 손'을 감지해 데이터 전송

if __name__ == '__main__':
    if maxHands == 1:
        one_hand_detection()
    else:
        two_hand_detection()