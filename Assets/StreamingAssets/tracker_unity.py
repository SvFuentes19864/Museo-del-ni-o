import os
import sys
import math
import json
import socket
import traceback

os.environ["GLOG_minloglevel"] = "3"
os.environ["TF_CPP_MIN_LOG_LEVEL"] = "3"

import cv2
import mediapipe as mp
import numpy as np

# Log en archivo junto al ejecutable (para depurar cuando corre sin consola)
LOG_PATH = os.path.join(os.path.dirname(sys.executable if getattr(sys, 'frozen', False) else __file__), "tracker_log.txt")

def log(msg):
    try:
        with open(LOG_PATH, "a", encoding="utf-8") as f:
            f.write(msg + "\n")
    except Exception:
        pass

try:
    from pyorbbecsdk import Pipeline, Config, OBSensorType
    USAR_FEMTO = True
    log("[INFO] pyorbbecsdk encontrado.")
except ImportError:
    USAR_FEMTO = False
    log("[INFO] pyorbbecsdk no encontrado, usando camara normal.")

UDP_IP   = "127.0.0.1"
UDP_PORT = 7654
PINCH_UMBRAL = 0.07

mp_manos = mp.solutions.hands
manos = mp_manos.Hands(
    static_image_mode=False,
    max_num_hands=1,
    model_complexity=0,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5,
)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
log(f"[INFO] Socket UDP listo → {UDP_IP}:{UDP_PORT}")

# Última posición conocida (se envía aunque no haya mano)
ultimo_x       = 0.5
ultimo_y       = 0.5
ultimo_pressed = False


def centroide(landmarks):
    xs = [lm.x for lm in landmarks.landmark]
    ys = [lm.y for lm in landmarks.landmark]
    return sum(xs) / len(xs), sum(ys) / len(ys)


def es_pinch(landmarks):
    pulgar = landmarks.landmark[4]
    indice = landmarks.landmark[8]
    dist = math.sqrt((pulgar.x - indice.x) ** 2 + (pulgar.y - indice.y) ** 2)
    return dist < PINCH_UMBRAL


def enviar(x, y, pressed):
    msg = json.dumps({"x": round(x, 4), "y": round(y, 4), "pressed": pressed})
    sock.sendto(msg.encode(), (UDP_IP, UDP_PORT))


def procesar_frame(frame):
    global ultimo_x, ultimo_y, ultimo_pressed
    rgb    = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    result = manos.process(rgb)
    if result.multi_hand_landmarks:
        lms            = result.multi_hand_landmarks[0]
        ultimo_x, ultimo_y = centroide(lms)
        ultimo_pressed = es_pinch(lms)
    # Siempre manda la última posición conocida (heartbeat)
    enviar(ultimo_x, ultimo_y, ultimo_pressed)


try:
    if USAR_FEMTO:
        log("[INFO] Iniciando Femto Bolt...")
        pipeline = Pipeline()
        config   = Config()
        color_profile = (
            pipeline.get_stream_profile_list(OBSensorType.COLOR_SENSOR)
            .get_default_video_stream_profile()
        )
        config.enable_stream(color_profile)
        pipeline.start(config)
        log("[INFO] Pipeline Femto OK. Entrando al loop.")

        try:
            while True:
                frames = pipeline.wait_for_frames(100)
                if frames is None:
                    enviar(ultimo_x, ultimo_y, ultimo_pressed)
                    continue
                color_frame = frames.get_color_frame()
                if color_frame is None:
                    enviar(ultimo_x, ultimo_y, ultimo_pressed)
                    continue
                frame = cv2.imdecode(
                    np.frombuffer(color_frame.get_data(), dtype=np.uint8),
                    cv2.IMREAD_COLOR,
                )
                if frame is not None:
                    procesar_frame(frame)
        finally:
            pipeline.stop()

    else:
        log("[INFO] Abriendo camara 0...")
        cap = cv2.VideoCapture(0)
        if not cap.isOpened():
            log("[ERROR] No se pudo abrir la camara 0.")
        else:
            log("[INFO] Camara OK. Entrando al loop.")
            while cap.isOpened():
                ok, frame = cap.read()
                if not ok:
                    log("[WARN] cap.read() fallo.")
                    break
                procesar_frame(frame)
            cap.release()

except Exception:
    log("[ERROR] Excepcion en el loop principal:\n" + traceback.format_exc())

finally:
    sock.close()
    log("[INFO] Tracker cerrado.")
