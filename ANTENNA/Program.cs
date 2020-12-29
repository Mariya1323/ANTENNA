using System;
using System.Numerics;
using System.IO;
using System.Collections.Generic;

namespace Антенна
{
    class Program
    {
        // Добавляем константы для перевода в градусы и радианы
        const double toDeg = 180/Math.PI;
        const double toRad = Math.PI / 180;
        static void Main(string[] args)
        {
            // Вносим частоту в ГГц
            const double F0 = 3;
            // Вносим скорость света в см/нс
            const double c = 30;
            // Считаем длину волны в см
            const double lambda0 = F0 * c;
            // Задаем желаемую ширину луча
            const double Th07 = 5 * toRad;
            // Определяем требуемую апертуру решетки
            const double L = 51 * lambda0 / (2 * Th07 * toDeg);
            // Определяем максимальный угол отклонения луча
            const double ThetaMax = 45 * toRad;
            // Определяем текущее отклонение луча
            const double Theta0 = 15 * toRad;

            // Вносим элемент решетки,и у нас получается что антенна должна лежать в отдельной библиотеке
            Antenna Element = new Horn();
            //Можно вывести его ДН в консоль, если стереть // на следующей строке
            //PrintPattern(Element, -90, 90, 1);

            // Выводим часть из этой информации в консоль
            Console.WriteLine("Расчет ДН решетки");
            Console.WriteLine("Частота {0} ГГц, длина волны {1} см",F0,lambda0);
            Console.WriteLine("Требуется обеспечить ширину луча {0} градусов", 2*Th07*toDeg);
            Console.WriteLine("Размер апертуры решетки {0} см ({1} м)", L, L/100);

            // Определяем расстояние между элементами, которое позволит отклонять луч на нужный угол и выводим его
            var dx = lambda0 / (1 + Math.Abs(Math.Sin(ThetaMax)));
            Console.WriteLine("Шаг между элементами решетки {0:f2} см", dx);

            // Считаем число элементов решетки с округлением вверх и выводим его
            var N = Math.Ceiling(L / dx);
            Console.WriteLine("Число элементов решетки {0}", N);

            // Считаем размер апертуры, который получился с учетом этих 2-х расчетов (Он будет больше, а луч уже, чем задано) и выводим его
            var Lr = N * dx;
            Console.WriteLine("Физический размер апертуры {0} см", Lr);

            // Создаем массив, для хранения координат элементов
            double[] X = new double[(int)N];
            // Пробегаемся по всем элементам массива и записываем в них координаты элементов решетки, посчитанные через шаг между элементами и номер элемента
            for (int n = 0; n < N;n++)
            {
                X[n] = n * dx;
            }

            // Создаем список
            
            List<PatternVal> F = new List<PatternVal>();

            // Теперь в этот список занесем множитель решетки 
            // Заполним список элементами с помощью функции, описание к которой тоже внизу
            F = GetArrayPattern(-90*toRad,toRad,91*toRad,lambda0,X);

            // Теперь нужно множитель решетки перемножить с ДН излучателя
            // Пробегаемся по всем элементам списка
            for (int i = 0; i < F.Count; i++)
            {
                // Поскольку список состоит из элементов в виде нашей структуры, мы создаем её экземпляр
                PatternVal P = new PatternVal();
                // Записываем в этот экземпляр угол, на котором будем перемножать ДН и множитель решетки
                P.Theta = F[i].Theta;
                // Значение излучения записываем как произведение множителя решетки и ДН излучателя в данном направлении
                P.Value = F[i].Value * Element.Pattern(F[i].Theta);
                // Заносим в список наш экземпляр структуры на место старого, в котором был только множитель решетки
                F[i] = P;
            }

            // Вызываем функцию вывода в файл
            WriteArrayPatternToFile(F, "pattern.txt");
        }

  
        private static void PrintPattern(Antenna A, double Tmin, double Tmax, double dT)
        {
            // Пробегаемся по всем углам 
            for(var T = Tmin; T<=Tmax; T+=dT)
            {
                // Записываем в переменную ДН излучателя по данному углу
                var f = A.Pattern(T*toRad);
                // Выводим угол и ДН в строку в консоли
                Console.WriteLine("{0,5:F1} => {1,8:F3}",T,Complex.Abs(f));
            }
        }

        // Функция для заполнения списка
        private static List<PatternVal> GetArrayPattern(double ThetaMin, double dTheta, double ThetaMax, double lambda, double[] X)
        {
            // Создаем пустой список, который мы будем заполнять
            List<PatternVal> values = new List<PatternVal>();

            // Пробегаемся от минимального угла к максимальному с заданным шагом
            for (double T = ThetaMin; T <= ThetaMax; T += dTheta)
            {
                // Поскольку список состоит из элементов в виде нашей структуры, мы создаем её экземпляр
                PatternVal P = new PatternVal();
                // Записываем в этот экземпляр угол, значение, для которого мы сейчас смотрим
                P.Theta = T;
                // И записываем само значение с применением функции ArrayPattern
                P.Value = ArrayPattern(T, X, lambda);
                // Добавляем этот экземпляр структуры в список
                values.Add(P);
            }
            // Возвращаем полученный список
            return values;
        }

        // Функция для определения множителя решетки на конкретном угле
        private static Complex ArrayPattern(double Theta, double[] X, double lambda)
        {
            // Создаем комплексную переменную, в которую будем включать результат
            Complex Res = 0;
            // Считаем волновое число
            double k = 2 * Math.PI / lambda;

            // Пробегаемся по всему массиву с координатами элементов решетки
            for (int n = 0; n < X.Length; n++)
            {
                // Считаем фазу, приходящую от элемента
                double phase = k * X[n] * Math.Sin(Theta);
                // Переводим её в вид комплексной экспоненты
                Complex f = Complex.Exp(new Complex(0, phase));
                // Складываем её с общим результатом
                Res += f;
            }

            // К этому моменту мы получили сумму из комплексных экспонент с фазами, приходящими от излучателей
            // Возвращаем нашу сумму, деленную на число элементов
            return Res / X.Length;
        }

        // Функция вывода в файл
        private static void WriteArrayPatternToFile(List<PatternVal> F, string FileName)
        {

            using (StreamWriter writer = new StreamWriter(FileName))
            {
                writer.WriteLine("Theta,deg;Re(f);Im(f);Abs(f)db;Phase(f)deg");
                foreach (var f in F)
                {
                    writer.WriteLine("{0},{1},{2},{3},{4},",
                        f.Theta * toDeg,f.Value.Real,f.Value.Imaginary,
                        20 * Math.Log10(f.Value.Magnitude),f.Value.Phase * toDeg);
                }
            }
        }
    }


    // Создаем структуру
    struct PatternVal
    {
        public double Theta;
        public Complex Value;
    }
}
