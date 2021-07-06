using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Si
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener(File.CreateText("Промежуточные.txt")));//Вывод промежуточных результатов
            Debug.AutoFlush = true;
            vvodZnach vz = new vvodZnach();
            vz.simplexBol();
            Console.ReadKey();
        }
    }
}
public class Simplex
    {
        double[,] table; //симплекс таблица
        int Sm, D;
        List<int> basis; //список базисных переменных
        public Simplex(double[,] source)
        {
            Sm = source.GetLength(0);
           D = source.GetLength(1);
            table = new double[Sm, D + Sm - 1];
            basis = new List<int>();
            // Составление симплекстаблицы
            for (int i = 0; i < Sm; i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    if (j < D)
                        table[i, j] = source[i, j];
                    else
                        table[i, j] = 0;
                }
                //выставляем коэффициент 1 перед базисной переменной в строке
                if ((D + i) < table.GetLength(1))
                {
                    table[i, D + i] = 1;
                    basis.Add(D + i);
                }
            }
            D = table.GetLength(1);
        }
        public double[,] Calculate(double[] result)
        {
            int mainCol, mainRow; //результирующие столбец и строка
            while (!IsItEnd())
            {
                mainCol = findMainCol();
                mainRow = findMainRow(mainCol);
                basis[mainRow] = mainCol;
                double[,] new_table = new double[Sm, D];
                for (int j = 0; j < D; j++)
                    new_table[mainRow, j] = table[mainRow, j] / table[mainRow, mainCol];
                for (int i = 0; i < Sm; i++)
                {
                    if (i == mainRow)
                        continue;
                    for (int j = 0; j < D; j++)
                        new_table[i, j] = table[i, j] - table[i, mainCol] * new_table[mainRow, j];
                }
                table = new_table;
            }
            
            for (int i = 0; i < result.Length; i++)
            {
                int k = basis.IndexOf(i + 1);
                if (k != -1)
                    result[i] = table[k, 0];
                else
                    result[i] = 0;
            }
            return table;
        }
        private bool IsItEnd() //остановка программы, если строка оценок меньше 0
        {
            bool flag = true;
            for (int j = 1; j < D; j++)
            {
                if (table[Sm - 1, j] < 0)
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }
        private int findMainCol()// Поиск разрешающего столбца
        {
            int mainCol = 1;
            for (int j = 2; j < D; j++)
                if (table[Sm - 1, j] < table[Sm - 1, mainCol])
                    mainCol = j;
            Debug.WriteLine("Разрешающий столбец: "+ mainCol);
            return mainCol;
        }
        private int findMainRow(int mainCol)//Поиск строки
        {
            int mainRow = 0;
            for (int i = 0; i < Sm - 1; i++)
                if (table[i, mainCol] > 0)
                {
                    mainRow = i;
                    break;
                }
            for (int i = mainRow + 1; i < Sm - 1; i++)
                if ((table[i, mainCol] > 0) && ((table[i, 0] / table[i, mainCol]) < (table[mainRow, 0] / table[mainRow, mainCol])))
                    mainRow = i;
            Debug.WriteLine("Разрешающая строка: " + mainRow);
            return mainRow;
        }
    }
    public class vvodZnach // Метод ввода и вывода данных
    {
        public double[,] mas;
        public double[] bufMass = { };
        public double[,] table_result;
        /// <summary>
        
        public void simplexBol()
        {
            double[] ms1 = { };
            string str1 = "";
            int raz1 = 0, d = 0;
            //Запись из csv в массив
            try
            {
                using (StreamReader sr = new StreamReader(@"Ввод.csv"))
                {
                    sr.ReadLine();
                    str1 = sr.ReadToEnd();
                    string[] st = str1.Split('\n');
                    raz1 = st.Length;
                    ms1 = Array.ConvertAll(st[0].Split(';'), double.Parse);
                    d = ms1.Length;
                    mas = new double[raz1, d];
                    for (int i = 0; i < raz1; i++)
                    {
                        ms1 = Array.ConvertAll(st[i].Split(';'), double.Parse);
                        for (int j = 0; j < d; j++)
                        {
                            mas[i, j] = ms1[j];

                        }
                    }

                    // Меняем первый и последний столбцы местами для того что бы удобно вводить в csv файл ограничения
                    for (int i = 0; i < raz1; i++)
                    {
                        for (int j = 0; j < d; j += d - 1)
                        {
                            double tmp = mas[i, j];
                            mas[i, j] = mas[i, d - 1];
                            mas[i, d - 1] = tmp;
                        }

                    }
                    // делаем строку оценок отрицательной для корректного вывода
                    for (int i = 0; i < raz1; i++)
                    {
                        for (int j = 0; j < d; j++)
                        {
                            if (i == raz1 - 1)
                            {
                                mas[i, j] = mas[i, j] * (-1);
                            }
                        }
                    }
                    Console.WriteLine("Исходная матрица");
                    for (int i = 0; i < raz1; i++)
                    {
                        for (int j = 0; j < d; j++)
                        {
                            Console.Write($"{mas[i, j],5}");
                        }
                        Console.WriteLine();
                    }
                }

                double[] result = new double[raz1 * 2];
                //Конструктор класса
                Simplex S = new Simplex(mas);
               
                table_result = S.Calculate(result);
                for (int i = 0; i < table_result.GetLength(0); i++)
                {
                    for (int j = 0; j < table_result.GetLength(1); j++)
                    {
                        if (i == raz1 - 1)
                        {
                            table_result[i, j] = table_result[i, j] * (-1);
                        }
                    }
                }
                Console.WriteLine("Решение:");
                for (int i = 0; i < table_result.GetLength(0); i++)
                {
                    for (int j = 0; j < table_result.GetLength(1); j++)
                        Console.Write($"{Math.Round(table_result[i, j]),5}" + ";");
                    Console.WriteLine("");
                }
                int ind1 = 1;
                for (int j = d - 2; j >= 0; j--)
                {
                    Console.WriteLine("X[{0}] = {1}", ind1, result[j]);
                    ind1++;
                }
                Console.WriteLine("F = " + (table_result[table_result.GetLength(0) - 1, 0] * -1));
                Console.WriteLine("F' = " + (table_result[table_result.GetLength(0) - 1, 0]));
                using (StreamWriter sw = new StreamWriter(@"Вывод.csv"))
                {
                    sw.WriteLine("reshenie:");
                    for (int i = 0; i < table_result.GetLength(0); i++)
                    {
                        for (int j = 0; j < table_result.GetLength(1); j++)
                            sw.Write($"{Math.Round(table_result[i, j]),5}" + ";");
                        sw.WriteLine();
                    }
                    ind1 = 1;
                    for (int j = d - 2; j >= 0; j--)
                    {
                        sw.WriteLine("X[{0}] = {1}", ind1, result[j]);
                        ind1++;
                    }
                    sw.WriteLine("F = " + (table_result[table_result.GetLength(0) - 1, 0] * -1));
                    sw.WriteLine("F' = " + (table_result[table_result.GetLength(0) - 1, 0]));
                }
            }
            catch
            {
                Console.WriteLine("В файле ошибка, измените данные");
            }
        }
    }
   