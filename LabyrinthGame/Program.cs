using System.Runtime.InteropServices;

namespace LabyrinthGame
{
    class Program
    {
        public static string map =
            "###########################" +
            "#.........................#" +
            "#.#####.#######.#.#.#####.#" +
            "#.#...#.#.....#.#...#...#.#" +
            "#.#.#.#.#.###.#.#####.#.#.#" +
            "#...#.#.#...#.#.....#.#.#.#" +
            "#.###.#.###.#.#######.#.#.#" +
            "#.#...#.#...#.......#.#...#" +
            "#.#.###.#.###########.#.###" +
            "#.#.#...#.............#...#" +
            "#.#.#.#################.#.#" + // X - Exit
            "#...#................X#.#.#" +
            "###########################";

        public static double PosX = 2, PosY = 2, Angle = 45;//Starting cord
        public static int ScreenWidth = 100, ScreenHeight = 40; // Screen Width and Height
        public static double Fov = Math.PI / 3;
        public static char[] buffer = new char[ScreenWidth * ScreenHeight];
        public static int mapWidth = 27, mapHeight = 13; //Map size
        public static double playerSpeed = 25;
        public static double raySpeed = 0.1;
        public static double rotationSpeed = 6;

        public static void Input(double deltaTime)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.A:
                        Angle += rotationSpeed * deltaTime;
                        break;
                    case ConsoleKey.D:
                        Angle -= rotationSpeed * deltaTime;
                        break;
                    case ConsoleKey.W:
                        PosX += Math.Sin(Angle) * playerSpeed * deltaTime;
                        PosY += Math.Cos(Angle) * playerSpeed * deltaTime;
                        CheckExit();
                        if (map[(int)PosY * mapWidth + (int)PosX] == '#')
                        {
                            PosX -= Math.Sin(Angle) * playerSpeed * deltaTime;
                            PosY -= Math.Cos(Angle) * playerSpeed * deltaTime;
                        }
                        break;
                    case ConsoleKey.S:
                        PosX -= Math.Sin(Angle) * playerSpeed * deltaTime;
                        PosY -= Math.Cos(Angle) * playerSpeed * deltaTime;
                        CheckExit();
                        if (map[(int)PosY * mapWidth + (int)PosX] == '#')
                        {
                            PosX += Math.Sin(Angle) * playerSpeed * deltaTime;
                            PosY += Math.Cos(Angle) * playerSpeed * deltaTime;
                        }
                        break;
                }
            }
        }


        //End game
        public static void CheckExit()
        {
            if (map[(int)PosY * mapWidth + (int)PosX] == 'X')
            {
                Console.Clear();
                Console.WriteLine("Congratulations! You found the exit and won!");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        public static void Main()
        {
            Console.CursorVisible = false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.SetWindowSize(ScreenWidth, ScreenHeight);
                Console.SetBufferSize(ScreenWidth, ScreenHeight);
            }
            else
            {
                Console.WriteLine("You need Windows to play");
            }


            var currentTime = DateTime.Now;

            while (true)
            {
                var newTime = DateTime.Now;
                var deltaTime = (newTime - currentTime).TotalSeconds;
                currentTime = newTime;

                Input(deltaTime);

                double[] distancesToWalls = new double[ScreenWidth];
                for (int x = 0; x < ScreenWidth; x++)
                {
                    double rayDirection = Angle + Fov / 2 - x * Fov / ScreenWidth;
                    double rayX = Math.Sin(rayDirection);
                    double rayY = Math.Cos(rayDirection);

                    double distance = 0;
                    bool hit = false;
                    double depth = 20;

                    while (!hit && distance < depth)
                    {
                        distance += raySpeed;

                        int tx = (int)(PosX + rayX * distance);
                        int ty = (int)(PosY + rayY * distance);

                        if (tx < 0 || tx >= mapWidth || ty < 0 || ty >= mapHeight || map[ty * mapWidth + tx] == '#')
                        {
                            hit = true;
                        }
                    }
                    distancesToWalls[x] = distance;

                    int wall = (int)(ScreenHeight / 2d - ScreenHeight * Fov / distance);
                    int floor = ScreenHeight - wall;

                    for (int y = 0; y < ScreenHeight; y++)
                    {
                        switch (y)
                        {
                            case var _ when y <= wall:
                                buffer[y * ScreenWidth + x] = ' ';
                                break;
                            case var _ when y > wall && y <= floor:
                                char wallCharacter;

                                switch (distance)
                                {
                                    case var d when d <= depth / 4:
                                        wallCharacter = '█';
                                        break;
                                    case var d when d <= depth / 3:
                                        wallCharacter = '▓';
                                        break;
                                    case var d when d <= depth / 2:
                                        wallCharacter = '▒';
                                        break;
                                    default:
                                        wallCharacter = '░';
                                        break;
                                }

                                buffer[y * ScreenWidth + x] = wallCharacter;
                                break;
                            default:
                                double floorDistance = 1 - (y - ScreenHeight / 2d) / (ScreenHeight / 2);
                                char floorCharacter;

                                switch (floorDistance)
                                {
                                    case var d when d <= 0.2:
                                        floorCharacter = '#';
                                        break;
                                    case var d when d <= 0.50:
                                        floorCharacter = 'X';
                                        break;
                                    case var d when d <= 0.75:
                                        floorCharacter = '-';
                                        break;
                                    default:
                                        floorCharacter = '.';
                                        break;
                                }

                                buffer[y * ScreenWidth + x] = floorCharacter;
                                break;
                        }
                    }
                }

                Console.SetCursorPosition(0, 0);
                Console.Write(buffer);
            }
        }
    }
}
