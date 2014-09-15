# -*- coding: cp1251 -*-
import os
import math
import numpy as np
import cv2
from math import atan, hypot, pi
# Galton's portrait
# good haarcascade_frontalface_alt_tree.xml
# good haarcascade_frontalface_alt2.xml
class Eye(object):
    def __init__(self, x, y):
        self.x = x
        self.y = y

    def __add__(self, other):
        return Eye(self.x + other.x, self.y + other.y)

    def __sub__(self, other):
        return Eye(self.x - other.x, self.y - other.y)

    def to_tuple(self):
        return self.x, self.y

class Transformation(object):
    def __init__(self):
        self.angle = 0
        self.scale = 1
        self.translation = (0, 0)
        self.center = (0, 0)

    def as_matrix(self):
        angle_in_grads = (self.angle / pi) * 180;
        matrix = cv2.getRotationMatrix2D(self.center, angle_in_grads, self.scale)
        return matrix


class FaceHelper(object):
    @staticmethod
    def get_transformation(eyes, width=None, standard_eyes=None):
        eyes = sorted(eyes, key=lambda eye: eye.x)
        transformation = Transformation()
        eye_vector = eyes[1] - eyes[0]
        if eye_vector.x != 0:
            transformation.angle = atan(float(eye_vector.y) / eye_vector.x)
            transformation.center = eyes[0].to_tuple()
        if standard_eyes:
            standard_eyes = sorted(standard_eyes, key=lambda eye: eye.x)
            standard_eyes_vector = standard_eyes[1] - standard_eyes[0]
            width = hypot(standard_eyes_vector.x, standard_eyes_vector.y)
            transformation.translation = (standard_eyes[0] - eyes[0]).to_tuple()
        if width is not None:
            transformation.scale = width / hypot(eye_vector.x, eye_vector.y)
        return transformation


class EyeProcessor(object):
    eye_cascade = cv2.CascadeClassifier('haarcascade_eye.xml')
    two_eye_cascade = cv2.CascadeClassifier('haarcascade_mcs_eyepair_big.xml')

    @staticmethod
    def get_eyes(gray_img):
        eyes = EyeProcessor.eye_cascade.detectMultiScale(gray_img)
        return eyes

    @staticmethod
    def get_two_eyes(gray_img):
        eyes = EyeProcessor.two_eye_cascade.detectMultiScale(gray_img)
        return eyes


