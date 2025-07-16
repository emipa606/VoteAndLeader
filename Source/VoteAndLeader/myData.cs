using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace election;

public class myData : IExposable
{
    private List<Pawn> _ar_candi;
    private List<int> ar_candi_id = [];
    public int assistTick = -1;
    public int elecTick = -1;

    // Data
    public int readyTick = -1;
    private World w;

    public List<Pawn> ar_candi
    {
        get
        {
            if (_ar_candi != null)
            {
                return _ar_candi;
            }

            _ar_candi = [];
            foreach (var i in ar_candi_id)
            {
                foreach (var p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
                {
                    if (p.thingIDNumber == i)
                    {
                        _ar_candi.Add(p);
                    }
                }
            }

            return _ar_candi;
        }
        set => _ar_candi = value;
    }


    // Data Save
    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            ar_candi_id = [];
            foreach (var p in ar_candi)
            {
                ar_candi_id.Add(p.thingIDNumber);
            }

            Log.Message($"save {string.Join(", ", ar_candi_id)}");
        }

        Scribe_Values.Look(ref readyTick, "readyTick", -1);
        Scribe_Values.Look(ref elecTick, "elecTick", -1);
        Scribe_Values.Look(ref assistTick, "assistTick", -1);
        Scribe_Collections.Look(ref ar_candi_id, "ar_candi_id", LookMode.Value);
    }


    public void setParent(World _w)
    {
        w = _w;
    }
}