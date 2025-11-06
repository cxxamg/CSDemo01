using System;
using System.Collections.Generic;
using System.Linq;

namespace CSConsoleApp3
{
    public static class CreditsAnalyzer
    {
        private static StringComparison CI = StringComparison.OrdinalIgnoreCase;

        // 1
        public static IEnumerable<MovieCredit> MoviesByDirector(IEnumerable<MovieCredit> movies, string directorName) =>
            movies.Where(m => m.Crew.Any(c => !string.IsNullOrEmpty(c.Job) &&
                                             c.Job.Equals("Director", CI) &&
                                             c.Name != null && c.Name.Equals(directorName, CI)));

        // 2
        public static IEnumerable<(int MovieId, string Title, string Character)> CharactersByActor(IEnumerable<MovieCredit> movies, string actorName) =>
            movies.SelectMany(m => m.Cast
                                   .Where(c => c.Name != null && c.Name.Equals(actorName, CI))
                                   .Select(c => (m.MovieId, m.Title, c.Character)));

        // 3
        public static IEnumerable<(int MovieId, string Title, int CastCount)> TopMoviesByCastCount(IEnumerable<MovieCredit> movies, int topN = 5) =>
            movies.Select(m => (m.MovieId, m.Title, CastCount: (m.Cast?.Count ?? 0)))
                  .OrderByDescending(x => x.CastCount)
                  .ThenBy(x => x.Title)
                  .Take(topN);

        // 4
        public static IEnumerable<(string Actor, int MovieCount)> TopActorsByMovieCount(IEnumerable<MovieCredit> movies, int topN = 10) =>
            movies.SelectMany(m => m.Cast ?? Enumerable.Empty<CastMember>())
                  .Where(c => !string.IsNullOrEmpty(c.Name))
                  .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                  .Select(g => (Actor: g.Key, MovieCount: g.Count()))
                  .OrderByDescending(x => x.MovieCount)
                  .ThenBy(x => x.Actor)
                  .Take(topN);

        // 5
        public static IEnumerable<string> UniqueDepartments(IEnumerable<MovieCredit> movies) =>
            movies.SelectMany(m => m.Crew ?? Enumerable.Empty<CrewMember>())
                  .Where(c => !string.IsNullOrEmpty(c.Department))
                  .Select(c => c.Department)
                  .Distinct(StringComparer.OrdinalIgnoreCase)
                  .OrderBy(d => d, StringComparer.OrdinalIgnoreCase);

        // 6
        public static IEnumerable<(int MovieId, string Title, string Job)> MoviesByComposer(IEnumerable<MovieCredit> movies, string composerName, string jobMatcher = "Original Music Composer") =>
            movies.SelectMany(m => (m.Crew ?? Enumerable.Empty<CrewMember>())
                                   .Where(c => c.Name != null && c.Name.Equals(composerName, CI) &&
                                               !string.IsNullOrEmpty(c.Job) &&
                                               (c.Job.Equals(jobMatcher, CI) || c.Job.IndexOf("music", StringComparison.OrdinalIgnoreCase) >= 0))
                                   .Select(c => (m.MovieId, m.Title, c.Job)))
                  .Distinct();

        // 7
        public static Dictionary<int, IEnumerable<string>> MovieIdToDirectors(IEnumerable<MovieCredit> movies) =>
            movies.ToDictionary(m => m.MovieId,
                                m => (m.Crew ?? Enumerable.Empty<CrewMember>())
                                      .Where(c => !string.IsNullOrEmpty(c.Job) && c.Job.Equals("Director", CI))
                                      .Select(c => c.Name ?? "")
                                      .Where(n => n != ""));