class FaceProcessor(object):
    def __init__(self):
        self.face_cascade = cv2.CascadeClassifier('haarcascade_frontalface_alt2.xml')
        self.eye_processor = EyeProcessor()

    def get_faces(self, gray_img):
        faces = self.face_cascade.detectMultiScale(gray_img, 1.3, 5)
        return faces

    def enlarge_area(self, area, percents=0, pixels=0):
        x, y, w, h = area
        half_w, half_h = w / 2, h / 2
        c_x, c_y = x + half_w, y + half_h
        if percents:
            half_w = half_w * percents
            half_h = half_h * percents
        elif pixels:
            half_w += pixels
            half_h += pixels
        x, y = c_x - half_w, c_y - half_h
        w, h = half_w * 2, half_h * 2
        return int(x), int(y), int(w), int(h)

    @staticmethod
    def get_rect_from_img(img, rect):
        x, y, w, h = rect
        return img[y:y + h, x:x + w]

    @staticmethod
    def scale_and_center_images(img, w, h):
        img_w, img_h = img.shape[0], img.shape[1]
        assert img_w <= w and img_h <= h
        result = np.zeros((h, w, 3), np.uint8)
        offset_w = (w - img_w) / 2
        offset_h = (h - img_h) / 2
        result[offset_h:offset_h + img_h, offset_w:offset_w + img_w] = img
        return result

    @staticmethod
    def scale_and_match_faces_by_eyes(images, max_w, max_h):
        if not images:
            return []
        result = []
        first_face = images[0]
        first_face = FaceProcessor.scale_and_center_images(first_face, max_w, max_h)
        first_face_gray = cv2.cvtColor(first_face, cv2.COLOR_BGR2GRAY)
        first_face_eyes = FaceProcessor.get_eyes(first_face_gray)

        real_eyes = [Eye(x, y) for x, y, _, _ in first_face_eyes]
        rotation_matrix = FaceHelper.get_transformation(real_eyes).as_matrix()
        img_w, img_h = first_face.shape[0], first_face.shape[1]
        result.append(cv2.warpAffine(first_face, rotation_matrix, (img_w, img_h)))

        for img in images[1:]:
            img = FaceProcessor.scale_and_center_images(img, max_w, max_h)
            img_gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
            eyes = FaceProcessor.get_eyes(img_gray)

            for (ex, ey, ew, eh) in eyes:
                cv2.rectangle(img, (ex, ey), (ex + ew, ey + eh), (0, 255, 0), 2)

            img_eyes = [Eye(x, y) for x, y, _, _ in eyes]
            transformation = FaceHelper.get_transformation(img_eyes, standard_eyes=real_eyes)
            rotation_matrix = transformation.as_matrix()
            img = cv2.warpAffine(img, rotation_matrix, (img_w, img_h))
            translation_matrix = np.float32([[1,0,transformation.translation[0]],[0,1,transformation.translation[1]]])
            img = cv2.warpAffine(img, translation_matrix, (img_w, img_h))

            result.append(img)
        return result


    @staticmethod
    def get_eyes(face_image):
        all_eye_pairs = EyeProcessor.get_two_eyes(face_image)
        if len(all_eye_pairs) == 0:
            return []
        all_eyes = EyeProcessor.get_eyes(face_image)
        centered_eyes = [(x + w / 2, y + h / 2, 10, 10) for (x, y, w, h) in all_eyes]
        bound_x, bound_y, bound_w, bound_h = all_eye_pairs[0]
        filtered_eyes = []
        for x, y, w, h in centered_eyes:
            if bound_x <= x <= bound_x + bound_w and bound_y <= y <= bound_y + bound_y:
                filtered_eyes.append((x, y, w, h))
        return filtered_eyes

    def get_faces_by_path(self, path_to_photo):
        result = []
        img = cv2.imread(path_to_photo)
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        faces = self.get_faces(gray)

        for face in faces:
            roi_color = self.get_rect_from_img(img, face)
            # cv2.rectangle(img,(x,y),(x+w,y+h),(255,0,0),2)
            result.append(roi_color)
        return result


    def prepare_faces(self, faces_with_eyes):
        min_h = min((f.shape[1] for f in faces_with_eyes))
        min_w = min((f.shape[0] for f in faces_with_eyes))

        max_h = max((f.shape[1] for f in faces_with_eyes))
        max_w = max((f.shape[0] for f in faces_with_eyes))

        rect = (0, 0, min_w, min_h)
        # return [self.get_rect_from_img(face, rect) for (face, _) in faces_with_eyes]
        #return [self.scale_and_center_images(face, max_w, max_h, eyes) for (face, eyes) in faces_with_eyes]
        return self.scale_and_match_faces_by_eyes(faces_with_eyes, max_w, max_h)


    def sum_images(self, faces):
        f1 = faces[0]
        image = np.zeros(f1.shape, np.uint64)
        result = np.zeros(f1.shape, np.uint8)
        # copy first face
        for x, y in np.nditer([f1, image], op_flags=[['readonly'], ['writeonly']]):
            y[...] = x

        # aggregate faces
        for face in faces[1:]:
            for x, y in np.nditer([face, image], op_flags=[['readwrite'], ['writeonly']]):
                y[...] += x

        # average faces
        face_count = len(faces)
        for x in np.nditer(image, op_flags=['readwrite']):
            x[...] = x / face_count

        # copy to result image
        for x, y in np.nditer([image, result], op_flags=[['readonly'], ['writeonly']]):
            y[...] = np.uint8(x)

        return result


def process_and_show(photos):
    face_processor = FaceProcessor()
    faces_and_eyes = []
    for photo in photos:
        faces_and_eyes = faces_and_eyes + face_processor.get_faces_by_path(photo)
    prepared_faces = face_processor.prepare_faces(faces_and_eyes)
    face = face_processor.sum_images(prepared_faces)
    cv2.imshow('img', face)
    cv2.waitKey(0)
    cv2.destroyAllWindows()


def main():
    for arg, _, photos in os.walk(os.path.join(".", "little input")):
        process_and_show([os.path.join(arg, p) for p in photos])


if __name__ == '__main__':
    main()


