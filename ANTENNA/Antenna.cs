using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Антенна
{
    // Создаем абстрактный класс Антенны
    abstract class Antenna
    {
        public abstract Complex Pattern(double Theta);

    }

    

    // Создаем класс Изотропный излучатель и говорим что это Антенна 
    class Isotrope : Antenna
    {
        // Говорим, что у Изотропного излучателя ДН  всегда равна 1
        public override Complex Pattern(double Theta)
        {
            return 1;
        }
    }

    // Создаем класс Диполь и говорим что это Антенна 
    class Dipole : Antenna
    {
        // Говорим, что его длина - 0.5 
        public double Length=0.5;

        // Говорим, что у Диполя ДН  зависит от угла так:
        public override Complex Pattern(double Theta)
        {
            var l = 2 * Math.PI * Length;
            return ((Math.Cos(l * Math.Sin(Theta)) - Math.Cos(l)) / (Math.Cos(Theta) * (1 - Math.Cos(l))));
        }

    }

    // Тут мы создаем класс Рупорный излучатель и говорим что это Антенна 
    class Horn : Antenna
    {
        // Говорим, что у Рупорного излучателя ДН  зависит от угла так:
        public override Complex Pattern(double Theta)
        {
            // Считаем что он излучает по закону косинуса
            var f = Math.Cos(Theta) * Math.Cos(Theta);
            return f;
        }
    }

}
