import sys
from math import sqrt

class Triangle:
    def Hypotenuse(self, a, b):
        return sqrt(a*a + b*b)

sys.triangle = Triangle()