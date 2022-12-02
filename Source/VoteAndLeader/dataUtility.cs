using Verse;

namespace election;

public static class dataUtility
{
    private static myData data;


    public static myData GetData()
    {
        if (data == null)
        {
            data = new myData();
        }

        data.setParent(Find.World);
        return data;
    }
}