        // 8
        public static IEnumerable<MovieCredit> MoviesWithBothActors(IEnumerable<MovieCredit> movies, string actorA, string actorB) =>
            movies.Where(m =>
            {
                var names = new HashSet<string>((m.Cast ?? Enumerable.Empty<CastMember>()).Where(c => c.Name != null).Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
                return names.Contains(actorA) && names.Contains(actorB);
            });

        // 9
        public static int UniquePeopleCountInDepartment(IEnumerable<MovieCredit> movies, string department) =>
            movies.SelectMany(m => m.Crew ?? Enumerable.Empty<CrewMember>())
                  .Where(c => !string.IsNullOrEmpty(c.Department) && c.Department.Equals(department, CI))
                  .Select(c => c.Id)
                  .Distinct()
                  .Count();

        // 10
        public static IEnumerable<(int MovieId, string Title, IEnumerable<(int Id, string Name)>)> PeopleInCastAndCrewForTitle(IEnumerable<MovieCredit> movies, string title) =>
            movies.Where(m => m.Title != null && m.Title.Equals(title, CI))
                  .Select(m =>
                  {
                      var castIds = new HashSet<int>((m.Cast ?? Enumerable.Empty<CastMember>()).Select(c => c.Id));
                      var crewIds = new HashSet<int>((m.Crew ?? Enumerable.Empty<CrewMember>()).Select(c => c.Id));
                      var both = castIds.Intersect(crewIds);
                      var list = (m.Cast ?? Enumerable.Empty<CastMember>()).Where(c => both.Contains(c.Id))
                                  .Select(c => (c.Id, c.Name ?? ""))
                                  .Distinct()
                                  .ToList();
                      

                      var crewExtras = (m.Crew ?? Enumerable.Empty<CrewMember>()).Where(c => both.Contains(c.Id) && string.IsNullOrEmpty(c.Name) == false)
                                      .Select(c => (c.Id, c.Name ?? ""));
                      var merged = list.Union(crewExtras).Distinct().ToList();
                      return (m.MovieId, m.Title, merged.AsEnumerable());
                  }).Where(x => x.Item3.Any());

        // 11
        public static IEnumerable<(int CrewId, string Name, int SharedFilms)> InnerCircleForDirector(IEnumerable<MovieCredit> movies, string directorName, int topN = 5)
        {
            var directorMovies = movies.Where(m => (m.Crew ?? Enumerable.Empty<CrewMember>()).Any(c => c.Name != null && c.Name.Equals(directorName, CI) && !string.IsNullOrEmpty(c.Job) && c.Job.Equals("Director", CI))).ToList();
            var counter = new Dictionary<int, (string Name, int Count)>();
            foreach (var m in directorMovies)
            {
                foreach (var c in m.Crew ?? Enumerable.Empty<CrewMember>())
                {
                    if (string.IsNullOrEmpty(c.Job)) continue;
                    

                    if (c.Job.Equals("Actor", CI) || (c.Name != null && c.Name.Equals(directorName, CI))) continue;
                    if (!counter.ContainsKey(c.Id)) counter[c.Id] = (c.Name ?? "", 0);
                    counter[c.Id] = (counter[c.Id].Name, counter[c.Id].Count + 1);
                }
            }
            return counter.Select(kv => (kv.Key, kv.Value.Name, kv.Value.Count))
                          .OrderByDescending(x => x.Count)
                          .ThenBy(x => x.Name)
                          .Take(topN);
        }

        // 12
        public static IEnumerable<((string A, string B) Pair, int Count)> TopActorPairs(IEnumerable<MovieCredit> movies, int topN = 10)
        {
            var pairCounts = new Dictionary<(string, string), int>(new PairComparer());
            foreach (var m in movies)
            {
                var names = (m.Cast ?? Enumerable.Empty<CastMember>()).Select(c => c.Name).Where(n => !string.IsNullOrEmpty(n)).Select(n => n.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                names.Sort(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < names.Count; i++)
                {
                    for (int j = i + 1; j < names.Count; j++)
                    {
                        var key = (names[i], names[j]);
                        if (pairCounts.ContainsKey(key)) pairCounts[key] += 1;
                        else pairCounts[key] = 1;
                    }
                }
            }
            return pairCounts.Select(kv => ((kv.Key.Item1, kv.Key.Item2), kv.Value))
                             .OrderByDescending(x => x.Value)
                             .ThenBy(x => x.Item1.Item1)
                             .ThenBy(x => x.Item1.Item2)
                             .Take(topN);
        }

        // 13
        public static IEnumerable<(int CrewId, string Name, int DeptCount, IEnumerable<string> Departments)> TopCrewByDeptDiversity(IEnumerable<MovieCredit> movies, int topN = 5)
        {
            var depsByCrew = new Dictionary<int, HashSet<string>>();
            var names = new Dictionary<int, string>();
            foreach (var m in movies)
            {
                foreach (var c in m.Crew ?? Enumerable.Empty<CrewMember>())
                {
                    if (!depsByCrew.ContainsKey(c.Id)) depsByCrew[c.Id] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (!string.IsNullOrEmpty(c.Department))
                        depsByCrew[c.Id].Add(c.Department);
                    names[c.Id] = c.Name ?? "";
                }
            }
            return depsByCrew.Select(kv => (CrewId: kv.Key, Name: names.GetValueOrDefault(kv.Key, ""), DeptCount: kv.Value.Count, Departments: kv.Value.OrderBy(d => d).AsEnumerable() ))
                              .OrderByDescending(x => x.DeptCount)
                              .ThenBy(x => x.Name)
                              .Take(topN);
        }

        // 14
        public static IEnumerable<(int MovieId, string Title, int PersonId, string PersonName, IEnumerable<string> Jobs)> CreativeTrios(IEnumerable<MovieCredit> movies)
        {
            var results = new List<(int, string, int, string, IEnumerable<string>)>();
            foreach (var m in movies)
            {
                var jobsByPerson = new Dictionary<int, HashSet<string>>();
                var nameById = new Dictionary<int, string>();
                foreach (var c in m.Crew ?? Enumerable.Empty<CrewMember>())
                {
                    if (!jobsByPerson.ContainsKey(c.Id)) jobsByPerson[c.Id] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (!string.IsNullOrEmpty(c.Job)) jobsByPerson[c.Id].Add(c.Job);
                    nameById[c.Id] = c.Name ?? "";
                }

                foreach (var kv in jobsByPerson)
                {
                    var jobsLower = kv.Value.Select(j => j.ToLowerInvariant()).ToList();
                    bool hasDirector = jobsLower.Any(j => j.Contains("director"));
                    bool hasWriter = jobsLower.Any(j => j.Contains("writer") || j.Contains("screenplay") || j.Contains("story"));
                    bool hasProducer = jobsLower.Any(j => j.Contains("producer"));
                    if (hasDirector && hasWriter && hasProducer)
                    {
                        results.Add((m.MovieId, m.Title, kv.Key, nameById.GetValueOrDefault(kv.Key, ""), kv.Value));
                    }
                }
            }
            return results;
        }

        // 15
        public static IEnumerable<string> TwoStepsFromActor(IEnumerable<MovieCredit> movies, string actorName)
        {
            var adj = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in movies)
            {
                var names = (m.Cast ?? Enumerable.Empty<CastMember>()).Select(c => c.Name).Where(n => !string.IsNullOrEmpty(n)).Select(n => n.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                foreach (var a in names)
                {
                    if (!adj.ContainsKey(a)) adj[a] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var b in names) if (!a.Equals(b, StringComparison.OrdinalIgnoreCase)) adj[a].Add(b);
                }
            }
            if (!adj.ContainsKey(actorName)) return Enumerable.Empty<string>();
            var direct = adj[actorName];
            var two = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in direct)
            {
                if (!adj.ContainsKey(d)) continue;
                foreach (var fof in adj[d])
                {
                    if (fof.Equals(actorName, CI)) continue;
                    if (direct.Contains(fof)) continue; 
                    two.Add(fof);
                }
            }
            return two.OrderBy(n => n, StringComparer.OrdinalIgnoreCase);
        }

