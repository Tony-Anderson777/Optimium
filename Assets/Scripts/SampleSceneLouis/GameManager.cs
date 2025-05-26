using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Slider sld;
    private Date[] sorted_Dates;
    private CubeColor[] allCubeColorScripts;

    void Start()
    {
        this.sld.minValue = 0;
        this.sld.maxValue = 100;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Another instance of GameManager already exist");
            Destroy(gameObject);
            return;
        }
    }

    public void setup()
    {
        allCubeColorScripts = FindObjectsByType<CubeColor>(FindObjectsSortMode.None);
        List<Date> allDatesCombined = new List<Date>();

        foreach (CubeColor scriptInstance in allCubeColorScripts)
        {
            allDatesCombined.AddRange(scriptInstance.Dates);
        }

        List<Date> allUniqueDates = allDatesCombined
                                .GroupBy(d => new { d.Year, d.Month, d.Day, d.Morning })
                                .Select(g => g.First())
                                .ToList();

        Date[] allDateArray = allUniqueDates.ToArray();

        this.sorted_Dates = this.algo_sort_date(allDateArray);

        List<(Date, List<CubeColor>)> link = this.link_date_cube();
        this.manage_multiple(link);
    }

    public List<(Date, List<CubeColor>)> link_date_cube()
    {
        List<(Date, List<CubeColor>)> link = new List<(Date, List<CubeColor>)>();

        foreach (Date targetDate in sorted_Dates)
        {
            List<CubeColor> cube = new List<CubeColor>();
            foreach (CubeColor cubeColor in allCubeColorScripts)
            {
                bool hasDate = cubeColor.Dates.Any(d =>
                    d.Year == targetDate.Year &&
                    d.Month == targetDate.Month &&
                    d.Day == targetDate.Day &&
                    d.Morning == targetDate.Morning
                );

                if (hasDate)
                {
                    cube.Add(cubeColor);
                }
            }
            link.Add((targetDate, cube));
        }

        return link;
    }

    public void manage_multiple(List<(Date, List<CubeColor>)> link)
    {
        foreach ((Date targetDate, List<CubeColor> cubes) in link)
        {
            Dictionary<string, List<CubeColor>> analCodeGroups = new();

            foreach (CubeColor cubeInstance in cubes)
            {
                int indexAnalCode = cubeInstance.GetOccupIndexByDate(targetDate);
                if (indexAnalCode == -1) continue;

                string analCode = cubeInstance.Occupations[indexAnalCode].Item1;

                if (!analCodeGroups.ContainsKey(analCode))
                {
                    analCodeGroups[analCode] = new List<CubeColor>();
                }

                analCodeGroups[analCode].Add(cubeInstance);
            }

            foreach (var entry in analCodeGroups)
            {
                string analCode = entry.Key;
                List<CubeColor> associatedCubes = entry.Value;

                int count = associatedCubes.Count;

                if (count > 1)
                {
                    int indexOccupation = associatedCubes[0].GetOccupIndexByDate(targetDate);
                    int nb = associatedCubes[0].Occupations[indexOccupation].Item2;
                    int remains = nb % count;
                    foreach (CubeColor cube in associatedCubes)
                    {
                        int indexOcc = cube.GetOccupIndexByDate(targetDate);
                        if (remains == 0)
                        {
                            cube.SetNbOccupation(indexOcc, nb / count);
                        }
                        else
                        {
                            cube.SetNbOccupation(indexOcc, nb / count + 1);
                            remains -= 1;
                        }
                    }

                    /*
                    int indexOccupation = associatedCubes[0].GetOccupIndexByDate(targetDate);
                    int remains = associatedCubes[0].Occupations[indexOccupation].Item2;
                    foreach (Cube_Color cube in associatedCubes)
                    {
                        int capacity = int.Parse(cube.Infos["Capacity"].ToString());
                        int indexOcc = cube.GetOccupIndexByDate(targetDate);
                        if (remains != 0)
                        {
                            cube.SetNbOccupation(indexOcc, 0);
                        }
                        else
                        {
                            cube.SetNbOccupation(indexOcc, capacity);
                            remains -= capacity;
                        }
                    }
                    */
                }
            }
        }
    }

    public void set_cube()
    {
        int nb_Date = this.sorted_Dates.Length;
        float range = this.sld.maxValue / nb_Date;

        for (int i = 0; i < nb_Date; i++)
        {
            if (sld.value >= i * range && sld.value < (i + 1) * range)
            {
                foreach (CubeColor scriptInstance in allCubeColorScripts)
                {
                    scriptInstance.setup_cube(this.sorted_Dates[i]);
                }
            }
        }
    }

    Date[] algo_sort_date(Date[] dates)
    {
        sort_by_year(ref dates);
        sort_by_month(ref dates);
        sort_by_day(ref dates);
        sort_by_half(ref dates);
        return dates;
    }

    void sort_by_year(ref Date[] dates)
    {
        for (int i = 0; i < dates.Length - 1; i++)
        {
            for (int j = 0; j < dates.Length - i - 1; j++)
            {
                if (dates[j].Year > dates[j + 1].Year)
                {
                    Date temp = dates[j];
                    dates[j] = dates[j + 1];
                    dates[j + 1] = temp;
                }
            }
        }
    }

    void sort_by_month(ref Date[] dates)
    {
        for (int i = 0; i < dates.Length - 1; i++)
        {
            for (int j = 0; j < dates.Length - i - 1; j++)
            {
                if (dates[j].Year == dates[j + 1].Year &&
                  dates[j].Month > dates[j + 1].Month)
                {
                    Date temp = dates[j];
                    dates[j] = dates[j + 1];
                    dates[j + 1] = temp;
                }
            }
        }
    }

    void sort_by_day(ref Date[] dates)
    {
        for (int i = 0; i < dates.Length - 1; i++)
        {
            for (int j = 0; j < dates.Length - i - 1; j++)
            {
                if (dates[j].Year == dates[j + 1].Year &&
                  dates[j].Month == dates[j + 1].Month &&
                  dates[j].Day > dates[j + 1].Day)
                {
                    Date temp = dates[j];
                    dates[j] = dates[j + 1];
                    dates[j + 1] = temp;
                }
            }
        }
    }

    void sort_by_half(ref Date[] dates)
    {
        for (int i = 0; i < dates.Length - 1; i++)
        {
            for (int j = 0; j < dates.Length - i - 1; j++)
            {
                if (dates[j].Year == dates[j + 1].Year &&
                  dates[j].Month == dates[j + 1].Month &&
                  dates[j].Day == dates[j + 1].Day)
                {
                    if (!dates[j].Morning && dates[j + 1].Morning)
                    {
                        Date temp = dates[j];
                        dates[j] = dates[j + 1];
                        dates[j + 1] = temp;
                        break;
                    }
                }
            }
        }
    }
}
