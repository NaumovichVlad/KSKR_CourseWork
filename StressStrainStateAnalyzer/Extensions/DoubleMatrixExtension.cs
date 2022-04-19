﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressStrainStateAnalyzer.Extensions
{
    internal static class DoubleMatrixExtension
    {
        //Произведение матриц
        public static double[,] Multiply(this double[,] a, double[,] b)
        {
            var c = new double[a.GetLength(0), b.GetLength(1)];
            for (var i = 0; i < a.GetLength(0); i++)
                for (var j = 0; j < b.GetLength(1); j++)
                    for (var k = 0; k < b.GetLength(0); k++)
                        c[i, j] = c[i, j] + a[i, k] * b[k, j];
            return c;
        }

        //Произвадение матрицы на число
        public static double[,] Multiply(this double[,] a, double b)
        {
            for (var i = 0; i < a.GetLength(0); i++)
                for (var j = 0; j < a.GetLength(1); j++)
                    a[i, j] = a[i, j] * b;
            return a;
        }

        //Транспонирование матрицы
        public static double[,] Transpose(this double[,] a)
        {
            var b = new double[a.GetLength(1), a.GetLength(0)];
            for (var i = 0; i < a.GetLength(0); i++)
                for (var j = 0; j < a.GetLength(1); j++)
                    b[j, i] = a[i, j];
            return b;
        }

        //Нахождение обратной матрицы с помощью метода Гаусса
        public static double[,] Inverse(this double[,] a)
        {
            var a_1 = new double[a.GetLength(0), a.GetLength(1)];
            for (var i = 0; i < 6; i++)
                a_1[i, i] = 1;

            double tmp;

            for (var i = 0; i < 6; i++)
            {
                if (a[i, i] == 0)
                {
                    int index = i + 1;
                    while (index < 6)
                    {
                        if (a[index, i] != 0)
                            break;
                        index++;
                    }

                    if (index != 6)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            a[i, j] = a[i, j] + a[index, j];
                            a[index, j] = a[i, j] - a[index, j];
                            a[i, j] = a[i, j] - a[index, j];


                            a_1[i, j] = a_1[i, j] + a_1[index, j];
                            a_1[index, j] = a_1[i, j] - a_1[index, j];
                            a_1[i, j] = a_1[i, j] - a_1[index, j];
                        }
                    }
                }

                tmp = a[i, i];
                for (int j = 0; j < 6; j++)
                {
                    a[i, j] = a[i, j] / tmp;
                    a_1[i, j] = a_1[i, j] / tmp;
                }

                for (int j = 0; j < 6; j++)
                {
                    if (j != i)
                    {
                        tmp = a[j, i];
                        if (tmp != 0)
                            for (int k = 0; k < 6; k++)
                            {
                                a[j, k] = a[j, k] - tmp * a[i, k];
                                a_1[j, k] = a_1[j, k] - tmp * a_1[i, k];
                            }
                    }
                }
            }
            return (a_1);
        }
    }
}