        // 16
        public static IEnumerable<(string Director, int FilmCount, double AvgCastSize, double AvgCrewSize)> DirectorGroupStats(IEnumerable<MovieCredit> movies)
        {
            var dict = new Dictionary<string, List<(int castSize, int crewSize)>>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in movies)
            {
                var directors = (m.Crew ?? Enumerable.Empty<CrewMember>()).Where(c => !string.IsNullOrEmpty(c.Job) && c.Job.Equals("Director", CI)).Select(c => c.Name).Where(n => !string.IsNullOrEmpty(n)).Distinct(StringComparer.OrdinalIgnoreCase);
                int castSize = m.Cast?.Count ?? 0;
                int crewSize = m.Crew?.Count ?? 0;
                foreach (var d in directors)
                {
                    if (!dict.ContainsKey(d)) dict[d] = new List<(int, int)>();
                    dict[d].Add((castSize, crewSize));
                }
            }

            return dict.Select(kv =>
            {
                var filmCount = kv.Value.Count;
                var avgCast = filmCount == 0 ? 0 : kv.Value.Average(x => x.castSize);
                var avgCrew = filmCount == 0 ? 0 : kv.Value.Average(x => x.crewSize);
                return (Director: kv.Key, FilmCount: filmCount, AvgCastSize: Math.Round(avgCast, 2), AvgCrewSize: Math.Round(avgCrew, 2));
            }).OrderByDescending(x => x.FilmCount).ThenBy(x => x.Director);
        }

