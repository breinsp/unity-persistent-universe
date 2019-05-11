using System;

namespace Assets.StarSystem.Generation
{
    public abstract class CelestialType
    {
        public double minRadius = 0; //in km
        public double maxRadius = 0; //in km
        public double density = 0; //in g/cm³
        public double minDistance = 0; //in million km
        public double maxDistance = 0; //in million km
        public int minTemperature; //kelvin
        public int maxTemperature; //kelvin
        public bool hasWater;
        public bool hasLava;
        public bool hasIce;
        public int minMoonCount;
        public int maxMoonCount;

        public static CelestialType GetRandomPlanetType(System.Random random)
        {
            double r = random.NextDouble();

            if (r > 0.7)
            {
                return new GasPlanet();
            }
            else if (r > 0.6)
            {
                return new IcePlanet();
            }
            else if (r > 0.4)
            {
                return new RockPlanet();
            }
            else if (r > 0.2)
            {
                return new MoltenPlanet();
            }
            return new WaterPlanet();
        }

        public static CelestialType GetRandomPlanetTypeExceptGas(Random random)
        {
            CelestialType type = null;
            while (type == null)
            {
                type = GetRandomPlanetType(random);
                if (type is CelestialType.GasPlanet) type = null;
            }
            return type;
        }

        public class Star : CelestialType
        {
            public Star()
            {
                density = 10.0;
                minRadius = 100;
                maxRadius = 300;
                minDistance = 0;
                maxDistance = 0;
                minTemperature = 4000;
                maxTemperature = 30000;
            }

            public override string ToString()
            {
                return "Star";
            }
        }
        public class GasPlanet : CelestialType
        {
            public GasPlanet()
            {
                density = 1.5;
                minRadius = 50;
                maxRadius = 150;
                minDistance = 2000;
                maxDistance = 5000;
            }

            public override string ToString()
            {
                return "Gas Planet";
            }
        }

        public class MoltenPlanet : CelestialType
        {
            public MoltenPlanet()
            {
                density = 5.0;
                minRadius = 5;
                maxRadius = 20;
                minDistance = 1000;
                maxDistance = 3000;
                hasLava = true;
            }

            public override string ToString()
            {
                return "Molten Planet";
            }
        }
        public class RockPlanet : CelestialType
        {
            public RockPlanet()
            {
                density = 5.0;
                minRadius = 5;
                maxRadius = 20;
                minDistance = 2000;
                maxDistance = 4000;
            }

            public override string ToString()
            {
                return "Rock Planet";
            }
        }
        public class IcePlanet : CelestialType
        {
            public IcePlanet()
            {
                density = 5.0;
                minRadius = 5;
                maxRadius = 20;
                minDistance = 4000;
                maxDistance = 6000;
                hasIce = true;
            }

            public override string ToString()
            {
                return "Ice Planet";
            }
        }
        public class WaterPlanet : CelestialType
        {
            public WaterPlanet()
            {
                density = 5.0;
                minRadius = 5;
                maxRadius = 20;
                minDistance = 3000;
                maxDistance = 4000;
                hasWater = true;
            }

            public override string ToString()
            {
                return "Water Planet";
            }
        }
        public class Moon : CelestialType
        {
            public CelestialType originalType;

            public Moon(CelestialType originalType)
            {
                this.originalType = originalType;
                density = 4.0;
                minRadius = 5;
                maxRadius = 10;
                minDistance = 500;
                maxDistance = 800;
                hasWater = originalType.hasWater;
                hasLava = originalType.hasLava;
                hasIce = originalType.hasIce;
            }

            public override string ToString()
            {
                return originalType.ToString().Replace("Planet", "Moon");
            }
        }
    }
}
