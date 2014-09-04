from unittest import TestCase
from face_and_eyes import FaceHelper, Eye
from math import pi, cos, sin


class FaceHelperShould(TestCase):
    def assertFloatEqual(self, expected, actual):
        eps = 0.1
        if abs(expected - actual) > eps:
            message = "%s expected but given %s" % (expected, actual)
            self.fail(message)

    def test_can_align_eyes(self):
        eyes = [Eye(0, 0), Eye(5, 5)]
        helper = FaceHelper()
        transformation = helper.get_transformation(eyes)
        self.assertFloatEqual(pi/4.0, transformation.angle)

    def test_can_align_eyes_negative_angle(self):
        eyes = [Eye(0, 0), Eye(3, -3)]
        helper = FaceHelper()
        transformation = helper.get_transformation(eyes)
        self.assertFloatEqual(-pi/4.0, transformation.angle)

    def test_can_scale_eyes(self):
        eyes = [Eye(0, 0), Eye(0, 4)]
        helper = FaceHelper()
        transformation = helper.get_transformation(eyes, width=8)
        self.assertFloatEqual(2, transformation.scale)

    def test_rotate_and_scale(self):
        eyes = [Eye(2, 2), Eye(2 + 4*cos(pi/6.0), 2 + 4*sin(pi/6))]
        helper = FaceHelper()
        transformation = helper.get_transformation(eyes, width=10)
        self.assertFloatEqual(pi/6.0, transformation.angle)
        self.assertFloatEqual(2.5, transformation.scale)

