from unittest import TestCase
from face_and_eyes import FaceHelper, Eye
from math import pi, cos, sin


class FaceHelperShould(TestCase):
    def setUp(self):
        self.helper = FaceHelper()

    def assertFloatEqual(self, expected, actual):
        eps = 0.1
        if abs(expected - actual) > eps:
            message = "%s expected but given %s" % (expected, actual)
            self.fail(message)

    def test_can_align_eyes(self):
        eyes = [Eye(0, 0), Eye(5, 5)]

        transformation = self.helper.get_transformation(eyes)

        self.assertFloatEqual(pi/4.0, transformation.angle)

    def test_can_align_eyes_negative_angle(self):
        eyes = [Eye(0, 0), Eye(3, -3)]

        transformation = self.helper.get_transformation(eyes)

        self.assertFloatEqual(-pi/4.0, transformation.angle)

    def test_can_scale_eyes(self):
        eyes = [Eye(0, 0), Eye(0, 4)]

        transformation = self.helper.get_transformation(eyes, width=8)

        self.assertFloatEqual(2, transformation.scale)

    def test_rotate_and_scale(self):
        eyes = [Eye(2, 2), Eye(2 + 4*cos(pi/6.0), 2 + 4*sin(pi/6))]

        transformation = self.helper.get_transformation(eyes, width=10)

        self.assertFloatEqual(pi/6.0, transformation.angle)
        self.assertEqual((2, 2), transformation.center)
        self.assertFloatEqual(2.5, transformation.scale)

    def test_can_translate(self):
        eyes = [Eye(0, 0), Eye(0, 4)]
        standard_eyes = [Eye(3, 5), Eye(3, 7)]

        transformation = self.helper.get_transformation(eyes, standard_eyes=standard_eyes)

        self.assertFloatEqual(0.5, transformation.scale)
        self.assertEqual((3, 5), transformation.translation)

    def test_eyes_sorted_from_left_to_right(self):
        eyes = [Eye(0, 0), Eye(3, 4)]
        standard_eyes = [Eye(3, 5), Eye(3, 7)]

        transformation = self.helper.get_transformation(eyes, standard_eyes=standard_eyes)

        eyes = [Eye(3, 4), Eye(0, 0)]
        standard_eyes = [Eye(3, 5), Eye(3, 7)]

        same_transformation = self.helper.get_transformation(eyes, standard_eyes=standard_eyes)

        self.assertFloatEqual(transformation.scale, same_transformation.scale)
        self.assertEqual(transformation.translation, same_transformation.translation)

    def test_can_be_casted_to_matrix(self):
        eyes = [Eye(0, 0), Eye(0, 4)]
        standard_eyes = [Eye(3, 5), Eye(3, 9)]

        transformation = self.helper.get_transformation(eyes, standard_eyes=standard_eyes)

        matrix = transformation.as_matrix()
        self.assertFloatEqual(3, matrix[0, 2])
        self.assertFloatEqual(5, matrix[1, 2])