        // 17
        public static IEnumerable<(int PersonId, string Name, string MostCommonDepartment, int Count)> UniversalsDepartment(IEnumerable<MovieCredit> movies)
        {
            var crewDeptCounts = new Dictionary<int, Dictionary<string, int>>();
            var actorIds = new HashSet<int>();
            var idToName = new Dictionary<int, string>();

            foreach (var m in movies)
            {
                foreach (var c in m.Cast ?? Enumerable.Empty<CastMember>())
                {
                    actorIds.Add(c.Id);
                    idToName[c.Id] = c.Name ?? idToName.GetValueOrDefault(c.Id, "");
                }
                foreach (var c in m.Crew ?? Enumerable.Empty<CrewMember>())
                {
                    if (!crewDeptCounts.ContainsKey(c.Id)) crewDeptCounts[c.Id] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    var dep = c.Department ?? "";
                    if (!crewDeptCounts[c.Id].ContainsKey(dep)) crewDeptCounts[c.Id][dep] = 0;
                    crewDeptCounts[c.Id][dep]++;
                    idToName[c.Id] = c.Name ?? idToName.GetValueOrDefault(c.Id, "");
                }
            }

            var results = new List<(int, string, string, int)>();
            foreach (var kv in crewDeptCounts)
            {
                if (!actorIds.Contains(kv.Key)) continue;
                var deptCounts = kv.Value;
                if (!deptCounts.Any()) continue;
                var top = deptCounts.OrderByDescending(d => d.Value).ThenBy(d => d.Key).First();
                results.Add((kv.Key, idToName.GetValueOrDefault(kv.Key, ""), top.Key, top.Value));
            }

            return results.OrderByDescending(x => x.Item4).ThenBy(x => x.Item2);
        }

        // 18
        public static IEnumerable<(int Id, string Name)> PeopleWhoWorkedWithBothDirectors(IEnumerable<MovieCredit> movies, string directorA, string directorB)
        {
            var withA = PeopleWhoWorkedWithDirector(movies, directorA);
            var withB = PeopleWhoWorkedWithDirector(movies, directorB);
            return withA.Intersect(withB).OrderBy(x => x.Name);
        }

        private static HashSet<(int Id, string Name)> PeopleWhoWorkedWithDirector(IEnumerable<MovieCredit> movies, string director)
        {
            var mids = new HashSet<int>(movies.Where(m => (m.Crew ?? Enumerable.Empty<CrewMember>()).Any(c => c.Name != null && c.Name.Equals(director, CI) && !string.IsNullOrEmpty(c.Job) && c.Job.Equals("Director", CI)))
                                             .Select(m => m.MovieId));
            var people = new HashSet<(int, string)>();
            foreach (var m in movies.Where(m => mids.Contains(m.MovieId)))
            {
                foreach (var c in m.Crew ?? Enumerable.Empty<CrewMember>()) people.Add((c.Id, c.Name ?? ""));
                foreach (var c in m.Cast ?? Enumerable.Empty<CastMember>()) people.Add((c.Id, c.Name ?? ""));
            }
            return people;
        }

        // 19
        public static IEnumerable<(string Department, double AvgCastSize, int MoviesCount)> DepartmentsByAvgCast(IEnumerable<MovieCredit> movies)
        {
            var deptToMovies = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in movies)
            {
                foreach (var c in m.Crew ?? Enumerable.Empty<CrewMember>())
                {
                    var dep = c.Department ?? "";
                    if (!deptToMovies.ContainsKey(dep)) deptToMovies[dep] = new HashSet<int>();
                    deptToMovies[dep].Add(m.MovieId);
                }
            }
            var results = new List<(string Department, double AvgCastSize, int MoviesCount)>();
            foreach (var kv in deptToMovies)
            {
                var mids = kv.Value;
                if (mids.Count == 0) continue;
                var avg = mids.Average(mid => movies.First(mm => mm.MovieId == mid).Cast?.Count ?? 0);
                results.Add((Department: kv.Key, AvgCastSize: Math.Round(avg, 2), MoviesCount: mids.Count));
            }
            return results.OrderByDescending(x => x.AvgCastSize).ThenBy(x => x.Department);
        }

        // 20
        public static IEnumerable<(string FirstWord, int Count)> ArchetypesForActor(IEnumerable<MovieCredit> movies, string actorName, int topN = 20)
        {
            var counter = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in movies)
            {
                foreach (var c in m.Cast ?? Enumerable.Empty<CastMember>())
                {
                    if (c.Name == null) continue;
                    if (!c.Name.Equals(actorName, CI)) continue;
                    var charName = c.Character ?? "";
                    var first = charName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                    if (string.IsNullOrEmpty(first)) continue;
                    if (!counter.ContainsKey(first)) counter[first] = 0;
                    counter[first]++;
                }
            }
            return counter.Select(kv => (kv.Key, kv.Value)).OrderByDescending(x => x.Value).ThenBy(x => x.Key).Take(topN);
        }

        
        private class PairComparer : IEqualityComparer<(string, string)>
        {
            public bool Equals((string, string) x, (string, string) y) =>
                string.Equals(x.Item1, y.Item1, CI) && string.Equals(x.Item2, y.Item2, CI);

            public int GetHashCode((string, string) obj)
            {
                var a = obj.Item1?.ToLowerInvariant() ?? "";
                var b = obj.Item2?.ToLowerInvariant() ?? "";
                return HashCode.Combine(a, b);
            }
        }
    }
}