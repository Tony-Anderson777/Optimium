using UnityEngine;

public class Date {
    private int _year;
    public int Year {
        get { return _year; }
        set { _year = value; }
    }
    private int _month;
    public int Month {
        get { return _month; }
        set { _month = value; }
    }
    private int _day;
    public int Day {
        get { return _day; }
        set { _day = value; }
    }
    private bool morning;
    public bool Morning {
        get { return morning; }
        set { morning = value; }
    }

    public Date(int year, int month, int day, bool morning) {
        this.Year = year;
        this.Month = month;
        this.Day = day;
        this.Morning = morning;
    }
}
