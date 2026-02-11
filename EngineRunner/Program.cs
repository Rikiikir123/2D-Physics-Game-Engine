using System;
using Engine.Math;


namespace EngineRunner
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Vector2 position = new Vector2(10, 20);
            //Console.WriteLine($"Vector position: X = {position.X}, Y = {position.Y}");
            Console.WriteLine("Vector position: X =" + position.X + ", Y = " + position.Y);
            Console.ReadLine();
        }
    }
}