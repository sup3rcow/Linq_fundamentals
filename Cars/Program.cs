using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Cars
{

    class Program
    {
        static void Main(string[] args)
        {
            var cars = ProcessCars("fuel.csv");
            var manifacturers = ProcessManifacturers("manufacturers.csv");

            //------------------------------linq and  EF--------------------------------------------------------------------------------//
            //working with iqueryables and expression trees, tu govori i o prevodjenju linq expresiona u executable code

            //neke od c# operacija npr split(' ') nisu podrzane za Linq IQueriable, ako moras to korititi, onda moras
            // linq query prebaciti u ienumerable sa npr ToList(), i onda mozes koristiti split(' '), jer se onda to izvrsava u memoriji
            // i ne prevodi se u sql upit


            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CarDb>());//naziv metode sve govori
            InsertData();
            QueryData();






            //-------------------------xml linq----------------------------------------------------------------------------------------------------------//
            /* xDocument - naziv xml datoteke
             * xDeclaration - parametri xml datoteke, npr encoding, version
             * xComment - komentari unutar xml-a
             * xAttribute - atributi unutar xml tag-a
             * xElement - tagovi.. tj elementi
             */
            
            /*
             
            CreateXml();//sam si kreirao ovu metodu, tu vidis kako radi System.Xml.Linq
            QueryXml();
            
            */

            //-------------------------linq join group itd----------------------------------------------------------------------------------------------------//
            //Aggregating data -- OVOOOOOO, SAM MORAS NAPISATI METODE KOJE CE RACUNATI TAKO DA SE SMANJI BR PROAZA KROZ PODATKE
            var query9 = from car in cars
                         group car by car.Manufacturer into carGroup
                         select new
                         {
                             Name = carGroup.Key,
                             Max = carGroup.Max(c => c.Combined),     //3 puta prolazis kroz podatke za svakog proizvodjaca, nije dobro ovako
                             Min = carGroup.Min(c => c.Combined),
                             Avg = carGroup.Average(c => c.Combined)
                         } into resultt
                         orderby resultt.Max descending
                         select resultt;

            /*efikasniji nacin jer prolazis kroz podatke samo jednom*/
            var query10 = cars.GroupBy(c => c.Manufacturer)
                  .Select(g =>
                  {                                              //1 put prodjes kroz podatke, za svakog proizvodjaca
                      var result = g.Aggregate(new CarStatistics(),     //prodje jednom za svakog proizvodjaca  
                                        (acc, c) => acc.Accumulate(c),  //prodje za svaki auto od pojedinog proizvodjaca
                                         acc => acc.Compute());         //prodje jednom za svakog proizvodjaca 

                      return new
                      {
                          Name = g.Key,
                          Avg = result.Avg,
                          Min = result.Min,
                          Max = result.Max
                      };
                  }).OrderByDescending(r => r.Max);

            //foreach (var res in query10)
            //{
            //    Console.WriteLine($"{res.Name}");
            //    Console.WriteLine($"\tMax:{res.Max}");
            //    Console.WriteLine($"\tMin:{res.Min}");
            //    Console.WriteLine($"\tAvg:{(int)res.Avg}");
            //}




            //vjezba, top 3 fuel eficiant cars by country
            /*prvi nacin*/
            var top3fuelCarsByCountry = manifacturers.Join(cars, m => m.Name, c => c.Manufacturer, (m, c) =>
                new
                {
                    Manufacturer = m.Name,
                    m.Headquarters,
                    c.Name,
                    c.Combined
                }).GroupBy(g => g.Headquarters).OrderBy(g => g.Key);

            //foreach (var item in top3fuelCarsByCountry)
            //{
            //    Console.WriteLine($"{item.Key}");
            //    foreach (var i in item.OrderByDescending(g => g.Combined).Take(3))
            //    {
            //        Console.WriteLine($"\t{i.Name}:{i.Combined}");
            //    }
            //}
            /*drugi nacin*/
            var top3fuelCarsByCountryV2 = manifacturers.GroupJoin(cars, m => m.Name, c => c.Manufacturer, (m, g) =>
            new
            {
                Manufacturer = m,
                Cars = g
            }).GroupBy(x => x.Manufacturer.Headquarters)
            .OrderBy(x => x.Key);

            //foreach (var drzave in top3fuelCarsByCountryV2)
            //{
            //    Console.WriteLine($"{drzave.Key}");
            //    foreach (var auto in drzave.SelectMany(x => x.Cars).OrderByDescending(x => x.Combined).Take(3))
            //    {
            //        Console.WriteLine($"\t{auto.Name}:{auto.Combined}");
            //    }
            //}

            //GROUPJOIN
            //groupjoin vraca GRUPIRANE rezultate po kljucu po kom se joinalo
            //dok join vraca NEGRUPIRANE podatke, njih ako naknadno grupiras, dobijes isto sto i grupjoin
            var query7 = manifacturers.GroupJoin(cars, m => m.Name, c => c.Manufacturer, (m, g) =>
            new
            {
                Manifacturer = m,
                Cars = g
            }).OrderBy(m => m.Manifacturer.Name);



            var query6 = from manifacturer in manifacturers//.OrderBy(m => m.Name)//moze i ovako ali sada korisi query sintaksu
                         join car in cars on manifacturer.Name equals car.Manufacturer into carGroup//ovo je GroupJoin
                         orderby manifacturer.Name
                         select new
                         {
                             Manifacturer = manifacturer,
                             Cars = carGroup
                         };

            //obican join za usporedbu sa group join-om
            var query75 = manifacturers.Join(cars, m => m.Name, c => c.Manufacturer,
                         (m, c) => new { Manifacturer = m, Cars = c }).Select(x => x);
            /*
            foreach (var group in query7)
            {
                Console.WriteLine(group.Manifacturer.Name);
                foreach (var car in group.Cars.OrderBy(c => c.Name))
                {
                    Console.WriteLine($"\t {car.Name}");
                }
                //for (int i = 0; i < group.Cars.Count(); i++)//preko for petlje, bolje je preko foreach..
                //{
                //    Console.WriteLine($"\t {group.Cars.OrderBy(c => c.Name).ElementAt(i).Name}");
                //}
            }
            foreach (var item in query75.OrderBy(m => m.Manifacturer.Name))
            {
                Console.WriteLine($"{item.Manifacturer.Name}:{item.Cars.Name}");
            }*/



            //GROUPBY
            var query5 = cars.GroupBy(c => c.Manufacturer.ToUpper()).OrderBy(g => g.Key);//isto sto i query4

            var query4 = from car in cars
                         group car by car.Manufacturer.ToUpper() into proizvodjac//key ce biti uppercase, dok je atribut manifacturer netaknut
                         orderby proizvodjac.Key//sortiras grupiranu kolekciju po keyu
                         select proizvodjac;

            //foreach (var group in query5)
            //{
            //    Console.WriteLine($"{group.Key} has {group.Count()} cars");                
            //    foreach (var i in group.OrderBy(c => c.Name))
            //    {
            //        Console.WriteLine($"\t {i.Name}");
            //    }
            //}


            //JOIN
            var query1 = cars
                .Join(manifacturers, c => new { c.Manufacturer, c.Year }, m => new { Manufacturer = m.Name, m.Year },//inner join na 2 atributa
                (c, m) => new
                {
                    m.Headquarters,
                    c.Name,
                    c.Combined,
                    Car = c,//u novom objektu mozes sacuvati i izvorne objekte
                    Manifacturer = m//u novom objektu mozes sacuvati i izvorne objekte
                })
                .OrderByDescending(x => x.Combined)
                .ThenBy(x => x.Name)//.First();//mozes pisati i Take(1); ali ako pises Frist, to je immediate operator i kao rezultat dobije Car a ne IEnumerable
                .Select(x => x);


            var query2 = from c in cars
                         join m in manifacturers on c.Manufacturer equals m.Name
                         orderby c.Combined ascending, c.Name ascending
                         select new
                         {
                             m.Headquarters,
                             c.Name,
                             c.Combined
                         };


            //foreach (var car in query1.Take(10))
            //{
            //    Console.WriteLine($"{car.Headquarters} {car.Name}: {car.Combined}");
            //}

            //var resulta = cars.Select(c => c.Name);

            //Console.WriteLine("\n");
            //foreach (var name in resulta.Take(10))
            //{
            //    foreach (var character in name)
            //    {
            //        Console.WriteLine(character);
            //    }
            //}

            //umjesto 2 puta da koristis foreach(primjer iznas), mozes koristiti SelectMany, koji ako mu das np listu, iz nje izvuce clanove
            var resulta = cars.Take(1).SelectMany(c => c.Name);
            //foreach (var character in resulta)
            //{
            //    Console.WriteLine(character);
            //}
        }

        private static void QueryData()
        {
            var db = new CarDb();

            //db.Database.Log = Console.WriteLine;//ispise ti log vezan za db u cmd

            //var query = db.Cars.Where(c => c.Manufacturer == "BMW").OrderByDescending(c => c.Combined).ThenBy(c => c.Name).Take(10);
            var query =
                db.Cars.GroupBy(c => c.Manufacturer)
                  .Select(g => new
                  {
                      Name = g.Key,
                      Cars = g.OrderByDescending(c => c.Combined).Take(2)
                  });
            foreach (var group in query)
            {
                Console.WriteLine(group.Name);
                foreach (var car in group.Cars)
                {
                    Console.WriteLine($"\t{car.Name} ");
                }
            }

            //foreach (var car in query)
            //{
            //    Console.WriteLine($"{car.Name}:{car.Combined}");
            //}
        }

        private static void InsertData()
        {
            var carsi = ProcessCars("fuel.csv");

            var db = new CarDb();

            //db.Database.Log = Console.WriteLine;//ispise ti log vezan za db u cmd


            if (!db.Cars.Any())
            {
                foreach (var car in carsi)
                {
                    db.Cars.Add(car);
                }
                db.SaveChanges();
            }
        }

        private static void QueryXml()
        {
            //ako imas jako veliki xml file, koristi stari xml reader, koji ne ucitava čitav xml u memoriju prvo, nego strema kroz xml dokument
            //dok XDocument pretpostavlvja da mozes citav xml ucitati u memoriju, mozes jos koristiti i readfrom, parse ili load metode
            var document = XDocument.Load("fuel.xml");

            var ns = (XNamespace)"https://nekiUrl/cars/2016";
            var ex = (XNamespace)"https://nekiUrl/cars/2016/extension";

            //                        jednina         mnozina
            var  dohvati = (document.Element(ns + "Cars")?.Elements(ex + "Car") ?? Enumerable.Empty<XElement>())
                                    //ako ne postoji ns + "Cars", vratiti ce se null, pa u tom slucaju moras vratiti praznu listu tj Enumrable, kako linq ne bi javio exception
                                    //ako ne postoji samo ex + "Car", a ns + "Cars" postoji vratit ce se prazan Enumerable
                           //document.Descendants(ex + "Car")?//zanemaris hijerarhiju, nego trazis sve "Car" unutar dokumenta
                            .Where(e => e.Attribute("Manufacturer")?.Value == "BMW")//upitnik - ako atribut ne postoji linq izbaciti exception
                            .Select(e => e.Attribute("Name").Value);                //nego ce ispitivati null=="BMW"
            
            foreach (var name in dohvati)
            {
                Console.WriteLine(name);
            }
        }

        private static void CreateXml()
        {
            var records = ProcessCars("fuel.csv");//isto sto i cars

            var ns = (XNamespace) "https://nekiUrl/cars/2016";
            var ex = (XNamespace) "https://nekiUrl/cars/2016/extension";

            var document = new XDocument();
            var carsi = new XElement(ns + "Cars",
                                      from record in records
                                      select new XElement(ex + "Car",//inace ce se u njemu kreirati prazan atribut za namespace ako je namespace definiran u parentu
                                                   new XAttribute("Name", record.Name),
                                                   new XAttribute("Combined", record.Combined),
                                                   new XAttribute("Manufacturer", record.Manufacturer)));

            carsi.Add(new XAttribute(XNamespace.Xmlns + "ex", ex));//unutar "Cars"kreiras prefix za "Car" element, kako ti nebi u svakom elementu "Car" pisao citav namespace kobasica

            document.Add(carsi);
            document.Save("fuel.xml");

            //koristis linq umjesto foreach
            /*foreach (var record in records)
            {
                //ovo pises kadnije, i koristi drguciji overload ove metode, da automatski dodas elemente, pa ne moras pisati car.add(element)
                //var car = new XElement("Car");

                //verzija: svi podaci u posebnom elementu
                //var name = new XElement("Name", record.Name);
                //var combined = new XElement("Combined", record.Combined);

                //verzija: podaci u atributima odredjenog elementa
                //var name = new XAttribute("Name", record.Name);                
                //var combined = new XAttribute("Combined", record.Combined);

                //var car = new XElement("Car", name, combined);
                var car = new XElement("Car", 
                                        new XAttribute("Name", record.Name), 
                                        new XAttribute("Combined", record.Combined),
                                        new XAttribute("Manufacturer", record.Manufacturer));


                //car.Add(name);
                //car.Add(combined);

                carsi.Add(car);
            }
            document.Add(carsi);
            document.Save("fuel.xml");*/
        }





        //WHERE, ove metode koristis za citanje csv-a
        private static List<Manifacturer> ProcessManifacturers(string path)
        {
            var query = File.ReadAllLines(path)
                .Where(line => line.Length > 1)
                .Select(line => Manifacturer.ParseFromCsv(line))
                .ToList();
            return query;
        }

        private static List<Car> ProcessCars(string path)
        {
            //1.nacin
            var query = File.ReadAllLines(path)
                .Skip(1)//preskocis 1. red jer su tamo nazivi atributa
                .Where(line => line.Length > 1)//zanemaris red ako je prazan
                .Select(Car.ParseFromCsv)//ili Select(line => Car.ParseFromCsv(line))
                .ToList();
            return query;

            //2.nacin
            //return (from line in File.ReadAllLines(path).Skip(1)
            //        where line.Length > 1
            //        select Car.ParseFromCsv(line)).ToList();

            //3.nacin
            //var query = File.ReadAllLines(path)
            //    .Skip(1)
            //    .Where(line => line.Length > 1)
            //    .ToCar()//napravio si drugu exstension metodu koja yeald vraca Car objekte
            //    .ToList();
            //return query;

            //razlika izmedju 1,2 vs 3 je sto 1,2 vraca Car a 3 vraca IEnueravble<Car>
        }
    }

    class CarStatistics//ova klasa nam treba za aggregate..
    {
        int _total = 0;
        int _count = 0;
        public int Max { get; set; } = Int32.MinValue;
        public int Min { get; set; } = Int32.MaxValue;
        public double Avg { get; set; }


        public CarStatistics Accumulate(Car car)
        {
            _total += car.Combined;
            _count++;
            Max = Math.Max(Max, car.Combined);
            Min = Math.Min(Max, car.Combined);

            return this;
        }

        public CarStatistics Compute()
        {
            Avg = (double)_total / _count;
            return this;
        }
    }
    public static class CarExtensions
    {
        public static IEnumerable<Car> ToCar(this IEnumerable<string> source)
        {
            foreach (var line in source)
            {
                var columns = line.Split(',');
                yield return new Car()
                {
                    Year = Int32.Parse(columns[0]),
                    Manufacturer = columns[1],
                    Name = columns[2],
                    Displacement = double.Parse(columns[3]),
                    Cylinders = int.Parse(columns[4]),
                    City = int.Parse(columns[5]),
                    Highway = int.Parse(columns[6]),
                    Combined = int.Parse(columns[7])
                };
            }
        }
    }
}
