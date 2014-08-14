# -*- coding: cp1251 -*-
import numpy as np
import cv2
#Galton's portrait
#good haarcascade_frontalface_alt_tree.xml
#good haarcascade_frontalface_alt2.xml
face_cascade = cv2.CascadeClassifier('haarcascade_frontalface_alt2.xml')
eye_cascade = cv2.CascadeClassifier('haarcascade_eye.xml')
two_eye_cascade = cv2.CascadeClassifier('haarcascade_mcs_eyepair_big.xml')

def get_faces(gray_img):
    faces = face_cascade.detectMultiScale(gray_img, 1.3, 5)
    return faces

def get_eyes(gray_img):
    eyes = eye_cascade.detectMultiScale(gray_img)
    return eyes

def get_two_eyes(gray_img):
    eyes = two_eye_cascade.detectMultiScale(gray_img)
    return eyes

def enlarge_area(area, percents = 0, pixels = 0):
    x,y,w,h = face
    half_w, half_h = w/2, h/2
    c_x, c_y = x + half_w, y + half_h
    if percents:
        half_w = half_w*percents
        half_h = half_h*percents
    elif pixels:
        half_w += pixels
        half_h += pixels
    x, y = c_x - half_w, c_y - half_h
    w, h = half_w * 2, half_h * 2
    return int(x),int(y),int(w),int(h)

def get_rect_from_img(img, rect):
    x,y,w,h = rect
    return img[y:y+h, x:x+w]

def get_faces_and_eyes():
    result = []
    img = cv2.imread('3.jpg')
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    faces = get_faces(gray)

    for face in faces:
        roi_gray = get_rect_from_img(gray, face)
        roi_color = get_rect_from_img(img, face)
        #cv2.rectangle(img,(x,y),(x+w,y+h),(255,0,0),2)
        eyes = get_eyes(roi_gray)
        result.append( (roi_color, [ (x,y) for (x,y,_,_) in eyes]))
        #for (ex,ey,ew,eh) in eyes:
        #    cv2.rectangle(roi_color,(ex,ey),(ex+ew,ey+eh),(0,255,0),2)
    return result

def prepare_faces(faces_with_eyes):
    min_h = min((f.shape[1] for (f, _) in faces_with_eyes))
    min_w = min((f.shape[0] for (f, _) in faces_with_eyes))
    rect = (0, 0, min_w, min_h)
    return [get_rect_from_img(face, rect) for (face, _) in faces_with_eyes]

def summ_images(faces):
    f1 = faces[0]
    image = np.zeros(f1.shape, np.uint64)
    result = np.zeros(f1.shape, np.uint8)
    #copy first face
    for x, y in np.nditer([f1, image], op_flags=[ ['readonly'], ['writeonly']]):
        y[...] = x
        
    #aggregate faces
    for face in faces[1:]:
        for x, y in np.nditer([face, image], op_flags=[ ['readwrite'], ['writeonly']]):
            y[...] += x
            
    #average faces
    face_count = len(faces)
    for x in np.nditer(image, op_flags=['readwrite']):
        x[...] = x / face_count

    #copy to result image
    for x, y in np.nditer( [image, result], op_flags=[ ['readonly'], ['writeonly']]):
        y[...] = np.uint8(x)
        
    return result

faces_and_eyes = get_faces_and_eyes()
prepared_faces = prepare_faces(faces_and_eyes)
face = summ_images(prepared_faces)
cv2.imshow('img',face)
cv2.waitKey(0)
cv2.destroyAllWindows()
