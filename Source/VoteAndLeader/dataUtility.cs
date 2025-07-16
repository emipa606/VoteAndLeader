using Verse;

namespace election;

public static class dataUtility
{
    private static myData data;


    public static myData GetData()
    {
        data ??= new myData();

        data.setParent(Find.World);
        return data;
    }
}