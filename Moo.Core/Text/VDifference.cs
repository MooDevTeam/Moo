using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
///VDifference 的摘要说明
/// </summary>
public class VDifference
{
    public enum ChangeFlag
    {
        Delete, Insert, Keep
    }

    public class Change
    {
        public int pos;
        public ChangeFlag flag;
        public string value;
        public Change(int _pos, ChangeFlag _flg, string _value)
        {
            pos = _pos;
            flag = _flg;
            value = _value;
        }
    }

    public class Result : IComparable<Result>
    {
        public List<string> Text;
        public ChangeFlag Flag;
        public int l, r;
        public int CompareTo(Result Other)
        {
            if (l.CompareTo(Other.l) == 0)
            {
                if (r.CompareTo(Other.r) != 0)
                    return r.CompareTo(Other.r);
                if (Flag == Other.Flag)
                    return 0;
                if (Flag == ChangeFlag.Keep)
                    return -1;
                return 1;
            }
            return l.CompareTo(Other.l);
        }
        public Result(Change c)
        {
            Text = new List<string>();
            Text.Add(c.value);
            l = r = c.pos;
            Flag = c.flag;
        }
        public Result()
        {
            Text = new List<string>();
            Flag = ChangeFlag.Keep;
            l = r = -1;
        }
        public void Add(Change c)
        {
            l = Math.Min(l, c.pos);
            Text.Insert(0, c.value);
        }
        public override string ToString()
        {
            string Ret = "";
            foreach (string s in Text)
            {
                Ret += s + "\r\n";
            }
            return Ret;
        }
    }

    class Array
    {
        int[] a;
        int Start, End;
        public Array(int _Start, int _End)
        {
            Start = _Start;
            End = _End;
            a = new int[End - Start + 1];
            for (int i = 0; i < a.Length; i++)
                a[i] = -1;
        }
        public int this[int index]
        {
            get
            {
                if (OutOfRange(index)) return -1;
                return a[index - Start];
            }
            set
            {
                if (!OutOfRange(index))
                    a[index - Start] = value;
            }
        }
        public bool OutOfRange(int index)
        {
            return index < Start || index > End;
        }
    }

    class Point : IComparable<Point>
    {
        public int x, y;
        public Point(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public int CompareTo(Point Other)
        {
            if (x == Other.x)
                return y.CompareTo(Other.y);
            return x.CompareTo(Other.x);
        }

    }

    public static List<Result> GetDifference(string str1, string str2, string[] split)
    {
        return
            GetDifference(new List<string>(str1.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)),
            new List<string>(str2.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)));
    }

    public static List<Result> GetDifference(List<string> str1, List<string> str2)
    {
        str1.Insert(0, "");
        str2.Insert(0, "");
        List<Change> ChangeList = new List<Change>();
        SortedDictionary<Point, Point> s = new SortedDictionary<Point, Point>();
        Array far = new Array(-str1.Count, str2.Count);
        for (int i = 0; i < str1.Count && i < str2.Count && str1[i] == str2[i]; i++)
            far[0] = i;
        Result tRes;
        if (far[0] == str1.Count - 1 && far[0] == str2.Count - 1)
        {
            tRes = new Result();
            List<Result> KeepAll = new List<Result>();
            tRes.l = 1;
            tRes.r = str1.Count;
            tRes.Flag = ChangeFlag.Keep;
            tRes.Text = str1;
            KeepAll.Add(tRes);
            return KeepAll;
        }
        Point begin = new Point(far[0], far[0]);
        for (int i = 1, flg = 0; flg == 0 && i <= str1.Count + str2.Count; i++)
        {
            Array tFar = far;
            for (int k = -i; k <= i; k += 2)
            {
                if (far.OutOfRange(k)) continue;
                int x = -1, lx = -1, ly = -1;
                if (far[k - 1] == -1 && far[k + 1] != -1)
                {
                    x = far[k + 1];
                    lx = far[k + 1];
                    ly = lx - k - 1;
                }
                if (far[k + 1] == -1 && far[k - 1] != -1)
                {
                    x = far[k - 1] + 1;
                    lx = far[k - 1];
                    ly = lx - k + 1;
                }
                if (far[k + 1] != -1 && far[k - 1] != -1)
                    if (far[k + 1] > far[k - 1] + 1)
                    {
                        x = far[k + 1];
                        lx = far[k + 1];
                        ly = lx - k - 1;
                    }
                    else
                    {
                        x = far[k - 1] + 1;
                        lx = far[k - 1];
                        ly = lx - k + 1;
                    }
                if (x == -1) continue;
                int y = x - k;
                while (x < str2.Count - 1 && y < str1.Count - 1 && str2[x + 1] == str1[y + 1])
                {
                    x++;
                    y++;
                }
                tFar[k] = x;
                s.Add(new Point(x, y), new Point(lx, ly));
                if (x == str2.Count - 1 && y == str1.Count - 1)
                {
                    flg = 1;
                    Point now = new Point(x, y), tmp;
                    while (now.CompareTo(begin) != 0)
                    {
                        tmp = s[now];
                        int delta = Math.Min(now.x - tmp.x, now.y - tmp.y);
                        if (now.x - delta == tmp.x)
                            ChangeList.Add(new Change(now.y - delta, ChangeFlag.Delete, str1[now.y - delta]));
                        else
                            ChangeList.Add(new Change(now.y - delta, ChangeFlag.Insert, str2[now.x - delta]));
                        now = tmp;
                    }
                    break;
                }
            }
            far = tFar;
        }
        int pos;
        List<Result> Res = new List<Result>();
        for (int i = 0; i < ChangeList.Count; i++)
        {
            if (Res.Count == 0)
                Res.Add(new Result(ChangeList[i]));
            else
            {
                pos = Res.Count - 1;
                if (ChangeList[i].flag == Res[pos].Flag && (
                    (ChangeList[i].pos == Res[pos].l && ChangeList[i].flag == ChangeFlag.Insert) ||
                    (ChangeList[i].pos == Res[pos].l - 1 && ChangeList[i].flag == ChangeFlag.Delete)))
                    Res[pos].Add(ChangeList[i]);
                else
                    Res.Add(new Result(ChangeList[i]));
            }
        }
        pos = str1.Count - 1;
        for (int i = 0, stop = Res.Count; i < stop; i++)
        {
            if (pos > Res[i].r)
            {
                tRes = new Result();
                tRes.l = Res[i].r + 1;
                tRes.r = pos;
                tRes.Flag = ChangeFlag.Keep;
                tRes.Text = str1.GetRange(tRes.l, tRes.r - tRes.l + 1);//str1.Substring(tRes.l, tRes.r - tRes.l + 1);
                Res.Add(tRes);
            }
            if (Res[i].Flag == ChangeFlag.Insert)
                pos = Res[i].l;
            else
                pos = Res[i].l - 1;
        }
        if (pos > 0)
        {
            tRes = new Result();
            tRes.l = 1;
            tRes.r = pos;
            tRes.Flag = ChangeFlag.Keep;
            tRes.Text = str1.GetRange(1, pos);//str1.Substring(1, pos);
            Res.Add(tRes);
        }
        Res.Sort();
        return Res;
    }
}