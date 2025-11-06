using System;
using System.Linq;
using System.Collections.Generic;

namespace CSConsoleApp3
{
    public static class Program
    {
        public static void Main()
        {
            try
            {
                var currentDirectory = System.IO.Directory.GetCurrentDirectory();
                var filePath = System.IO.Directory.GetFiles(currentDirectory, "*.csv").First();
                var parser = new MovieCreditsParser(filePath);
                var movies = parser.Parse();

                Console.WriteLine($"Загружено фильмов: {movies.Count}\n");

                // 1
                var spielberg = CreditsAnalyzer.MoviesByDirector(movies, "Steven Spielberg").ToList();
                Console.WriteLine($"1) Фильмы режиссёра 'Steven Spielberg': {spielberg.Count}");
                foreach (var m in spielberg.OrderBy(x => x.Title).Take(20))
                    Console.WriteLine($"   ID: {m.MovieId} — Название: {m.Title}");
                Console.WriteLine();

                // 2
                var tomRoles = CreditsAnalyzer.CharactersByActor(movies, "Tom Hanks").ToList();
                Console.WriteLine($"2) Роли актёра 'Tom Hanks': {tomRoles.Count}");
                foreach (var r in tomRoles.Take(30))
                    Console.WriteLine($"   ID фильма: {r.MovieId} | {r.Title} → Персонаж: {r.Character}");
                Console.WriteLine();

                // 3
                Console.WriteLine("3) Пять фильмов с наибольшим количеством актёров:");
                foreach (var t in CreditsAnalyzer.TopMoviesByCastCount(movies, 5))
                    Console.WriteLine($"   ID: {t.MovieId} | {t.Title} — количество актёров: {t.CastCount}");
                Console.WriteLine();

                // 4
                Console.WriteLine("4) Десять актёров, снявшихся в наибольшем количестве фильмов:");
                foreach (var a in CreditsAnalyzer.TopActorsByMovieCount(movies, 10))
                    Console.WriteLine($"   {a.Actor} — фильмов: {a.MovieCount}");
                Console.WriteLine();

                // 5
                var depts = CreditsAnalyzer.UniqueDepartments(movies).ToList();
                Console.WriteLine($"5) Департаменты: {depts.Count}");
                foreach (var d in depts.Take(30))
                    Console.WriteLine($"   {d}");
                Console.WriteLine();

                // 6
                var hans = CreditsAnalyzer.MoviesByComposer(movies, "Hans Zimmer").ToList();
                Console.WriteLine($"6) Фильмы с композитором 'Hans Zimmer': {hans.Count}");
                foreach (var h in hans)
                    Console.WriteLine($"   {h.Title} (ID: {h.MovieId}) — должность: {h.Job}");
                Console.WriteLine();

                // 7
                var movieToDirs = CreditsAnalyzer.MovieIdToDirectors(movies);
                Console.WriteLine("7) ID фильма → режиссёры:");
                foreach (var kv in movieToDirs.Take(10))
                    Console.WriteLine($"   {kv.Key} → [{string.Join(", ", kv.Value)}]");
                Console.WriteLine();

                // 8
                var bradGeorge = CreditsAnalyzer.MoviesWithBothActors(movies, "Brad Pitt", "George Clooney").ToList();
                Console.WriteLine($"8) Фильмы с участием 'Brad Pitt' и 'George Clooney': {bradGeorge.Count}");
                foreach (var m in bradGeorge)
                    Console.WriteLine($"   {m.Title} (ID: {m.MovieId})");
                Console.WriteLine();

                // 9
                var cameraCount = CreditsAnalyzer.UniquePeopleCountInDepartment(movies, "Camera");
                Console.WriteLine($"9) Количество людей в департаменте 'Camera': {cameraCount}\n");

                // 10
                var titanicBoth = CreditsAnalyzer.PeopleInCastAndCrewForTitle(movies, "Titanic").ToList();
                Console.WriteLine($"10) Люди, указанные и в актёрском составе, и в съёмочной группе фильма 'Titanic': {titanicBoth.Sum(x => x.Item3.Count())}");
                foreach (var item in titanicBoth)
                {
                    Console.WriteLine($"   {item.Title} (ID: {item.MovieId}):");
                    foreach (var p in item.Item3)
                        Console.WriteLine($"      ID: {p.Id} — {p.Name}");
                }
                Console.WriteLine();

                // 11
                Console.WriteLine("11) 5 Ближайших соратников 'Quentin Tarantino':");
                foreach (var c in CreditsAnalyzer.InnerCircleForDirector(movies, "Quentin Tarantino", 5))
                    Console.WriteLine($"   {c.Name} (ID: {c.CrewId}) — совместных фильмов: {c.SharedFilms}");
                Console.WriteLine();

                // 12
                Console.WriteLine("12) 10 актёрских пар:");
                foreach (var p in CreditsAnalyzer.TopActorPairs(movies, 10))
                    Console.WriteLine($"   {p.Pair.A} и {p.Pair.B} — совместных фильмов: {p.Count}");
                Console.WriteLine();

                // 13
                Console.WriteLine("13) 5 сотрудников по разнообразию департаментов:");
                foreach (var x in CreditsAnalyzer.TopCrewByDeptDiversity(movies, 5))
                    Console.WriteLine($"   {x.Name} (ID: {x.CrewId}) — департаментов: {x.DeptCount} [{string.Join(", ", x.Departments)}]");
                Console.WriteLine();

                // 14
                var trios = CreditsAnalyzer.CreativeTrios(movies).ToList();
                Console.WriteLine($"14) Творческие трио (режиссёр + сценарист + продюсер): {trios.Count}");
                foreach (var t in trios.Take(20))
                    Console.WriteLine($"   {t.Title} (ID: {t.MovieId}) → {t.PersonName} (ID: {t.PersonId}) — должности: {string.Join(", ", t.Jobs)}");
                Console.WriteLine();

                // 15
                var twoFromKevin = CreditsAnalyzer.TwoStepsFromActor(movies, "Kevin Bacon").ToList();
                Console.WriteLine($"15) Актёры, связанные с 'Kevin Bacon' через одного человека: {twoFromKevin.Count}");
                foreach (var name in twoFromKevin.Take(50))
                    Console.WriteLine($"   {name}");
                Console.WriteLine();

                // 16
                Console.WriteLine("16) Статистика режиссёров (топ 10):");
                foreach (var d in CreditsAnalyzer.DirectorGroupStats(movies).Take(10))
                    Console.WriteLine($"   {d.Director} — фильмов: {d.FilmCount}, средний размер актёрского состава: {d.AvgCastSize}, съёмочной группы: {d.AvgCrewSize}");
                Console.WriteLine();

                // 17
                Console.WriteLine("17) Первые 20 человек, которые были и актёрами, и членами съёмочной группы:");
                foreach (var p in CreditsAnalyzer.UniversalsDepartment(movies).Take(20))
                    Console.WriteLine($"   {p.Name} (ID: {p.PersonId}) — департамент: {p.MostCommonDepartment} ({p.Count})");
                Console.WriteLine();

                // 18
                var elite = CreditsAnalyzer.PeopleWhoWorkedWithBothDirectors(movies, "Martin Scorsese", "Christopher Nolan").ToList();
                Console.WriteLine($"18) Люди, работавшие с 'Martin Scorsese' и с 'Christopher Nolan': {elite.Count}");
                foreach (var e in elite.Take(50))
                    Console.WriteLine($"   {e.Name} (ID: {e.Id})");
                Console.WriteLine();

                // 19
                Console.WriteLine("19) Департаменты по среднему количеству актёров:");
                foreach (var d in CreditsAnalyzer.DepartmentsByAvgCast(movies).Take(10))
                    Console.WriteLine($"   {d.Department} — среднее количество актёров: {d.AvgCastSize}, фильмов: {d.MoviesCount}");
                Console.WriteLine();

                // 20
                Console.WriteLine("20) Повторяющиеся архетипы ролей актёра 'Johnny Depp':");
                foreach (var a in CreditsAnalyzer.ArchetypesForActor(movies, "Johnny Depp", 20))
                    Console.WriteLine($"   '{a.FirstWord}' — количество: {a.Count}");
                Console.WriteLine();

            }
            catch (Exception exc)
            {
                Console.WriteLine("Ошибка при выполнении анализа: " + exc.Message);
                Console.WriteLine(exc.StackTrace);
                Environment.Exit(1);
            }
        }
    }
}